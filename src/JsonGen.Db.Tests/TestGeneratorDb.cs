using FluentAssertions;
using FluentAssertions.Json;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
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

        public class DbDataProvider : IDbDataProvider
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
                        DataProviderFullName = typeof(DbDataProvider).FullName,
                        DbConnection = connection.Object,
                        Query = "MyQuery"
                    },
                    new DataSource {
                        Key = "B",
                        DataProviderFullName = typeof(DbDataProvider).FullName,
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
    }
}
