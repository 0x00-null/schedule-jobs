

namespace LoadProjectData
{
    public class CrProject
    {
        public string ProjectName  { get; set; }
        public int ProjectStatusId { get; set; }
        public string LocationName { get; set; }
        public string ProjectTypeName { get; set; }
        public string OrganizationName { get; set; }
        public int InitiativeId { get; set; }
        public int MinVol { get; set; }
        public int MaxVol { get; set; }
        public int AbsoluteMaxVol { get; set; }
        public int DomainId { get; set; }
    }
}
