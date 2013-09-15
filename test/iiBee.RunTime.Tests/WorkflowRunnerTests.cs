using System;
using System.IO;
using Xunit;

namespace iiBee.RunTime.Tests
{
    public class WorkflowRunnerTests
    {
        [Fact]
        public void ExitWithFinishedTest()
        {
            WorkflowRunner wfRunner = new WorkflowRunner(@".\Data\", 
                new FileInfo(@".\TestResources\SimpleWorkflow.xaml"));

            ExitReaction reaction = wfRunner.RunWorkflow();
            Assert.Equal<ExitReaction>(ExitReaction.Finished, reaction);
        }

        [Fact]
        public void ExitWithRebootTest()
        {
            WorkflowRunner wfRunner = new WorkflowRunner(@".\Data\",
                new FileInfo(@".\TestResources\RebootWorkflow.xaml"));

            ExitReaction reaction = wfRunner.RunWorkflow();
            Assert.Equal<ExitReaction>(ExitReaction.Reboot, reaction);
        }

        [Fact]
        public void ExitWithUnhandeledExceptionTest()
        {
            WorkflowRunner wfRunner = new WorkflowRunner(@".\Data\",
                new FileInfo(@".\TestResources\ExceptionWorkflow.xaml"));

            ExitReaction reaction = wfRunner.RunWorkflow();
            Assert.Equal<ExitReaction>(ExitReaction.ErrorExecuting, reaction);
        }

        [Fact]
        public void ExitWithLoadErrorTest()
        {
            WorkflowRunner wfRunner = new WorkflowRunner(@".\Data\",
                new FileInfo(@".\TestResources\LoadFailedWorkflow.xaml"));

            ExitReaction reaction = wfRunner.RunWorkflow();
            Assert.Equal<ExitReaction>(ExitReaction.ErrorLoading, reaction);
        }
    }
}
