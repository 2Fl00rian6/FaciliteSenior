[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$RuntimeIdentifier = "win-x64",
    [string]$Version,
    [switch]$SkipWebView2Bootstrapper
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptDir "..")
$projectPath = Join-Path $repoRoot "src\FaciliteSenior\FaciliteSenior.csproj"
$publishDir = Join-Path $repoRoot "artifacts\publish\$RuntimeIdentifier"
$outputDir = Join-Path $repoRoot "artifacts\installer"
$installerScript = Join-Path $scriptDir "FaciliteSenior.iss"
$webView2BootstrapperPath = Join-Path $scriptDir "prereqs\MicrosoftEdgeWebview2Setup.exe"
$outputBaseFilename = "FaciliteSenior-Setup-$((Get-Date).ToString('yyyyMMdd-HHmmss'))"
$dotnetCandidates = @(
    (Join-Path $env:ProgramFiles "dotnet\dotnet.exe"),
    (Join-Path ${env:ProgramFiles(x86)} "dotnet\dotnet.exe")
)
$dotnetPath = $dotnetCandidates | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1

if (-not (Test-Path $projectPath)) {
    throw "Projet introuvable : $projectPath"
}

if (-not $dotnetPath) {
    throw "Le SDK .NET est introuvable. Installez .NET 8 SDK puis relancez ce script."
}

if (-not $Version) {
    [xml]$projectXml = Get-Content -Path $projectPath
    $Version = $projectXml.Project.PropertyGroup.Version | Select-Object -First 1

    if ([string]::IsNullOrWhiteSpace($Version)) {
        $Version = "1.0.0"
    }
}

New-Item -ItemType Directory -Force -Path $publishDir | Out-Null
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

Write-Host "Publication de l'application..."
& $dotnetPath publish $projectPath `
    -c $Configuration `
    -r $RuntimeIdentifier `
    --self-contained true `
    -o $publishDir

if (-not $?) {
    throw "La publication de l'application a echoue."
}

$isccCandidates = @(
    (Join-Path ${env:LOCALAPPDATA} "Programs\Inno Setup 6\ISCC.exe"),
    (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"),
    (Join-Path $env:ProgramFiles "Inno Setup 6\ISCC.exe")
)

$isccPath = $isccCandidates | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1

if (-not $isccPath) {
    throw "Inno Setup 6 est introuvable. Installez-le depuis https://jrsoftware.org/isinfo.php"
}

$compilerArguments = @(
    "/Qp",
    "/DAppVersion=$Version",
    "/DSourceDir=$publishDir",
    "/DOutputDir=$outputDir",
    "/DOutputBaseFilename=$outputBaseFilename",
    $installerScript
)

if (-not $SkipWebView2Bootstrapper) {
    if (Test-Path $webView2BootstrapperPath) {
        $compilerArguments = @(
            "/DIncludeWebView2Bootstrapper=1",
            "/DWebView2BootstrapperPath=$webView2BootstrapperPath"
        ) + $compilerArguments
    }
    else {
        Write-Warning "Bootstrapper WebView2 absent. Le setup sera genere sans installation automatique de WebView2."
        Write-Warning "Placez MicrosoftEdgeWebview2Setup.exe dans installer\prereqs pour inclure ce prerequis."
    }
}

Write-Host "Compilation du setup..."
& $isccPath @compilerArguments

if (-not $?) {
    throw "La compilation du setup a echoue."
}

Write-Host ""
Write-Host "Setup genere : $outputDir\$outputBaseFilename.exe"
