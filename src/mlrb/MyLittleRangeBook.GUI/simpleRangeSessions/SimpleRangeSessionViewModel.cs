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
        readonly Guid     _causationId;
        readonly Guid     _correlationId;
        readonly string   _originalAmmoDescription;
        readonly DateTime _originalEventDate;
        readonly string   _originalFirearm;
        readonly string   _originalNotes;
        readonly string   _originalRange;
        readonly int      _originalRoundCount;

        /// <summary>
        ///     Use this constructor when creating a new simple range event.
        /// </summary>
        public SimpleRangeSessionViewModel()
        {
            Guid id = Guid.CreateVersion7();
            _causationId             = id;
            _correlationId           = id;
            _originalRoundCount      = 0;
            _originalNotes           = string.Empty;
            _originalAmmoDescription = string.Empty;
            _originalRange           = string.Empty;
            _originalFirearm         = string.Empty;
            _originalEventDate       = DateTime.Now;

            Id              = id;
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
        public SimpleRangeSessionViewModel(SimpleRangeSession rangeSession,
                                           Guid?              correlationId = null,
                                           Guid?              causationId   = null)
        {
            _correlationId           = correlationId ?? rangeSession.Id;
            _causationId             = causationId   ?? rangeSession.Id;
            _originalRoundCount      = rangeSession.RoundsFired;
            _originalNotes           = rangeSession.Notes           ?? string.Empty;
            _originalAmmoDescription = rangeSession.AmmoDescription ?? string.Empty;
            _originalRange           = rangeSession.RangeName       ?? string.Empty;
            _originalFirearm         = rangeSession.FirearmName     ?? string.Empty;
            _originalEventDate       = rangeSession.EventDate;

            Id              = rangeSession.Id;
            EventDate       = _originalEventDate;
            FirearmName     = _originalFirearm;
            RangeName       = _originalRange;
            RoundsFired     = _originalRoundCount;
            AmmoDescription = _originalAmmoDescription;
            Notes           = _originalNotes;
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

        public List<object> GetEvents()
        {
            List<object> events = [];
            if (IsNew)
            {
                SimpleRangeSessionCreatedFromGui e = new SimpleRangeSessionCreatedFromGui(
                 EventDate,
                    Id,
                    _correlationId,
                    _causationId,
                    DateTimeOffset.UtcNow,
                    FirearmName,
                    RangeName,
                    RoundsFired,
                    AmmoDescription,
                    Notes,
                    DateTimeOffset.UtcNow,
                    DateTimeOffset.UtcNow);
                events.Add(e);
            }

            // TODO [TO20261005]
            return events;
        }
    }
}