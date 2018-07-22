using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using JsonGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JsonGenTestProject
{
    [TestClass]
    public class TestFilterableDataProvider
    {
        public class DataProvider : IFilterableDataProvider
        {
            public async Task<IEnumerable<dynamic>> GetDataAsync() => await Task.FromResult(new[]
            {
                new { x = 1, y = 2, z = 3 },
                new { x = 2, y = 2, z = 3 },
                new { x = 3, y = 2, z = 3 }
            });

            public async Task<IEnumerable<dynamic>> GetDataAsync(Func<dynamic, bool> predicate)
            {
                var result = await GetDataAsync();
                return result.Where(predicate);
            }
        }

        [TestMethod]
        public async Task Filterable_dataprovider_should_filter_the_data()
        {
            var dataProvider = new DataProvider();
            var data = await dataProvider.GetDataAsync(r => r.x >= 2);
            data.Should().HaveCount(2).And.Contain(new[] {
                new { x = 2, y = 2, z = 3 },
                new { x = 3, y = 2, z = 3 }
            });
        }
    }
}
