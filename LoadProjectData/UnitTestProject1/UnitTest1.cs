using System;
using System.Configuration;
using System.Security.Cryptography;
using NUnit.Framework;
using LoadProjectData;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;

namespace UnitTestProject1
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public void TestEmailParse()
        {
            var mp = new MpDao();
            const string teststr = "Garritanot@gmail.com";
            var rc=mp.GetParticipantId(teststr);
            Assert.IsNotNull(rc);
            Assert.IsTrue(rc != -1);
        }

        [Test]
        public void Test_Insert_Project()
        {
            var dao = new CrProjectDao();
            var proj = new CrProject
            {
                ProjectName = "Build Something New",
                LocationName = "Oakley",
                OrganizationName = "Crossroads",
                ProjectTypeName = "Painting",
                DomainId = 1,
                InitiativeId = 1,
                ProjectStatusId = 1,
                MinVol = 2,
                MaxVol = 5,
                AbsoluteMaxVol = 7
            };
            proj.CheckInFloor = "1";
            proj.CheckInRoomNumber = "200";
            proj.CheckInArea = "Front of the room";
            proj.ParkingLocation = "Down the road";
            proj.Note1 = "note number 1";
            proj.Note2 = "another note";

            proj.Address1 = "123 Main St";
            proj.City = "Walton";
            proj.State = "KY";
            proj.Zip = "12345";


            var rc =dao.Insert(proj);
            Assert.IsTrue(rc > 0);
        }

        [Test]
        public void Test_GetProjectId()
        {
            var dao = new CrProjectDao();
            var rc = dao.GetProjectId("Build Matts Barn");
            Assert.IsTrue(rc > 0);
        }

        [Test]
        public void Test_Update_Project()
        {
            var dao = new CrProjectDao();
            var proj = new CrProject
            {
                ProjectName = "Build Matts Barn",
                LocationName = "Mason",
                OrganizationName = "Crossroads",
                ProjectTypeName = "Construction",
                DomainId = 1,
                InitiativeId = 1,
                ProjectStatusId = 1,
                MinVol = 2,
                MaxVol = 5,
                AbsoluteMaxVol = 1000,
                CheckInFloor = "1",
                CheckInRoomNumber = "999",
                CheckInArea = "Arg",
                Note1 = " Note 1",
                Note2 = "Note 2222",
                ParkingLocation = "Park Loc",
                Address1 = "123 Main St.",
                City = "Walton",
                State = "KY",
                Zip = "90210"
            };


            var rc = dao.Update(proj);
            Assert.IsTrue(rc > 0);
        }

        [Test]
        public void Test_Insert_Registration()
        {
            var dao = new CrRegistrationDao();
            var proj = new CrRegistration
            {
                LocationName = "Oakley",
                OrganizationName = "Crossroads",
                ParticipantId = 5989947,
                DomainId = 1,
                InitiativeId = 1,
                SpouseParticipation = 0,
                AddlInfo = "Additional Information",
                CreationDate = DateTime.Now
            };


            var rc = dao.Insert(proj);
            Assert.IsTrue(rc > 0);
        }

        [Test]
        public void Test_Get_Name()
        {
            var dao = new ContactsDao();

            var rc = dao.GetContactNameFromParticipantId(5989947);
            Assert.IsFalse(rc.Equals("Unknown"));
            Assert.IsFalse(rc.Equals(""));
        }

        [Test]
        public void Test_Insert_GroupConnector()
        {
            var m = new MpDao();
            var dao = new CrGroupConnectorDao();
            var groupconnector = new CrGroupConnector
            {
                ProjectId = 1,
                DomainId = 1,
                PrimaryRegistration = 1
            };

            var rc = dao.Insert(groupconnector);
            Assert.IsTrue(rc > 0);
            
        }

        [Test]
        public void Test_Insert_GroupConnectorRegistrations()
        {
            var m = new MpDao();
            var dao = new CrGroupConnectorRegistrationDao();
            var groupconnectorregistration = new CrGroupConnectorRegistration
            {
                GroupConnectorId = 20,
                RegistrationId = 1,
                DomainId = 1,
                RoleId = 22
            };

            var rc = dao.Insert(groupconnectorregistration);
            Assert.IsTrue(rc > 0);

        }

        [Test]
        public void Test_Insert_Address()
        {
            var addrDao = new CrAddressDao();
            
            var addr = new CrAddress
            {
                AddressLine1 = "13010 Farmview Dr",
                City = "Independence",
                State = "KY",
                PostalCode = "41051"
            };

            var rc = addrDao.Insert(addr);
            Assert.IsTrue(rc > 0);

        }

        [Test]
        public void Test_Get_AddressId()
        {
            var dao = new CrProjectDao();

            var rc = dao.GetProjectAddressId("Build Matts Barn");
            Assert.IsFalse(rc > 1);
        }
    }
}
