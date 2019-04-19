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
using TraceWizard.Logging.Adapters;

namespace TraceWizard.TwApp {
    public partial class LogPropertiesWindow : Window {

        public Analysis Analysis;
        
        public LogPropertiesWindow() {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            KeyDown += new KeyEventHandler(window_KeyDown);
        }

        public void Initialize() {

            Title = "LogProperties - " + Analysis.KeyCode;

            LogPropertiesDetail.Log = Analysis.Log as LogMeter;
            LogPropertiesDetail.Initialize();

        }
        void window_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
