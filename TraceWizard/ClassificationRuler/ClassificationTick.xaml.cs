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
using TraceWizard.Notification;

namespace TraceWizard.TwApp {
    public partial class ClassificationTick : UserControl {
        public Event Event;
        public ClassificationRuler.OnRemove OnRemove;
        public ClassificationRuler.OnChangePosition OnChangePosition;

        public ClassificationTick() {
            InitializeComponent();
        }

        public void Initialize() {
            SetVisibility();
            Event.PropertyChanged += new PropertyChangedEventHandler(Event_PropertyChanged);
        }

        void SetVisibility() {
            if (Event.ManuallyClassified) {
                TickManuallyClassified.Visibility = Visibility.Visible;
                TickClassifiedUsingFixtureList.Visibility = Visibility.Collapsed;
                TickClassifiedUsingMachineLearning.Visibility = Visibility.Collapsed;
//                this.ToolTip = "Manually Classified";
            } else if (Event.ClassifiedUsingFixtureList) {
                TickManuallyClassified.Visibility = Visibility.Collapsed;
                TickClassifiedUsingFixtureList.Visibility = Visibility.Visible;
                TickClassifiedUsingMachineLearning.Visibility = Visibility.Collapsed;
//                this.ToolTip = "Classified Using Fixture List";
            } else {
                TickManuallyClassified.Visibility = Visibility.Collapsed;
                TickClassifiedUsingFixtureList.Visibility = Visibility.Collapsed;
                TickClassifiedUsingMachineLearning.Visibility = Visibility.Visible;
//                this.ToolTip = "Classified Using Machine Learning";
            }
        }

        void Event_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case TwNotificationProperty.OnEndClassify:
                    SetVisibility();
                    break;
                case TwNotificationProperty.OnChangeStartTime:
                    OnChangePosition(this);
                    break;
                case TwNotificationProperty.OnRemoveEvent:
                    OnRemove(this);
                    break;
            }
        }
    }
}
