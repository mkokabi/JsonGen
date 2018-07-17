using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using JsonGen;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Report.ViewModels;

namespace ReportEngineTestProject
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
            public IEnumerable<dynamic> GetData() => new[]
            {
                new { x = 1, y = 2, z = 3 }
            };
        }

        public class DataProviderB : IDataProvider
        {
            public IEnumerable<dynamic> GetData() => new[]
            {
                new { x = 10, y = 20, z = 30 }
            };
        }

        [TestMethod]
        public void Generator_should_replace_data()
        {
            // Arrange
            DataSource dataSourceA = new DataSource
            {
                Key = "A",
                Fields = new[] { "x", "y", "z" },
                DataProviderFullName = typeof(DataProviderA).FullName
            };

            DataSource dataSourceB = new DataSource
            {
                Key = "B",
                Fields = new[] { "xp", "yp", "zp" },
                DataProviderFullName = typeof(DataProviderB).FullName
            };

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
                        Fields = new[] {"x", "y", "z" },
                        DataProviderFullName = typeof(DataProviderA).FullName
                    },
                    new DataSource
                    {
                        Key = "B",
                        Fields = new[] {"xp", "yp", "zp" },
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
    }
}
