using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using System.Windows;
using ServiceProcess.Helpers.ViewModels;

namespace ServiceProcess.Helpers
{
    public static class ServiceRunner
    {
        /// <summary>
        /// Loads the services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="showGuiWhenDebuggerAttached">if set to <c>true</c> shows the GUI when debugger is attached.</param>
        /// <param name="showGuiWhenArgumentDetected">if set to <c>true</c> shows GUI when argument detected (/startService) by default.</param>
        /// <param name="argumentToDetect">The argument to detect. (/startService) by default</param>
        /// <param name="startServiceImmediately">if set to <c>true</c> [start service immediately] - instead of waiting for click start.</param>
        public static void LoadServices(this IEnumerable<ServiceBase> services, bool showGuiWhenDebuggerAttached = true, bool showGuiWhenArgumentDetected=false, string argumentToDetect = "/startService", bool startServiceImmediately = true)
        {
            if (ShouldShowGui(showGuiWhenDebuggerAttached, showGuiWhenArgumentDetected, argumentToDetect))
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
            else
            {
                ServiceBase.Run(services.ToArray());
            }
        }

        /// <summary>
        /// Decides if system should show the GUI or run in service mode
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
    }
}