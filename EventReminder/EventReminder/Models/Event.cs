using System;
using EventReminder.Services;

namespace EventReminder.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public DateTime EventStartDate { get; set; }

        public void ReminderSent(string token)
        {
            EventService.SetReminderFlag(token, this.EventId);
        }
    }
}