using System;

namespace FragmentServerWV.Models
{
    public class NewsSectionModel
    {
        public virtual ushort ArticleID { get; set; }
        public virtual string ArticleTitle { get; set; }
        public virtual string ArticleBody { get; set; }
        public virtual DateTime ArticleDate { get; set; }
        public virtual bool IsNew { get; set; }
        public virtual byte[] ArticleImage { get; set; }
        
        public virtual byte[] ArticleByteArray { get; set; }
        public virtual byte[] ImageSizeInfo { get; set; }
        public virtual byte[] ImageDetails { get; set; }
    }
}