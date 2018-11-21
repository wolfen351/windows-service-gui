using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using System.Windows;
using ServiceProcess.Helpers.Helpers;
using ServiceProcess.Helpers.ViewModels;

namespace ServiceProcess.Helpers
{
    public static class ServiceRunner
    {
        /// <summary>
        ///     Loads the services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="showGuiWhenDebuggerAttached">if set to <c>true</c> shows the GUI when debugger is attached.</param>
        /// <param name="showGuiWhenArgumentDetected">
        ///     if set to <c>true</c> shows GUI when argument detected (/startService) by
        ///     default.
        /// </param>
        /// <param name="argumentToDetect">The argument to detect. (/startService) by default</param>
        /// <param name="startServiceImmediatelyWhenDebuggerAttached">
        ///     if set to <c>true</c> [start service immediately when
        ///     debugger attached] - Auto start on debugger
        /// </param>
        /// <param name="startServiceImmediatelyWhenArgumentDetected">
        ///     if set to <c>true</c> [start service immediately when
        ///     argument detected] - Auto start on cli param
        /// </param>
        public static void LoadServices(this IEnumerable<ServiceBase> services, bool showGuiWhenDebuggerAttached = true,
                                        bool showGuiWhenArgumentDetected = false, string argumentToDetect = "/startService",
                                        bool startServiceImmediatelyWhenDebuggerAttached = true, bool startServiceImmediatelyWhenArgumentDetected = true)
        {
            bool startServiceImmediately = ShouldAutoStart(
                startServiceImmediatelyWhenDebuggerAttached,
                startServiceImmediatelyWhenArgumentDetected,
                argumentToDetect);

            List<ServiceBase> servicesAll = services.ToList();

            // OPTION 1 - Run via the GUI
            if (ShouldShowGui(showGuiWhenDebuggerAttached, showGuiWhenArgumentDetected, argumentToDetect))
            {
                ShowGUI(servicesAll, startServiceImmediately);
            }
            // OPTION 2 - Run as a app WITHOUT the gui, but also not as a full windows service
            else if (startServiceImmediately)
            {
                Task t = Task.Factory.StartNew(
                    () =>
                    {
                        int servicesRunning = 0;
                        foreach (ServiceBase x in servicesAll)
                        {
                            ServiceBaseHelpers.StartService(x);
                            servicesRunning++;
                        }

                        WaitForAllServicesToExit(servicesAll, servicesRunning);
                    },
                    CancellationToken.None,
                    TaskCreationOptions.PreferFairness,
                    new StaTaskScheduler(25)
                );
                t.Wait();
            }
            else
            {
                // OPTION 3 - Run as a full windows service
                ServiceBase.Run(servicesAll.ToArray());
            }
        }

        private static void WaitForAllServicesToExit(List<ServiceBase> services, int servicesRunning)
        {
            while (servicesRunning > 0)
            {
                servicesRunning = 0;
                foreach (ServiceBase x in services)
                {
                    object status = GetInstanceField(typeof(ServiceBase), x, "status");
                    object state = GetInstanceField(status.GetType(), status, "currentState");
                    if ((int) state != 3)
                    {
                        servicesRunning++;
                    }
                }

                Thread.Sleep(100);
            }
        }

        internal static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                     | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }

        private static void ShowGUI(IEnumerable<ServiceBase> services, bool startServiceImmediately)
        {
            Task t = Task.Factory.StartNew(
                () =>
                {
                    App app = new App();
                    app.InitializeComponent();
                    app.Startup += (o, e) =>
                    {
                        Window window = new Window
                        {
                            Width = 350,
                            Height = 200,
                            Title = "Windows Service Runner",
                            Content = new ServicesControllerViewModel(
                                services.Select(
                                             s =>
                                             {
                                                 ServiceViewModel serviceViewModel = new ServiceViewModel(s);
                                                 if (startServiceImmediately)
                                                 {
                                                     serviceViewModel.StartCommand.Execute(null);
                                                 }

                                                 return serviceViewModel;
                                             })
                                        .ToList())
                        };

                        window.Show();
                    };
                    app.Run();
                },
                CancellationToken.None,
                TaskCreationOptions.PreferFairness,
                new StaTaskScheduler(25)
            );
            t.Wait();
        }

        /// <summary>
        ///     Decides if system should show the GUI or run in service mode
        /// </summary>
        /// <param name="showGuiWhenDebuggerAttached">if set to <c>true</c> [show GUI when debugger attached].</param>
        /// <param name="showGuiWhenArgumentDetected">if set to <c>true</c> [show GUI when argument detected].</param>
        /// <param name="argumentToDetect">The argument to detect.</param>
        /// <returns><c>true</c> if GUI should be shown, <c>false</c> otherwise.</returns>
        private static bool ShouldShowGui(bool showGuiWhenDebuggerAttached, bool showGuiWhenArgumentDetected, string argumentToDetect)
        {
            if (showGuiWhenDebuggerAttached && Debugger.IsAttached)
            {
                return true;
            }

            if (showGuiWhenArgumentDetected && Environment.GetCommandLineArgs()
                                                          .Any(
                                                               x => string.Equals(
                                                                   x.ToLowerInvariant(),
                                                                   argumentToDetect,
                                                                   StringComparison.InvariantCultureIgnoreCase)))
            {
                return true;
            }

            return false;
        }


        private static bool ShouldAutoStart(bool startWhenDebuggerAttached, bool startWhenArgumentDetected, string argumentToDetect)
        {
            if (startWhenDebuggerAttached && Debugger.IsAttached)
            {
                return true;
            }

            if (startWhenArgumentDetected && Environment.GetCommandLineArgs()
                                                        .Any(
                                                             x => string.Equals(
                                                                 x.ToLowerInvariant(),
                                                                 argumentToDetect,
                                                                 StringComparison.InvariantCultureIgnoreCase)))
            {
                return true;
            }

            return false;
        }
    }
}