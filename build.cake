#tool "nuget:?package=xunit.runners&version=1.9.2";
#tool "nuget:?package=Squirrel.Windows";
#tool "nuget:?package=GitVersion.CommandLine";

#addin "nuget:?package=Cake.FileHelpers&version=1.0.4";
#addin "nuget:?package=Cake.Squirrel&version=0.12.0";
#addin "nuget:?package=Newtonsoft.Json";
using Newtonsoft.Json;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var githubRepo = Argument("githubrepo", "mugiseyebrows/carnac");
var githubAuthToken = Argument("authtoken", "");

var githubRepoUrl = "https://github.com/mugiseyebrows/carnac";
var solutionFile = "./src/Carnac.sln";
var buildDir = Directory("./src/Carnac/bin") + Directory(configuration);
var toolsDir = Directory("./tools");
var deployDir = Directory("./deploy");
var zipFileHash = "";

var squirrelDeployDir = deployDir + Directory("Squirrel");
var squirrelReleaseDir = squirrelDeployDir + Directory("Releases");
var gitHubDeployDir = deployDir + Directory("GitHub");
GitVersion gitVersionInfo;
string nugetVersion;

Setup(context => 
{
	gitVersionInfo = GitVersion(new GitVersionSettings {
		UpdateAssemblyInfo = true,
		OutputType = GitVersionOutput.Json
	});
	nugetVersion = gitVersionInfo.NuGetVersion;

	Information("Output from GitVersion:");
	Information(JsonConvert.SerializeObject(gitVersionInfo, Formatting.Indented));

	if (BuildSystem.IsRunningOnAppVeyor) {
		BuildSystem.AppVeyor.UpdateBuildVersion(nugetVersion);
	}

	Information("Building " + githubRepo + "v" + nugetVersion);
	Information("Informational version " + gitVersionInfo.InformationalVersion);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        NuGetRestore(solutionFile);
    });

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() => 
    {
        MSBuild(solutionFile, settings =>
            settings.SetConfiguration(configuration));
    });

Task("Default")
    .IsDependentOn("Build");

RunTarget(target);