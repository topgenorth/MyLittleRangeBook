using Fisher;
using Fisher.Projections;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Models;
using Weasel.Core.Identity;
using Weasel.Core.Sequences;

namespace MyLittleRangeBook.Recipes
{

    public partial class AmmoDescriptionProjection2 : EventProjection
    {
        const string UPSERT_FIREARM_AMMO_DESCRIPTIONS_SQL = """
                                                            INSERT INTO firearm_ammo_descriptions (id, firearm_name, ammo_description)
                                                            VALUES (?, ?, ?)
                                                            ON CONFLICT(id) DO UPDATE SET
                                                                firearm_name = EXCLUDED.firearm_name,
                                                                ammo_description = EXCLUDED.ammo_description;
                                                            """;

        AmmoDescriptionSentence Create(FirearmUsedAmmo evt)
        {
            string ammo = evt.AmmoDescription ?? "Unknown";
            return new AmmoDescriptionSentence(CreateAmmoDescriptionId(evt.FirearmName, ammo),
                                               ammo,
                                               evt.CorrelationId,
                                               evt.CausationId
                                              );
        }
        AmmoDescriptionSentence Create(FirearmUsedAtRange evt)
        {
            string ammo = evt.AmmoDescription ?? "Unknown";
            return new AmmoDescriptionSentence(CreateAmmoDescriptionId(evt.FirearmName, ammo),
                                               ammo,
                                               evt.CorrelationId,
                                               evt.CausationId
                                              );
        }

        /// <summary>
        ///     Creates a unique ID for an ammo description based on its name and the firearm it's associated with.
        /// </summary>
        /// <remarks>All of these are assumed to be created at DateTimeOffset.MinValue.</remarks>
        /// <param name="firearmName"></param>
        /// <param name="ammoDescription"></param>
        /// <returns></returns>
        string CreateAmmoDescriptionId(string firearmName, string ammoDescription)
        {
            string s  = $"{firearmName}->{ammoDescription}";
            string id = MlrbId.FromString(s, DateTimeOffset.MinValue);
            return id;
        }

        public void Project(FirearmUsedAtRange @event, IDocumentSession ops)
        {
            AmmoDescriptionSentence x = Create(@event);
            ops.QueueSqlCommand(UPSERT_FIREARM_AMMO_DESCRIPTIONS_SQL,
                                CreateAmmoDescriptionId(@event.FirearmName, x.AmmoDescription),
                                @event.FirearmName,
                                x.AmmoDescription);
            ops.Store(x);
        }

        public void Project(FirearmUsedAmmo @event, IDocumentSession ops)
        {
            AmmoDescriptionSentence x = Create(@event);
            ops.QueueSqlCommand(UPSERT_FIREARM_AMMO_DESCRIPTIONS_SQL,
                                CreateAmmoDescriptionId(@event.FirearmName, x.AmmoDescription),
                                @event.FirearmName,
                                x.AmmoDescription);
            ops.Store(x);
        }

    }



    public class AmmoDescriptionIdGenerator: IIdentification<AmmoDescriptionSentence, string>
    {
        public string Identity(AmmoDescriptionSentence document)
        {
            return DeterministicUlid.MlrbIdFromString(document.Id);
        }

        public string AssignIfMissing(AmmoDescriptionSentence document, ISequenceSource sequences)
        {
            if (string.IsNullOrWhiteSpace(document.Id))
            {
                return DeterministicUlid.MlrbIdFromString(document.AmmoDescription);
            }

            return document.Id;
        }
    }

    /// <summary>
    ///     This document is a "summary" of all the ammo descriptions - a free-text format of a reloading recipe.
    /// </summary>
    /// <param name="FirearmName"></param>
    /// <param name="AmmoDescription"></param>
    public readonly record struct AmmoDescriptionSentence(
        string Id,
        string AmmoDescription,
        Guid   CorrelationId,
        Guid   CausationId);
}