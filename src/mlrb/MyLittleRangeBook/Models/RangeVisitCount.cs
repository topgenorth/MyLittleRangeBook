using Fisher.Attributes;
using Fisher.Projections.Flattened;
using JasperFx.Events.Aggregation;
using MyLittleRangeBook.EventSourcing;

namespace MyLittleRangeBook.Models
{
    public class RangeVisitProjection : FlatTableProjection
    {
        public RangeVisitProjection() : base("range_visit_counts")
        {
            // 1. Set the natural key column as the primary key
            Table.AddColumn("range_name",  "TEXT").AsPrimaryKey();
            Table.AddColumn("visit_count", "INTEGER").DefaultValue(0).NotNull();

            // 2. Pass the natural key selector as the second parameter (primaryKeySource)
            Project<FirearmUsedAtRange>(
                                        map =>
                                        {
                                            map.Map(x => x.RangeName, "range_name");
                                            map.Increment("visit_count"); // Or map.Increment(x => 1, "visit_count");
                                        },
                                        x => x.RangeName); // <-- Natural Key / Primary Key Source
        }
    }

    public class RangeVisitCount
    {
        public               Guid           Id              { get; set; }
        [UniqueIndex, NaturalKey] public string         Name            { get; set; } = "Unknown Range";
        public               int            VisitCount      { get; set; }
        public               DateTimeOffset MostRecentVisit { get; set; }
    }
}