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
    public partial class PieChartsPanel : UserControl {

        public Analysis Analysis;

        public PieChartsPanel() {
            InitializeComponent();
        }

        public void Initialize() {

            PieChartByInstances.Analysis = Analysis;
            PieChartByInstances.ByInstances = true;
            PieChartByInstances.Radius = 22;
            PieChartByInstances.IsEnlargeable = true;
            PieChartByInstances.Initialize();

            PieChartByVolume.Analysis = Analysis;
            PieChartByVolume.ByInstances = false;
            PieChartByVolume.Radius = 22;
            PieChartByVolume.IsEnlargeable = true;
            PieChartByVolume.Initialize();

            PieChartByInstances.MouseEnter += new MouseEventHandler(pieChartByInstances_MouseEnter);
            PieChartByVolume.MouseEnter += new MouseEventHandler(pieChartByVolume_MouseEnter);

            Properties.Settings.Default.PropertyChanged += new PropertyChangedEventHandler(Default_PropertyChanged);
            Visibility = SetVisibility();
        }

        Visibility SetVisibility() {
            return Properties.Settings.Default.ShowPieChartsPanel ? Visibility.Visible : Visibility.Collapsed;
        }

        public void Default_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "ShowPieChartsPanel":
                    Visibility = SetVisibility();
                    break;
            }
        }

        void pieChartByInstances_MouseEnter(object sender, MouseEventArgs e) {
            var panel = new FixtureSummaryReportTable();
            panel.ByInstances = true;
            panel.Analysis = Analysis;
            panel.Initialize();
            PieChartByInstances.ToolTip = panel;
        }

        void pieChartByVolume_MouseEnter(object sender, MouseEventArgs e) {
            var panel = new FixtureSummaryReportTable();
            panel.ByInstances = false;
            panel.Analysis = Analysis;
            panel.Initialize();
            PieChartByVolume.ToolTip = panel;
        }

    }
}
