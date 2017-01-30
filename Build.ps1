param(
    [Parameter(Mandatory=$false)][bool]   $RestorePackages  = $false,
    [Parameter(Mandatory=$false)][string] $Configuration    = "Release",
    [Parameter(Mandatory=$false)][string] $VersionSuffix    = "",
    [Parameter(Mandatory=$false)][string] $OutputPath       = "",
    [Parameter(Mandatory=$false)][bool]   $PatchVersion     = $false,
    [Parameter(Mandatory=$false)][bool]   $RunTests         = $true,
    [Parameter(Mandatory=$false)][bool]   $PublishWebsite   = $true
)

$ErrorActionPreference = "Stop"

$solutionPath  = Split-Path $MyInvocation.MyCommand.Definition
$framework     = "netcoreapp1.1"
$getDotNet     = Join-Path $solutionPath "tools\install.ps1"
$dotnetVersion = "1.0.0-rc3-004530"

if ($OutputPath -eq "") {
    $OutputPath = "$(Convert-Path "$PSScriptRoot")\artifacts"
}

$env:DOTNET_INSTALL_DIR = "$(Convert-Path "$PSScriptRoot")\.dotnetcli"

if ($env:CI -ne $null -Or $env:TF_BUILD -ne $null) {
    $RestorePackages = $true
    $PatchVersion = $true
}

if (!(Test-Path $env:DOTNET_INSTALL_DIR)) {
    mkdir $env:DOTNET_INSTALL_DIR | Out-Null
    & $getDotNet -Version "$dotnetVersion" -InstallDir "$env:DOTNET_INSTALL_DIR" -NoPath
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
        & $dotnet build $Project --output $OutputPath --framework $framework --configuration $Configuration --version-suffix "$VersionSuffix"
    } else {
        & $dotnet build $Project --output $OutputPath --framework $framework --configuration $Configuration
    }
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE"
    }
}

function DotNetTest { param([string]$Project)
    & $dotnet test $Project --output $OutputPath --framework $framework --no-build
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed with exit code $LASTEXITCODE"
    }
}

function DotNetPublish { param([string]$Project)
    $publishPath = (Join-Path $OutputPath "publish")
    if ($VersionSuffix) {
        & $dotnet publish $Project --output $publishPath --framework $framework --configuration $Configuration --version-suffix "$VersionSuffix"
    } else {
        & $dotnet publish $Project --output $publishPath --framework $framework --configuration $Configuration
    }
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE"
    }
}

if ($PatchVersion -eq $true) {

    $gitBranch = $env:BUILD_SOURCEBRANCHNAME

    if ([string]::IsNullOrEmpty($gitBranch)) {
        $gitBranch = (git rev-parse --abbrev-ref HEAD | Out-String).Trim()
    }

    $gitRevision = (git rev-parse HEAD | Out-String).Trim()
    $timestamp   = [DateTime]::UtcNow.ToString("yyyy-MM-ddTHH:mm:ssK")

    $assemblyVersion = Get-Content ".\AssemblyVersion.cs" -Raw
    $assemblyVersionWithMetadata = "{0}using System.Reflection;`r`n`r`n[assembly: AssemblyMetadata(""CommitHash"", ""{1}"")]`r`n[assembly: AssemblyMetadata(""CommitBranch"", ""{2}"")]`r`n[assembly: AssemblyMetadata(""BuildTimestamp"", ""{3}"")]" -f $assemblyVersion, $gitRevision, $gitBranch, $timestamp

    Set-Content ".\AssemblyVersion.cs" $assemblyVersionWithMetadata -Encoding utf8
}

$projects = @(
    (Join-Path $solutionPath "src\Website\Website.csproj"),
    (Join-Path $solutionPath "tests\Website.Tests\Website.Tests.csproj")
)

$testProjects = @(
    (Join-Path $solutionPath "tests\Website.Tests\Website.csproj")
)

$publishProjects = @(
    (Join-Path $solutionPath "src\Website\Website.csproj")
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
