using System;
using System.Configuration;
using System.IO;
using Xunit;

namespace iiBee.RunTime.Tests
{
    public class WorkflowRunnerTests
    {
        [Fact]
        public void ExitWithFinishedTest()
        {
            WorkflowRunner wfRunner = new WorkflowRunner(ConfigurationManager.AppSettings["WF4DataFolderDirectory"], 
                new FileInfo(@".\TestResources\SimpleWorkflow.xaml"), false);

            ExitReaction reaction = wfRunner.RunWorkflow();
            Assert.Equal<ExitReaction>(ExitReaction.Finished, reaction);
        }

        [Fact]
        public void ExitWithRebootTest()
        {
            WorkflowRunner wfRunner = new WorkflowRunner(ConfigurationManager.AppSettings["WF4DataFolderDirectory"],
                new FileInfo(@".\TestResources\RebootWorkflow.xaml"), false);

            ExitReaction reaction = wfRunner.RunWorkflow();
            Assert.Equal<ExitReaction>(ExitReaction.Reboot, reaction);
        }

        [Fact]
        public void ExitWithUnhandeledExceptionTest()
        {
            WorkflowRunner wfRunner = new WorkflowRunner(ConfigurationManager.AppSettings["WF4DataFolderDirectory"],
                new FileInfo(@".\TestResources\ExceptionWorkflow.xaml"), false);

            ExitReaction reaction = wfRunner.RunWorkflow();
            Assert.Equal<ExitReaction>(ExitReaction.ErrorExecuting, reaction);
        }

        [Fact]
        public void ExitWithLoadErrorTest()
        {
            WorkflowRunner wfRunner = new WorkflowRunner(ConfigurationManager.AppSettings["WF4DataFolderDirectory"],
                new FileInfo(@".\TestResources\LoadFailedWorkflow.xaml"), false);

            ExitReaction reaction = wfRunner.RunWorkflow();
            Assert.Equal<ExitReaction>(ExitReaction.ErrorLoading, reaction);
        }
    }
}
