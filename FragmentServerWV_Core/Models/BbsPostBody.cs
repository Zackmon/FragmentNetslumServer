namespace FragmentServerWV.Models
{
    public class BbsPostBody
    {
        public virtual int postBodyID {get; set; }
        public virtual byte[] postBody { get; set;}
        public virtual int postID { get; set;}
    }
}