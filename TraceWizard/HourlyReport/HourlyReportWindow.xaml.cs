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
using System.Windows.Shapes;

using TraceWizard.Entities;

namespace TraceWizard.TwApp {
    public partial class HourlyReportWindow : Window {
        public Analysis Analysis;

        public HourlyReportWindow() {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            KeyDown +=new KeyEventHandler(window_KeyDown);        
        }

        public void Initialize() {

            Title = "Daily and Hourly Report - " + Analysis.KeyCode;

            HourlyReportPanel.Analysis = Analysis;
            HourlyReportPanel.Initialize();
        }

        void window_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
