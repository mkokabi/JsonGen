using System;
using System.Collections.Generic;
using System.Linq;
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
            public IEnumerable<dynamic> GetData() => new[]
            {
                new { x = 1, y = 2, z = 3 },
                new { x = 2, y = 2, z = 3 },
                new { x = 3, y = 2, z = 3 }
            };

            public IEnumerable<dynamic> GetData(Func<dynamic, bool> predicate)
            {
                return GetData().Where(predicate);
            }
        }

        [TestMethod]
        public void Filterable_dataprovider_should_filter_the_data()
        {
            var dataProvider = new DataProvider();
            var data = dataProvider.GetData(r => r.x >= 2);
            data.Should().HaveCount(2).And.Contain(new[] {
                new { x = 2, y = 2, z = 3 },
                new { x = 3, y = 2, z = 3 }
            });
        }
    }
}
