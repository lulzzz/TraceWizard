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
    public partial class HourlyTotalsDetail : UserControl {
        public Analysis Analysis;

        public HourlyTotalsDetail() {
            InitializeComponent();
        }

        public void Initialize() {

            var fixtureSummaries = Analysis.FixtureSummaries;
            double[] hoursVolume;
            double totalVolume;
             if (Analysis.Events != null) {
                 hoursVolume = fixtureSummaries.HourlyVolume;
                 totalVolume = Analysis.Events.Volume;
             } else {
                 hoursVolume = Analysis.Log.HoursVolume;
                 totalVolume = Analysis.Log.Volume;
             }

            HorizontalAlignment = HorizontalAlignment.Left;

            for (int i = 0; i < hoursVolume.Length + 1; i++) 
                Grid.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < 2; i++) 
                Grid.ColumnDefinitions.Add(new ColumnDefinition());

            if (fixtureSummaries != null) {
                foreach(FixtureSummary fixtureSummary in fixtureSummaries.Values)
                    if (fixtureSummary.Volume > 0)
                        Grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            BuildRowHeader(0, "Hour", "Volume", fixtureSummaries);

            int hour = 0;
            for (hour = 0; hour < hoursVolume.Length; hour++) {
                BuildRow(hour + 1, hour.ToString() + ":", hoursVolume[hour].ToString("0.0"), fixtureSummaries, hour);
            }

            BuildRowFooter(hoursVolume.Length + 1, "Total", totalVolume.ToString("0.0"), fixtureSummaries);
        }

        void BuildRowBase(int row, ref int column, string label, string value, bool bold) {
            TextBlock txt;

            txt = new TextBlock();
            txt.Padding = new Thickness(0, 0, 10, 0);
            txt.Text = label;
            txt.FontWeight = bold ? FontWeights.Bold : FontWeights.Normal;
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, column++);
            Grid.Children.Add(txt);

            txt = new TextBlock();
            txt.Padding = new Thickness(0, 0, 10, 0);
            txt.Text = value;
            txt.FontWeight = bold ? FontWeights.Bold : FontWeights.Normal;
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, column++);
            Grid.Children.Add(txt);
        }
        
        void BuildRowHeader(int row, string label, string value, FixtureSummaries fixtureSummaries) {
            int column = 0;
            BuildRowBase(row, ref column, label, value, true);

            if (fixtureSummaries != null) {

                IEnumerable<FixtureSummary> sorted = Enumerable.OrderByDescending(Analysis.FixtureSummaries.Values, n => n.Volume);

                foreach (FixtureSummary fixtureSummary in sorted) {
                    if (fixtureSummary.Volume > 0) {
                        var fixture = new ShortFixtureLabel(fixtureSummary.FixtureClass);
                        fixture.HorizontalImageAlignment = HorizontalAlignment.Right;
                        fixture.HorizontalAlignment = HorizontalAlignment.Right;
//                        fixture.Padding = new Thickness(0, 0, 10, 0);
fixture.Padding = new Thickness(0);
fixture.Margin = new Thickness(0, 0, 10, 0);
                        Grid.SetRow(fixture, row);
                        Grid.SetColumn(fixture, column++);
                        Grid.Children.Add(fixture);
                    }
                }
            }
        }

        void BuildRowFooter(int row, string label, string value, FixtureSummaries fixtureSummaries) {
            int column = 0;
            BuildRowBase(row, ref column, label, value, true);
            if (fixtureSummaries != null)
                BuildRowFixtureSummaries(row, ref column, true, fixtureSummaries, 0, true);
        }

        void BuildRowFixtureSummaries(int row, ref int column, bool bold, FixtureSummaries fixtureSummaries, int hour, bool isTotalRow) {
            TextBlock txt;
            double volume = 0.0;

            IEnumerable<FixtureSummary> sorted = Enumerable.OrderByDescending(Analysis.FixtureSummaries.Values, n => n.Volume);

            foreach (FixtureSummary fixtureSummary in sorted) {
                if (fixtureSummary.Volume > 0) {
                    if (isTotalRow)
                        volume = fixtureSummaries[fixtureSummary.FixtureClass].Volume;
                    else 
                        volume = fixtureSummaries[fixtureSummary.FixtureClass].HourlyVolume[hour];

                    txt = new TextBlock();

//                    txt.Padding = new Thickness(0, 0, 10, 0);
                    txt.Padding = new Thickness(0);
                    txt.Margin = new Thickness(0, 0, 10, 0);

                    txt.Text = volume.ToString("0.0");
                    txt.Foreground = (volume == 0) ? Brushes.LightGray : Brushes.Black;
                    txt.FontWeight = bold ? FontWeights.Bold : FontWeights.Normal;
                    txt.HorizontalAlignment = HorizontalAlignment.Right;
                    Grid.SetRow(txt, row);
                    Grid.SetColumn(txt, column++);
                    Grid.Children.Add(txt);
                }
            }
        }

        void BuildRow(int row, string label, string value, FixtureSummaries fixtureSummaries, int hour) {

            int column = 0;
            BuildRowBase(row, ref column, label, value, false);

            if (fixtureSummaries != null)
                BuildRowFixtureSummaries(row, ref column, false, fixtureSummaries, hour, false);
        }
    }
}
