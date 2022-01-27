using System;

namespace FragmentNetslumServer.Models
{
    public class MailMetaModel
    {
        public virtual int Mail_ID { get; set; }
        public virtual int Receiver_Account_ID { get; set; }
        public virtual DateTime date {get; set; }
        public virtual int Sender_Account_ID { get; set; }
        public virtual byte[] Sender_Name {get; set; }
        public virtual byte[] Receiver_Name {get; set; }
        public virtual byte[] Mail_Subject {get; set; }
        public virtual Boolean Mail_Delivered { get; set; }
    }
}