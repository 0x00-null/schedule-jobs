using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var rc = "";

            const string query = "SELECT c.nickname, c.last_name from contacts c join participants p on p.contact_id = c.contact_id " +
                                 "where p.participant_id = @Participant_Id";

            var mp = new MpDao();

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
