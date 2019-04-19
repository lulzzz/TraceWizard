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
    public partial class EventPolygonMenu : ContextMenu {

        public EventPolygon EventPolygon;
        public AnalysisPanel.TwManualClassificationMode ManualClassificationMode = AnalysisPanel.TwManualClassificationMode.None;

        EventsCanvas EventsCanvas;

        Event Event;
        Polygon Polygon;

        public EventPolygonMenu() {
            InitializeComponent();
        }

        public void Initialize() {
            EventsCanvas = EventPolygon.EventsCanvas;
            Event = EventPolygon.Event;
            Polygon = EventPolygon.Polygon;

            InitializeMenu();
            UpdateMenu();
        }

        public void UpdateMenu() {
            UpdateFirstCycleMenu();
            UpdateFixturesMenu();
            UpdateFindMenu();
            UpdateMergeMenu();
            UpdateSplitMenu();
            UpdateUserNotesMenu();
        }
        
        void InitializeMenu() {

            InitializeFirstCycleMenu();

            InitializeFixturesMenu();

            InitializeAddFixtureMenu();

            InitializeFindMenu();

            InitializeMergeMenu();
            InitializeSplitMenu();

            InitializeUserNotesMenu();
        }

        void InitializeFirstCycleMenu() {
            InitializeMenuItem(MenuFirstCycle, contextMenuFirstCycle_Click);
        }

        void UpdateFirstCycleMenu() {
            MenuFirstCycle.IsEnabled = ((Event)(Polygon.Tag)).FixtureClass.CanHaveCycles ? true : false;
            if (((Event)(Polygon.Tag)).FirstCycle)
                MenuFirstCycle.IsChecked = true;
        }

        void InitializeFixturesMenu() {
            int index = MenuContext.Items.IndexOf(SeparatorFixtures) + 1;
            foreach (FixtureClass fixtureClass in FixtureClasses.ItemsDescending.Values) {
                MenuItem menuItemFixture = CreateFixtureMenuItem(fixtureClass);
                if (fixtureClass.LowFrequency)
                    MenuMoreFixtures.Items.Add(menuItemFixture);
                else
                    MenuContext.Items.Insert(index++, menuItemFixture);
            }
        }

        MenuItem CreateFixtureMenuItem(FixtureClass fixtureClass) {
            var menuItem = CreateFixtureMenuItemLow(fixtureClass);
            menuItem.IsChecked = (Event.FixtureClass == fixtureClass);
            menuItem.Click += new RoutedEventHandler(contextMenuFixture_Click);
            return menuItem;
        }

        void InitializeMenuItem(MenuItem menuItem, RoutedEventHandler handler) {
            menuItem.Tag = Polygon;
            menuItem.Click += handler;
        }

        void InitializeAddFixtureMenu() {
            InitializeMenuItem(MenuAddFixture, contextMenuAddFixture_Click);
            InitializeMenuItem(MenuApplyFixture, contextMenuApplyFixture_Click);
        }

        void InitializeFindMenu() {
            InitializeMenuItem(MenuFindNext,contextMenuFindNext_Click);
            InitializeMenuItem(MenuFindPrevious, contextMenuFindPrevious_Click);
            InitializeMenuItem(MenuFindNextAny, contextMenuFindNextAny_Click);
            InitializeMenuItem(MenuFindPreviousAny, contextMenuFindPreviousAny_Click);

            InitializeFindNextOtherMenu();
            InitializeFindPreviousOtherMenu();

            InitializeMenuItem(MenuFindNextSimilar, contextMenuFindNextSimilar_Click);
            InitializeMenuItem(MenuFindPreviousSimilar, contextMenuFindPreviousSimilar_Click);

            InitializeMenuItem(MenuFindNextUserNote, contextMenuFindNextUserNote_Click);
            InitializeMenuItem(MenuFindPreviousUserNote, contextMenuFindPreviousUserNote_Click);
        }

        void InitializeFindNextOtherMenu() {
            foreach (FixtureClass fixtureClass in FixtureClasses.ItemsDescending.Values) {
                MenuItem menuItemFixture = CreateFixtureMenuFindNextOtherItem(fixtureClass);
                if (fixtureClass.LowFrequency)
                    MenuFindNextOtherMore.Items.Add(menuItemFixture);
                else
                    MenuFindNextOther.Items.Add(menuItemFixture);
            }
        }

        MenuItem CreateFixtureMenuFindNextOtherItem(FixtureClass fixtureClass) {
            var menuItem = CreateFixtureMenuItemLow(fixtureClass);
            menuItem.Click += new RoutedEventHandler(contextMenuFindNextOther_Click);
            return menuItem;
        }

        MenuItem CreateFixtureMenuItemLow(FixtureClass fixtureClass) {
            var menuItem = new MenuItem();
            menuItem.Header = TwGui.FixtureWithImageLeftMenu(fixtureClass);
            menuItem.Tag = Polygon;
            menuItem.InputGestureText = fixtureClass.Character.ToString();
            return menuItem;
        }

        void InitializeFindPreviousOtherMenu() {
            foreach (FixtureClass fixtureClass in FixtureClasses.ItemsDescending.Values) {
                MenuItem menuItemFixture = CreateFixtureMenuFindPreviousOtherItem(fixtureClass);
                if (fixtureClass.LowFrequency)
                    MenuFindPreviousOtherMore.Items.Add(menuItemFixture);
                else
                    MenuFindPreviousOther.Items.Add(menuItemFixture);
            }
        }

        MenuItem CreateFixtureMenuFindPreviousOtherItem(FixtureClass fixtureClass) {
            var menuItem = CreateFixtureMenuItemLow(fixtureClass);
            menuItem.Click += new RoutedEventHandler(contextMenuFindPreviousOther_Click);
            return menuItem;
        }

        void InitializeMergeMenu() {

            MenuMergeNextEventIntoThisEvent.Click += new RoutedEventHandler(contextMenuMergeNext_Click);
            MenuMergeNextEventIntoThisEvent.Tag = Polygon;

            MenuMergePreviousEventIntoThisEvent.Click += new RoutedEventHandler(contextMenuMergePrevious_Click);
            MenuMergePreviousEventIntoThisEvent.Tag = Polygon;

            MenuMergeAllPreviousAndNextEventsIntoThisEvent.Click += new RoutedEventHandler(contextMenuMergeAllContiguous_Click);
            MenuMergeAllPreviousAndNextEventsIntoThisEvent.Tag = Polygon;

            MenuMergeThisEventIntoBaseEvent.Click += new RoutedEventHandler(contextMenuMergeIntoBase_Click);
            MenuMergeThisEventIntoBaseEvent.Tag = Polygon;

            MenuMergeAllSuperEvents.Click += new RoutedEventHandler(contextMenuMergeAllIntoBase_Click);
            if (Events.CanMergeVerticallyIntoThisBase(((Event)Polygon.Tag).BaseEvent)) {
                MenuMergeAllSuperEvents.Header = "All Super Events Into Base Event";
                MenuMergeAllSuperEvents.Tag = (Polygon)(Event.BaseEvent.Tag);
            } else {
                MenuMergeAllSuperEvents.Header = "All Super Events Into This Event";
                MenuMergeAllSuperEvents.Tag = Polygon;
            }
        }

        void UpdateMergeMenu() {
            MenuMergeNextEventIntoThisEvent.IsEnabled = Events.CanMergeNextHorizontally(Event);
            MenuMergePreviousEventIntoThisEvent.IsEnabled = EventsCanvas.Events.CanMergePreviousHorizontally(Event);
            MenuMergeAllPreviousAndNextEventsIntoThisEvent.IsEnabled = EventsCanvas.Events.CanMergeAllHorizontally(Event);
            MenuMergeThisEventIntoBaseEvent.IsEnabled = EventsCanvas.Events.CanMergeVerticallyIntoBase((Event)Polygon.Tag);

            if (Events.CanMergeVerticallyIntoThisBase(((Event)Polygon.Tag).BaseEvent)) {
                MenuMergeAllSuperEvents.IsEnabled = Events.CanMergeVerticallyIntoThisBase(Event.BaseEvent);
            } else {
                MenuMergeAllSuperEvents.IsEnabled = Events.CanMergeVerticallyIntoThisBase(Event);
            }
        }

        void InitializeSplitMenu() {
            MenuSplitIntoContiguousEvents.Click += new RoutedEventHandler(contextMenuSplitHorizontally_Click);
            MenuSplitIntoContiguousEvents.Tag = Polygon;

            MenuSplitIntoSimultaneousEvents.Click += new RoutedEventHandler(contextMenuSplitVertically_Click);
            MenuSplitIntoSimultaneousEvents.Tag = Polygon;
        }

        void UpdateSplitMenu() {
            MenuSplitIntoContiguousEvents.IsEnabled = Events.CanSplitHorizontally(Event);
            MenuSplitIntoSimultaneousEvents.IsEnabled = Events.CanSplitVertically(Event);
        }


        void UpdateFixturesMenu() {
            UpdateFixturesMenu(MenuMoreFixtures.Items);
            UpdateFixturesMenu(MenuContext.Items);
        }

        void UpdateFixturesMenu(ItemCollection items) {
            foreach (var item in items) {
                var menuItem = item as MenuItem;
                if (menuItem != null) {
                    Grid grid = menuItem.Header as Grid;

                    if (grid != null) {
                        FixtureClass fixtureClass = grid.Tag as FixtureClass;
                        if (fixtureClass != null) {
                            var polygon = menuItem.Tag as Polygon;
                            if (polygon != null) {
                                var @event = polygon.Tag as Event;
                                if (@event != null) {
                                    if (@event.FixtureClass == fixtureClass)
                                        menuItem.IsChecked = true;
                                    else
                                        menuItem.IsChecked = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        void UpdateFindMenu() {
            MenuFindNext.Header = "Find Next " + Event.FixtureClass.FriendlyName;
            MenuFindPrevious.Header = "Find Previous " + Event.FixtureClass.FriendlyName;
        }

        void UpdateUserNotesMenu() {
            MenuUserNotes.IsEnabled = EventsCanvas == null ? true : (EventsCanvas.Events.SelectedEvents.Count <= 1);
        }

        void InitializeUserNotesMenu() {
            InitializeMenuItem(MenuUserNotes, contextMenuUserNotes_Click);
        }

        void contextMenuFirstCycle_Click(object sender, RoutedEventArgs e) {
            if (EventsCanvas.Events.SelectedEvents.Contains(Event))
                EventsCanvas.Events.ManuallyToggleFirstCycle(EventsCanvas.Events.SelectedEvents, false, Event.FirstCycle, EventPolygon.EventsCanvas.UndoPosition);
            else
                EventsCanvas.Events.ManuallyToggleFirstCycle(Event, EventPolygon.EventsCanvas.UndoPosition);
        }

        FixtureClass GetFixtureClass(MenuItem menuItem) {
            return ((FrameworkElement)(menuItem.Header)).Tag as FixtureClass;
        }

        bool AdoptionRequested() {
            return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)
                || ManualClassificationMode == AnalysisPanel.TwManualClassificationMode.WithAdoption;
        }
        
        void contextMenuFixture_Click(object sender, RoutedEventArgs e) {
            FixtureClass fixtureClass = GetFixtureClass(sender as MenuItem);
                Classify(fixtureClass, AdoptionRequested());
        }

        void Classify(FixtureClass fixtureClass, bool adopt) {
            if (EventsCanvas.Events.SelectedEvents.Contains(Event))
                EventsCanvas.Events.ManuallyClassify(EventsCanvas.Events.SelectedEvents, fixtureClass, adopt, EventPolygon.EventsCanvas.UndoPosition);
            else
                EventsCanvas.Events.ManuallyClassify(Event, fixtureClass, adopt, EventPolygon.EventsCanvas.UndoPosition);
        }

        void contextMenuAddFixture_Click(object sender, RoutedEventArgs e) {
            EventPolygon.EventsCanvas.OnPropertyChanged(Notification.TwNotificationProperty.OnAddFixtureRequested);
        }

        void contextMenuApplyFixture_Click(object sender, RoutedEventArgs e) {
            EventPolygon.EventsCanvas.OnPropertyChanged(Notification.TwNotificationProperty.OnApplyFixtureRequested);
        }

        void contextMenuFindNext_Click(object sender, RoutedEventArgs e) {
            EventsCanvas.EventsViewer.FindNextSameFixture(Event);
        }

        void contextMenuFindPrevious_Click(object sender, RoutedEventArgs e) {
            EventsCanvas.EventsViewer.FindPreviousSameFixture(Event);
        }

        void contextMenuFindNextAny_Click(object sender, RoutedEventArgs e) {
            EventsCanvas.EventsViewer.FindNextAny(Event);
        }

        void contextMenuFindPreviousAny_Click(object sender, RoutedEventArgs e) {
            EventsCanvas.EventsViewer.FindPreviousAny(Event);
        }

        void contextMenuFindNextSimilar_Click(object sender, RoutedEventArgs e) {
            EventsCanvas.EventsViewer.FindNextSimilar(Event);
        }

        void contextMenuFindPreviousSimilar_Click(object sender, RoutedEventArgs e) {
            EventsCanvas.EventsViewer.FindPreviousSimilar(Event);
        }

        void contextMenuFindNextOther_Click(object sender, RoutedEventArgs e) {
            FixtureClass fixtureClass = GetFixtureClass(sender as MenuItem);
            EventsCanvas.EventsViewer.FindNext(Event, fixtureClass);
        }

        void contextMenuFindPreviousOther_Click(object sender, RoutedEventArgs e) {
            FixtureClass fixtureClass = GetFixtureClass(sender as MenuItem);
            EventsCanvas.EventsViewer.FindPrevious(Event, fixtureClass);
        }

        void contextMenuFindNextUserNote_Click(object sender, RoutedEventArgs e) {
            EventsCanvas.EventsViewer.FindNextNote(Event);
        }

        void contextMenuFindPreviousUserNote_Click(object sender, RoutedEventArgs e) {
            EventsCanvas.EventsViewer.FindPreviousNote(Event);
        }

        void contextMenuSplitVertically_Click(object sender, RoutedEventArgs e) {
            EventPolygon.SplitVertically(Polygon, EventPolygon.GetMouseClickPosition());
        }

        void contextMenuSplitHorizontally_Click(object sender, RoutedEventArgs e) {
            EventPolygon.SplitHorizontally(EventPolygon.GetMouseClickPosition());
        }

        void contextMenuMergePrevious_Click(object sender, RoutedEventArgs e) {
            EventPolygon.MergePreviousHorizontally(Event);
        }

        void contextMenuMergeNext_Click(object sender, RoutedEventArgs e) {
            EventPolygon.MergeNextHorizontally(Event);
        }

        void contextMenuMergeAllContiguous_Click(object sender, RoutedEventArgs e) {
            EventPolygon.MergeAllHorizontally(Event);
        }

        void contextMenuMergeAllIntoBase_Click(object sender, RoutedEventArgs e) {
            EventPolygon.MergeAllVerticallyIntoBase(Event);
        }
        
        void contextMenuMergeIntoBase_Click(object sender, RoutedEventArgs e) {
            EventPolygon.MergeVertically(Event, Event.BaseEvent, true);
        }

        void contextMenuUserNotes_Click(object sender, RoutedEventArgs e) {
            EventsCanvas.EventsViewer.ExecuteUserNotes(Event);
        }
    }
}
