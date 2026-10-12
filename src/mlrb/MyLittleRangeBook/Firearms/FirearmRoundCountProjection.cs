using Fisher.Attributes;
using Fisher.Projections.Flattened;

namespace MyLittleRangeBook.Firearms
{
    /// <summary>
    ///     Represents the count of rounds for a specific firearm.
    /// </summary>
    public class FirearmRoundCount
    {
        [UniqueIndex] public string Name       { get; set; }
        public               int    RoundCount { get; set; }
    }

    public class FirearmRoundCountProjection : FlatTableProjection
    {
        public FirearmRoundCountProjection() : base("firearm_round_counts")
        {
            Table.AddColumn("firearm_name", "TEXT").NotNull().AddIndex().AsPrimaryKey();
            Table.AddColumn("round_count",  "INTEGER").DefaultValue(0).NotNull();
            Options.TeardownDataOnRebuild = true;

            /*
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
                                              */

            /*Project<SimpleRangeSessionCreatedFromGui>(map =>
                                                      {
                                                          map.Map(x => x.FirearmName, "firearm_name");
                                                          map.Increment(x => x.RoundsFired, "round_count");
                                                      });*/
            Project<SimpleRangeSessionCreatedFromCli>(map =>
                                                      {
                                                          map.Map(x => x.FirearmName, "firearm_name");
                                                          map.Increment(x => x.RoundsFired, "round_count");
                                                      },
                                                      x => x.FirearmName);
            Delete<FirearmDeactivated>(e => e.FirearmName);
        }
    }
}