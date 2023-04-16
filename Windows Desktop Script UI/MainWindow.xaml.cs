using Microsoft.UI;
using Microsoft.UI.Xaml;
using System;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using System.Diagnostics;
using System.Xml.Linq;
using Windows.UI.WindowManagement;

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
            if (m_CLIArgs.hasFlag("help") || m_CLIArgs.hasFlag("h"))
            {

                Log.Write("Showing help message");

                // Show help message
                ShowHelp();

                // Exit app
                m_App.Exit();

                // Terminate process
                Environment.Exit(0);
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

                // TODO
            }

        }


        private void GetAppWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            m_AppWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(wndId);
        }


        private void ChangeWindowName(string name)
        {
            m_AppWindow.Title = name;
        }

        private void SetFullScreen()
        {
            m_AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        }


        private void ShowHelp()
        {
            Console.WriteLine("Orchestrated Window");
            Console.WriteLine("Usage : <bin>.exe [OPTIONS] [FLAGS]");
            Console.WriteLine("Options : ");
            Console.WriteLine("--WatchPath=<FILE>         Set orchestrator file path");
            Console.WriteLine("--WindowTitle=<NAME>       Set Window name");
            Console.WriteLine("--WelcomeMessage=<MESSAGE> Set Window name");
            Console.WriteLine("-FullScreen                Set Window full screen");
            Console.WriteLine("-h, -help                  Display this message");
        }

    }
}


   

   

