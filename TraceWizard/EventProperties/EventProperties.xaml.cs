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
using TraceWizard.TwHelper;

namespace TraceWizard.TwApp {
    public partial class EventProperties : UserControl {

        public Event @event;
        public bool PerformUpdate;
        public Point mousePosition;
        public double widthMultiplier;
        public double heightMultiplier;

        public EventProperties() {
            InitializeComponent();
        }

        public void Initialize() {

            int row = 0;

//            var panel = new StyledFixtureLabel(@event.FixtureClass, FontWeights.Bold, false, @event.ManuallyClassified, @event.FirstCycle, @event.FirstCycleManuallyClassified, true, false, false);
            var panel = new StyledFixtureLabel(@event, true, false);
            panel.Margin = new Thickness(3, 3, 3, 6);

            Grid.SetRow(panel, row++);
            Grid.SetColumn(panel, 0);
            Grid.SetColumnSpan(panel, 2);
            grid.Children.Add(panel);

            WriteEventProperty(row++, "Vol", Math.Round(@event.Volume, 2).ToString("0.00"));
            WriteEventProperty(row++, "Peak", @event.Peak.ToString("0.00"));
            WriteEventProperty(row++, "Mode", @event.Mode.ToString("0.00"));
            WriteEventProperty(row++, "Dur", (new TwHelper.DurationConverter()).Convert(@event.Duration, null, null, null).ToString());

            WriteHorizontalSeparator(row++,8);

            if (Properties.Settings.Default.ShowSimilarCountInEventToolTips)
                ShowSimilarCounts(@event, ref row, PerformUpdate);

            WriteEventPropertySmall(row++, "Start", @event.StartTime.DayOfWeek.ToString());
            WriteEventPropertySmall(row++, string.Empty, @event.StartTime.ToShortDateString().ToString());
            WriteEventPropertySmall(row++, string.Empty, @event.StartTime.ToLongTimeString().ToString());

            if (@event.EndTime.DayOfYear != @event.StartTime.DayOfYear) {
                WriteEventPropertySmall(row++, "End", @event.EndTime.DayOfWeek.ToString());
                WriteEventPropertySmall(row++, string.Empty, @event.EndTime.ToShortDateString().ToString());
                WriteEventPropertySmall(row++, string.Empty, @event.EndTime.ToLongTimeString().ToString());
            } else {
                WriteEventPropertySmall(row++, "End", @event.EndTime.ToLongTimeString().ToString());
            }

            if (!string.IsNullOrEmpty(@event.UserNotes)) {
                WriteHorizontalSeparator(row++);
                WriteEventPropertySmall(row++, "Notes", @event.UserNotes, FontWeights.Bold, 200);
            }

            if (Properties.Settings.Default.ShowDetailedEventToolTips)
                ShowDetailProperties(@event, ref row, mousePosition, widthMultiplier, heightMultiplier);

            if (Properties.Settings.Default.ShowDiagnosticEventToolTips)
                ShowDiagnosticProperties(@event, ref row);
       }


        void ShowSimilarCounts(Event @event, ref int row, bool performUpdate) {
            if (performUpdate)
                @event.UpdateSimilarCounts();
            WriteEventPropertySmall(row++, "Sim Forward", @event.SimilarForwardCount.ToString());
            WriteEventPropertySmall(row++, "Sim Back", @event.SimilarBackwardCount.ToString());
            WriteHorizontalSeparator(row++);
        }

        void WriteEventPropertySmall(int row, string label, string value) {
            WriteEventPropertySmall(row, label, value, FontWeights.Normal, null);
        }

        void WriteEventPropertySmall(int row, string label, string value, FontWeight fontWeight, double? maxWidth) {
            WriteEventProperty(row, label, value, 10, fontWeight, maxWidth);
        }

        void WriteEventProperty(int row, string label, string value) {
            WriteEventProperty(row, label, value, (new TextBlock()).FontSize);
        }

        void WriteEventProperty(int row, string label, string value, double fontSize) {
            WriteEventProperty(row, label, value, fontSize, FontWeights.Normal, null);
        }

