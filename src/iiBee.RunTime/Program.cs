using iiBee.RunTime.Library.Activities;
using NLog;
using System;
using System.Activities;
using System.Activities.DurableInstancing;
using System.Activities.XamlIntegration;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.DurableInstancing;
using System.Threading;
using System.Xaml;
using WF4Samples.WF4Persistence;

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
        
        private static void SendRebootCommand()
        {
            Process.Start("shutdown", "-r -f -t 10");
        }
    }
}
