using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using MyLittleRangeBook.RangeEvents;

namespace MyLittleRangeBook.GUI
{
    /// <summary>
    ///     ViewModel representing a single ManageSimpleRangeEventsVM with comprehensive property management and validation.
    ///     Provides observable properties for UI binding, calculated status properties, and
    ///     persistence operations for ToDoItems in the application.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    [UnconditionalSuppressMessage("Trimming", "IL2112",
                                  Justification =
                                      "We have all needed members added via DynamicallyAccessedMembers-Attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2026",
                                  Justification =
                                      "We have all needed members added via DynamicallyAccessedMembers-Attribute")]
    public partial class SimpleRangeSessionViewModel : ViewModelBase, ICloneable
    {
        readonly Guid         _causationId;
        readonly Guid         _correlationId;

        /// <summary>
        ///     Use this constructor when creating a new simple range event.
        /// </summary>
        public SimpleRangeSessionViewModel()
        {
            Guid streamId = Guid.CreateVersion7();
            _causationId   = streamId;
            _correlationId = streamId;

            Id              = streamId;
            Modified        = DateTimeOffset.UtcNow;
            Created         = DateTimeOffset.UtcNow;
            Notes           = string.Empty;
            AmmoDescription = string.Empty;
            RangeName       = string.Empty;
            FirearmName     = string.Empty;
            EventDate       = DateTime.Now;
            IsNew           = true;
        }

        /// <summary>
        ///     Use this constructor when editing a simple range event.
        /// </summary>
        /// <param name="rangeSession"></param>
        /// <param name="correlationId">Defaults to the ID of the simple range session.</param>
        /// <param name="causationId">Defaults to the ID of the simple range session.</param>
        public SimpleRangeSessionViewModel(SimpleRangeSession rangeSession, Guid? correlationId = null, Guid? causationId = null)
        {
            _correlationId = correlationId ?? rangeSession.Id;
            _causationId   = causationId   ?? rangeSession.Id;

            Id              = rangeSession.Id;
            EventDate       = rangeSession.EventDate;
            FirearmName     = rangeSession.FirearmName;
            RangeName       = rangeSession.RangeName;
            RoundsFired     = rangeSession.RoundsFired;
            AmmoDescription = rangeSession.AmmoDescription ?? "N/A";
            Notes           = rangeSession.Notes           ?? string.Empty;
            Modified        = rangeSession.Modified;
            Created         = rangeSession.Created;
            IsNew           = false;
        }

        public                              bool IsNew { get; private set; }
        [ObservableProperty] public partial Guid Id    { get; private set; }

        /// <summary>
        ///     Gets or sets the date and time when the range event occurred. Always in the local time zone.
        /// </summary>
        [ObservableProperty]
        [Required]
        [NotifyDataErrorInfo]
        public partial DateTime EventDate { get; set; }

        [ObservableProperty]
        [Required]
        [NotifyDataErrorInfo]
        public partial string FirearmName { get; set; }

        [ObservableProperty]
        [Required]
        [NotifyDataErrorInfo]
        public partial string RangeName { get; set; }

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(-10000, 10000)]
        [Required]
        public partial int RoundsFired { get; set; }

        [ObservableProperty] public partial string AmmoDescription { get; set; }

        [ObservableProperty] public partial string Notes { get; set; }

        [ObservableProperty]
        [Required]
        [NotifyDataErrorInfo]
        public partial DateTimeOffset Modified { get; set; }

        [ObservableProperty]
        [Required]
        [NotifyDataErrorInfo]
        public partial DateTimeOffset Created { get; set; }

        public object Clone() => MemberwiseClone();

        public SimpleRangeSession ToSimpleRangeSession() =>
            new()
            {
                Id              = Id,
                CorrelationId   = _correlationId,
                CausationId     = _causationId,
                EventDate       = EventDate,
                FirearmName     = FirearmName.Trim(),
                RangeName       = RangeName.Trim(),
                RoundsFired     = RoundsFired,
                AmmoDescription = AmmoDescription.Trim(),
                Notes           = Notes.Trim(),
                Modified        = Modified,
                Created         = Created,
            };
    }
}