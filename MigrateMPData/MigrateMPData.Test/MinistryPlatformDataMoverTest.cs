using MigrateMPData.Models;
using Moq;
using NUnit.Framework;
using System.Data;
using System.Data.Common;


namespace MigrateMPData.Test
{
    [TestFixture]
    public class MinistryPlatformDataMoverTest
    {
        private MinistryPlatformDataMover fixture;
        private Mock<IDbConnection> dbConnection;
        private Mock<IDbCommand> dbCommand;
        private Mock<IDbTransaction> dbTransaction;
        private string sourceDbName;
        private string targetDbName;

        [SetUp]
        public void SetUp()
        {
            sourceDbName = "src";
            targetDbName = "dest";
            dbConnection = new Mock<IDbConnection>(MockBehavior.Strict);
            fixture = new MinistryPlatformDataMover(dbConnection.Object, sourceDbName, targetDbName);

            dbCommand = new Mock<IDbCommand>(MockBehavior.Strict);
            dbTransaction = new Mock<IDbTransaction>(MockBehavior.Strict);
        }

        [Test]
        public void testMoveDataWithoutExecute()
        {
            MinistryPlatformTable table = new MinistryPlatformTable
            {
                tableName = "table1",
                filterClause = "1 = 1"
            };

            Assert.IsTrue(fixture.moveData(table, false));
        }

        [Test]
        public void testMoveData()
        {
            MinistryPlatformTable table = new MinistryPlatformTable
            {
                tableName = "table1",
                filterClause = "1 = 1"
            };

            dbConnection.Setup(mocked => mocked.CreateCommand()).Returns(dbCommand.Object);
            dbConnection.Setup(mocked => mocked.BeginTransaction(IsolationLevel.ReadUncommitted)).Returns(dbTransaction.Object);
            dbCommand.SetupSet(mocked => mocked.CommandType = CommandType.Text).Verifiable();
            dbCommand.SetupSet(mocked => mocked.CommandText = It.IsRegex(@"^INSERT INTO dest\.table1.*FROM src\.table1 WHERE 1 = 1 EXCEPT.*FROM dest\.table1.*")).Verifiable();
            dbCommand.Setup(mocked => mocked.ExecuteNonQuery()).Returns(1);
            dbTransaction.Setup(mocked => mocked.Commit());
            dbCommand.Setup(mocked => mocked.Dispose());

            Assert.IsTrue(fixture.moveData(table, true));

            dbConnection.VerifyAll();
            dbCommand.VerifyAll();
            dbTransaction.VerifyAll();
        }

        [Test]
        public void testMoveDataWithDbException()
        {
            MinistryPlatformTable table = new MinistryPlatformTable
            {
                tableName = "table1",
                filterClause = ""
            };

            var dbException = new Mock<DbException>();

            dbConnection.Setup(mocked => mocked.CreateCommand()).Returns(dbCommand.Object);
            dbConnection.Setup(mocked => mocked.BeginTransaction(IsolationLevel.ReadUncommitted)).Returns(dbTransaction.Object);
            dbCommand.SetupSet(mocked => mocked.CommandType = CommandType.Text).Verifiable();
            dbCommand.SetupSet(mocked => mocked.CommandText = It.IsRegex(@"^INSERT INTO dest\.table1.*FROM src\.table1  EXCEPT.*FROM dest\.table1.*")).Verifiable();
            dbCommand.Setup(mocked => mocked.ExecuteNonQuery()).Throws(dbException.Object);
            dbTransaction.Setup(mocked => mocked.Rollback());
            dbCommand.Setup(mocked => mocked.Dispose());

            try
            {
                fixture.moveData(table, true);
            }
            catch (DbException e)
            {
                Assert.AreSame(dbException.Object, e);
            }

            dbConnection.VerifyAll();
            dbCommand.VerifyAll();
            dbTransaction.VerifyAll();
        }
    }
}
