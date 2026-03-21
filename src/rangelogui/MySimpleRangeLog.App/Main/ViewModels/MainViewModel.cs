using MySimpleRangeLog.ViewModels;
using SharedControls.Services;
using ManageSimpleRangeEventsViewModel = MySimpleRangeLog.ViewModels.ManageSimpleRangeEventsViewModel;
using SettingsViewModel = MySimpleRangeLog.ViewModels.SettingsViewModel;

namespace MySimpleRangeLog.Main.ViewModels
{
    /// <summary>
    ///     Main ViewModel that orchestrates the entire application's ViewModels.
    ///     Acts as the root container for all major application sections and coordinates navigation
    ///     between SimpleLogEvents and Settings. Implements IDialogParticipant to support
    ///     dialog interactions throughout the application.
    /// </summary>
    public class MainViewModel : ViewModelBase, IDialogParticipant
    {
        /// <summary>
        ///     The ViewModel that manages the SimpleRangeEvents and CRUD operations.
        /// </summary>
        public ManageSimpleRangeEventsViewModel ManageSimpleRangeEventsVM { get; set; } = new();

        public ManageFirearmsViewModel ManageFirearmsVM { get; set; } = new();

        /// <summary>
        ///     The ViewModel that manages application settings and configuration.
        /// </summary>
        public SettingsViewModel SettingsVM { get; set; } = new();
    }
}
