using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using JsonGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace JsonGenTestProject
{
    [TestClass]
    public class AnotherSimpleForm
    {
        public class DataProvider : IDataProvider
        {
            public async Task<IEnumerable<dynamic>> GetDataAsync() => await Task.FromResult((new dynamic[]
                {
                    1, 2, 3
                }).AsEnumerable()
            );
        }

        [TestMethod]
        public void Another_simple_form()
        {
            var generator = new Generator(new BasicMetadataProvider(name => new Metadata
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
            generatedJson.Should().NotBeNull();
            var actual = JToken.Parse(generatedJson);
            var expected = JToken.Parse(@"{
                                            '_dataSource': 'W',
                                            'data': [1, 2, 3]
                                        }");
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
