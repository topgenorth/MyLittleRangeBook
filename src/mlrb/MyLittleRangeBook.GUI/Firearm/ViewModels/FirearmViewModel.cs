using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.GUI.ViewModels
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    [UnconditionalSuppressMessage("Trimming", "IL2112",
        Justification = "We have all needed members added via DynamicallyAccessedMembers-Attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "We have all needed members added via DynamicallyAccessedMembers-Attribute")]
    public partial class FirearmViewModel : ViewModelBase, ICloneable
    {
        public FirearmViewModel(FirearmTableRow firearmTableRow)
        {
            Name = firearmTableRow.Name;
            RoundsFired = firearmTableRow.RoundsFired;
            Notes = firearmTableRow.Notes;
            Modified = firearmTableRow.Modified;
            Created = firearmTableRow.Created;
        }

        [ObservableProperty] [Required] public partial long? Id { get; private set; }


        [ObservableProperty] [Required] [NotifyDataErrorInfo] public partial string Name { get; set; }
        [ObservableProperty] [Required] [NotifyDataErrorInfo] public partial int RoundsFired { get; set; }
        [ObservableProperty] public partial string? Notes { get; set; }
        [ObservableProperty] [Required] [NotifyDataErrorInfo] public partial DateTimeOffset Modified { get; set; }
        [ObservableProperty] [Required] [NotifyDataErrorInfo] public partial DateTimeOffset Created { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public FirearmTableRow ToFirearm()
        {
            return new FirearmTableRow { Modified = Modified, Created = Created, Name = Name, RoundsFired = RoundsFired, Notes = Notes };
        }

        public FirearmViewModel CloneFirearmViewModel()
        {
            return (FirearmViewModel)Clone();
        }
    }
}
