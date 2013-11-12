using NLog;
using System;
using System.IO;
using System.Threading;

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
                if (files.Length == 1)
                    return files[0];
                if (files.Length > 0)
                {
                    throw new Exception("There are more than one stored workflow in directory " + _StoreDirectory.FullName);
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
                try
                {
                    StoredWorkflowFile.Delete();
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                }
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
