using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iiBee.RunTime
{
    public class WorkflowRunner
    {
        public WorkflowRunner(FileInfo workflow)
        {
            //TODO: Load Workflow here
        }

        public ExitReaction RunWorkflow()
        {
            //TODO: Add Code to execute Workflow here
            return ExitReaction.Finished;
        }
    }
}
