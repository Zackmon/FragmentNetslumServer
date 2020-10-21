using System;

namespace FragmentServerWV.Models
{
    public class BbsPostMetaModel
    {
        public virtual int unk0 { get; set; }
        public virtual int postID {get; set; }
        public virtual int unk2 {get; set; }
        public virtual DateTime date {get; set; }
        public virtual string username {get; set; }
        public virtual string subtitle {get; set; }
        public virtual byte[] title {get; set; }
        public virtual string unk3 {get; set; }
        public virtual int threadID {get; set; }

    }
}