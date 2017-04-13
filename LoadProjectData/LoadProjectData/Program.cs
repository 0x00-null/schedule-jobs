using System;
using System.Linq;
using System.IO;
using OfficeOpenXml;

namespace LoadProjectData
{
    internal class Program
    {
        //First Pass Columns
        private static int _projnamecol;
        private static int _projtypecol;
        private static int _mincol;
        private static int _maxcol;
        private static int _supermaxcol;
        private static int _locationnamecol;
        private static int _orgnamecol;
        private static int _tcemailscol;
        private static int _projectaddresscol;
        private static int _projectcitycol;
        private static int _projectstatecol;
        private static int _projectzipcol;

        //Second Pass Columns
        private static int _checkinfloorcol;
        private static int _checkinareacol;
        private static int _checkinroomcol;
        private static int _notetovol1col;
        private static int _notetovol2col;
        private static int _projectparkingloccol;

        private static int _initiative;
        private static bool _updatePass;
        private static string _orgName;
        private static string _locationName;
        private static string _fileName;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static void Main(string[] args)
        {
            if (args.Length <= 1 || args.Length > 5)
            {
                Console.WriteLine("Usage: LoadProjectData filename [-I:initiative] [-Update] [-O:organization_name] [-L:location_name]");
                Console.WriteLine("\t-I:initiative the ID of the initiative");
                Console.WriteLine("\t-update: pass this flag if this is the second pass adding additional data.");
                Console.WriteLine("\t-O:organization_name: The name of the organization these projects are for. eg. Crossroads or Archdiocese");
                Console.WriteLine("\t-L:location_name: The name of the location these projects are for. eg. Oakley or Andover");
                Console.Read();
                return;
            }
            foreach (var arg in args)
            {
                if (arg.ToUpper().StartsWith("-I"))
                {
                    _initiative = int.Parse(arg.Substring(arg.IndexOf(':') + 1));
                }
                else if (arg.ToUpper().StartsWith("-U"))
                {
                    _updatePass = true;
                }
                else if (arg.ToUpper().StartsWith("-O"))
                {
                    _orgName = arg.Substring(arg.IndexOf(':') + 1);
                }
                else if (arg.ToUpper().StartsWith("-L"))
                {
                    _locationName = arg.Substring(arg.IndexOf(':') + 1);
                }
                else
                {
                    _fileName = arg;
                }    
            }

            _projnamecol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Projnamecol"]);
            _projtypecol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Projtypecol"]);
            _mincol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Mincol"]);
            _maxcol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Maxcol"]);
            _supermaxcol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Supermaxcol"]);
            _locationnamecol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Locationnamecol"]);
            _orgnamecol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Orgnamecol"]);
            _tcemailscol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Tcemailscol"]);
            _projectaddresscol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ProjectAddressCol"]);
            _projectcitycol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ProjectCityCol"]);
            _projectstatecol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ProjectStateCol"]);
            _projectzipcol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ProjectZipCol"]);

            _checkinfloorcol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["CheckInFloorCol"]);
            _checkinareacol  = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["CheckInAreaCol"]);
            _checkinroomcol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["CheckInRoomCol"]);
            _notetovol1col  = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["NoteToVol1Col"]);
            _notetovol2col  = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["NoteToVol2Col"]);
            _projectparkingloccol = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ProjectParkingLocCol"]);

            var m = new MpDao();
            
            var package = new ExcelPackage(new FileInfo(_fileName));

            
            ExcelWorksheet workSheet = package.Workbook.Worksheets.First();

           // loop over each row
            for (var row = workSheet.Dimension.Start.Row + 1; row <= workSheet.Dimension.End.Row; row++)
            {
                //create project with info from the sheet
                var p = new CrProject
                {
                    ProjectName = ReadStringFromFile(workSheet, row, _projnamecol),
                    OrganizationName = _orgName ?? ReadStringFromFile(workSheet, row, _orgnamecol),
                    ProjectTypeName = ReadStringFromFile(workSheet, row, _projtypecol),
                    LocationName = _locationName ?? ReadStringFromFile(workSheet, row, _locationnamecol),
                    MinVol = ReadIntFromFile(workSheet, row, _mincol),
                    MaxVol = ReadIntFromFile(workSheet, row, _maxcol),
                    AbsoluteMaxVol = ReadIntFromFile(workSheet, row, _supermaxcol),
                    DomainId = 1,
                    InitiativeId = _initiative,
                    CheckInFloor = _updatePass ? ReadStringFromFile(workSheet, row, _checkinfloorcol) : "",
                    CheckInArea = _updatePass ? ReadStringFromFile(workSheet, row, _checkinareacol) : "",
                    CheckInRoomNumber = _updatePass ? ReadStringFromFile(workSheet, row, _checkinroomcol) : "",
                    Note1 = _updatePass ? ReadStringFromFile(workSheet, row, _notetovol1col) : "",
                    Note2 = _updatePass ? ReadStringFromFile(workSheet, row, _notetovol2col) : "",
                    ParkingLocation = _updatePass ? ReadStringFromFile(workSheet, row, _projectparkingloccol) : "",
                    Address1 = ReadStringFromFile(workSheet, row, _projectaddresscol),
                    City= ReadStringFromFile(workSheet, row, _projectcitycol),
                    State = ReadStringFromFile(workSheet, row, _projectstatecol),
                    Zip = ReadStringFromFile(workSheet, row, _projectzipcol)
                };

                Console.WriteLine(row.ToString());

                var projDao = new CrProjectDao();
                var projectId = projDao.Insert(p);

                //Get a Participant ID for each TC in list
                var tcEmailList = ReadStringFromFile(workSheet, row, _tcemailscol).Split(',').ToList();
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
                            AddlInfo = "Created By GO Local Import App",
                            CreationDate = DateTime.Now,
                            DomainId = 1,
                            InitiativeId = _initiative,
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

        private static string ReadStringFromFile(ExcelWorksheet workSheet, int row, int column)
        {
            return workSheet.Cells[row, column].Value == null ? "" : workSheet.Cells[row, column].Value.ToString();
        }

        private static int ReadIntFromFile(ExcelWorksheet workSheet, int row, int column)
        {
            return Convert.ToInt32(workSheet.Cells[row, column].Value);
        }
    }
}
