<img width="100%" alt="screenshot" src="https://github.com/user-attachments/assets/ba52d876-81b9-47a2-a100-15c3f3fc0e64" />

# Project Update Cloner

Desktop tool for packaging only the project files modified on a selected date while preserving the original folder structure.

## Features

- Select source project folder.
- Select output folder.
- Pick a modified date.
- Preserve relative folder paths with `Path.GetRelativePath`.
- Ignore common folders: `.git`, `vendor`, `node_modules`, `bin`, `obj`, and `storage/logs`.
- Select project folders to skip completely.
- Optional file filters such as `*.php, *.js, *.css`.
- Optional ZIP export.
- Generates `update-log.txt` inside the package.
- Handles inaccessible and locked files with per-file error logging.

## Build

Install the .NET 8 SDK, then run:

```powershell
dotnet build
dotnet run
```

The current machine did not have `dotnet` available on PATH when this project was scaffolded.

## Install Locally

Publish the app:

```powershell
& "C:\Program Files\dotnet\dotnet.exe" publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Install it for the current Windows user and launch it:

```powershell
powershell -ExecutionPolicy Bypass -File .\installer\install.ps1 -Launch
```

This copies the app to:

```text
%LOCALAPPDATA%\ProjectUpdateClonerApp
```

It also creates Desktop and Start Menu shortcuts named `Project Update Cloner`.

To uninstall:

```powershell
powershell -ExecutionPolicy Bypass -File .\installer\uninstall.ps1
```
