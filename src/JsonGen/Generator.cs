﻿using Newtonsoft.Json.Linq;
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

        public string Generate(string metadataName, Func<dynamic, bool> predicate = null)
        {
            var metadata = metadataProvider.GetMetadata(metadataName);
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

                IEnumerable<dynamic> data = null;

                if (predicate != null && typeof(IFilterableDataProvider).IsAssignableFrom(dataProviderType))
                {
                    var filterableDataProvider =
                        (IFilterableDataProvider)Activator.CreateInstance(dataProviderType);

                    data = filterableDataProvider.GetData(predicate);
                }
                else if (typeof(IDataProvider).IsAssignableFrom(dataProviderType))
                {
                    var dataProvider = (IDataProvider)Activator.CreateInstance(dataProviderType);

                    data = dataProvider.GetData();
                }
                data?.ToList().ForEach(row =>
                {
                    if (fields.Any())
                    {
                        var newRow = new JObject();
                        fields.Keys.ToList().ForEach(key =>
                        {
                            newRow.Add(key, row.GetType().GetProperty(key)?.GetValue(row, null));
                        });
                        (dataToken as JArray).Add(JObject.FromObject(newRow));
                    }
                    else
                    {
                        (dataToken as JArray).Add(JObject.FromObject(row));
                    }
                });
            }
            return jLayout.ToString();
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
