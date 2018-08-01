# JsonGen
A Json generator engine based on the metadata including layout and data providers defined in .Net languages.

## Sample
Let's start with the simplest form:
```csharp
var generator = new Generator(new BasicMetadataProvider(name => new Metadata
{
  DataSources = new[]
  {
    new DataSource {
      Key = "W",
      DataProviderFullName = typeof(DataProvider).FullName
    }
  },
  Labels = new Labels { },
  Layout = new Layout { Content = "{'_dataSource':'W', 'data':[]}" }
}));
var generatedJson = generator.Generate("MyMetadata");
```
The output will be 
```
{
  '_dataSource': 'W',
  'data': [1, 2, 3]
};
```

or another way with setting fields
```csharp
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
        '_dataSource': 'W',
        'data': [
          {
            'field1': 'Hello',
            'field2': 'World'
          }
        ]
      }";
  Assert.AreEqual(expected, generatedJson);
}
  }
```

## Metadata:
Metadata class contains 3 main components: Layout, Datasources and Labels

## Layout
Simply it holds the content as a Json
```csharp
public class Layout
{
    public string Content { get; set; }
}
```
In this Json the **_datasource** element is pointing to the key of a datasource. The first array element sibling to this node is where the data is going to be placed.

## Datasources
Another important part of Metadata is Datasources which is an array of **Datasource**. The main element of datasource is the **Key** which is the reference used in the layout. the next important part is **DataProviderFullName** which is the full name of a class implementing IDataProvider

### IDataProvider
This interface must be implemented by any data provider and has one simple method GetData which returns an IEnumerable of dynamic.
There is a DbDataProvider in JsonGen.Db project which can be used on the databases. 

## Labels

