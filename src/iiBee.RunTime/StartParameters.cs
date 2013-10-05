using iiBee.RunTime.Helper;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace iiBee.RunTime
{
    public class StartParameters
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public StartParameters(string[] args)
        {
            this.InputArguments = null;

            if (args.Count() < 1)
            {
                HasParameters = false;
                return;
            }
            if(args.Count() >= 1)
            {
                HasParameters = true;
                WorkflowFile = new FileInfo(args[0]);
            }
            if( args.Count() >= 2)
            {
                try
                {
                    this.InputArguments = JsonConvert.DeserializeObject<Dictionary<string, string>>(args[1]);
                }
                catch (Exception ex)
                {
                    log.Error("Failed to parse Input Args: " + ex.Message);
                }
            }
        }

        public bool HasParameters { private set; get; }
        public FileInfo WorkflowFile { private set; get; }
        public Dictionary<string, string> InputArguments { private set; get; }
    }
}
