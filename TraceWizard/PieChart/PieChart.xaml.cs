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

    public class PieChartByVolume : PieChart {
        public PieChartByVolume() : base() { ByInstances = false; }
    }

    public class PieChartByInstances : PieChart {
        public PieChartByInstances() : base() { ByInstances = true; }
    }

    public partial class PieChart : UserControl {

        public Analysis Analysis;
        public double Radius;
        public bool ByInstances; 
        public bool IsEnlargeable;

        CommandBinding openPieChartCommandBinding;
        public static RoutedUICommand openPieChartCommand;

        public PieChart() {
            InitializeComponent();
        }

        public void Initialize() {
            openPieChartCommand = new RoutedUICommand();
            openPieChartCommandBinding = new CommandBinding(
                openPieChartCommand,
                OpenPieChartCommandExecuted,
                OpenPieChartCanExecute);

            CommandBindings.Add(openPieChartCommandBinding);
            InputBindings.Add(new MouseBinding(openPieChartCommand, new MouseGesture(MouseAction.LeftDoubleClick, ModifierKeys.None)));

            canvas.Height = Radius * 2;
            canvas.Width = Radius * 2;

            Analysis.Events.PropertyChanged += new PropertyChangedEventHandler(PropertyChanged);

            UpdatePieChart(Analysis.FixtureSummaries, Radius);
        }

        void OpenPieChartCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            var window = new FixtureSummaryReportWindow();
            window.ByInstances = ByInstances;
            window.Radius = 200;
            window.Analysis = Analysis;
            window.Initialize();

            window.ShowDialog();
        }

        public void OpenPieChartCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsEnlargeable;
            e.Handled = true;
        }

        void window_KeyDown(object sender, KeyEventArgs e) {
            var window = sender as Window;

            if (e.Key == Key.Escape)
                window.Close();
        }
        
        public void PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case TwNotificationProperty.OnEndMergeSplit:
                case TwNotificationProperty.OnEndClassify:
                    UpdatePieChart(Analysis.FixtureSummaries, Radius);
                    break;
            }
        }

        protected void UpdatePieChart(FixtureSummaries fixtureSummaries, double radius) {
            canvas.Children.Clear();
            double startAngle = 0;
            double endAngle;
            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values) {
                canvas.Children.Add(DrawWedge(radius, startAngle, out endAngle, fixtureSummaries[fixtureClass]));
                startAngle = endAngle;
            }
            return;
        }

        Path DrawWedge(double radius, double startAngle, out double endAngle, FixtureSummary fixtureSummary) {
            Path path = new Path();

            Canvas.SetLeft(path, radius);
            Canvas.SetTop(path, radius);

            path.Fill = TwBrushes.FrozenSolidColorBrush(fixtureSummary.FixtureClass);

            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();

            pathFigure.StartPoint = new Point(0,0);
            pathFigure.IsClosed = true;

            Point startArc = new Point(Math.Cos(startAngle * Math.PI / 180) * radius, Math.Sin(startAngle * Math.PI / 180) * radius);
            LineSegment lineSegment = new LineSegment(startArc, true);

            ArcSegment arcSegment = new ArcSegment();
            double percent = ByInstances ? fixtureSummary.PercentCount : fixtureSummary.PercentVolume;

            double angle;
            if (fixtureSummary.PercentCount == 1) {
                angle = 359.99; // WPF won't draw a wedge from 0 to 360 degrees, so we fake it
            } else {
                angle = percent * 360;
            }

            arcSegment.IsLargeArc = angle >= 180.0;
            arcSegment.Point = new Point(Math.Cos((startAngle + angle) * Math.PI / 180) * radius, Math.Sin((startAngle + angle) * Math.PI / 180) * radius);
            arcSegment.Size = new Size(radius, radius);
            arcSegment.SweepDirection = SweepDirection.Clockwise;

            pathFigure.Segments.Add(lineSegment);
            pathFigure.Segments.Add(arcSegment);

            pathGeometry.Figures.Add(pathFigure);
            path.Data = pathGeometry;

            endAngle = startAngle + angle;

            if (!IsEnlargeable) {
                if (ByInstances)
                    path.ToolTip = new PieChartToolTip(fixtureSummary.FixtureClass, ByInstances, fixtureSummary.Count.ToString(), fixtureSummary.PercentCount);
                else
                    path.ToolTip = new PieChartToolTip(fixtureSummary.FixtureClass, ByInstances, fixtureSummary.Volume.ToString("0.0"), fixtureSummary.PercentVolume);
            }
            ToolTipService.SetShowDuration(path, 60000);
            ToolTipService.SetInitialShowDelay(path, 500);

            return path;
        }
    }
}
