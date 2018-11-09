# Source 

A little library forked from windowsservicehelper for adding a gui in debug mode to windows services

Please see original source for the original code: https://archive.codeplex.com/?p=windowsservicehelper

# Overview 

Helps by creating a Play/Stop/Pause UI when running with a debugger attached, but also allows the windows service to be installed and run by the Windows Services environment as well. All this with one line of code!

# Project Description

Helps by creating a Play/Stop/Pause UI when running with a debugger attached, but also allows the windows service to be installed and run by the Windows Services environment as well. All this with one line of code!
What is Service Helper
Being someone who writes Windows Services a lot, it can be frustrating to deal with the headaches involved in debugging services. Often it involves tricks, hacks, and partial workarounds to test all of your code. There is no "just hit F5" experience for Windows Services developers.

Service Helper solves this by triggering a UI to be shown if a debugger is attached that simulates (as closely as possible) the Windows Services Environment.

![image](https://user-images.githubusercontent.com/5477547/46771760-8dcf0580-cd51-11e8-8310-6dd6c1131a67.png)

Just hit F5, and this UI automatically appears. If there is no debugger attached, your service will execute as normal and can be installed in the Windows Services system.

# How to use?

The easiest way to get Windows Service Helper in your project is to use the NuGet package ServiceProcess.Helpers on the NuGet official feed.

Simply make a few changes to the typical code in the "Program.cs" for your application:

	using System.ServiceProcess;
	using ServiceProcess.Helpers; //HERE

	namespace DemoService
	{
	    static class Program
	    {
		static void Main()
		{
		    ServiceBase[] ServicesToRun;

		    ServicesToRun = new ServiceBase[] 
				{ 
					new Service1() 
				};

		    //ServiceBase.Run(ServicesToRun);
		    ServicesToRun.LoadServices(); //AND HERE
		}
	    }
	}


That's it!

# Nuget Package

To install simply add the following nuget package:

    install-package WindowsService.Gui
    
# Config Options - You can control all the behaviours:

    serviceBases.LoadServices(
        showGuiWhenDebuggerAttached:true,
	showGuiWhenArgumentDetected:true,
	argumentToDetect:"/startService",
	startServiceImmediately:true
	);

# Future Enhancements

* More closely simulating the Windows Services environment and allowing calls like "RequestAdditionalTime" and enforcing timeouts.

# Improve the code

This code is not perfection. If you have a bugfix or enhancement you'd like to see in Windows Service Helper, please send me a pull request and I will put your name in lights (er... maybe here on this wiki page at least).
