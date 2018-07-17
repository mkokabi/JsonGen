# JsonGen
A Json generator engine based on the metadata including layout and data providers defined in .Net languages.

## Sample
Let's start with the simplest form:

```csharp
    [TestClass]
    public class SimplestForm
    {
        public class MyMetadataProvider : MetadataProvider
        {
            public override Metadata GetMetadata(string metadataName)
            {
                return new Metadata
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
                };
            }
        }

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
            var metadataProvider = new MyMetadataProvider();

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
```

## Create metadata:
Metadata class build of 3 main components: Layout, Datasources and Labels

## Layout
Simply it holds the content as a Json

## DataSources
An array of datasources. Each datasource has a key, a list of fields and and the full name of a class implementing IDataProvider

### IDataProvider
Is the interface must be implemented by any data provider and has one simple method GetData which returns an IEnumerable of dynamic

## Labels

