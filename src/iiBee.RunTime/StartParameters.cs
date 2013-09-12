using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iiBee.RunTime
{
    public class StartParameters
    {
        public StartParameters(string[] args)
        {
            if (args.Count() < 1)
            {
                HasParameters = false;
            }
            else
            {
                HasParameters = true;
                WorkflowFile = new FileInfo(args[0]);
            }
        }

        public bool HasParameters { private set; get; }
        public FileInfo WorkflowFile { private set; get; }
    }
}
