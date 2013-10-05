using NLog;
using System.IO;

namespace iiBee.RunTime
{
    public class TemporaryStorage
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        private DirectoryInfo _StoreDirectory = null;

        public TemporaryStorage(DirectoryInfo storageDir)
        {
            _StoreDirectory = storageDir;
            _StoreDirectory.Create();
        }

        /// <summary>
        /// Get Location of the stored Workflow
        /// </summary>
        public FileInfo StoredWorkflowFile
        {
            get 
            {
                FileInfo[] files = _StoreDirectory.GetFiles("*.xaml", SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                {
                    return files[0];
                }
                else
                {
                    return new FileInfo(Path.Combine(_StoreDirectory.FullName, "NOWORKFLOWFOUND.xaml"));
                }
            }
        }

        /// <summary>
        /// Stores an workflow to an save location
        /// </summary>
        /// <param name="wf">path to existing wf</param>
        public void StoreWorkflow(FileInfo wf)
        {
            RemoveStoredWorkflow();

            string filename = Path.Combine(_StoreDirectory.FullName, wf.Name);
            log.Debug("Storing " + wf.FullName + " to " + filename);
            wf.CopyTo(filename, true);
        }

        /// <summary>
        /// Removes workflow from save location
        /// </summary>
        public void RemoveStoredWorkflow()
        {
            if (WorkflowIsStored())
            {
                log.Debug("Deleting stored Workflow");
                StoredWorkflowFile.Delete();
            }
        }

        /// <summary>
        /// true if an workflow is stored
        /// </summary>
        public bool WorkflowIsStored()
        {
            return StoredWorkflowFile.Exists;
        }
    }
}
