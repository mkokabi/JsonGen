using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JsonGen;

namespace ReportEngineTestProject
{
    [TestClass]
    public class TestModels
    {
        [TestMethod]
        public void Load_Metadata_from_Empty_Json()
        {
            string json = "{}";
            var metadata = Metadata.Load(json);
            metadata.Should().NotBeNull();
        }

        [TestMethod]
        public void Load_Metadata_from_Json()
        {
            string json = "{'Layout':{}, 'Labels':{}, 'DataSources':[]}";
            var metadata = Metadata.Load(json);
            metadata.Should().NotBeNull();
            metadata.Labels.Should().NotBeNull();
            metadata.Layout.Should().NotBeNull();
            metadata.DataSources.Should().NotBeNull().And.BeEmpty();
        }
    }
}
