//Used this Source: http://www.codeproject.com/Articles/17652/How-to-Test-Console-Applications

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

        public void SetFixture(ProgramTests.TestContext data)
        {
            this._Context = data;

            // Clear string builder
            _Context.TestingSB.Remove(0, _Context.TestingSB.Length);
        }
        
        // Test TearDown 
        public void Dispose()
        {
            // Verbose output in console
            _Context.NormalOutput.Write(_Context.TestingSB.ToString());
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
            Console.WriteLine(proc.StandardOutput.ReadToEnd());
            Console.Write(proc.StandardError.ReadToEnd());

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
            Assert.True(_Context.TestingSB.ToString().Contains(new Options().GetUsage()));
        }

        [Fact]
        public void StartExecutionOfSimpleWorkflowTest()
        {
            Assert.Equal<int>(
                ExitCodes.FinishedSuccessfully, 
                StartConsoleApplication("run -f \".\\TestResources\\SimpleWorkflow.xaml\""));

            Assert.True(_Context.TestingSB.ToString().Contains("Finished with SimpleWorkflow"));
        }

        [Fact]
        public void StartExecutionOfInputWorkflowTest()
        {
            Assert.Equal<int>(
                ExitCodes.FinishedSuccessfully,
                StartConsoleApplication("run -f \".\\TestResources\\InputWorkflow.xaml\" -i \"{'IntInput':'10', 'BoolInput':'true', 'StringInput':'Test' }\""));

            Assert.True(_Context.TestingSB.ToString().Contains("Int=10"));
            Assert.True(_Context.TestingSB.ToString().Contains("Bool=true"));
            Assert.True(_Context.TestingSB.ToString().Contains("String=Test"));
        }

        [Fact]
        public void StartExecutionOfRebootWorkflowTest()
        {
            Assert.Equal<int>(
                ExitCodes.ClosedForReboot,
                StartConsoleApplication("run -f \".\\TestResources\\RebootWorkflow.xaml\""));
            Assert.True(_Context.TestingSB.ToString().Contains("Started RebootWorkflow"));

            Assert.Equal<int>(
                ExitCodes.FinishedSuccessfully,
                StartConsoleApplication("resume"));
            Assert.True(_Context.TestingSB.ToString().Contains("Finished with RebootWorkflow"));
        }

        [Fact]
        public void ResumeOfWorkflowFailsWithMessageTest()
        {
            Assert.Equal<int>(
                ExitCodes.HaveDoneNothing,
                StartConsoleApplication("resume"));
            Assert.True(_Context.TestingSB.ToString().Contains("There is nothing to resume"));
        }

        public class TestContext : IDisposable
        {
            public TextWriter NormalOutput;
            public StringWriter TestingConsole;
            public StringBuilder TestingSB;

            // One Time Setup
            public TestContext()
            {
                // Set current folder to testing folder
                string assemblyCodeBase =
                    Assembly.GetExecutingAssembly().CodeBase;

                // Get directory name
                string dirName = Path.GetDirectoryName(assemblyCodeBase);

                // Remove URL-prefix if it exists
                if (dirName.StartsWith("file:\\"))
                    dirName = dirName.Substring(6);

                // Set current folder
                Environment.CurrentDirectory = dirName;

                // Initialize string builder to replace console
                this.TestingSB = new StringBuilder();
                this.TestingConsole = new StringWriter(this.TestingSB);

                //swap normal output console with testing console - to reuse
                // it later
                this.NormalOutput = Console.Out;
                Console.SetOut(this.TestingConsole);
            }

            // One Time Teardown
            public void Dispose()
            {
                Console.SetOut(this.NormalOutput);
            }
        }
    }
}
