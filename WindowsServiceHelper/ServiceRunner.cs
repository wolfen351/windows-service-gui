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
        ///     Loads the services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="startImmediatelyWhenShowingGui">
        ///     if set to <c>true</c> start immediately when showing GUI - don't wait for
        ///     start click
        /// </param>
        /// <param name="commandLineParamForAutoStart">The command line parameter to check for</param>
        public static void LoadServices(this IEnumerable<ServiceBase> services, bool startImmediatelyWhenShowingGui = false,
                                        string commandLineParamForAutoStart = null)
        {
            if (ShouldShowGui(commandLineParamForAutoStart))
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
                                                     if (startImmediatelyWhenShowingGui)
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
                try
                {
                    ServiceBase.Run(services.ToArray());
                }
                catch (Exception ee)
                {

                }
            }
        }

        private static bool ShouldShowGui(string commandLineParamForAutoStart)
        {
            //if (Debugger.IsAttached)
            //{
            //    return true;
            //}

            if (commandLineParamForAutoStart != null && Environment.GetCommandLineArgs()
                                                                   .Any(
                                                                        x => string.Equals(
                                                                            x.ToLowerInvariant(),
                                                                            commandLineParamForAutoStart,
                                                                            StringComparison.InvariantCultureIgnoreCase)))
            {
                return true;
            }

            return false;
        }
    }
}