using MigrateMPData.Models;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
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
        private Mock<IDataReader> dataReader;
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
            dataReader = new Mock<IDataReader>();
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

            dbConnection.Setup(mocked => mocked.Open());
            dbConnection.Setup(mocked => mocked.Close());
            dbConnection.Setup(mocked => mocked.CreateCommand()).Returns(dbCommand.Object);
            dbConnection.Setup(mocked => mocked.BeginTransaction(IsolationLevel.ReadUncommitted)).Returns(dbTransaction.Object);
            dbCommand.SetupSet(mocked => mocked.CommandType = CommandType.Text).Verifiable();
            dbCommand.SetupSet(mocked => mocked.Transaction = dbTransaction.Object).Verifiable();
            dbCommand.SetupSet(mocked => mocked.CommandText = It.IsRegex(@"^INSERT INTO dest\.table1.*(column_name).*FROM src\.table1 WHERE 1 = 1 EXCEPT.*FROM dest\.table1.*")).Verifiable();
            dbCommand.SetupSet(mocked => mocked.CommandText = "SET IDENTITY_INSERT dest.table1 ON").Verifiable();
            dbCommand.SetupSet(mocked => mocked.CommandText = "SET IDENTITY_INSERT dest.table1 OFF").Verifiable();
            dbCommand.Setup(mocked => mocked.ExecuteReader()).Returns(dataReader.Object);
            dbCommand.Setup(mocked => mocked.ExecuteNonQuery()).Returns(1);
            dbTransaction.Setup(mocked => mocked.Commit());
            dbCommand.Setup(mocked => mocked.Dispose());

            dbCommand.SetupSet(mocked => mocked.CommandType = CommandType.Text).Verifiable();
            dbCommand.SetupSet(mocked => mocked.CommandText = "SELECT CONCAT('[', [Column_Name], ']') FROM src.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'table1' ORDER BY ordinal_position").Verifiable();
            dbCommand.Setup(mocked => mocked.Dispose());
            dataReader.Setup(mocked => mocked.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);
            dataReader.Setup(mocked => mocked.GetString(0)).Returns("column_name");
            dataReader.Setup(mocked => mocked.Close());
            dataReader.Setup(mocked => mocked.Dispose());

            Assert.IsTrue(fixture.moveData(table, true));

            dbConnection.VerifyAll();
            dbCommand.Verify(mocked => mocked.ExecuteNonQuery(), Times.Exactly(3));
            dbCommand.VerifyAll();
            dbTransaction.VerifyAll();
            dataReader.VerifyAll();
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

            dbConnection.Setup(mocked => mocked.Open());
            dbConnection.Setup(mocked => mocked.Close());
            dbConnection.Setup(mocked => mocked.CreateCommand()).Returns(dbCommand.Object);
            dbConnection.Setup(mocked => mocked.BeginTransaction(IsolationLevel.ReadUncommitted)).Returns(dbTransaction.Object);
            dbCommand.SetupSet(mocked => mocked.CommandType = CommandType.Text).Verifiable();
            dbCommand.SetupSet(mocked => mocked.Transaction = dbTransaction.Object).Verifiable();
            dbCommand.SetupSet(mocked => mocked.CommandText = It.IsRegex(@"^INSERT INTO dest\.table1.*FROM src\.table1  EXCEPT.*FROM dest\.table1.*")).Verifiable();
            dbCommand.SetupSet(mocked => mocked.CommandText = "SELECT CONCAT('[', [Column_Name], ']') FROM src.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'table1' ORDER BY ordinal_position").Verifiable();
            dbCommand.SetupSet(mocked => mocked.CommandText = "SET IDENTITY_INSERT dest.table1 ON").Verifiable();
            dbCommand.SetupSet(mocked => mocked.CommandText = "SET IDENTITY_INSERT dest.table1 OFF").Verifiable();
            dbCommand.Setup(mocked => mocked.ExecuteReader()).Returns(dataReader.Object);
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
            dbCommand.Verify(mocked => mocked.ExecuteNonQuery(), Times.Exactly(3));
            dbCommand.VerifyAll();
            dbTransaction.VerifyAll();
        }
    }
}
