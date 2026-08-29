using Fisher.Attributes;

namespace MyLittleRangeBook.Models
{
    public class RangeVisitCount
    {
        public               Guid           Id              { get; set; }
        [UniqueIndex] public string         Name            { get; set; } = "Unknown Range";
        public               int            VisitCount      { get; set; }
        public               DateTimeOffset MostRecentVisit { get; set; }
    }
}