# Xenia Settings

A lightweight Windows UI for configuring **Xenia Canary**, the Xbox 360 emulator. Edit emulator settings and toggle game patches without opening TOML files in a text editor.

## Features

- **Config:** browse settings by section, search names and descriptions, and edit values.
- **Patches:** browse games and their patches, toggle individual patches, and search by game name, title ID, patch name, or patch contents.
- View the original game metadata and complete patch contents, including nested write tables.
- Save changes while preserving comments, formatting, encoding, and line endings.
- Detect external file changes before saving and keep pending edits in memory if saving fails.
- Use Patches even when the emulator config is unavailable.

## Screenshots

| Config | Patches |
| --- | --- |
| ![Config tab: setting search, current value, and description](screenshots/ConfigTab.png) | ![Patches tab: game tree, patch checkboxes, and patch description](screenshots/PatchesTab.png) |

## Getting started

Requires **Windows** and **.NET Framework 4.7.2 or later**.

1. Build the application using the instructions below.
2. Copy `xenia_settings.exe` and `xenia_settings.exe.config` into your Xenia Canary folder.
3. Run `xenia_settings.exe`.

The application reads the existing config and patch files next to its executable:

```text
xenia/
├── xenia_canary.exe
├── xenia_settings.exe
├── xenia_settings.exe.config
├── xenia-canary.config.toml
└── patches/
    ├── Game.patch.toml
    └── Another Game.patch.toml
```

### Config

Select a setting and edit **Current value**. Changes stay in memory until you choose **File → Save config**. Saving replaces only changed values in the original file.

**File → Reload config** loads the file again and discards pending config edits after a successful load. If loading fails, the previous settings and pending edits remain available. If the config is missing at startup, restore it and choose **Reload config** to enable the Config tab.

Values are currently entered as raw TOML and are not validated. For example, keep quotation marks around string values.

### Patches

Expand a game to see its patch checkboxes. Select a game to view the file header before the first `[[patch]]`, or select a patch to view its complete contents.

Checkbox changes stay in memory until you choose **File → Save patches**. Saving changes only `is_enabled`. **File → Reload patches** reloads the folder and discards pending patch edits.

If a file changes outside the application, saving that file is blocked with an error message. Reload it before editing again. A missing `patches` folder produces an empty tree; unreadable patch files are reported while other files remain available.

## Building

Install Visual Studio with the **.NET desktop development** workload and the **.NET Framework 4.7.2 targeting pack**. Open `XeniaSettings.sln` and build **Release / Any CPU**, or run the following in a Developer PowerShell terminal:

```powershell
msbuild XeniaSettings.sln /t:Build /p:Configuration=Release '/p:Platform=Any CPU'
```

Build output: `XeniaSettings/bin/Release/`.

The build copies sample files from `XeniaSettings/TestData` into the output folder, preserving the `patches` subfolder. When installing the application, copy the application files into your emulator folder and keep your existing emulator config and patches.

## Regression checks

The test runner uses .NET Framework and requires no additional test packages:

```powershell
msbuild Tests/XeniaSettings.Tests.csproj /t:Build /p:Configuration=Release /p:Platform=AnyCPU
./Tests/bin/Release/XeniaSettings.Tests.exe
```

To also check an existing emulator patch collection and its neighboring config without modifying either, pass the patch folder:

```powershell
./Tests/bin/Release/XeniaSettings.Tests.exe 'C:\Path\To\Xenia\patches'
```

Tests modify only copied fixtures in their own output folder. Rebuild the test project before another run to restore its fixtures.

GitHub Actions builds and runs regression checks for **Release / Any CPU** and **Debug / x64** on Windows.

## License

MIT. See [LICENSE](LICENSE).
