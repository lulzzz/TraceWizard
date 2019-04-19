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
    public partial class FixtureTick : UserControl {
        public Event Event;
        public FixturesRuler.OnRemove OnRemove;
        public FixturesRuler.OnChangePosition OnChangePosition;

        public FixtureTick() {
            InitializeComponent();
        }

        public void Initialize() {
            SetVisibility();
            Event.PropertyChanged += new PropertyChangedEventHandler(Event_PropertyChanged);

            Update();
        }

        void Update() {
            Image.Source = TwGui.GetImage(Event.FixtureClass.ImageFilename);
            Border.Background = TwSingletonBrushes.Instance.FrozenSolidColorBrush(Event.FixtureClass.Color);
        }

        void SetVisibility() {
            ;
        }

        void Event_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case TwNotificationProperty.OnEndClassify:
                    Update();
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
