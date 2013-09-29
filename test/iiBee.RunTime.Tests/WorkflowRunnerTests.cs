using iiBee.RunTime.WorkflowHandling;
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
    }
}
