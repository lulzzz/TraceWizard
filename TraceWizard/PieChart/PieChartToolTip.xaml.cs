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
    public partial class PieChartToolTip : UserControl {
        FixtureClass fixtureClass;
        public FixtureClass FixtureClass {
            get { return fixtureClass; }
            set {
                fixtureClass = value;
                Initialize();
            }
        }

        bool byInstances;
        public bool ByInstances {
            get { return byInstances; }
            set {
                byInstances = value;
                Initialize();
            }
        }

        string data;
        public string Data {
            get { return data; }
            set {
                data = value;
                Initialize();
            }
        }

        double percent;
        public double Percent {
            get { return percent; }
            set {
                percent = value;
                Initialize();
            }
        }

        public PieChartToolTip() {
            InitializeComponent();
        }

        public PieChartToolTip(FixtureClass fixtureClass, bool byInstances, string data, double percent) {
            InitializeComponent();

            this.fixtureClass = fixtureClass;
            this.byInstances = byInstances;
            this.data = data;
            this.percent = percent;

            Initialize();
        }

        void Initialize() {
            Border.Child = TwGui.FixtureWithImageLeft(fixtureClass, FontWeights.Bold);
            if (byInstances)
                Label.Text = "Number of " + fixtureClass.FriendlyName + " Events: " + data + " \r\nPercentage of Events that are " + fixtureClass.FriendlyName + " Events: " + (percent * 100.0).ToString("0.0") + "%";
            else
                Label.Text = "Total Volume of " + fixtureClass.FriendlyName + " Events: " + data + " \r\nPercentage of Total Volume Attributed to " + fixtureClass.FriendlyName + " Events: " + (percent * 100.0).ToString("0.0") + "%";
        }
    }
}
