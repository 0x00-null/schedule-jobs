using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace MigrateMPData.UnityUtil
{
    /// <summary>
    /// Accepts a ConnectionString, and returns an IDbConnection instance of the proper type.
    /// 
    /// Borrowed from an MSDN blog post, circa 2009:
    /// http://blogs.msdn.com/b/ukadc/archive/2009/02/04/creating-database-connections-with-unity-part-2.aspx
    /// 
    /// Why is something so simple not built into the Unity or ComponentModel framework somewhere??
    /// </summary>
    public class DbConnectionNameTypeConverter : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string connectionStringName = (string)value;
            ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];

            DbProviderFactory providerFactory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);
            IDbConnection connection = providerFactory.CreateConnection();
            connection.ConnectionString = Environment.ExpandEnvironmentVariables(connectionStringSettings.ConnectionString);
            return connection;
        }
    }
}
