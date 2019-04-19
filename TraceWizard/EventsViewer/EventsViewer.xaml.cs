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
using TraceWizard.Classification;
using TraceWizard.Services;

using TraceWizard.Adoption;
using TraceWizard.Commanding;
using TraceWizard.Notification;

namespace TraceWizard.TwApp {
    public partial class EventsViewer : UserControl {

        protected double ViewportHeight { get; set; }

        public HorizontalRuler HorizontalRuler;
        public VerticalRuler VerticalRuler;
        public SelectionRuler SelectionRuler;
        public ApprovalRuler ApprovalRuler;
        public ClassificationRuler ClassificationRuler;
        public FixturesRuler FixturesRuler;

        AnalysisPanel.TwManualClassificationMode manualClassificationMode = AnalysisPanel.TwManualClassificationMode.None;
        public AnalysisPanel.TwManualClassificationMode ManualClassificationMode {
            get {return manualClassificationMode;}
            set {
                manualClassificationMode = value;
                LinedEventsCanvas.ManualClassificationMode = manualClassificationMode;
            }
        }

        public bool BookmarkIsSet() {
            return !(BookmarkPosition.IsNull);
        }
        public void SetBookmark() {
            BookmarkPosition.PercentElapsedHorizontal = UndoPosition.PercentElapsedHorizontal;
            BookmarkPosition.PercentElapsedVertical = UndoPosition.PercentElapsedVertical;
            BookmarkPosition.ViewportSeconds = UndoPosition.ViewportSeconds;
            BookmarkPosition.ViewportVolume = UndoPosition.ViewportVolume;
            BookmarkPosition.IsNull = false;
        }
        public void GoToBookmark() {
            RestoreScrollPosition(BookmarkPosition);
        }

        public void GoToStart() {
            ScrollViewer.ScrollToLeftEnd();
        }

        public void GoToEnd() {
            ScrollViewer.ScrollToRightEnd();
        }

        public UndoPosition UndoPosition = new UndoPosition();
        public UndoPosition BookmarkPosition = new UndoPosition(true);

        public Events Events { get; set; }
        public double ViewportSeconds { get; set; }
        public double ViewportVolume { get; set; }

        public const double VolumeTwenty = 20;
        public const double VolumeTen = 10;
        public const double VolumeFive = 5;
        public const double VolumeTwo = 2;
        public const double VolumeOne = 1;

        public bool zoomInProgress = false;

        double[] viewportSeconds;
        double[] viewportVolumes;
        double maxViewportVolume;
        double maxViewportSeconds;

        public const double SecondsSevenDays = 60 * 60 * 24 * 7;
        public const double SecondsOneDay = 60 * 60 * 24 * 1;
        public const double SecondsSixHours = 60 * 60 * 6;
        public const double SecondsTwoHours = 60 * 60 * 2;
        public const double SecondsTwentyMinutes = 60 * 20;
        public const double SecondsTenMinutes = 60 * 10;

        public static double GetViewportSeconds(string s) {
            switch (s.ToLower().Trim()) {
                case "10 minutes":
                    return SecondsTenMinutes;
                case "20 minutes":
                    return SecondsTwentyMinutes;
                case "2 hours":
                    return SecondsTwoHours;
                case "6 hours":
                    return SecondsSixHours;
                case "1 day":
                    return SecondsOneDay;
                case "7 days":
                    return SecondsSevenDays;
                default:
                    return SecondsTwoHours;
            }
        }

        bool showTwentyFiveVolumeLabel = true;
        bool showFiveVolumeLabel = true;
        bool showOneVolumeLabel = true;

        public EventsViewer() {
            InitializeZoomCommandBindings();
            InitializeComponent();
        }

        public void Initialize() {

            InitializeRulers();
            
            InitializeEventsCanvas();

            LinedEventsCanvas.EventsViewer = this;

            InitializeViewport();

            InitializeZoomMenu();

            InitializeZoomInputBindings();

            this.KeyDown += new KeyEventHandler(eventsViewer_KeyDown);
            this.MouseDown += new MouseButtonEventHandler(OnMouseDown);

            ScrollViewer.PreviewKeyDown += new KeyEventHandler(scrollViewer_PreviewKeyDown);

            this.ContextMenuOpening += new ContextMenuEventHandler(eventsViewer_ContextMenuOpening);
            this.ContextMenuClosing += new ContextMenuEventHandler(eventsViewer_ContextMenuClosing);

        }

        void InitializeRulers() {
            HorizontalRuler.Events = Events;
            HorizontalRuler.Initialize();

            VerticalRuler.Events = Events;
            VerticalRuler.Initialize();
        }
        
        public void OnMouseDown(object sender, MouseEventArgs args) {
            ScrollViewer.Focus();
        }

        bool isZoomEnabled = true;
        public bool IsZoomEnabled { 
            get {
                return isZoomEnabled;
            } 
            set {
                isZoomEnabled = value;
                ContextMenu.Visibility = isZoomEnabled ? Visibility.Visible : Visibility.Collapsed;
            } 
        }

        public Point? MousePosition { get; set; }


        void BuildViewportSeconds() {
            if (LinedEventsCanvas.MaximumSecondsInTrace == SecondsSevenDays
                || LinedEventsCanvas.MaximumSecondsInTrace == SecondsOneDay
                || LinedEventsCanvas.MaximumSecondsInTrace == SecondsSixHours
                || LinedEventsCanvas.MaximumSecondsInTrace == SecondsTwoHours
                || LinedEventsCanvas.MaximumSecondsInTrace == SecondsTwentyMinutes
                || LinedEventsCanvas.MaximumSecondsInTrace == SecondsTenMinutes
                ) {
                viewportSeconds = new double[] { SecondsSevenDays, SecondsOneDay, SecondsSixHours, SecondsTwoHours, SecondsTwentyMinutes, SecondsTenMinutes };
            } else if (SecondsSevenDays < LinedEventsCanvas.MaximumSecondsInTrace) {
                viewportSeconds = new double[] { LinedEventsCanvas.MaximumSecondsInTrace, SecondsSevenDays, SecondsOneDay, SecondsSixHours, SecondsTwoHours, SecondsTwentyMinutes, SecondsTenMinutes };
            } else if (SecondsOneDay < LinedEventsCanvas.MaximumSecondsInTrace && LinedEventsCanvas.MaximumSecondsInTrace < SecondsSevenDays) {
                viewportSeconds = new double[] { SecondsSevenDays, LinedEventsCanvas.MaximumSecondsInTrace, SecondsOneDay, SecondsSixHours, SecondsTwoHours, SecondsTwentyMinutes, SecondsTenMinutes };
            } else if (SecondsSixHours < LinedEventsCanvas.MaximumSecondsInTrace && LinedEventsCanvas.MaximumSecondsInTrace < SecondsOneDay) {
                viewportSeconds = new double[] { SecondsSevenDays, SecondsOneDay, LinedEventsCanvas.MaximumSecondsInTrace, SecondsSixHours, SecondsTwoHours, SecondsTwentyMinutes, SecondsTenMinutes };
            } else if (SecondsTwoHours < LinedEventsCanvas.MaximumSecondsInTrace && LinedEventsCanvas.MaximumSecondsInTrace < SecondsSixHours) {
                viewportSeconds = new double[] { SecondsSevenDays, SecondsOneDay, SecondsSixHours, LinedEventsCanvas.MaximumSecondsInTrace, SecondsTwoHours, SecondsTwentyMinutes, SecondsTenMinutes };
            } else if (SecondsTwentyMinutes < LinedEventsCanvas.MaximumSecondsInTrace && LinedEventsCanvas.MaximumSecondsInTrace < SecondsTwoHours) {
                viewportSeconds = new double[] { SecondsSevenDays, SecondsOneDay, SecondsSixHours, SecondsTwoHours, LinedEventsCanvas.MaximumSecondsInTrace, SecondsTwentyMinutes, SecondsTenMinutes };
            } else if (SecondsTenMinutes < LinedEventsCanvas.MaximumSecondsInTrace && LinedEventsCanvas.MaximumSecondsInTrace < SecondsTwentyMinutes) {
                viewportSeconds = new double[] { SecondsSevenDays, SecondsOneDay, SecondsSixHours, SecondsTwoHours, SecondsTwentyMinutes, LinedEventsCanvas.MaximumSecondsInTrace, SecondsTenMinutes };
            } else {
                viewportSeconds = new double[] { VolumeTwenty, VolumeTen, VolumeFive, VolumeTwo, VolumeOne };
            }
        }

