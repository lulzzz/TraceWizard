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
using TraceWizard.Services;

namespace TraceWizard.TwApp {

    public partial class FixturePanel : UserControl {
        public FixturePanel() {
            InitializeComponent();
        }

        public Analysis Analysis;
        public FixtureClass FixtureClass;
        public string Label;

        public void Initialize() {
            if (FixtureClass != null) {
                Tag = FixtureClass;
                FixtureLabel.CanDrag = true;
                //                Button.Tag = FixtureClass;
            } else {
                FixtureLabel.Border.Visibility = Visibility.Hidden;
            }

            FixtureLabel.FixtureClass = FixtureClass;
            FixtureLabel.Label= Label;

            FixtureSummaryPanel.FixtureClass = FixtureClass;
            if (Analysis != null && FixtureClass != null) {
                FixtureSummaryPanel.FixtureSummary = Analysis.FixtureSummaries[FixtureClass];
                FixtureSummaryPanel.Initialize();
            }
        }
    }
}