        void WriteEventProperty(int row, string label, string value, double fontSize, FontWeight fontWeight, double? maxWidth) {
            TextBlock txt;

            txt = new TextBlock();

            if (maxWidth.HasValue && !string.IsNullOrEmpty(value)) {
                txt.MaxWidth = maxWidth.Value;
            }

            txt.Padding = new Thickness(0, 0, 6, 0);
            if (!string.IsNullOrEmpty(label))
                txt.Text = label + ":";
            txt.FontSize = fontSize;
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 0);
            grid.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(0, 0, 0, 0);
            txt.Text = value;
            txt.FontSize = fontSize;
            txt.FontWeight = fontWeight;
            Grid.SetRow(txt, row++);
            Grid.SetColumn(txt, 1);
            grid.Children.Add(txt);

        }

        void WriteHorizontalSeparator(int row) {
            WriteHorizontalSeparator(row, 0);
        }

        void WriteHorizontalSeparator(int row, double margin) {
            Border border = new Border();
            border.Style = (Style)ResourceLocator.FindResource("HorizontalSeparatorStyle");
            border.BorderBrush = Brushes.LightGray;
            border.Margin = new Thickness(0,margin,0,0);
            Grid.SetRow(border, row++);
            Grid.SetColumn(border, 0);
            Grid.SetColumnSpan(border, grid.ColumnDefinitions.Count);
            grid.Children.Add(border);
        }

        void ShowDiagnosticProperties(Event @event, ref int row) {
            for (int i = 0; i < 13; i++) grid.RowDefinitions.Add(new RowDefinition());

            WriteHorizontalSeparator(row++);

            WriteEventPropertySmall(row++, "Channel", @event.Channel.ToString());

            if (@event.Channel == Channel.Super) {
                WriteEventPropertySmall(row++, "Base Start", @event.BaseEvent.StartTime.ToLongTimeString());
                WriteEventPropertySmall(row++, "Base End", @event.BaseEvent.EndTime.ToLongTimeString());
                WriteEventPropertySmall(row++, "Base Fixture", @event.BaseEvent.FixtureClass.FriendlyName);
            } else if (@event.Channel == Channel.Base) {
                WriteEventPropertySmall(row++, "Supers", @event.SuperEvents == null ? 0.ToString() : @event.SuperEvents.Count.ToString());
            }
        }

        void ShowDetailProperties(Event @event, ref int row, Point mousePosition, double widthMultiplier, double heightMultiplier) {
            for (int i = 0; i < 9; i++) grid.RowDefinitions.Add(new RowDefinition());

            WriteHorizontalSeparator(row++);

//            WriteEventPropertySmall(row++, "X factor", widthMultiplier.ToString("0.000000"));
//            WriteEventPropertySmall(row++, "Mouse X", mousePosition.X.ToString("0.0000"));

            double secondsOffset = (double)(mousePosition.X / widthMultiplier);
            double remainder = secondsOffset - (int)Math.Floor(secondsOffset);
            DateTime startTime = @event.StartTime.Add(new TimeSpan(0, 0, 0, (int)Math.Floor(secondsOffset), (int)(remainder * 1000.0)));

            DateTime dateTime = @event.GetSplitDate(mousePosition.X, widthMultiplier);
            double rate = @event.GetSplitRate(mousePosition.Y, heightMultiplier);
            Flow thisFlow = @event.GetFlow(dateTime, rate);

//            WriteEventPropertySmall(row++, "Mouse Time", dateTime.ToLongTimeString().ToString() + "." + dateTime.Millisecond.ToString("000"));
//            WriteEventPropertySmall(row++, "Split Time", startTime.ToLongTimeString().ToString() + "." + startTime.Millisecond.ToString("000"));
//            WriteEventPropertySmall(row++, "Mouse Y", (-1.0 * mousePosition.Y).ToString("0.00"));
            WriteEventPropertySmall(row++, "Mouse Rate", rate.ToString("0.00"));
            WriteEventPropertySmall(row++, "Split Time", startTime.ToLongTimeString().ToString());
            WriteEventPropertySmall(row++, "Secs offset", secondsOffset.ToString("0.00"));


            WriteHorizontalSeparator(row++);

            if (thisFlow != null) {
//                WriteEventPropertySmall(row++, "Flow Start", thisFlow.StartTime.ToLongTimeString().ToString() + "." + thisFlow.StartTime.Millisecond.ToString("000"));
                WriteEventPropertySmall(row++, "Flow Start", thisFlow.StartTime.ToLongTimeString().ToString());
                WriteEventPropertySmall(row++, "Flow Rate", thisFlow.Rate.ToString("0.00"));
            } else {
                WriteEventPropertySmall(row++, "Flow Start", "none");
                WriteEventPropertySmall(row++, "Flow Rate", "none");
            }
        }
    }
}
