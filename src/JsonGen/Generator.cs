using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JsonGen
{
    public class Generator : IGenerator
    {
        private readonly IMetadataProvider metadataProvider;

        public Generator(IMetadataProvider metadataProvider)
        {
            this.metadataProvider = metadataProvider;
        }

        private const string dataSourceNode = "_dataSource";
        
        public string Generate(string metadataName, Func<dynamic, bool> predicate = null, Dictionary<string, dynamic> parameters = null)
        {
            try
            {
                return GenerateAsync(metadataName, predicate, parameters: parameters).Result;
            }
            catch (AggregateException e) when (e.InnerExceptions.FirstOrDefault().GetType()== typeof(GenerateException))
            {
                throw e.InnerException;
            }
        }

        public async Task<string> GenerateAsync(string metadataName, Func<dynamic, bool> predicate = null, Filter[] filters = null, 
                                                Dictionary<string, dynamic> parameters = null)
        {
            var metadata = metadataProvider.GetMetadata(metadataName)?? 
                throw new GenerateException("Metadata provider return null.");
            
            var layout = metadata.Layout;
            JObject jLayout = JObject.Parse(layout.Content);
            var dataTokens =
                jLayout.SelectTokens("$..*")
                        .Where(jt => //(jt.Type == JTokenType.Array) &&
                                     (jt.Parent?.Previous != null) &&
                                     (jt.Parent.Previous.Type == JTokenType.Property) &&
                                     (jt.Parent.Previous as JProperty).Name
                                                        .StartsWith(dataSourceNode,
                                                            StringComparison.InvariantCultureIgnoreCase));

            foreach (var dataToken in dataTokens.ToList())
            {
                var dataSourceName = ((dataToken.Parent.Parent as JObject).Children()
                                        .First(child => (child.Type == JTokenType.Property) && (child as JProperty).Name
                                        .StartsWith(dataSourceNode, StringComparison.InvariantCultureIgnoreCase))
                                        as JProperty).Value.ToString();

                if (parameters != null && parameters.Any())
                {
                    dataSourceName = applyParametersOnDataSourceName(dataSourceName, parameters);
                }

                var dataSource = metadata.DataSources.FirstOrDefault(ds => ds.Key == dataSourceName);
                if (dataSource == null)
                {
                    continue;
                }

                var dataProviderType = GetDataProviderType(dataSource.DataProviderFullName);
                if (dataProviderType == null)
                {
                    continue;
                }
                
                if (typeof(IScalarDataProvider).IsAssignableFrom(dataProviderType))
                {
                    if (dataToken is JValue)
                    {
                        dynamic scalarData = await GetScalarData(ApplyOptions(dataSource.Options, filters),
                            dataProviderType, dataSource);

                        (dataToken as JValue).Value = scalarData;
                        continue;
                    }
                    else
                    {
                        throw new GenerateException("ScalarDataProvider needs a value.");
                    }
                }

                var vs = dataToken.Values();

                var fields = new Dictionary<string, string>();
                vs.Where(f => f is JProperty).ToList().ForEach(field =>
                    fields[((JProperty)field).Name] = ((JProperty)field).Value.ToString().TrimStart('@')
                );
                // values will be used for an array element like data: [x, y]
                var values = vs.Where(f => f is JValue).ToList();

                (dataToken as JContainer)?.RemoveAll();

                IEnumerable<dynamic> data = await GetData(
                    ApplyOptions(dataSource.Options, predicate),
                    ApplyOptions(dataSource.Options, filters), dataProviderType, dataSource);

                List<string> keysToBeRemoved = null;
                data?.ToList().ForEach(row =>
                {
                    if (fields.Any())
                    {
                        var newRow = JObject.FromObject(row);

                        if (keysToBeRemoved == null)
                        {
                            keysToBeRemoved = new List<string>();
                            foreach (var property in ((JObject)newRow).Children().OfType<JProperty>())
                            {
                                if (!fields.Keys.Any(k => k.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    keysToBeRemoved.Add(property.Name);
                                }
                            }
                       }
                        foreach (var key in keysToBeRemoved)
                        {
                            newRow.Remove(key);
                        }

                        (dataToken as JArray).Add(newRow);

                    }
                    else
                    {
                        if ((row.GetType() != null) && row.GetType().IsPrimitive)
                        {
                            (dataToken as JArray).Add((object)row);
                        }
                        else
                        {
                            var jContainer = JObject.FromObject(row) as JContainer;
                            if (jContainer != null)
                            {
                                var rowProperties = jContainer.ToList();
                                rowProperties
                                    .Where(rp => values.Any(v => v.ToString().Equals(((JProperty)rp).Name, StringComparison.InvariantCultureIgnoreCase)))
                                    .ToList().ForEach(rowProp => (dataToken as JArray).Add(((JProperty)rowProp).Value));
                            }
                        }
                    }
                });
            }
            return jLayout.ToString();
        }

        private Func<dynamic, bool> ApplyOptions(DatasourceOptions options, Func<dynamic, bool> predicate)
        {
            if (options?.ApplyFilter == false)
            {
                return null;
            }
            return predicate;
        }

        private Filter[] ApplyOptions(DatasourceOptions options, Filter[] filters)
        {
            if (options?.ApplyFilter == false)
            {
                return null;
            }
            if (options?.IgnoreFilteringOn != null)
            {
                return filters?.Where(f => 
                    !options.IgnoreFilteringOn.Any(oif => 
                        oif.Equals(f.FieldName, StringComparison.InvariantCultureIgnoreCase)))
                        .ToArray();
            }
            return filters;
        }

        private string applyParametersOnDataSourceName(string dataSourceName, Dictionary<string, dynamic> parameters)
        {
            foreach (var parameter in parameters)
            {
                dataSourceName = dataSourceName.Replace($"[{parameter.Key}]", parameter.Value);
            }
            return dataSourceName;
        }

        private async Task<dynamic> GetScalarData(Filter[] filters, Type dataProviderType, DataSource dataSource)
        {
            dynamic data = null;

            if (typeof(IScalarDbDataProvider).IsAssignableFrom(dataProviderType))
            {
                var dbDataProvider = (IScalarDbDataProvider)Activator.CreateInstance(dataProviderType);
                dbDataProvider.DbConnection = dataSource.DbConnection;
                dbDataProvider.Query = dataSource.Query;

                if (filters != null)
                {
                    data = await dbDataProvider.GetScalarDataAsync(filters);
                }
                else
                {
                    data = await dbDataProvider.GetScalarDataAsync();
                }
            }
            
            else if (typeof(IScalarDataProvider).IsAssignableFrom(dataProviderType))
            {
                var dataProvider = (IScalarDataProvider)Activator.CreateInstance(dataProviderType);

                data = await dataProvider.GetScalarDataAsync();
            }

            return data;
        }

        private async Task<IEnumerable<dynamic>> GetData(Func<dynamic, bool> predicate, Filter[] filters, Type dataProviderType, DataSource dataSource)
        {
            IEnumerable<dynamic> data = null;

            if (typeof(IDbDataProvider).IsAssignableFrom(dataProviderType))
            {
                var dbDataProvider = (IDbDataProvider)Activator.CreateInstance(dataProviderType);
                dbDataProvider.DbConnection = dataSource.DbConnection;
                dbDataProvider.Query = dataSource.Query;
                
                if (filters != null)
                {
                    data = await dbDataProvider.GetDataAsync(filters);
                }
                else
                {
                    data = await dbDataProvider.GetDataAsync(predicate);
                }
            }
            else if (predicate != null && typeof(IFilterableDataProvider).IsAssignableFrom(dataProviderType))
            {
                var filterableDataProvider =
                    (IFilterableDataProvider)Activator.CreateInstance(dataProviderType);

                data = await filterableDataProvider.GetDataAsync(predicate);
            }
            else if (typeof(IDataProvider).IsAssignableFrom(dataProviderType))
            {
                var dataProvider = (IDataProvider)Activator.CreateInstance(dataProviderType);

                data = await dataProvider.GetDataAsync();
            }

            return data;
        }

        private Type GetDataProviderType(string dataProviderFullName)
        {
            var dataProviderType = 
                AppDomain.CurrentDomain
                .GetAssemblies().Where(p => !p.IsDynamic)
                .SelectMany(asm =>
                {
                    try
                    {
                        return asm.GetExportedTypes();
                    }
                    catch (Exception)
                    {
                        // Ignore the assembly if it fails to export its types
                    }
                    return new Type[] { };
                })
                .FirstOrDefault(type => string.Equals(type.FullName, dataProviderFullName, StringComparison.InvariantCultureIgnoreCase));

            if (dataProviderType.IsAssignableFrom(typeof(IDataProvider)))
            {
                return null;
            }

            return dataProviderType;
        }
    }
}
