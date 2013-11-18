using iiBee.RunTime.Helper;
using iiBee.RunTime.WorkflowHandling;
using Microsoft.Win32;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace iiBee.RunTime
{
    public class Program
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        private TemporaryStorage _Storage = null;

        static void Main(string[] args)
        {
            Program prog = new Program();
            prog.Run(args);
        }

        /// <summary>
        /// Initialize the program
        /// </summary>
        public Program()
        {
            _Storage = new TemporaryStorage(new DirectoryInfo(
            ConfigurationManager.AppSettings["TemporaryStorage"]));
        }

        /// <summary>
        /// Runs the application to execute an workflow
        /// </summary>
        /// <param name="args">Arguments to start an workflow</param>
        public void Run(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            LoadWorkflowLibraryExtensions();

            string invokedVerb = null;
            object invokedVerbInstance = null;

            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options,
              (verb, subOptions) =>
              {
                  // if parsing succeeds the verb name and correct instance
                  // will be passed to onVerbCommand delegate (string,object)
                  invokedVerb = verb;
                  invokedVerbInstance = subOptions;
              }))
            {
                Environment.Exit(ExitCodes.HaveDoneNothing);
            }

            WorkflowRunner wfRunner = null;

            if (invokedVerb == "run")
            {
                var runSubOptions = (RunSubOptions)invokedVerbInstance;

                Dictionary<string, string> inputs = null;
                if (runSubOptions.InputParameters != null)
                {
                    try
                    {
                        inputs = JsonConvert.DeserializeObject<Dictionary<string, string>>(runSubOptions.InputParameters);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Failed to convert input paramters: " + ex.Message);
                        Environment.Exit(ExitCodes.InputError);
                    }
                }
                FileInfo workflow = new FileInfo(runSubOptions.WorkingFile);
                wfRunner = CreateWorkflowRunnerForStart(workflow, inputs);
            }
            else if (invokedVerb == "resume")
            {
                var resumeSubOptions = (ResumeSubOptions)invokedVerbInstance;

                wfRunner = CreateWorkflowRunnerForResume();
            }

            StartWorkflow(wfRunner);
        }

        /// <summary>
        /// Load Assemblies for Worklow Runner
        /// </summary>
        private void LoadWorkflowLibraryExtensions()
        {
            DirectoryInfo extDirectory = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["WorkflowLibraries"]));
            if (!extDirectory.Exists)
                return;

            WorkflowRunner.LoadAssemblies(extDirectory.GetFiles("*.dll", SearchOption.AllDirectories));
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            log.Fatal(ex.Message);
            Environment.Exit(ExitCodes.ApplicationError);
        }

        private WorkflowRunner CreateWorkflowRunnerForStart(FileInfo workflowFile, Dictionary<string, string> inputs)
        {
            WorkflowRunner wfRunner = null;
            _Storage.StoreWorkflow(workflowFile);
            Dictionary<string, object> parameters = GetInputArguments(_Storage.StoredWorkflowFile, inputs);
            wfRunner = new WorkflowRunner(_Storage.StoredWorkflowFile, false, parameters);

            return wfRunner;
        }

        private WorkflowRunner CreateWorkflowRunnerForResume()
        {
            WorkflowRunner wfRunner = null;

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

            FileInfo wfFile = _Storage.StoredWorkflowFile;
            if (!wfFile.Exists)
            {
                log.Warn("There is nothing to resume");
                Environment.Exit(ExitCodes.HaveDoneNothing);
            }
            log.Info("Found persisted workflow file[" + wfFile.FullName + "]");

            wfRunner = new WorkflowRunner(wfFile, true);

            return wfRunner;
        }

        private void StartWorkflow(WorkflowRunner wfRunner)
        {
            ExitReaction reaction = wfRunner.RunWorkflow();
            if (reaction == ExitReaction.Reboot)
            {
                log.Info("Preparing for system reboot");
                SendRebootCommand();
                Environment.Exit(ExitCodes.ClosedForReboot);
            }
            else if (reaction == ExitReaction.Finished)
            {
                _Storage.RemoveStoredWorkflow();

                log.Info("Finished workflow, application is stopping");
                Environment.Exit(ExitCodes.FinishedSuccessfully);
            }
        }

        private Dictionary<string, object> GetInputArguments(FileInfo fileInfo, Dictionary<string, string> dictionary)
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
        private void SendRebootCommand()
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
        private void EnableAutoLogon()
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
        private void DisableAutoLogon()
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
        private void EnableAutoStart()
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
        private void DisableAutoStart()
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
        private bool InGuestMode()
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
