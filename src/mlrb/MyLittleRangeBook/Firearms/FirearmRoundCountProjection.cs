using Fisher.Projections.Flattened;
using MyLittleRangeBook.EventSourcing;

namespace MyLittleRangeBook.Firearms
{
    public class FirearmRoundCountProjection : FlatTableProjection
    {
        public FirearmRoundCountProjection() : base("firearm_round_counts")
        {
            Table.AddColumn("id",           "TEXT").AsPrimaryKey();
            Table.AddColumn("firearm_name", "TEXT").AddIndex(c => { c.IsUnique = true; }).NotNull();
            Table.AddColumn("round_count",  "INTEGER").DefaultValue(0).NotNull();


            Project<FirearmCreated>(map =>
                                    {
                                        map.Map(x => x.Name, "firearm_name");
                                        map.SetValue("round_count", 0);
                                    });

            // Project<FirearmUsedAtRange>(map =>
            //                             {
            //                                 map.Map(x => x.FirearmName, "firearm_name");
            //                                 map.Increment(x => x.RoundsFired, "round_count");
            //                             });
            Project<FirearmRoundCountAltered>(map =>
                                              {
                                                  map.Map(x => x.FirearmName, "firearm_name");
                                                  map.Increment(x => x.RoundsDelta, "round_count");
                                              });
        }
    }
}