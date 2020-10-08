namespace FragmentServerWV.Models
{
    public class RankingDataModel
    {
        public virtual int id { get; set; }
        public virtual string antiCheatEngineResult {get; set; }
        public virtual string loginTime {get; set; }
        public virtual string diskID {get; set; }
        public virtual string saveID {get; set; }
        public virtual string characterSaveID {get; set; }
        public virtual string characterName {get; set; }
        public virtual int characterLevel {get; set; }
        public virtual string characterClassName {get; set; }
        public virtual int characterHP {get; set; }
        public virtual int characterSP {get; set; }
        public virtual int characterGP {get; set; }
        public virtual int godStatusCounterOnline {get; set; }
        public virtual int averageFieldLevel {get; set; }

    }
}