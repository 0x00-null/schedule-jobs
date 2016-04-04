using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace LoadProjectData
{
    public class MpDao
    {
        readonly string _connectionString = ConfigurationManager.ConnectionStrings["MinistryPlatform"].ConnectionString;
     
        public int GetProjectTypeId(string projectType)
        {
            var projectTypeId = -1;
            const string query = "select project_type_id from cr_project_types where description = @Description";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 100).Value = projectType;
                                
                cn.Open();
                try{
                    projectTypeId = (int)cmd.ExecuteScalar();
                }
                catch(Exception ex)
                {
                    // write to log
                }
                
                cn.Close();
            }

            return projectTypeId;
        }

        public int GetLocationId(string locationName)
        {
            var locationId = -1;
            const string query = "select location_id from locations where location_name = @Location_Name";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@Location_Name", SqlDbType.NVarChar, 100).Value = locationName;

                cn.Open();
                try
                {
                    locationId = (Int32)cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    // write to log
                }

                cn.Close();
            }

            return locationId;
        }

        public int GetOrganizationId(string orgName)
        {
            var orgId = -1;
            const string query = "select organization_id from cr_organizations where name = @Org_Name";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@Org_Name", SqlDbType.NVarChar, 100).Value = orgName;

                cn.Open();
                try
                {
                    orgId = (int)cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    // write to log
                }

                cn.Close();
            }

            return orgId;
        }

        public int GetParticipantId(string email)
        {
            int orgId;
            const string query = "select top 1 p.participant_id from participants p join contacts c on c.contact_id = p.contact_id where c.email_address = @Email_Address";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@Email_Address", SqlDbType.NVarChar, 100).Value = email.Trim();

                cn.Open();
                try
                {
                    orgId = (int)cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    // write to log
                    throw;
                }

                cn.Close();
            }

            return orgId;
        }
        
    }
}

        