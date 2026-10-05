using Fisher.Projections.Flattened;
using MyLittleRangeBook.Firearms;

namespace MyLittleRangeBook.RangeEvents
{

    /// <summary>
    /// Update the count of visits to a shooting range.
    /// </summary>
    public class RangeVisitProjection : FlatTableProjection
    {
        public RangeVisitProjection() : base("range_visit_counts")
        {
            // 1. Set the natural key column as the primary key
            Table.AddColumn("range_name",  "TEXT").AsPrimaryKey();
            Table.AddColumn("visit_count", "INTEGER").DefaultValue(0).NotNull();
            Options.TeardownDataOnRebuild = true;

            // 2. Pass the natural key selector as the second parameter (primaryKeySource)
            Project<FirearmUsedAtRange>(
                                        map =>
                                        {
                                            map.Map(x => x.RangeName, "range_name");
                                            map.Increment("visit_count"); // Or map.Increment(x => 1, "visit_count");
                                        },
                                        x => x.RangeName); // <-- Natural Key / Primary Key Source

            Project<SimpleRangeSessionCreatedFromGui>(map =>
                                                      {
                                                          map.Map(x => x.RangeName, "range_name");
                                                          map.Increment("visit_count");
                                                      });

            Project<SimpleRangeSessionCreatedFromGui>(map =>
                                                      {
                                                          map.Map(x => x.RangeName, "range_name");
                                                          map.Increment("visit_count");
                                                      });
        }
    }
}