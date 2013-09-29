using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace iiBee.RunTime.WorkflowHandling.XmlInstanceStore 
{
    /// <summary>
    /// This class is helper to deal with all IO related issues (folders, paths, etc.)
    /// </summary>
    public class IOHelper 
    {
        public static IOHelper Instance = null;

        public readonly string InstanceFormatString = "{0}.xml";
        public readonly string TempDirectory = null;

        private IOHelper(string tempDirectory)
        {
            this.TempDirectory = tempDirectory;
        }

        public static void CreateInstance(string tempDir)
        {
            IOHelper.Instance = new IOHelper(tempDir);
        }

        public string GetFileName(Guid id) 
        {
            EnsurePersistenceFolderExists();
            return Path.Combine(TempDirectory, 
                                string.Format(CultureInfo.InvariantCulture, InstanceFormatString, id));
        }

        public string GetAllRfpsFileName() 
        {
            EnsurePersistenceFolderExists();
            return Path.Combine(TempDirectory, "rfps.xml");
        }

        public string GetTrackingFilePath(Guid instanceId) 
        {
            EnsurePersistenceFolderExists();
            return Path.Combine(TempDirectory, instanceId.ToString() + ".tracking");
        }

        public void EnsurePersistenceFolderExists() 
        {
            if (!Directory.Exists(TempDirectory)) 
            {
                Directory.CreateDirectory(TempDirectory);
            }
        }

        public void EnsureAllRfpFileExists() 
        {
            string fileName = this.GetAllRfpsFileName();
            if (!File.Exists(fileName)) 
            {
                XElement root = new XElement("requestForProposals");
                root.Save(fileName);
            }
        }
    }
}
