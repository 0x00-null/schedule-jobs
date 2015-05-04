using log4net;
using MigrateMPData.Interfaces;
using MigrateMPData.Models;
using MigrateMPData.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text.RegularExpressions;

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
                dbConnection.Open();
            }

            try
            {
                return (doMoveData(table, execute));
            }
            finally
            {
                if (execute)
                {
                    try
                    {
                        dbConnection.Close();
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Error closing DB Connection for table " + table.tableName, e);
                    }
                }
            }
        }

        private void setAllowInsertIdentityColumn(string tableName, IDbTransaction tx, bool allow)
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
            command.Transaction = tx;

            try
            {
                logger.Info("Setting IDENTITY_INSERT " + allow + " for table " + tableName);
                logger.Debug("Sql: " + sql);
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
            if (!execute)
            {
                logger.Debug("Not moving data");
                return (true);
            }

            var sqlCommand = createInsertNewSqlCommand(table);
            logger.Debug("SQL Command: " + sqlCommand);

            var command = dbConnection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sqlCommand;

            var tx = dbConnection.BeginTransaction(IsolationLevel.ReadUncommitted);
            command.Transaction = tx;
            try
            {
                setAllowInsertIdentityColumn(table.tableName, tx, true);

                int numRows = command.ExecuteNonQuery();

                setAllowInsertIdentityColumn(table.tableName, tx, false);

                tx.Commit();

                return (true);
            }
            catch (DbException e)
            {
                logger.Error("Error updating table " + table.tableName, e);
                setAllowInsertIdentityColumn(table.tableName, tx, false);
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
                  "INSERT INTO {targetDbName}.{tableName} ({columns}) "
                + "SELECT * FROM {sourceDbName}.{tableName} {filterClause} "
                + "EXCEPT SELECT * FROM {targetDbName}.{tableName}";
            string filterClause = table.filterClause != null && table.filterClause.Length > 0 ? "WHERE " + table.filterClause : "";

            var parms = new Dictionary<string, string>{
                            { "columns", getColumnNamesForTable(table.tableName) },
                            { "tableName", table.tableName},
                            { "sourceDbName", sourceDatabaseName},
                            { "targetDbName", targetDatabaseName},
                            { "filterClause", filterClause},
                        };
            return (sql.Inject(parms));
        }

        private string getColumnNamesForTable(string tableName)
        {
            Regex regex = new Regex(@".*\.\[(.*)\]$");
            var table = regex.Replace(tableName, "$1");
            string sql = "SELECT CONCAT('[', [Column_Name], ']') FROM {sourceDbName}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' ORDER BY ordinal_position".Inject(new Dictionary<string, object>
            {
                { "sourceDbName", sourceDatabaseName},
                { "tableName", table},
            });

            var command = dbConnection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql;

            List<string> columns = new List<string>();
            IDataReader reader = null;
            try {
                logger.Info("Getting column list for table " + tableName);
                logger.Debug("Sql: " + sql);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    columns.Add(reader.GetString(0));
                }
            }
            catch (DbException e)
            {
                logger.Error("Error getting column list from database", e);
                throw (e);
            }
            finally
            {
                try
                {
                    if (reader != null) {
                        reader.Close();
                        reader.Dispose();
                    }
                    command.Dispose();
                }
                catch (Exception e)
                {
                    logger.Warn("Error disposing command for table " + tableName, e);
                }
            }

            var columnNames = String.Join(", ", columns.ToArray());
            logger.Debug("Column names: " + columnNames);

            return(columnNames);

        }
    }
}