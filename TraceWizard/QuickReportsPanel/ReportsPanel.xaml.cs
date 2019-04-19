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
using System.ComponentModel;

using TraceWizard.Entities;

namespace TraceWizard.TwApp {
    public partial class QuickReportsPanel : UserControl {

        public Analysis Analysis { get; set; }

        public QuickReportsPanel() {
            InitializeComponent();
        }

        public void Initialize() {

//            ToolTipService.SetShowDuration(ButtonHourly, 60000);
//            ToolTipService.SetInitialShowDelay(ButtonHourly, 500);

            ToolTipService.SetShowDuration(ButtonDistribution, 60000);
            ToolTipService.SetInitialShowDelay(ButtonDistribution, 500);

            ButtonByInstances.MouseEnter += new MouseEventHandler(buttonByInstances_MouseEnter);
            ButtonByVolume.MouseEnter += new MouseEventHandler(buttonByVolume_MouseEnter);
//            ButtonHourly.MouseEnter += new MouseEventHandler(buttonHourly_MouseEnter);
            ButtonDistribution.MouseEnter += new MouseEventHandler(buttonDistribution_MouseEnter);

            ButtonByInstances.Click += new RoutedEventHandler(buttonByInstances_Click);
            ButtonByVolume.Click += new RoutedEventHandler(buttonByVolume_Click);
//            ButtonHourly.Click += new RoutedEventHandler(buttonHourly_Click);
            ButtonDistribution.Click += new RoutedEventHandler(buttonDistribution_Click);

            Properties.Settings.Default.PropertyChanged += new PropertyChangedEventHandler(Default_PropertyChanged);
            Visibility = SetVisibility();
        }

        Visibility SetVisibility() {
            return Properties.Settings.Default.ShowReportsPanel ? Visibility.Visible : Visibility.Collapsed;
        }

        public void Default_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "ShowReportsPanel":
                    Visibility = SetVisibility();
                    break;
            }
        }

        void buttonByInstances_MouseEnter(object sender, MouseEventArgs e) {

            var panel = new FixtureSummaryReportTable();
            panel.ByInstances = true;
            panel.Analysis = Analysis;
            panel.Initialize();
            ButtonByInstances.ToolTip = panel;
        }

        void buttonByVolume_MouseEnter(object sender, MouseEventArgs e) {

            var panel = new FixtureSummaryReportTable();
            panel.ByInstances = false;
            panel.Analysis = Analysis;
            panel.Initialize();
            ButtonByVolume.ToolTip = panel;
        }

        void buttonHourly_MouseEnter(object sender, MouseEventArgs e) {
            Analysis.UpdateHourlyTotals();

            var panel = new HourlyTotalsDetail();
            panel.Analysis = Analysis;

            panel.Initialize();

//            ButtonHourly.ToolTip = panel;
        }

        void buttonDistribution_MouseEnter(object sender, MouseEventArgs e) {
            var panel = new DistributionReportPanel();
            panel.Analysis = Analysis;

            panel.Initialize();

            ButtonDistribution.ToolTip = panel;
        }

        void buttonByInstances_Click(object sender, System.Windows.RoutedEventArgs e) {
            OpenPieChart(true);
        }

        void buttonByVolume_Click(object sender, System.Windows.RoutedEventArgs e) {
            OpenPieChart(false);
        }

        void OpenPieChart(bool byInstances) {
            var window = new FixtureSummaryReportWindow();
            window.ByInstances = byInstances;
            window.Radius = 200;
            window.Analysis = Analysis;
            window.Initialize();

            window.ShowDialog();
        }

        void buttonHourly_Click(object sender, System.Windows.RoutedEventArgs e) {
            Analysis.UpdateHourlyTotals();

            var report = new HourlyReportWindow();
            report.Analysis = Analysis;
            report.Initialize();
            report.ShowDialog();
        }

        void buttonDistribution_Click(object sender, System.Windows.RoutedEventArgs e) {
            var window = new DistributionReportWindow();
            window.Analysis = Analysis;
            window.Initialize();
            window.ShowDialog();
        }
    }
}
