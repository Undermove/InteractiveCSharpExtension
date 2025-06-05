using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using InteractiveExtension.Services;
using JetBrains.Application.UI.Controls.JetPopupMenu;
using JetBrains.IDE.UI;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.Rd.Tasks;
using JetBrains.UI.RichText;
using JetBrains.Util;

namespace InteractiveExtension.UI
{
    public class CSharpScriptToolWindowControl : UserControl
    {
        private readonly ISolution _solution;
        private readonly ILogger _logger;
        private readonly Lifetime _lifetime;
        
        private TextBox _codeEditor;
        private RichTextBox _outputConsole;
        private Button _runButton;
        private Button _clearButton;
        private SplitContainer _splitContainer;

        public CSharpScriptToolWindowControl(Lifetime lifetime, ISolution solution, ILogger logger)
        {
            _lifetime = lifetime;
            _solution = solution;
            _logger = logger;
            
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Create the main split container
            _splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 300
            };
            
            // Create the code editor
            _codeEditor = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                AcceptsTab = true,
                AcceptsReturn = true,
                Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular),
                Text = "// Enter your C# code here\nConsole.WriteLine(\"Hello, World!\");"
            };
            
            // Create the output console
            _outputConsole = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular)
            };
            
            // Create the button panel
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40
            };
            
            // Create the run button
            _runButton = new Button
            {
                Text = "Run Script",
                Dock = DockStyle.Left,
                Width = 100,
                Height = 30,
                Margin = new Padding(5)
            };
            _runButton.Click += RunButton_Click;
            
            // Create the clear button
            _clearButton = new Button
            {
                Text = "Clear Output",
                Dock = DockStyle.Right,
                Width = 100,
                Height = 30,
                Margin = new Padding(5)
            };
            _clearButton.Click += ClearButton_Click;
            
            // Add the buttons to the panel
            buttonPanel.Controls.Add(_runButton);
            buttonPanel.Controls.Add(_clearButton);
            
            // Add the code editor and button panel to the top panel
            var topPanel = new Panel { Dock = DockStyle.Fill };
            topPanel.Controls.Add(_codeEditor);
            topPanel.Controls.Add(buttonPanel);
            
            // Add the panels to the split container
            _splitContainer.Panel1.Controls.Add(topPanel);
            _splitContainer.Panel2.Controls.Add(_outputConsole);
            
            // Add the split container to the control
            Controls.Add(_splitContainer);
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            try
            {
                _outputConsole.Clear();
                _outputConsole.AppendText("Running script...\n");
                
                // Create a temporary file for the script
                var tempScriptPath = Path.Combine(Path.GetTempPath(), $"CSharpScript_{Guid.NewGuid()}.cs");
                File.WriteAllText(tempScriptPath, _codeEditor.Text);
                
                // Run the script
                var scriptService = _solution.GetComponent<CSharpScriptService>();
                
                // Create a process to run the script and capture output
                var dotnetPath = scriptService.GetDotNetPath();
                if (string.IsNullOrEmpty(dotnetPath))
                {
                    _outputConsole.AppendText("Error: Could not find the dotnet executable. Make sure .NET 10 Preview 4 or later is installed.\n");
                    return;
                }
                
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = dotnetPath,
                        Arguments = $"run \"{tempScriptPath}\" --verbosity detailed",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(tempScriptPath)
                    }
                };
                
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();
                
                process.OutputDataReceived += (s, args) =>
                {
                    if (args.Data != null)
                    {
                        outputBuilder.AppendLine(args.Data);
                        AppendToOutputConsole(args.Data + "\n");
                    }
                };
                
                process.ErrorDataReceived += (s, args) =>
                {
                    if (args.Data != null)
                    {
                        errorBuilder.AppendLine(args.Data);
                        AppendToOutputConsole("ERROR: " + args.Data + "\n", System.Drawing.Color.Red);
                    }
                };
                
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                
                process.WaitForExit();
                
                // Clean up the temporary file
                try
                {
                    File.Delete(tempScriptPath);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error deleting temporary script file");
                }
                
                _outputConsole.AppendText("\nScript execution completed.\n");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error running C# script");
                _outputConsole.AppendText($"Error: {ex.Message}\n");
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            _outputConsole.Clear();
        }
        
        private void AppendToOutputConsole(string text, System.Drawing.Color? color = null)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AppendToOutputConsole(text, color)));
                return;
            }
            
            _outputConsole.SelectionStart = _outputConsole.TextLength;
            _outputConsole.SelectionLength = 0;
            
            if (color.HasValue)
            {
                _outputConsole.SelectionColor = color.Value;
            }
            else
            {
                _outputConsole.SelectionColor = System.Drawing.Color.White;
            }
            
            _outputConsole.AppendText(text);
            _outputConsole.SelectionStart = _outputConsole.Text.Length;
            _outputConsole.ScrollToCaret();
        }
    }
}