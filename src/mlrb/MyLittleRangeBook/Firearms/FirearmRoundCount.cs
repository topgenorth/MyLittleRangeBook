using Fisher.Attributes;

namespace MyLittleRangeBook.Models
{
    public class RangeVisitCount
    {
        public               Guid   Id         { get; set; }
        [UniqueIndex] public string Name       { get; set; } = "Unknown Range";
        public               int    VisitCount { get; set; }
    }

    /// <summary>
    ///     Represents the count of rounds for a specific firearm.
    /// </summary>
    public class FirearmRoundCount
    {
        public               Guid   Id         { get; set; }
        [UniqueIndex] public string Name       { get; set; } = "Unknown Firearm";
        public               int    RoundCount { get; set; }
    }
}