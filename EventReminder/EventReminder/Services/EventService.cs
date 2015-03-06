using System;
using System.Collections.Generic;
using EventReminder.Models;
using EventReminder.Utils;

namespace EventReminder.Services
{
    public class EventService : BaseService
    {
        private static readonly int EventPageIdNeedReminders = "event-needs-reminder-page-id".IntSetting();
        private static readonly int ParticipantSubPageId = "event-participant-subpage-id".IntSetting();

        public static IEnumerable<Event> EventsReadyForReminder(string token)
        {
            var events = new List<Event>();

            var pageRecords = GetPageRecords(EventPageIdNeedReminders, token, string.Empty);
            foreach (var pageRecord in pageRecords)
            {
                var e = new Event
                {
                    EventId = (int) pageRecord["Event_ID"],
                    EventTitle = (string) pageRecord["Event_Title"],
                    EventStartDate = (DateTime) pageRecord["Event_Start_Date"]
                };
                events.Add(e);
            }
            return events;
        }

        public static IEnumerable<Contact> EventParticipants(string token, int eventId)
        {
            var contacts = new List<Contact>();
            var subPageRecords = GetSubPageRecords(ParticipantSubPageId, eventId, token);

            foreach (var subPageRecord in subPageRecords)
            {
                var participationStatus = (string) subPageRecord["Participation_Status"];
                if (participationStatus != "02 Registered") continue;

                var contact = new Contact
                {
                    Contact_Id = (int) subPageRecord["Contact_ID"],
                    Email_Address = (string) subPageRecord["Email_Address"],
                    Nickname = (string) subPageRecord["Nickname"]
                };
                contacts.Add(contact);
            }

            return contacts;
        }

        public static void SetReminderFlag(string token, int eventId)
        {
            var dict = new Dictionary<string, object>
            {
                {"Event_ID", eventId},
                {"Reminder_Sent", 1}
            };

            UpdateRecord(EventPageIdNeedReminders, dict, token);
        }
    }
}