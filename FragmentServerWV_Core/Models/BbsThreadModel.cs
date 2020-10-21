namespace FragmentServerWV.Models
{
    public class BbsThreadModel
    {
        public virtual int threadID { get; set; }
        public virtual byte[] threadTitle { get; set; }
        public virtual int categoryID { get; set; }
    }
}