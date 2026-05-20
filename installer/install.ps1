param(
    [switch]$Launch
)

$ErrorActionPreference = "Stop"

$appName = "Project Update Cloner"
$exeName = "ProjectUpdateCloner.exe"
$projectRoot = Split-Path -Parent $PSScriptRoot
$publishPath = Join-Path $projectRoot "bin\Release\net8.0-windows\win-x64\publish"
$sourceExe = Join-Path $publishPath $exeName
$localAppData = [Environment]::GetFolderPath("LocalApplicationData")
$baseInstallPath = Join-Path $localAppData "ProjectUpdateClonerApp"
$installPath = $baseInstallPath
$desktopShortcut = Join-Path ([Environment]::GetFolderPath("DesktopDirectory")) "$appName.lnk"
$startMenuFolder = Join-Path ([Environment]::GetFolderPath("Programs")) $appName
$startMenuShortcut = Join-Path $startMenuFolder "$appName.lnk"

if (-not (Test-Path $sourceExe)) {
    throw "Published app was not found at $sourceExe. Run dotnet publish first."
}

if (Test-Path $installPath) {
    try {
        Remove-Item -LiteralPath $installPath -Recurse -Force
    }
    catch {
        $timestamp = Get-Date -Format "yyyyMMddHHmmss"
        $installPath = Join-Path $localAppData "ProjectUpdateClonerApp-$timestamp"
        Write-Host "Existing install is locked. Installing to $installPath"
    }
}

New-Item -ItemType Directory -Path $installPath | Out-Null
Copy-Item -Path (Join-Path $publishPath "*") -Destination $installPath -Recurse -Force

New-Item -ItemType Directory -Path $startMenuFolder -Force | Out-Null

$shell = New-Object -ComObject WScript.Shell

$desktop = $shell.CreateShortcut($desktopShortcut)
$desktop.TargetPath = Join-Path $installPath $exeName
$desktop.WorkingDirectory = $installPath
$desktop.IconLocation = Join-Path $installPath $exeName
$desktop.Description = "Package changed project files by date"
$desktop.Save()

$startMenu = $shell.CreateShortcut($startMenuShortcut)
$startMenu.TargetPath = Join-Path $installPath $exeName
$startMenu.WorkingDirectory = $installPath
$startMenu.IconLocation = Join-Path $installPath $exeName
$startMenu.Description = "Package changed project files by date"
$startMenu.Save()

Write-Host "Installed $appName to $installPath"
Write-Host "Desktop shortcut: $desktopShortcut"
Write-Host "Start Menu shortcut: $startMenuShortcut"

if ($Launch) {
    Start-Process -FilePath (Join-Path $installPath $exeName) -WorkingDirectory $installPath
}
