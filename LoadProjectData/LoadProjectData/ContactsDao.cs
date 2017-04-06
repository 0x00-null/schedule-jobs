using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace LoadProjectData
{
    public class ContactsDao
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MinistryPlatform"].ConnectionString;

        public string GetContactNameFromParticipantId(int participantid)
        {
            string rc;

            const string query = "SELECT c.nickname, c.last_name from contacts c join participants p on p.contact_id = c.contact_id " +
                                 "where p.participant_id = @Participant_Id";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.Add("@Participant_Id", SqlDbType.NVarChar, 100).Value = participantid;
                cn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        rc = reader.GetString(reader.GetOrdinal("nickname")) + " " +
                             reader.GetString(reader.GetOrdinal("last_name"));
                    }
                    else
                    {
                        rc = "Unknown";
                    }
                }
                cn.Close();
            }
            return rc;
        }
    }
}
