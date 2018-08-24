using FluentAssertions;
using FluentAssertions.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using JsonGen;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Report.ViewModels;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace JsonGenTestProject
{
    [TestClass]
    public class TestGenerator
    {
        [TestMethod]
        public void Generator_should_generate_report()
        {
            string layoutString = @"{
    'version': '1.0.0',
    'criteria': {
        'filters': {
            'roeType': 'deal'
        }
    },
    'page': {
        'res': {
            'filters': 'Filters',
            'apply': 'Apply',
            'clear': 'Clear'
        },
        'filterGroups': [
            {
                'name': 'type',
                'display': 'ROE Type'
            },
            {
                'name': 'whatif',
                'display': 'What If?'
            }
        ],
        'filters': [
            {
                'name': 'roeType',
                'display': null,
                'groupName': 'type',
                'primary': true,
                'type': 2,
                'lookups': [
                    {
                        'code': 'deal',
                        'text': 'Deal'
                    },
                    {
                        'code': 'customer',
                        'text': 'Relationship'
                    }
                ]
            }
        ]
    },
    'visuals': [
        {
            'primary': true,
            'title': 'ROE performance',
            'titleTag': 'Relationship',
            'type': 2,
            'chart': {
                'xAxisTitle': null,
                'xAxisDate': true,
                'xAxisDateFormat': '%d %b %y',
                'xColumn': 'date',
                'yAxisTitle': 'Score (%)',
                'yColumn': 'score',
                'scatterPlotTooltipFormat': '<b>Id</b>: {point.id} <br/><b>Score</b>: {point.y} % <br/><b>Date</b>: {point.x:%d %b %y}',
                'yAxisPlotLine': 68,
                'series': [
                    {
                        'name': 'Within Hurdle',
                        'color': 'rgba(223, 83, 83, .5)',
                        'data': []
                    }
                ]
            }
        }
    ]
}";
            // Arrange
            var metadata = new Metadata
            {
                Layout = new JsonGen.Layout { Content = layoutString },
                Labels = new Labels { },
                DataSources = new DataSource[0]
            };
            var metadataProvider = new Mock<IMetadataProvider>();
            metadataProvider.Setup(mp => mp.GetMetadata(It.IsAny<string>())).Returns(metadata);
            var generator = new Generator(metadataProvider.Object);

            // Action
            var report = generator.Generate("Report");

            // Assertion
            report.Should().NotBeNull();
            var analyticModel = JsonConvert.DeserializeObject<AnalyticsModel>(report);
            analyticModel.Should().NotBeNull();
            analyticModel.Version.Should().Be("1.0.0");
            analyticModel.Visuals?.FirstOrDefault()
                    .Chart?.Series?.FirstOrDefault()
                    .Data.Should().BeEmpty();
        }

        public class ReportModel
        {
            public Parent Parent { get; set; }
        }

        public class Parent
        {
            public Child Child { get; set; }
            public Data[] Data { get; set; }
        }

        public class Child
        {
            public Data[] Data { get; set; }
        }

        public class Data
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
        }

        public class DataProviderA : IDataProvider
        {
            public async Task<IEnumerable<dynamic>> GetDataAsync() => await Task.FromResult(new[]
            {
                new { x = 1, y = 2, z = 3 }
            });
        }

        public class DataProviderB : IDataProvider
        {
            public async Task<IEnumerable<dynamic>> GetDataAsync() => await Task.FromResult(new[]
            {
                new { x = 10, y = 20, z = 30 }
            });
        }

        public class DataProviderC : IFilterableDataProvider
        {
            public async Task<IEnumerable<dynamic>> GetDataAsync() => await Task.FromResult(new[]
            {
                new { x = 1 }, 
                new { x = 2 }, 
                new { x = 3 }, 
            });

            public async Task<IEnumerable<dynamic>> GetDataAsync(System.Func<dynamic, bool> predicate)
            {
                return (await GetDataAsync()).Where(predicate);
            }
        }

        public class ScalarDataProvider : IScalarDataProvider
        {
            public Task<IEnumerable<dynamic>> GetDataAsync()
            {
                throw new System.NotImplementedException();
            }

            public async Task<dynamic> GetScalarDataAsync() => await Task.FromResult(1);
           
        }

        [TestMethod]
        public void Generator_should_replace_data()
        {
            // Arrange
            var layoutString = File.ReadAllText("Jsons\\Test1-Layout.json");
            var layout = JsonConvert.DeserializeObject<JsonGen.Layout>(layoutString);
            var metadata = new Metadata
            {
                Layout = layout,
                Labels = new Labels(),
                DataSources = new[] {
                    new DataSource
                    {
                        Key = "A",
                        DataProviderFullName = typeof(DataProviderA).FullName
                    },
                    new DataSource
                    {
                        Key = "B",
                        DataProviderFullName = typeof(DataProviderB).FullName
                    }
                }
            };

            var metadataProvider = new Mock<IMetadataProvider>();
            metadataProvider.Setup(mp => mp.GetMetadata(It.IsAny<string>())).Returns(metadata);

            var generator = new Generator(metadataProvider.Object);

            // Action 
            var report = generator.Generate("Report");

            // Assertion
            report.Should().NotBeNull();
            var reportModel = JsonConvert.DeserializeObject<ReportModel>(report);
            reportModel?.Parent?.Child?.Data?.Should().HaveCount(1);
            reportModel?.Parent?.Child?.Data[0].X.Should().Be(1);
            reportModel?.Parent?.Child?.Data[0].Y.Should().Be(2);
            reportModel?.Parent?.Child?.Data[0].Z.Should().Be(3);
            reportModel?.Parent?.Data[0].X.Should().Be(10);
            reportModel?.Parent?.Data[0].Y.Should().Be(20);
            reportModel?.Parent?.Data[0].Z.Should().Be(30);
        }

        [TestMethod]
        public void Generator_should_throw_if_metadataprovider_return_metadata_null()
        {
            // Arrange
            var metadataProvider = new Mock<IMetadataProvider>();
            metadataProvider.Setup(mdp => mdp.GetMetadata(It.IsAny<string>())).Returns<Metadata>(null);
            var generator = new Generator(metadataProvider.Object);

            // Action + Assertion
            generator.Invoking(g => g.Generate(It.IsAny<string>())).Should().Throw<GenerateException>();
        }

        [TestMethod]
        public void Generator_should_ignore_datasources_with_ApplyFilter_option_set_to_false()
        {
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new JsonGen.Layout { Content = "{'node1':{'_dataSource':'C', 'data':[{'x':''}]},'node2':{'_dataSource':'D', 'data':[{'x':''}]}}" },
                DataSources = new[] { 
                    new DataSource { DataProviderFullName = typeof(DataProviderC).FullName, Key = "C" },
                    new DataSource { DataProviderFullName = typeof(DataProviderC).FullName, Key = "D",
                        Options = new DatasourceOptions {ApplyFilter = false }
                    }
                }
            });
            var generator = new Generator(metadataProvider);
            var json = generator.Generate("MyMetadata", predicate: row => row.x > 2);
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expected = JObject.Parse("{'node1':{'_dataSource':'C', 'data':[{'x':3}]}, 'node2':{'_dataSource':'D', 'data':[{'x':1},{'x':2},{'x':3}]}}");
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Generator_should_get_scalar_values()
        {
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new JsonGen.Layout { Content = "{'node1':{'_dataSource':'S', 'data': 'x' } }" },
                DataSources = new[] {
                    new DataSource { DataProviderFullName = typeof(ScalarDataProvider).FullName, Key = "S" }
                }
            });
            var generator = new Generator(metadataProvider);
            var json = generator.Generate("MyMetadata", predicate: row => row.x > 2);
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expected = JObject.Parse("{'node1':{'_dataSource':'S', 'data':1 }}");
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Generator_should_get_scalar_values_in_second_level()
        {
            var layout = @"{
            'criteria': {
                'filters': {
                    '_dataSource': 'S',
			        'data': 'reportperiod'
                    }
                }
            } ";
            var metadataProvider = new BasicMetadataProvider(_ => new Metadata
            {
                Layout = new JsonGen.Layout { Content = layout },
                DataSources = new[] {
                    new DataSource { DataProviderFullName = typeof(ScalarDataProvider).FullName, Key = "S" }
                }
            });
            var generator = new Generator(metadataProvider);
            var json = generator.Generate("MyMetadata", predicate: row => row.x > 2);
            json.Should().NotBeNull();
            var actual = JObject.Parse(json);
            var expected = JObject.Parse("{'criteria':{'filters':{'_dataSource':'S', 'data':1 }}}");
            actual.Should().BeEquivalentTo(expected);

        }
    }
}
