﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Squirrel;

// This file would normally be mostly empty when applying the MVVM pattern,
// but since this app attempts to be a simple and focused sample ...

namespace Resquirrelly
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ScheduleAppUpdates();
        }

        public async Task ScheduleAppUpdates()
        {
            const int appUpdateInterval = 20000; // 20 seconds
            var appUpdateTimer = new Timer(ScheduleApplicationUpdates, null, 0, appUpdateInterval);
            await appUpdateTimer.Start();
        }

        private async void ScheduleApplicationUpdates(Object o)
        {
            var location = UpdateHelper.AppUpdateCheckLocation;
            var appName = Assembly.GetExecutingAssembly().GetName().Name;
            using (var mgr = new UpdateManager(location, appName, FrameworkVersion.Net45))
            {
                try
                {
                    UpdateInfo updateInfo = await mgr.CheckForUpdate();
                    if (updateInfo.FutureReleaseEntry != null)
                    {
                        if (updateInfo.CurrentlyInstalledVersion.Version == updateInfo.FutureReleaseEntry.Version) return;
                        await mgr.UpdateApp();

                        // This will show a button that will let the user restart the app
                        Dispatcher.Invoke(ShowUpdateIsAvailable);

                        // This will restart the app automatically
                        //Dispatcher.InvokeAsync<Task>(ShutdownApp);
                    }
                }
                catch (Exception ex)
                {
                    var a = ex;
                }
            }
        }

        private void RestartButtonClicked(object sender, RoutedEventArgs e)
        {
            ShutdownApp();
        }

        private async Task ShutdownApp()
        {
            UpdateManager.RestartApp();
            await Task.Delay(1000);
            Application.Current.Shutdown(0);
        }

        private void ShowUpdateIsAvailable()
        {
            RestartButton.Visibility = Visibility.Visible;
            Instructions.Visibility = Visibility.Hidden;
        }

        // This listens for Windows messages so we can pop up this window if the
        // user tries to launch a second instance of the application. You can 
        // find more information in NativeMethods.cs and StartupManager.cs.
        protected override void OnSourceInitialized(EventArgs eventArgs)
        {
            base.OnSourceInitialized(eventArgs);
            var source = PresentationSource.FromVisual(this) as HwndSource;
            if (source != null) source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_SHOWME)
            {
                Show();
                WindowState = WindowState.Normal;
            }
            return IntPtr.Zero;
        }

    }
}
