using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

using TraceWizard.Entities;
using TraceWizard.Services;
using TraceWizard.Logging;
using TraceWizard.Logging.Adapters;
using TraceWizard.Reporters;
using TraceWizard.Classification.Classifiers.Null;
using TraceWizard.Disaggregation.Disaggregators.Tw4;

namespace TraceWizard.TwApp {
    public class HourlyReporter : MultiReporter {

        public List<string> files;

        public List<UIElement> Report() {

            var reports = new List<UIElement>();
            
            files = TwFile.GetLogOrAnalysisFilesIncludingZipped();
            if (files.Count != 0) {
                foreach (string file in files) {
                    reports.Add(LoadHourlyReport(file));
                }
            }
            return reports;

        }

        HourlyReportPanel LoadHourlyReport(string fileName) {
            HourlyReportPanel reportPanel = null;

            if (TwServices.IsLog(fileName))
                reportPanel = LoadLog(fileName);
            else if (TwServices.IsAnalysis(fileName))
                reportPanel = LoadAnalysis(fileName);

            return reportPanel;
        }

        HourlyReportPanel LoadLog(string fileName) {
            Mouse.OverrideCursor = Cursors.Wait;
            
            Log log = TwServices.CreateLog(fileName);
            log.UpdateHourlyTotals();
            log.UpdateDailyTotals();

            var reportPanel = new HourlyReportPanel();
            reportPanel.Analysis = new Analysis(log);
            reportPanel.Initialize();

            Mouse.OverrideCursor = null;

            return reportPanel;
        }

        HourlyReportPanel LoadAnalysis(string fileName) {
            Mouse.OverrideCursor = Cursors.Wait;

            Analysis analysis = TwServices.CreateAnalysis(fileName);
            analysis.UpdateFixtureSummaries();
            analysis.UpdateHourlyTotals();
            analysis.UpdateDailyTotals();

            var reportPanel = new HourlyReportPanel();
            reportPanel.Analysis = analysis;
            reportPanel.Initialize();

            Mouse.OverrideCursor = null;

            return reportPanel;
        }
    }
}