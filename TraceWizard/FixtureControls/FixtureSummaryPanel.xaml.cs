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
    public partial class FixtureSummaryPanel : UserControl {

        public FixtureClass FixtureClass { get;set;}
        public FixtureSummary FixtureSummary { get;set;}

        public FixtureSummaryPanel() {
            InitializeComponent();
        }

        public void Initialize() {

            ToolTipService.SetShowDuration(textBlockInstancesCount, 30000);
            ToolTipService.SetInitialShowDelay(textBlockInstancesCount, 500);

            textBlockInstancesCount.MouseEnter +=new MouseEventHandler(textBlockInstancesCount_MouseEnter);
        }

        void textBlockInstancesCount_MouseEnter(object sender, MouseEventArgs e) {
            FixtureSummary.Events.UpdateMedians();
            textBlockInstancesCount.ToolTip = new EventsProperties(
                FixtureClass, FixtureSummary.Events,
                FixtureSummary.Events.Count);
            ;
        }
    }
}
