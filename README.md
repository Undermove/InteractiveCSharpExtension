# C# Script Runner for Rider

A JetBrains Rider extension that allows you to run C# scripts directly from the IDE.

## Features

- Run C# scripts using the `dotnet run` feature
- Context menu integration for .cs files
- Output displayed in a Terminal window on macOS
- Notifications for script execution status
- Support for macOS (including Intel processors) and Windows

## Requirements

- JetBrains Rider 2025.1 or later
- .NET 8.0 or later installed

## Usage

1. Right-click on a .cs file in the Solution Explorer or Editor
2. Select "Run C# Script" from the context menu
3. The script will be executed using `dotnet run` and the output will be displayed in the Build tool window

You can also use the keyboard shortcut `Ctrl+Shift+R` to run the current script.

## C# Script Features

.NET 10 Preview 4 introduces file-based apps that allow you to run C# files directly without a project file. These scripts support:

- File-level directives for configuration:
  - `#:package` - Reference NuGet packages
  - `#:sdk` - Specify the SDK to use
  - `#:property` - Set MSBuild properties
- Shebang lines for shell scripts

Example script:

```csharp
#:package Humanizer@2.14.1
#:sdk Microsoft.NET.Sdk.Web

using Humanizer;

var dotNet9Released = DateTimeOffset.Parse("2024-12-03");
var since = DateTimeOffset.Now - dotNet9Released;

Console.WriteLine($"It has been {since.Humanize()} since .NET 9 was released.");
```

## Installation

### Method 1: Through Rider Interface

1. Download the `CSharpScriptRunner-1.0.0.zip` file from the `output` folder
2. In Rider, go to Settings > Plugins > Install Plugin from Disk
3. Select the downloaded ZIP file
4. Restart Rider

### Method 2: Manual Installation

1. Close Rider if it's running
2. Copy the contents of the ZIP archive to the Rider plugins folder:
   - macOS: `~/Library/Application Support/JetBrains/Rider2025.1/plugins/CSharpScriptRunner`
   - Windows: `%APPDATA%\JetBrains\Rider2025.1\plugins\CSharpScriptRunner`
3. Start Rider

## Building the Extension

1. Clone the repository
2. Open the solution in Rider
3. Run the build script: `./build.sh`
4. The extension will be available in the `output` folder

## Troubleshooting

If the extension doesn't work:

1. Make sure you have .NET 8.0 SDK or later installed
2. Check that the extension is activated in Settings > Plugins
3. Restart Rider after installing the extension
4. On macOS Intel, make sure the dotnet path is correctly detected (common paths are supported)

## License

This project is licensed under the MIT License - see the LICENSE file for details.