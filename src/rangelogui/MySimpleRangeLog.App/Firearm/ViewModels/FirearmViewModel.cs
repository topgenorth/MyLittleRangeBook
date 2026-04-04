using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using MyLittleRangeBook.Gui.Models;

namespace MyLittleRangeBook.Gui.ViewModels
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    [UnconditionalSuppressMessage("Trimming", "IL2112",
        Justification = "We have all needed members added via DynamicallyAccessedMembers-Attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "We have all needed members added via DynamicallyAccessedMembers-Attribute")]
    public partial class FirearmViewModel : ViewModelBase, ICloneable
    {
        public FirearmViewModel(Firearm firearm)
        {
            Id = firearm.RowId;
            Name = firearm.Name;
            Modified = firearm.Modified;
            Created = firearm.Created;
        }

        [ObservableProperty] [Required] public partial long? Id { get; private set; }


        [ObservableProperty] [Required] [NotifyDataErrorInfo] public partial string Name { get; set; }
        [ObservableProperty] [Required] [NotifyDataErrorInfo] public partial DateTimeOffset Modified { get; set; }
        [ObservableProperty] [Required] [NotifyDataErrorInfo] public partial DateTimeOffset Created { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public Firearm ToFirearm()
        {
            return new Firearm { RowId = Id, Modified = Modified, Created = Created, Name = Name };
        }

        public FirearmViewModel CloneFirearmViewModel()
        {
            return (FirearmViewModel)Clone();
        }
    }
}
