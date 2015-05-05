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
        private Mock<IDbConnection> sourceDbConnection;
        private Mock<IDbConnection> targetDbConnection;
        private Mock<IDbCommand> sourceDbCommand;
        private Mock<IDbCommand> targetDbCommand;
        private Mock<IDbTransaction> dbTransaction;
        private Mock<IDataReader> sourceDbColumnNameReader;
        private Mock<IDataReader> sourceDbDataReader;
        private Mock<IDbDataParameter> dataParameter;
        private Mock<IDataParameterCollection> preparedStatementParameterCollection;
        private DataTable schemaTable;
        private string sourceDbName;
        private string targetDbName;

        [SetUp]
        public void SetUp()
        {
            sourceDbName = "src";
            targetDbName = "dest";
            sourceDbConnection = new Mock<IDbConnection>(MockBehavior.Strict);
            targetDbConnection = new Mock<IDbConnection>(MockBehavior.Strict);
            fixture = new MinistryPlatformDataMover(sourceDbConnection.Object, targetDbConnection.Object, sourceDbName, targetDbName);
            sourceDbCommand = new Mock<IDbCommand>(MockBehavior.Strict);
            targetDbCommand = new Mock<IDbCommand>(MockBehavior.Strict);
            dbTransaction = new Mock<IDbTransaction>(MockBehavior.Strict);
            dataParameter = new Mock<IDbDataParameter>(MockBehavior.Strict);
            sourceDbDataReader = new Mock<IDataReader>(MockBehavior.Strict);
            sourceDbColumnNameReader = new Mock<IDataReader>(MockBehavior.Strict);
            preparedStatementParameterCollection = new Mock<IDataParameterCollection>(MockBehavior.Strict);
            schemaTable = new DataTable();
            schemaTable.Rows.Add(schemaTable.NewRow());
            schemaTable.Columns.Add("ColumnSize", typeof(int));
            schemaTable.Columns.Add("NumericPrecision", typeof(short));
            schemaTable.Columns.Add("NumericScale", typeof(short));
            schemaTable.Rows[0].SetField<int>("ColumnSize", 1);
            schemaTable.Rows[0].SetField<short>("NumericPrecision", 2);
            schemaTable.Rows[0].SetField<short>("NumericScale", 3);
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

            sourceDbConnection.Setup(mocked => mocked.Open());
            sourceDbConnection.Setup(mocked => mocked.Close());
            sourceDbConnection.Setup(mocked => mocked.CreateCommand()).Returns(sourceDbCommand.Object);
            sourceDbCommand.SetupSet(mocked => mocked.CommandType = CommandType.Text).Verifiable();
            sourceDbCommand.Setup(mocked => mocked.ExecuteReader()).Returns(new Queue<IDataReader>(new[] { sourceDbColumnNameReader.Object, sourceDbColumnNameReader.Object, sourceDbDataReader.Object }).Dequeue);
            sourceDbColumnNameReader.Setup(mocked => mocked.Read()).Returns(new Queue<bool>(new[] { true, false, true, false }).Dequeue);
            sourceDbColumnNameReader.Setup(mocked => mocked.GetString(0)).Returns("column_name");
            sourceDbColumnNameReader.Setup(mocked => mocked.Close());
            sourceDbColumnNameReader.Setup(mocked => mocked.Dispose());
            sourceDbDataReader.SetupGet(mocked => mocked.FieldCount).Returns(1);
            sourceDbDataReader.Setup(mocked => mocked.GetSchemaTable()).Returns(schemaTable);
            sourceDbDataReader.Setup(mocked => mocked.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);
            sourceDbDataReader.Setup(mocked => mocked.GetName(0)).Returns("column_name");
            sourceDbDataReader.Setup(mocked => mocked.GetFieldType(0)).Returns(typeof(string));
            sourceDbDataReader.Setup(mocked => mocked.GetValue(0)).Returns("value1");

            sourceDbCommand.SetupSet(mocked => mocked.CommandType = CommandType.Text).Verifiable();
            sourceDbCommand.SetupSet(mocked => mocked.CommandText = "SELECT column_name FROM src.table1 WHERE 1 = 1 EXCEPT SELECT * FROM dest.table1").Verifiable();
            sourceDbCommand.SetupSet(mocked => mocked.CommandText = "SELECT CONCAT('[', [Column_Name], ']') FROM src.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'table1' ORDER BY ordinal_position").Verifiable();
            sourceDbCommand.Setup(mocked => mocked.Dispose());

            targetDbConnection.Setup(mocked => mocked.Open());
            targetDbConnection.Setup(mocked => mocked.Close());
            targetDbConnection.Setup(mocked => mocked.CreateCommand()).Returns(new Queue<IDbCommand>(new [] {targetDbCommand.Object, targetDbCommand.Object, targetDbCommand.Object}).Dequeue);
            targetDbConnection.Setup(mocked => mocked.BeginTransaction(IsolationLevel.ReadUncommitted)).Returns(dbTransaction.Object);
            targetDbCommand.SetupSet(mocked => mocked.CommandType = CommandType.Text).Verifiable();
            targetDbCommand.SetupGet(mocked => mocked.CommandText).Returns("");
            targetDbCommand.Setup(mocked => mocked.CreateParameter()).Returns(dataParameter.Object);
            targetDbCommand.Setup(mocked => mocked.Prepare());
            preparedStatementParameterCollection.SetupGet(mocked => mocked[0]).Returns(dataParameter.Object);
            preparedStatementParameterCollection.Setup(mocked => mocked.Add(It.IsAny<IDbDataParameter>())).Returns(1);
            targetDbCommand.SetupGet(mocked => mocked.Parameters).Returns(preparedStatementParameterCollection.Object);
            dataParameter.SetupSet(mocked => mocked.ParameterName = "@column_name");
            dataParameter.SetupSet(mocked => mocked.DbType = DbType.String);
            dataParameter.SetupSet(mocked => mocked.Size = 1);
            dataParameter.SetupSet(mocked => mocked.Precision = 2);
            dataParameter.SetupSet(mocked => mocked.Scale = 3);
            dataParameter.SetupSet(mocked => mocked.Value = "value1");
            dataParameter.SetupGet(mocked => mocked.ParameterName).Returns("@column_name");
            dataParameter.SetupGet(mocked => mocked.DbType).Returns(DbType.String);
            targetDbCommand.Setup(mocked => mocked.Dispose());

            targetDbCommand.SetupSet(mocked => mocked.Transaction = dbTransaction.Object).Verifiable();
            targetDbCommand.SetupSet(mocked => mocked.CommandText = "INSERT INTO dest.table1 (column_name) VALUES (@column_name) ").Verifiable();
            targetDbCommand.SetupSet(mocked => mocked.CommandText = "SET IDENTITY_INSERT dest.table1 ON").Verifiable();
            targetDbCommand.SetupSet(mocked => mocked.CommandText = "SET IDENTITY_INSERT dest.table1 OFF").Verifiable();
            targetDbCommand.Setup(mocked => mocked.ExecuteNonQuery()).Returns(1);
            dbTransaction.Setup(mocked => mocked.Commit());


            Assert.IsTrue(fixture.moveData(table, true));

            sourceDbConnection.VerifyAll();
            targetDbCommand.Verify(mocked => mocked.ExecuteNonQuery(), Times.Exactly(3));
            sourceDbCommand.VerifyAll();
            dbTransaction.VerifyAll();
            sourceDbDataReader.VerifyAll();
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

            sourceDbConnection.Setup(mocked => mocked.Open());
            sourceDbConnection.Setup(mocked => mocked.Close());
            sourceDbConnection.Setup(mocked => mocked.CreateCommand()).Returns(sourceDbCommand.Object);
            sourceDbCommand.SetupSet(mocked => mocked.CommandType = CommandType.Text).Verifiable();
            sourceDbCommand.Setup(mocked => mocked.ExecuteReader()).Returns(new Queue<IDataReader>(new[] { sourceDbColumnNameReader.Object, sourceDbColumnNameReader.Object, sourceDbDataReader.Object }).Dequeue);
            sourceDbColumnNameReader.Setup(mocked => mocked.Read()).Returns(new Queue<bool>(new[] { true, false, true, false }).Dequeue);
            sourceDbColumnNameReader.Setup(mocked => mocked.GetString(0)).Returns("column_name");
            sourceDbColumnNameReader.Setup(mocked => mocked.Close());
            sourceDbColumnNameReader.Setup(mocked => mocked.Dispose());
            sourceDbDataReader.SetupGet(mocked => mocked.FieldCount).Returns(1);
            sourceDbDataReader.Setup(mocked => mocked.GetSchemaTable()).Returns(schemaTable);
            sourceDbDataReader.Setup(mocked => mocked.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);
            sourceDbDataReader.Setup(mocked => mocked.GetName(0)).Returns("column_name");
            sourceDbDataReader.Setup(mocked => mocked.GetFieldType(0)).Returns(typeof(string));
            sourceDbDataReader.Setup(mocked => mocked.GetValue(0)).Returns("value1");

            sourceDbCommand.SetupSet(mocked => mocked.CommandType = CommandType.Text).Verifiable();
            sourceDbCommand.SetupSet(mocked => mocked.CommandText = "SELECT column_name FROM src.table1  EXCEPT SELECT * FROM dest.table1").Verifiable();
            sourceDbCommand.SetupSet(mocked => mocked.CommandText = "SELECT CONCAT('[', [Column_Name], ']') FROM src.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'table1' ORDER BY ordinal_position").Verifiable();
            sourceDbCommand.Setup(mocked => mocked.Dispose());

            targetDbConnection.Setup(mocked => mocked.Open());
            targetDbConnection.Setup(mocked => mocked.Close());
            targetDbConnection.Setup(mocked => mocked.CreateCommand()).Returns(new Queue<IDbCommand>(new[] { targetDbCommand.Object, targetDbCommand.Object, targetDbCommand.Object }).Dequeue);
            targetDbConnection.Setup(mocked => mocked.BeginTransaction(IsolationLevel.ReadUncommitted)).Returns(dbTransaction.Object);
            targetDbCommand.SetupSet(mocked => mocked.CommandType = CommandType.Text).Verifiable();
            targetDbCommand.SetupGet(mocked => mocked.CommandText).Returns("");
            targetDbCommand.Setup(mocked => mocked.CreateParameter()).Returns(dataParameter.Object);
            targetDbCommand.Setup(mocked => mocked.Prepare());
            preparedStatementParameterCollection.SetupGet(mocked => mocked[0]).Returns(dataParameter.Object);
            preparedStatementParameterCollection.Setup(mocked => mocked.Add(It.IsAny<IDbDataParameter>())).Returns(1);
            targetDbCommand.SetupGet(mocked => mocked.Parameters).Returns(preparedStatementParameterCollection.Object);
            dataParameter.SetupSet(mocked => mocked.ParameterName = "@column_name");
            dataParameter.SetupSet(mocked => mocked.DbType = DbType.String);
            dataParameter.SetupSet(mocked => mocked.Size = 1);
            dataParameter.SetupSet(mocked => mocked.Precision = 2);
            dataParameter.SetupSet(mocked => mocked.Scale = 3);
            dataParameter.SetupSet(mocked => mocked.Value = "value1");
            dataParameter.SetupGet(mocked => mocked.ParameterName).Returns("@column_name");
            dataParameter.SetupGet(mocked => mocked.DbType).Returns(DbType.String);
            targetDbCommand.Setup(mocked => mocked.Dispose());

            targetDbCommand.SetupSet(mocked => mocked.Transaction = dbTransaction.Object).Verifiable();
            targetDbCommand.SetupSet(mocked => mocked.CommandText = "INSERT INTO dest.table1 (column_name) VALUES (@column_name) ").Verifiable();
            targetDbCommand.SetupSet(mocked => mocked.CommandText = "SET IDENTITY_INSERT dest.table1 ON").Verifiable();
            targetDbCommand.SetupSet(mocked => mocked.CommandText = "SET IDENTITY_INSERT dest.table1 OFF").Verifiable();
            targetDbCommand.Setup(mocked => mocked.ExecuteNonQuery()).Throws(dbException.Object);
            dbTransaction.Setup(mocked => mocked.Rollback());

            try
            {
                fixture.moveData(table, true);
            }
            catch (DbException e)
            {
                Assert.AreSame(dbException.Object, e);
            }

            sourceDbConnection.VerifyAll();
            targetDbCommand.Verify(mocked => mocked.ExecuteNonQuery(), Times.Exactly(3));
            sourceDbCommand.VerifyAll();
            dbTransaction.VerifyAll();
            sourceDbDataReader.VerifyAll();
        }
    }
}
