# JsonGen
A Json generator engine based on the metadata including layout and data providers defined in .Net languages.

## Create metadata:
Metadata class build of 3 main components: Layout, Datasources and Labels

## Layout
Simply it holds the content as a Json

## DataSources
An array of datasources. Each datasource has a key, a list of fields and and the full name of a class implementing IDataProvider

### IDataProvider
Is the interface must be implemented by any data provider and has one simple method GetData which returns an IEnumerable of dynamic

## Labels

