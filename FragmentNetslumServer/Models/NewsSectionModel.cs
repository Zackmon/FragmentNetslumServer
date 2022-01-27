using System;

namespace FragmentNetslumServer.Models
{
    public class NewsSectionModel
    {
        public virtual ushort ArticleID { get; set; }
        public virtual string ArticleTitle { get; set; }
        public virtual string ArticleBody { get; set; }
        public virtual DateTime ArticleDate { get; set; }
        public virtual byte[] ArticleImage { get; set; }
        
        public virtual byte[] ArticleByteArray { get; set; }
        public virtual byte[] ImageSizeInfo { get; set; }
        public virtual byte[] ImageDetails { get; set; }

        public virtual NewsSectionModel Clone()
        {
            NewsSectionModel clone = new NewsSectionModel
            {
                ArticleID = this.ArticleID,
                ArticleTitle = this.ArticleTitle,
                ArticleBody = this.ArticleBody,
                ArticleDate = this.ArticleDate,
                ArticleImage = this.ArticleImage,
                ArticleByteArray = this.ArticleByteArray,
                ImageSizeInfo = this.ImageSizeInfo
            };

            return clone;
        }
    }
}