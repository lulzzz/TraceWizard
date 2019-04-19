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
using TraceWizard.Adoption;
using TraceWizard.Services;
using TraceWizard.Notification;
using TraceWizard.Environment;
using TraceWizard.Classification;

namespace TraceWizard.TwApp {
    public partial class EventPolygon : UserControl, INotifyPropertyChanged {
        public Event Event {get; set;}
        public double WidthMultiplier;
        public double HeightMultiplier;
        public Classifier ClassifierDisaggregation;

        Events Events { get; set; }
        
        EventsCanvas eventsCanvas;
            public EventsCanvas EventsCanvas {
            get { return eventsCanvas; }
            set {
                eventsCanvas = value;
                Events = eventsCanvas.Events;
            }
        }

        EventPolygonMenu contextMenu = null;

        public static RoutedUICommand HorizontalSplitCommand;
        public static RoutedUICommand VerticalSplitCommand;
        public static RoutedUICommand SelectCommand;
        public static RoutedUICommand AddToSelectionCommand;
        public static RoutedUICommand ExtendSelectionCommand;

        CommandBinding horizontalSplitCommandBinding;
        CommandBinding verticalSplitCommandBinding;
        CommandBinding selectCommandBinding;
        CommandBinding addToSelectionCommandBinding;
        CommandBinding extendSelectionCommandBinding;

        Point? MousePosition { get; set; }

        public EventPolygon() {
            InitializeComponent();
        }

        public void Initialize() {

            InitializePolygon();

            InitializeContextMenuCommandBinding();

            InitializeSplitCommandBinding();
            InitializeSelectCommandBinding();
            TerminateZoomCommandBinding();

            this.ContextMenuOpening += new ContextMenuEventHandler(eventPolygon_ContextMenuOpening);
            this.ContextMenuClosing += new ContextMenuEventHandler(eventPolygon_ContextMenuClosing);

            Event.PropertyChanged += new PropertyChangedEventHandler(event_PropertyChanged);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        void event_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case TwNotificationProperty.OnEndClassify:
                case TwNotificationProperty.OnEndSelect:
                    Polygon.Fill = GetBrush();
                    break;
            }
        }

        public Point GetMouseClickPosition() {
            if (MousePosition.HasValue)
                return MousePosition.Value;
            else
                return Mouse.GetPosition(Polygon);
        }

        void eventPolygon_ContextMenuOpening(object sender, ContextMenuEventArgs e) {
            MousePosition = Mouse.GetPosition(Polygon);
            contextMenu = new EventPolygonMenu();
            contextMenu.EventPolygon = this;
            contextMenu.ManualClassificationMode = EventsCanvas.ManualClassificationMode;

            contextMenu.Initialize();

            Polygon.ContextMenu = contextMenu;
        }

        void eventPolygon_ContextMenuClosing(object sender, ContextMenuEventArgs e) {
            MousePosition = null;
        }

        void InitializePolygon() {
            Polygon.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            Polygon.SnapsToDevicePixels = false;

            InitializePolygonPath();
            InitializePolygonBrush();
            InitializeMouse();

            Polygon.Tag = Event;
            Event.Tag = Polygon;

            PositionToolTip();
        }

        void InitializeMouse() {
            this.AllowDrop = true;
            this.Drop += new DragEventHandler(polygon_DragDrop);
            this.MouseMove += new MouseEventHandler(polygon_MouseMove);

            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(polygon_PreviewMouseLeftButtonDown);
            this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(polygon_PreviewMouseLeftButtonUp);
            this.PreviewMouseMove += new MouseEventHandler(polygon_PreviewMouseMove);

            this.MouseEnter += polygon_MouseEnter;
            this.MouseLeave += polygon_MouseLeave;
        }
        
        void InitializePolygonBrush() {
            Polygon.Fill = GetBrush();
            Polygon.Stroke = null;
            Polygon.StrokeThickness = 0.0;
        }
        
