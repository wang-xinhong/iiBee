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
                Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "StoredWF.xaml")));

        static void Main(string[] args)
        {
            StartParameters startParams = new StartParameters(args);

            WorkflowRunner wfRunner = null;
            if (storage.WorkflowIsStored())
            {
                FileInfo wfFile = storage.StoredWorkflowFile;
                wfRunner = new WorkflowRunner(ConfigurationManager.AppSettings["WF4DataFolderDirectory"], wfFile, true);
            }
            else if (startParams.HasParameters)
            {
                wfRunner = new WorkflowRunner(ConfigurationManager.AppSettings["WF4DataFolderDirectory"], startParams.WorkflowFile, false);
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

            //TODO: Move this code to WorkflowRunner
            //Generate Instance Store
        //    DirectoryInfo workingDir = new DirectoryInfo(ConfigurationManager.AppSettings["WF4DataFolderDirectory"] + "WF4DataFolder");
        //    FileInfo rebootFile = new FileInfo(".\\reboot");
        //    string workflowfile = ".\\StartAllSySeTestsWorkflow.xaml";

        //    //Create Workflow
        //    if (!rebootFile.Exists)
        //    {
        //        Console.WriteLine("Press <enter> to start the workflow");
        //        Console.ReadLine();

        //        foreach (FileInfo file in workingDir.GetFiles())
        //        {
        //            file.Delete();
        //        }

        //        AutoResetEvent waitHandler = new AutoResetEvent(false);
        //        DynamicActivity wf = LoadWorkflow(workflowfile);
        //        WorkflowApplication wfApp = new WorkflowApplication(wf);
        //        XmlWorkflowInstanceStore instanceStore = SetupXmlpersistenceStore(wfApp.Id);

        //        wfApp.InstanceStore = instanceStore;
        //        ///persists application state and remove it from memory    
        //        wfApp.PersistableIdle = (e) =>
        //        {
        //            if (SetRebootFlag.RebootPending)
        //            {
        //                rebootFile.Create().Close();
        //                return PersistableIdleAction.Unload;
        //            }
        //            else
        //                return PersistableIdleAction.None;
        //        };
        //        wfApp.Unloaded = (e) =>
        //        {
        //            Console.WriteLine("unload");
        //            waitHandler.Set();
        //        };
        //        wfApp.Run();
        //        waitHandler.WaitOne();
        //    }
        //    else
        //    {
        //        Console.WriteLine("Press <enter> to continue the workflow");
        //        Console.ReadLine();

        //        rebootFile.Delete();
        //        FileInfo file = workingDir.GetFiles().Single();
        //        string guidstring = Path.GetFileNameWithoutExtension(file.FullName);
        //        Guid id = new Guid(guidstring);

        //        AutoResetEvent waitHandler = new AutoResetEvent(false);
        //        Activity wf = LoadWorkflow(workflowfile);
        //        WorkflowApplication wfApp = new WorkflowApplication(wf);
        //        wfApp.InstanceStore = SetupXmlpersistenceStore(id);

        //        wfApp.Completed = (workflowApplicationCompletedEventArgs) =>
        //        {
        //            Console.WriteLine("\nWorkflowApplication has Completed in the {0} state.", workflowApplicationCompletedEventArgs.CompletionState);
        //        };
        //        wfApp.PersistableIdle = (e) =>
        //        {
        //            if (SetRebootFlag.RebootPending)
        //            {
        //                rebootFile.Create().Close();
        //                return PersistableIdleAction.Unload;
        //            }
        //            else
        //                return PersistableIdleAction.None;
        //        };
        //        wfApp.Unloaded = (workflowApplicationEventArgs) =>
        //        {
        //            Console.WriteLine("WorkflowApplication has Unloaded\n");
        //            waitHandler.Set();
        //        };

        //        wfApp.Load(id);
        //        wfApp.Run();
        //        waitHandler.WaitOne();
        //    }

        //    if (SetRebootFlag.RebootPending)
        //        SendRebootCommand();
        //}
        
        private static void SendRebootCommand()
        {
            Process.Start("shutdown", "-r -f -t 10");
        }

        ///// <summary>
        ///// Start And Unload Instance
        ///// </summary>
        //static void StartAndUnloadInstance()
        //{
        //    AutoResetEvent waitHandler = new AutoResetEvent(false);
        //    WorkflowApplication wfApp = new WorkflowApplication(new Workflow1());
        //    XmlWorkflowInstanceStore instanceStore =
        //                              SetupXmlpersistenceStore(wfApp.Id);

        //    wfApp.InstanceStore = instanceStore;

        //    ///persists application state and remove it from memory    
        //    wfApp.PersistableIdle = (e) =>
        //    {
        //        if (Activities.SetRebootFlag.RebootRequested)
        //            return PersistableIdleAction.Unload;
        //        else
        //            return PersistableIdleAction.None;
        //    };

        //    wfApp.Unloaded = (e) =>
        //    {
        //        Console.WriteLine("unload");
        //        waitHandler.Set();
        //    };

        //    Guid id = wfApp.Id;

        //    wfApp.Run();
        //    waitHandler.WaitOne();
        //    LoadAndCompleteInstance(id, instanceStore);
        //}

        //static void LoadAndCompleteInstance(Guid id, InstanceStore instanceStore)
        //{
        //    AutoResetEvent waitHandler = new AutoResetEvent(false);
        //    Console.WriteLine("Press <enter> to load the persisted workflow");
        //    Console.ReadLine();
        //    WorkflowApplication wfApp = new WorkflowApplication(new Workflow1());
        //    wfApp.InstanceStore = instanceStore;

        //    wfApp.Completed = (workflowApplicationCompletedEventArgs) =>
        //    {
        //        Console.WriteLine("\nWorkflowApplication has Completed in the {0} state.", workflowApplicationCompletedEventArgs.CompletionState);
        //    };

        //    wfApp.Unloaded = (workflowApplicationEventArgs) =>
        //    {
        //        Console.WriteLine("WorkflowApplication has Unloaded\n");
        //        waitHandler.Set();
        //    };

        //    wfApp.Load(id);
        //    wfApp.Run();
        //    waitHandler.WaitOne();
        //}       
    }
}
