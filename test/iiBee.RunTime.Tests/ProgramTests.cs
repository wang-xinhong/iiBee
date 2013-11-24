/*  iiBee - Automation with reboots
    Copyright (C) 2013 AbraxasCSharp (@github)
 */ 

using iiBee.RunTime.WorkflowHandling;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit;

namespace iiBee.RunTime.Tests
{
    public class ProgramTests : IUseFixture<ProgramTests.TestContext>, IDisposable
    {
        private TestContext _Context = null;
        private string ApplicationOutput = string.Empty;

        public void SetFixture(ProgramTests.TestContext data)
        {
            this._Context = data;

            // Clear string builder
            this.ApplicationOutput = string.Empty;

            //Delete Test Data
            if (Directory.Exists(@".\dat\"))
                Directory.Delete(@".\dat\", true);
            if (Directory.Exists(@".\tmp\"))
                Directory.Delete(@".\tmp\", true);
        }
        
        // Test TearDown 
        public void Dispose()
        {
            
        }

        /// <summary>
        /// Starts the console application.
        /// </summary>
        /// <param name="arguments">The arguments for console application.</param>
        /// <returns>exitcode</returns>
        private int StartConsoleApplication(string arguments)
        {
            // Initialize process here
            Process proc = new Process();
            proc.StartInfo.FileName = "wBee.exe";
            // Add arguments as while string
            proc.StartInfo.Arguments = arguments;

            // Use it to start from testing environment
            proc.StartInfo.UseShellExecute = false;

            // Redirect outputs to have it in testing console
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            // Set working directory
            proc.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            // Start and wait for exit
            proc.Start();
            proc.WaitForExit();
            // Get output to testing console.
            string stdout = proc.StandardOutput.ReadToEnd();
            string stderr = proc.StandardError.ReadToEnd();

            this.ApplicationOutput += stdout + Environment.NewLine + stderr;

            Console.WriteLine(stdout);
            Console.Write(stderr);

            // Return exit code
            return proc.ExitCode;
        }

        [Fact]
        public void ShowCmdHelpIfNoArgumentsTest()
        {
            // Check exit is normal
            Assert.Equal<int>(
                ExitCodes.HaveDoneNothing, 
                StartConsoleApplication(""));

            // Check that help information shown correctly.
            Assert.True(this.ApplicationOutput.Contains("Copyright"));
        }

        [Fact]
        public void StartExecutionOfSimpleWorkflowTest()
        {
            Assert.Equal<int>(
                ExitCodes.FinishedSuccessfully, 
                StartConsoleApplication("run -f \".\\TestResources\\SimpleWorkflow.xaml\""));

            Assert.True(this.ApplicationOutput.Contains("Finished with SimpleWorkflow"));
        }

        [Fact]
        public void StartExecutionOfInputWorkflowTest()
        {
            Assert.Equal<int>(
                ExitCodes.FinishedSuccessfully,
                StartConsoleApplication("run -f \".\\TestResources\\InputWorkflow.xaml\" -i \"{'IntInput':'10', 'BoolInput':'true', 'StringInput':'Test' }\""));

            Assert.True(this.ApplicationOutput.Contains("Int=10"), "Int Input is not 10");
            Assert.True(this.ApplicationOutput.Contains("Bool=True"), "Bool Input is not True");
            Assert.True(this.ApplicationOutput.Contains("String=Test"), "String Input is not \"Test\"");
        }

        [Fact]
        public void StartExecutionOfRebootWorkflowTest()
        {
            Assert.Equal<int>(
                ExitCodes.ClosedForReboot,
                StartConsoleApplication("run -f \".\\TestResources\\RebootWorkflow.xaml\""));
            Assert.True(this.ApplicationOutput.Contains("Started RebootWorkflow"));

            Assert.Equal<int>(
                ExitCodes.FinishedSuccessfully,
                StartConsoleApplication("resume"));
            Assert.True(this.ApplicationOutput.Contains("Finished with RebootWorkflow"));
        }

        [Fact]
        public void ResumeOfWorkflowFailsWithMessageTest()
        {
            Assert.Equal<int>(
                ExitCodes.HaveDoneNothing,
                StartConsoleApplication("resume"));
            Assert.True(this.ApplicationOutput.Contains("There is nothing to resume"));
        }

        public class TestContext : IDisposable
        {
            // One Time Setup
            public TestContext()
            {

            }

            // One Time Teardown
            public void Dispose()
            {
                
            }
        }
    }
}
