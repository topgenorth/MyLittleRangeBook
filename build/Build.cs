using System;
using System.IO;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Serilog;

using static Nuke.Common.ChangeLog.ChangelogTasks;                                              // CHANGELOG
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;                                              // DOTNET
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;                                            // MSBUILD
using static Nuke.Common.Tools.NuGet.NuGetTasks; 

[GitHubActions(
    "continuous",
    GitHubActionsImage.UbuntuLatest,
    On = new[] { GitHubActionsTrigger.Push },
    InvokedTargets = new[] { nameof(Compile) })]
partial class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    
    [Parameter("Installation directory for the 'install' target.")]
    AbsolutePath InstallDir =  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).ToString(), "MyLittleRangeBook");

    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode

    [GitRepository] readonly GitRepository Repository;
    [CI] readonly GitHubActions GitHubActions;
    [Solution] readonly Solution Solution;     
    [GitVersion(NoFetch = true)] readonly GitVersion GitVer;
    
    const string MasterBranch = "master";
    const string DevelopBranch = "develop";
    
    AbsolutePath OutputDirectory => RootDirectory / "output";
    AbsolutePath SolutionDirectory => RootDirectory / "src/XeroReader";
    readonly AbsolutePath ArtifactsDirectory = RootDirectory / "artifacts";
    AbsolutePath PublishDirectory => OutputDirectory / "publish";
    AbsolutePath CliProject => SolutionDirectory  / "XeroReader.CLI" / "XeroReader.CLI.csproj";
    
    string ChangelogFile => RootDirectory / "CHANGELOG.md";   
    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            Log.Debug("Cleaning the solution: {source}", SolutionDirectory);
            SolutionDirectory.GlobDirectories("*/bin", "*/obj").DeleteDirectories();
            OutputDirectory.CreateOrCleanDirectory();
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Before(Publish)
        .Before(Compile)
        .Executes(() =>
        {
            DotNetRestore(s => s.SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s =>
            {
                return s.SetProjectFile(Solution)
                    .SetConfiguration(Configuration)
                    .SetFramework("net8.0")
                    .SetAssemblyVersion(GitVer.AssemblySemVer)
                    .SetFileVersion(GitVer.AssemblySemFileVer)
                    .SetInformationalVersion(GitVer.InformationalVersion)
                    .SetVerbosity(DotNetVerbosity.minimal)
                    .SetNoLogo(true)
                    .EnableNoRestore();
            });
            
            Log.Information("Compiled version {version}, {infoversion}", GitVer.AssemblySemVer, GitVer.InformationalVersion);
        });

    Target UnitTests => _ => _
        .Executes(() =>
        {
            Log.Verbose("Running all unit tests");
        });

    
    Target Publish => _ => _
        .DependsOn(UnitTests)
        .Executes(() =>
        {
            // [TO20250103] .NET RID Catalog - https://learn.microsoft.com/en-us/dotnet/core/rid-catalog
            string runtime = IsLinux() ? "linux-x64" : "win-x64";
            DotNetPublish(s => s
                .SetProject(CliProject)
                .SetPublishSingleFile(true)
                .SetProperty("DebugType", "embedded")
                .SetProperty("IncludeNativeLibrariesForSelfExtract", "true")
                .SetOutput(ArtifactsDirectory)
                .SetSelfContained(true)
                .SetFramework("net8.0")
                .SetRuntime(runtime)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVer.AssemblySemVer)
                .SetFileVersion(GitVer.AssemblySemFileVer)
                .SetInformationalVersion(GitVer.InformationalVersion)
                .SetVerbosity(DotNetVerbosity.minimal)
                .SetNoLogo(true)
            );
        });
    
    Target Install => _ => _
        .DependsOn(Publish)
        .Executes(() =>
        {
            AbsolutePath app = IsLinux() ? ArtifactsDirectory / "xeror" : ArtifactsDirectory / "xeror.exe";
            app.CopyToDirectory(InstallDir, ExistsPolicy.FileOverwrite, createDirectories:true);
            Log.Information("Installed the application to {installDir} ", InstallDir);
        });

    Target Print => _ => _
        .Executes(() =>
        {
            Log.Information("Commit = {Value}", Repository.Commit);
            Log.Information("Branch = {Value}", Repository.Branch);
            Log.Information("Tags = {Value}", Repository.Tags);

            Log.Information("main branch = {Value}", Repository.IsOnMainBranch());
            Log.Information("main/master branch = {Value}", Repository.IsOnMainOrMasterBranch());
            Log.Information("release/* branch = {Value}", Repository.IsOnReleaseBranch());
            Log.Information("hotfix/* branch = {Value}", Repository.IsOnHotfixBranch());

            Log.Information("Https URL = {Value}", Repository.HttpsUrl);
            Log.Information("SSH URL = {Value}", Repository.SshUrl);
        });

}
