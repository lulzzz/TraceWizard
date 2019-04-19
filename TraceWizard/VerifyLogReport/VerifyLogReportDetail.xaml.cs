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
using TraceWizard.Logging.Adapters.Telematics;

namespace TraceWizard.TwApp {
    public partial class VerifyLogReportDetail : UserControl {

        public TelematicsLogAdapter.IntegrityData IntegrityData;

        public VerifyLogReportDetail() {
            InitializeComponent();
        }
    }
}
