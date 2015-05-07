using System;
using System.Web.Script.Serialization;

namespace MigrateMPData.Models
{
    public class MinistryPlatformTable
    {
        public String tableName { get; set; }
        public String filterClause { get; set; }
        public MigrationType migrationType { get; set; }

        public override string ToString()
        {
            return (new JavaScriptSerializer().Serialize(this));
        }
    }

    public enum MigrationType
    {
        INSERT_OR_UPDATE,
        INSERT_ONLY
    }
}