        void InitializePolygonPath() {
            Polygon.Points.Add(new Point(0 * WidthMultiplier, 0));
            foreach (Flow flow in Event) {
                Point startPoint = new Point(flow.StartTime.Subtract(Event.StartTime).TotalSeconds * WidthMultiplier,
                    -flow.Peak * HeightMultiplier);
                Polygon.Points.Add(startPoint);
                Point endPoint = new Point(startPoint.X + (flow.Duration.TotalSeconds * WidthMultiplier), startPoint.Y);
                Polygon.Points.Add(endPoint);
            }
            Polygon.Points.Add(new Point(Event.Duration.TotalSeconds * WidthMultiplier, 0));
        }

        Point startPoint = new Point();
        void polygon_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            startPoint = e.GetPosition(null);
            originatedMouseDown = true;
        }
        void polygon_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            originatedMouseDown = false;
        }

        bool originatedMouseDown = false;

        void polygon_PreviewMouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed && ((LinedEventsCanvas)(Parent)).CanStartDragging && originatedMouseDown) {
                Point mousePos = e.GetPosition(null);
                Vector diff = startPoint - mousePos;
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    var eventPolygon = sender as EventPolygon;
                    var polygon = eventPolygon.Polygon as Polygon;
                    OnPropertyChanged(TwNotificationProperty.OnStartDrag);

                    if (eventPolygon.Event.Selected && Events.SelectedEvents.Count > 1) {
                        var polygons = new List<Polygon>();
                        foreach(var @event in Events.SelectedEvents)
                            polygons.Add((Polygon)@event.Tag);
                        DragDrop.DoDragDrop(polygon, new DataObject(typeof(List<Polygon>), polygons), DragDropEffects.All);
                    } else
                        DragDrop.DoDragDrop(polygon, new DataObject(typeof(Polygon), polygon), DragDropEffects.All);

                    OnPropertyChanged(TwNotificationProperty.OnEndDrag);
                    originatedMouseDown = false;
                }
            }
            e.Handled = true;
        }

        void polygon_MouseMove(object sender, MouseEventArgs e) {
            if (Properties.Settings.Default.ShowDetailedEventToolTips) {
                ShowEventProperties(sender, e, false);
            }
        }

        bool AdoptionRequested() {
            return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)
                || EventsCanvas.ManualClassificationMode == AnalysisPanel.TwManualClassificationMode.WithAdoption;
        }

        void DispatchDrop(FixtureClass fixtureClass) {
            if (fixtureClass != null) {
                bool withAdoption = AdoptionRequested();

                if (Events.SelectedEvents.Contains(Event))
                    Events.ManuallyClassify(Events.SelectedEvents, fixtureClass, withAdoption, EventsCanvas.UndoPosition);
                else
                    Events.ManuallyClassify(Event, fixtureClass, withAdoption, EventsCanvas.UndoPosition);
            } else {
                if (Events.SelectedEvents.Contains(Event))
                    Events.ManuallyToggleFirstCycle(Events.SelectedEvents, false, null, EventsCanvas.UndoPosition);
                else
                    Events.ManuallyToggleFirstCycle(Event, EventsCanvas.UndoPosition);
            }
        }

        void DispatchDrop(Polygon polygonSource, DragEventArgs e) {
            Event eventTarget = Event;
            Event eventSource = polygonSource.Tag as Event;

            if (eventSource == eventTarget)
                return;

            if (!Merge(eventSource, eventTarget, e))
                MessageBox.Show(
                    "Cannot merge",
                    TwAssembly.Title(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

        }

        bool Merge(Event eventSource, Event eventTarget, DragEventArgs e) {
            if (Events.CanMergeVertically(eventSource, eventTarget)) {
                DispatchDragMergeVertical(eventSource, eventTarget, e.KeyStates);
            } else if (Events.CanMergeHorizontally(eventSource, eventTarget)) {
                DispatchDragMergeHorizontal(eventSource, eventTarget, e.KeyStates);
            } else
                return false;
            return true;
        }

        public int MergeAllVerticallyIntoBase(Event eventTarget, List<Event> events) {
            return Events.MergeAllVerticallyIntoBaseWithNotification(eventTarget, events, ClassifierDisaggregation, eventsCanvas.Remove, eventsCanvas.RenderEvent, eventsCanvas.EventsViewer.UndoPosition);
        }

        void DispatchDrop(List<Polygon> polygons, DragEventArgs e) {
            Event eventTarget = Event;

            var events = new List<Event>();
            foreach(Polygon polygon in polygons)
                events.Add((Event)polygon.Tag);

            if (Events.CanMergeAllVerticallyIntoBase(eventTarget, events))
                MergeAllVerticallyIntoBase(eventTarget, events);
            else
                MessageBox.Show(
                    "Cannot merge all",
                    TwAssembly.Title(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
        }

        void polygon_DragDrop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(typeof(FixtureButton))) {
                FixtureButton fixtureButton = e.Data.GetData(typeof(FixtureButton)) as FixtureButton;
                DispatchDrop(fixtureButton.FixtureClass);
            } else if (e.Data.GetDataPresent(typeof(ShortFixtureLabel))) {
                var shortFixtureLabel = e.Data.GetData(typeof(ShortFixtureLabel)) as ShortFixtureLabel;
                DispatchDrop(shortFixtureLabel.FixtureClass);
            } else if (e.Data.GetDataPresent(typeof(Polygon))) {
                DispatchDrop((Polygon)e.Data.GetData(typeof(Polygon)), e);
            } else if (e.Data.GetDataPresent(typeof(List<Polygon>))) {
                DispatchDrop((List<Polygon>)e.Data.GetData(typeof(List<Polygon>)), e);
            }
        }

        void DispatchDragMergeVertical(Event eventSource, Event eventTarget, DragDropKeyStates keyStates) {
            if (keyStates == DragDropKeyStates.ControlKey || ((LinedEventsCanvas)(Parent)).MergeAllIntoBaseMode) {
                MergeAllVerticallyIntoBase(eventTarget);
            } else {
                MergeVertically(eventSource, eventTarget, true);
            }
        }

        public void MergeVertically(Event eventSource, Event eventTarget, bool remove) {
            Events.MergeVerticallyWithNotification(eventSource, eventTarget, remove, ClassifierDisaggregation, eventsCanvas.Remove, eventsCanvas.RenderEvent, eventsCanvas.EventsViewer.UndoPosition);
        }

        public int MergeAllVerticallyIntoBase(Event eventTarget) {
            Event @event = eventTarget.Channel == Channel.Super ? eventTarget.BaseEvent : eventTarget;
            return Events.MergeAllVerticallyIntoBaseWithNotification(@event, @event.SuperEvents, ClassifierDisaggregation, eventsCanvas.Remove, eventsCanvas.RenderEvent, eventsCanvas.EventsViewer.UndoPosition);
        }

        void DispatchDragMergeHorizontal(Event eventSource, Event eventTarget, DragDropKeyStates keyStates) {
            if (keyStates == DragDropKeyStates.ControlKey || ((LinedEventsCanvas)(Parent)).MergeAllIntoBaseMode) {
                MergeAllHorizontally(eventTarget);
            } else {
                MergeHorizontally(eventSource, eventTarget, true);
            }
        }

        public bool MergePreviousHorizontally(Event eventTarget) {
            return Events.MergePreviousHorizontallyWithNotification(eventTarget, ClassifierDisaggregation, eventsCanvas.Remove, eventsCanvas.RenderEvent, eventsCanvas.EventsViewer.UndoPosition);
        }

        public bool MergeNextHorizontally(Event eventTarget) {
            return Events.MergeNextHorizontallyWithNotification(eventTarget, ClassifierDisaggregation, eventsCanvas.Remove, eventsCanvas.RenderEvent, eventsCanvas.EventsViewer.UndoPosition);
        }
        
        public int MergeAllHorizontally(Event eventTarget) {
            return Events.MergeAllHorizontallyWithNotification(
                eventTarget, ClassifierDisaggregation, eventsCanvas.Remove, eventsCanvas.RenderEvent, eventsCanvas.EventsViewer.UndoPosition);
        }

        public void MergeHorizontally(Event eventSource, Event eventTarget, bool remove) {
            Events.MergeHorizontallyWithNotification(eventSource, eventTarget, remove, ClassifierDisaggregation, eventsCanvas.Remove, eventsCanvas.RenderEvent, eventsCanvas.EventsViewer.UndoPosition);
        }

        void polygon_MouseEnter(object sender, MouseEventArgs e) {
            Event.UpdateSimilarCounts();
            Events.CurrentEvent = Event;
            OnPropertyChanged(TwNotificationProperty.OnMouseEnterPolygon);
            ShowEventProperties(sender, e, true);
            e.Handled = true;
        }

        void polygon_MouseLeave(object sender, MouseEventArgs e) {
            Events.CurrentEvent = null;
            OnPropertyChanged(TwNotificationProperty.OnMouseLeavePolygon);
            e.Handled = true;
        }

        void ShowEventProperties(object sender, MouseEventArgs e, bool performUpdate) {
            if (Properties.Settings.Default.ShowEventToolTips) {
                var eventProperties = new EventProperties();
                eventProperties.@event = Event;
                eventProperties.PerformUpdate = performUpdate;
                eventProperties.mousePosition = e.GetPosition(Polygon);
                eventProperties.widthMultiplier = EventsCanvas.WidthMultiplier;
                eventProperties.heightMultiplier = EventsCanvas.HeightMultiplier;
                eventProperties.Initialize();

                Polygon.ToolTip = eventProperties;
            } else
                Polygon.ToolTip = null;
        }

        void HorizontalSplitCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (Events.CanSplitHorizontally(Event));
            e.Handled = true;
        }

        void HorizontalSplitExecuted(object sender, ExecutedRoutedEventArgs e) {
            MousePosition = Mouse.GetPosition(Polygon);
            HorizontalSplit();
            eventsCanvas.EventsViewer.ScrollViewer.Focus();
        }

        void HorizontalSplit() {
            SplitHorizontally(GetMouseClickPosition());
        }
        
        public void SplitHorizontally(Point point) {
            if (!Event.CanSplitHorizontally(point.X, point.Y, 
                eventsCanvas.WidthMultiplier, eventsCanvas.HeightMultiplier)) {
                MessageBox.Show(
                    "Cannot split into contiguous events",
                    TwAssembly.Title(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            SplitHorizontally(Event, point);
            MousePosition = null;
        }

        public void SplitHorizontally(Event @event, Point point) {
            DateTime splitTime = @event.GetSplitDate(point.X, eventsCanvas.WidthMultiplier);
            Events.SplitHorizontallyWithNotification(@event, splitTime, ClassifierDisaggregation, eventsCanvas.Remove, eventsCanvas.RenderEvent, false, eventsCanvas.EventsViewer.UndoPosition);
        }

        void VerticalSplitCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
            e.Handled = true;
        }

        void VerticalSplitExecuted(object sender, ExecutedRoutedEventArgs e) {
            MousePosition = Mouse.GetPosition(Polygon);
            SplitVertically(sender as Polygon, GetMouseClickPosition());
            eventsCanvas.EventsViewer.ScrollViewer.Focus();
        }

        public void SplitVertically(Polygon polygon, Point point) {

            if (!Events.CanSplitVertically(Event)) {
                MessageBox.Show(
                    "Cannot split into simultaneous events",
                    TwAssembly.Title(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            DateTime dateTime = Event.GetSplitDate(point.X, EventsCanvas.WidthMultiplier);
            double rate = Event.GetSplitRate(point.Y, EventsCanvas.HeightMultiplier);
            if (!Event.IsLegal(dateTime, rate))
                return;

            SplitVertically(Event, point,  eventsCanvas.WidthMultiplier, eventsCanvas.HeightMultiplier);
            MousePosition = null;
        }

        public Event SplitVertically(Event @event, Point point, double widthMultiplier, double heightMultiplier) {
            return Events.SplitVerticallyWithNotification(@event, @event.GetSplitDate(point.X, widthMultiplier),
                @event.GetSplitRate(point.Y, heightMultiplier),
                ClassifierDisaggregation, eventsCanvas.Remove, eventsCanvas.RenderEvent, eventsCanvas.EventsViewer.UndoPosition);
        }

        bool DispatchSplitModes(object sender, ExecutedRoutedEventArgs e) {
            if (((LinedEventsCanvas)(Parent)).HorizontalSplitMode) {
                HorizontalSplitExecuted(sender, e);
                return true;
            } else if (((LinedEventsCanvas)(Parent)).VerticalSplitMode) {
                VerticalSplitExecuted(sender, e);
                return true;
            } else
                return false;
        }

        void SelectExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (DispatchSplitModes(sender, e))
                return;

            Events.ClearSelectedEventsLow(Event);
            Events.ToggleSelected(Event);
            if (Event.Selected)
                EventsCanvas.SelectedPolygon = Polygon;
            else if (Events.SelectedEvents.Count > 0)
                EventsCanvas.SelectedPolygon = (Polygon)(Events.SelectedEvents[Events.SelectedEvents.Count - 1].Tag);
            else {
                EventsCanvas.SelectedPolygon = null;
            }

            eventsCanvas.EventsViewer.ScrollViewer.Focus();
        }


        void AddToSelectionExecuted(object sender, ExecutedRoutedEventArgs e) {
            //if (DispatchSplitModes(sender, e))
            //    return;

            var polygon = sender as Polygon;

            Events.ToggleSelected(Event);

            if (Event.Selected)
                EventsCanvas.SelectedPolygon = Polygon;
            else if (Events.SelectedEvents.Count > 0)
                EventsCanvas.SelectedPolygon = (Polygon)(Events.SelectedEvents[Events.SelectedEvents.Count-1].Tag);
            else {
                EventsCanvas.SelectedPolygon = null;
            }

            eventsCanvas.EventsViewer.ScrollViewer.Focus();

        }

        void ExtendSelectionExecuted(object sender, ExecutedRoutedEventArgs e) {
            //if (DispatchSplitModes(sender, e))
            //    return;

            var polygon = sender as Polygon;

            Polygon previouslySelectedPolygon = EventsCanvas.SelectedPolygon;

            if (previouslySelectedPolygon == null)
                return;

            Event previouslySelectedEvent = (Event) previouslySelectedPolygon.Tag;
            
            if (Event.Selected) {
                Events.ToggleSelected(Event);
                if (Events.SelectedEvents.Count > 0) {
                    EventsCanvas.SelectedPolygon = (Polygon)(Events.SelectedEvents[Events.SelectedEvents.Count - 1].Tag);
                } else {
                    EventsCanvas.SelectedPolygon = null;
                }
            } else {
                Events.SelectRange(previouslySelectedEvent, Event);
                EventsCanvas.SelectedPolygon = Polygon;
            }

            eventsCanvas.EventsViewer.ScrollViewer.Focus();
        }

        void CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
            e.Handled = true;
        }

        void InitializeSplitCommandBinding() {
            HorizontalSplitCommand = new RoutedUICommand("Horizontal Split", "HorizontalSplit", typeof(EventPolygon));
            horizontalSplitCommandBinding = new CommandBinding(
                HorizontalSplitCommand,
                HorizontalSplitExecuted,
                HorizontalSplitCanExecute);

            Polygon.CommandBindings.Add(horizontalSplitCommandBinding);
            Polygon.InputBindings.Add(
              new MouseBinding(HorizontalSplitCommand, new MouseGesture(MouseAction.LeftClick, ModifierKeys.Alt)));

            VerticalSplitCommand = new RoutedUICommand("Vertical Split", "VerticalSplit", typeof(EventPolygon));
            verticalSplitCommandBinding = new CommandBinding(
                VerticalSplitCommand,
                VerticalSplitExecuted,
                VerticalSplitCanExecute);

            Polygon.CommandBindings.Add(verticalSplitCommandBinding);
            Polygon.InputBindings.Add(
              new MouseBinding(VerticalSplitCommand, new MouseGesture(MouseAction.LeftClick, ModifierKeys.Control | ModifierKeys.Alt)));
        }

        void TerminateZoomCommandBinding() {
            var DummyCommand = new RoutedUICommand("DummyCommand", "Dummy Command", typeof(EventPolygon));

            Polygon.InputBindings.Add(
              new MouseBinding(DummyCommand, new MouseGesture(MouseAction.LeftDoubleClick, ModifierKeys.None)));

            Polygon.InputBindings.Add(
              new MouseBinding(DummyCommand, new MouseGesture(MouseAction.LeftDoubleClick, ModifierKeys.Shift)));

            Polygon.InputBindings.Add(
              new MouseBinding(DummyCommand, new MouseGesture(MouseAction.MiddleDoubleClick, ModifierKeys.None)));

            Polygon.InputBindings.Add(
              new MouseBinding(DummyCommand, new MouseGesture(MouseAction.LeftDoubleClick, ModifierKeys.Control)));

            Polygon.InputBindings.Add(
              new MouseBinding(DummyCommand, new MouseGesture(MouseAction.LeftDoubleClick, ModifierKeys.Shift | ModifierKeys.Control)));
        }

        void InitializeSelectCommandBinding() {
            SelectCommand = new RoutedUICommand("Select", "Select", typeof(EventPolygon));
            selectCommandBinding = new CommandBinding(
                SelectCommand,
                SelectExecuted,
                CanExecute);

            Polygon.CommandBindings.Add(selectCommandBinding);
            Polygon.InputBindings.Add(
              new MouseBinding(SelectCommand, new MouseGesture(MouseAction.LeftClick, ModifierKeys.None)));

            AddToSelectionCommand = new RoutedUICommand("Select Additional", "SelectAdditional", typeof(EventPolygon));
            addToSelectionCommandBinding = new CommandBinding(
                AddToSelectionCommand,
                AddToSelectionExecuted,
                CanExecute);

            Polygon.CommandBindings.Add(addToSelectionCommandBinding);
            Polygon.InputBindings.Add(
              new MouseBinding(AddToSelectionCommand, new MouseGesture(MouseAction.LeftClick, ModifierKeys.Control)));

            ExtendSelectionCommand = new RoutedUICommand("Extend Selection", "ExtendSelection", typeof(EventPolygon));
            extendSelectionCommandBinding = new CommandBinding(
                ExtendSelectionCommand,
                ExtendSelectionExecuted,
                CanExecute);

            Polygon.CommandBindings.Add(extendSelectionCommandBinding);
            Polygon.InputBindings.Add(
              new MouseBinding(ExtendSelectionCommand, new MouseGesture(MouseAction.LeftClick, ModifierKeys.Shift)));
        }

        void InitializeContextMenuCommandBinding() {
            var contextMenuCanvasCommandBinding = new CommandBinding(
                ApplicationCommands.ContextMenu,
                ContextMenuExecuted,
                CanExecute);

            Polygon.CommandBindings.Add(contextMenuCanvasCommandBinding);

            Polygon.InputBindings.Add(
              new MouseBinding(ApplicationCommands.ContextMenu, new MouseGesture(MouseAction.RightClick, ModifierKeys.None)));
        }

        void ContextMenuExecuted(object sender, ExecutedRoutedEventArgs e) {
            eventPolygon_ContextMenuOpening(null, null);
            contextMenu.UpdateMenu();
            eventsCanvas.EventsViewer.ScrollViewer.Focus();
        }

        Brush GetBrush() {
            Brush brush;

            if (Event.Selected)
                    brush = TwBrushes.DiagonalLowerLeftStripedColorBrush(Event.FixtureClass.Color);
            else if (Event.FirstCycle)
                    brush = TwBrushes.HorizontalStripedColorBrush(Event.FixtureClass.Color);
            else if (Event.Channel == Channel.Super)
                brush = TwBrushes.DiagonalUpperLeftStripedColorBrush(Event.FixtureClass.Color);
            else
                brush = TwSingletonBrushes.Instance.FrozenSolidColorBrush(Event.FixtureClass.Color);
            return brush;
        }

        void PositionToolTip() {
            ToolTipService.SetShowDuration(Polygon, 60000);
            ToolTipService.SetInitialShowDelay(Polygon, 000);
            ToolTipService.SetPlacement(Polygon, System.Windows.Controls.Primitives.PlacementMode.MousePoint);
            ToolTipService.SetHorizontalOffset(Polygon, -200);
            ToolTipService.SetVerticalOffset(Polygon, -200);
        }
    }
}