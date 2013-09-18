using iiBee.RunTime.Library.Activities;
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
    public class WorkflowRunner
    {
        private DirectoryInfo _WorkingDirectory = null;
        private Guid _WorkflowId = new Guid("50863C72-3ABA-4631-8995-0ACA0385B7A3");
        private WorkflowApplication _WorkflowApp = null;
        private bool _IsResumedWorkflow = false;

        public WorkflowRunner(string dataFolder, FileInfo workflow, bool resume = false)
        {
            _WorkingDirectory = new DirectoryInfo(
                dataFolder + "WF4DataFolder");

            _IsResumedWorkflow = resume;

            if (!resume && _WorkingDirectory.Exists)
                _WorkingDirectory.Delete(true);

            _WorkingDirectory.Create();

            DynamicActivity wf = LoadWorkflow(workflow.FullName);
            _WorkflowApp = new WorkflowApplication(wf);
            _WorkflowApp.InstanceStore = SetupXmlpersistenceStore(_WorkflowId);
        }

        public ExitReaction RunWorkflow()
        {
            ExitReaction ret = ExitReaction.Finished;
            _WorkflowApp.PersistableIdle = (e) =>
                {
                    if (SetRebootFlag.RebootPending)
                    {
                        ret = ExitReaction.Reboot;
                        return PersistableIdleAction.Unload;
                    }
                    else
                        return PersistableIdleAction.None;
                };

            AutoResetEvent waitHandler = new AutoResetEvent(false);
            _WorkflowApp.Unloaded = (e) =>
                {
                    waitHandler.Set();
                };

            _WorkflowApp.OnUnhandledException = (e) =>
                {
                    ret = ExitReaction.ErrorExecuting;
                    waitHandler.Set();
                    return UnhandledExceptionAction.Abort;
                };

            if (_IsResumedWorkflow)
            {
                try { _WorkflowApp.Load(_WorkflowId); }
                catch { return ExitReaction.ErrorLoadingFromInstanceStore; }
            }
            _WorkflowApp.Run();
            waitHandler.WaitOne();

            return ret;
        }

        private static XmlWorkflowInstanceStore SetupXmlpersistenceStore(Guid workflowId)
        {
            XmlWorkflowInstanceStore instanceStore = new XmlWorkflowInstanceStore(workflowId);
            InstanceHandle handle = instanceStore.CreateInstanceHandle();
            InstanceView view = instanceStore.Execute(handle,
                                                    new CreateWorkflowOwnerCommand(),
                                                    TimeSpan.FromSeconds(5));
            handle.Free();
            instanceStore.DefaultInstanceOwner = view.InstanceOwner;
            return instanceStore;
        }

        private static DynamicActivity LoadWorkflow(string workflow)
        {
            XamlXmlReaderSettings settings = new XamlXmlReaderSettings();
            settings.LocalAssembly = Assembly.GetExecutingAssembly();

            using (XamlXmlReader reader = new XamlXmlReader(workflow, settings))
            {
                return ActivityXamlServices.Load(reader) as DynamicActivity;
            }
        }
    }
}