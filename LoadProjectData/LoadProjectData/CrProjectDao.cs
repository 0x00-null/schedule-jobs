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

        public bool Exists(string projectName, int initiativeId)
        {
            var count = 0;
            const string query = "SELECT count(*) FROM dbo.cr_Projects WHERE project_name = @ProjectName AND Initiative_ID = @InitiativeId";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@ProjectName", SqlDbType.NVarChar, 100).Value = projectName;
                cmd.Parameters.Add("@InitiativeId", SqlDbType.Int).Value = initiativeId;

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

        public int Insert(CrProject project, bool update = false)
        {
            if (Exists(project.ProjectName, project.InitiativeId))
            {
                if (update)
                {
                    Update(project);
                }
                return GetProjectId(project.ProjectName);
            }

            var addrDao = new CrAddressDao();
            var addr = new CrAddress
            {
                AddressLine1 = project.Address1,
                City = project.City,
                PostalCode = project.Zip,
                State = project.State
            };

            var addressID = addrDao.Insert(addr);
            

            var rc = -1;   
            const string query = "INSERT INTO dbo.cr_Projects (Project_Name,Project_Status_ID,Location_ID,Project_Type_ID," +
                                                              "Organization_ID,Initiative_ID,Minimum_Volunteers,Maximum_Volunteers," +
                                                              "Absolute_Maximum_Volunteers,Domain_ID," +
                                                              "Check_In_Floor,Check_In_Area,Check_In_Room_Number,Note_To_Volunteers_1," +
                                                              "Note_To_Volunteers_2,Project_Parking_Location,Address_ID) " +
                                 "OUTPUT INSERTED.Project_ID " +
                                 "VALUES (@ProjectName,@ProjectStatusID,@LocationID,@ProjectTypeID," +
                                         "@OrganizationID,@InitiativeID,@MinimumVolunteers,@MaximumVolunteers," +
                                         "@AbsoluteMaximumVolunteers,@DomainID," +
                                         "@CheckInFloor,@CheckInArea,@CheckInRoomNumber,@NoteToVolunteers1," +
                                         "@NoteToVolunteers2,@ProjectParkingLocation,@Address_ID) ";

            var mp = new MpDao();

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {
                
                cmd.Parameters.Add("@ProjectName", SqlDbType.NVarChar, 100).Value = project.ProjectName;
                cmd.Parameters.Add("@ProjectStatusID", SqlDbType.Int).Value = 1;
                cmd.Parameters.Add("@LocationID", SqlDbType.Int).Value = mp.GetLocationId(project.LocationName);
                cmd.Parameters.Add("@ProjectTypeID", SqlDbType.Int).Value = mp.GetProjectTypeId(project.ProjectTypeName);
                cmd.Parameters.Add("@OrganizationID", SqlDbType.Int).Value = mp.GetOrganizationId(project.OrganizationName);
                cmd.Parameters.Add("@InitiativeID", SqlDbType.Int).Value = project.InitiativeId;
                cmd.Parameters.Add("@MinimumVolunteers", SqlDbType.Int).Value = project.MinVol;
                cmd.Parameters.Add("@MaximumVolunteers", SqlDbType.Int).Value = project.MaxVol;
                cmd.Parameters.Add("@AbsoluteMaximumVolunteers", SqlDbType.Int).Value = project.AbsoluteMaxVol;
                cmd.Parameters.Add("@DomainID", SqlDbType.Int).Value = 1;

                cmd.Parameters.Add("@CheckInFloor", SqlDbType.NVarChar, 50).Value = project.CheckInFloor;
                cmd.Parameters.Add("@CheckInArea", SqlDbType.NVarChar, 50).Value = project.CheckInArea;
                cmd.Parameters.Add("@CheckInRoomNumber", SqlDbType.NVarChar, 50).Value = project.CheckInRoomNumber;
                cmd.Parameters.Add("@NoteToVolunteers1", SqlDbType.NVarChar, 500).Value = project.Note1;
                cmd.Parameters.Add("@NoteToVolunteers2", SqlDbType.NVarChar, 500).Value = project.Note2;
                cmd.Parameters.Add("@ProjectParkingLocation", SqlDbType.NVarChar, 500).Value = project.ParkingLocation;
                cmd.Parameters.Add("@Address_ID", SqlDbType.Int).Value = addressID;

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
            var addrId = GetProjectAddressId(project.ProjectName);
            if (addrId < 0)
            {
                var addrDao = new CrAddressDao();
                var addr = new CrAddress
                {
                    AddressLine1 = project.Address1,
                    City = project.City,
                    State = project.State,
                    PostalCode = project.Zip
                };
                addrId = addrDao.Insert(addr);
            }

            var rc = -1;
            const string query = "UPDATE dbo.cr_Projects SET " +
                                 "Check_In_Floor = @CheckInFloor, " +
                                 "Check_In_Area = @CheckInArea, " +
                                 "Check_In_Room_Number = @CheckInRoomNumber, " +
                                 "Note_To_Volunteers_1 = @NoteToVolunteers1, " +
                                 "Note_To_Volunteers_2 = @NoteToVolunteers2, " +
                                 "Project_Parking_Location = @ProjectParkingLocation, " +
                                 "WHERE Project_Name = @ProjectName";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@ProjectName", SqlDbType.NVarChar, 100).Value = project.ProjectName;
                cmd.Parameters.Add("@CheckInFloor", SqlDbType.NVarChar, 50).Value = project.CheckInFloor;
                cmd.Parameters.Add("@CheckInArea", SqlDbType.NVarChar, 50).Value = project.CheckInArea;
                cmd.Parameters.Add("@CheckInRoomNumber", SqlDbType.NVarChar, 50).Value = project.CheckInRoomNumber;
                cmd.Parameters.Add("@NoteToVolunteers1", SqlDbType.NVarChar, 500).Value = project.Note1;
                cmd.Parameters.Add("@NoteToVolunteers2", SqlDbType.NVarChar, 500).Value = project.Note2;
                cmd.Parameters.Add("@ProjectParkingLocation", SqlDbType.NVarChar, 500).Value = project.ParkingLocation;
                cmd.Parameters.Add("@AddressID", SqlDbType.Int).Value = addrId;
                
                // open connection, execute UPDATE, close connection
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

        public int GetProjectAddressId(string projectName)
        {
            var rc = -1;

            if (!ProjectHasAddressId(projectName))
            {
                return -1;
            }

            const string query = "SELECT address_ID FROM dbo.cr_Projects WHERE project_name = @ProjectName ";

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

        private bool ProjectHasAddressId(string projectName)
        {
            var rc = -1;


            const string query =
                "SELECT count(*) FROM dbo.cr_Projects WHERE project_name = @ProjectName AND address_id is NOT NULL ";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@ProjectName", SqlDbType.NVarChar, 100).Value = projectName;

                // open connection, execute INSERT, close connection
                cn.Open();
                try
                {
                    rc = (int) cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    // write to log
                    log.Error(ex.Message);
                }

                cn.Close();
            }
            return rc > 0;
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
