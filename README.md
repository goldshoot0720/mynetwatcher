# NetWatcher

Windows desktop app for monitoring total network bandwidth and per-process traffic in real time.

## Download

Latest release:

- [NetWatcher v1.0.0](https://github.com/goldshoot0720/mynetwatcher/releases/tag/v1.0.0)

Release assets:

- `NetWatcher.App.exe`
- `NetWatcher-v1.0.0-win-x64.zip`

## Features

- Real-time total download and upload speed
- Traffic history chart for recent bandwidth changes
- Per-process traffic list with search and sorting
- CSV export for captured traffic history
- Responsive layout optimized for both laptop and desktop screens

## Requirements

- Windows 10 or Windows 11
- Administrator permission on startup

## Quick Start

1. Download `NetWatcher.App.exe` from the latest release.
2. Run the app as administrator.
3. Allow the Windows permission prompt if shown.
4. Watch live traffic, filter processes, and export CSV when needed.

## Development

Build locally:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' build NetWatcher.App.csproj -c Release
```

Publish Windows release:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' publish NetWatcher.App.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true -o .\artifacts\release\v1.0.0
```

## Notes

- The app uses Windows ETW network events for per-process traffic monitoring.
- Exported CSV files are written to the `exports` folder.
