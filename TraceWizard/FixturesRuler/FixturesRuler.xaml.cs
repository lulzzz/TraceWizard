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

using System.ComponentModel;

using TraceWizard.Entities;
using TraceWizard.Notification;

namespace TraceWizard.TwApp {
    public partial class FixturesRuler : UserControl {

        public Events Events { get; set; }
        public double ViewportSeconds;

        public bool CollapsedNotHidden;

        public bool VisibleRequested;

        public FixturesRuler() {
            InitializeComponent();

            Properties.Settings.Default.PropertyChanged += new PropertyChangedEventHandler(Default_PropertyChanged);
        }

        public void Events_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
//                case TwNotificationProperty.OnEndMergeSplit:
                case TwNotificationProperty.OnEndClassify:
                case TwNotificationProperty.OnAddEvent:
                    Render();
                    break;
            }
        }

        public void Initialize() {
            Events.PropertyChanged += new PropertyChangedEventHandler(Events_PropertyChanged);

            SetSettingTopAlignment();

            VisibleRequested = Properties.Settings.Default.ShowFixturesRulerByDefault;
            CollapsedNotHidden = !Properties.Settings.Default.ShowFixturesRulerByDefault;
            SetVisibility();
        }

        void SetVisibility() {
            if (VisibleRequested && ViewportSeconds <= EventsViewer.SecondsTwentyMinutes)
                Visibility = Visibility.Visible;
            else
                Visibility = CollapsedNotHidden ? Visibility.Collapsed : Visibility.Hidden;
        }
        
        void Default_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "ShowFixturesRulerAboveGraph":
                    SetTopAlignment();
                    SetSettingTopAlignment();
                    break;
            }
        }

        public static readonly DependencyProperty TopAlignmentProperty =
          DependencyProperty.Register("TopAlignment", typeof(bool),
          typeof(FixturesRuler), new UIPropertyMetadata(true));

        public bool TopAlignment {
            get { return (bool)GetValue(TopAlignmentProperty); }
            set {
                SetValue(TopAlignmentProperty, value);
                SetTopAlignment();
            }
        }

        void SetTopAlignment() {
            var panel = this.Parent as DockPanel;
            DockPanel.SetDock((DockPanel)this.Parent, TopAlignment ? Dock.Top : Dock.Bottom);
        }

        void SetSettingTopAlignment() {
            TopAlignment = Properties.Settings.Default.ShowFixturesRulerAboveGraph;
        }

        double WidthMultiplier = 0.0;

        public void Clear() {
            eventsInRuler.Clear();
            Canvas.Children.Clear();
        }

        public void Render() {
            Render(WidthMultiplier, ViewportSeconds);
        }

        public void Render(double widthMultiplier, double viewportSeconds) {

            WidthMultiplier = widthMultiplier;
            ViewportSeconds = viewportSeconds;

            SetVisibility();
            if (Visibility != Visibility.Visible) {
                Clear();
                return;
            }

            if (Canvas.Children.Count == 0)
                RenderInitial();
            else
                RenderUpdate();
        }

        List<Event> eventsInRuler = new List<Event>();

        void Add(Event @event) {
            var tick = new FixtureTick();
            tick.Event = @event;
            tick.OnRemove = Remove;
            tick.OnChangePosition = ChangePosition;
            tick.Initialize();

            Canvas.SetBottom(tick, 0);

            if (tick.Event.Channel == Channel.Super)
                Canvas.SetBottom(tick, Height/2);
            else
                Canvas.SetBottom(tick, 0);

            double offset = @event.StartTime.Subtract(Events.StartTime).TotalSeconds * WidthMultiplier;
            Canvas.SetLeft(tick, offset);
            Canvas.Children.Add(tick);

            eventsInRuler.Add(@event);
        }

        public delegate void OnRemove(FixtureTick tick);
        void Remove(FixtureTick tick) {
            eventsInRuler.Remove(tick.Event);
            Canvas.Children.Remove(tick);
        }

        public delegate void OnChangePosition(FixtureTick tick);
        void ChangePosition(FixtureTick tick) {
            double offset = tick.Event.StartTime.Subtract(Events.StartTime).TotalSeconds * WidthMultiplier;
            Canvas.SetLeft(tick, offset);
        }

        void RenderInitial() {
            foreach (Event @event in Events)
                Add(@event);
        }

        void RenderUpdate() {
            foreach (Event @event in Events) {
                if (!eventsInRuler.Contains(@event))
                    Add(@event);
            }
        }
    }
}
