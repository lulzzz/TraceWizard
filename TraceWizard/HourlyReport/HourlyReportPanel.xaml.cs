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
using TraceWizard.Logging;
using TraceWizard.Logging.Adapters;

namespace TraceWizard.TwApp {
    public partial class HourlyReportPanel : UserControl {

        public Analysis Analysis;

        public HourlyReportPanel() {
            InitializeComponent();
        }

        public void Initialize() {

            GeneralProperties.Analysis = Analysis;
            GeneralProperties.Initialize();

            HourlyTotalsDetail.Analysis = Analysis;
            HourlyTotalsDetail.Initialize();

            DailyTotalsDetail.Analysis = Analysis;
            DailyTotalsDetail.Initialize();

            LogMeter log = Analysis.Log as LogMeter;
            if (log != null) {
                LogPropertiesDetail.Log = log;
                LogPropertiesDetail.Initialize();
            }
        }
    }
}

