using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using vb14 = VBlib.pkarlibmodule14;

namespace AutyzmTest
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
#if NETFX_CORE
            // ConfigureFilters(Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory);
#else
            InitializeLogging(); // nowsze Uno
#endif 

            this.InitializeComponent();

#if HAS_UNO || NETFX_CORE
            this.Suspending += OnSuspending;
#endif
        }

        protected Frame OnLaunchFragment(Window win)
        {
            Frame mRootFrame = win.Content as Frame;

            //' Do not repeat app initialization when the Window already has content,
            //' just ensure that the window is active

            if (mRootFrame is null)
            {
                //' Create a Frame to act as the navigation context and navigate to the first page
                mRootFrame = new Frame();

                mRootFrame.NavigationFailed += OnNavigationFailed;

                //' PKAR added wedle https://stackoverflow.com/questions/39262926/uwp-hardware-back-press-work-correctly-in-mobile-but-error-with-pc
                mRootFrame.Navigated += OnNavigatedAddBackButton;
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += OnBackButtonPressed;

                //' Place the frame in the current Window
                Window.Current.Content = mRootFrame;

                p.k.InitLib(null);
            }

            return mRootFrame;
        }

#region "Back button"

        private void OnNavigatedAddBackButton(object sender, NavigationEventArgs e)
        {
            try
            {
                Frame oFrame = sender as Frame;
                if (oFrame is null) return;

                Windows.UI.Core.SystemNavigationManager oNavig = Windows.UI.Core.SystemNavigationManager.GetForCurrentView();


                if (oFrame.CanGoBack)
                    oNavig.AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible;
                else
                    oNavig.AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;

                return;
            }
            catch (Exception ex)
            {
                p.k.CrashMessageExit("@OnNavigatedAddBackButton", ex.Message);
            }
        }

        private void OnBackButtonPressed(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            try
            {
                (Window.Current.Content as Frame)?.GoBack();
                e.Handled = true;
            }
            catch { }
        }

#endregion


        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

#if NET5_0 && WINDOWS
            _window = new Window();
            _window.Activate();
#else
            _window = Windows.UI.Xaml.Window.Current;
#endif

            Frame RootFrame = OnLaunchFragment(_window);
            var rootFrame = _window.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                _window.Content = rootFrame;
            }

#if !(NET5_0 && WINDOWS)
            if (args.PrelaunchActivated == false)
#endif
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), args.Arguments);
                }
                // Ensure the current window is active
                _window.Activate();
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private async System.Threading.Tasks.Task<string> AppServiceLocalCommand(string sCommand)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return "NO";
        }


        //  RemoteSystems, Timer
        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            moTaskDeferal = args.TaskInstance.GetDeferral(); // w pkarmodule.App


            bool bNoComplete = false;
            bool bObsluzone = false;

            //' lista komend danej aplikacji
            string sLocalCmds = "";

            //' zwroci false gdy to nie jest RemoteSystem; gdy true, to zainicjalizowało odbieranie
            if (!bObsluzone) bNoComplete = RemSysInit(args, sLocalCmds);

            if (!bNoComplete) moTaskDeferal.Complete();
        }

        //' CommandLine, Toasts
        protected override async void OnActivated(IActivatedEventArgs args)
        {
            //' to jest m.in. dla Toast i tak dalej?

            //' próba czy to commandline
            if (args.Kind == ActivationKind.CommandLineLaunch)
            {

                CommandLineActivatedEventArgs commandLine = args as CommandLineActivatedEventArgs;
                CommandLineActivationOperation operation = commandLine?.Operation;
                string strArgs = operation?.Arguments;


                p.k.InitLib(strArgs.Split(' ')); // mamy command line, próbujemy zrobić z tego string() (.Net Standard 1.4)

                if (!string.IsNullOrEmpty(strArgs))
                {
                    await ObsluzCommandLine(strArgs);
                    Window.Current.Close();
                }
                return;
            }

            p.k.InitLib(null);    // nie mamy dostępu do commandline (.Net Standard 1.4)

            //' jesli nie cmdline (a np. toast), albo cmdline bez parametrow, to pokazujemy okno
            Frame rootFrame = OnLaunchFragment(Windows.UI.Xaml.Window.Current);

            if (args.Kind == ActivationKind.ToastNotification)
                rootFrame.Navigate(typeof(MainPage));


            Window.Current.Activate();
        }


        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        #region "Uno log init"

        /// <summary>
        /// Configures global Uno Platform logging
        /// </summary>
