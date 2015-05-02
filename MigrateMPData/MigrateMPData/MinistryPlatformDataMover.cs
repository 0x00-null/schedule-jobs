using log4net;
using MigrateMPData.Interfaces;
using MigrateMPData.Models;
using MigrateMPData.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Reflection;

namespace MigrateMPData
{
    public class MinistryPlatformDataMover : IMinistryPlatformDataMover
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string sourceDatabaseName;

        private string targetDatabaseName;

        private IDbConnection dbConnection;

        public MinistryPlatformDataMover(IDbConnection dbConnection, string sourceDatabaseName, string targetDatabaseName)
        {
            this.dbConnection = dbConnection;
            this.sourceDatabaseName = sourceDatabaseName;
            this.targetDatabaseName = targetDatabaseName;
        }
        
        public bool moveData(MinistryPlatformTable table, bool execute)
        {
            if (execute)
            {
                setAllowInsertIdentityColumn(table.tableName, true);
            }

            try
            {
                return (doMoveData(table, execute));
            }
            finally
            {
                if (execute)
                {
                    setAllowInsertIdentityColumn(table.tableName, false);
                }
            }
        }

        private void setAllowInsertIdentityColumn(string tableName, bool allow)
        {
            var sql = "SET IDENTITY_INSERT {targetDbName}.{tableName} {allow}".Inject(new Dictionary<string, object>
            {
                { "targetDbName", targetDatabaseName },
                { "tableName", tableName },
                { "allow", allow ? "ON" : "OFF" },
            });
            var command = dbConnection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql;

            try
            {
                command.ExecuteNonQuery();
            }
            catch (DbException e)
            {
                logger.Warn("Could not set identity insert for table " + tableName + ": inserts may fail", e);
            }
            finally
            {
                try
                {
                    command.Dispose();
                }
                catch (Exception e)
                {
                    logger.Warn("Error disposing identity insert command for table " + tableName, e);
                }
            }

        }

        private bool doMoveData(MinistryPlatformTable table, bool execute)
        {
            var sqlCommand = createInsertNewSqlCommand(table);
            if (!execute)
            {
                logger.Debug("SQL Command: " + sqlCommand);
                return (true);
            }

            var command = dbConnection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sqlCommand;

            var tx = dbConnection.BeginTransaction(IsolationLevel.ReadUncommitted);
            try
            {
                int numRows = command.ExecuteNonQuery();
                tx.Commit();
                return (true);
            }
            catch (DbException e)
            {
                logger.Error("Error updating table " + table.tableName, e);
                tx.Rollback();
                throw (e);
            }
            finally
            {
                try
                {
                    command.Dispose();
                }
                catch (Exception e)
                {
                    logger.Warn("Error disposing command for table " + table.tableName, e);
                }
            }
        }

        private string createInsertNewSqlCommand(MinistryPlatformTable table)
        {
            string sql =
                  "INSERT INTO {targetDbName}.{tableName} "
                + "SELECT * FROM {sourceDbName}.{tableName} {filterClause} "
                + "EXCEPT SELECT * FROM {targetDbName}.{tableName} ";
            string filterClause = table.filterClause != null && table.filterClause.Length > 0 ? "WHERE " + table.filterClause : "";

            var parms = new Dictionary<string, string>{
                            { "tableName", table.tableName},
                            { "sourceDbName", sourceDatabaseName},
                            { "targetDbName", targetDatabaseName},
                            { "filterClause", filterClause},
                        };
            return (sql.Inject(parms));
        }
    }
}