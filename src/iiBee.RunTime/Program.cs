using iiBee.RunTime.WorkflowHandling;
using Microsoft.Win32;
using NLog;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace iiBee.RunTime
{
    class Program
    {
        static Logger log = LogManager.GetCurrentClassLogger();
        static TemporaryStorage storage = new TemporaryStorage(new FileInfo(
            ConfigurationManager.AppSettings["TemporaryStorage"]));

        static void Main(string[] args)
        {
            StartParameters startParams = new StartParameters(args);

            if (!InGuestMode())
            {
                //Remove AutoLogon after Start, if it exists
                DisableAutoLogon();
                //Remove AutoStart entry after Start, if it exists
                DisableAutoStart();
            }

            WorkflowRunner wfRunner = null;
            if (storage.WorkflowIsStored())
            {
                FileInfo wfFile = storage.StoredWorkflowFile;
                wfRunner = new WorkflowRunner(wfFile, true);
            }
            else if (startParams.HasParameters)
            {
                wfRunner = new WorkflowRunner(startParams.WorkflowFile, false);
            }
            else
            {
                log.Info("No workflow file to execute, stopping application");
                Environment.Exit(ExitCodes.HaveDoneNothing);
            }

            ExitReaction reaction = wfRunner.RunWorkflow();
            if (reaction == ExitReaction.Reboot)
            {
                if (startParams.HasParameters) //otherwise it is already stored
                    storage.StoreWorkflow(startParams.WorkflowFile);

                log.Info("Preparing for system reboot");
                SendRebootCommand();
                Environment.Exit(ExitCodes.ClosedForReboot);
            }
            else if (reaction == ExitReaction.Finished)
            {
                storage.RemoveStoredWorkflow();

                log.Info("Finished workflow, application is stopping");
                Environment.Exit(ExitCodes.FinishedSuccessfully);
            }
        }
        
        /// <summary>
        /// Execute reboot execution and system preparation.
        /// </summary>
        private static void SendRebootCommand()
        {
            if (InGuestMode())
            {
                // Don't do anything, wBee is only a guest.
                log.Info("I'm a guest, no reboots are done by me.");
                return;
            }

            EnableAutoLogon();
            EnableAutoStart();

            string rebootArgs = "-r -f -t 10";
            log.Debug("Starting reboot with args[" + rebootArgs + "]");
            Process.Start("shutdown", rebootArgs);
        }

        /// <summary>
        /// Enable AutoLogon with default user
        /// </summary>
        private static void EnableAutoLogon()
        {
            RegistryKey myKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\winlogon");
            myKey.SetValue("AutoAdminLogon", 1, RegistryValueKind.String);
            myKey.SetValue("DefaultUserName", ConfigurationManager.AppSettings["DefaultUserName"], RegistryValueKind.String);
            myKey.SetValue("DefaultPassword", ConfigurationManager.AppSettings["DefaultPassword"], RegistryValueKind.String);
            myKey.SetValue("DefaultDomainName", ConfigurationManager.AppSettings["DefaultDomainName"], RegistryValueKind.String);
            myKey.Close();
        }

        /// <summary>
        /// Disable AutoLogon and remove default user from registry
        /// </summary>
        private static void DisableAutoLogon()
        {
            RegistryKey myKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\winlogon");
            myKey.SetValue("AutoAdminLogon", 0, RegistryValueKind.String);
            myKey.DeleteValue("DefaultUserName");
            myKey.DeleteValue("DefaultPassword");
            myKey.DeleteValue("DefaultDomainName");
            myKey.Close();
        }

        /// <summary>
        /// Set an AutoStart Entry in Registry for this Application
        /// </summary>
        private static void EnableAutoStart()
        {
            RegistryKey myKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\wBee");
            myKey.SetValue("wBee", Assembly.GetExecutingAssembly().GetName().CodeBase, RegistryValueKind.String);
            myKey.Close();
        }

        /// <summary>
        /// Remove the AutoStart Entry for this Application
        /// </summary>
        private static void DisableAutoStart()
        {
            Registry.CurrentUser.DeleteSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\wBee");
        }

        /// <summary>
        /// In Guest Mode no reboots are executed, also no registry changes are done.
        /// Application only return exit codes, the Program that starts wBee musst handle the reboots according to it's exit codes.
        /// </summary>
        /// <returns>Returns true if Guest Mode is active.</returns>
        private static bool InGuestMode()
        {
            try
            {
                return (ConfigurationManager.AppSettings["Guest"] == "true") ? true : false;
            }
            catch
            {
                // If Parameter can't be found set it to default.
                return false;
            }
        }
    }
}
