using System;
using System.IO;
using System.Runtime.InteropServices;
using Nuke.Common;
using Nuke.Common.IO;
using Serilog;

public partial class Build
{
    const string LINUX_APP_NAME = "xeror";
    const string INSTALL_SUBDIRECTORY = ".mlrb";
    readonly AbsolutePath UsersHomeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    Target InstallLinux => _ => _
        .DependsOn(Publish)
        .Executes(() =>
        {
            if (Configuration != Configuration.Release)
            {
                Log.Warning("This is NOT a release build.");
            }

            if (!IsLinux())
            {
                Log.Error("InstallLinux is only supported on Linux.");

                return;
            }

            AbsolutePath localBinDir = Path.Combine(UsersHomeDir, ".local", "bin");

            var publishedApp = PublishDirectory / LINUX_APP_NAME;
            var installDir = UsersHomeDir / INSTALL_SUBDIRECTORY;
            var installedApp = installDir / LINUX_APP_NAME;
            var symLink = Path.Combine(localBinDir, LINUX_APP_NAME);


            #region Cleanup anything that might already be there
            if (File.Exists(symLink))
            {
                File.Delete(symLink);
                Log.Information("Deleting symlink {symLink}", symLink);
            }

            if (!installDir.DirectoryExists())
            {
                installDir.CreateDirectory();
            }

            if (installedApp.FileExists())
            {
                installedApp.DeleteFile();
                Log.Information("Deleted the existing file {appName}", installedApp);
            }
            #endregion

            #region Copy the app to ~/.mlrb
            publishedApp.CopyToDirectory(installDir, ExistsPolicy.FileOverwrite, createDirectories: true);
            Log.Debug("Copied the app from {publishedApp} to {installApp}.", publishedApp, installDir);
            #endregion

            #region Create ~/.local/bin if necessary
            if (!Directory.Exists(localBinDir))
            {
                localBinDir.CreateDirectory();
                Log.Information("Created {localBinDir} - make sure to add it to your path!", localBinDir);
            }
            #endregion

            #region SymLink the file to ~/.local/bin
            try
            {
                File.CreateSymbolicLink(symLink, installedApp);
                Log.Debug("Created symlink from {installedApp} to {symLink}.", installedApp, symLink);
            }
            catch (Exception e)
            {
                Log.Warning(e, "Could not symlink {installedApp} to {symLink}", installedApp, symLink);
            }
            #endregion

            #region Copy over appsettings.json if needed
            var appSettingsJson = installDir / "appsettings.json";
            if (!appSettingsJson.FileExists())
            {
                var appSettingsTemplate = XeroReaderCliProject / "appsettings.json";
                appSettingsTemplate.CopyToDirectory(installDir, ExistsPolicy.FileSkip);
                Log.Information("Copied {appsettings} to {installDir} - make sure you edit it.", appSettingsTemplate,
                    installDir);
            }
            #endregion
        });

    bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
}
