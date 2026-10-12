using System.Collections.Generic;
using MyLittleRangeBook.RangeEvents;

namespace MyLittleRangeBook.GUI
{
    public class SimpleRangeSessionMerger
    {
        public List<object> GetDomainEvents(SimpleRangeSessionViewModel vm, SimpleRangeSession? doc)
        {
            if (doc is null)
            {
                (SimpleRangeSession newDoc, List<object> events) = NewSimpleRangeSessionDoc(vm);
            }

            // Implementation for merging domain events
            return new List<object>(0);
        }

        public (SimpleRangeSession doc, List<object>) NewSimpleRangeSessionDoc(SimpleRangeSessionViewModel vm)
        {
            SimpleRangeSession doc = new()
                                     {
                                         Id              = vm.Id,
                                         CausationId     = vm.Id,
                                         CorrelationId   = vm.Id,
                                         EventDate       = vm.EventDate,
                                         FirearmName     = vm.FirearmName.Trim(),
                                         RangeName       = vm.RangeName.Trim(),
                                         RoundsFired     = vm.RoundsFired,
                                         AmmoDescription = vm.AmmoDescription.Trim(),
                                         Notes           = vm.Notes.Trim(),
                                         Modified        = vm.Modified,
                                         Created         = vm.Created,
                                     };

            List<object> events =
                [new SimpleRangeSessionCreatedFromGui(doc.EventDate,
                                                      doc.Id,
                                                      doc.CorrelationId,
                                                      doc.CausationId,
                                                      DateTimeOffset.UtcNow,
                                                      doc.FirearmName.Trim(),
                                                      doc.RangeName.Trim(),
                                                      doc.RoundsFired,
                                                      doc.AmmoDescription.Trim(),
                                                      doc.Notes.Trim(),
                                                      doc.Modified,
                                                      doc.Created
                                                      )];

            if (string.IsNullOrWhiteSpace(vm.RangeName))
            {
                if (vm.RoundsFired != 0)
                {
                    var x = new FirearmRoundCountAltered(doc.FirearmName,
                                                         doc.CorrelationId,
                                                         doc.CausationId,
                                                         doc.RoundsFired,
                                                         doc.OccurredUtc,
                                                         doc.AmmoDescription ?? string.Empty);
                    events.Add(x);
                }
                if (!string.IsNullOrWhiteSpace(vm.Notes))
                {
                    var x = new FirearmNoteAdded(doc.FirearmName,
                                                 doc.CorrelationId,
                                                 doc.CausationId,
                                                 vm.Notes.Trim(),
                                                 doc.OccurredUtc);
                    events.Add(x);
                }
            }
            else
            {
                events.Add(new FirearmUsedAtRange(doc.FirearmName,
                                                  doc.CorrelationId,
                                                  doc.CausationId,
                                                  doc.RangeName,
                                                  doc.RoundsFired,
                                                  doc.AmmoDescription,
                                                  DateTimeOffset.UtcNow));
            }

            return (doc, events);
        }
    }
}