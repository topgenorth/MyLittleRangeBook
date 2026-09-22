using Fisher.Projections.Flattened;
using MyLittleRangeBook.Firearms;
using Weasel.Sqlite.Tables;

namespace MyLittleRangeBook.Recipes
{
    public class AmmoDescriptionProjection : FlatTableProjection
    {
        public AmmoDescriptionProjection() : base("ammo_descriptions")
        {
            Table.AddColumn("id",               "TEXT").AsPrimaryKey();
            Table.AddColumn("firearm_name",     "TEXT").NotNull().AddIndex();
            Table.AddColumn("ammo_description", "TEXT").NotNull();
            IndexDefinition id = new IndexDefinition("idx_firearm_ammo_descriptions")
               .AgainstColumns("firearm_name", "ammo_description");
            id.IsUnique = true;
            Table.Indexes.Add(id);
            Options.TeardownDataOnRebuild = true;
            Project<FirearmUsedAtRange>(map =>
                                        {
                                            map.Map(x => x.FirearmName,     "firearm_name");
                                            map.Map(x => x.AmmoDescription, "ammo_description");
                                        });
        }

    }
    /// <summary>
    ///     This is a "summary" of all the ammo descriptions that a user entered for a firearm.
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="FirearmName"></param>
    /// <param name="AmmoDescription"></param>
    public record struct AmmoDescriptionSentence(string FirearmName, string AmmoDescription);
}