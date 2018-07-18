using Dapper;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Dapper;
using System.Data;
using System.Linq;

namespace JsonGen.Db.Tests
{
    [TestClass]
    public class TestDbDataProvider
    {
        public class DataType
        {
            public long Date { get; set; }
            public int Score { get; set; }
            public int Id { get; set; }
        }

        [TestMethod]
        public void DbDataProvider_should_return_data()
        {
            var connection = new Mock<IDbConnection>();

            var data = new[]
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

            connection.SetupDapper(c => c.Query<DataType>(It.IsAny<string>(), null, null, true, null, null))
                      .Returns(data);

            var myDataProvider = new DbDataProvider(connection.Object, "MyStoreProc");
            var actualData = myDataProvider.GetData();
            actualData.Should().NotBeNullOrEmpty()
                .And.HaveCount(2);
            actualData.ToList().ForEach(row =>
            {
                data.Where(d => d.Id == row.Id).Should().HaveCount(1);
            });
        }
    }
}