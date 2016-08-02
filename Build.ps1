param(
    [Parameter(Mandatory=$false)][bool]   $RestorePackages  = $false,
    [Parameter(Mandatory=$false)][string] $Configuration    = "Release",
    [Parameter(Mandatory=$false)][string] $VersionSuffix    = "",
    [Parameter(Mandatory=$false)][bool]   $PatchVersion     = $false,
    [Parameter(Mandatory=$false)][bool]   $RunTests         = $true,
    [Parameter(Mandatory=$false)][bool]   $PublishWebsite   = $true
)

$ErrorActionPreference = "Stop"

$solutionPath  = Split-Path $MyInvocation.MyCommand.Definition
$framework     = "netcoreapp1.0"
$outputPath    = "$(Convert-Path "$PSScriptRoot")\artifacts"
$getDotNet     = Join-Path $solutionPath "tools\install.ps1"
$dotnetVersion = "latest"

$env:DOTNET_INSTALL_DIR = "$(Convert-Path "$PSScriptRoot")\.dotnetcli"

if ($env:CI -ne $null -Or $env:WEBSITE_SITE_NAME -ne $null) {
    $RestorePackages = $true
    $PatchVersion = $true
}

if (!(Test-Path $env:DOTNET_INSTALL_DIR)) {
    mkdir $env:DOTNET_INSTALL_DIR | Out-Null
    $installScript = Join-Path $env:DOTNET_INSTALL_DIR "install.ps1"
    Invoke-WebRequest "https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/dotnet-install.ps1" -OutFile $installScript
    & $installScript -Version "$dotnetVersion" -InstallDir "$env:DOTNET_INSTALL_DIR" -NoPath
}

$env:PATH = "$env:DOTNET_INSTALL_DIR;$env:PATH"
$dotnet   = "$env:DOTNET_INSTALL_DIR\dotnet"

function DotNetRestore { param([string]$Project)
    & $dotnet restore $Project --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore failed with exit code $LASTEXITCODE"
    }
}

function DotNetBuild { param([string]$Project, [string]$Configuration, [string]$VersionSuffix)
    if ($VersionSuffix) {
        & $dotnet build $Project --output $outputPath --framework $framework --configuration $Configuration --version-suffix "$VersionSuffix"
    } else {
        & $dotnet build $Project --output $outputPath --framework $framework --configuration $Configuration
    }
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE"
    }
}

function DotNetTest { param([string]$Project)
    & $dotnet test $Project --output $outputPath --framework $framework --no-build
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed with exit code $LASTEXITCODE"
    }
}

function DotNetPublish { param([string]$Project)
    if ($VersionSuffix) {
        & $dotnet publish $Project --output (Join-Path $outputPath "publish") --framework $framework --configuration $Configuration --version-suffix "$VersionSuffix" --no-build
    } else {
        & $dotnet publish $Project --output (Join-Path $outputPath "publish") --framework $framework --configuration $Configuration --no-build
    }
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE"
    }
}

if ($PatchVersion -eq $true) {

    $gitRevision = (git rev-parse HEAD | Out-String).Trim()
    $gitBranch   = (git rev-parse --abbrev-ref HEAD | Out-String).Trim()

    $assemblyVersion = Get-Content ".\AssemblyVersion.cs" -Raw
    $assemblyVersionWithMetadata = "{0}using System.Reflection;`r`n`r`n[assembly: AssemblyMetadata(""CommitHash"", ""{1}"")]`r`n[assembly: AssemblyMetadata(""CommitBranch"", ""{2}"")]" -f $assemblyVersion, $gitRevision, $gitBranch

    Set-Content ".\AssemblyVersion.cs" $assemblyVersionWithMetadata -Encoding utf8
}

$projects = @(
    (Join-Path $solutionPath "src\Website\project.json"),
    (Join-Path $solutionPath "tests\Website.Tests\project.json")
)

$testProjects = @(
    (Join-Path $solutionPath "tests\Website.Tests\project.json")
)

$publishProjects = @(
    (Join-Path $solutionPath "src\Website\project.json")
)

if ($RestorePackages -eq $true) {
    Write-Host "Restoring NuGet packages for $($projects.Count) projects..." -ForegroundColor Green
    ForEach ($project in $projects) {
        DotNetRestore $project
    }
}

Write-Host "Building $($projects.Count) projects..." -ForegroundColor Green
ForEach ($project in $projects) {
    DotNetBuild $project $Configuration $PrereleaseSuffix
}

if ($RunTests -eq $true) {
    Write-Host "Testing $($testProjects.Count) project(s)..." -ForegroundColor Green
    ForEach ($project in $testProjects) {
        DotNetTest $project
    }
}

if ($PublishWebsite -eq $true) {
    Write-Host "Publishing $($publishProjects.Count) projects..." -ForegroundColor Green
    ForEach ($project in $publishProjects) {
        DotNetPublish $project $Configuration $PrereleaseSuffix
    }
}

if ($PatchVersion -eq $true) {
    Set-Content ".\AssemblyVersion.cs" $assemblyVersion -Encoding utf8
}
