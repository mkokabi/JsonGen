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
        
        public string Generate(string metadataName, Func<dynamic, bool> predicate = null)
        {
            try
            {
                return GenerateAsync(metadataName, predicate).Result;
            }
            catch (AggregateException e) when (e.InnerExceptions.FirstOrDefault().GetType()== typeof(GenerateException))
            {
                throw e.InnerException;
            }
        }

        public async Task<string> GenerateAsync(string metadataName, Func<dynamic, bool> predicate = null, Filter[] filters = null)
        {
            var metadata = metadataProvider.GetMetadata(metadataName)?? 
                throw new GenerateException("Metadata provider return null.");
            
            var layout = metadata.Layout;
            JObject jLayout = JObject.Parse(layout.Content);
            var dataTokens =
                jLayout.SelectTokens("$..*")
                        .Where(jt => jt.Type == JTokenType.Array &&
                                     jt.Parent.Parent is JObject &&
                                    (jt.Parent.Parent as JObject).Children()
                                        .Any(child => (child.Type == JTokenType.Property) && (child as JProperty).Name
                                                        .Equals(dataSourceNode, 
                                                            StringComparison.InvariantCultureIgnoreCase)));

            foreach (var dataToken in dataTokens)
            {
                var dataSourceName = ((dataToken.Parent.Parent as JObject).Children()
                                        .First(child => (child.Type == JTokenType.Property) && (child as JProperty).Name
                                        .Equals(dataSourceNode, StringComparison.InvariantCultureIgnoreCase))
                                        as JProperty).Value.ToString();

                var vs = dataToken.Values();

                var fields = new Dictionary<string, string>();
                vs.ToList().ForEach(field =>
                    fields[((JProperty)field).Name] = ((JProperty)field).Value.ToString().TrimStart('@')
                );

                var dataSource = metadata.DataSources.FirstOrDefault(ds => ds.Key == dataSourceName);
                if (dataSource == null)
                {
                    continue;
                }

                (dataToken as JArray).RemoveAll();
                var dataProviderType = GetDataProviderType(dataSource.DataProviderFullName);
                if (dataProviderType == null)
                {
                    continue;
                }

                IEnumerable<dynamic> data = await GetData(predicate, filters, dataProviderType, dataSource);

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
                            (dataToken as JArray).Add(JObject.FromObject(row));
                        }
                    }
                });
            }
            return jLayout.ToString();
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
