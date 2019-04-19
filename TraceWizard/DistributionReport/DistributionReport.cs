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

namespace TraceWizard.TwApp {
    public class DistributionReporter : MultiReporter {

        List<string> files;

        public List<UIElement> Report() {

            var reports = new List<UIElement>();
            
            files = TwFile.GetAnalysisFilesIncludingZipped();
            if (files.Count != 0) {
                foreach (string file in files) {
                    reports.Add(Load(file));
                }
            }
            return reports;

        }

        DistributionReportPanel Load(string fileName) {

            Mouse.OverrideCursor = Cursors.Wait;

            Analysis analysis = TwServices.CreateAnalysis(fileName);
            analysis.UpdateFixtureSummaries();

            var reportPanel = new DistributionReportPanel();
            reportPanel.Analysis = analysis;
            reportPanel.Initialize();

            Mouse.OverrideCursor = null;

            return reportPanel;
        }
    }
}

