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
    public partial class SelectionRuler : UserControl {

        public Events Events { get; set; }
        public bool VisibleRequested;

        public bool CollapsedNotHidden;

        public SelectionRuler() {
            InitializeComponent();

            Properties.Settings.Default.PropertyChanged += new PropertyChangedEventHandler(Default_PropertyChanged);
        }

        public void Events_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case TwNotificationProperty.OnAddEvent:
                    Render();
                    break;
            }
        }

        public void Initialize() {
            Events.PropertyChanged += new PropertyChangedEventHandler(Events_PropertyChanged);

            SetSettingTopAlignment();

            VisibleRequested = Properties.Settings.Default.ShowSelectionRulerByDefault;
            CollapsedNotHidden = !Properties.Settings.Default.ShowSelectionRulerByDefault;
            SetVisibility();
        }

        void SetVisibility() {
            if (VisibleRequested)
                Visibility = Visibility.Visible;
            else
                Visibility = CollapsedNotHidden ? Visibility.Collapsed : Visibility.Hidden;
        }
        
        void Default_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "ShowSelectionRulerAboveGraph":
                    SetTopAlignment();
                    SetSettingTopAlignment();
                    break;
                case "SelectionTickColor":
                    break;
            }
        }

        public static readonly DependencyProperty TopAlignmentProperty =
          DependencyProperty.Register("TopAlignment", typeof(bool),
          typeof(SelectionRuler), new UIPropertyMetadata(true));

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
            TopAlignment = Properties.Settings.Default.ShowSelectionRulerAboveGraph;
        }

        public void Clear() {
            eventsInRuler.Clear();
            Canvas.Children.Clear();
        }

        double WidthMultiplier = 0.0;
        
        public void Render() {
            Render(WidthMultiplier);
        }

        public void Render(double widthMultiplier) {

            WidthMultiplier = widthMultiplier;

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
            var tick = new SelectionTick();
            tick.Event = @event;
            tick.OnRemove = Remove;
            tick.OnChangePosition = ChangePosition;
            tick.Initialize();

            const int widthOfTick = 6;
            Canvas.SetBottom(tick, 0);
            double offset = (@event.StartTime.Subtract(Events.StartTime).TotalSeconds * WidthMultiplier) - widthOfTick / 2;
            Canvas.SetLeft(tick, offset);
            Canvas.Children.Add(tick);

            eventsInRuler.Add(@event);
        }

        public delegate void OnRemove(SelectionTick tick);
        void Remove(SelectionTick tick) {
            eventsInRuler.Remove(tick.Event);
            Canvas.Children.Remove(tick);
        }

        public delegate void OnChangePosition(SelectionTick tick);
        void ChangePosition(SelectionTick tick) {
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
