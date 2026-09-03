using Fisher.Projections.Flattened;
using MyLittleRangeBook.EventSourcing;

namespace MyLittleRangeBook.Firearms
{
    public class FirearmRoundCountProjection : FlatTableProjection
    {
        public FirearmRoundCountProjection() : base("firearm_round_counts")
        {
            Table.AddColumn("firearm_name", "TEXT").AsPrimaryKey();
            Table.AddColumn("round_count",  "INTEGER").DefaultValue(0).NotNull();


            Project<FirearmCreated>(map =>
                                    {
                                        map.Map(x => x.FirearmName, "firearm_name");
                                        map.SetValue("round_count", 0);
                                    },
                                    x => x.FirearmName);

            Project<FirearmUsedAtRange>(map =>
                                        {
                                            map.Map(x => x.FirearmName, "firearm_name");
                                            map.Increment(x => x.RoundsFired, "round_count");
                                        },
                                        x => x.FirearmName);
            Project<FirearmRoundCountAltered>(map =>
                                              {
                                                  map.Map(x => x.FirearmName, "firearm_name");
                                                  map.Increment(x => x.RoundsDelta, "round_count");
                                              },
                                              x => x.FirearmName);

            Delete<FirearmDeactivated>(e => e.FirearmName);
        }
    }
}