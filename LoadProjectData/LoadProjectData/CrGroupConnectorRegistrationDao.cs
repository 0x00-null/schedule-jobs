using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace LoadProjectData
{
    public class CrGroupConnectorRegistrationDao
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MinistryPlatform"].ConnectionString;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool Exists(int groupconnectorid, int registrationid)
        {
            var rc = false;
            var count = 0;
            const string query = "SELECT count(*) FROM dbo.cr_Group_Connector_Registrations WHERE group_connector_id = @GroupConnectorId and registration_id = @RegistrationId";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@GroupConnectorId", SqlDbType.Int).Value = groupconnectorid;
                cmd.Parameters.Add("@RegistrationId", SqlDbType.Int).Value = registrationid;

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
            rc = count > 0 ? true : false;
            return rc;
        }

        public int Insert(CrGroupConnectorRegistration groupconnectorregistration)
        {
            if (Exists(groupconnectorregistration.GroupConnectorId, groupconnectorregistration.RegistrationId))
            {
                return GetGroupConnectorRegistrationId(groupconnectorregistration.GroupConnectorId, groupconnectorregistration.RegistrationId);
            }

            var rc = -1;
            const string query = "INSERT INTO dbo.cr_Group_Connector_Registrations (Group_Connector_ID,Registration_ID,Domain_ID,Role_ID) " +
                                 "OUTPUT INSERTED.Group_Connector_Registration_ID " +
                                 "VALUES (@GroupConnectorID,@RegistrationID,@DomainID,@RoleID) ";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@GroupConnectorID", SqlDbType.Int).Value = groupconnectorregistration.GroupConnectorId;
                cmd.Parameters.Add("@RegistrationID", SqlDbType.Int).Value = groupconnectorregistration.RegistrationId;
                cmd.Parameters.Add("@DomainID", SqlDbType.Int).Value = groupconnectorregistration.DomainId;
                cmd.Parameters.Add("@RoleID", SqlDbType.Int).Value = groupconnectorregistration.RoleId;

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

        public int GetGroupConnectorRegistrationId(int groupconnectorid, int registrationid)
        {
            var rc = -1;

            const string query = "SELECT group_connector_registration_id FROM dbo.cr_Group_Connector_Registrations WHERE group_connector_id = @GroupConnectorId and registration_id = @RegistrationId";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@GroupConnectorId", SqlDbType.NVarChar, 100).Value = groupconnectorid;
                cmd.Parameters.Add("@RegistrationId", SqlDbType.Int).Value = registrationid;

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
