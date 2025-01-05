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

    Target Install => _ => _
        .DependsOn(Publish)
        .Executes(() =>
        {
            if (!IsLinux())
            {
                Log.Warning("Install is only supported on Linux.");

                return;
            }

            var publishedApp = PublishDirectory / LINUX_APP_NAME;
            var installDir = UsersHomeDir / INSTALL_SUBDIRECTORY;
            var installedApp = installDir / LINUX_APP_NAME;
            AbsolutePath localBinDir = Path.Combine(UsersHomeDir, ".local", "bin");
            var symLink = Path.Combine(localBinDir, LINUX_APP_NAME);

            if (File.Exists(symLink))
            {
                File.Delete(symLink);
                Log.Information("Deleting symlink {symLink}", symLink);
            }

            #region Copy the app to ~/.mlrb
            installDir.CreateDirectory();
            publishedApp.CopyToDirectory(installDir, ExistsPolicy.FileOverwrite, createDirectories: true);
            Log.Debug("Copied the app from {publishedApp} to {installDir}.", publishedApp, installDir);
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
        });

    bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
}
