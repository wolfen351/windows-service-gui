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
        public static void LoadServices(this IEnumerable<ServiceBase> services, bool autoStartInDebugMode)
        {
            if (ShouldStartServiceInstantly())
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
                                                     if (autoStartInDebugMode)
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

        private static bool ShouldStartServiceInstantly()
        {
            if (Debugger.IsAttached) return true;
            if (Environment.GetCommandLineArgs().Any(x=>x.ToLowerInvariant() == "/startservice")) return true;
            return false;
        }
    }
}