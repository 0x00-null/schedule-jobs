using System;
using EventReminder.Services;
using NUnit.Framework;

namespace EventReminder.Testing
{
    [TestFixture]
    public class ReminderTests
    {
        private string _token;

        [SetUp]
        public void Setup()
        {
            var uid = Environment.GetEnvironmentVariable("API_USER");
            var pwd = Environment.GetEnvironmentVariable("API_PASSWORD");
            _token = AuthenticationService.Authenticate(uid, pwd);
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
    }
}
