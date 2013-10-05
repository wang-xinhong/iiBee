using iiBee.RunTime.Helper;
using iiBee.RunTime.WorkflowHandling;
using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace iiBee.RunTime
{
    class Program
    {
        static Logger log = LogManager.GetCurrentClassLogger();
        static TemporaryStorage storage = new TemporaryStorage(new DirectoryInfo(
            ConfigurationManager.AppSettings["TemporaryStorage"]));

        static void Main(string[] args)
        {
            try
            {
                StartExecution(args);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private static void StartExecution(string[] args)
        {
            StartParameters startParams = new StartParameters(args);

            WorkflowRunner wfRunner = null;
            if (!startParams.HasParameters && storage.WorkflowIsStored())
            {
                // WBee returns from an reboot/restart
                if (!InGuestMode())
                {
                    //Remove AutoLogon after Start, if it exists
                    DisableAutoLogon();
                    //Remove AutoStart entry after Start, if it exists
                    DisableAutoStart();
                }
                else
                {
                    log.Info("Guest Mode active: WBee will not change registry or start reboots.");
                }

                FileInfo wfFile = storage.StoredWorkflowFile;
                log.Info("Found persisted workflow file[" + wfFile.FullName + "]");

                wfRunner = new WorkflowRunner(wfFile, true);
            }
            else if (startParams.HasParameters)
            {
                storage.StoreWorkflow(startParams.WorkflowFile);
                Dictionary<string, object> input = GetInputArguments(storage.StoredWorkflowFile, startParams.InputArguments);
                wfRunner = new WorkflowRunner(storage.StoredWorkflowFile, false, input);
            }
            else
            {
                log.Info("No workflow file to execute, stopping application");
                Environment.Exit(ExitCodes.HaveDoneNothing);
            }

            ExitReaction reaction = wfRunner.RunWorkflow();
            if (reaction == ExitReaction.Reboot)
            {
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


        private static Dictionary<string, object> GetInputArguments(FileInfo fileInfo, Dictionary<string, string> dictionary)
        {
            Dictionary<string, object> dic = null;
            if (dictionary != null)
            {
                List<WorkflowArgument> args = XamlHelper.GetArgumentsNames(fileInfo.FullName);
                dic = XamlHelper.ConvertDictionary(dictionary, args);
            }
            return dic;
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
            log.Debug("Add autologon to registry ...");
            RegistryKey myKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\winlogon", true);
            myKey.SetValue("AutoAdminLogon", 1, RegistryValueKind.String);
            myKey.SetValue("DefaultUserName", ConfigurationManager.AppSettings["DefaultUserName"], RegistryValueKind.String);
            myKey.SetValue("DefaultPassword", ConfigurationManager.AppSettings["DefaultPassword"], RegistryValueKind.String);
            myKey.SetValue("DefaultDomainName", ConfigurationManager.AppSettings["DefaultDomainName"], RegistryValueKind.String);
            myKey.Close();
            log.Debug("Add autologon to registry ... done");
        }

        /// <summary>
        /// Disable AutoLogon and remove default user from registry
        /// </summary>
        private static void DisableAutoLogon()
        {
            log.Debug("Remove autologon from registry ...");
            RegistryKey myKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\winlogon", true);
            myKey.SetValue("AutoAdminLogon", 0, RegistryValueKind.String);
            myKey.DeleteValue("DefaultUserName", false);
            myKey.DeleteValue("DefaultPassword", false);
            myKey.DeleteValue("DefaultDomainName", false);
            myKey.Close();
            log.Debug("Remove autologon from registry ... done");
        }

        /// <summary>
        /// Set an AutoStart Entry in Registry for this Application
        /// </summary>
        private static void EnableAutoStart()
        {
            log.Debug("Add wbee autostart entry in registry ...");
            RegistryKey myKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\wBee");
            myKey.SetValue("wBee", Assembly.GetExecutingAssembly().GetName().CodeBase, RegistryValueKind.String);
            myKey.Close();
            log.Debug("Add wbee autostart entry in registry ... done");
        }

        /// <summary>
        /// Remove the AutoStart Entry for this Application
        /// </summary>
        private static void DisableAutoStart()
        {
            log.Debug("Remove wbee autostart entry from registry ...");
            Registry.CurrentUser.DeleteSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\wBee", false);
            log.Debug("Remove wbee autostart entry from registry ... done");
        }

        /// <summary>
        /// In Guest Mode no reboots are executed, also no registry changes are done.
        /// Application only return exit co
        /// 
        /// des, the Program that starts wBee musst handle the reboots according to it's exit codes.
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
