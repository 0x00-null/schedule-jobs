using EventReminder.Models;
using EventReminder.Services;
using EventReminder.Utils;
using NUnit.Framework;

namespace EventReminder.Testing
{
    [TestFixture]
    public class Class1
    {
        private string _token;

        [SetUp]
        public void Setup()
        {
            _token = AuthenticationService.Authenticate("register-api-user".StringSetting(), "register-api-password".StringSetting());
        }

        [Test]
        public void GetTemplate()
        {
            var template = CommunicationService.GetTemplate(1459, _token);

            Assert.IsNotNull(template);
            Assert.AreEqual("Event Reminder", template.Subject);
        }
        [Test]
        public void GetContact()
        {
            var contact = ContactService.GetContact(1519180, _token);

            Assert.IsNotNull(contact);
            Assert.AreEqual(1519180, contact.Contact_Id);
        }

        [Test]
        public void EventParticipants()
        {
            const int eventId = 721130;
            var participants = EventService.EventParticipants(_token, eventId);

            Assert.IsNotNull(participants);
        }

        [Test]
        public void GetEvents()
        {
            var events = EventService.EventsReadyForReminder(_token);
            Assert.IsNotNull(events);
        }

        [Test]
        [Ignore]
        //Only used for development testing. Not needed in automated testing.
        public void SendCommunicationRest()
        {
            //Change this!
            var token = AuthenticationService.Authenticate("tmaddox@aol.com", "crds1234");
            var communication = new Communication
            {
                AuthorUserId = 5,
                DomainId = 1,
                EmailSubject = "test message",
                EmailBody = "test message",
                FromEmailAddress = "andrew.canterbury@ingagepartners.com",
                FromContactId = 768379,
                ReplyContactId = 768379,
                ReplyToEmailAddress = "andrew.canterbury@ingagepartners.com",
                ToContactId = 1668084,
                ToEmailAddress = "andrew.canterbury@ingagepartners.com",
            };
            CommunicationService.SendMessage(communication, token);
        }

    }
}
