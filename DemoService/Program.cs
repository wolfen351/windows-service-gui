﻿using System.ServiceProcess;
using ServiceProcess.Helpers;

namespace DemoService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;

            ServicesToRun = new ServiceBase[] 
			{ 
				new Service1() 
			};
           
            //ServiceBase.Run(ServicesToRun);
            ServicesToRun.LoadServices(
                showGuiWhenDebuggerAttached: true, 
                showGuiWhenArgumentDetected: true, 
                argumentToDetect: "/start", 
                startServiceImmediatelyWhenDebuggerAttached: true,
                startServiceImmediatelyWhenArgumentDetected: true
                );
        }
    }
}
