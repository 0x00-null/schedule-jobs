using log4net;
using MigrateMPData.Interfaces;
using MigrateMPData.Models;
using MigrateMPData.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
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

        public MinistryPlatformDataMover(IDbConnection sourceDbConnection, IDbConnection targetDbConnection, string sourceDbName, string targetDbName)
        {
            this.sourceDbConnection = sourceDbConnection;
            this.targetDbConnection = targetDbConnection;
            this.sourceDatabaseName = sourceDbName;
            this.targetDatabaseName = targetDbName;
        }
        
        public bool moveData(MinistryPlatformTable table, bool execute)
        {
            var connections = new[] { sourceDbConnection, targetDbConnection };
            if (execute)
            {
                foreach (var conn in connections)
                {
                    conn.Open();
                }
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
                    foreach (var conn in connections)
                    {
                        try
                        {
                            conn.Close();
                        }
                        catch (Exception e)
                        {
                            logger.Warn("Error closing DB Connection for table " + table.tableName, e);
                        }
                    }
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

            var selectSql = createSelectSqlCommandString(table);
            logger.Debug("Select SQL: " + selectSql);
            var selectCommand = sourceDbConnection.CreateCommand();
            selectCommand.CommandType = CommandType.Text;
            selectCommand.CommandText = selectSql;

            var tx = targetDbConnection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);

            var insertCommand = targetDbConnection.CreateCommand();
            insertCommand.CommandType = CommandType.Text;
            insertCommand.CommandText = createInsertSqlCommandString(table);
            insertCommand.Transaction = tx;

            var updateCommand = createUpdateSqlCommand(table, tx);

            IDataReader reader = null;
            try
            {
                setAllowInsertIdentityColumn(table.tableName, tx, true);

                reader = selectCommand.ExecuteReader();

                logger.Debug("Insert SQL: " + insertCommand.CommandText);

                bool firstInsert = true;
                bool firstUpdate = true;
                var counts = new Counters();
                while (reader.Read())
                {
                    if (firstInsert)
                    {
                        prepareCommand(insertCommand, reader);
                        firstInsert = false;
                    }

                    try
                    {
                        counts.inserts += bindAndExecuteCommand(insertCommand, reader); ;
                    }
                    catch (SqlException e)
                    {
                        if (e.Number == 2601 || e.Number == 2627)
                        {
                            if(table.migrationType != MigrationType.INSERT_OR_UPDATE) {
                                logger.Warn("Row already exists in table " + table.tableName + ", but migration mode is INSERT_ONLY, skipping: " + e.Message);
                                counts.skips++;
                            }
                            else
                            {
                                logger.Warn("Row already exists in table " + table.tableName + ", and migration mode is INSERT_OR_UPDATE, updating: " + e.Message);
                                
                                if (firstUpdate)
                                {
                                    prepareCommand(updateCommand, reader);
                                    firstUpdate = false;
                                }

                                counts.updates += bindAndExecuteCommand(updateCommand, reader);
                            }

                        }
                        else
                        {
                            throw (e);
                        }
                    }
                    if (counts.totals % 10 == 0)
                    {
                        logger.Info("Inserted " + counts.inserts + " rows for table " + table.tableName);
                        logger.Info("Updated  " + counts.updates + " rows for table " + table.tableName);
                        logger.Info("Skipped  " + counts.skips + " rows for table " + table.tableName);
                        logger.Info("Total    " + counts.totals + " rows for table " + table.tableName);
                    }
                }
                logger.Info("Inserted " + counts.inserts + " rows for table " + table.tableName);
                logger.Info("Updated  " + counts.updates + " rows for table " + table.tableName);
                logger.Info("Skipped  " + counts.skips + " rows for table " + table.tableName);
                logger.Info("Total    " + counts.totals + " rows for table " + table.tableName);

                setAllowInsertIdentityColumn(table.tableName, tx, false);
                tx.Commit();

                return (true);
            }
            catch (Exception e)
            {
                logger.Error("Error updating table " + table.tableName + ", updates will be rolled back", e);
                setAllowInsertIdentityColumn(table.tableName, tx, false);
                tx.Rollback();
                return (false);
            }
            finally
            {
                var commands = new [] { selectCommand, insertCommand, updateCommand };
                foreach (var cmd in commands)
                {
                    try
                    {
                        cmd.Dispose();
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Error disposing command for table " + table.tableName, e);
                    }
                }
            }

        }

        private int bindAndExecuteCommand(IDbCommand dbCommand, IDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                ((IDbDataParameter)dbCommand.Parameters[i]).Value = reader.GetValue(i);
            }

            return(dbCommand.ExecuteNonQuery());
        }

        private void prepareCommand(IDbCommand dbCommand, IDataReader reader)
        {
            var schemaTable = reader.GetSchemaTable();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var p = dbCommand.CreateParameter();
                p.ParameterName = "@" + reader.GetName(i);
                p.DbType = Parameter.ConvertTypeCodeToDbType(Type.GetTypeCode(reader.GetFieldType(i)));
                p.Size = schemaTable.Rows[i].Field<int>("ColumnSize");
                p.Precision = (byte)schemaTable.Rows[i].Field<short>("NumericPrecision");
                p.Scale = (byte)schemaTable.Rows[i].Field<short>("NumericScale");

                dbCommand.Parameters.Add(p);
                logger.Debug("Parameter: " + p.ParameterName + " DbType: " + p.DbType);
            }
            dbCommand.Prepare();
        }

        private string createSelectSqlCommandString(MinistryPlatformTable table)
        {
            var sql = "SELECT {columns} FROM {sourceDbName}.{tableName} AS S {filterClause} EXCEPT SELECT * FROM {targetDbName}.{tableName} ";
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

        private string createInsertSqlCommandString(MinistryPlatformTable table)
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
                            { "targetDbName", targetDbConnection.Database },
                            { "tableName", table.tableName},
                            { "placeholders", placeholders.ToString() },
                        };
            return (sql.Inject(parms));
        }

        private IDbCommand createUpdateSqlCommand(MinistryPlatformTable table, IDbTransaction tx)
        {
            // Determine the primary key column(s)
            var pkSql = "SELECT column_name FROM {targetDbName}.INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE OBJECTPROPERTY(OBJECT_ID(constraint_name), 'IsPrimaryKey') = 1 AND table_name = '{tableName}'".Inject(new Dictionary<string, string>
            {
                { "targetDbName", targetDbConnection.Database },
                { "tableName", getTableName(table.tableName) },
            });
            var pkCommand = targetDbConnection.CreateCommand();
            pkCommand.CommandType = CommandType.Text;
            pkCommand.CommandText = pkSql;
            pkCommand.Transaction = tx;

            var pkColumns = new List<string>();
            var pkReader = pkCommand.ExecuteReader();
            while (pkReader.Read())
            {
                pkColumns.Add(pkReader.GetString(0));
            }
            pkReader.Close();
            pkReader.Dispose();
            pkCommand.Dispose();

            var columns = getColumnsForTable(table.tableName);
            StringBuilder sets = new StringBuilder();
            StringBuilder where = new StringBuilder();
            Regex columnName = new Regex(@".*\[(.*)\]");
            bool firstSet = true;
            bool firstWhere = true;
            for (int i = 0; i < columns.Count; i++)
            {
                var col = columnName.Replace(columns[i], "$1");
                if (pkColumns.Contains(col))
                {
                    if (!firstWhere)
                    {
                        where.Append("AND ");
                    }
                    firstWhere = false;
                    where.Append(col).Append(" = @").Append(col);
                }
                else
                {
                    if (!firstSet)
                    {
                        sets.Append(", ");
                    }
                    firstSet = false;
                    sets.Append(col).Append(" = @").Append(col);
                }
            }

            if (where.Length <= 0)
            {
                throw (new InvalidOperationException("No primary key column was defined for table " + table.tableName));
            }

            var sql = "UPDATE {targetDbName}.{tableName} SET {sets} WHERE {where}".Inject(new Dictionary<string, string>{
                            { "targetDbName", targetDbConnection.Database },
                            { "tableName", table.tableName},
                            { "sets", sets.ToString() },
                            { "where", where.ToString() }
                        });


            var updateCommand = targetDbConnection.CreateCommand();
            updateCommand.CommandType = CommandType.Text;
            updateCommand.CommandText = sql;
            updateCommand.Transaction = tx;

            return (updateCommand);
        }

        private List<string> getColumnsForTable(string tableName)
        {
            string sql = "SELECT CONCAT('[', [Column_Name], ']') FROM {sourceDbName}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' ORDER BY ordinal_position".Inject(new Dictionary<string, object>
            {
                { "sourceDbName", sourceDatabaseName},
                { "tableName", getTableName(tableName) },
            });

            var command = sourceDbConnection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql;

            List<string> columns = new List<string>();
            IDataReader reader = null;
            try {
                logger.Debug("Getting column list for table " + tableName);
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

        private void setAllowInsertIdentityColumn(string tableName, IDbTransaction tx, bool allow)
        {
            if (!hasIdentityColumn(tableName, tx))
            {
                return;
            }
            var sql = "SET IDENTITY_INSERT {targetDbName}.{tableName} {allow}".Inject(new Dictionary<string, object>
            {
                { "targetDbName", targetDbConnection.Database },
                { "tableName", tableName },
                { "allow", allow ? "ON" : "OFF" },
            });
            var command = targetDbConnection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql;
            command.Transaction = tx;

            try
            {
                logger.Debug("Setting IDENTITY_INSERT " + allow + " for table " + tableName);
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

        private bool hasIdentityColumn(string tableName, IDbTransaction tx)
        {
            var sql = "SELECT COUNT(*) FROM {targetDbName}.sys.columns WHERE IS_IDENTITY = 1 AND OBJECT_NAME(object_id) = '{tableName}'".Inject(new Dictionary<string, object>
            {
                { "targetDbName", targetDbConnection.Database },
                { "tableName", getTableName(tableName) },
            });
            var command = targetDbConnection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql;
            command.Transaction = tx;

            try
            {
                logger.Debug("Checking IDENTITY column for table " + tableName);
                logger.Debug("Sql: " + sql);
                int result = (int)command.ExecuteScalar();
                return (result > 0);
            }
            catch (DbException e)
            {
                logger.Warn("Could not check IDENTITY column for table " + tableName, e);
                return (true);
            }
            finally
            {
                try
                {
                    command.Dispose();
                }
                catch (Exception e)
                {
                    logger.Warn("Error disposing identity insert check command for table " + tableName, e);
                }
            }
        }

        private string getTableName(string tableName)
        {
            Regex regex = new Regex(@".*\.\[(.*)\]");
            var table = regex.Replace(tableName, "$1");
            return (table);
        }
    }

    class Counters
    {
        public int updates { get; set; }
        public int inserts { get; set; }
        public int skips { get; set; }
        public int totals { get { return updates + inserts + skips; } }
    }
}