using Dapper;
using FluentAssertions;
using Moq;
using Moq.Dapper;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace JsonGen.Db.Tests
{
    public class DataType
    {
        public long Date { get; set; }
        public int Score { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class TestDbDataProvider
    {

        DataType[] data = new[]
        {
            new DataType {
                Date = 2490313600000,
                Score = 100,
                Id = 1000,
                Name = "Test1"
            },
            new DataType {
                Date = 2490313600000,
                Score = 100,
                Id = 1001,
                Name = "Test2"
            }
        };

        [Fact]
        public async Task DbDataProvider_should_return_data()
        {
            var connection = new Mock<IDbConnection>();
            
            connection.SetupDapperAsync(c => c.QueryAsync<DataType>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(data);

            var myDataProvider = new DbDataProvider { DbConnection = connection.Object, Query = "MyQuery" };
            var actualData = await myDataProvider.GetDataAsync();
            actualData.Should().NotBeNullOrEmpty()
                .And.HaveCount(2);
            actualData.ToList().ForEach(row =>
            {
                data.Where(d => d.Id == row.Id).Should().HaveCount(1);
            });
        }

        [Fact]
        public async Task DbDataProvider_should_apply_filter_to_query()
        {
            var connection = new Mock<IDbConnection>();

            connection.SetupDapperAsync(c => c.QueryAsync<DataType>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(data);

            var myDataProvider = new DbDataProvider { DbConnection = connection.Object, Query = "Select * from MyView" };
            await myDataProvider.GetDataAsync(new[] {
                new Filter { FieldName = "Id", Value = 1000, Operator = Filter.Operators.G },
                new Filter { FieldName = "Name", Value = "Test" }
            });
            myDataProvider.Query.Should().BeEquivalentTo("Select * from MyView where Id > 1000 and Name = 'Test'");            
        }

        [Fact]
        public async Task DbDataProvider_should_apply_filter_to_query_with_where()
        {
            var connection = new Mock<IDbConnection>();

            connection.SetupDapperAsync(c => c.QueryAsync<DataType>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(data);

            var myDataProvider = new DbDataProvider { DbConnection = connection.Object, Query = "Select * from MyView where score > 1000" };
            await myDataProvider.GetDataAsync(new[] {
                new Filter { FieldName = "Id", Value = 1000, Operator = Filter.Operators.G },
                new Filter { FieldName = "Name", Value = "Test" }
            });
            myDataProvider.Query.Should().BeEquivalentTo("Select * from MyView where score > 1000 and Id > 1000 and Name = 'Test'");
        }

    }
}