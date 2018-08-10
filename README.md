# JsonGen
A Json generator engine based on the metadata including layout and data providers defined in .Net languages. Usually, if a specific Json needs to be created based on a defined layout, developer has to create all the classes and hierarchy and then map his data to this structure, and then finally serialize his root class to get the Json. **JsonGen** is following another appraoch. The steps is: tag the Json layout with **\_datasource** and then define your datasource(s). Finally, ask **JsonGen** to generate the finall Json with the data.

## Sample
Let's start with the simplest form (https://dotnetfiddle.net/hNKHYQ):
```csharp
public class DataProvider : IDataProvider
{
   public async Task<IEnumerable<dynamic>> GetDataAsync() => 
	  await Task.FromResult((new dynamic[]
		{
		  1, 2, 3
		}).AsEnumerable());
}

var generator = new Generator(new BasicMetadataProvider(name => 
	new Metadata
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
var metadataProvider = new BasicMetadataProvider(name => new Metadata
  {
    DataSources = new[]
    {
      new DataSource {
      Key = "W",
      DataProviderFullName = typeof(DataProvider).FullName
    }
  },
  Labels = new Labels { },
  Layout = new Layout { 
	Content = "{'_dataSource':'W', 'data':[{'field1': '', 'field2': ''}]}" }
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
        '_dataSource': 'W',
        'data': [
          {
            'field1': 'Hello',
            'field2': 'World'
          }
        ]
      }";  
}
  }
```
<br><br>

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
In this Json the **\_datasource** element is pointing to the key of a datasource. The first array element sibling to this node is where the data is going to be placed.

## Datasources
Another important part of Metadata is Datasources which is an array of **Datasource**. The main element of datasource is the **Key** which is the reference used in the layout. the next important part is **DataProviderFullName** which is the full name of a class implementing IDataProvider

### IDataProvider
This interface must be implemented by any data provider and has one simple method GetData which returns an IEnumerable of dynamic.
```csharp
public interface IDataProvider
{
    Task<IEnumerable<dynamic>> GetDataAsync();
}
```
There is a DbDataProvider in JsonGen.Db project which can be used on the databases. 

## Labels
To ba added

<br><br>

# Sepcial features

## Filtering
For filtering, **Generate** method can accept one of these 2 optional parameters: **predicate** or **filters**. **predicate** is a condition which can filter the datasource rows. On the other hand **filters** parameter is used for database sources and basically is a list of **Filter** objects which have _name_, _value_ and optionally _operator_. 

**note** : to use the **predicate** the data provider should be implementing **IFilterableDataProvider** and for using **filters** it should be implementing **IDbDataProvider**. 

Supported filter operators are:
* **Eq**: Equals
* **G**: Greater than
* **L**: Less than
* **GE**: Greater than or equal to
* **LE**: Less than or equal to
* **In**: In a set values. Value should be an array

## Sample
```csharp
var metadataProvider = new BasicMetadataProvider(_ => new Metadata
{
    Layout = new Layout { 
       Content = "{'_dataSource': 'A', 'data': [ { 'name': 'X' }]}" 
    },
    Labels = new Labels(),
    DataSources = new[]
    {
        new DataSource {
            Key = "A",
            DataProviderFullName = typeof(DbDataProvider).FullName,
            DbConnection = new SqlConnection(connStr),
            Query = "Select * from TestTable",
        }
    }
});

var generator = new Generator(metadataProvider);
var json = await generator.GenerateAsync("myMeta", 
	filters: new[] 
		{ new Filter { FieldName = "Id", Value = 1 } }
	);
```
The output would be:
```
{'_dataSource': 'A', 'data': [ { 'Name': 'MK' }]}
```

### Excluding from filtering
When there are multiple data sources used in one layout and some of them shouldn't be filtered that data source can use DatasourceOptions of ApplyFilter = false.

```csharp
new DataSource {
    Key = "A",
    DataProviderFullName = typeof(DbDataProvider).FullName,
    DbConnection = new SqlConnection(connStr),
    Query = "Select * from TestTable",
    Options = new DatasourceOptions { ApplyFilter = false }
}

```

## Stored procedures
For calling stored procedure instead of query should start with **Exec** (case insensitive).
In this case, filters would be passed as parameters to the stored procedure:

## Sample
```sql
CREATE PROCEDURE [dbo].[TestProc]
	@param1 int = 0,
	@param2 VARCHAR(10)
AS
	SELECT * FROM dbo.TestTable WHERE Id = @param1 OR Name = @param2
RETURN 0
```
<br>

```csharp
var metadataProvider = new BasicMetadataProvider(_ => new Metadata
{
  Layout = new Layout { Content = "{'_dataSource': 'A', 'data': [ { 'Name': 'X' }]}" },
  Labels = new Labels(),
  DataSources = new[]
  {
    new DataSource {
      Key = "A",
      DataProviderFullName = typeof(DbDataProvider).FullName,
      DbConnection = new SqlConnection(connStr),
      Query = " eXec TestProc",
    }
  }
});

var generator = new Generator(metadataProvider);
var json = await generator.GenerateAsync("myMeta", filters: new[] {
  new Filter {
    FieldName = "param1",
    Value = 1,
  },
  new Filter {
    FieldName = "param2",
    Value = "AK",
  }});
```

## Scalar
The data provider can be retunring scalar values instead of a list of data. For these scenarios the **DataProvider** should be implementing **IScalarDataProvider**. 

```csharp
public interface IScalarDataProvider: IDataProvider
{
    Task<dynamic> GetScalarDataAsync();
}
```
For data sources connecting to databases **ScalarDbDataProvider** can be used.

## Sample:
```csharp
var metadataProvider = new BasicMetadataProvider(_ => new Metadata
{
    Layout = new Layout { Content = "{'_dataSource': 'A', 'data': 'x' }" },
    Labels = new Labels(),
    DataSources = new[]
    {
        new DataSource {
            Key = "A",
            DataProviderFullName = typeof(ScalarDbDataProvider).FullName,
            DbConnection = new SqlConnection(connStr),
            Query = "Select Max(Id) from TestTable",
        }
    }
});

var generator = new Generator(metadataProvider);
```
The ouput would be 
```
{'_dataSource': 'A', 'data': 2 }
```

## parameters
The other parameter which can be passed to the Generate method is **parameters** which is simply a dictionary of named objects. Based on these parameters, the data source could be switched at run time.

## sample
```csharp
var basicMetadataProvider = new BasicMetadataProvider(_ => 
    new Metadata
    {
      Labels = new Labels(),
      Layout = new Layout { Content = "{'_dataSource':'ds_[p]', 'data':[]}" },
      DataSources = new[]
        {
          new DataSource { Key = "ds_A", DataProviderFullName = typeof(DataProviderA).FullName },
          new DataSource { Key = "ds_B", DataProviderFullName = typeof(DataProviderB).FullName },
        }
    });

var generator = new Generator(basicMetadataProvider);
var json = generator.Generate(
    "MetadataName", 
    parameters: new Dictionary<string, dynamic> { { "p", "B" } }
);

```
At run time the parameter **p** would be changed with its value of **B** therefore datasource **ds_B** would be used:

```
{'_dataSource':'ds_[p]', 'data':[4, 5, 6]}
```
