using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace DemoService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Thread.Sleep(2000);
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            Thread.Sleep(2000);
        }

        protected override void OnContinue()
        {
            Thread.Sleep(2000);
        }

        protected override void OnPause()
        {
            Thread.Sleep(2000);
        }

        
    }
}