        int GetCurrentViewportSecondsIndex() {
            int index = 0;
            for (int i = 0; i < viewportSeconds.Length; i++) {
                if (ViewportSeconds == viewportSeconds[i])
                    index = i;
            }
            return index;
        }

        int GetCurrentViewportVolumeIndex() {
            int index = 0;
            for (int i = 0; i < viewportVolumes.Length; i++) {
                if (ViewportVolume == viewportVolumes[i])
                    index = i;
            }
            return index;
        }

        void DecreaseViewportSeconds() {
            if (!CanDecreaseViewportSeconds())
                return;
            ViewportSeconds = viewportSeconds[GetCurrentViewportSecondsIndex() +1];
        }

        public bool CanDecreaseViewportSeconds() {
            return GetCurrentViewportSecondsIndex() == viewportSeconds.Length - 1 ? false : true;
        }

        void IncreaseViewportSeconds() {
            if (!CanIncreaseViewportSeconds()) 
                return;
            ViewportSeconds = viewportSeconds[GetCurrentViewportSecondsIndex() - 1];
        }

        public bool CanIncreaseViewportSeconds() {
            return GetCurrentViewportSecondsIndex() == 0 ? false : true;
        }

        void BuildViewportVolumes() {
            if (LinedEventsCanvas.MaximumVolumeInTrace == VolumeTwenty
                || LinedEventsCanvas.MaximumVolumeInTrace == VolumeTen
                || LinedEventsCanvas.MaximumVolumeInTrace == VolumeFive
                || LinedEventsCanvas.MaximumVolumeInTrace == VolumeTwo
                || LinedEventsCanvas.MaximumVolumeInTrace == VolumeOne
                ) {
                viewportVolumes = new double[] { VolumeTwenty, VolumeTen, VolumeFive, VolumeTwo, VolumeOne };
            } else if (VolumeTwenty < LinedEventsCanvas.MaximumVolumeInTrace) {
                viewportVolumes = new double[] { LinedEventsCanvas.MaximumVolumeInTrace, VolumeTwenty, VolumeTen, VolumeFive, VolumeTwo, VolumeOne };
            } else if (VolumeTen < LinedEventsCanvas.MaximumVolumeInTrace && LinedEventsCanvas.MaximumVolumeInTrace < VolumeTwenty) {
                viewportVolumes = new double[] { VolumeTwenty, LinedEventsCanvas.MaximumVolumeInTrace, VolumeTen, VolumeFive, VolumeTwo, VolumeOne };
            } else if (VolumeFive < LinedEventsCanvas.MaximumVolumeInTrace && LinedEventsCanvas.MaximumVolumeInTrace < VolumeTen) {
                viewportVolumes = new double[] { VolumeTwenty, VolumeTen, LinedEventsCanvas.MaximumVolumeInTrace, VolumeFive, VolumeTwo, VolumeOne };
            } else if (VolumeTwo < LinedEventsCanvas.MaximumVolumeInTrace && LinedEventsCanvas.MaximumVolumeInTrace < VolumeFive) {
                viewportVolumes = new double[] { VolumeTwenty, VolumeTen, VolumeFive, LinedEventsCanvas.MaximumVolumeInTrace, VolumeTwo, VolumeOne };
            } else {
                viewportVolumes = new double[] { VolumeTwenty, VolumeTen, VolumeFive, VolumeTwo, VolumeOne };
            }
        }

        void DecreaseViewportVolume() {
            if (!CanDecreaseViewportVolume()) 
                return;
            ViewportVolume = viewportVolumes[GetCurrentViewportVolumeIndex() + 1];
        }            

        public bool CanDecreaseViewportVolume() {
            return GetCurrentViewportVolumeIndex() == viewportVolumes.Length - 1 ? false : true;
        }

        void IncreaseViewportVolume() {
            if (!CanIncreaseViewportVolume()) 
                return;
            ViewportVolume = viewportVolumes[GetCurrentViewportVolumeIndex() - 1];
        }            

        public bool CanIncreaseViewportVolume() {
            return GetCurrentViewportVolumeIndex() == 0 ? false : true;
        }

