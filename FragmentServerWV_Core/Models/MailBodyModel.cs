namespace FragmentServerWV.Models
{
    public class MailBodyModel
    {
        public virtual int Mail_Body_ID { get; set; }
        public virtual int Mail_ID { get; set; }
        public virtual byte[] Mail_Body { get; set; }
        public virtual string Mail_Face_ID { get; set; }
        
    }
}