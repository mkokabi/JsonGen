using Dapper;
using FluentAssertions;
using Microsoft.CSharp;
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
    }
    public class TestDbDataProvider
    {

        DataType[] data = new[]
        {
            new DataType {
                Date = 2490313600000,
                Score = 100,
                Id = 1000
            },
            new DataType {
                Date = 2490313600000,
                Score = 100,
                Id = 1001
            }
        };

        [Fact]
        public async Task DbDataProvider_should_return_data()
        {
            var connection = new Mock<IDbConnection>();
            
            connection.SetupDapperAsync(c => c.QueryAsync<DataType>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(data);

            var myDataProvider = new DbDataProvider(connection.Object, "MyStoreProc");
            var actualData = await myDataProvider.GetDataAsync();
            actualData.Should().NotBeNullOrEmpty()
                .And.HaveCount(2);
            actualData.ToList().ForEach(row =>
            {
                data.Where(d => d.Id == row.Id).Should().HaveCount(1);
            });
        }

        [Fact]
        public async Task DbDataProvider_should_filter_data()
        {
            var connection = new Mock<IDbConnection>();

            connection.SetupDapperAsync(c => c.QueryAsync<DataType>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(data);

            var myDataProvider = new DbDataProvider(connection.Object, "MyStoreProc");
            var actualData = await myDataProvider.GetDataAsync(row => row.Id == 1000);
            actualData.Should().NotBeNullOrEmpty()
                .And.HaveCount(1);
            actualData.ToList().ForEach(row =>
            {
                data.Where(d => d.Id == 1000).Should().HaveCount(1);
            });
        }
    }
}