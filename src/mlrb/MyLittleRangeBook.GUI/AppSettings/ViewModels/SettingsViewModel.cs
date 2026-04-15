using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dapper;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.GUI.Database;
using MyLittleRangeBook.GUI.Helper;
using MyLittleRangeBook.GUI.Messages;
using MyLittleRangeBook.GUI.Properties;
using MyLittleRangeBook.Models;
using SharedControls.Controls;
using SharedControls.Services;
using JsonContextHelper = MyLittleRangeBook.GUI.Helper.JsonContextHelper;
using WeakReferenceMessenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace MyLittleRangeBook.GUI.ViewModels
{
    /// <summary>
    ///     ViewModel responsible for managing application settings and data operations.
    ///     Handles theme switching, data import/export, and database management.
    /// </summary>
    public partial class SettingsViewModel : ViewModelBase, IDialogParticipant
    {
        readonly ISqliteHelper _sqliteHelper;

        public SettingsViewModel(ISqliteHelper sqliteHelper)
        {
            _sqliteHelper = sqliteHelper;
        }

        /// <summary>
        ///     Gets the application settings instance for binding to UI controls.
        /// </summary>
        public Settings Settings => Settings.Default;

        /// <summary>
        ///     Array of available theme variants that users can select from.
        ///     Includes Default, Dark, and Light theme options.
        /// </summary>
        [UsedImplicitly]
        public string[] AvailableThemeVariants { get; } =
        [
            ThemeVariant.Default.ToString(),
            ThemeVariant.Dark.ToString(),
            ThemeVariant.Light.ToString()
        ];

        /// <summary>
        ///     A command that will export the entire database to JSON.
        /// </summary>
        [RelayCommand]
        async Task ExportDataAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
            /*try
            {
                // Show the file save dialog for the user to choose an export location
                SaveFilePickerResult? safeFilePickerResult = await this.SafeFileDialogAsync("Export Data",
                    [FileHelper.JsonFileType]);

                if (safeFilePickerResult?.File is { } storageFile)
                {
                    await using Stream fs = await storageFile.OpenWriteAsync();

                    try
                    {
                        SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);
                        await DatabaseHelper.ExportToJsonAsync(conn, fs, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        // Show an error dialog if export fails
                        await this.ShowOverlayDialogAsync<DialogResult>("Error",
                            "An error occured during exporting data. " + e.Message,
                            DialogCommands.OkOnly);
                    }

                    // Show a success dialog with the filename
                    await this.ShowOverlayDialogAsync<DialogResult>("Exported Data",
                        $"Successfully exported data to '{storageFile.Name}'.",
                        DialogCommands.Ok);
                }
            }
            catch (Exception e)
            {
                await this.ShowOverlayDialogAsync<DialogResult>("Could not export the data",
                    e.Message, DialogCommands.OkOnly);
            }*/
        }

        /// <summary>
        ///     A command that will import the selected JSON file into the current database.
        ///     Existing items will be updated.
        /// </summary>
        [RelayCommand]
        async Task ImportDataAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
            /*try
            {
                // NOTE: Existing items will be updated / overridden. You may want to let the user choose
                // how to handle it.

                // Show file open dialog for user to select JSON import file
                IEnumerable<IStorageFile>? openFilePickerResult = await this.OpenFileDialogAsync("Import Data",
                    [FileHelper.JsonFileType]);

                if (openFilePickerResult?.FirstOrDefault() is { } storageFile)
                {
                    // Open a file stream for reading
                    await using Stream fs = await storageFile.OpenReadAsync();

                    // Deserialize JSON into database DTO structure
                    DatabaseDto? dto = await JsonSerializer.DeserializeAsync<DatabaseDto>(fs,
                        JsonContextHelper.Default.DatabaseDto, cancellationToken);

                    if (dto is null)
                    {
                        throw new FileLoadException("Could not load data");
                    }

                    await using SqliteConnection connection =
                        await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);
                    // Save all ToDoItems from imported data (updates existing ones)
                    foreach (SimpleRangeEvent rangeEvent in dto.SimpleRangeEvents ?? [])
                    {
                        await rangeEvent.SaveAsync(connection, cancellationToken);
                    }

                    // Notify other ViewModels about updated DB to refresh their views
                    WeakReferenceMessenger.Default.Send(new UpdateDataMessage<SimpleRangeEvent>(UpdateAction.Reset));
                }
            }
            catch (Exception e)
            {
                await this.ShowOverlayDialogAsync<DialogResult>("Error importing JSON file",
                    e.Message, DialogCommands.OkOnly);
            }*/
        }

        /// <summary>
        ///     A command that clears all data from the database.
        ///     Shows a confirmation dialog first, then drops, and recreates all tables.
        ///     Notifies other ViewModels to refresh their data after completion.
        /// </summary>
        [RelayCommand]
        async Task ClearDatabaseAsync(CancellationToken cancellationToken = default)
        {
            // Show a confirmation dialog with a warning about data loss
            DialogResult choice = await this.ShowOverlayDialogAsync<DialogResult>("Clear Database",
                """
                Are you sure you want to clear the database? This cannot be undone.
                TIP: Consider to export the data before you continue.

                Press "Yes" to continue.
                """,
                DialogCommands.YesNo);

            if (choice == DialogResult.Yes)
            {
                // Get database connection and clear all data
                await using SqliteConnection connection =
                    await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);

                // Drop existing tables and vacuum the database to reclaim space
                await connection.ExecuteAsync(
                    """
                    TRUNCATE SimpleRangeEvents;
                    TRUNCATE TABLE Firearms;
                    TRUNCATE TABLE FitFiles;
                    VACUUM;
                    """);


                // Notify other ViewModels about updated DB to refresh their views
                WeakReferenceMessenger.Default.Send(new UpdateDataMessage<SimpleRangeEvent>(UpdateAction.Reset));
            }
        }
    }
}
