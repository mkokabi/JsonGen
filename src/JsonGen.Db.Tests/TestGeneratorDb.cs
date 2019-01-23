using FluentAssertions;
using FluentAssertions.Json;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace JsonGen.Db.Tests
{
    public class TestGeneratorDb
    {
        private readonly ITestOutputHelper output;

        public TestGeneratorDb(ITestOutputHelper output)
        {
            this.output = output;
        }

        Layout layout = new Layout { Content = File.ReadAllText("Jsons\\Test1-Layout.json") };

        public class MyDbDataProvider : IDbDataProvider
        {
            public IDbConnection DbConnection { get; set; }
            public string Query { get; set; }

            public async Task<IEnumerable<dynamic>> GetDataAsync(Func<dynamic, bool> predicate)
            {
                return (await GetDataAsync()).Where(predicate);
            }

            public async Task<IEnumerable<dynamic>> GetDataAsync() => await Task.FromResult(new[]
            {
                new { x = 10, y = 20, z = 30, xp = 110, yp = 210, zp = 310 },
                new { x = 11, y = 21, z = 31, xp = 111, yp = 211, zp = 311 },
                new { x = 12, y = 22, z = 32, xp = 112, yp = 212, zp = 312 },
            });

            public Task<IEnumerable<dynamic>> GetDataAsync(Filter[] filters)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void DbGenerator_should_replace_filtered_data()
        {
            var connection = new Mock<IDbConnection>();

            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = layout,
                Labels = new Labels(),
                DataSources = new[]
                {
                    new DataSource {
                        Key = "A",
                        DataProviderFullName = typeof(MyDbDataProvider).FullName,
                        DbConnection = connection.Object,
                        Query = "MyQuery"
                    },
                    new DataSource {
                        Key = "B",
                        DataProviderFullName = typeof(MyDbDataProvider).FullName,
                        DbConnection = connection.Object,
                        Query = "MyQuery"
                    },
                    new DataSource {
                        Key = "C",
                        DataProviderFullName = typeof(MyDbDataProvider).FullName,
                        DbConnection = connection.Object,
                        Query = "MyQuery"
                    },
                    new DataSource {
                        Key = "D",
                        DataProviderFullName = typeof(MyDbDataProvider).FullName,
                        DbConnection = connection.Object,
                        Query = "MyQuery"
                    }
                }
            });
            var generator = new Generator(metadataProvider);
            var json = generator.Generate("myMeta", row => row.x < 12);
            output.WriteLine(json);
            json.Should().NotBeNull();
            JObject actual = JObject.Parse(json);
            JObject expected = JObject.Parse(File.ReadAllText("Jsons\\Test1-Output.json"));
            actual.Should().BeEquivalentTo(expected);
        }

        public class MyDataType
        {
            public string Name { get; set; }
        }

        #if DEBUG
                const string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Work\JsonGen\src\JsonGen.Db.Tests\TestDatabase.mdf;Integrated Security=True;Connect Timeout=30";
        #else
                string connStr => Environment.GetEnvironmentVariable("connStr");
        #endif

        [Fact]
        public void DbGenerator_should_throw_exception_showing_invalid_sql()
        {
            var invalidSql = "Select * from InvalidTable";
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new Layout { Content = "{'_dataSource': 'A', 'data': [ { 'name': 'X' }]}" },
                Labels = new Labels(),
                DataSources = new[]
                {
                    new DataSource {
                        Key = "A",
                        DataProviderFullName = typeof(DbDataProvider).FullName,
                        DbConnection = new SqlConnection(connStr),
                        Query = invalidSql,
                    }
                }
            });

            var generator = new Generator(metadataProvider);
            generator.Invoking(g => generator.GenerateAsync("myMeta").Wait())
                .Should().Throw<DbGenerateException>()
                .Where(exc => exc.Query == invalidSql && exc.InnerException is SqlException);
        }

        [Fact]
        public void DbGenerator_scalar_should_throw_exception_showing_invalid_sql()
        {
            var invalidSql = "Select max(id) from InvalidTable";
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new Layout { Content = "{'_dataSource': 'A', 'data': [ { 'name': 'X' }]}" },
                Labels = new Labels(),
                DataSources = new[]
                {
                    new DataSource {
                        Key = "A",
                        DataProviderFullName = typeof(DbDataProvider).FullName,
                        DbConnection = new SqlConnection(connStr),
                        Query = invalidSql,
                    }
                }
            });

            var generator = new Generator(metadataProvider);
            generator.Invoking(g => generator.GenerateAsync("myMeta").Wait())
                .Should().Throw<DbGenerateException>()
                .Where(exc => exc.Query == invalidSql && exc.InnerException is SqlException);
        }

        [Fact]
        public void DbGenerator_scalar_should_get_scalar_values_in_second_level()
        {
            var layout = @"{
            'criteria': {
                'filters': {
                    '_dataSource': 'S',
			        'data': 'reportperiod'
                    }
                }
            } ";
            {
                var sql = "Select max(id) from TestTable";
                var metadataProvider = new BasicMetadataProvider(_ => new Metadata
                {
                    Layout = new Layout { Content = layout },
                    Labels = new Labels(),
                    DataSources = new[]
                    {
                    new DataSource {
                        Key = "S",
                        DataProviderFullName = typeof(ScalarDbDataProvider).FullName,
                        DbConnection = new SqlConnection(connStr),
                        Query = sql,
                    }
                }
                });
                var generator = new Generator(metadataProvider);
                var json = generator.Generate("MyMetadata", predicate: row => row.x > 2);
                json.Should().NotBeNull();
                var actual = JObject.Parse(json);
                var expected = JObject.Parse("{'criteria':{'filters':{'_dataSource':'S', 'data':2 }}}");
                actual.Should().BeEquivalentTo(expected);

            }
        }


        [Fact]
        public void DbGenerator_scalar_with_select_query_should_throw_exception()
        {
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new Layout { Content = "{'_dataSource': 'A', 'data': [ { 'name': 'X' }]}" },
                Labels = new Labels(),
                DataSources = new[]
                {
                    new DataSource {
                        Key = "A",
                        DataProviderFullName = typeof(ScalarDbDataProvider).FullName,
                        DbConnection = new SqlConnection(connStr),
                        Query = "Select * from Table"
                    }
                }
            });

            var generator = new Generator(metadataProvider);
            generator.Invoking(g => generator.GenerateAsync("myMeta").Wait())
                .Should().Throw<GenerateException>()
                .Where(exc => exc.Message == "ScalarDataProvider needs a value.");
        }

        [Fact]
        public async Task DbGenerator_should_return_multiple_columns_data_from_db()
        {
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new Layout { Content = "{'_dataSource': 'A', 'data': [ { 'Id': 0, 'name': 'X' }]}" },
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
            var json = await generator.GenerateAsync("myMeta");
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expected = JObject.Parse("{'_dataSource': 'A', 'data': [ { 'Id': 1, 'Name': 'MK' }, { 'Id': 2, 'Name': 'AK' }]}");
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task DbGenerator_should_return_multiple_columns_data_from_db_in_single_row()
        {
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new Layout { Content = "{'_dataSource': 'A', 'data': [ { 'MaxId': 0, 'MaxName': 'X' }]}" },
                Labels = new Labels(),
                DataSources = new[]
                {
                    new DataSource {
                        Key = "A",
                        DataProviderFullName = typeof(DbDataProvider).FullName,
                        DbConnection = new SqlConnection(connStr),
                        Query = "Select Max(Id) as MaxId, Max(Name) as MaxName from TestTable",
                    }
                }
            });

            var generator = new Generator(metadataProvider);
            var json = await generator.GenerateAsync("myMeta");
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expected = JObject.Parse("{'_dataSource': 'A', 'data': [ { 'MaxId': 2, 'MaxName': 'MK' }]}");
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task DbGenerator_should_return_filtered_data_from_db()
        {
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new Layout { Content = "{'_dataSource': 'A', 'data': [ { 'name': 'X' }]}" },
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
            var json = await generator.GenerateAsync("myMeta", filters: new[] { new Filter { FieldName = "Id", Value = 1 } });
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expected = JObject.Parse("{'_dataSource': 'A', 'data': [ { 'Name': 'MK' }]}");
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task DbGenerator_should_replace_macros_with_filtes()
        {
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new Layout { Content = "{'_dataSource': 'A', 'data': [ { 'name': 'X' }]}" },
                Labels = new Labels(),
                DataSources = new[]
                {
                    new DataSource {
                        Key = "A",
                        DataProviderFullName = typeof(DbDataProvider).FullName,
                        DbConnection = new SqlConnection(connStr),
                        Query = "Select * from [TableName]",
                        Options = new DatasourceOptions { ReplaceMacrosOnly = true }
                    }
                }
            });

            var generator = new Generator(metadataProvider);
            var json = await generator.GenerateAsync("myMeta", 
                filters: new[] { new Filter { FieldName = "TableName", Value = "TestTable" } });
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expected = JObject.Parse("{'_dataSource': 'A', 'data': [ { 'Name': 'MK' }, { 'Name': 'AK' }]}");
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task DbGenerator_should_ignore_filtering_if_filteroption_is_set_to_false()
        {
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new Layout { Content = "{'_dataSource': 'A', 'data': [ { 'name': 'X' }]}" },
                Labels = new Labels(),
                DataSources = new[]
                {
                    new DataSource {
                        Key = "A",
                        DataProviderFullName = typeof(DbDataProvider).FullName,
                        DbConnection = new SqlConnection(connStr),
                        Query = "Select * from TestTable",
                        Options = new DatasourceOptions { ApplyFilter = false }
                    }
                }
            });

            var generator = new Generator(metadataProvider);
            var json = await generator.GenerateAsync("myMeta", 
                filters: new[] { new Filter { FieldName = "Id", Value = 1 } });
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expected = JObject.Parse("{'_dataSource': 'A', 'data': [ { 'Name': 'MK' }, { 'Name': 'AK' }]}");
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task DbGenerator_should_ignore_filtering_with_IgnoreFilteringOn()
        {
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new Layout { Content = "{'_dataSource': 'A', 'data': [ { 'name': 'X' }]}" },
                Labels = new Labels(),
                DataSources = new[]
                {
                    new DataSource {
                        Key = "A",
                        DataProviderFullName = typeof(DbDataProvider).FullName,
                        DbConnection = new SqlConnection(connStr),
                        Query = "Select * from TestTable",
                        Options = new DatasourceOptions { IgnoreFilteringOn = new [] { "id" } }
                    }
                }
            });

            var generator = new Generator(metadataProvider);
            var json = await generator.GenerateAsync("myMeta", 
                filters: new[] { new Filter { FieldName = "Id", Value = 1 } });
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expected = JObject.Parse("{'_dataSource': 'A', 'data': [ { 'Name': 'MK' }, { 'Name': 'AK' }]}");
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task DbGenerator_should_ignore_filtering_with_multiple_IgnoreFilteringOn()
        {
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new Layout { Content = "{'_dataSource': 'A', 'data': [ { 'name': 'X' }]}" },
                Labels = new Labels(),
                DataSources = new[]
                {
                    new DataSource {
                        Key = "A",
                        DataProviderFullName = typeof(DbDataProvider).FullName,
                        DbConnection = new SqlConnection(connStr),
                        Query = "Select * from TestTable",
                        Options = new DatasourceOptions { IgnoreFilteringOn = new [] { "id", "Name" } }
                    }
                }
            });

            var generator = new Generator(metadataProvider);
            var json = await generator.GenerateAsync("myMeta",
                filters: new[] {
                    new Filter { FieldName = "Id", Value = 1 },
                    new Filter { FieldName = "Name", Value = "MK" },
                });
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expected = JObject.Parse("{'_dataSource': 'A', 'data': [ { 'Name': 'MK' }, { 'Name': 'AK' }]}");
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task DbGenerator_should_replace_scalar_values()
        {
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
            var json = await generator.GenerateAsync("myMeta");
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expected = JObject.Parse("{'_dataSource': 'A', 'data': 2 }");
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task DbGenerator_should_replace_scalar_values_with_filter()
        {
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
            var json = await generator.GenerateAsync("myMeta", filters: new[] { new Filter { FieldName = "Id", Value = 1 } });
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expected = JObject.Parse("{'_dataSource': 'A', 'data': 1 }");
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task DbGenerator_should_replace_scalar_null_value()
        {
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
                        Query = "Select null from TestTable",
                    }
                }
            });

            var generator = new Generator(metadataProvider);
            var json = await generator.GenerateAsync("myMeta", filters: new[] { new Filter { FieldName = "Id", Value = 1 } });
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expected = JObject.Parse("{'_dataSource': 'A', 'data': null }");
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task DbGenerator_should_return_filtered_by_in_operator_on_integers()
        {
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new Layout { Content = "{'_dataSource': 'A', 'data': [ { 'name': 'X' }]}" },
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
            var json = await generator.GenerateAsync("myMeta", filters: new[] {
                new Filter {
                    FieldName = "Id",
                    Value = new [] { 1, 2 },
                    Operator = Filter.Operators.In
                } });
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expected = JObject.Parse("{'_dataSource': 'A', 'data': [ { 'Name': 'MK' }, { 'Name': 'AK' }]}");
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task DbGenerator_should_return_filtered_by_in_operator_on_strings()
        {
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new Layout { Content = "{'_dataSource': 'A', 'data': [ { 'name': 'X' }]}" },
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
            var json = await generator.GenerateAsync("myMeta", filters: new[] {
                new Filter {
                    FieldName = "Name",
                    Value = new [] { "AK" },
                    Operator = Filter.Operators.In
                } });
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expected = JObject.Parse("{'_dataSource': 'A', 'data': [ { 'Name': 'AK' }]}");
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void DbGenerator_should_throw_exception_if_filter_operator_is_in_but_value_is_not_array()
        {
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new Layout { Content = "{'_dataSource': 'A', 'data': [ { 'name': 'X' }]}" },
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
            generator.Invoking(g => generator.GenerateAsync("myMeta", filters: new[] {
                new Filter {
                    FieldName = "Id",
                    Value = 1,
                    Operator = Filter.Operators.In
                } }).Wait()).Should().Throw<GenerateException>();
        }

        [Fact]
        public async Task DbGenerator_should_return_filtered_by_between_operator_on_integers()
        {
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new Layout { Content = "{'_dataSource': 'A', 'data': [ { 'name': 'X' }]}" },
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
            var json = await generator.GenerateAsync("myMeta", filters: new[] {
                new Filter {
                    FieldName = "Id",
                    Value = new [] { 1, 2 },
                    Operator = Filter.Operators.In
                } });
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expected = JObject.Parse("{'_dataSource': 'A', 'data': [ { 'Name': 'MK' }, { 'Name': 'AK' }]}");
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void DbGenerator_should_throw_exception_if_filter_operator_is_between_but_value_is_not_array()
        {
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new Layout { Content = "{'_dataSource': 'A', 'data': [ { 'name': 'X' }]}" },
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
            generator.Invoking(g => generator.GenerateAsync("myMeta", filters: new[] {
                new Filter {
                    FieldName = "Id",
                    Value = 1,
                    Operator = Filter.Operators.Bw
                } }).Wait()).Should().Throw<GenerateException>();
        }

        [Fact]
        public void DbGenerator_should_throw_exception_if_filter_operator_is_between_but_value_doesnt_have_2_items()
        {
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new Layout { Content = "{'_dataSource': 'A', 'data': [ { 'name': 'X' }]}" },
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
            generator.Invoking(g => generator.GenerateAsync("myMeta", filters: new[] {
                new Filter {
                    FieldName = "Id",
                    Value = new [] { 1 },
                    Operator = Filter.Operators.Bw
                } }).Wait()).Should().Throw<GenerateException>();
        }

        [Fact]
        public async Task DbGenerator_should_support_multiple_level_datasources()
        {
            var layoutStr = @"{
	'parent': {
		'node': {
			'_dataSource': 'A',
			'data': ['Name'],
			'series': [{
				'name': 'some name',
				'type': 'some type',
				'_dataSource': 'B',
				'data': ['NameA']}
				]
		}		
	}
}
";
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new Layout { Content = layoutStr },
                Labels = new Labels(),
                DataSources = new[]
                {
                    new DataSource {
                        Key = "A",
                        DataProviderFullName = typeof(DbDataProvider).FullName,
                        DbConnection = new SqlConnection(connStr),
                        Query = "Select Name from TestTable",
                    },
                    new DataSource
                    {
                        Key = "B",
                        DataProviderFullName = typeof(DbDataProvider).FullName,
                        DbConnection = new SqlConnection(connStr),
                        Query = "Select NameA, NameB from AnotherTestTable",
                    }
                }
            });

            var generator = new Generator(metadataProvider);
            var json = await generator.GenerateAsync("myMeta");
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expectedjsonStr = @"{
	'parent': {
		'node': {
			'_dataSource': 'A',
			'data': ['MK', 'AK'],
			'series': [{
				'name': 'some name',
				'type': 'some type',
				'_dataSource': 'B',
				'data': ['MK', 'AK', 'DK']}
				]
		}		
	}
}
";
            var expected = JObject.Parse(expectedjsonStr);
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task DbGenerator_should_return_filtered_on_date_fields()
        {
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new Layout { Content = "{'_dataSource': 'A', 'data': [ { 'NameA': 'X' }]}" },
                Labels = new Labels(),
                DataSources = new[]
                {
                    new DataSource {
                        Key = "A",
                        DataProviderFullName = typeof(DbDataProvider).FullName,
                        DbConnection = new SqlConnection(connStr),
                        Query = "Select * from AnotherTestTable",
                    }
                }
            });

            var generator = new Generator(metadataProvider);
            var json = await generator.GenerateAsync("myMeta", filters: new[] {
                new Filter {
                    FieldName = "DOB",
                    Value = new DateTime(2018, 2, 1),
                    Operator = Filter.Operators.G
                } });
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expected = JObject.Parse("{'_dataSource': 'A', 'data': [ { 'NameA': 'AK' }, { 'NameA': 'DK' }]}");
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task DbGenerator_should_put_the_where_before_groupby()
        {
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
                        Query = "Select Name from TestTable Group by Name",
                    }
                }
            });

            var generator = new Generator(metadataProvider);
            var json = await generator.GenerateAsync("myMeta", filters: new[] {
                new Filter {
                    FieldName = "Id",
                    Value = 1,
                } });
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expected = JObject.Parse("{'_dataSource': 'A', 'data': [ { 'Name': 'MK' } ]}");
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task DbGenerator_should_get_data_from_sp()
        {
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
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expected = JObject.Parse("{'_dataSource': 'A', 'data': [ { 'Name': 'MK' }, { 'Name': 'AK' } ]}");
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
