using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public string Generate(string reportName)
        {
            var metadata = metadataProvider.GetMetadata(reportName);
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
                var dataProvider = GetDataProvider(dataSource.DataProviderFullName);
                if (dataProvider == null)
                {
                    continue;
                }

                dataProvider.ToList().ForEach(row =>
                {
                    (dataToken as JArray).Add(JObject.FromObject(row));
                });
            }
            return jLayout.ToString();
        }

        private IEnumerable<dynamic> GetDataProvider(string dataProviderFullName)
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

            IDataProvider dataProvider = (IDataProvider)Activator.CreateInstance(dataProviderType);
            return dataProvider.GetData();
        }
    }
}
