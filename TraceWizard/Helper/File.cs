using System;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using System.Diagnostics;

using TraceWizard.Logging.Adapters.MeterMasterJet;
using TraceWizard.Logging.Adapters.MeterMasterCsv;
using TraceWizard.Logging.Adapters.Telematics;
using TraceWizard.Entities.Adapters.Tw4Jet;
using TraceWizard.Entities.Adapters.Arff;
using TraceWizard.Environment;

using Ionic.Zip;

namespace TraceWizard.TwApp {
    public static class TwFile {

        [Obsolete]
        public static string ChangeExtension(string fileName, string extension) {
            return System.IO.Path.ChangeExtension(fileName, extension);
        }

        [Obsolete]
        public static bool HasFileExtension(string fileName, string extension) {
            return string.Equals(System.IO.Path.GetExtension(fileName), "." + extension, StringComparison.InvariantCultureIgnoreCase);
        }

        public static void LaunchNotepad(string fileName) {
            Launch("NOTEPAD.EXE", fileName);
        }

        public static void Launch(string fileName, string arguments) {
            try {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = fileName;
                startInfo.Arguments = arguments;
                Process.Start(startInfo);
            } catch { }
        }

        public static void Launch(string fileName) {
            try {
                Process.Start(fileName);
            } catch { }
        }

        private static string extractedDirectory() {
            return "TraceWizardTempFiles";
        }

        private static string extractedDirectoryWithPath() {
            return System.IO.Path.GetTempPath() + extractedDirectory();
        }

        public static void DeleteExtractedDirectory() {
            if (!Directory.Exists(extractedDirectoryWithPath()))
                return;

            try {
                foreach (string file in Directory.GetFiles(extractedDirectoryWithPath())) {
                    File.SetAttributes(file, ~FileAttributes.ReadOnly);
                }
                Directory.Delete(extractedDirectoryWithPath(), true);
            } catch (Exception ex) {
                string message = "Unable to delete " + extractedDirectoryWithPath() + " folder. ";
                message += System.Environment.NewLine + System.Environment.NewLine + ex.Message;
                MessageBox.Show(message, TwAssembly.Title());
            }
        }
        public static void CreateExtractedDirectory() {
            Directory.CreateDirectory(extractedDirectoryWithPath());
        }

        public static List<string> Extract(string zipToUnpack) {
            CreateExtractedDirectory();

            List<string> list = new List<string>();

            using (ZipFile zip = ZipFile.Read(zipToUnpack)) {
                foreach (ZipEntry zipEntry in zip) {
                    zipEntry.Extract(extractedDirectoryWithPath(), ExtractExistingFileAction.OverwriteSilently);
                    list.Add(extractedDirectoryWithPath() + "\\" + zipEntry.FileName);
                }
            }
            return list;
        }
        
        private static bool IsZip(string file) {
            string extension = Path.GetExtension(file);
            switch (Path.GetExtension(file).ToLower()) {
                case ".zip":
                //case ".7z":
                    return true;
            }
            return false;
        }

        public static List<string> GetCommandLineArgs() {
            return GetFilesIncludingZipped(System.Environment.GetCommandLineArgs());
        }

        static List<string> GetFilesIncludingZipped(string[] files) {
            List<string> list = new List<string>();

            foreach (string arg in files) {
                if (IsZip(arg)) {
                    List<string> listZip = Extract(arg);
                    foreach (string file in listZip) {
                        list.Add(file);
                    }
                } else {
                    list.Add(arg);
                }
            }
            return list;
        }

        static List<string> GetFiles(string[] files) {
            List<string> list = new List<string>();

            foreach (string arg in files)
                list.Add(arg);
            return list;
        }

        private static string[] GetFileNames(string title, string filter, string folder) {
            Microsoft.Win32.OpenFileDialog dialog = GetFileNameDialog(title, filter, true, folder);
            if (dialog == null) {
                return new string[0];
            }
            return dialog.FileNames; 
        }

        private static string GetFileName(string title, string filter, string folder) {
            Microsoft.Win32.OpenFileDialog dialog = GetFileNameDialog(title, filter, false,folder);
            if (dialog == null) {
                return string.Empty;
            }
            return dialog.FileName;
        }

        private static Microsoft.Win32.OpenFileDialog GetFileNameDialog(string title, string filter, bool multiselect, string folder) {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Title = title;
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.Multiselect = multiselect;
            dlg.FileName = string.Empty;
            dlg.Filter = filter;
            dlg.RestoreDirectory = true;
            dlg.InitialDirectory = folder;

            Nullable<bool> result = dlg.ShowDialog();

            return (result == true) ? dlg : null;
        }

        public static string GetLogFile() {
            return GetLogFile(null);
        }
        public static string GetLogFile(string folder) {
            string filter =
                "*." + TwEnvironment.MeterMasterJetLogExtension
                + ";*." + TwEnvironment.CsvLogExtension
                + ";*." + TwEnvironment.TelematicsLogExtension
                + ";*." + TwEnvironment.TextLogExtension
                + ";*." + "zip";
            return GetFileName("Select Log file", "Log File (" + filter + ")|" + filter, folder);
        }

        public static List<string> GetLogFilesIncludingZipped() {
            return GetLogFilesIncludingZipped(null);
        }

        public static List<string> GetLogFilesIncludingZipped(string folder) {
            return GetFilesIncludingZipped(GetLogFiles(folder));
        }

