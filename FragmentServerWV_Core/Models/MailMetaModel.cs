using System;
namespace FragmentServerWV.Models
{
    public class MailMetaModel
    {
        public virtual int Mail_ID { get; set; }
        public virtual int Receiver_Account_ID { get; set; }
        public virtual DateTime date {get; set; }
        public virtual int Sender_Account_ID { get; set; }
        public virtual string Sender_Name {get; set; }
        public virtual string Receiver_Name {get; set; }
        public virtual string Mail_Subject {get; set; }
    }
}