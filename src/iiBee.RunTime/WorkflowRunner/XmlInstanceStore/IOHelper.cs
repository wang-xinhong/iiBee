using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace Common {
    /// <summary>
    /// This class is helper to deal with all IO related issues (folders, paths, etc.)
    /// </summary>
    public static class IOHelper {
        public static readonly string InstanceFormatString = "{0}.xml";
        public static readonly string TempDirectory =
            Path.Combine(ConfigurationManager.AppSettings["WorkingDirectory"].ToString(), "WF4DataFolder");

        public static string GetFileName(Guid id) {
            EnsurePersistenceFolderExists();
            return Path.Combine(TempDirectory, 
                                string.Format(CultureInfo.InvariantCulture, InstanceFormatString, id));
        }

        public static string GetAllRfpsFileName() {
            EnsurePersistenceFolderExists();
            return Path.Combine(TempDirectory, "rfps.xml");
        }

        public static string GetTrackingFilePath(Guid instanceId) {
            EnsurePersistenceFolderExists();
            return Path.Combine(TempDirectory, instanceId.ToString() + ".tracking");
        }

        public static void EnsurePersistenceFolderExists() {
            if (!Directory.Exists(TempDirectory)) {
                Directory.CreateDirectory(TempDirectory);
            }
        }

        public static void EnsureAllRfpFileExists() {
            string fileName = IOHelper.GetAllRfpsFileName();
            if (!File.Exists(fileName)) {
                XElement root = new XElement("requestForProposals");
                root.Save(fileName);
            }
        }
    }
}
