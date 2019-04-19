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
    public partial class DistributionReportPanel : UserControl {
        public Analysis Analysis;
        public DistributionReportPanel() {
            InitializeComponent();
        }

        public void Initialize() {

            FixtureSummaries fixtureSummaries = Analysis.FixtureSummaries;

            foreach (FixtureSummary fixtureSummary in fixtureSummaries.Values) {
                fixtureSummary.Events.UpdateMedians();
            } 

            string keyCode = Analysis.KeyCode;

            IEnumerable<FixtureSummary> sorted = Enumerable.OrderByDescending(Analysis.FixtureSummaries.Values, n => n.Volume);

            foreach (FixtureSummary fixtureSummary in sorted) {
                var fixtureClass = fixtureSummary.FixtureClass;
                if (fixtureSummary.Events.Count != 0) {
                    var eventsProperties = new EventsProperties(fixtureClass,
                        fixtureSummaries[fixtureClass].Events, fixtureSummaries[fixtureClass].Events.Count,
                        fixtureSummaries[fixtureClass].FirstCycles);
                    eventsProperties.Margin = new Thickness(20, 0, 20, 20);
                    grid.Children.Add(eventsProperties);
                }
            }

        }
    }
}
