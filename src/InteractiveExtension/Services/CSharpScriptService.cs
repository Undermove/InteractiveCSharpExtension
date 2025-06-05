using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using JetBrains.Application.Parts;
using JetBrains.ProjectModel;
using JetBrains.Util;
using MessageBox = System.Windows.Forms.MessageBox;

namespace InteractiveExtension.Services
{
    [SolutionComponent(Instantiation.DemandAnyThreadSafe)]
    public class CSharpScriptService
    {
        private readonly ISolution _solution;
        private readonly ILogger _logger;

        public CSharpScriptService(
            ISolution solution,
            ILogger logger)
        {
            _solution = solution;
            _logger = logger;
        }

        public void RunScript(string scriptPath)
        {
            if (!File.Exists(scriptPath))
            {
                MessageBox.Show($"The script file {scriptPath} does not exist.", "Script not found", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if the file is a .cs file
            if (!scriptPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Only .cs files are supported.", "Invalid file type", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var isMacOS = IsMacOs();
            try
            {
                // Create a new process to run the script
                var dotnetPath = GetDotNetPath();
                if (string.IsNullOrEmpty(dotnetPath))
                {
                    // Determine if we're on macOS

                    
                    string installInstructions = isMacOS
                        ? "Make sure .NET 8.0 or later is installed. On macOS, you can install it using:\n\nbrew install dotnet@8\n\nOr download from https://dotnet.microsoft.com/download/dotnet/8.0"
                        : "Make sure .NET 8.0 or later is installed from https://dotnet.microsoft.com/download/dotnet/8.0";
                    
                    MessageBox.Show($"Could not find the dotnet executable. {installInstructions}", 
                        "dotnet not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // Check if .NET 8.0 or later is installed
                if (!IsDotNet8OrLaterInstalled(dotnetPath))
                {
                    MessageBox.Show("This feature requires .NET 8.0 or later. Please install it from https://dotnet.microsoft.com/download/dotnet/8.0", 
                        ".NET 8.0 required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                _logger.Info($"Running on macOS: {isMacOS}");
                
                // Create a process to run the script
                var startInfo = new ProcessStartInfo
                {
                    FileName = dotnetPath,
                    Arguments = $"run \"{scriptPath}\" --verbosity detailed",
                    WorkingDirectory = Path.GetDirectoryName(scriptPath)
                };
                
                if (isMacOS)
                {
                    // On macOS, we need to use Terminal to see the output
                    _logger.Info("Using Terminal to run the script on macOS");
                    
                    // Create a temporary shell script to run the command
                    var scriptDir = Path.GetDirectoryName(scriptPath);
                    var tempScriptPath = Path.Combine(scriptDir, "_temp_run_script.sh");
                    var command = $"cd \"{scriptDir}\" && \"{dotnetPath}\" run \"{scriptPath}\" --verbosity detailed; echo; echo \"Press any key to close this window...\"; read -n 1";
                    
                    try
                    {
                        File.WriteAllText(tempScriptPath, "#!/bin/bash\n" + command);
                        _logger.Info($"Created temporary script at {tempScriptPath}");
                        
                        // Make the script executable
                        var chmodProcess = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "chmod",
                                Arguments = $"+x \"{tempScriptPath}\"",
                                UseShellExecute = false,
                                RedirectStandardError = true
                            }
                        };
                        
                        chmodProcess.Start();
                        var errorOutput = chmodProcess.StandardError.ReadToEnd();
                        chmodProcess.WaitForExit();
                        
                        if (!string.IsNullOrEmpty(errorOutput))
                        {
                            _logger.Warn($"chmod error: {errorOutput}");
                        }
                        
                        if (chmodProcess.ExitCode != 0)
                        {
                            _logger.Warn($"chmod exited with code {chmodProcess.ExitCode}");
                        }
                        
                        // Run the script in Terminal
                        startInfo = new ProcessStartInfo
                        {
                            FileName = "open",
                            Arguments = $"-a Terminal \"{tempScriptPath}\"",
                            UseShellExecute = true,
                            CreateNoWindow = false
                        };
                        
                        _logger.Info("Configured to run script with Terminal");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error creating temporary script");
                        // Fall back to standard execution
                        startInfo = new ProcessStartInfo
                        {
                            FileName = dotnetPath,
                            Arguments = $"run \"{scriptPath}\" --verbosity detailed",
                            UseShellExecute = true,
                            CreateNoWindow = false,
                            WorkingDirectory = Path.GetDirectoryName(scriptPath)
                        };
                        
                        _logger.Info("Falling back to standard execution due to error");
                    }
                }
                else
                {
                    // On Windows, we can use the standard approach
                    startInfo.UseShellExecute = true;
                    startInfo.CreateNoWindow = false;
                }
                
                var process = new Process { StartInfo = startInfo };
                
                // Start the process
                process.Start();
                _logger.Info($"Started process for script: {scriptPath}");

                // Show a notification that the script is running
                MessageBox.Show($"Running script {Path.GetFileName(scriptPath)}. Check the console window for output.", 
                    "Running C# Script", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error running C# script");
                MessageBox.Show($"An error occurred while running the script: {ex.Message}", 
                    "Error running script", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool IsMacOs()
        {
            bool isMacOS = Environment.OSVersion.Platform == PlatformID.Unix && 
                           Directory.Exists("/Applications") && 
                           Directory.Exists("/System") && 
                           Directory.Exists("/Users");
            return isMacOS;
        }

        public string GetDotNetPath()
        {
            // Try to find dotnet in the PATH
            var paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator);
            if (paths != null)
            {
                foreach (var path in paths)
                {
                    var dotnetPath = Path.Combine(path, "dotnet");
                    if (File.Exists(dotnetPath))
                    {
                        _logger.Info($"Found dotnet in PATH: {dotnetPath}");
                        return dotnetPath;
                    }

                    // Check for Windows executable
                    var dotnetExePath = Path.Combine(path, "dotnet.exe");
                    if (File.Exists(dotnetExePath))
                    {
                        _logger.Info($"Found dotnet.exe in PATH: {dotnetExePath}");
                        return dotnetExePath;
                    }
                }
            }

            // Default locations - expanded for macOS Intel and Apple Silicon
            var defaultPaths = new[]
            {
                "/usr/local/share/dotnet/dotnet",
                "/usr/local/bin/dotnet",
                "/opt/homebrew/bin/dotnet",
                "/opt/homebrew/Cellar/dotnet/8.0.*/bin/dotnet",
                "/usr/local/Cellar/dotnet/8.0.*/bin/dotnet", // Intel Mac Homebrew path
                "/usr/local/Cellar/dotnet@8/*/bin/dotnet",   // Intel Mac Homebrew versioned path
                "/usr/bin/dotnet",
                "/usr/local/share/dotnet/x64/dotnet",        // Intel Mac specific path
                "C:\\Program Files\\dotnet\\dotnet.exe"
            };

            foreach (var path in defaultPaths)
            {
                // Handle wildcard paths for Homebrew installations
                if (path.Contains("*"))
                {
                    var directory = Path.GetDirectoryName(path);
                    if (Directory.Exists(directory))
                    {
                        var pattern = Path.GetFileName(Path.GetDirectoryName(path));
                        var subdirs = Directory.GetDirectories(directory, pattern);
                        foreach (var subdir in subdirs)
                        {
                            var fullPath = Path.Combine(subdir, "bin", "dotnet");
                            if (File.Exists(fullPath))
                            {
                                _logger.Info($"Found dotnet in Homebrew path: {fullPath}");
                                return fullPath;
                            }
                        }
                    }
                    continue;
                }

                if (File.Exists(path))
                {
                    _logger.Info($"Found dotnet in default path: {path}");
                    return path;
                }
            }

            _logger.Warn("Could not find dotnet executable in any standard location");
            return null;
        }
        
        private bool IsDotNet8OrLaterInstalled(string dotnetPath)
        {
            try
            {
                // Create a process to check the .NET version
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = dotnetPath,
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                
                process.Start();
                var versionOutput = process.StandardOutput.ReadToEnd().Trim();
                var errorOutput = process.StandardError.ReadToEnd().Trim();
                process.WaitForExit();
                
                if (!string.IsNullOrEmpty(errorOutput))
                {
                    _logger.Warn($"Error output when checking .NET version: {errorOutput}");
                }
                
                _logger.Info($"Detected .NET version: {versionOutput}");
                
                // Parse the version
                if (Version.TryParse(versionOutput, out var version))
                {
                    // Check if it's .NET 8.0 or later
                    if (version.Major >= 8)
                    {
                        _logger.Info($"Detected compatible .NET version: {version}");
                        return true;
                    }
                    
                    _logger.Warn($"Detected .NET version {version} is less than required version 8.0");
                }
                else
                {
                    // Handle preview versions or non-standard version strings
                    if (versionOutput.Contains("8.") || versionOutput.Contains("9.") || versionOutput.Contains("10."))
                    {
                        _logger.Info($"Detected compatible .NET version from string: {versionOutput}");
                        return true;
                    }
                    
                    _logger.Warn($"Could not parse .NET version: {versionOutput}");
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking .NET version");
                return false;
            }
        }
    }
}