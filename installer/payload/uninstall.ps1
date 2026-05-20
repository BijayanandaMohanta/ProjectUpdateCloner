$ErrorActionPreference = "Stop"

$appName = "Project Update Cloner"
$desktopShortcut = Join-Path ([Environment]::GetFolderPath("DesktopDirectory")) "$appName.lnk"
$startMenuFolder = Join-Path ([Environment]::GetFolderPath("Programs")) $appName
$uninstallRegistryPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\ProjectUpdateCloner"
$installPath = Split-Path -Parent $MyInvocation.MyCommand.Path

Get-Process ProjectUpdateCloner -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

if (Test-Path $desktopShortcut) {
    Remove-Item -LiteralPath $desktopShortcut -Force
}

if (Test-Path $startMenuFolder) {
    Remove-Item -LiteralPath $startMenuFolder -Recurse -Force
}

if (Test-Path $uninstallRegistryPath) {
    Remove-Item -LiteralPath $uninstallRegistryPath -Recurse -Force
}

if (Test-Path $installPath) {
    Start-Process powershell.exe -ArgumentList "-NoProfile -ExecutionPolicy Bypass -Command `"Start-Sleep -Seconds 1; Remove-Item -LiteralPath '$installPath' -Recurse -Force`"" -WindowStyle Hidden
}
