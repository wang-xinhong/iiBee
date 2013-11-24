/*  iiBee - Automation with reboots
    Copyright (C) 2013 AbraxasCSharp (@github)
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace iiBee.RunTime
{
    public class Options
    {
        [VerbOption("run", HelpText="Run an workflow.")]
        public RunSubOptions RunVerb { get; set; }

        [VerbOption("resume", HelpText = "Resume an rebooted workflow.")]
        public ResumeSubOptions ResumeVerb { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            return HelpText.AutoBuild(this, verb);
        }
    }

    public class RunSubOptions
    {
        [Option('f', "file", HelpText = "Workflow xaml file to execute.", Required = true)]
        public string WorkingFile { get; set; }

        [Option('i', "input", HelpText = "Set input of workflow. Example: \"{'name':'value', 'text':'value'}\"")]
        public string InputParameters { get; set; }
    }

    public class ResumeSubOptions
    {

    }
}
