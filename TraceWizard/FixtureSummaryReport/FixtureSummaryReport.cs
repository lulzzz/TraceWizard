using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

using TraceWizard.Entities;
using TraceWizard.Services;
using TraceWizard.Reporters;

namespace TraceWizard.TwApp {
    public class FixtureSummaryReporter : MultiReporter {

        List<string> files;

        public bool ByInstances;
        
        public List<UIElement> Report() {

            var reports = new List<UIElement>();

            files = TwFile.GetAnalysisFilesIncludingZipped();
            if (files.Count != 0) {
                foreach (string file in files) {
                    reports.Add(Load(file, ByInstances));
                }
            }
            return reports;
        }

        FixtureSummaryReportPanel Load(string fileName, bool byInstances) {

            Analysis analysis = TwServices.CreateAnalysis(fileName);
            analysis.UpdateFixtureSummaries();

            var reportPanel = new FixtureSummaryReportPanel();
            reportPanel.ByInstances = byInstances;
            reportPanel.Analysis = analysis;
            reportPanel.Radius = 200;
            reportPanel.Initialize();

            return reportPanel;
        }
    }
}