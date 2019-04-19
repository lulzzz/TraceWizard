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
    public partial class HorizontalRuler : UserControl {

        public Events Events;

        public HorizontalRuler() {
            InitializeComponent();
        }

        public void Initialize() {
            this.Background = TwBrushes.BrushFromColor(Properties.Settings.Default.GraphRulerColor);
        }

        public void Clear() {
            Canvas.Children.Clear();
        }

        DateTime GetNextOneMinuteBoundary(DateTime dateTime) {
            if (dateTime.Second == 0)
                return dateTime;
            else
                return dateTime.AddSeconds(-dateTime.Second).AddMinutes(1);
        }

        DateTime GetNextTenMinuteBoundary(DateTime dateTime) {
            if (dateTime.Minute % 10 == 0)
                return dateTime;
            else
                return dateTime.AddMinutes(10 - (dateTime.Minute % 10));
        }

        DateTime GetNextOneHourBoundary(DateTime dateTime) {
            if (dateTime.Minute == 0)
                return dateTime;
            else
                return dateTime.AddMinutes(60 - dateTime.Minute);
        }

        DateTime GetNextOneDayBoundary(DateTime dateTime) {
            if (dateTime.Hour == 0)
                return dateTime;
            else
                return dateTime.AddHours(24 - dateTime.Hour);
        }

        double MaximumSecondsColumns { get { return Events.Duration.TotalSeconds; } }

        public void RenderColumns(double widthMultiplier,
            bool showOneDayTicks, bool showOneDayLabel, bool showOneHourTicks, bool showOneHourLabel,
            bool showTenMinutesTicks, bool showTenMinutesLabel, bool showOneMinuteTicks, bool showOneMinuteLabel) {
            DateTime startTime = Events.StartTime;

            DateTime nextOneMinuteBoundary = GetNextOneMinuteBoundary(startTime);
            DateTime nextTenMinuteBoundary = GetNextTenMinuteBoundary(nextOneMinuteBoundary);
            DateTime nextOneHourBoundary = GetNextOneHourBoundary(nextTenMinuteBoundary);
            DateTime nextOneDayBoundary = GetNextOneDayBoundary(nextOneHourBoundary);

            int secondsToOneMinuteBoundary = (int)nextOneMinuteBoundary.Subtract(startTime).TotalSeconds;
            int secondsToTenMinuteBoundary = (int)nextTenMinuteBoundary.Subtract(startTime).TotalSeconds;
            int secondsToOneHourBoundary = (int)nextOneHourBoundary.Subtract(startTime).TotalSeconds;
            int secondsToOneDayBoundary = (int)nextOneDayBoundary.Subtract(startTime).TotalSeconds;

            int totalSeconds = (int)MaximumSecondsColumns;
            for (int seconds = 0; seconds < totalSeconds; seconds++) {
                if (showOneDayTicks && seconds % (24 * 60 * 10 * 6) == secondsToOneDayBoundary) {
                    RenderColumn(seconds, Brushes.Black, 1.0, widthMultiplier);
                    if (showOneDayLabel)
                        RenderColumnDay(seconds, startTime.Add(new TimeSpan(0, 0, seconds)), widthMultiplier);
                } else if (showOneHourTicks && seconds % (60 * 10 * 6) == secondsToOneHourBoundary) {
                    RenderColumn(seconds, Brushes.Black, 1.0, widthMultiplier);
                    if (showOneHourLabel)
                        RenderColumnHour(seconds, startTime.Add(new TimeSpan(0, 0, seconds)), widthMultiplier);
                } else if (showTenMinutesTicks && seconds % (60 * 10) == secondsToTenMinuteBoundary) {
                    RenderColumn(seconds, Brushes.Black, 1.0, widthMultiplier);
                    if (showTenMinutesLabel)
                        RenderColumnMinutes(seconds, startTime.Add(new TimeSpan(0, 0, seconds)), widthMultiplier);
                } else if (showOneMinuteTicks && seconds % 60 == secondsToOneMinuteBoundary) {
                    ;
                }
            }
        }

        Brush DayOfWeekBrush(DateTime dateTime) {
            return (dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday) ?
                Brushes.Blue : Brushes.Black;
        }

        FontWeight DayOfWeekFontWeight(DateTime dateTime) {
            return (dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday) ?
                FontWeights.Bold : FontWeights.Normal;
        }

        void RenderColumnDay(int seconds, DateTime dateTime, double widthMultiplier) {
            string text = dateTime.ToString("MMM dd");
            RenderColumnTime(seconds, text, DayOfWeekBrush(dateTime), DayOfWeekFontWeight(dateTime), widthMultiplier);
        }

        void RenderColumnHour(int seconds, DateTime dateTime, double widthMultiplier) {
            RenderColumnTime(seconds, dateTime.ToString("hh") + GetAmPm(dateTime), DayOfWeekBrush(dateTime), DayOfWeekFontWeight(dateTime), widthMultiplier);
        }

        void RenderColumnMinutes(int seconds, DateTime dateTime, double widthMultiplier) {
            RenderColumnTime(seconds, dateTime.ToString("hh:mm") + GetAmPm(dateTime), DayOfWeekBrush(dateTime), DayOfWeekFontWeight(dateTime), widthMultiplier);
        }

        string GetAmPm(DateTime dateTime) { return dateTime.Hour < 12 ? "am" : "pm"; }
        
        void RenderColumnTime(int seconds, string text, Brush foreground, FontWeight fontWeight, double widthMultiplier) {

            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.FontFamily = new FontFamily("Courier New");
            textBlock.FontSize = 11;
            textBlock.Foreground = foreground;
            textBlock.FontWeight = fontWeight;

            System.Windows.Controls.Canvas.SetBottom(textBlock, 0);
            System.Windows.Controls.Canvas.SetLeft(textBlock, (seconds * widthMultiplier) - 12);
            this.Canvas.Children.Add(textBlock);
        }

        void RenderColumn(int seconds, Brush brush, double thickness, double widthMultiplier) {
            LineGeometry line = new LineGeometry();
            line.StartPoint = new Point(seconds * widthMultiplier, 0);
            line.EndPoint = new Point(seconds * widthMultiplier, 4);
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            line.Freeze();

            Path path = new Path();
            path.Stroke = brush;
            path.StrokeThickness = thickness;
            path.SnapsToDevicePixels = false;

            path.Data = line;
            System.Windows.Controls.Canvas.SetZIndex(path, -1);
            this.Canvas.Children.Add(path);
        }
    }
}