        void scrollViewer_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.PageDown && !ControlKeyDown()) {
                ScrollViewer.PageRight();
                e.Handled = true;
            } else if (e.Key == Key.PageUp && !ControlKeyDown()) {
                ScrollViewer.PageLeft();
                e.Handled = true;
            } else if (e.Key == Key.Home && ControlKeyDown()) {
                ScrollViewer.ScrollToHome();
                e.Handled = true;
            } else if (e.Key == Key.End && ControlKeyDown()) {
                ScrollViewer.ScrollToEnd();
                e.Handled = true;
            } else if (e.Key == Key.Right) {
                if (ControlKeyDown() && ShiftKeyDown()) {
                    FindNextSimilarBySelectedInView();
                    e.Handled = true;
                } else if (ShiftKeyDown()) {
                    FindNextAnyBySelectedInView();
                    e.Handled = true;
                } else if (ControlKeyDown()) {
                    FindNextBySelectedInView();
                    e.Handled = true;
                }
            } else if (e.Key == Key.Left) {
                if (ControlKeyDown() && ShiftKeyDown()) {
                    FindPreviousSimilarBySelectedInView();
                    e.Handled = true;
                } else if (ShiftKeyDown()) {
                    FindPreviousAnyBySelectedInView();
                    e.Handled = true;
                } else if (ControlKeyDown()) {
                    FindPreviousBySelectedInView();
                    e.Handled = true;
                }
            }
        }

        public void FindPreviousAnyBySelectedInView() {
            Event @event = null;
            if (SingleSelectedInView())
                @event = Events.SelectedEvents[0];
            else
                @event = GetPreviousFromPosition();
            FindPreviousAny(@event);
        }

        public void FindNextAnyBySelectedInView() {
            Event @event = null;
            if (SingleSelectedInView())
                @event = Events.SelectedEvents[0];
            else
                @event = GetNextFromPosition();
            FindNextAny(@event);
        }

        public void FindPreviousBySelectedInView() {
            if (Events.SelectedEvents.Count == 1) {
                Event @event = Events.SelectedEvents[0];
                var fixtureClass = @event.FixtureClass;
                if (!IsStartTimeInView(@event))
                    @event = GetPreviousFromPosition();
                FindPrevious(@event, fixtureClass);
            }
        }

        public void FindNextBySelectedInView() {
            if (Events.SelectedEvents.Count == 1) {
                Event @event = Events.SelectedEvents[0];
                var fixtureClass = @event.FixtureClass;
                if (!IsStartTimeInView(@event))
                    @event = GetNextFromPosition();
                FindNext(@event, fixtureClass);
            }
        }

        public void FindPreviousSimilarBySelectedInView() {
            if (SingleSelectedInView())
                FindPreviousSimilar(Events.SelectedEvents[0]);
        }

        public void FindNextSimilarBySelectedInView() {
            if (SingleSelectedInView())
                FindNextSimilar(Events.SelectedEvents[0]);
        }

        void UpdateZoomLevelFeedback() {
            foreach (MenuItem menuItem in MenuZoomHorizontally.Items)
                menuItem.IsChecked = (ViewportSeconds == (double)(menuItem.Tag));
            foreach (MenuItem menuItem in MenuZoomVertically.Items)
                menuItem.IsChecked = (ViewportVolume == (double)(menuItem.Tag));
        }

        void eventsViewer_ContextMenuOpening(object sender, ContextMenuEventArgs e) {
            MousePosition = Mouse.GetPosition(ScrollViewer);
            UpdateZoomLevelFeedback();
        }

        void eventsViewer_ContextMenuClosing(object sender, ContextMenuEventArgs e) {
            MousePosition = null;
        }

        void InitializeViewport() {
            maxViewportSeconds = Math.Max(LinedEventsCanvas.MaximumSecondsInTrace, SecondsSevenDays);
            BuildViewportSeconds();

            maxViewportVolume = Math.Max(LinedEventsCanvas.MaximumVolumeInTrace, VolumeTwenty);
            BuildViewportVolumes();
        }
        
        double GetWidthMultiplier() { return ViewportWidth / ViewportSeconds; }
        double GetHeightMultiplier() { return ViewportHeight / ViewportVolume; }

        public void InitializeZoomMenu() {
            
            InitializeHorizontalMenuItem(MenuZoomHorizontallyTenMinutes, SecondsTenMinutes);
            InitializeHorizontalMenuItem(MenuZoomHorizontallyTwentyMinutes, SecondsTwentyMinutes);
            InitializeHorizontalMenuItem(MenuZoomHorizontallyTwoHours, SecondsTwoHours);
            InitializeHorizontalMenuItem(MenuZoomHorizontallySixHours, SecondsSixHours);
            InitializeHorizontalMenuItem(MenuZoomHorizontallyOneDay, SecondsOneDay);
            InitializeHorizontalMenuItem(MenuZoomHorizontallySevenDays, SecondsSevenDays);
            InitializeHorizontalMenuItem(MenuZoomHorizontallyAllTheWayOut, LinedEventsCanvas.MaximumSecondsInTrace);

            InitializeMenuItem(MenuZoomInHorizontally, IncreaseZoomCommand);
            InitializeMenuItem(MenuZoomOutHorizontally, DecreaseZoomCommand);

            InitializeVerticalMenuItem(MenuZoomVerticallyOneVolumeUnit, VolumeOne);
            InitializeVerticalMenuItem(MenuZoomVerticallyTwoVolumeUnits, VolumeTwo);
            InitializeVerticalMenuItem(MenuZoomVerticallyFiveVolumeUnits, VolumeFive);
            InitializeVerticalMenuItem(MenuZoomVerticallyTenVolumeUnits, VolumeTen);
            InitializeVerticalMenuItem(MenuZoomVerticallyTwentyVolumeUnits, VolumeTwenty);
            InitializeVerticalMenuItem(MenuZoomVerticallyAllTheWayOut, LinedEventsCanvas.MaximumVolumeInTrace);

            InitializeMenuItem(MenuZoomInVertically, IncreaseVerticalZoomCommand);
            InitializeMenuItem(MenuZoomOutVertically, DecreaseVerticalZoomCommand);
      }

        static MenuItem InitializeHorizontalZoomMenuItem(string title, double seconds, RoutedEventHandler handler) {
            return InitializeZoomMenuItem(title, seconds, handler);
        }

        static MenuItem InitializeVerticalZoomMenuItem(string title, double volume, RoutedEventHandler handler) {
            return InitializeZoomMenuItem(title, volume, handler);
        }

        static MenuItem InitializeZoomMenuItem(string title, double value, RoutedEventHandler handler) {
            MenuItem menuItem = new MenuItem();

            menuItem.Tag = value;
            menuItem.Click += new RoutedEventHandler(handler);

            menuItem.Header = title;
            return menuItem;
        }

        void HorizontalZoom_Click(object sender, RoutedEventArgs e) {
            double seconds = (double) ((Control)sender).Tag;
            HorizontalZoomExecuted(GetMouseClickPosition().X, seconds);
            MousePosition = null;
        }

        void VerticalZoom_Click(object sender, RoutedEventArgs e) {
            double volume = (double)((Control)sender).Tag;
            VerticalZoomExecuted(GetMouseClickPosition().X, volume);
            MousePosition = null;
        }

        public void InitializeHorizontalMenuItem(MenuItem menuItem, double value) {
            InitializeMenuItem(menuItem, value, HorizontalZoom_Click);
        }

        public void InitializeMenuItem(MenuItem menuItem, RoutedUICommand command) {
            menuItem.Command = command;
            menuItem.CommandTarget = LinedEventsCanvas.EventsViewer;
        }

        public void InitializeVerticalMenuItem(MenuItem menuItem, double value) {
            InitializeMenuItem(menuItem, value, VerticalZoom_Click);
        }

        public void InitializeMenuItem(MenuItem menuItem, double value, RoutedEventHandler handler) {
            menuItem.CommandTarget = LinedEventsCanvas.EventsViewer;
            menuItem.Click += handler;
            menuItem.Tag = value;
        }

        public static double Between(double min, double max, double value) {
            if (min > value) return min;
            if (value > max) return max;
            return value;
        }

        public Point GetMouseClickPosition() {
            if (MousePosition.HasValue)
                return MousePosition.Value;
            else
                return Mouse.GetPosition(ScrollViewer);
        }

        public void HorizontalZoomExecuted(double focalOffsetViewport, double seconds) {
            if (zoomInProgress) return;
            zoomInProgress = true;

            double percentElapsed = (ScrollViewer.HorizontalOffset + focalOffsetViewport) / LinedEventsCanvas.Width;

            ViewportSeconds = seconds;

            Render();

            ScrollViewer.ScrollToHorizontalOffset(Between(0, LinedEventsCanvas.Width - ViewportWidth, (LinedEventsCanvas.Width * percentElapsed) - focalOffsetViewport));

            zoomInProgress = false;
        }

        public void IncreaseZoomExecuted(double focalOffsetViewport) {
            if (zoomInProgress) return;
            zoomInProgress = true;

            double percentElapsed = (ScrollViewer.HorizontalOffset + focalOffsetViewport) / LinedEventsCanvas.Width;

            DecreaseViewportSeconds();

            Render();

            ScrollViewer.ScrollToHorizontalOffset(Between(0, LinedEventsCanvas.Width - ViewportWidth, (LinedEventsCanvas.Width * percentElapsed) - focalOffsetViewport));

            zoomInProgress = false;
        }

        public void DecreaseZoomExecuted(double focalOffsetViewport) {
            if (zoomInProgress) return;
            zoomInProgress = true;

            double percentElapsed = (ScrollViewer.HorizontalOffset + focalOffsetViewport) / LinedEventsCanvas.Width;

            IncreaseViewportSeconds();

            Render();

            ScrollViewer.ScrollToHorizontalOffset(Between(0, LinedEventsCanvas.Width - ViewportWidth, (LinedEventsCanvas.Width * percentElapsed) - focalOffsetViewport));

            zoomInProgress = false;
        }

        public void VerticalZoomExecuted(double focalOffsetViewport, double volume) {
            if (zoomInProgress) return;
            zoomInProgress = true;

            double percentElapsed = (ScrollViewer.HorizontalOffset + focalOffsetViewport) / LinedEventsCanvas.Width;

            ViewportVolume = volume;

            Render();

            ScrollViewer.ScrollToHorizontalOffset(Between(0, LinedEventsCanvas.Width - ViewportWidth, (LinedEventsCanvas.Width * percentElapsed) - focalOffsetViewport));

            zoomInProgress = false;
        }

        public void IncreaseVerticalZoomExecuted(double focalOffsetViewport) {
            if (zoomInProgress) return;
            zoomInProgress = true;

            double percentElapsed = (ScrollViewer.HorizontalOffset + focalOffsetViewport) / LinedEventsCanvas.Width;
            DecreaseViewportVolume();

            Render();

            ScrollViewer.ScrollToHorizontalOffset(Between(0, LinedEventsCanvas.Width - ViewportWidth, (LinedEventsCanvas.Width * percentElapsed) - focalOffsetViewport));

            zoomInProgress = false;
        }

        public void DecreaseVerticalZoomExecuted(double focalOffsetViewport) {
            if (zoomInProgress) return;
            zoomInProgress = true;

            double percentElapsed = (ScrollViewer.HorizontalOffset + focalOffsetViewport) / LinedEventsCanvas.Width;
            IncreaseViewportVolume();

            Render();

            ScrollViewer.ScrollToHorizontalOffset(Between(0, LinedEventsCanvas.Width - ViewportWidth, (LinedEventsCanvas.Width * percentElapsed) - focalOffsetViewport));

            zoomInProgress = false;
        }

        public void CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
            e.Handled = true;
        }

        public void FindDateTime(DateTime dateTime) {
            if (dateTime > LinedEventsCanvas.Events.StartTime && dateTime < LinedEventsCanvas.Events.EndTime) {
                double percentElapsed = (double)((dateTime.Ticks - LinedEventsCanvas.Events.StartTime.Ticks)) / ((double)(LinedEventsCanvas.Events.EndTime.Ticks - LinedEventsCanvas.Events.StartTime.Ticks));
                ScrollViewer.ScrollToHorizontalOffset(Between(0, LinedEventsCanvas.Width - ViewportWidth, (LinedEventsCanvas.Width * percentElapsed) - (0 * ViewportWidth)));
            }
        }

        public void Select(TwCommand command) {
            var events = LinedEventsCanvas.Events.Select(command);

            LinedEventsCanvas.Events.ClearSelectedEvents(null);
            LinedEventsCanvas.Events.Select(events);
            if (events.Count > 0) {
                LinedEventsCanvas.SelectedPolygon = (Polygon)@events[0].Tag;
                BringIntoView(LinedEventsCanvas.SelectedPolygon);
            }
        }

        public void FindNextSameFixture(Event eventSource) {
            FindNext(eventSource, eventSource.FixtureClass);
        }

        Event NextEventNotFullyInView(Event eventSource) {
            Event @event = eventSource;
            while (@event != null && IsFullyInView(@event))
                @event = @event.Next;
            return @event;
        }
        Event PreviousEventNotFullyInView(Event eventSource) {
            Event @event = eventSource;
            while (@event != null && IsFullyInView(@event))
                @event = @event.Previous;
            return @event;
        }

        Event GetEventSourcePrevious(Event @event) {
            if (@event != null && @event.Selected)
                return @event;
            else
                return null;
//                return LinedEventsCanvas.Events.GetLast();
        }

        Event GetEventSourceNext(Event @event) {
            if (@event != null && @event.Selected)
                return @event;
            else
                return null;
//                return LinedEventsCanvas.Events.GetFirst();
        }

        public Event FindPrevious(TwCommand command, Event @event) {
            Event eventTarget = GetEventSourcePrevious(@event);
            var cycle = Properties.Settings.Default.CycleCommandActions;
            while ((eventTarget = LinedEventsCanvas.Events.FindPrevious(eventTarget, command, cycle))
                != null 
                && IsFullyInView(eventTarget)
                ) ;

            if (eventTarget != null)
                BringEventIntoView(eventTarget);
            else {
                eventTarget = FindLast(command);
            }

            return eventTarget;
        }

        [Obsolete]
        public bool CanFindPrevious(TwCommand command, Event @event) {
            Event eventTarget = GetEventSourcePrevious(@event);
            var cycle = Properties.Settings.Default.CycleCommandActions;
            while ((eventTarget = LinedEventsCanvas.Events.FindPrevious(eventTarget, command, cycle))
                != null
                && IsFullyInView(eventTarget)
                ) ;

            return eventTarget != null;
        }

        [Obsolete]
        public bool CanFindNext(TwCommand command, Event @event) {
            Event eventTarget = GetEventSourceNext(@event);
            var cycle = Properties.Settings.Default.CycleCommandActions;
            while ((eventTarget = LinedEventsCanvas.Events.FindNext(eventTarget, command, cycle))
                != null
                && IsFullyInView(eventTarget)
                ) ;

            return eventTarget != null;
        }

        public Event FindNext(TwCommand command, Event @event) {
            Event eventTarget = GetEventSourceNext(@event);
            var cycle = Properties.Settings.Default.CycleCommandActions;
            while ((eventTarget = LinedEventsCanvas.Events.FindNext(eventTarget, command, cycle))
                != null 
                && IsFullyInView(eventTarget)
                );

            if (eventTarget != null)
                BringEventIntoView(eventTarget);
            else {
                eventTarget = FindFirst(command);
            }

            return eventTarget;
        }

        public Event GetNextFromPosition() {
            var @event = LinedEventsCanvas.Events.FindNext(UndoPosition);
            //if (@event == LinedEventsCanvas.Events[0])
            //    return null;
            if (@event == LinedEventsCanvas.Events[0])
                return null;
            if (@event != null && @event.Previous != null)
                @event = @event.Previous;
            return @event;
        }

        public Event GetPreviousFromPosition() {
            var @event = LinedEventsCanvas.Events.FindPrevious(UndoPosition, PercentCanvasWidth());
            if (@event == LinedEventsCanvas.Events[LinedEventsCanvas.Events.Count - 1])
                return null;
            if (@event != null && @event.Next != null)
                @event = @event.Next;
            return @event;
        }

        public void FindPreviousAndSelect(Event eventSource, TwCommand command) {
            var cycle = Properties.Settings.Default.CycleCommandActions;
            Event eventTarget = LinedEventsCanvas.Events.FindPrevious(eventSource, command, cycle);
            Find(eventTarget);
        }

        public void FindNextAndSelect(Event eventSource, TwCommand command) {
            var cycle = Properties.Settings.Default.CycleCommandActions;
            Event eventTarget = LinedEventsCanvas.Events.FindNext(eventSource, command, cycle);
            Find(eventTarget);
        }

        public void FindNext(FixtureClass fixtureClass) {
            Event eventSource = LinedEventsCanvas.Events.SelectedEvents[0];
            Event eventTarget = LinedEventsCanvas.Events.FindNext(eventSource, fixtureClass);
            Find(eventTarget);
        }

        public void FindNext(Event eventSource, FixtureClass fixtureClass) {
            Event eventTarget = LinedEventsCanvas.Events.FindNext(eventSource, fixtureClass);
            Find(eventTarget);
        }

        public void FindPreviousSameFixture() {
            if (NotExactlyOneSelectedEvent())
                return;

            Event eventSource = (Event)LinedEventsCanvas.SelectedPolygon.Tag;
            FindPreviousSameFixture(eventSource);
        }

        public void FindPreviousSameFixture(Event eventSource) {
            FindPrevious(eventSource, eventSource.FixtureClass);
        }

        public void FindPrevious(FixtureClass fixtureClass) {
            if (NotExactlyOneSelectedEvent())
                return;

            Event eventSource = (Event)LinedEventsCanvas.SelectedPolygon.Tag;
            Event eventTarget = LinedEventsCanvas.Events.FindPrevious(eventSource, fixtureClass);
            Find(eventTarget);
        }
        public void FindPrevious(Event eventSource, FixtureClass fixtureClass) {
            Event eventTarget = LinedEventsCanvas.Events.FindPrevious(eventSource, fixtureClass);
            Find(eventTarget);
        }

        public void FindNextAny() {
            if (NotExactlyOneSelectedEvent())
                return;

            Event eventSource = (Event)LinedEventsCanvas.SelectedPolygon.Tag;
            FindNextAny(eventSource);
        }

        public Event FindFirst(TwCommand command) {
            Event eventTarget = LinedEventsCanvas.Events.FindFirst(command);
            if (eventTarget != null)
                BringEventIntoView(eventTarget);
            return eventTarget;
        }

        public Event FindLast(TwCommand command) {
            Event eventTarget = LinedEventsCanvas.Events.FindLast(command);
            if (eventTarget != null)
                BringEventIntoView(eventTarget);
            return eventTarget;
        }

        public void FindLast() {
            Event eventTarget = LinedEventsCanvas.Events.GetLast();
            Find(eventTarget);
        }

        public void FindFirst() {
            Event eventTarget = LinedEventsCanvas.Events.GetFirst();
            Find(eventTarget);
        }
        
        public void FindNextAny(Event eventSource) {
            Event eventTarget = LinedEventsCanvas.Events.GetNext(eventSource);
            Find(eventTarget);
        }

        void Find(Event @event) {

            if (@event == null)
                return;

            LinedEventsCanvas.Events.ClearSelectedEvents(null);
            LinedEventsCanvas.Events.ToggleSelected(@event);
            LinedEventsCanvas.SelectedPolygon = (Polygon)@event.Tag;
            BringIntoView(LinedEventsCanvas.SelectedPolygon);
        }
        
        public void FindPreviousAny(Event eventSource) {
            Event eventTarget = LinedEventsCanvas.Events.GetPrevious(eventSource);

            if (eventTarget != null)
                Find(eventTarget);
        }

        public double PercentCanvasWidth() {
            return ScrollViewer.ViewportWidth / LinedEventsCanvas.Width;
        }
        
        public void FindFirstSameFixture() {
            if (NotExactlyOneSelectedEvent())
                return;

            Event eventSource = (Event)LinedEventsCanvas.SelectedPolygon.Tag;
            FindFirst(eventSource.FixtureClass);
        }

        public void FindLastSameFixture() {
            if (NotExactlyOneSelectedEvent())
                return;

            Event eventSource = (Event)LinedEventsCanvas.SelectedPolygon.Tag;
            FindLast(eventSource.FixtureClass);
        }

        public void FindLast(FixtureClass fixtureClass) {
            Event eventTarget = LinedEventsCanvas.Events.FindLast(fixtureClass);

            Find(eventTarget);
        }


        public void FindFirst(FixtureClass fixtureClass) {
            Event eventTarget = LinedEventsCanvas.Events.FindFirst(fixtureClass);

            Find(eventTarget);
        }

        public void FindNextSimilar() {
            if (NotExactlyOneSelectedEvent())
                return;

            Event eventSource = (Event)LinedEventsCanvas.SelectedPolygon.Tag;
            FindNextSimilar(eventSource);
        }

        public void FindNextSimilar(Event eventSource) {
            Event eventTarget = LinedEventsCanvas.Events.FindNextSimilar(eventSource);

            Find(eventTarget);
        }

        public void FindPreviousSimilar() {
            if (NotExactlyOneSelectedEvent())
                return;

            Event eventSource = (Event)LinedEventsCanvas.SelectedPolygon.Tag;
            FindPreviousSimilar(eventSource);
        }

        public void FindPreviousSimilar(Event eventSource) {
            Event eventTarget = LinedEventsCanvas.Events.FindPreviousSimilar(eventSource);
            Find(eventTarget);
        }

        public void FindNextNote() {
            if (NotExactlyOneSelectedEvent())
                return;

            Event eventSource = (Event)LinedEventsCanvas.SelectedPolygon.Tag;
            FindNextNote(eventSource);
        }

        public void FindNextNote(Event eventSource) {
            Event eventTarget = LinedEventsCanvas.Events.FindNextNote(eventSource);

            Find(eventTarget);
        }

        public void FindPreviousNote() {
            if (NotExactlyOneSelectedEvent())
                return;

            Event eventSource = (Event)LinedEventsCanvas.SelectedPolygon.Tag;
            FindPreviousNote(eventSource);
        }

        public void FindPreviousNote(Event eventSource) {
            Event eventTarget = LinedEventsCanvas.Events.FindPreviousNote(eventSource);
            Find(eventTarget);
        }

        public bool SingleSelectedInView() {
            return (Events.SelectedEvents.Count == 1 && IsStartTimeInView(Events.SelectedEvents[0]));
        }
        
        public bool IsStartTimeInView(Event @event) {
            return IsTimeInView(@event.StartTime);
        }

        public bool IsEndTimeInView(Event @event) {
            return IsTimeInView(@event.EndTime);
        }

        bool IsTimeInView(DateTime dateTime) {
            double ticksInEventsViewer = (double)(LinedEventsCanvas.Events.EndTime.Ticks - LinedEventsCanvas.Events.StartTime.Ticks);
            double percentEventStart = (double)((dateTime.Ticks - LinedEventsCanvas.Events.StartTime.Ticks) / ticksInEventsViewer);

            double percentScrollViewerStart = (ScrollViewer.HorizontalOffset / LinedEventsCanvas.Width);
            double percentScrollViewerEnd = ((ScrollViewer.HorizontalOffset + ViewportWidth) / LinedEventsCanvas.Width);

            return (percentEventStart >= percentScrollViewerStart && percentEventStart < percentScrollViewerEnd);
        }

        bool IsFullyInView(Event @event) {
            return (IsStartTimeInView(@event) && IsEndTimeInView(@event));
        }

        public void BringEventIntoView(Event @event) {
            double percentElapsed = (double)((@event.StartTime.Ticks - LinedEventsCanvas.Events.StartTime.Ticks)) / ((double)(LinedEventsCanvas.Events.EndTime.Ticks - LinedEventsCanvas.Events.StartTime.Ticks));
            ScrollViewer.ScrollToHorizontalOffset(Between(0, LinedEventsCanvas.Width - ViewportWidth, (LinedEventsCanvas.Width * percentElapsed) - ViewportWidth / 2));
        }

        void BringIntoView(Polygon polygon) {
            Event @event = polygon.Tag as Event;
            BringEventIntoView(@event);
        }
        
        void FindPreviousCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
            e.Handled = true;
        }

        public void ExecuteUserNotes(Event @event) {
            UserNotes userNotes = new UserNotes();
            userNotes.textBox.Focus();
            userNotes.textBox.Text = @event.UserNotes;
            userNotes.textBox.CaretIndex = @event.UserNotes == null? 0: @event.UserNotes.Length;

            userNotes.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            userNotes.Owner = Application.Current.MainWindow;
            userNotes.ShowDialog();

            if (userNotes.DialogResult == true) {
                @event.UserNotes = userNotes.textBox.Text.Replace(',', ' ').Trim();
                LinedEventsCanvas.Events.OnPropertyChanged(TwNotificationProperty.OnUserNotesChanged);
            }
        }

        void ExecuteUserNotes() {
            if (LinedEventsCanvas.Events.SelectedEvents.Count == 1) {
                Event @event = LinedEventsCanvas.Events.SelectedEvents[0];
                ExecuteUserNotes(@event);
            }
        }

        bool AdoptionRequested() {
            return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)
                || ManualClassificationMode == AnalysisPanel.TwManualClassificationMode.WithAdoption;
        }

        void eventsViewer_KeyDown(object sender, KeyEventArgs e) {
            char character = char.MinValue;
            
            string key = e.Key.ToString();

            if (key == "D1") {
                LinedEventsCanvas.Events.ManuallyToggleFirstCycle(LinedEventsCanvas.Events.SelectedEvents, false, null, UndoPosition);
                return;
            } else if (key == "D2") {
                LinedEventsCanvas.OnPropertyChanged(TwNotificationProperty.OnAddFixtureRequested);
                return;
            } else if (key == "D3") {
                LinedEventsCanvas.OnPropertyChanged(TwNotificationProperty.OnApplyFixtureRequested);
                return;
            } else 
                char.TryParse(key, out character);

            if (character == char.MinValue)
                return;

            if (char.ToLower(character) == 'z' && !(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) {
                ExecuteUserNotes();
                return;
            }

            if (char.ToLower(character) == 'q' 
                && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
            {
                LinedEventsCanvas.Events.ToggleManuallyClassified(LinedEventsCanvas.Events.SelectedEvents);
                return;
            }

            FixtureClass fixtureClass = FixtureClasses.GetByCharacter(character);
            if (fixtureClass == null)
                return;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                return;

            if (AdoptionRequested()) 
                LinedEventsCanvas.Events.ManuallyClassify(LinedEventsCanvas.Events.SelectedEvents, fixtureClass, true, UndoPosition);
            else
                LinedEventsCanvas.Events.ManuallyClassify(LinedEventsCanvas.Events.SelectedEvents, fixtureClass, false, UndoPosition);
        }

        bool NotExactlyOneSelectedEvent() {
            return ((LinedEventsCanvas.SelectedPolygon == null ) || (LinedEventsCanvas.Events.SelectedEvents.Count !=1 ));
        }

        public static readonly DependencyProperty ViewportWidthProperty =
          DependencyProperty.Register("ViewportWidth", typeof(double),
          typeof(EventsViewer), new UIPropertyMetadata(0.0));

        public double ViewportWidth {
            get { return (double)GetValue(ViewportWidthProperty); }
            set { SetValue(ViewportWidthProperty, value); }
        }

        protected void InitializeEventsCanvas() {

            LinedEventsCanvas.Events = Events;
            LinedEventsCanvas.Initialize();

//            LinedEventsCanvas.Render();

            InitializeScrollViewer();

            Properties.Settings.Default.PropertyChanged += new PropertyChangedEventHandler(Default_PropertyChanged);
        }

        void Default_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "ShowVerticalGuideline":
                case "GraphVerticalGuidelineColor":
                    LinedEventsCanvas.RenderVerticalGuideline();
                    break;
                case "ShowHorizontalGuideline":
                case "GraphHorizontalGuidelineColor":
                    LinedEventsCanvas.RenderRows(showTwentyFiveVolumeLabel,showFiveVolumeLabel,showOneVolumeLabel);
                    break;
            }
        }

        public DateTime StartTime {
            get {
                double percentElapsed = ScrollViewer.HorizontalOffset / LinedEventsCanvas.Width;
                return LinedEventsCanvas.Events.StartTime.Add(new TimeSpan((long)(percentElapsed * LinedEventsCanvas.Events.Duration.Ticks)));
            }
        }

        public DateTime EndTime {
            get {
                double percentElapsed = (ScrollViewer.HorizontalOffset + ScrollViewer.ViewportWidth) / LinedEventsCanvas.Width;
                return LinedEventsCanvas.Events.StartTime.Add(new TimeSpan((long)(percentElapsed * LinedEventsCanvas.Events.Duration.Ticks)));
            }
        }

        public TimeSpan Duration {
            get {
                return EndTime.Subtract(StartTime);
            }
        }

        void InitializeScrollViewer() {
            ScrollViewer.ScrollChanged += new ScrollChangedEventHandler(ScrollViewer_ScrollChanged);
        }

        bool SizeHasChanged() {
            return ViewportHeight != ScrollViewer.ViewportHeight || ViewportWidth != ScrollViewer.ViewportWidth;
        }

        public void SaveScrollPosition() {
            UndoPosition.ViewportSeconds = ViewportSeconds;
            UndoPosition.ViewportVolume = ViewportVolume;
            UndoPosition.PercentElapsedHorizontal = LinedEventsCanvas.Width == 0 ? 0.0 : ScrollViewer.HorizontalOffset / LinedEventsCanvas.Width;
            UndoPosition.PercentElapsedVertical = LinedEventsCanvas.Height == 0 ? 0.0 : ScrollViewer.VerticalOffset / LinedEventsCanvas.Height;
            LinedEventsCanvas.UndoPosition = UndoPosition;
        }

        public void RestoreHorizontalScrollPosition(UndoPosition undoPosition) {
            if (ViewportSeconds != undoPosition.ViewportSeconds || ViewportVolume != undoPosition.ViewportVolume) {
                ViewportSeconds = undoPosition.ViewportSeconds;
                Render();
            }
            if (!double.IsNaN(undoPosition.PercentElapsedHorizontal))
                ScrollViewer.ScrollToHorizontalOffset(Between(0, LinedEventsCanvas.Width - ViewportWidth, LinedEventsCanvas.Width * undoPosition.PercentElapsedHorizontal));
        }

        public void RestoreScrollPosition(UndoPosition undoPosition) {
            if (ViewportSeconds != undoPosition.ViewportSeconds || ViewportVolume != undoPosition.ViewportVolume) {
                ViewportSeconds = undoPosition.ViewportSeconds;
                ViewportVolume = undoPosition.ViewportVolume;
                Render();
            }
            if (!double.IsNaN(undoPosition.PercentElapsedHorizontal))
                ScrollViewer.ScrollToHorizontalOffset(Between(0, LinedEventsCanvas.Width - ViewportWidth, LinedEventsCanvas.Width * undoPosition.PercentElapsedHorizontal));
            if (!double.IsNaN(undoPosition.PercentElapsedVertical))
                ScrollViewer.ScrollToVerticalOffset(Between(0, LinedEventsCanvas.Height - ViewportHeight, LinedEventsCanvas.Height * undoPosition.PercentElapsedVertical));
        }

        void SaveViewportDimensions() {
            ViewportWidth = ScrollViewer.ViewportWidth;
            ViewportHeight = ScrollViewer.ViewportHeight;
        }

        void WorkaroundForScrollViewerProblem() {
            // Note: It doesn't work to call ScrollToHorizontalOffset here in ScrollChanged event (or in LayoutUpdated event). If you try it, 
            // it just sets the offset to zero. As a result, the scroll position is wrong. So we go with the lesser of two
            // evils when changing ScrollViewer size: reset scrollbar to initial position.

            // RestoreScrollPosition();

            ScrollViewer.ScrollToLeftEnd();
            ScrollViewer.ScrollToEnd();
        }

        bool firstScroll = true;
        public void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            ScrollChanged(false);
        }

        public void ScrollChanged(bool forceRedraw) {
            SaveScrollPosition();

            if (forceRedraw || SizeHasChanged()) {
                SaveViewportDimensions();
                Render();
                WorkaroundForScrollViewerProblem();

                if (firstScroll) {
                    firstScroll = false;
                    if (Properties.Settings.Default.SelectFirstEvent)
                        FindFirst();
                }
            } else {
                LinedEventsCanvas.RenderVerticalGuideline();
            }
        }

        protected Boolean ControlKeyDown() { return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl); }
        protected Boolean AltKeyDown() { return Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt); }
        protected Boolean ShiftKeyDown() { return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift); }

        void Render(double viewportSeconds, double viewportVolume) {
            LinedEventsCanvas.Render();
            RenderRowsColumnsRulers(viewportSeconds, viewportVolume);
        }
        
        public void RenderRowsColumnsRulers(double viewportSeconds, double viewportVolume) {
            bool showOneDayTicks = false;
            bool showOneHourTicks = false;
            bool showTenMinutesTicks = false;
            bool showOneMinuteTicks = false;

            bool showOneDayLabel = false;
            bool showOneHourLabel = false;
            bool showTenMinutesLabel = false;
            bool showOneMinuteLabel = false;

            bool showTwentyFiveVolumeRows = true;
            bool showFiveVolumeRows = true;
            bool showOneVolumeRows = true;

            bool showTwentyFiveVolumeTicks = true;
            bool showFiveVolumeTicks = true;
            bool showOneVolumeTicks = true;

            if (ViewportSeconds <= EventsViewer.SecondsTwentyMinutes) {
                showTenMinutesTicks = true;
                showTenMinutesLabel = true;
            } else if (ViewportSeconds <= EventsViewer.SecondsTwoHours) {
                showTenMinutesTicks = true;
                showOneHourTicks = true;
                showOneHourLabel = true;
            } else if (ViewportSeconds <= EventsViewer.SecondsSixHours) {
                showOneHourTicks = true;
                showOneHourLabel = true;
            } else if (ViewportSeconds <= EventsViewer.SecondsOneDay) {
                showOneHourTicks = true;
                showOneHourLabel = true;
            } else if (ViewportSeconds <= EventsViewer.SecondsSevenDays) {
                showOneDayTicks = true;
                showOneDayLabel = true;
            } else {
                showOneDayTicks = true;
            }

            if (ViewportVolume > 20) {
                showOneVolumeRows = false;
                showOneVolumeLabel = false;
            }
            if (ViewportVolume > 50) {
                showOneVolumeTicks = false;
                showFiveVolumeRows = false;
                showFiveVolumeLabel = false;
            }
            if (ViewportVolume > 100) {
                showFiveVolumeTicks = false;
            }

            int blankRows = Math.Max((int)ViewportVolume - LinedEventsCanvas.MaximumVolumeInTrace, 0);
            LinedEventsCanvas.RenderRowsColumns(
                showOneDayTicks, showOneDayLabel,
                showOneHourTicks, showOneHourLabel,
                showTenMinutesTicks, showTenMinutesLabel,
                showOneMinuteTicks, showOneMinuteLabel,
                showTwentyFiveVolumeRows, showTwentyFiveVolumeTicks, showTwentyFiveVolumeLabel,
                showFiveVolumeRows, showFiveVolumeTicks, showFiveVolumeLabel,
                showOneVolumeRows, showOneVolumeTicks, showOneVolumeLabel, blankRows);

            HorizontalRuler.Clear();
            HorizontalRuler.RenderColumns(GetWidthMultiplier(), showOneDayTicks, showOneDayLabel, showOneHourTicks, showOneHourLabel,
                showTenMinutesTicks, showTenMinutesLabel, showOneMinuteTicks, showOneMinuteLabel);

            VerticalRuler.Clear();
            VerticalRuler.RenderRows(GetHeightMultiplier(), showTwentyFiveVolumeTicks, showTwentyFiveVolumeLabel, showFiveVolumeTicks, showFiveVolumeLabel, showOneVolumeTicks, showOneVolumeLabel, blankRows);

            SelectionRuler.Clear();
            SelectionRuler.Render(GetWidthMultiplier());

            ApprovalRuler.Clear();
            ApprovalRuler.Render(GetWidthMultiplier());

            ClassificationRuler.Clear();
            ClassificationRuler.Render(GetWidthMultiplier());

            FixturesRuler.Clear();
            FixturesRuler.Render(GetWidthMultiplier(), viewportSeconds);
        }

        void Render() {

            Mouse.OverrideCursor = Cursors.Wait;

            if (ViewportWidth == 0 || ViewportHeight == 0)
                return;

            LinedEventsCanvas.WidthMultiplier = GetWidthMultiplier();
            LinedEventsCanvas.HeightMultiplier = GetHeightMultiplier();

            Render(ViewportSeconds, ViewportVolume);

//            double scrollToHorizontal = LinedEventsCanvas.Width * percentElapsedHorizontal;
//            double scrollToVertical = LinedEventsCanvas.Height * percentElapsedVertical;

//            ScrollViewer.ScrollToHorizontalOffset(Between(0, LinedEventsCanvas.Width - ViewportWidth, scrollToHorizontal));
////            ScrollViewer.ScrollToVerticalOffset(Between(0, LinedEventsCanvas.Height - ViewportHeight, scrollToVertical));
//            ScrollViewer.ScrollToEnd();

            Mouse.OverrideCursor = null;

            ScrollViewer.Focus();
        }

        public static RoutedUICommand IncreaseZoomCommand;
        public static RoutedUICommand DecreaseZoomCommand;
        public static RoutedUICommand IncreaseVerticalZoomCommand;
        public static RoutedUICommand DecreaseVerticalZoomCommand;

        public static RoutedUICommand ClearSelectedEventsCommand;

        void DecreaseVerticalZoomCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = !zoomInProgress && IsZoomEnabled && CanIncreaseViewportVolume();
            e.Handled = true;
        }

        void DecreaseVerticalZoomCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            DecreaseVerticalZoomExecuted(GetMouseClickPosition().X);
            MousePosition = null;
        }

        void IncreaseVerticalZoomCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = !zoomInProgress && IsZoomEnabled && CanDecreaseViewportVolume();
            e.Handled = true;
        }

        void IncreaseVerticalZoomCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            IncreaseVerticalZoomExecuted(GetMouseClickPosition().X);
            MousePosition = null;
        }

        void DecreaseZoomCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = !zoomInProgress && IsZoomEnabled && CanIncreaseViewportSeconds();
            e.Handled = true;
        }

        void DecreaseZoomCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            DecreaseZoomExecuted(GetMouseClickPosition().X);
            MousePosition = null;
        }

        void IncreaseZoomCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            IncreaseZoomExecuted(GetMouseClickPosition().X);
            MousePosition = null;
        }

        void IncreaseZoomCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = !zoomInProgress && IsZoomEnabled && CanDecreaseViewportSeconds();
            e.Handled = true;
        }

        RoutedUICommand InitializeCommand(string commandText, string commandName, ExecutedRoutedEventHandler handlerExecute, CanExecuteRoutedEventHandler handlerCanExecute) {
            var command = new RoutedUICommand(commandText, commandName, typeof(EventsViewer));
            CommandBinding commandBinding = new CommandBinding(command, handlerExecute, handlerCanExecute);
            CommandBindings.Add(commandBinding);
            return command;
        }

        RoutedUICommand InitializeCommand(string commandText, string commandName, ExecutedRoutedEventHandler handlerExecute) {
            return InitializeCommand(commandText, commandName, handlerExecute, CanExecute);
        }

        void InitializeZoomCommandBindings() {
            IncreaseZoomCommand = InitializeCommand("Horizontal Zoom In", "IncreaseHorizontalZoom", IncreaseZoomCommandExecuted, IncreaseZoomCanExecute);
            DecreaseZoomCommand = InitializeCommand("Horizontal Zoom Out", "DecreaseHorizontalZoomCommand", DecreaseZoomCommandExecuted, DecreaseZoomCanExecute);
            IncreaseVerticalZoomCommand = InitializeCommand("Vertical Zoom In", "IncreaseVerticalZoomCommand", IncreaseVerticalZoomCommandExecuted, IncreaseVerticalZoomCanExecute);
            DecreaseVerticalZoomCommand = InitializeCommand("Vertical Zoom Out", "DecreaseVerticalZoomCommand", DecreaseVerticalZoomCommandExecuted, DecreaseVerticalZoomCanExecute);

            ClearSelectedEventsCommand = InitializeCommand("Clear Selected Events", "ClearSelectedEvents", ClearSelectedEventsCommandExecuted, CanExecute);
        }

        void ClearSelectedEventsCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            LinedEventsCanvas.OnPropertyChanged(TwNotificationProperty.OnLeaveHorizontalSplitMode);
            //LinedEventsCanvas.OnPropertyChanged(TwNotificationProperty.OnLeaveVerticalSplitMode);
            //LinedEventsCanvas.OnPropertyChanged(TwNotificationProperty.OnLeaveMergeAllIntoBaseMode);

            LinedEventsCanvas.Events.ClearSelectedEvents(null);
            LinedEventsCanvas.SelectedPolygon = null;
            ScrollViewer.Focus();
        }

        void InitializeZoomInputBindings() {

            LinedEventsCanvas.InputBindings.Add(
              new MouseBinding(IncreaseZoomCommand, new MouseGesture(MouseAction.LeftDoubleClick, ModifierKeys.None)));

            LinedEventsCanvas.InputBindings.Add(
              new MouseBinding(DecreaseZoomCommand, new MouseGesture(MouseAction.LeftDoubleClick, ModifierKeys.Shift)));

            LinedEventsCanvas.InputBindings.Add(
              new MouseBinding(DecreaseZoomCommand, new MouseGesture(MouseAction.MiddleDoubleClick, ModifierKeys.None)));

            LinedEventsCanvas.InputBindings.Add(
              new MouseBinding(IncreaseVerticalZoomCommand, new MouseGesture(MouseAction.LeftDoubleClick, ModifierKeys.Control)));

            LinedEventsCanvas.InputBindings.Add(
              new MouseBinding(DecreaseVerticalZoomCommand, new MouseGesture(MouseAction.LeftDoubleClick, ModifierKeys.Shift | ModifierKeys.Control)));

            LinedEventsCanvas.InputBindings.Add(
              new MouseBinding(ClearSelectedEventsCommand, new MouseGesture(MouseAction.LeftClick, ModifierKeys.None)));
        }
    }
}
