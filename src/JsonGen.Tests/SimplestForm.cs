using System;
using System.Collections.Generic;
using JsonGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                                Fields = new[] { "field1", "field2" },
                                DataProviderFullName = typeof(DataProvider).FullName
                            }
                        },
                Labels = new Labels { },
                Layout = new Layout { Content = "{'_dataSource':'W', 'data':[{'field1': '@val', 'field2': '@val'}]}" }
            }
        );

        public class DataProvider : IDataProvider
        {
            public IEnumerable<dynamic> GetData() => new[]
            {
                new { field1 ="Hello",  field2 = "World" }
            };
        }

        [TestMethod]
        public void Simple_Test()
        {

            var generator = new Generator(metadataProvider);
            var generatedJson = generator.Generate("SomeMetadata");
            var expected = @"{
  ""_dataSource"": ""W"",
  ""data"": [
    {
      ""field1"": ""Hello"",
      ""field2"": ""World""
    }
  ]
}";
            Assert.AreEqual(expected, generatedJson);
        }
    }
}
