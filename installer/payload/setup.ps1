$ErrorActionPreference = "Stop"

$appName = "Project Update Cloner"
$publisher = "UpdateCloner"
$exeName = "ProjectUpdateCloner.exe"
$sourceRoot = $PSScriptRoot
$sourceExe = Join-Path $sourceRoot $exeName
$sourceUninstaller = Join-Path $sourceRoot "uninstall.ps1"
$localAppData = [Environment]::GetFolderPath("LocalApplicationData")
$installPath = Join-Path $localAppData "ProjectUpdateClonerApp"
$desktopShortcut = Join-Path ([Environment]::GetFolderPath("DesktopDirectory")) "$appName.lnk"
$startMenuFolder = Join-Path ([Environment]::GetFolderPath("Programs")) $appName
$startMenuShortcut = Join-Path $startMenuFolder "$appName.lnk"
$uninstallRegistryPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\ProjectUpdateCloner"

if (-not (Test-Path $sourceExe)) {
    throw "Installer payload is missing $exeName"
}

if (Test-Path $installPath) {
    try {
        Remove-Item -LiteralPath $installPath -Recurse -Force
    }
    catch {
        $timestamp = Get-Date -Format "yyyyMMddHHmmss"
        $installPath = Join-Path $localAppData "ProjectUpdateClonerApp-$timestamp"
    }
}

New-Item -ItemType Directory -Path $installPath -Force | Out-Null
Copy-Item -LiteralPath $sourceExe -Destination (Join-Path $installPath $exeName) -Force
Copy-Item -LiteralPath $sourceUninstaller -Destination (Join-Path $installPath "uninstall.ps1") -Force

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

New-Item -Path $uninstallRegistryPath -Force | Out-Null
New-ItemProperty -Path $uninstallRegistryPath -Name DisplayName -Value $appName -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallRegistryPath -Name DisplayVersion -Value "1.0.0" -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallRegistryPath -Name Publisher -Value $publisher -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallRegistryPath -Name InstallLocation -Value $installPath -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallRegistryPath -Name DisplayIcon -Value (Join-Path $installPath $exeName) -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallRegistryPath -Name UninstallString -Value "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$installPath\uninstall.ps1`"" -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallRegistryPath -Name NoModify -Value 1 -PropertyType DWord -Force | Out-Null
New-ItemProperty -Path $uninstallRegistryPath -Name NoRepair -Value 1 -PropertyType DWord -Force | Out-Null

Start-Process -FilePath (Join-Path $installPath $exeName) -WorkingDirectory $installPath
