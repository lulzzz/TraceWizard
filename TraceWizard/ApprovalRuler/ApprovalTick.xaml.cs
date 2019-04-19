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
    public partial class ApprovalTick : UserControl {
        public Event Event;
        public ApprovalRuler.OnRemove OnRemove;
        public ApprovalRuler.OnChangePosition OnChangePosition;

        public ApprovalTick() {
            InitializeComponent();
        }

        public void Initialize() {
            SetVisibility();
            Event.PropertyChanged += new PropertyChangedEventHandler(Event_PropertyChanged);
        }

        void SetVisibility() {
            Tick.Visibility = Event.ManuallyApproved ? Visibility.Visible : Visibility.Collapsed;
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
