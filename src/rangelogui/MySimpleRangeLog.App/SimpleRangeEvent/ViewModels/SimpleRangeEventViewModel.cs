using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using MySimpleRangeLog.Models;

namespace MySimpleRangeLog.ViewModels
{
    /// <summary>
    ///     ViewModel representing a single ManageSimpleRangeEventsVM with comprehensive property management and validation.
    ///     Provides observable properties for UI binding, calculated status properties, and
    ///     persistence operations for ToDoItems in the application.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    [UnconditionalSuppressMessage("Trimming", "IL2112",
        Justification = "We have all needed members added via DynamicallyAccessedMembers-Attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "We have all needed members added via DynamicallyAccessedMembers-Attribute")]
    public partial class SimpleRangeEventViewModel : ViewModelBase, ICloneable
    {
        public SimpleRangeEventViewModel(SimpleRangeEvent rangeEvent)
        {
            Id = rangeEvent.RowId;
            EventDate = rangeEvent.EventDate;
            FirearmName = rangeEvent.FirearmName;
            RangeName = rangeEvent.RangeName;
            RoundsFired = rangeEvent.RoundsFired;
            AmmoDescription = rangeEvent.AmmoDescription ?? string.Empty;
            Notes = rangeEvent.Notes ?? string.Empty;
            Modified = rangeEvent.Modified;
            Created = rangeEvent.Created;
        }

        [ObservableProperty] public partial long? Id { get; private set; }

        [ObservableProperty] [Required] [NotifyDataErrorInfo] public partial DateTime EventDate { get; set; }

        [ObservableProperty] [Required] [NotifyDataErrorInfo] public partial string FirearmName { get; set; }

        [ObservableProperty] [Required] [NotifyDataErrorInfo] public partial string RangeName { get; set; }


        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(0, 10000)]
        [Required]
        public partial int RoundsFired { get; set; }

        [ObservableProperty] public partial string AmmoDescription { get; set; }
        [ObservableProperty] public partial string Notes { get; set; }

        [ObservableProperty] [Required] [NotifyDataErrorInfo] public partial DateTimeOffset Modified { get; set; }

        [ObservableProperty] [Required] [NotifyDataErrorInfo] public partial DateTimeOffset Created { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        [UsedImplicitly]
        public SimpleRangeEvent ToSimpleRangeEvent()
        {
            return new SimpleRangeEvent
            {
                RowId = Id,
                EventDate = EventDate,
                FirearmName = FirearmName,
                RangeName = RangeName,
                RoundsFired = RoundsFired,
                AmmoDescription = AmmoDescription,
                Notes = Notes,
                Modified = Modified,
                Created = Created
            };
        }

        [UsedImplicitly]
        public SimpleRangeEventViewModel CloneSimpleRangeEventViewModel()
        {
            return (SimpleRangeEventViewModel)Clone();
        }
    }
}