#if NETFX_CORE
        // previous UNO
        /// <summary>
        /// Configures global logging
        /// </summary>
        /// <param name="factory"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        static void ConfigureFilters(ILoggerFactory factory)
        {
            factory
                .WithFilter(new FilterLoggerSettings
                    {
                        { "Uno", LogLevel.Warning },
                        { "Windows", LogLevel.Warning },

						// Debug JS interop
						// { "Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug },

						// Generic Xaml events
						// { "Windows.UI.Xaml", LogLevel.Debug },
						// { "Windows.UI.Xaml.VisualStateGroup", LogLevel.Debug },
						// { "Windows.UI.Xaml.StateTriggerBase", LogLevel.Debug },
						// { "Windows.UI.Xaml.UIElement", LogLevel.Debug },

						// Layouter specific messages
						// { "Windows.UI.Xaml.Controls", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.Layouter", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.Panel", LogLevel.Debug },
						// { "Windows.Storage", LogLevel.Debug },

						// Binding related messages
						// { "Windows.UI.Xaml.Data", LogLevel.Debug },

						// DependencyObject memory references tracking
						// { "ReferenceHolder", LogLevel.Debug },
					}
                )
#if DEBUG
                .AddConsole(LogLevel.Debug);
#else
                .AddConsole(LogLevel.Information);
#endif
        }
#else
        // nowsze Uno
        /// <summary>
        /// Configures global Uno Platform logging
        /// </summary>
        private static void InitializeLogging()
        {
            var factory = LoggerFactory.Create(builder =>
            {
#if __WASM__
                builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__
                builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());
#elif NETFX_CORE
                builder.AddDebug();
#else
                builder.AddConsole();
#endif

                // Exclude logs below this level
                builder.SetMinimumLevel(LogLevel.Information);

                // Default filters for Uno Platform namespaces
                builder.AddFilter("Uno", LogLevel.Warning);
                builder.AddFilter("Windows", LogLevel.Warning);
                builder.AddFilter("Microsoft", LogLevel.Warning);

                // Generic Xaml events
                // builder.AddFilter("Windows.UI.Xaml", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.VisualStateGroup", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.StateTriggerBase", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.UIElement", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.FrameworkElement", LogLevel.Trace );

                // Layouter specific messages
                // builder.AddFilter("Windows.UI.Xaml.Controls", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Controls.Layouter", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Controls.Panel", LogLevel.Debug );

                // builder.AddFilter("Windows.Storage", LogLevel.Debug );

                // Binding related messages
                // builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug );

                // Binder memory references tracking
                // builder.AddFilter("Uno.UI.DataBinding.BinderReferenceHolder", LogLevel.Debug );

                // RemoteControl and HotReload related
                // builder.AddFilter("Uno.UI.RemoteControl", LogLevel.Information);

                // Debug JS interop
                // builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug );
            });

            global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;
        }
