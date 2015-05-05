using log4net;
using MigrateMPData.Interfaces;
using MigrateMPData.Models;
using MigrateMPData.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace MigrateMPData
{
    public class MinistryPlatformDataMover : IMinistryPlatformDataMover
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string sourceDatabaseName;

        private string targetDatabaseName;

        private IDbConnection sourceDbConnection;

        private IDbConnection targetDbConnection;

        public MinistryPlatformDataMover(IDbConnection sourceDbConnection, IDbConnection targetDbConnection, string sourceDatabaseName, string targetDatabaseName)
        {
            this.sourceDbConnection = sourceDbConnection;
            this.targetDbConnection = targetDbConnection;
            this.sourceDatabaseName = sourceDatabaseName;
            this.targetDatabaseName = targetDatabaseName;
        }
        
        public bool moveData(MinistryPlatformTable table, bool execute)
        {
            if (execute)
            {
                sourceDbConnection.Open();
                targetDbConnection.Open();
            }

            try
            {
                logger.Info("BEGIN: Moving data for table " + table.tableName);
                var response = doMoveData(table, execute);
                logger.Info("DONE:  Moving data for table " + table.tableName);

                return (response);
            }
            finally
            {
                if (execute)
                {
                    try
                    {
                        sourceDbConnection.Close();
                        targetDbConnection.Close();
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
            var command = targetDbConnection.CreateCommand();
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
                logger.Info("Running in test mode, not moving data for table " + table.tableName);
                return (true);
            }

            var selectCommand = sourceDbConnection.CreateCommand();
            selectCommand.CommandType = CommandType.Text;
            selectCommand.CommandText = createSelectSqlCommand(table);

            var insertCommand = targetDbConnection.CreateCommand();
            insertCommand.CommandType = CommandType.Text;
            insertCommand.CommandText = createInsertSqlCommand(table);
            var tx = insertCommand.Transaction = targetDbConnection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);

            IDataReader reader = null;
            try
            {
                setAllowInsertIdentityColumn(table.tableName, tx, true);

                reader = selectCommand.ExecuteReader();

                logger.Debug("Insert SQL: " + insertCommand.CommandText);

                bool first = true;
                int count = 0;
                while (reader.Read())
                {
                    if (first)
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var p = insertCommand.CreateParameter();
                            p.ParameterName = "@" + reader.GetName(i);
                            p.DbType = Parameter.ConvertTypeCodeToDbType(Type.GetTypeCode(reader.GetFieldType(i)));
                            p.Size = reader.GetSchemaTable().Rows[i].Field<int>("ColumnSize");
                            p.Precision = (byte)reader.GetSchemaTable().Rows[i].Field<short>("NumericPrecision");
                            p.Scale = (byte)reader.GetSchemaTable().Rows[i].Field<short>("NumericScale");

                            insertCommand.Parameters.Add(p);
                            logger.Debug("Parameter: " + p.ParameterName + " DbType: " + p.DbType);
                        }
                        insertCommand.Prepare();
                        first = false;
                    }

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        ((IDbDataParameter)insertCommand.Parameters[i]).Value = reader.GetValue(i);
                    }

                    insertCommand.ExecuteNonQuery();
                    if (++count % 10 == 0)
                    {
                        logger.Info("Migrated " + count + " rows for table " + table.tableName);
                    }
                }
                logger.Info("Migrated " + count + " rows for table " + table.tableName);

                setAllowInsertIdentityColumn(table.tableName, tx, false);
                tx.Commit();

                return (true);
            }
            catch (DbException e)
            {
                logger.Error("Error updating table " + table.tableName, e);
                setAllowInsertIdentityColumn(table.tableName, tx, false);
                tx.Rollback();
                return (false);
            }
            finally
            {
                try
                {
                    selectCommand.Dispose();
                    insertCommand.Dispose();
                }
                catch (Exception e)
                {
                    logger.Warn("Error disposing command for table " + table.tableName, e);
                }
            }

        }

        private string createSelectSqlCommand(MinistryPlatformTable table)
        {
            var sql = "SELECT {columns} FROM {sourceDbName}.{tableName} {filterClause} EXCEPT SELECT * FROM {targetDbName}.{tableName}";
            string filterClause = table.filterClause != null && table.filterClause.Length > 0 ? "WHERE " + table.filterClause : "";
            var columns = getColumnsForTable(table.tableName);
            var parms = new Dictionary<string, string>{
                            { "columns", String.Join(", ", columns) },
                            { "sourceDbName", sourceDatabaseName},
                            { "targetDbName", targetDatabaseName},
                            { "tableName", table.tableName},
                            { "filterClause", filterClause},
                        };
            return (sql.Inject(parms));
        }

        private string createInsertSqlCommand(MinistryPlatformTable table)
        {
            var sql = "INSERT INTO {targetDbName}.{tableName} ({columns}) VALUES ({placeholders}) ";

            var columns = getColumnsForTable(table.tableName);
            StringBuilder placeholders = new StringBuilder();
            Regex columnName = new Regex(@".*\[(.*)\]");
            for (int i = 0; i < columns.Count; i++)
            {
                if (i > 0)
                {
                    placeholders.Append(", ");
                }
                placeholders.Append("@").Append(columnName.Replace(columns[i], "$1"));
            }

            var parms = new Dictionary<string, string>{
                            { "columns", String.Join(", ", columns) },
                            { "targetDbName", targetDatabaseName},
                            { "tableName", table.tableName},
                            { "placeholders", placeholders.ToString() },
                        };
            return (sql.Inject(parms));
        }

        private List<string> getColumnsForTable(string tableName)
        {
            Regex regex = new Regex(@".*\.\[(.*)\]");
            var table = regex.Replace(tableName, "$1");
            string sql = "SELECT CONCAT('[', [Column_Name], ']') FROM {sourceDbName}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' ORDER BY ordinal_position".Inject(new Dictionary<string, object>
            {
                { "sourceDbName", sourceDatabaseName},
                { "tableName", table},
            });

            var command = sourceDbConnection.CreateCommand();
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

            return(columns);
        }

    }
}