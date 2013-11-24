/*  iiBee - Automation with reboots
    Copyright (C) 2013 AbraxasCSharp (@github)
 */ 

using iiBee.RunTime.WorkflowHandling;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Xunit;

namespace iiBee.RunTime.Tests
{
    public class WorkflowRunnerTests
    {
        public string WorkingDir = ConfigurationManager.AppSettings["WorkingDirectory"];

        [Fact]
        public void ExitWithFinishedTest()
        {
            WorkflowRunner wfRunner = new WorkflowRunner( 
                new FileInfo(@".\TestResources\SimpleWorkflow.xaml"), false);

            ExitReaction reaction = wfRunner.RunWorkflow();
            Assert.Equal<ExitReaction>(ExitReaction.Finished, reaction);
        }

        [Fact]
        public void ExitWithRebootTest()
        {
            WorkflowRunner wfRunner = new WorkflowRunner(
                new FileInfo(@".\TestResources\RebootWorkflow.xaml"), false);

            ExitReaction reaction = wfRunner.RunWorkflow();
            Assert.Equal<ExitReaction>(ExitReaction.Reboot, reaction);
        }

        [Fact]
        public void ExitWithUnhandeledExceptionTest()
        {
            WorkflowRunner wfRunner = new WorkflowRunner(
                new FileInfo(@".\TestResources\ExceptionWorkflow.xaml"), false);

            ExitReaction reaction = wfRunner.RunWorkflow();
            Assert.Equal<ExitReaction>(ExitReaction.ErrorExecuting, reaction);
        }

        [Fact]
        public void ExitWithLoadErrorTest()
        {
            WorkflowRunner wfRunner = new WorkflowRunner(
                new FileInfo(@".\TestResources\RebootWorkflow.xaml"), false);
            ExitReaction reaction = wfRunner.RunWorkflow();
            Assert.Equal<ExitReaction>(ExitReaction.Reboot, reaction);

            //Destroy Instance Store for Test
            Directory.Delete(WorkingDir, true);

            wfRunner = new WorkflowRunner(
                new FileInfo(@".\TestResources\RebootWorkflow.xaml"), true);
            reaction = wfRunner.RunWorkflow();
            Assert.Equal<ExitReaction>(ExitReaction.ErrorLoadingFromInstanceStore, reaction);
        }

        [Fact]
        public void FinishRebootWorkflowTest()
        {
            WorkflowRunner wfRunner = new WorkflowRunner(
                new FileInfo(@".\TestResources\RebootWorkflow.xaml"), false);
            ExitReaction reaction = wfRunner.RunWorkflow();
            Assert.Equal<ExitReaction>(ExitReaction.Reboot, reaction);

            wfRunner = new WorkflowRunner(
                new FileInfo(@".\TestResources\RebootWorkflow.xaml"), true);
            reaction = wfRunner.RunWorkflow();
            Assert.Equal<ExitReaction>(ExitReaction.Finished, reaction);
        }

        [Fact]
        public void InputReachWorkflowTest()
        {
            IDictionary<string, object> input = new Dictionary<string, object>()
            {
                { "IntInput" , 100 },
                { "StringInput" , "HelloWorld" },
                { "BoolInput" , true }
            };

            WorkflowRunner wfRunner = new WorkflowRunner(
                new FileInfo(@".\TestResources\InputWorkflow.xaml"), false, input);
            
            StringWriter writer = new StringWriter();
            wfRunner.WorkflowApp.Extensions.Add(writer);

            ExitReaction reaction = wfRunner.RunWorkflow();
            Assert.Equal<ExitReaction>(ExitReaction.Finished, reaction);

            wfRunner.WorkflowApp.Completed += (e) =>
            {
                Assert.Equal<int>(100, (int)e.Outputs["IntOutput"]);
                Assert.Equal<string>("HelloWorld", (string)e.Outputs["StringOutput"]);
                Assert.Equal<bool>(true, (bool)e.Outputs["BoolOutput"]);
            };
        }
    }
}
