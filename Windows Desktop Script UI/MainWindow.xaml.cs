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

                Log.Write("Showing help message");

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

                Log.Write("use Full screen");

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

                MyText.Text = string.Join(" ", welcomeMessage);
            }

            // Watch file
            if (m_CLIArgs.hasOption("WatchPath"))
            {
                string watchPath = m_CLIArgs.getOption("WatchPath");

                Log.Write("Watching file for changes '" + watchPath + "'");

                m_fileWatcher = new FileWatcher(watchPath, m_CLIArgs.hasFlag("Debug"));
                m_fileWatcher.NewLine += OnNewLine;
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
            Log.Write("New line detected\n\x1b[93m" + String.Join("\n", newLines) + "\x1b[39m");

            if (newLines[0] == "Terminate")
            {
                Terminate();
            }

            bool isQueued = this.DispatcherQueue.TryEnqueue(
                Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
                () =>
                    {
                        MyText.Text = string.Join(" ", newLines[0]);
                    }
                );
        }

    }
}


   

   

