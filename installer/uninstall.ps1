$ErrorActionPreference = "Stop"

$appName = "Project Update Cloner"
$localAppData = [Environment]::GetFolderPath("LocalApplicationData")
$installPath = Join-Path $localAppData "ProjectUpdateClonerApp"
$legacyInstallPath = Join-Path $localAppData "ProjectUpdateCloner"
$desktopShortcut = Join-Path ([Environment]::GetFolderPath("DesktopDirectory")) "$appName.lnk"
$startMenuFolder = Join-Path ([Environment]::GetFolderPath("Programs")) $appName

if (Test-Path $desktopShortcut) {
    Remove-Item -LiteralPath $desktopShortcut -Force
}

if (Test-Path $startMenuFolder) {
    Remove-Item -LiteralPath $startMenuFolder -Recurse -Force
}

if (Test-Path $installPath) {
    Remove-Item -LiteralPath $installPath -Recurse -Force
}

if (Test-Path $legacyInstallPath) {
    Remove-Item -LiteralPath $legacyInstallPath -Recurse -Force
}

Write-Host "Uninstalled $appName"
