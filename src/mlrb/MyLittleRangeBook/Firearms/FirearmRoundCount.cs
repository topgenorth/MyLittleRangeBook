using Fisher.Attributes;

namespace MyLittleRangeBook.Models
{

    public class FirearmRangeVisitCount
    {
        public Guid   Id         { get; set; }
        [UniqueIndex]
        public string Name       { get; set; }
        public int    VisitCount { get; set; }
    }
    /// <summary>
    /// Represents the count of rounds for a specific firearm.
    /// </summary>
    public class FirearmRoundCount
    {
        public Guid   Id         { get; set; }
        [UniqueIndex]
        public string Name       { get; set; }
        public int    RoundCount { get; set; }
    }
}