using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace LoadProjectData
{
    public class CrProjectDao
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MinistryPlatform"].ConnectionString;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool Exists(string projectName)
        {
            var rc = false;
            var count = 0;
            const string query = "SELECT count(*) FROM dbo.cr_Projects WHERE project_name = @ProjectName";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@ProjectName", SqlDbType.NVarChar, 100).Value = projectName;

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

        public int Insert(CrProject project)
        {
            if (Exists(project.ProjectName))
            {
                Update(project);
                return GetProjectId(project.ProjectName);
            }

            var rc = -1;   
            const string query = "INSERT INTO dbo.cr_Projects (Project_Name,Project_Status_ID,Location_ID,Project_Type_ID,Organization_ID,Initiative_ID,Minimum_Volunteers,Maximum_Volunteers,Absolute_Maximum_Volunteers,Domain_ID) " +
                                 "OUTPUT INSERTED.Project_ID " +
                                 "VALUES (@ProjectName,@ProjectStatusID,@LocationID,@ProjectTypeID,@OrganizationID,@InitiativeID,@MinimumVolunteers,@MaximumVolunteers,@AbsoluteMaximumVolunteers,@DomainID) ";

            var mp = new MpDao();

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {
                
                cmd.Parameters.Add("@ProjectName", SqlDbType.NVarChar, 100).Value = project.ProjectName;
                cmd.Parameters.Add("@ProjectStatusID", SqlDbType.Int).Value = 1;
                cmd.Parameters.Add("@LocationID", SqlDbType.Int).Value = mp.GetLocationId(project.LocationName);
                cmd.Parameters.Add("@ProjectTypeID", SqlDbType.Int).Value = mp.GetProjectTypeId(project.ProjectTypeName);
                cmd.Parameters.Add("@OrganizationID", SqlDbType.Int).Value = mp.GetOrganizationId(project.OrganizationName);
                cmd.Parameters.Add("@InitiativeID", SqlDbType.Int).Value = 1;
                cmd.Parameters.Add("@MinimumVolunteers", SqlDbType.Int).Value = project.MinVol;
                cmd.Parameters.Add("@MaximumVolunteers", SqlDbType.Int).Value = project.MaxVol;
                cmd.Parameters.Add("@AbsoluteMaximumVolunteers", SqlDbType.Int).Value = project.AbsoluteMaxVol;
                cmd.Parameters.Add("@DomainID", SqlDbType.Int).Value = 1;

                // open connection, execute INSERT, close connection
                cn.Open();
                try
                {
                    rc = (int) cmd.ExecuteScalar();
                }
                catch(Exception ex)
                {
                    // write to log
                    log.Error(ex.Message);
                }
                
                cn.Close();
            }
            return rc;
        }

        public int Update(CrProject project)
        {
            var rc = -1;
            const string query = "UPDATE dbo.cr_Projects SET Project_Status_ID = @ProjectStatusID ,Location_ID = @LocationID ,Project_Type_ID = @ProjectTypeID," +
                                 "Organization_ID = @OrganizationID,Initiative_ID=@InitiativeID,Minimum_Volunteers = @MinimumVolunteers," +
                                 "Maximum_Volunteers = @MaximumVolunteers,Absolute_Maximum_Volunteers = @AbsoluteMaximumVolunteers,Domain_ID = @DomainID " +
                                 "WHERE Project_Name = @ProjectName";

            var mp = new MpDao();

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@ProjectName", SqlDbType.NVarChar, 100).Value = project.ProjectName;
                cmd.Parameters.Add("@ProjectStatusID", SqlDbType.Int).Value = 1;
                cmd.Parameters.Add("@LocationID", SqlDbType.Int).Value = mp.GetLocationId(project.LocationName);
                cmd.Parameters.Add("@ProjectTypeID", SqlDbType.Int).Value = mp.GetProjectTypeId(project.ProjectTypeName);
                cmd.Parameters.Add("@OrganizationID", SqlDbType.Int).Value = mp.GetOrganizationId(project.OrganizationName);
                cmd.Parameters.Add("@InitiativeID", SqlDbType.Int).Value = 1;
                cmd.Parameters.Add("@MinimumVolunteers", SqlDbType.Int).Value = project.MinVol;
                cmd.Parameters.Add("@MaximumVolunteers", SqlDbType.Int).Value = project.MaxVol;
                cmd.Parameters.Add("@AbsoluteMaximumVolunteers", SqlDbType.Int).Value = project.AbsoluteMaxVol;
                cmd.Parameters.Add("@DomainID", SqlDbType.Int).Value = 1;

                // open connection, execute INSERT, close connection
                cn.Open();
                try
                {
                    rc = cmd.ExecuteNonQuery();
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

        public int GetProjectId(string projectName)
        {
            var rc = -1;
           
            const string query = "SELECT project_ID FROM dbo.cr_Projects WHERE project_name = @ProjectName";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@ProjectName", SqlDbType.NVarChar, 100).Value = projectName;

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
