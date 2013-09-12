using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iiBee.RunTime
{
    public class TemporaryStorage
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        private FileInfo _StoredFile = null;

        public TemporaryStorage(FileInfo storageFile)
        {
            _StoredFile = storageFile;
        }

        /// <summary>
        /// Get Location of the stored Workflow
        /// </summary>
        public FileInfo StoredWorkflowFile
        {
            get { return _StoredFile; }
        }

        /// <summary>
        /// Stores an workflow to an save location
        /// </summary>
        /// <param name="wf">path to existing wf</param>
        public void StoreWorkflow(FileInfo wf)
        {
            RemoveStoredWorkflow();

            log.Debug("Storing " + wf.FullName + " to " + _StoredFile.FullName);
            wf.CopyTo(wf.FullName);
        }

        /// <summary>
        /// Removes workflow from save location
        /// </summary>
        public void RemoveStoredWorkflow()
        {
            if (WorkflowIsStored())
            {
                log.Debug("Deleting stored Workflow");
                _StoredFile.Delete();
            }
        }

        /// <summary>
        /// true if an workflow is stored
        /// </summary>
        public bool WorkflowIsStored()
        {
            return _StoredFile.Exists;
        }
    }
}
