using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using MyLittleRangeBook.RangeEvents;

namespace MyLittleRangeBook.GUI.ViewModels
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
    public partial class SimpleRangeEventViewModel : ViewModelBase, ICloneable
    {
        bool _isNew = true;

        /// <summary>
        /// Use this constructor when creating a new simple range event.
        /// </summary>
        public SimpleRangeEventViewModel()
        {
            Id              = Guid.CreateVersion7();
            Modified        = DateTimeOffset.UtcNow;
            Created         = DateTimeOffset.UtcNow;
            Notes           = string.Empty;
            AmmoDescription = string.Empty;
            RangeName       = string.Empty;
            FirearmName     = string.Empty;
            EventDate       = DateTime.Now;
        }

        /// <summary>
        /// Use this constructor when editing a simple range event.
        /// </summary>
        /// <param name="rangeEvent"></param>
        public SimpleRangeEventViewModel(SimpleRangeEvent rangeEvent)
        {
            Id              = rangeEvent.Id;
            EventDate       = rangeEvent.EventDate;
            FirearmName     = rangeEvent.FirearmName;
            RangeName       = rangeEvent.RangeName;
            RoundsFired     = rangeEvent.RoundsFired;
            AmmoDescription = rangeEvent.AmmoDescription ?? "N/A";
            Notes           = rangeEvent.Notes           ?? string.Empty;
            Modified        = rangeEvent.Modified;
            Created         = rangeEvent.Created;
            _isNew          = false;
        }

        [ObservableProperty] public partial Guid Id { get; private set; }

        /// <summary>
        /// Gets or sets the date and time when the range event occurred. Always in the local time zone.
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

        [UsedImplicitly]
        public SimpleRangeEvent ToSimpleRangeEvent() =>
            new()
            {
                Id              = Id,
                EventDate       = EventDate,
                FirearmName     = FirearmName?.Trim() ?? string.Empty,
                RangeName       = RangeName,
                RoundsFired     = RoundsFired,
                AmmoDescription = AmmoDescription,
                Notes           = Notes,
                Modified        = Modified,
                Created         = Created,
            };

        public SimpleRangeEventCreatedFromGui ToEventCreatedFromGui() =>
            new(EventDate, FirearmName!, Id, Id, Id, RangeName, RoundsFired, AmmoDescription, Notes, Modified);

        [UsedImplicitly]
        public SimpleRangeEventViewModel CloneSimpleRangeEventViewModel() => (SimpleRangeEventViewModel)Clone();
    }
}