

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

        public string CheckInFloor { get; set; }
        public string CheckInArea { get; set; }
        public string CheckInRoomNumber { get; set; }
        public string Note1 { get; set; }
        public string Note2 { get; set; }
        public string ParkingLocation { get; set; }

        public string Address1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }
}
