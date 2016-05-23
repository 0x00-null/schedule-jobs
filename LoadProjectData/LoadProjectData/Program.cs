using System;
using System.Linq;
using System.IO;
using OfficeOpenXml;

namespace LoadProjectData
{
    internal class Program
    {
        private static int _projnamecol = 8;
        private static int _projtypecol = 20;
        private static int _mincol = 24;
        private static int _maxcol = 25;
        private static int _supermaxcol = 26;
        private static int _locationnamecol = 82;
        private static int _orgnamecol = 83;
        private static int _tcemailscol = 39;

        private static int _checkinfloorcol;
        private static int _checkinareacol;
        private static int _checkinroomcol;
        private static int _notetovol1col;
        private static int _notetovol2col;
        private static int _projectaddresscol;
        private static int _projectcitycol;
        private static int _projectstatecol;
        private static int _projectzipcol;
        private static int _projectparkingloccol;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static void Main(string[] args)
        {
            if(args.Length != 1)
            {
                Console.WriteLine("Usage: 1 parameter with path to the .xlsx file");
                Console.Read();
                return;
            }

            _projnamecol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Projnamecol"]);
            _projtypecol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Projtypecol"]);
            _mincol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Mincol"]);
            _maxcol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Maxcol"]);
            _supermaxcol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Supermaxcol"]);
            _locationnamecol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Locationnamecol"]);
            _orgnamecol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Orgnamecol"]);
            _tcemailscol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Tcemailscol"]);

            _checkinfloorcol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["CheckInFloorCol"]);
            _checkinareacol  = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["CheckInAreaCol"]);
            _checkinroomcol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["CheckInRoomCol"]);
            _notetovol1col  = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["NoteToVol1Col"]);
            _notetovol2col  = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["NoteToVol2Col"]);
            _projectaddresscol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ProjectAddressCol"]);
            _projectcitycol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ProjectCityCol"]);
            _projectstatecol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ProjectStateCol"]);
            _projectzipcol  = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ProjectZipCol"]);
            _projectparkingloccol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ProjectParkingLocCol"]);

            var m = new MpDao();
            
            var package = new ExcelPackage(new FileInfo(args[0]));

            
            ExcelWorksheet workSheet = package.Workbook.Worksheets.First();

           // loop over each row
            for (var row = workSheet.Dimension.Start.Row; row <= workSheet.Dimension.End.Row; row++)
            {
                // get info from the sheet
                var projectName = workSheet.Cells[row, _projnamecol].Value.ToString();

                //var projectType = workSheet.Cells[row, _projtypecol].Value.ToString();
                //var min = Convert.ToInt32( workSheet.Cells[row, _mincol].Value);
                //var max = Convert.ToInt32( workSheet.Cells[row, _maxcol].Value);
                //var supermax = Convert.ToInt32( workSheet.Cells[row, _supermaxcol].Value);
                //var locationName = workSheet.Cells[row, _locationnamecol].Value.ToString();
                //var orgName = workSheet.Cells[row, _orgnamecol].Value.ToString();
                //var tcEmails = workSheet.Cells[row, _tcemailscol].Value.ToString();

                var projectType = "";
                var min = 99;
                var max = 99;
                var supermax = 99;
                var locationName = "";
                var orgName = "";
                var tcEmails = "";


                var checkinfloor = workSheet.Cells[row, _checkinfloorcol].Value == null ? "" : workSheet.Cells[row, _checkinfloorcol].Value.ToString();
                var checkinarea = workSheet.Cells[row, _checkinareacol].Value == null ? "" : workSheet.Cells[row, _checkinareacol].Value.ToString();
                var checkinroomnumber = workSheet.Cells[row, _checkinroomcol].Value == null ? "" : workSheet.Cells[row, _checkinroomcol].Value.ToString();
                var note1 = workSheet.Cells[row, _notetovol1col].Value == null ? "" : workSheet.Cells[row, _notetovol1col].Value.ToString();
                var note2 = workSheet.Cells[row, _notetovol2col].Value == null ? "" : workSheet.Cells[row, _notetovol2col].Value.ToString();
                var parking = workSheet.Cells[row, _projectparkingloccol].Value == null ? "" : workSheet.Cells[row, _projectparkingloccol].Value.ToString();
                var projaddr = workSheet.Cells[row, _projectaddresscol].Value == null ? "" : workSheet.Cells[row, _projectaddresscol].Value.ToString();
                var projcity = workSheet.Cells[row, _projectcitycol].Value == null ? "" : workSheet.Cells[row, _projectcitycol].Value.ToString();
                var projstate = workSheet.Cells[row, _projectstatecol].Value == null ? "" : workSheet.Cells[row, _projectstatecol].Value.ToString();
                var projzip = workSheet.Cells[row, _projectzipcol].Value == null ? "" : workSheet.Cells[row, _projectzipcol].Value.ToString();

                //create project
                var p = new CrProject
                {
                    ProjectName = projectName,
                    OrganizationName = orgName,
                    ProjectTypeName = projectType,
                    LocationName = locationName,
                    MinVol = min,
                    MaxVol = max,
                    AbsoluteMaxVol = supermax,
                    DomainId = 1,
                    InitiativeId = 1,
                    CheckInFloor = checkinfloor,
                    CheckInArea = checkinarea,
                    CheckInRoomNumber = checkinroomnumber,
                    Note1 = note1,
                    Note2 = note2,
                    ParkingLocation = parking,
                    Address1 = projaddr,
                    City= projcity,
                    State = projstate,
                    Zip=projzip
                };

                Console.WriteLine(row.ToString());

                var projDao = new CrProjectDao();
                var projectId = projDao.Insert(p);

                if (false)
                {
                    
                    //Get a Participant ID for each TC in list
                    var tcEmailList = tcEmails.Split(',').ToList();
                    foreach (var email in tcEmailList)
                    {
                        try
                        {
                            var participantid = m.GetParticipantId(email);
                            //create registration
                            var regDao = new CrRegistrationDao();
                            var reg = new CrRegistration
                            {
                                ParticipantId = participantid,
                                AddlInfo = "Created By GO Cincy Import App",
                                CreationDate = DateTime.Now,
                                DomainId = 1,
                                InitiativeId = 1,
                                LocationName = p.LocationName,
                                OrganizationName = p.OrganizationName,
                                SpouseParticipation = 0
                            };
                            var registrationId = regDao.Insert(reg);

                            //create GroupConnector
                            var gc = new CrGroupConnector
                            {
                                ProjectId = projectId,
                                DomainId = 1,
                                PrimaryRegistration = registrationId
                            };
                            var gcdao = new CrGroupConnectorDao();
                            var groupconnectorid = gcdao.Insert(gc);

                            //create GroupConnectorRegistration
                            var gcr = new CrGroupConnectorRegistration
                            {
                                GroupConnectorId = groupconnectorid,
                                RegistrationId = registrationId,
                                DomainId = 1,
                                RoleId = 22
                            };
                            var gcregdao = new CrGroupConnectorRegistrationDao();
                            gcregdao.Insert(gcr);

                        }


                        catch
                            (Exception ex)
                        {
                            var str = p.ProjectName + ":" + p.LocationName + ":" + email + " not found";
                            log.Warn(str);
                        }

                    }
                }

            }
           
        }

    }
}
