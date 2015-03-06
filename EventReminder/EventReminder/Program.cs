using System;
using System.Linq;
using System.Reflection;
using EventReminder.Models;
using EventReminder.Services;
using EventReminder.Utils;
using log4net;
using log4net.Config;

[assembly: XmlConfigurator(Watch = true)]

namespace EventReminder
{
    public class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static string _token;

        private static void Main()
        {
            //config logger
            XmlConfigurator.Configure();

            GetToken();

            SendEventReminders();
        }

        private static void GetToken()
        {
            try
            {
                _token = AuthenticationService.Authenticate("register-api-user".StringSetting(),
                    "register-api-password".StringSetting());
            }
            catch (Exception ex)
            {
                Log.Error("Error Authenticating", ex);
                Environment.Exit(1);
            }
        }

        private static void SendEventReminders()
        {
            try
            {
                var events = EventService.EventsReadyForReminder(_token).ToList();
                if (!events.Any())
                {
                    Log.Info("No event reminders to send today.");
                    return;
                }

                var template = CommunicationService.GetTemplate("message-template-id".IntSetting(), _token);
                var fromContact = ContactService.GetContact("from-contact-id".IntSetting(), _token);

                foreach (var e in events)
                {
                    //get participants
                    var participants = EventService.EventParticipants(_token, e.EventId);

                    foreach (var participant in participants)
                    {
                        //merge template!
                        var emailBody = template.Body;
                        emailBody = emailBody.Replace("[Nickname]", participant.Nickname);
                        emailBody = emailBody.Replace("[Event_Title]", e.EventTitle);
                        emailBody = emailBody.Replace("[Event_Start_Date]", e.EventStartDate.ToShortDateString());
                        emailBody = emailBody.Replace("[Event_Start_Time]", e.EventStartDate.ToShortTimeString());
                        
                        var communication = new Communication
                        {
                            AuthorUserId = "author-user-id".IntSetting(),
                            DomainId = "crds-domain".IntSetting(),
                            EmailBody = emailBody,
                            EmailSubject = template.Subject,
                            FromContactId = fromContact.Contact_Id,
                            FromEmailAddress = fromContact.Email_Address,
                            ReplyContactId = fromContact.Contact_Id,
                            ReplyToEmailAddress = fromContact.Email_Address,
                            ToContactId = participant.Contact_Id,
                            ToEmailAddress = participant.Email_Address
                        };

                        CommunicationService.SendMessage(communication, _token);
                    }
                    //update reminder sent switch
                    e.ReminderSent(_token);
                    Log.Info(e.EventTitle + " finished.");
                }
                Log.Info(events.Count() + " events processed.");
            }
            catch (Exception e)
            {
                Log.Error(e);
                Environment.Exit(1);
            }
        }
    }
}