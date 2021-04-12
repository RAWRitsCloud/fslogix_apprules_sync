using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Timers;
using System.Diagnostics;
using System.Xml;

namespace FSLogixAppRules
{
    public partial class FSLogixAppRulesCopy : ServiceBase
    {
        private System.Timers.Timer timer;
        string rulesSource;
        string rulesDestination;
        EventLog appLog;

        public FSLogixAppRulesCopy()
        {
            appLog = new EventLog("Application");
            appLog.Source = "FSLogix App Masking Rules Copier";

            XmlDocument config = new XmlDocument();
            string fileName = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + @"config.xml";
            config.Load(fileName);

            rulesSource = config.SelectSingleNode("AppConfig/Source").InnerText;
            rulesDestination = config.SelectSingleNode("AppConfig/Destination").InnerText;
            double RunEveryMinutes = Double.Parse(config.SelectSingleNode("AppConfig/RunEveryMinutes").InnerText);

            InitializeComponent();

            timer = new System.Timers.Timer();
            timer.Interval = TimeSpan.FromMinutes(RunEveryMinutes).TotalMilliseconds;
            timer.Elapsed += (s, e) => CopyFiles();
        }

        protected override void OnStart(string[] args)
        {
            timer.Enabled = true;
        }

        private void CopyFiles()
        {
            appLog.WriteEntry("Starting Copy Job, Rule Source: " + rulesSource + ", Rule Destination: " + rulesDestination, EventLogEntryType.Information, 101);


            string fileName;
            string destFile;
            string[] files = System.IO.Directory.GetFiles(rulesSource);

            foreach (string s in files)
            {
                // Use static Path methods to extract only the file name from the path.
                fileName = System.IO.Path.GetFileName(s);
                destFile = System.IO.Path.Combine(rulesDestination, fileName);
                System.IO.File.Copy(s, destFile, true);
            }
        }

        protected override void OnStop()
        {
            timer.Enabled = false;
        }
    }
}
