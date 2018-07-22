using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace JsonGenTestProject
{
    [TestClass]
    public class SimplestForm
    {
        BasicMetadataProvider metadataProvider = new BasicMetadataProvider(name => new Metadata
            {
                DataSources = new[]
                        {
                            new DataSource {
                                Key = "W",
                                DataProviderFullName = typeof(DataProvider).FullName
                            }
                        },
                Labels = new Labels { },
                Layout = new Layout { Content = "{'_dataSource':'W', 'data':[{'field1': '@val', 'field2': '@val'}]}" }
            }
        );

        public class DataProvider : IDataProvider
        {
            public async Task<IEnumerable<dynamic>> GetDataAsync() => await Task.FromResult(new[]
            {
                new { field1 ="Hello",  field2 = "World" }
            });
        }

        [TestMethod]
        public void Simple_Test()
        {

            var generator = new Generator(metadataProvider);
            var generatedJson = generator.Generate("SomeMetadata");
            var expected = @"{
  '_dataSource': 'W',
  'data': [
    {
      'field1': 'Hello',
      'field2': 'World'
    }
  ]
}";
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expected), JObject.Parse(generatedJson)));
        }
    }
}
