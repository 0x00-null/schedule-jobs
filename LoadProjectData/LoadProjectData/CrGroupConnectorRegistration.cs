using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;

namespace LoadProjectData
{
    public class CrGroupConnectorRegistration
    {
        public int GroupConnectorId { get; set; }
        public int RegistrationId { get; set; }
        public int DomainId { get; set; }
        public int RoleId { get; set; }
    }
}
