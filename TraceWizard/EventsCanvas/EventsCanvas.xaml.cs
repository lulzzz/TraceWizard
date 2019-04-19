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
using System.Globalization;

using System.ComponentModel;

using TraceWizard.Entities;
using TraceWizard.Classification;
using TraceWizard.Notification;

namespace TraceWizard.TwApp {
    public partial class EventsCanvas : ScrollableCanvas, INotifyPropertyChanged {

        public Events Events { get; set; }
        public int MaximumVolumeInTrace { get { return (int)Math.Ceiling(Events.SuperPeak); } }
        public double MaximumSecondsInTrace { get { return Events.Duration.TotalSeconds; } }
        public double HeightMultiplier { get; set; }
        public double WidthMultiplier { get; set; }
        public bool IsDragging { get; set; }
        public Point MousePosition { get; set; }
        public AnalysisPanel.TwManualClassificationMode ManualClassificationMode = AnalysisPanel.TwManualClassificationMode.None;
        public UndoPosition UndoPosition;
        public Classifier ClassifierDisaggregation;

        public Polygon SelectedPolygon;

        EventsViewer eventsViewer = null;
        public EventsViewer EventsViewer {
            get { return eventsViewer; } 
            set {
            eventsViewer = value;
            ScrollViewer = eventsViewer.ScrollViewer;
            } 
        }
        ScrollViewer ScrollViewer { get; set; }

        public EventsCanvas() {
            InitializeComponent();
        }

        public void Initialize() {

            this.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            this.SnapsToDevicePixels = false;
            this.VerticalAlignment = VerticalAlignment.Bottom;

            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(EventsCanvas_PreviewMouseLeftButtonDown);
            this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(EventsCanvas_PreviewMouseLeftButtonUp);
            this.MouseMove += new MouseEventHandler(EventsCanvas_MouseMove);
            this.MouseEnter += new MouseEventHandler(EventsCanvas_MouseEnter);
            this.MouseLeave += new MouseEventHandler(EventsCanvas_MouseLeave);
        }

        Point startPoint;
        Point startScrollViewerOffset;
        bool initiatedDragNearTop = false;
        void EventsCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (Mouse.OverrideCursor == null & (sender as Polygon == null))
                Mouse.OverrideCursor = Cursors.Hand;

            startPoint = e.GetPosition(ScrollViewer);
            initiatedDragNearTop = (startPoint.Y < (ScrollViewer.ViewportHeight / 2));

            startScrollViewerOffset = new Point(ScrollViewer.HorizontalOffset, ScrollViewer.VerticalOffset);
        }

        void EventsCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (Mouse.OverrideCursor == Cursors.Hand)
                Mouse.OverrideCursor = null;
        }

        void EventsCanvas_MouseEnter(object sender, MouseEventArgs e) {
            startPoint = new Point();
            if (Mouse.OverrideCursor == Cursors.Hand)
                Mouse.OverrideCursor = null;
        }

        void EventsCanvas_MouseLeave(object sender, MouseEventArgs e) {
            if (Mouse.OverrideCursor == Cursors.Hand)
                Mouse.OverrideCursor = null;
        }

        void EventsCanvas_MouseMove(object sender, MouseEventArgs e) {
            if (startPoint.X == 0 && startPoint.Y == 0)
                return;

            // Get the current mouse position
            Point mousePos = e.GetPosition(ScrollViewer);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)) {
                ScrollViewer.ScrollToHorizontalOffset(startScrollViewerOffset.X + diff.X);
                if (initiatedDragNearTop)
                    ScrollViewer.ScrollToVerticalOffset(startScrollViewerOffset.Y + diff.Y);
            }
        }
        public virtual void Render() {
            Children.Clear();
            Resize();
            RenderEvents();
        }

        void Resize() {
            Width = MaximumSecondsInTrace * WidthMultiplier;
            Height = MaximumVolumeInTrace * HeightMultiplier;
        }
        
        void RenderEvents() {
            foreach (Event @event in Events)
                RenderEvent(@event);
        }

        public virtual void RenderEvent(Event @event) {
            var eventPolygon = new EventPolygon();
            eventPolygon.Event = @event;
            eventPolygon.WidthMultiplier = WidthMultiplier;
            eventPolygon.HeightMultiplier = HeightMultiplier;
            eventPolygon.PropertyChanged +=new PropertyChangedEventHandler(eventPolygon_PropertyChanged);
            eventPolygon.ClassifierDisaggregation = ClassifierDisaggregation;
            eventPolygon.Initialize();
            eventPolygon.EventsCanvas = this;
            
            double secondsOffset = @event.StartTime.Subtract(Events.StartTime).TotalSeconds * WidthMultiplier;
            System.Windows.Controls.Canvas.SetLeft(eventPolygon, secondsOffset);
            System.Windows.Controls.Canvas.SetBottom(eventPolygon, @event.Baseline * HeightMultiplier);

            this.Children.Add(eventPolygon);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        void eventPolygon_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case TwNotificationProperty.OnMouseEnterPolygon:
                    OnPropertyChanged(TwNotificationProperty.OnMouseEnterPolygon);
                    break;
                case TwNotificationProperty.OnMouseLeavePolygon:
                    OnPropertyChanged(TwNotificationProperty.OnMouseLeavePolygon);
                    break;
                case TwNotificationProperty.OnStartDrag:
                    OnPropertyChanged(TwNotificationProperty.OnStartDrag);
                    break;
                case TwNotificationProperty.OnEndDrag:
                    OnPropertyChanged(TwNotificationProperty.OnEndDrag);
                    break;
            }
        }

        public virtual void Remove(Polygon polygon) {
            this.Children.Remove(polygon);
        }

        public virtual void Remove(Event @event) {
            var polygon = @event.Tag as Polygon;
            if (polygon != null && SelectedPolygon == polygon)
                SelectedPolygon = null;
            this.Children.Remove((Polygon)@event.Tag);
        }
    }
}
