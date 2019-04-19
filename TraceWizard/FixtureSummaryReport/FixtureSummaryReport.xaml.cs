using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using TraceWizard.Entities;

namespace TraceWizard.TwApp {
    public partial class FixtureSummaryReportTable : UserControl {
        public Analysis Analysis;
        public bool ByInstances;

        public FixtureSummaryReportTable() {
            InitializeComponent();
        }
        public void Initialize() {

            Header.LabelCount.Text = ByInstances ? "Events" : "Volume";
            Header.LabelCount.FontWeight = FontWeights.Bold;
            Header.LabelPercent.Text = "Percent";
            Header.LabelPercent.FontWeight = FontWeights.Bold;

            IEnumerable<FixtureSummary> sorted;
            if (ByInstances)
                sorted = Enumerable.OrderByDescending(Analysis.FixtureSummaries.Values, n => n.Count);
            else
                sorted = Enumerable.OrderByDescending(Analysis.FixtureSummaries.Values, n => n.Volume);

            foreach (FixtureSummary fixtureSummary in sorted) {
                if (fixtureSummary.Count > 0) {
                    var fixtureSummaryReportDetail = new FixtureSummaryReportRow();
                    fixtureSummaryReportDetail.FixtureSummary = fixtureSummary;
                    fixtureSummaryReportDetail.ByInstances = ByInstances;
                    StackPanel.Children.Add(fixtureSummaryReportDetail);
                    fixtureSummaryReportDetail.Initialize();
                }
            }
        }
    }
}
