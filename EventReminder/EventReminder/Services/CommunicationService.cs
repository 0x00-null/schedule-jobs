using System;
using System.Collections.Generic;
using EventReminder.Models;
using EventReminder.Utils;
using Newtonsoft.Json;

namespace EventReminder.Services
{
    public class CommunicationService : BaseService
    {
        private static readonly int MessagePageId = "messages-page-id".IntSetting();
        private static readonly int RecipientsSubPageId = "recipients-subpage-id".IntSetting();

        public static void SendMessage(Communication communication, string token)
        {
            var communicationId = AddCommunication(communication, token);
            AddCommunicationMessage(communication, communicationId, token);
        }

        private static int AddCommunication(Communication communication, string token)
        {
            var dictionary = new Dictionary<string, object>
            {
                {"Subject", communication.EmailSubject},
                {"Body", communication.EmailBody},
                {"Author_User_Id", communication.AuthorUserId},
                {"Start_Date", DateTime.Now},
                {"From_Contact", communication.FromContactId},
                {"Reply_to_Contact", communication.ReplyContactId},
                {"Communication_Status_ID", "communication-status-id".IntSetting()}
            };

            var communicationId = CreatePageRecord(MessagePageId, dictionary, token);
            return communicationId;
        }

        private static void AddCommunicationMessage(Communication communication, int communicationId, string token)
        {
            var dictionary = new Dictionary<string, object>
            {
                {"Action_Status_ID", "action-status-id".IntSetting()},
                {"Action_Status_Time", DateTime.Now},
                {"Contact_ID", communication.ToContactId},
                {"From", communication.FromEmailAddress},
                {"To", communication.ToEmailAddress},
                {"Reply_To", communication.ReplyToEmailAddress},
                {"Subject", communication.EmailSubject},
                {"Body", communication.EmailBody}
            };
            CreateSubPageRecord(RecipientsSubPageId, communicationId, dictionary, token);
        }

        public static MessageTemplate GetTemplate(int templateId, string token)
        {
            var pageRecords = GetPageRecord(MessagePageId, token, templateId);

            if (pageRecords == null || pageRecords.Count == 0)
            {
                throw new InvalidOperationException("Couldn't find message template.");
            }

            var json = JsonConvert.SerializeObject(pageRecords);
            var template = JsonConvert.DeserializeObject<MessageTemplate>(json);

            return template;
        }
    }
}