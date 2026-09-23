using Fisher.Projections.Flattened;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Recipes
{
    /*/// <summary>
    ///     Create the AmmoDescription lookup table.
    /// </summary>
    public partial class AmmoDescriptionProjection : MultiStreamProjection<AmmoDescriptionSentence, string>
    {
        public AmmoDescriptionProjection()
        {
            Identity<FirearmUsedAtRange>(e => CreateId(e.FirearmName, e.AmmoDescription!));
            Identity<FirearmUsedAmmo>(e => CreateId(e.FirearmName,    e.AmmoDescription!));
        }

        /// <summary>
        ///     Creates a deterministic ID for the name/description.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        static string CreateId(string name, string description)
        {
            string id = $"{name.Trim().ToUpperInvariant()}|{description.Trim().ToUpperInvariant()}";
            return MlrbId.FromString(id);
        }

        /// <summary>
        ///     Change the event into an <c cref="AmmoDescriptionSentence" />.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static AmmoDescriptionSentence Create(FirearmUsedAtRange e) =>
            new(CreateId(e.FirearmName, e.AmmoDescription), e.FirearmName, e.AmmoDescription);

        /// <summary>
        ///     Applies the changes from the event to the current sentence.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="current"></param>
        public void Apply(FirearmUsedAtRange evt, AmmoDescriptionSentence current)
        {
            current.FirearmName     = evt.FirearmName;
            current.AmmoDescription = evt.AmmoDescription!;
        }

        public void Apply(FirearmUsedAmmo evt, AmmoDescriptionSentence current)
        {
            current.FirearmName = evt.FirearmName;
            current.AmmoDescription = evt.AmmoDescription!;
        }
    }*/


    public class AmmoDescriptionProjection : FlatTableProjection
    {
        public AmmoDescriptionProjection() : base("firearm_ammo_descriptions")
        {
            Table.AddColumn("id",               "TEXT").AsPrimaryKey();
            Table.AddColumn("firearm_name",     "TEXT").NotNull().AddIndex();
            Table.AddColumn("ammo_description", "TEXT").NotNull().AddIndex();

            Project<FirearmUsedAtRange>(map =>
                                        {
                                            map.Map(x => x.AmmoDescription, "ammo_description");
                                            map.Map(x => x.FirearmName,     "firearm_name");

                                        });
        }

        /// <summary>
        ///     Creates a deterministic ID for the name/description.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        static string CreateId(string name, string description)
        {
            string id = $"{name.Trim().ToUpperInvariant()}|{description.Trim().ToUpperInvariant()}";
            return MlrbId.FromString(id);
        }
    }

    /// <summary>
    ///     This is a "summary" of all the ammo descriptions that a user entered for a firearm.
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="FirearmName"></param>
    /// <param name="AmmoDescription"></param>
    public record struct AmmoDescriptionSentence(string Id, string FirearmName, string AmmoDescription);
}