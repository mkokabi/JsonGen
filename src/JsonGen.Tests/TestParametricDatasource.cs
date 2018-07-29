using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Json;
using JsonGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace JsonGenTestProject
{
    [TestClass]
    public class TestParametricDatasource
    {
        public class DataProviderA : IDataProvider
        {
            public async Task<IEnumerable<dynamic>> GetDataAsync()
            {
                return await Task.FromResult((new dynamic[] { 1, 2, 3 }).AsEnumerable());
            }
        }
        public class DataProviderB : IDataProvider
        {
            public async Task<IEnumerable<dynamic>> GetDataAsync()
            {
                return await Task.FromResult((new dynamic[] { 4, 5, 6 }).AsEnumerable());
            }
        }

        [TestMethod]
        public void Datasource_should_switch_based_on_parameters()
        {
            var basicMetadataProvider = new BasicMetadataProvider(_ => new Metadata
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
            var json = generator.Generate("MetadataName", parameters: new Dictionary<string, dynamic> { { "p", "B" } });

            json.Should().NotBeNullOrEmpty();
            var expected = JObject.Parse("{'_dataSource':'ds_[p]', 'data':[4, 5, 6]}");
            var actual = JObject.Parse(json);
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
