using Fisher.Attributes;

namespace MyLittleRangeBook.Models
{
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