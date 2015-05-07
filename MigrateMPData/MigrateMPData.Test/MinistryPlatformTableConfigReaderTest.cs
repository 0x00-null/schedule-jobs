using MigrateMPData.Models;
using NUnit.Framework;
using System.IO;
using System.Text;

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
            sw.WriteLine("table1,INSERT_ONLY,");
            expectedTables.Add(new MinistryPlatformTable
            {
                tableName = "table1",
                migrationType = MigrationType.INSERT_ONLY,
            });

            sw.WriteLine("// this is another comment in between some tables");

            sw.WriteLine("table2_no_type,,filter2");
            expectedTables.Add(new MinistryPlatformTable
            {
                tableName = "table2_no_type",
                filterClause = "filter2",
                migrationType = MigrationType.INSERT_OR_UPDATE,
            });

            sw.WriteLine("table3_no_filter,INSERT_ONLY,");
            expectedTables.Add(new MinistryPlatformTable
            {
                tableName = "table3_no_filter",
                migrationType = MigrationType.INSERT_ONLY,
            });

            sw.WriteLine("table4,INSERT_ONLY,filter4");
            expectedTables.Add(new MinistryPlatformTable
            {
                tableName = "table4",
                migrationType = MigrationType.INSERT_ONLY,
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
                Assert.AreEqual(e.migrationType, a.migrationType);
            }
        }
    }
}
