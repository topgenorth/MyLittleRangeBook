using System.Diagnostics.CodeAnalysis;
using MyLittleRangeBook.GUI.ViewModels;
using SharedControls.Services;
using ListSimpleRangeSessionsViewModel = MyLittleRangeBook.GUI.listSimpleRangeSession.ListSimpleRangeSessionsViewModel;

namespace MyLittleRangeBook.GUI
{
    /// <summary>
    ///     Main ViewModel that orchestrates the entire application's ViewModels.
    ///     Acts as the root container for all major application sections and coordinates navigation
    ///     between SimpleLogEvents and Settings. Implements IDialogParticipant to support
    ///     dialog interactions throughout the application.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class MainViewModel(
        ListSimpleRangeSessionsViewModel listSimpleRangeSessionsVm,
        ManageFirearmsViewModel manageFirearmsVm,
        SettingsViewModel settingsVm)
        : ViewModelBase, IDialogParticipant
    {
        /// <summary>
        ///     The ViewModel that manages the SimpleRangeEvents and CRUD operations.
        /// </summary>
        public ListSimpleRangeSessionsViewModel ListSimpleRangeSessionsVm { get; set; } = listSimpleRangeSessionsVm;

        public ManageFirearmsViewModel ManageFirearmsVM { get; set; } = manageFirearmsVm;

        /// <summary>
        ///     The ViewModel that manages application settings and configuration.
        /// </summary>
        public SettingsViewModel SettingsVM { get; set; } = settingsVm;
    }
}
