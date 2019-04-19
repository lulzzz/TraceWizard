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
    public partial class FixtureSummaryReportRow : UserControl {
        public FixtureSummary FixtureSummary;
        public bool ByInstances;

        public FixtureSummaryReportRow() {
            InitializeComponent();
        }
        public void Initialize() {
            LabelFixture.Child = TwGui.FixtureWithImageRight(FixtureSummary.FixtureClass);
            LabelCount.Text = ByInstances ? FixtureSummary.Count.ToString() : FixtureSummary.Volume.ToString("0.0");
            LabelPercent.Text = ByInstances ? (FixtureSummary.PercentCount * 100).ToString("0.0") + "%" : (FixtureSummary.PercentVolume * 100).ToString("0.0") + "%";
        }
    }
}
