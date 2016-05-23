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

            var m = new MpDao();
            
            var package = new ExcelPackage(new FileInfo(args[0]));

            
            ExcelWorksheet workSheet = package.Workbook.Worksheets.First();

           // loop over each row
            for (var row = workSheet.Dimension.Start.Row; row < workSheet.Dimension.End.Row; row++)
            {
                // get info from the sheet
                var projectName = workSheet.Cells[row, _projnamecol].Value.ToString();
                var projectType = workSheet.Cells[row, _projtypecol].Value.ToString();
                var min = Convert.ToInt32( workSheet.Cells[row, _mincol].Value);
                var max = Convert.ToInt32( workSheet.Cells[row, _maxcol].Value);
                var supermax = Convert.ToInt32( workSheet.Cells[row, _supermaxcol].Value);
                var locationName = workSheet.Cells[row, _locationnamecol].Value.ToString();
                var orgName = workSheet.Cells[row, _orgnamecol].Value.ToString();
                var tcEmails = workSheet.Cells[row, _tcemailscol].Value.ToString();

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
                    InitiativeId = 1
                };

                var projDao = new CrProjectDao();
                var projectId = projDao.Insert(p);

                //Get a Participant ID for each TC in list
                var tcEmailList = tcEmails.Split(';').ToList();
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
                    catch (Exception ex)
                    {
                        var str = p.ProjectName + ":" + email + " not found";
                       log.Warn(str);
                    }
                    
                }

            }
           
        }

    }
}
