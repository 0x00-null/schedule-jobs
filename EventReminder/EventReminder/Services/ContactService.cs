using System;
using EventReminder.Models;
using EventReminder.Utils;
using Newtonsoft.Json;

namespace EventReminder.Services
{
    public class ContactService : BaseService
    {
        public static Contact GetContact(int contactId, string token)
        {
            var pageRecords = GetPageRecord("contact-page-id".IntSetting(), token, contactId);

            if (pageRecords == null || pageRecords.Count == 0)
            {
                throw new InvalidOperationException("GetMyContact - no data returned.");
            }

            var json = JsonConvert.SerializeObject(pageRecords);
            var contact = JsonConvert.DeserializeObject<Contact>(json);

            return contact;
        }
    }
}