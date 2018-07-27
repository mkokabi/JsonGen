using Dapper;
using FluentAssertions;
using Moq;
using Moq.Dapper;
using Newtonsoft.Json.Linq;
using System.Data;
using Xunit;

namespace JsonGen.Db.Tests
{
    public class TestDBExtensions
    {
        DataType[] data = new[]
        {
            new DataType {
                Date = 2490313600000,
                Score = 100,
                Id = 1000,
                Name = "TestA"
            },
            new DataType {
                Date = 2490313600000,
                Score = 101,
                Id = 1001,
                Name = "TestB"
            }
        };

        [Fact]
        public void Generator_should_filter_using_filters()
        {
            var connection = new Mock<IDbConnection>();
            connection.SetupDapperAsync(c => c.QueryAsync<DataType>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(data);

            // var dbDataProvider = new DbDataProvider();

            var myMetadataProvider = new BasicMetadataProvider(_ => 
                new Metadata
                {
                    Layout = new Layout { Content = "{'_dataSource':'A', 'data':[]}" },
                    Labels = new Labels(),
                    DataSources = new[] {
                        new DataSource {
                            Key = "A",
                            DataProviderFullName = typeof(DbDataProvider).FullName,
                            DbConnection = connection.Object,
                            Query = "MyQuery"
                        } }
                });
            var jsonGenerator = new Generator(myMetadataProvider);

            var json = jsonGenerator.Generate("metadataName", data => data.Score > 100);
            json.Should().NotBeNull();

            var actual = JObject.Parse(json);
            var expected = JObject.Parse(@"{
  '_dataSource': 'A',
  'data': [
    {
      'Date': 2490313600000,
      'Score': 100,
      'Id': 1001,
      'Name': 'TestB'
    }
  ]
}");
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
