using System.Diagnostics.CodeAnalysis;
using SharedControls.Services;

namespace MyLittleRangeBook.Gui.ViewModels
{
    /// <summary>
    ///     Main ViewModel that orchestrates the entire application's ViewModels.
    ///     Acts as the root container for all major application sections and coordinates navigation
    ///     between SimpleLogEvents and Settings. Implements IDialogParticipant to support
    ///     dialog interactions throughout the application.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class MainViewModel : ViewModelBase, IDialogParticipant
    {
        public MainViewModel(ManageSimpleRangeEventsViewModel manageSimpleRangeEventsVM,
            ManageFirearmsViewModel manageFirearmsVM,
            SettingsViewModel settingsVM)
        {
            ManageSimpleRangeEventsVM = manageSimpleRangeEventsVM;
            ManageFirearmsVM = manageFirearmsVM;
            SettingsVM = settingsVM;
        }

        /// <summary>
        ///     The ViewModel that manages the SimpleRangeEvents and CRUD operations.
        /// </summary>
        public ManageSimpleRangeEventsViewModel ManageSimpleRangeEventsVM { get; set; }

        public ManageFirearmsViewModel ManageFirearmsVM { get; set; }

        /// <summary>
        ///     The ViewModel that manages application settings and configuration.
        /// </summary>
        public SettingsViewModel SettingsVM { get; set; }
    }
}
