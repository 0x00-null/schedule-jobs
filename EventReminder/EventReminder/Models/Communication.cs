namespace EventReminder.Models
{
    public class Communication
    {
        public int AuthorUserId { get; set; }
        public int DomainId { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public int FromContactId { get; set; }
        public string FromEmailAddress { get; set; }
        public int ReplyContactId { get; set; }
        public string ReplyToEmailAddress { get; set; }
        public int ToContactId { get; set; }
        public string ToEmailAddress { get; set; }
    }
}