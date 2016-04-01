using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace LoadProjectData
{
    public class CrRegistrationDao
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MinistryPlatform"].ConnectionString;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public bool Exists(int participantid)
        {
            var rc = false;
            var count = 0;
            const string query = "SELECT count(*) FROM dbo.cr_Registrations WHERE participant_id = @ParticipantId";

            var mp = new MpDao();

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@ParticipantId", SqlDbType.NVarChar, 100).Value = participantid;

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

        public int Insert(CrRegistration registration)
        {
            if (Exists(registration.ParticipantId))
            {
                return (GetRegistrationId(registration.ParticipantId));
            }
            var rc = -1;
            const string query = "INSERT INTO dbo.cr_Registrations (Organization_ID,Preferred_Launch_Site_ID,Participant_ID,Initiative_ID,Spouse_Participation,Domain_ID, Additional_Information,Registration_Creation_Date) " +
                                 "OUTPUT INSERTED.Registration_ID " +
                                 "VALUES (@OrganizationID,@PreferredLaunchSiteID,@ParticipantID,@InitiativeID,@SpouseParticipation,@DomainID, @AddlInfo, @CreationDate) ";

            var mp = new MpDao();

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@OrganizationID", SqlDbType.Int).Value = mp.GetOrganizationId(registration.OrganizationName);
                cmd.Parameters.Add("@PreferredLaunchSiteID", SqlDbType.Int).Value = mp.GetLocationId(registration.LocationName);
                cmd.Parameters.Add("@ParticipantID", SqlDbType.Int).Value = registration.ParticipantId;
                cmd.Parameters.Add("@InitiativeID", SqlDbType.Int).Value = registration.InitiativeId;
                cmd.Parameters.Add("@SpouseParticipation", SqlDbType.Bit).Value = registration.SpouseParticipation;
                cmd.Parameters.Add("@DomainID", SqlDbType.Int).Value = registration.DomainId;
                cmd.Parameters.Add("@AddlInfo", SqlDbType.NVarChar).Value = registration.AddlInfo;
                cmd.Parameters.Add("@CreationDate", SqlDbType.SmallDateTime).Value = registration.CreationDate;
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

        public int GetRegistrationId(int participantid)
        {
            var rc = -1;

            const string query = "SELECT registration_ID FROM dbo.cr_Registrations WHERE participant_id = @ParticipantID";

            var mp = new MpDao();

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@ParticipantID", SqlDbType.NVarChar, 100).Value = participantid;

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
