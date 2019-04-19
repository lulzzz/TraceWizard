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
    public partial class ReportGeneralProperties : UserControl {
        public Analysis Analysis;
        public ReportGeneralProperties() {
            InitializeComponent();
        }
        public void Initialize() {
            LabelFile.Text = Analysis.KeyCode;
            if (Analysis.Events != null) {
                LabelStart.Text = Analysis.Events.StartTime.ToLongDateString() + " " + Analysis.Events.StartTime.ToLongTimeString();
                LabelEnd.Text = Analysis.Events.EndTime.ToLongDateString() + " " + Analysis.Events.EndTime.ToLongTimeString();
            } else if (Analysis.Log != null) {
                LabelStart.Text = Analysis.Log.StartTime.ToLongDateString() + " " + Analysis.Log.StartTime.ToLongTimeString();
                LabelEnd.Text = Analysis.Log.EndTime.ToLongDateString() + " " + Analysis.Log.EndTime.ToLongTimeString();
            }
        }
    }
}
