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
    public partial class DailyTotalsDetail : UserControl {
        public Analysis Analysis;

        public DailyTotalsDetail() {
            InitializeComponent();
        }

        double TotalVolume(Dictionary<DateTime,double> dailyVolume) {
            double totalVolume = 0.0;
            foreach (DateTime dateTime in dailyVolume.Keys)
                totalVolume += dailyVolume[dateTime];
            return totalVolume;
        }
        
        public void Initialize() {

            var fixtureSummaries = Analysis.FixtureSummaries;
            Dictionary<DateTime,double> dailyVolume;
            double totalVolume;
            if (Analysis.Events != null) {
                dailyVolume = fixtureSummaries.DailyVolume;
                totalVolume = TotalVolume(fixtureSummaries.DailyVolume);
            } else {
                dailyVolume = Analysis.Log.DailyVolume;
                totalVolume = TotalVolume(Analysis.Log.DailyVolume);
            }

            HorizontalAlignment = HorizontalAlignment.Left;

            for (int i = 0; i < dailyVolume.Count + 1; i++)
                Grid.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < 2; i++)
                Grid.ColumnDefinitions.Add(new ColumnDefinition());

            if (fixtureSummaries != null) {
                foreach (FixtureSummary fixtureSummary in fixtureSummaries.Values)
                    if (fixtureSummary.Volume > 0)
                        Grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            BuildRowHeader(0, "Full\r\nDay", "Volume", fixtureSummaries);

            int iRow = 0;
            foreach (DateTime dateTime in dailyVolume.Keys) {
                BuildRow(iRow + 1, (iRow+1).ToString() + ":", dailyVolume[dateTime].ToString("0.0"), fixtureSummaries, iRow, dateTime);
                iRow++;
            }

            BuildRowFooter(dailyVolume.Count + 1, "Total", totalVolume.ToString("0.0"), fixtureSummaries);
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
                BuildRowFixtureSummaries(row, ref column, true, fixtureSummaries, 0, DateTime.MinValue,true);
        }

        void BuildRowFixtureSummaries(int row, ref int column, bool bold, FixtureSummaries fixtureSummaries, int day, DateTime date, bool isTotalRow) {
            TextBlock txt;
            double volume = 0.0;

            IEnumerable<FixtureSummary> sorted = Enumerable.OrderByDescending(Analysis.FixtureSummaries.Values, n => n.Volume);

            foreach (FixtureSummary fixtureSummary in sorted) {
                if (fixtureSummary.Volume > 0) {
                    if (isTotalRow)
                        volume = fixtureSummaries[fixtureSummary.FixtureClass].DailyVolumeTotal;
                    else
                        volume = fixtureSummaries[fixtureSummary.FixtureClass].DailyVolume[date];

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

        void BuildRow(int row, string label, string value, FixtureSummaries fixtureSummaries, int day, DateTime date) {

            int column = 0;
            BuildRowBase(row, ref column, label, value, false);

            if (fixtureSummaries != null)
                BuildRowFixtureSummaries(row, ref column, false, fixtureSummaries, day, date, false);
        }
    }
}
