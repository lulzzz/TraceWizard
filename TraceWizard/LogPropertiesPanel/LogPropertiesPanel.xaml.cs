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
using TraceWizard.Logging;
using TraceWizard.Logging.Adapters;

namespace TraceWizard.TwApp {
    public partial class LogPropertiesPanel : UserControl {

        LogMeter log;

        public Analysis Analysis;

        public LogPropertiesPanel() {
            InitializeComponent();
        }

        public void Initialize() {

            log = Analysis.Log as LogMeter;
            if (log == null)
                return;
            if (log.Meter.MeterMasterVolume.HasValue) {
                LabelMeterMasterVolume.Text = log.Meter.MeterMasterVolume.Value.ToString("0.0");
                LabelMeterMasterVolume.ToolTip = "Meter Master Volume";
            }

            bool volumeToleranceExceeded = Analysis.VolumeToleranceExceeded(log.Meter.MeterMasterVolume, 0.05);
            if (volumeToleranceExceeded) {
                LabelMeterMasterVolume.Background = new SolidColorBrush(Colors.Red);
                LabelMeterMasterVolume.ToolTip = "Warning: Trace Wizard Volume differs significantly from Meter Master Volume";
            }

            if (log.Meter.RegisterVolume.HasValue) {
                LabelRegisterVolume.Text = log.Meter.RegisterVolume.Value.ToString("0.0");
                LabelRegisterVolume.ToolTip = "Register Volume";
            }
            if (log.Meter.ConversionFactor.HasValue) {
                LabelConversionFactor.Text = log.Meter.ConversionFactor.Value.ToString("0.00");
                LabelConversionFactor.ToolTip = "Conversion Factor";
            }
            Properties.Settings.Default.PropertyChanged +=new PropertyChangedEventHandler(Default_PropertyChanged);
            Visibility = SetVisibility();

            ButtonLog.MouseEnter += new MouseEventHandler(buttonLog_MouseEnter);
            ButtonLog.Click += new RoutedEventHandler(buttonLog_Click);
        }

        void buttonLog_MouseEnter(object sender, MouseEventArgs e) {
            var logPropertiesDetail = new LogPropertiesDetail();
            logPropertiesDetail.Log = log;
            logPropertiesDetail.Initialize();

            ButtonLog.ToolTip = logPropertiesDetail;
        }

        void buttonLog_Click(object sender, System.Windows.RoutedEventArgs e) {
            var window = new LogPropertiesWindow();
            window.Analysis = Analysis;
            window.Initialize();
            window.ShowDialog();
        }



        Visibility SetVisibility() {
            return Properties.Settings.Default.ShowLogPanel ? Visibility.Visible : Visibility.Collapsed;
        }

        public void Default_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "ShowLogPanel":
                    Visibility = SetVisibility();
                    break;}
        }
    }
}
