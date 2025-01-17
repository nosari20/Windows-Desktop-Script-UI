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
using Microsoft.UI.Xaml.Media.Imaging;
using Windows_Desktop_Script_UI.InputTypes;
using Windows_Desktop_Script_UI.UserInputTypes;
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
        readonly string OUTFILE_DEFAULT_URI = Path.GetTempPath().ToString() + "/out";
        string m_OutFileUri;

        // Current input
        //string m_CurrentInput;
        IUserInput m_CurrentInput;


        public MainWindow(App app, string[] cmdargs)
        {

            m_App = app;

            // Parse command line arguments
            m_CLIArgs = new CLIArgs(cmdargs);

            // Show command line arguments for debug
#if DEBUG
            Console.WriteLine(m_CLIArgs.showArgs());
            Log.level = Log.Level.Debug;
#endif

            // Display help message if requested and close app
            if (m_CLIArgs.hasFlag("Help") | m_CLIArgs.hasFlag("h") | !m_CLIArgs.hasOption("WatchPath"))
            {

                if (m_CLIArgs.hasFlag("Debug")) Log.Write("Showing help message");

                // Show help message
                ShowHelp();

                // Terminate process
                Terminate();
            }
            if (m_CLIArgs.hasFlag("Debug"))
            {
                Log.level = Log.Level.Debug;
            }




            // Init Component
            this.InitializeComponent();

            // Retriev AppWindow object
            GetAppWindow();

            // Modernize window
            ModernWindow m_SetSystemBackdrop = new ModernWindow(this);


            // Handle options

            

            // AlwaysOnTop
            if (m_CLIArgs.hasFlag("AlwaysOnTop"))
            {

                Log.Write("Set window to be always on top");

                SetAlwaysOnTop();
            }

            // FullScreen
            if (m_CLIArgs.hasFlag("FullScreen"))
            {

                Log.Write("Use full screen");

                SetFullScreen();
            }

            // Window name
            if (m_CLIArgs.hasOption("WindowTitle"))
            {
                string windowName = m_CLIArgs.getOption("WindowTitle");

                Log.Write("Settting window title to '" + windowName + "'");

                ChangeWindowName(windowName);
            }

            // Welcome message
            if (m_CLIArgs.hasOption("WelcomeMessage"))
            {
                string welcomeMessage = m_CLIArgs.getOption("WelcomeMessage");

                Log.Write("Settting welcome message to '" + welcomeMessage + "'");

                MainText.Text = string.Join(" ", welcomeMessage);
            }


            // Handle resize
            Windows.Graphics.SizeInt32 windowSize = m_AppWindow.Size;
            if (m_CLIArgs.hasOption("Height"))
            {
                windowSize.Height = Convert.ToInt32(m_CLIArgs.getOption("Height"));       
            }
            if (m_CLIArgs.hasOption("Width"))
            {
                windowSize.Width = Convert.ToInt32(m_CLIArgs.getOption("Width"));
            }
            m_AppWindow.Resize(windowSize);

            // Watch file
            if (m_CLIArgs.hasOption("WatchPath"))
            {
                string watchPath = m_CLIArgs.getOption("WatchPath");

                Log.Write("Watching file for changes '" + watchPath + "' (" + Path.GetFullPath(watchPath) + ")");

                m_fileWatcher = new FileWatcher(watchPath, m_CLIArgs.hasFlag("Debug"));
                m_fileWatcher.NewLine += OnNewLine;
                m_fileWatcher.StartWatching();
            }
            else
            {
                Console.WriteLine("Argument 'WatchPath' is missing");
                ShowHelp();
                Terminate();

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

        // Set window to be always on top
        private void SetAlwaysOnTop()
        {
            (m_AppWindow.Presenter as OverlappedPresenter).IsAlwaysOnTop = true;
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
            Console.WriteLine("-AlwaysOnTop               Set Window full screen");
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

            // Command format : <command> --<option>=<value> -<flag>
            // Exemple : MainText --Text="Hello World!"

            // Regex for empty or space only string
            Regex regexSpace = new Regex("^\\s*$");

            // Regex to split by space but respect quotes and double quotes
            Regex regex = new Regex("(--[^\\s']*='.*?'[^\\s']*|--[^\\s\"]*=\".*?\"[^\\s\"]*|\\S+)");

            // Split string
            string[] substrings = regex.Split(commandStr);

            // Remove empty matches
            IList<string> commandList = substrings.ToList().FindAll(e => regexSpace.Matches(e).Count == 0);

            
            // Parse arguments
            CLIArgs command = new CLIArgs(commandList.ToArray<string>());

            Log.Write(command.showArgs());

            switch (command.getCommand())
            {
                case "Terminate":
                    Log.Write("Terminate");
                    Terminate();
                    break;

                case "MainText":
                    if (command.hasOption("Text"))
                    {
                        MainText.Text = command.getOption("Text");
                    }

                    Log.Write("MainText : [Text: " + SubText.Text + "]");

                    break;

                case "SubText":
                    if (command.hasOption("Text"))
                    {
                        SubText.Text = command.getOption("Text");
                    }

                    Log.Write("SubText : [Text: " + SubText.Text + "]");
                    break;

                case "MainImage":
                    if (command.hasOption("Source"))
                    {

                        MainImage.Source = new BitmapImage(new Uri(Path.GetFullPath(command.getOption("Source"))));

                        if (command.hasOption("Height"))
                        {
                            MainImage.Height = Convert.ToDouble(command.getOption("Height")); ;

                        }

                        if (command.hasOption("Width"))
                        {
                            MainImage.Width = Convert.ToDouble(command.getOption("Width")); ;

                        }


                        Log.Write("MainImage : [Source: " + Path.GetFullPath(command.getOption("Source")) +
                            ", Height: " + MainImage.Height +
                            ", Width: " + MainImage.Width +
                            "]"
                         );


                    }
                    break;

                case "Load":

                    if (command.hasFlag("Hide"))
                    {
                        Loader.Visibility = Visibility.Collapsed;
                        LoaderText.Visibility = Visibility.Collapsed;
                        FormSubmit.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        Loader.Visibility = Visibility.Visible;
                        LoaderText.Visibility = Visibility.Visible;
                        FormPanel.VerticalAlignment = VerticalAlignment.Center;
                        InputPanel.Children.Clear();
                        FormSubmit.Visibility = Visibility.Collapsed;

                        if (command.hasOption("Text"))
                        {
                            LoaderText.Text = command.getOption("Text").Replace("\\n","\n");
                        }
                        else
                        {
                            LoaderText.Text = "";
                        }
                    }

                    Log.Write("Load [Hide: " + command.hasFlag("Hide") + "]");
                    break;

                case "Input":

                    if (command.hasFlag("Hide"))
                    {
                        InputPanel.Children.Clear();
                        break;
                    }


                    if (command.hasOption("Type"))
                    {
                        InputPanel.Children.Clear();
                        Loader.Visibility = Visibility.Collapsed;
                        LoaderText.Visibility = Visibility.Collapsed;
                        FormSubmit.Visibility = Visibility.Visible;
                        FormPanel.VerticalAlignment = VerticalAlignment.Top;


                        if (command.hasOption("Out"))
                        {
                            m_OutFileUri = command.getOption("Out");
                        }
                        else
                        {
                            m_OutFileUri = OUTFILE_DEFAULT_URI;
                        }

                        if (command.hasOption("Button"))
                        {
                            FormSubmit.Content = command.getOption("Button");
                        }
                        else
                        {
                            FormSubmit.Content = "Submit";
                        }


                        m_CurrentInput = null;


                        switch (command.getOption("Type"))
                        {
                            case "Text":

                                m_CurrentInput = new UserInputText(m_OutFileUri);
                               
                                break;
                            
                            case "Password":

                                m_CurrentInput = new UserInputPassword(m_OutFileUri);

                                break;

                            case "ComboBox":
                                m_CurrentInput = new UserInputComboBox(m_OutFileUri);

                                break;

                            case "ImageChooser":
                                m_CurrentInput = new UserInputImageChooser(m_OutFileUri);

                                break;

                            case "Button":
                                m_CurrentInput = new UserInputButton(m_OutFileUri);

                                break;

                            case "ButtonImage":
                                m_CurrentInput = new UserInputButtonImage(m_OutFileUri);

                                break;

                            case "ButtonVideo":
                                m_CurrentInput = new UserInputButtonVideo(m_OutFileUri);

                                if (command.hasFlag("Autoplay"))
                                {
                                    ((UserInputButtonVideo) m_CurrentInput).AutoPlay = true;
                                }
                                if (command.hasFlag("ShowControl"))
                                {
                                    ((UserInputButtonVideo)m_CurrentInput).AreTransportControlsEnabled = true;
                                }

                                if (command.hasFlag("SoundOn"))
                                {
                                    ((UserInputButtonVideo)m_CurrentInput).IsMuted = false;
                                }
                                break;

                            case "ButtonText":
                                m_CurrentInput = new UserInputButtonText(m_OutFileUri);
                             
                                break;


                        }
                        if (m_CurrentInput != null)
                        {

                            if (command.hasOption("Height"))
                            {
                                m_CurrentInput.Height = Convert.ToDouble(command.getOption("Height"));
                            }

                            if (command.hasOption("Width"))
                            {
                                m_CurrentInput.Width = Convert.ToDouble(command.getOption("Width"));
                            }

                            if (command.hasOption("Header"))
                            {
                                m_CurrentInput.Header = command.getOption("Header");
                            }

                            if (command.hasOption("Value"))
                            {
                                m_CurrentInput.Value = command.getOption("Value");
                            }

                            if (command.hasOption("AllowedValues"))
                            {
                                m_CurrentInput.AllowedValues = command.getOption("AllowedValues");
                            }

                            if (command.hasOption("PlaceHolder"))
                            {
                                m_CurrentInput.PlaceHolder = command.getOption("PlaceHolder");
                            }



                            if (m_CurrentInput.GetElement() != null)
                            {
                                InputPanel.Children.Insert(0, m_CurrentInput.GetElement());
                            }


                            Log.Write("Input : " + m_CurrentInput.ToString());
                            

                        }
                        
                    }
                    break;

            }


        }



        private void OnButtonSubmit(object sender, RoutedEventArgs e)
        {
            m_CurrentInput.WriteOutput();
        }
    }
}


   

   

