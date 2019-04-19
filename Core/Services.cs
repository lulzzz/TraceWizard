using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using TraceWizard.Entities;
using TraceWizard.Logging;
using TraceWizard.Logging.Adapters;
using TraceWizard.Logging.Adapters.MeterMasterCsv;
using TraceWizard.Logging.Adapters.MeterMasterJet;
using TraceWizard.Entities.Adapters;
using TraceWizard.Entities.Adapters.Arff;
using TraceWizard.Entities.Adapters.Tw4Jet;
using TraceWizard.Environment;

using TraceWizard.Classification;
using TraceWizard.Classification.Classifiers.Null;

using TraceWizard.Disaggregation;

namespace TraceWizard.Services {

    public static class TwServices {

        public static Log CreateLog(string dataSource) {
            return CreateLogAdapter(dataSource).Load(dataSource);
        }

        public static Analysis CreateAnalysis(string dataSource) {
            var analysisAdapter = CreateAnalysisAdapter(dataSource);
            return analysisAdapter.Load(dataSource);
        }

        public static bool IsLog(string dataSource) {
            var factory = new LogAdapterFactory();
            return (factory.GetAdapter(dataSource) != null);
        }

        public static bool IsAnalysis(string dataSource) {
            var factory = new AnalysisAdapterFactory();
            return (factory.GetAdapter(dataSource) != null);
        }

        public static LogAdapter CreateLogAdapter(string dataSource) {
            var factory = new LogAdapterFactory();
            return factory.GetAdapter(dataSource) as LogAdapter;
        }

        public static AnalysisAdapter CreateAnalysisAdapter(string dataSource) {
            var factory = new AnalysisAdapterFactory();
            return factory.GetAdapter(dataSource) as AnalysisAdapter;
        }

        public static int ExportMdbToCsv(List<string> files) {
            var csvAdapter = CreateLogAdapter("dummy.csv") as MeterMasterCsvLogAdapter;
            var factory = new LogAdapterFactory();
            int countFilesSaved = 0;
            foreach (var file in files) {
                var logAdapter = factory.GetAdapter(file) as MeterMasterJetLogAdapter;
                if (logAdapter != null) {
                    var log = logAdapter.Load(file) as LogMeter;
                    csvAdapter.Save(System.IO.Path.ChangeExtension(file, TwEnvironment.CsvLogExtension),log);
                    countFilesSaved++;
                }
            }
            return countFilesSaved;
        }

        public static int ExportTdbToTwdb(List<string> files) {
            var twdbAdapter = CreateAnalysisAdapter("dummy.twdb") as ArffAnalysisAdapter;
            var factory = new AnalysisAdapterFactory();
            int countFilesSaved = 0;
            foreach (var file in files) {
                var analysisAdapter = factory.GetAdapter(file) as Tw4JetAnalysisAdapter;
                if (analysisAdapter != null) {
                    var analysis = analysisAdapter.Load(file);
                    twdbAdapter.Save(System.IO.Path.ChangeExtension(file, TwEnvironment.ArffAnalysisExtension), analysis, true);
                    countFilesSaved++;
                }
            }
            return countFilesSaved;
        }

        public static int ExportLogToTwdb(List<string> files, Disaggregator disaggregator) {
            var twdbAdapter = CreateAnalysisAdapter("dummy.twdb") as ArffAnalysisAdapter;
            var factory = new LogAdapterFactory();
            int countFilesSaved = 0;
            foreach (var file in files) {
                var logAdapter = factory.GetAdapter(file) as LogAdapter;
                if (logAdapter != null) {
                    var log = logAdapter.Load(file);

                    var classifier = new NullClassifier();
                    
                    disaggregator.Log = log;
                    Events events = disaggregator.Disaggregate();

                    var analysis = new AnalysisDatabase(file, events, log);
                    classifier.Classify(analysis);

                    analysis.UpdateFixtureSummaries();

                    twdbAdapter.Save(System.IO.Path.ChangeExtension(file, TwEnvironment.ArffAnalysisExtension), analysis, true);
                    countFilesSaved++;
                }
            }
            return countFilesSaved;
        }

        public static bool DispatchUtilities(string[] files, Disaggregator disaggregator) {
            string currentDirectory = System.Environment.CurrentDirectory;

            if (files.Length < 2 || files[1].Length < 2)
                return false;

            string dispatchArg = files[1];
            if (dispatchArg[0] != '-' && dispatchArg[0] != '/')
                return false;

            var listNormalized = new List<string>();
            for (int i = 2; i < files.Length; i++) {
                string fileSpec = System.IO.Path.GetFileName(files[i]);
                string directory = System.IO.Path.GetDirectoryName(files[i]);
                string[] filesDirectory = System.IO.Directory.GetFiles(directory, fileSpec);
                foreach (string file in filesDirectory) {
                    listNormalized.Add(file);
                    if (!System.IO.File.Exists(file))
                        throw new Exception("File not found: " + file);
                }
            }

            try {
                switch (dispatchArg.Substring(1)) {
                    case TwEnvironment.MeterMasterJetLogExtension + "to" + TwEnvironment.CsvLogExtension:
                        TwServices.ExportMdbToCsv(listNormalized);
                        break;
                    case TwEnvironment.Tw4JetAnalysisExtension + "to" + TwEnvironment.ArffAnalysisExtension:
                        TwServices.ExportTdbToTwdb(listNormalized);
                        break;
                    //case TwEnvironment.MeterMasterJetLogExtension + "to" + TwEnvironment.ArffAnalysisExtension:
                    //case TwEnvironment.MeterMasterCsvLogExtension + "to" + TwEnvironment.ArffAnalysisExtension:
                    //case TwEnvironment.TelematicsLogExtension + "to" + TwEnvironment.ArffAnalysisExtension:
                    //case TwEnvironment.LogExtension + "to" + TwEnvironment.ArffAnalysisExtension:
                    case "log" + "to" + TwEnvironment.ArffAnalysisExtension:
                    case "logs" + "to" + TwEnvironment.ArffAnalysisExtension:
                        TwServices.ExportLogToTwdb(listNormalized, disaggregator);
                        break;
                    default:
                        throw new Exception("Invalid command line argument: " + dispatchArg.Substring(1));
                }
            } catch (Exception ex) {
                throw new Exception("Trace Wizard Utility error: " + ex.Message);
            }

            return true;
        }
    }
}
