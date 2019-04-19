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
    public partial class EventPanel : UserControl {

        public bool ShowSelectedEvent = true;

        public Analysis Analysis;

        public EventPanel() {
            InitializeComponent();
        }

        public void Initialize() {
            Properties.Settings.Default.PropertyChanged +=new PropertyChangedEventHandler(Default_PropertyChanged);
            Visibility = SetVisibility();

            if (ShowSelectedEvent)
                UpdateSelectedEvent();
        }

        Visibility SetVisibility() {
            if (ShowSelectedEvent)
                return Properties.Settings.Default.ShowSelectedEventPanel ? Visibility.Visible : Visibility.Collapsed;
            else
                return Properties.Settings.Default.ShowCurrentEventPanel ? Visibility.Visible : Visibility.Collapsed;
        }

        public void Default_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            //switch (e.PropertyName) {
                //case "ShowSelectedEventPanel":
                //    if (ShowSelectedEvent)
                //        Visibility = SetVisibility();
                //    break;
                //case "ShowCurrentEventPanel":
                //    if (!ShowSelectedEvent)
                //        Visibility = SetVisibility();
                //    break;
            //}
        }

        public void Events_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case TwNotificationProperty.OnEndMergeSplit:
                case TwNotificationProperty.OnEndClassify:
                case TwNotificationProperty.OnEndSelect:
                case TwNotificationProperty.OnUserNotesChanged:
                case TwNotificationProperty.OnChangeStartTime:
                case TwNotificationProperty.OnEndApplyConversionFactor:
                    UpdateSelectedEvent();
                    break;
            }
        }

        public void EventsCanvas_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case TwNotificationProperty.OnMouseEnterPolygon:
                case TwNotificationProperty.OnMouseLeavePolygon:
                    UpdateCurrentEvent();
                    break;
            }
        }

        void UpdateSelectedEvent() {
            if (Analysis.Events.SelectedEvents.Count == 1) {
                Analysis.Events.SelectedEvents[0].UpdateSimilarCounts();
                UpdateEvent(Analysis.Events.SelectedEvents[0]);
        }
            else
                Clear();
        }

        void UpdateCurrentEvent() {
            if (Analysis.Events.CurrentEvent != null)
                UpdateEvent(Analysis.Events.CurrentEvent);
            else
                Clear();
        }

        void UpdateEvent(Event @event) {

            if (@event == null)
                return;

            var fixtureLabel = new StyledFixtureLabel(@event, true, true);

            labelFixtureClassImage.Content = fixtureLabel;

            labelVolume.Content = Math.Round(@event.Volume, 2).ToString("0.00");
            labelPeak.Content = @event.Peak.ToString("0.00");
            labelMode.Content = @event.Mode.ToString("0.00");
            labelDuration.Content = (new TwHelper.DurationConverter()).Convert(@event.Duration, null, null, null).ToString();
            labelDuration.ToolTip = "Duration of Selected Event: " + (new TwHelper.FriendlierDurationConverter()).Convert(@event.Duration, null, null, null).ToString();

            labelStartTime.Content = @event.StartTime.ToLongTimeString().ToString();
            labelStartDate.Content = @event.StartTime.ToShortDateString().ToString();

            labelEndTime.Content = @event.EndTime.ToLongTimeString().ToString();

            labelEndTime.ToolTip = "End Time of Selected Event (End Date is " + (new TwHelper.ShortDateConverter().Convert(@event.EndTime, null, null, null).ToString()) + ")";

            if (@event.EndTime.DayOfYear != @event.StartTime.DayOfYear)
                labelEndDate.Content = @event.EndTime.ToShortDateString().ToString();
            else
                labelEndDate.Content = string.Empty;

            labelSimilar.Content = @event.SimilarBackwardCount.ToString("0") + @" + " + @event.SimilarForwardCount.ToString("0") + @" = " + @event.SimilarCount.ToString("0");
            labelSimilar.ToolTip = "Number of Previous + Next Events Similar to Selected Event = Total Events Similar to Selected Event";

            labelNotes.Content = @event.UserNotes;
            labelNotes.ToolTip = "User Notes of Selected Event: " + @event.UserNotes;
        }

        void Clear() {
            labelFixtureClassImage.Content = null;
            labelVolume.Content = null;
            labelPeak.Content = null;
            labelMode.Content = null;
            labelDuration.Content = null;
            labelStartTime.Content = null;
            labelStartDate.Content = null;
            labelEndTime.Content = null;
            labelEndDate.Content = null;
            labelSimilar.Content = null;
            labelNotes.Content = null;
        }
    }
}
