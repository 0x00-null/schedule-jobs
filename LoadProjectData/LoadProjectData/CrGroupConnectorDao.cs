using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace LoadProjectData
{
    public class CrGroupConnectorDao
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MinistryPlatform"].ConnectionString;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool Exists(int primaryreg, int projectid)
        {
            var count = 0;
            const string query = "SELECT count(*) FROM dbo.cr_Group_Connectors WHERE Primary_Registration = @PrimReg and project_id = @ProjectId";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@PrimReg", SqlDbType.Int).Value = primaryreg;
                cmd.Parameters.Add("@ProjectId", SqlDbType.Int).Value = projectid;

                // open connection, execute INSERT, close connection
                cn.Open();
                try
                {
                    count = (int)cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    // write to log
                    log.Error(ex.Message);
                }

                cn.Close();
            }
            var rc = count > 0;
            return rc;
        }

        public int Insert(CrGroupConnector groupconnector)
        {
            if (Exists(groupconnector.PrimaryRegistration, groupconnector.ProjectId))
            {
                return GetGroupConnectorId(groupconnector.PrimaryRegistration,groupconnector.ProjectId);
            }

            var rc = -1;
            const string query = "INSERT INTO dbo.cr_Group_Connectors (Project_ID,Primary_Registration,Domain_ID) " +
                                 "OUTPUT INSERTED.Group_Connector_ID " +
                                 "VALUES (@ProjectID,@PrimaryRegistration,@DomainID) ";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.Add("@ProjectID", SqlDbType.Int).Value = groupconnector.ProjectId;
                cmd.Parameters.Add("@PrimaryRegistration", SqlDbType.Int).Value = groupconnector.PrimaryRegistration;
                cmd.Parameters.Add("@DomainID", SqlDbType.Int).Value = groupconnector.DomainId;

                // open connection, execute INSERT, close connection
                cn.Open();
                try
                {
                    rc = (int)cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    // write to log
                    log.Error(ex.Message);
                }

                cn.Close();
            }
            return rc;
        }

        public int GetGroupConnectorId(int primaryreg, int projectid)
        {
            var rc = -1;

            const string query = "SELECT group_connector_id FROM dbo.cr_Group_Connectors WHERE Primary_Registration = @PrimReg and project_id = @ProjectId";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@PrimReg", SqlDbType.Int).Value = primaryreg;
                cmd.Parameters.Add("@ProjectId", SqlDbType.Int).Value = projectid;

                // open connection, execute INSERT, close connection
                cn.Open();
                try
                {
                    rc = (int)cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    // write to log
                    log.Error(ex.Message);
                }

                cn.Close();
            }
            return rc;
        }
    }
}
