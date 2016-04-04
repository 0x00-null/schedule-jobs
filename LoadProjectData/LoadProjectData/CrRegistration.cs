using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadProjectData
{
    public class CrRegistration
    {
        
        public string OrganizationName { get; set; }
        public string LocationName { get; set; }
        public int ParticipantId { get; set; }
        public int InitiativeId { get; set; }
        public int SpouseParticipation { get; set; }
        public string AddlInfo { get; set; }
        public int DomainId { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
