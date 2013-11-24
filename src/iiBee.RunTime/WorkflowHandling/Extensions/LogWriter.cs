/*  iiBee - Automation with reboots
    Copyright (C) 2013 AbraxasCSharp (@github)
 */ 

using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iiBee.RunTime.WorkflowHandling.Extensions
{
    /// <summary>
    /// Makes sure that WriteLine Activity writes to Console and LogFile
    /// </summary>
    public class LogWriter : TextWriter
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public override void WriteLine(string value)
        {
            log.Info(value);
        }

        public override Encoding Encoding
        {
            get
            {
                return Encoding.Default;
            }
        }
    }
}
