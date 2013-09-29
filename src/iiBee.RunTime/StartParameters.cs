using System.Configuration;
using System.IO;
using System.Linq;

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

            this.GuestMode = (ConfigurationManager.AppSettings["Guest"] == "true") ? true : false;
        }

        public bool HasParameters { private set; get; }
        public FileInfo WorkflowFile { private set; get; }
        public bool GuestMode { private set; get; }
    }
}
