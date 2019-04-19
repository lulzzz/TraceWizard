using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

using TraceWizard.Entities;

using TraceWizard.TwHelper;

namespace TraceWizard.TwApp {

    public class EventsProperties : Grid {

        public EventsProperties(FixtureClass fixtureClassPredicted, FixtureClass fixtureClassActual, FixtureSummary fixtureSummary) {
            CreateEventsProperties(fixtureClassPredicted, "Predicted", fixtureClassActual, "Actual", fixtureSummary.Events, fixtureSummary.Count, false);
        }

        public EventsProperties(FixtureClass fixtureClassPredicted, FixtureClass fixtureClassActual, Events events, int count) {
            CreateEventsProperties(fixtureClassPredicted, "Predicted", fixtureClassActual, "Actual", events, count, false);
        }

        public EventsProperties(FixtureClass fixtureClass, Events events, int count) {
            CreateEventsProperties(fixtureClass, null, null, string.Empty, events, count, false);
        }

        public EventsProperties(FixtureClass fixtureClass, Events events, int count, int cycles) {
            CreateEventsProperties(fixtureClass, null, null, string.Empty, events, count, cycles, false);
        }

        public void CreateEventsProperties(FixtureClass fixtureClassPredicted, string fixtureClassPredictedLabel, FixtureClass fixtureClassActual, string fixtureClassActualLabel, Events events, int count, bool showKey) {
            CreateEventsProperties(fixtureClassPredicted, fixtureClassPredictedLabel, fixtureClassActual, fixtureClassActualLabel, events, count, 0, showKey); 
        }

        public void CreateEventsProperties(FixtureClass fixtureClassPredicted, string fixtureClassPredictedLabel, FixtureClass fixtureClassActual, string fixtureClassActualLabel, Events events, int count, int cycles, bool showKey) {
            for (int i = 0; i < 10; i++) {
                this.RowDefinitions.Add(new RowDefinition());
            }
            for (int i = 0; i < 6; i++) {
                this.ColumnDefinitions.Add(new ColumnDefinition());
            }

            int row = 0;
            TextBlock txt;

            HorizontalAlignment = HorizontalAlignment.Left;
            
            if (fixtureClassPredicted != null) {
                txt = new TextBlock();
                txt.Padding = new Thickness(0, 0, 6, 0);
                if (!string.IsNullOrEmpty(fixtureClassPredictedLabel))
                    txt.Text = fixtureClassPredictedLabel + ": ";
                txt.HorizontalAlignment = HorizontalAlignment.Left;
                txt.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetRow(txt, row);
                Grid.SetColumn(txt, 0);
                this.Children.Add(txt);

                var fixtureImage = TwGui.FixtureWithImageLeft(fixtureClassPredicted, showKey);
                fixtureImage.HorizontalAlignment = HorizontalAlignment.Left;

                Grid.SetRow(fixtureImage, row);
                Grid.SetColumn(fixtureImage, 0);
                Grid.SetColumnSpan(fixtureImage, 2);
                this.Children.Add(fixtureImage);
            }

            txt = new TextBlock();
            txt.Padding = new Thickness(0, 0, 6, 0);
            txt.Text = "Events: " + count.ToString();
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            txt.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 2);
            Grid.SetColumnSpan(txt, 2);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            txt.VerticalAlignment = VerticalAlignment.Top;
            txt.Padding = new Thickness(0, 0, 6, 0);
            txt.Text = "1st Cycles: " + cycles.ToString();
            Grid.SetRow(txt, row++);
            Grid.SetColumn(txt, 4);
            Grid.SetColumnSpan(txt, 2);
            this.Children.Add(txt);
            if (cycles == 0)
                txt.Visibility = Visibility.Hidden;

            WriteHorizontalSeparator(row++);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = "Min";
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            txt.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 1);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = "10%";
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            txt.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 2);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = "50%";
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            txt.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 3);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = "90%";
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            txt.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 4);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = "Max";
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            txt.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(txt, row++);
            Grid.SetColumn(txt, 5);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = "Vol:";
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 0);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = Math.Round(events.MinVolume, 2).ToString("0.00");
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 1);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = Math.Round(events.LowerLimitVolume, 2).ToString("0.00");
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 2);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = Math.Round(events.MedianVolume, 2).ToString("0.00");
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 3);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = Math.Round(events.UpperLimitVolume, 2).ToString("0.00");
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 4);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = Math.Round(events.MaxVolume, 2).ToString("0.00");
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row++);
            Grid.SetColumn(txt, 5);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = "Peak:";
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 0);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = events.MinPeak.ToString("0.00");
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 1);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = events.LowerLimitPeak.ToString("0.00");
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 2);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = events.MedianPeak.ToString("0.00");
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 3);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = events.UpperLimitPeak.ToString("0.00");
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 4);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = events.MaxPeak.ToString("0.00");
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row++);
            Grid.SetColumn(txt, 5);
            this.Children.Add(txt);
            
            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = "Mode:";
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 0);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = events.MinMode.ToString("0.00");
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 1);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = events.LowerLimitMode.ToString("0.00");
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 2);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = events.MedianMode.ToString("0.00");
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 3);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = events.UpperLimitMode.ToString("0.00");
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 4);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = events.MaxMode.ToString("0.00");
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row++);
            Grid.SetColumn(txt, 5);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = "Dur:";
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 0);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = ((string)(new FriendlyDurationConverter()).Convert(events.MinDuration, null, null, null)).Trim();
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 1);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = ((string)(new FriendlyDurationConverter()).Convert(events.LowerLimitDuration, null, null, null)).Trim();
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 2);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = ((string)(new FriendlyDurationConverter()).Convert(events.MedianDuration, null, null, null)).Trim();
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 3);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = ((string)(new FriendlyDurationConverter()).Convert(events.UpperLimitDuration, null, null, null)).Trim();
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 4);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(6, 0, 6, 0);
            txt.Text = ((string)(new FriendlyDurationConverter()).Convert(events.MaxDuration, null, null, null)).Trim();
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row++);
            Grid.SetColumn(txt, 5);
            this.Children.Add(txt);

        }

        void WriteHorizontalSeparator(int row) {
            Border border = new Border();

            border.Style = (Style)ResourceLocator.FindResource("HorizontalSeparatorStyle");
            Grid.SetRow(border, row++);
            Grid.SetColumn(border, 0);
            Grid.SetColumnSpan(border, this.ColumnDefinitions.Count);
            this.Children.Add(border);
        }
    }
}