using System.Diagnostics.CodeAnalysis;
using SharedControls.Services;

namespace MyLittleRangeBook.GUI.ViewModels
{
    /// <summary>
    ///     Main ViewModel that orchestrates the entire application's ViewModels.
    ///     Acts as the root container for all major application sections and coordinates navigation
    ///     between SimpleLogEvents and Settings. Implements IDialogParticipant to support
    ///     dialog interactions throughout the application.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class MainViewModel(
        ManageSimpleRangeEventsViewModel manageSimpleRangeEventsVm,
        ManageFirearmsViewModel manageFirearmsVm,
        SettingsViewModel settingsVm)
        : ViewModelBase, IDialogParticipant
    {
        /// <summary>
        ///     The ViewModel that manages the SimpleRangeEvents and CRUD operations.
        /// </summary>
        public ManageSimpleRangeEventsViewModel ManageSimpleRangeEventsVM { get; set; } = manageSimpleRangeEventsVm;

        public ManageFirearmsViewModel ManageFirearmsVM { get; set; } = manageFirearmsVm;

        /// <summary>
        ///     The ViewModel that manages application settings and configuration.
        /// </summary>
        public SettingsViewModel SettingsVM { get; set; } = settingsVm;
    }
}
