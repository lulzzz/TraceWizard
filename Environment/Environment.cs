using System;

namespace TraceWizard.Environment {

    public static class TwEnvironment {

        public const string MeterMasterJetLogExtension = "mdb";
        public const string CsvLogExtension = "csv";
        public const string TelematicsLogExtension = "tlmw";
        public const string TextLogExtension = "txt";

        public const string ArffAnalysisExtension = "twdb";
        public const string Tw4JetAnalysisExtension = "tdb";

        public const string TwExecutable = @"TraceWizard.exe";

        public const string BackupFolder = @"TwBackup";

        public const string TwExemplars = @"exemplars.twdb";

        public const string TwEventsDatabase = @"TraceWizardEventsDatabase.twdb";
        public const string TwProjectReport = @"TraceWizardProjectReport.txt";
        public const string TwWeka = @"TraceWizardWeka.twdb";

        public const string TwReleaseNotes = @"twReleaseNotes.html";
        public const string TwHelpShortcuts = @"twShortcutsHelp.html";
        public const string TwHelpCommands = @"twCommandsHelp.html";
        public const string TwHelpCommandLine = @"twCommandLineHelp.html";
        public const string TwReadMe = @"twReadMe.html";

        public const string TwTipsBase = @"TwTips";
        public const string TwTipsExtension = @"pdf";

        public const string WebSite = "http://www.aquacraft.com";

        static public string ExecutableDirectory() {
            return System.AppDomain.CurrentDomain.BaseDirectory;
        }
        
        static public void SetCurrentDirectoryToExecutableDirectory() {
            System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}