using MigrateMPData.Models;
using Moq;
using NUnit.Framework;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Linq;

namespace MigrateMPData.Test
{
    [TestFixture]
    public class MinistryPlatformTableConfigReaderTest
    {
        private MinistryPlatformTableConfigReader fixture;

        private string inputFileContents;

        private System.Collections.Generic.List<MinistryPlatformTable> expectedTables;

        [SetUp]
        public void SetUp()
        {
            fixture = new MinistryPlatformTableConfigReader();

            expectedTables = new System.Collections.Generic.List<MinistryPlatformTable>();

            StringWriter sw = new StringWriter();
            sw.WriteLine("# this is a comment");
            sw.WriteLine("-- this is another comment");
            sw.WriteLine("' this,is,yet another comment");
            sw.WriteLine("table1,,");
            expectedTables.Add(new MinistryPlatformTable
            {
                tableName = "table1",
            });

            sw.WriteLine("// this is another comment in between some tables");

            sw.WriteLine("table2_no_join,filter2,");
            expectedTables.Add(new MinistryPlatformTable
            {
                tableName = "table2_no_join",
                filterClause = "filter2"
            });

            sw.WriteLine("table3_no_filter,,join3");
            expectedTables.Add(new MinistryPlatformTable
            {
                tableName = "table3_no_filter",
                joinClause = "join3"
            });

            sw.WriteLine("table4,filter4,join4");
            expectedTables.Add(new MinistryPlatformTable
            {
                tableName = "table4",
                joinClause = "join4",
                filterClause = "filter4"
            });

            sw.Close();

            inputFileContents = sw.ToString();
        }

        [Test]
        public void testReadConfigStream()
        {
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(inputFileContents));
            var tables = fixture.readConfig(stream);

            Assert.NotNull(tables);
            Assert.AreEqual(expectedTables.Count, tables.Count);
            for (int i = 0; i < expectedTables.Count; i++)
            {
                var e = expectedTables[i];
                var a = tables[i];
                Assert.AreEqual(e.tableName, a.tableName);
                Assert.AreEqual(e.filterClause, a.filterClause);
                Assert.AreEqual(e.joinClause, a.joinClause);
            }
        }
    }
}
