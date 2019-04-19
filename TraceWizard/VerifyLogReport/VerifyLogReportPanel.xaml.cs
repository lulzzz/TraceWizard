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

using System.Windows.Controls.Primitives;

using TraceWizard.Entities;
using TraceWizard.Logging.Adapters.Telematics;

namespace TraceWizard.TwApp {
    public partial class VerifyLogReportPanel : UserControl {

        public List<TelematicsLogAdapter.IntegrityData> IntegrityDatas = new List<TelematicsLogAdapter.IntegrityData>();
                
        public VerifyLogReportPanel() {
            InitializeComponent();
        }

        public void Initialize() {
            foreach(var data in IntegrityDatas) {
                var detail = new VerifyLogReportDetail();
                detail.FileName.Text = System.IO.Path.GetFileNameWithoutExtension(data.FileName);

                detail.HeaderStartDate.Text = data.HeaderStartTime.ToLongDateString();
                detail.HeaderStartTime.Text = data.HeaderStartTime.ToLongTimeString();

                detail.FileNameStartDate.Text = data.FileNameStartTime.ToLongDateString();
                detail.FileNameStartTime.Text = data.FileNameStartTime.ToLongTimeString();

                detail.StartDate.Text = data.StartTime.ToLongDateString();
                detail.StartTime.Text = data.StartTime.ToLongTimeString();

                detail.EndDate.Text = data.EndTime.ToLongDateString();
                detail.EndTime.Text = data.EndTime.ToLongTimeString();

                detail.DurationByCounters.Text = new TwHelper.FriendlierDurationConverter().Convert(data.DurationByCounters, null, null, null).ToString();

                foreach (TelematicsLogAdapter.Discontinuity discontinuity in data.Discontinuities) {
                    detail.Discontinuities.Text += discontinuity.CounterStart.ToString("0") + " - " + discontinuity.CounterEnd.ToString("0") + "\r\n";
                }

                StackPanel.Children.Add(detail);
            }
        }
    }
}
