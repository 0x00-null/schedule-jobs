using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace LoadProjectData
{
    public class CrAddressDao
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MinistryPlatform"].ConnectionString;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool Exists(CrAddress addr)
        {
            var rc = false;
            var count = 0;
            const string query = "SELECT count(*) FROM dbo.Addresses WHERE address_line_1 = @Address AND City = @City AND [State/Region] = @State AND Postal_Code = @Zip";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 75).Value = addr.AddressLine1;
                cmd.Parameters.Add("@City", SqlDbType.NVarChar, 50).Value = addr.City;
                cmd.Parameters.Add("@State", SqlDbType.NVarChar, 50).Value = addr.State;
                cmd.Parameters.Add("@Zip", SqlDbType.NVarChar, 15).Value = addr.PostalCode;

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

        public int Insert(CrAddress addr)
        {
           
            var rc = -1;
            const string query = "INSERT INTO dbo.Addresses (Address_Line_1,City,[State/Region],Postal_Code,Domain_ID) " +
                                 "OUTPUT INSERTED.Address_ID " +
                                 "VALUES (@Address,@City,@State,@Zip,@DomainID) ";

            var mp = new MpDao();

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {

                cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 100).Value = addr.AddressLine1;
                cmd.Parameters.Add("@City", SqlDbType.NVarChar).Value = addr.City;
                cmd.Parameters.Add("@State", SqlDbType.NVarChar).Value = addr.State;
                cmd.Parameters.Add("@Zip", SqlDbType.NVarChar).Value = addr.PostalCode;
                cmd.Parameters.Add("@DomainID", SqlDbType.NVarChar).Value = 1;

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