        private static string[] GetLogFiles() {
            return GetLogFiles(null);
        }

        public static string[] GetTdbAnalysisFiles(string folder) {
            string filter =
                "*." + TwEnvironment.Tw4JetAnalysisExtension
                 + ";*." + "zip";
            return GetFileNames("Select Tdb Trace Files", "Tdb Trace File (" + filter + ")|" + filter, folder);
        }

        public static string[] GetMdbLogFiles(string folder) {
            string filter =
                "*." + TwEnvironment.MeterMasterJetLogExtension
                + ";*." + "zip";
            return GetFileNames("Select Mdb Log Files", "Mdb Log File (" + filter + ")|" + filter, folder);
        }

        private static string[] GetLogFiles(string folder) {
            string filter =
                "*." + TwEnvironment.MeterMasterJetLogExtension
                + ";*." + TwEnvironment.CsvLogExtension
                + ";*." + TwEnvironment.TelematicsLogExtension
                + ";*." + TwEnvironment.TextLogExtension
                + ";*." + "zip";
            return GetFileNames("Select Log Files", "Log File (" + filter + ")|" + filter, folder);
        }

        public static string GetAnalysisFile() {
            return GetAnalysisFile(null);
        }

        public static string GetAnalysisFile(string folder) {
            string filter =
                "*." + TwEnvironment.ArffAnalysisExtension
                + ";*." + TwEnvironment.Tw4JetAnalysisExtension
                + ";*." + "zip";
            return GetFileName("Select Trace File", "Trace File (" + filter + ")|" + filter, folder);
        }

        public static List<string> GetArffAnalysisFilesRecover() {
            return GetFiles(GetArffAnalysisFiles(@".\" + TwEnvironment.BackupFolder));
        }

        public static List<string> GetAnalysisFilesIncludingZipped() {
            return GetFilesIncludingZipped(GetAnalysisFiles(Properties.Settings.Default.DirectoryAnalysis));
        }

        [Obsolete]
        private static string[] GetAnalysisFiles() {
            return GetAnalysisFiles(Properties.Settings.Default.DirectoryAnalysis);
        }

        private static string[] GetArffAnalysisFiles(string folder) {
            string filter = "*." + TwEnvironment.ArffAnalysisExtension;
            return GetFileNames("Select Trace files", "Trace File (" + filter + ")|" + filter, folder);
        }

        private static string[] GetAnalysisFiles(string folder) {
            string filter =
                "*." + TwEnvironment.ArffAnalysisExtension
                + ";*." + TwEnvironment.Tw4JetAnalysisExtension
                + ";*." + "zip";
            return GetFileNames("Select Trace files", "Trace File (" + filter + ")|" + filter, folder);
        }

        public static List<string> GetLogOrAnalysisFilesIncludingZipped() {
            return GetFilesIncludingZipped(GetLogOrAnalysisFiles());
        }

        private static string[] GetLogOrAnalysisFiles() {
            string filter =
                "*." + TwEnvironment.ArffAnalysisExtension
                + ";*." + TwEnvironment.Tw4JetAnalysisExtension
                + ";*." + TwEnvironment.MeterMasterJetLogExtension
                + ";*." + TwEnvironment.CsvLogExtension
                + ";*." + TwEnvironment.TelematicsLogExtension
                + ";*." + TwEnvironment.TextLogExtension
                + ";*." + "zip";
            return GetFileNames("Select Log or Trace files", "Trace or Log File (" + filter + ")|" + filter, null);
        }

        public static string GetBackupArffFileToSave(string path, string fileName) {
            string extension = TwEnvironment.ArffAnalysisExtension;
            return GetUniqueFileName(path, TwEnvironment.BackupFolder, fileName, extension);
        }

        static string GetUniqueFileName(string pathWithBackupFolder, string fileName, string extension, int version) {
            return pathWithBackupFolder + @"\" + fileName + "-" + TwEnvironment.BackupFolder + "-" + version + "." + extension;        
        }

        static string GetUniqueFileName(string path, string backupFolder, string fileName, string extension) {
            string pathWithBackupFolder = path + @"\" + backupFolder;
            try {
                if (!System.IO.Directory.Exists(pathWithBackupFolder))
                    System.IO.Directory.CreateDirectory(pathWithBackupFolder);

                for (int i = 0; ; i++) {
                    string pathQualifiedFileName = GetUniqueFileName(pathWithBackupFolder, fileName, extension, i);
                    if (!System.IO.File.Exists(pathQualifiedFileName)) {
                        return pathQualifiedFileName;
                    }
                }
            } catch {
                return string.Empty;
            }
        }

        public static string GetArffFileToSave(string defaultFileName) {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = defaultFileName;
            dlg.InitialDirectory = Properties.Settings.Default.DirectoryAnalysis;
            dlg.CheckPathExists = true;
            dlg.DefaultExt = "." + TwEnvironment.ArffAnalysisExtension;
            dlg.Filter = "Trace files (." + TwEnvironment.ArffAnalysisExtension + ")|*." + TwEnvironment.ArffAnalysisExtension;
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != true)
                return null;
            else
                return dlg.FileName;
        }

        public static string GetProjectReportFileToSave(string defaultFileName) {
            string extension = "txt";
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = defaultFileName;
            dlg.DefaultExt = "." + extension;
            dlg.Filter = "Project Report files (." + extension + ")|*." + extension;
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != true)
                return null;
            else
                return dlg.FileName;
        }
    }
}