#endif

        #endregion 

        #region "RemoteSystem/Background"

        private Windows.ApplicationModel.Background.BackgroundTaskDeferral moTaskDeferal = null;
        private Windows.ApplicationModel.AppService.AppServiceConnection moAppConn;
        private string msLocalCmdsHelp = "";

        private void RemSysOnServiceClosed(Windows.ApplicationModel.AppService.AppServiceConnection appCon, Windows.ApplicationModel.AppService.AppServiceClosedEventArgs args)
        {
            if (appCon != null) appCon.Dispose();
            if (moTaskDeferal != null)
            {
                moTaskDeferal.Complete();
                moTaskDeferal = null;
            }
        }

        private void RemSysOnTaskCanceled(Windows.ApplicationModel.Background.IBackgroundTaskInstance sender, Windows.ApplicationModel.Background.BackgroundTaskCancellationReason reason)
        {
            if (moTaskDeferal != null)
            {
                moTaskDeferal.Complete();
                moTaskDeferal = null;
            }
        }

        ///<summary>
        ///do sprawdzania w OnBackgroundActivated
        ///jak zwróci True, to znaczy że nie wolno zwalniać moTaskDeferal !
        ///sLocalCmdsHelp: tekst do odesłania na HELP
        ///</summary>
        public bool RemSysInit(BackgroundActivatedEventArgs args , string sLocalCmdsHelp)
        {
            Windows.ApplicationModel.AppService.AppServiceTriggerDetails oDetails =
             args.TaskInstance.TriggerDetails as Windows.ApplicationModel.AppService.AppServiceTriggerDetails;
            if (oDetails is null) return false;

            msLocalCmdsHelp = sLocalCmdsHelp;

            args.TaskInstance.Canceled += RemSysOnTaskCanceled;
            moAppConn = oDetails.AppServiceConnection;
            moAppConn.RequestReceived += RemSysOnRequestReceived;
            moAppConn.ServiceClosed += RemSysOnServiceClosed;
            return true;
    }

        public async System.Threading.Tasks.Task<string> CmdLineOrRemSys(string sCommand)
        {
            string sResult = p.k.AppServiceStdCmd(sCommand, msLocalCmdsHelp);
            if (string.IsNullOrEmpty(sResult))
                sResult = await AppServiceLocalCommand(sCommand);

            return sResult;
        }

        public async System.Threading.Tasks.Task ObsluzCommandLine(string sCommand)

        {
            Windows.Storage.StorageFolder oFold = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            if (oFold is null) return;

            string sLockFilepathname = System.IO.Path.Combine(oFold.Path, "cmdline.lock");
            string sResultFilepathname = System.IO.Path.Combine(oFold.Path, "stdout.txt");

            try
            {
                File.WriteAllText(sLockFilepathname, "lock");
            }
            catch
            {
                return;
                    }

            string sResult = await CmdLineOrRemSys(sCommand);
            if (string.IsNullOrEmpty(sResult))
                sResult = "(empty - probably unrecognized command)";

            File.WriteAllText(sResultFilepathname, sResult);

            File.Delete(sLockFilepathname);
        }

        private async void RemSysOnRequestReceived(Windows.ApplicationModel.AppService.AppServiceConnection sender, Windows.ApplicationModel.AppService.AppServiceRequestReceivedEventArgs args)
        {
            // 'Get a deferral so we can use an awaitable API to respond to the message

            string sStatus;
            string sResult = "";
            Windows.ApplicationModel.AppService.AppServiceDeferral messageDeferral = args.GetDeferral();

            if (vb14.GetSettingsBool("remoteSystemDisabled"))
            {
                sStatus = "No permission";
            }
            else
            {
                Windows.Foundation.Collections.ValueSet oInputMsg = args.Request.Message;

                sStatus = "ERROR while processing command";

                if (oInputMsg.ContainsKey("command"))
                {

                    String sCommand = (string)oInputMsg["command"];
                    sResult = await CmdLineOrRemSys(sCommand);
                }

                if (sResult != "") sStatus = "OK";
            }

            Windows.Foundation.Collections.ValueSet oResultMsg = new Windows.Foundation.Collections.ValueSet();
            oResultMsg.Add("status", sStatus);
            oResultMsg.Add("result", sResult);

            await args.Request.SendResponseAsync(oResultMsg);

            messageDeferral.Complete();
            moTaskDeferal.Complete();
        }


#endregion

    }
}
