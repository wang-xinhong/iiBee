﻿using iiBee.RunTime.Library.Activities;
using iiBee.RunTime.WorkflowHandling.Extensions;
using iiBee.RunTime.WorkflowHandling.XmlInstanceStore;
using NLog;
using System;
using System.Activities;
using System.Activities.DurableInstancing;
using System.Activities.Tracking;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.DurableInstancing;
using System.Threading;
using System.Xaml;

namespace iiBee.RunTime.WorkflowHandling
{
    public class WorkflowRunner
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        private DirectoryInfo _WorkingDirectory = null;
        private Guid _WorkflowId = Guid.Empty;
        private WorkflowApplication _WorkflowApp = null;
        private bool _IsResumedWorkflow = false;

        public WorkflowApplication WorkflowApp
        {
            get
            {
                return _WorkflowApp;
            }
        }

        public WorkflowRunner(FileInfo workflow, bool resume = false, IDictionary<string, object> input = null)
        {
            log.Trace("Constructor WorkflowRunner ...");
            log.Trace("Parameters: workflow[" + workflow.FullName + "], resume[" + resume.ToString() + "]");

            _WorkingDirectory = new DirectoryInfo(
                ConfigurationManager.AppSettings["WorkingDirectory"] + "WF4DataFolder");
            IOHelper.CreateInstance(_WorkingDirectory.FullName);

            _IsResumedWorkflow = resume;

            if (!resume && _WorkingDirectory.Exists)
            {
                log.Debug("Deleting WorkingDirectory[" + _WorkingDirectory.FullName + "]");
                _WorkingDirectory.Delete(true);
            }
            else if (resume && _WorkingDirectory.Exists)
            {
                _WorkflowId = Guid.Parse(
                    Path.GetFileNameWithoutExtension(
                    _WorkingDirectory.GetFiles().First().Name));
            }

            if (!_WorkingDirectory.Exists)
            {
                log.Debug("Creating WorkingDirectory[" + _WorkingDirectory.FullName + "]");
                _WorkingDirectory.Create();
            }
            
            DynamicActivity wf = LoadWorkflow(workflow.FullName);
            if (input != null)
                _WorkflowApp = new WorkflowApplication(wf, input);
            else
                _WorkflowApp = new WorkflowApplication(wf);
            log.Debug("Workflow App created");

            // Add Extension to make activity WriteLine write to log file and console.
            _WorkflowApp.Extensions.Add(new LogWriter());
            _WorkflowApp.Extensions.Add(CreateLogParticipant());
            log.Debug("Added Extensions");
            
            if(_WorkflowId == Guid.Empty)
                _WorkflowId = _WorkflowApp.Id;
            _WorkflowApp.InstanceStore = SetupXmlpersistenceStore(_WorkflowId);
            log.Debug("Added Persistence Store");

            log.Trace("Constructor WorkflowRunner ... done");
        }

        public static void LoadAssemblies(FileInfo[] assemblies)
        {
            foreach (FileInfo assembly in assemblies)
            {
                Assembly.LoadFrom(assembly.FullName);
            }
        }

        public ExitReaction RunWorkflow()
        {
            ExitReaction ret = ExitReaction.Finished;
            _WorkflowApp.PersistableIdle = (e) =>
                {
                    if (SetRebootFlag.RebootPending)
                    {
                        SetRebootFlag.RebootPending = false;
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
                    log.Error(e.UnhandledException.Message);
                    waitHandler.Set();
                    return UnhandledExceptionAction.Abort;
                };

            if (_IsResumedWorkflow)
            {
                try { _WorkflowApp.Load(_WorkflowId); }
                catch (Exception ex)
                {
                    log.Error("An error occured while loading the last Workflow from instance store: " + ex.Message);
                    return ExitReaction.ErrorLoadingFromInstanceStore; 
                }
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
            log.Debug("Loading Workflow ...");
            XamlXmlReaderSettings settings = new XamlXmlReaderSettings();
            settings.LocalAssembly = Assembly.GetExecutingAssembly();

            ActivityXamlServicesSettings activitySettings = new ActivityXamlServicesSettings
            {
                CompileExpressions = true
            };

            using (XamlXmlReader reader = new XamlXmlReader(workflow, settings))
            {
                DynamicActivity activity = ActivityXamlServices.Load(reader, activitySettings) as DynamicActivity;
                log.Debug("Loading Workflow ... done");

                return activity;
            }
            
        }

        private static LogParticipant CreateLogParticipant()
        {
            LogParticipant tracking = new LogParticipant()
            {
                TrackingProfile = new TrackingProfile()
                {
                    Name = "LogTrackingProfile",
                    Queries =
                    {
                        // For instance data, only track the started and completed events
                        new WorkflowInstanceQuery()
                        {
                            States =
                            {
                                WorkflowInstanceStates.Started,
                                WorkflowInstanceStates.Resumed,
                                WorkflowInstanceStates.Unloaded,
                                WorkflowInstanceStates.Completed
                            }
                        },

                        // Log all bookmarks
                        new BookmarkResumptionQuery()
                        {
                            Name = "*"
                        },

                        //Log all Activities
                        new ActivityStateQuery()
                        {
                            ActivityName = "*",
                            States = { "*" }
                        },

                        new CustomTrackingQuery()
                        {
                            Name= "*",
                            ActivityName = "*"
                        }
                    }
                }
            };

            return tracking;
        }
    }
}