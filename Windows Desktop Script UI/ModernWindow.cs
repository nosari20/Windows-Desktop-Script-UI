﻿using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices; // For DllImport
using WinRT;
using WinRT.Interop;

namespace Windows_Desktop_Script_UI
{
    public class ModernWindow
    {

        Window m_window;
        AppWindow m_AppWindow;
        WindowsSystemDispatcherQueueHelper m_wsdqHelper; // See below for implementation.
        DesktopAcrylicController m_backdropController;
        SystemBackdropConfiguration m_configurationSource;


        public ModernWindow(Window window)
        {
            m_window = window;


            TryCustomizeAppWindowTitleBar();
            TrySetSystemBackdrop();
        }

        private bool TryCustomizeAppWindowTitleBar()
        {
            
            m_AppWindow = GetAppWindowForCurrentWindow();

            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                AppWindowTitleBar titleBar = m_AppWindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                m_AppWindow.Title = "App";
                return true;
            }
            else
            {
                return false;
            }
        }

        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(m_window);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }

        private bool TrySetSystemBackdrop()
        {
            if (DesktopAcrylicController.IsSupported())
            {
                m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
                m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

                // Create the policy object.
                m_configurationSource = new SystemBackdropConfiguration();
                m_window.Activated += Window_Activated;
                m_window.Closed += Window_Closed;
                ((FrameworkElement)m_window.Content).ActualThemeChanged += Window_ThemeChanged;

                // Initial configuration state.
                m_configurationSource.IsInputActive = true;
                SetConfigurationSourceTheme();
                m_backdropController = new DesktopAcrylicController();
                m_backdropController.TintOpacity = 0.3f;
                m_backdropController.LuminosityOpacity = 0.4f;
                m_backdropController.TintColor = (new Windows.UI.ViewManagement.UISettings()).GetColorValue(Windows.UI.ViewManagement.UIColorType.Background);
                // Enable the system backdrop.
                // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
                m_backdropController.AddSystemBackdropTarget(m_window.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());

                m_backdropController.SetSystemBackdropConfiguration(m_configurationSource);
                return true; // succeeded
            }

            return false; // Mica is not supported on this system
        }


        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            // Make sure any Mica/Acrylic controller is disposed
            // so it doesn't try to use this closed window.
            if (m_backdropController != null)
            {
                m_backdropController.Dispose();
                m_backdropController = null;
            }
            m_window.Activated -= Window_Activated;
            m_configurationSource = null;
        }

        private void Window_ThemeChanged(FrameworkElement sender, object args)
        {
            if (m_configurationSource != null)
            {
                SetConfigurationSourceTheme();
                m_backdropController.TintColor = (new Windows.UI.ViewManagement.UISettings()).GetColorValue(Windows.UI.ViewManagement.UIColorType.Background);
            }
        }

        private void SetConfigurationSourceTheme()
        {
            switch (((FrameworkElement)m_window.Content).ActualTheme)
            {
                case ElementTheme.Dark:     m_configurationSource.Theme = SystemBackdropTheme.Dark;break;
                case ElementTheme.Light:    m_configurationSource.Theme = SystemBackdropTheme.Light; break;
                case ElementTheme.Default:  m_configurationSource.Theme = SystemBackdropTheme.Default; break;
            }
        }

        public void SetOpacity(float opacity)
        {
            m_backdropController.LuminosityOpacity = opacity;
        }

    }
    
    class WindowsSystemDispatcherQueueHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        struct DispatcherQueueOptions
        {
            internal int dwSize;
            internal int threadType;
            internal int apartmentType;
        }

        [DllImport("CoreMessaging.dll")]
        private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

        object m_dispatcherQueueController = null;
        public void EnsureWindowsSystemDispatcherQueueController()
        {
            if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
            {
                // one already exists, so we'll just use it.
                return;
            }

            if (m_dispatcherQueueController == null)
            {
                DispatcherQueueOptions options;
                options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
                options.threadType = 2;    // DQTYPE_THREAD_CURRENT
                options.apartmentType = 2; // DQTAT_COM_STA

                CreateDispatcherQueueController(options, ref m_dispatcherQueueController);
            }
        }
    }
    
}
