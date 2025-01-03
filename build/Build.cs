
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
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// [GitHubActions(
//     "ci",
//     GitHubActionsImage.UbuntuLatest,
//     FetchDepth = 0,
//     On = new[] { GitHubActionsTrigger.Push },
//     InvokedTargets = new[] { nameof(Compile) })]
partial class Build : NukeBuild
{
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [CI] readonly GitHubActions GitHubActions;
    [GitVersion(NoFetch = true)] readonly GitVersion GitVer;
    [GitRepository] readonly GitRepository Repository;
    [Solution] readonly Solution Solution;
    
    [Parameter("Installation directory for the 'install' target.")]
    AbsolutePath InstallDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "MyLittleRangeBook");
    readonly AbsolutePath ArtifactsDirectory = RootDirectory / "artifacts";
    AbsolutePath OutputDirectory => RootDirectory / "output";
    AbsolutePath XeroReaderSolutionDirectory => RootDirectory / "src/XeroReader";
    AbsolutePath PublishDirectory => OutputDirectory / "publish";
    AbsolutePath XeroReaderCliProject => XeroReaderSolutionDirectory / "XeroReader.CLI" / "XeroReader.CLI.csproj";

    string ChangelogFile => RootDirectory / "CHANGELOG.md";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            Log.Debug("Cleaning the solution: {source}", XeroReaderSolutionDirectory);
            XeroReaderSolutionDirectory.GlobDirectories("*/bin", "*/obj").DeleteDirectories();
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
            
            Log.Information("Branch = {Branch}", GitHubActions.Ref);
            Log.Information("Commit = {Commit}", GitHubActions.Sha);
            
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
            var runtime = IsLinux() ? "linux-x64" : "win-x64";
            DotNetPublish(s => s
                .SetProject(XeroReaderCliProject)
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
            var app = IsLinux() ? ArtifactsDirectory / "xeror" : ArtifactsDirectory / "xeror.exe";
            app.CopyToDirectory(InstallDir, ExistsPolicy.FileOverwrite, createDirectories: true);
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

    public static int Main() => Execute<Build>(x => x.Compile);
}