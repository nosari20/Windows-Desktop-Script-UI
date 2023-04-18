using Microsoft.UI;
using Microsoft.UI.Xaml;
using System;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using System.Diagnostics;
using System.Xml.Linq;
using Windows.UI.WindowManagement;
using System.Collections.Generic;
using Windows.UI.Core;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;

namespace Windows_Desktop_Script_UI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        // App Window instance
        Microsoft.UI.Windowing.AppWindow m_AppWindow;

        // App instance
        App m_App;

        // Args parsing helper
        CLIArgs m_CLIArgs;

        // FileWatcher instance
        FileWatcher m_fileWatcher;

        // Out file URI where user input is stored
        const string OUTFILE_DEFAULT_URI = "C:/out";
        string m_OutFileUri = OUTFILE_DEFAULT_URI;

        // Current input
        string m_CurrentInput;


        public MainWindow(App app, string[] cmdargs)
        {

            m_App = app;

            // Parse command line arguments
            m_CLIArgs = new CLIArgs(cmdargs);

            // Show command line arguments for debug
#if DEBUG
            Console.WriteLine(m_CLIArgs.showArgs());
#endif


            // Display help message if requested and close app
            if (m_CLIArgs.hasFlag("help") | m_CLIArgs.hasFlag("h") | !m_CLIArgs.hasOption("WatchPath"))
            {

                if (m_CLIArgs.hasFlag("Debug")) Log.Write("Showing help message");

                // Show help message
                ShowHelp();

                // Terminate process
                Terminate();
            }


            // Init Component
            this.InitializeComponent();

            // Retriev AppWindow object
            GetAppWindow();

            // Modernize window
            ModernWindow m_SetSystemBackdrop = new ModernWindow(this);


            // Handle options

            // FullScreen
            if (m_CLIArgs.hasFlag("FullScreen"))
            {

                if (m_CLIArgs.hasFlag("Debug")) Log.Write("use Full screen");

                SetFullScreen();
            }

            // Window name
            if (m_CLIArgs.hasOption("WindowTitle"))
            {
                string windowName = m_CLIArgs.getOption("WindowTitle");

                if (m_CLIArgs.hasFlag("Debug")) Log.Write("Settting window title to '" + windowName + "'");

                ChangeWindowName(windowName);
            }

            // Welcome message
            if (m_CLIArgs.hasOption("WelcomeMessage"))
            {
                string welcomeMessage = m_CLIArgs.getOption("WelcomeMessage");

                if (m_CLIArgs.hasFlag("Debug")) Log.Write("Settting welcome message to '" + welcomeMessage + "'");

                MainText.Text = string.Join(" ", welcomeMessage);
            }

            // Watch file
            if (m_CLIArgs.hasOption("WatchPath"))
            {
                string watchPath = m_CLIArgs.getOption("WatchPath");

                if (m_CLIArgs.hasFlag("Debug")) Log.Write("Watching file for changes '" + watchPath + "'");

                m_fileWatcher = new FileWatcher(watchPath, m_CLIArgs.hasFlag("Debug"));
                m_fileWatcher.NewLine += OnNewLine;
                m_fileWatcher.StartWatching();
            }

        }

        // Terminate process
        private void Terminate()
        {
            // Exit app
            m_App.Exit();

            // Terminate process
            Environment.Exit(0);
        }


        // Retrieve AppWindow object
        private void GetAppWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            m_AppWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(wndId);
        }


        // Update window name
        private void ChangeWindowName(string name)
        {
            m_AppWindow.Title = name;
        }

        // Change window to full screen
        private void SetFullScreen()
        {
            m_AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        }


        // Show help message
        private void ShowHelp()
        {
            Console.WriteLine("Orchestrated Window");
            Console.WriteLine("Usage : <bin>.exe --WatchPath=<FILE> [OPTIONS] [FLAGS]");
            Console.WriteLine("Options : ");
            Console.WriteLine("--WatchPath=<FILE>         Set orchestrator file path");
            Console.WriteLine("--WindowTitle=<NAME>       Set Window name");
            Console.WriteLine("--WelcomeMessage=<MESSAGE> Set Window name");
            Console.WriteLine("-FullScreen                Set Window full screen");
            Console.WriteLine("-Debug                     Set Window full screen");
            Console.WriteLine("-h, -help                  Display this message");
        }


        // New line handler
        private async void OnNewLine(object sender, IList<string> newLines)
        {

            if (newLines[0] == "Terminate")
            {
                Terminate();
            }

            bool isQueued = this.DispatcherQueue.TryEnqueue(
                Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
                () =>
                    {
                        foreach (var line in newLines)
                        {
                            ExecuteCommand(line);
                        }
                    }
                );
        }


        private void ExecuteCommand(string commandStr)
        {

            if (m_CLIArgs.hasFlag("Debug")) Log.Write("Command: \n\x1b[93m" + commandStr + "\x1b[39m");

            // Command format : <command> --<option>=<value> -<flag>
            // Exemple : MainText --Text="Hello World!"

            // Regex for empty or space only string
            Regex regexSpace = new Regex("^\\s*$");

            // Regex to split by space but respect quotes and double quotes
            Regex regex = new Regex("([^\\s]*'.*?'[^\\s]*|[^\\s]*\".*?\"[^\\s]*|\\S+)");

            // Split string
            string[] substrings = regex.Split(commandStr);

            // Remove empty matches
            IList<string> commandList = substrings.ToList().FindAll(e => regexSpace.Matches(e).Count == 0);


            // Parse arguments
            CLIArgs command = new CLIArgs(commandList.ToArray<string>());


            switch (command.getCommand())
            {
                case "Terminate":
                    Terminate();
                    break;

                case "MainText":
                    if (command.hasOption("Text"))
                    {
                        MainText.Text = command.getOption("Text");
                    }
                    break;

                case "SubText":
                    if (command.hasOption("Text"))
                    {
                        SubText.Text = command.getOption("Text");
                    }
                    break;

                case "MainImage":
                    if (command.hasOption("Source"))
                    {
                        MainImage.Source = new BitmapImage(new Uri(command.getOption("Source"), UriKind.Relative));

                        if (command.hasOption("Height"))
                        {
                            MainImage.Height = Convert.ToDouble(command.getOption("Height")); ;

                        }

                        if (command.hasOption("Width"))
                        {
                            MainImage.Width = Convert.ToDouble(command.getOption("Width")); ;

                        }

                    }
                    break;

                case "Progress":
                    if (command.hasOption("Type"))
                    {

                        if (command.getOption("Type") == "Determinate" & command.hasOption("Value"))
                        {
                            Progress.IsIndeterminate = false;
                            Progress.Value = Convert.ToDouble(command.getOption("Value"));
                        }
                        else
                        {
                            Progress.IsIndeterminate = true;
                            ProgressText.Text = "";
                        }

                        if (command.hasOption("Height"))
                        {
                            Progress.MinHeight = Convert.ToDouble(command.getOption("Height"));
                        }

                        if (command.hasOption("Width"))
                        {
                            Progress.Width = Convert.ToDouble(command.getOption("Width"));
                        }

                        if (command.hasFlag("ShowPercentage"))
                        {
                            ProgressText.Text = (Progress.Value + "%");
                        }
                    }
                    break;
                case "Input":

                    if (command.hasFlag("Hide"))
                    {
                        InputPanel.Scale = new System.Numerics.Vector3(0, 0, 0);
                        break;
                    }


                    if (command.hasOption("Type"))
                    {
                        // Hide all input
                        foreach (UIElement el in InputPanel.Children)
                        {
                            el.Visibility = Visibility.Collapsed;
                        }
                        InputSubmit.Visibility = Visibility.Visible;


                        if (command.hasOption("Out"))
                        {
                            m_OutFileUri = command.getOption("Out");
                        }
                        else
                        {
                            m_OutFileUri = OUTFILE_DEFAULT_URI;
                        }


                        m_CurrentInput = command.getOption("Type");

                        switch (command.getOption("Type"))
                        {
                            case "Text":

                                InputText.Text = "";
                                InputText.Visibility = Visibility.Visible;
                                InputPanel.Scale = new System.Numerics.Vector3(1, 1, 1);

                                if (command.hasOption("PlaceHolder"))
                                {
                                    InputText.PlaceholderText = command.getOption("PlaceHolder");
                                }
                                else
                                {
                                    InputText.PlaceholderText = "";
                                }

                                if (command.hasOption("Header"))
                                {
                                    InputText.Header = command.getOption("Header");
                                }
                                else
                                {
                                    InputText.Header = "";
                                }

                                if (command.hasOption("Button"))
                                {
                                    InputSubmit.Content = command.getOption("Button");
                                }
                                else
                                {
                                    InputSubmit.Content = "Submit";
                                }

                                break;

                            case "Password":
                                
                                InputPassword.Password = "";
                                InputPassword.Visibility = Visibility.Visible;
                                InputPanel.Scale = new System.Numerics.Vector3(1, 1, 1);

                                if (command.hasOption("Header"))
                                {
                                    InputPassword.Header = command.getOption("Header");
                                }
                                else
                                {
                                    InputPassword.Header = "";
                                }

                                if (command.hasOption("Button"))
                                {
                                    InputSubmit.Content = command.getOption("Button");
                                }
                                else
                                {
                                    InputSubmit.Content = "Submit";
                                }

                                break;
                        }

                    }
                    break;

            }


        }



        private void OnButtonSubmit(object sender, RoutedEventArgs e)
        {
            string output = "";

            switch (m_CurrentInput)
            {
                case "Text":
                    output = InputText.Text;
                    break;
                case "Password":
                    output = InputPassword.Password;
                    break;

                default:
                    return;
            }


            using (StreamWriter writetext = new StreamWriter(m_OutFileUri))
            {
                writetext.WriteLine(output);
            }
            InputPanel.Scale = new System.Numerics.Vector3(0, 0, 0);

        }
    }
}


   

   

