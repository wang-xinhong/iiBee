using NLog;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;

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

            WorkflowRunner wfRunner = null;
            if (storage.WorkflowIsStored())
            {
                FileInfo wfFile = storage.StoredWorkflowFile;
                wfRunner = new WorkflowRunner(ConfigurationManager.AppSettings["WorkingDirectory"], wfFile, true);
            }
            else if (startParams.HasParameters)
            {
                wfRunner = new WorkflowRunner(ConfigurationManager.AppSettings["WorkingDirectory"], startParams.WorkflowFile, false);
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

            //TODO: Add logic for #6 here

            string rebootArgs = "-r -f -t 10";
            log.Debug("Starting reboot with args[" + rebootArgs + "]");
            Process.Start("shutdown", rebootArgs);
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
