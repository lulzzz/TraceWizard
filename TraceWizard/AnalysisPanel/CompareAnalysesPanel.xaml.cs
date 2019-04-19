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
using TraceWizard.Environment;
using TraceWizard.Classification;
using TraceWizard.Classification.Classifiers.J48;
using TraceWizard.Classification.Classifiers.Composite;
using TraceWizard.Classification.Classifiers.FixtureList;

namespace TraceWizard.TwApp {
    public partial class CompareAnalysesPanel : UserControl {

        public Analysis AnalysisUpper { get; set; }
        public Analysis AnalysisLower { get; set; }

        public Classifier ClassifierMachineLearning;
        public FixtureListClassifier ClassifierFixtureListUpper;
        public FixtureListClassifier ClassifierFixtureListLower;

        public CompareAnalysesPanel() {
            InitializeComponent();
        }

        public void Initialize() {

            ClassifierMachineLearning = GetClassifierMachineLearning();
            ClassifierFixtureListUpper = new FixtureListClassifier();
            ClassifierFixtureListUpper.FixtureProfiles = AnalysisUpper.FixtureProfiles;

            var classifierCompositeUpper = new CompositeClassifier();
            classifierCompositeUpper.ClassifierMachineLearning = ClassifierMachineLearning;
            classifierCompositeUpper.ClassifierFixtureList = ClassifierFixtureListUpper;

            StyledEventsViewerUpper.EventsViewer.LinedEventsCanvas.ClassifierDisaggregation = classifierCompositeUpper;
            StyledEventsViewerUpper.Events = AnalysisUpper.Events;
            StyledEventsViewerUpper.ViewportSeconds = EventsViewer.SecondsTwoHours;
            StyledEventsViewerUpper.ViewportVolume = EventsViewer.VolumeTen;
            StyledEventsViewerUpper.Initialize();

            ClassifierFixtureListLower = new FixtureListClassifier();
            ClassifierFixtureListLower.FixtureProfiles = AnalysisLower.FixtureProfiles;

            var classifierCompositeLower = new CompositeClassifier();
            classifierCompositeLower.ClassifierMachineLearning = ClassifierMachineLearning;
            classifierCompositeLower.ClassifierFixtureList = ClassifierFixtureListLower;

            StyledEventsViewerLower.EventsViewer.LinedEventsCanvas.ClassifierDisaggregation = classifierCompositeLower;
            StyledEventsViewerLower.Events = AnalysisLower.Events;
            StyledEventsViewerLower.ViewportSeconds = EventsViewer.SecondsTwoHours;
            StyledEventsViewerLower.ViewportVolume = EventsViewer.VolumeTen;
            StyledEventsViewerLower.Initialize();

            ButtonLinkScrollBars.IsEnabled = CanLinkScrollBars(AnalysisUpper, AnalysisLower);
            ButtonSynchronizeGraphs.IsEnabled = CanSynchronizeGraphs(AnalysisUpper, AnalysisLower);

            ButtonSynchronizeGraphs.Click += new RoutedEventHandler(ButtonSynchronizeGraphs_Click);
            ButtonLinkScrollBars.Click += new RoutedEventHandler(ButtonLinkScrollBars_Click);
            ButtonAppend.Click += new RoutedEventHandler(ButtonAppend_Click);

            ButtonAppend.MouseEnter +=new MouseEventHandler(ButtonAppend_MouseEnter);
            ButtonSynchronizeGraphs.MouseEnter += new MouseEventHandler(ButtonSynchronizeGraphs_MouseEnter);

            this.Loaded += new RoutedEventHandler(CompareAnalysesPanel_Loaded);
        }

        Classifier GetClassifierMachineLearning() {
            var classifiers = TwClassifiers.CreateClassifiers();
            foreach (var classifier in classifiers) {
                if (classifier.Name.ToLower() == Properties.Settings.Default.Classifier.ToLower().Trim())
                    return classifier;
            }
            return new J48Classifier();
        }

        void ButtonAppend_MouseEnter(object sender, MouseEventArgs e) {
            var dateTimeUpperToStartCopyingAt = GetEndTimeView(StyledEventsViewerUpper, AnalysisUpper);
            var dateTimeLowerToEndCopyingAt = GetEndTimeView(StyledEventsViewerLower, AnalysisLower);
            ButtonAppend.ToolTip = "Creates new trace starting at beginning of lower trace until " + dateTimeLowerToEndCopyingAt.ToString() 
                + ", then appending the upper trace starting at "
                + dateTimeUpperToStartCopyingAt.ToString() + ".";
        }

        void ButtonSynchronizeGraphs_MouseEnter(object sender, MouseEventArgs e) {
            var dateTimeUpperToStartCopyingAt = GetEndTimeView(StyledEventsViewerUpper, AnalysisUpper);
            var dateTimeLowerToEndCopyingAt = GetEndTimeView(StyledEventsViewerLower, AnalysisLower);

            ButtonSynchronizeGraphs.ToolTip = ToolTip = string.Format("Synchronizes upper and lower graphs, scrolling the end of both views to the end of the lower trace (at {0}). (Lower trace must end during upper one.",
                AnalysisLower.Events.EndTime);
        }

        void CompareAnalysesPanel_Loaded(object sender, RoutedEventArgs e) {
            StyledEventsViewerLower.EventsViewer.ScrollViewer.Focus();
            WorkaroundForIsDirtyBug();
        }

        void WorkaroundForIsDirtyBug() {
            AnalysisUpper.Events.IsDirty = false;
            AnalysisLower.Events.IsDirty = false;
        }

        bool CanLinkScrollBars(Analysis analysisUpper, Analysis analysisLower) {
            return (AreDatesClose(analysisUpper.Events.StartTime, analysisLower.Events.StartTime)
                &&
                AreDatesClose(analysisUpper.Events.EndTime, analysisLower.Events.EndTime));
        }

        bool CanSynchronizeGraphs(Analysis analysisUpper, Analysis analysisLower) {
            return (analysisLower.Events.EndTime >= analysisUpper.Events.StartTime
                && analysisLower.Events.EndTime <= analysisUpper.Events.EndTime);
        }

        bool AreDatesClose(DateTime datetime1, DateTime datetime2) {
            return (Math.Abs(datetime1.Subtract(datetime2).TotalSeconds) < 60);
//            return (Math.Abs(datetime1.Subtract(datetime2).TotalSeconds) == 0);
        }

        double Between(double min, double max, double value) {
            if (min > value) return min;
            if (value > max) return max;
            return value;
        }

        void ButtonSynchronizeGraphs_Click(object sender, RoutedEventArgs e) {
            StyledEventsViewerLower.EventsViewer.ScrollViewer.ScrollToRightEnd();

            double percentElapsed = (double)AnalysisLower.Events.EndTime.Subtract(AnalysisUpper.Events.StartTime).Ticks / (double)AnalysisUpper.Events.Duration.Ticks;
            StyledEventsViewerUpper.EventsViewer.ScrollViewer.ScrollToHorizontalOffset(Between(0, StyledEventsViewerUpper.EventsViewer.LinedEventsCanvas.Width - StyledEventsViewerUpper.EventsViewer.ScrollViewer.ViewportWidth, (StyledEventsViewerUpper.EventsViewer.LinedEventsCanvas.Width * percentElapsed) - StyledEventsViewerUpper.EventsViewer.ScrollViewer.ViewportWidth));
            
            StyledEventsViewerUpper.EventsViewer.ScrollViewer.Focus();
        }

        void ButtonLinkScrollBars_Click(object sender, RoutedEventArgs e) {
            Mouse.OverrideCursor = Cursors.Wait;

            StyledEventsViewerLower.EventsViewer.ScrollViewer.ScrollToLeftEnd();
            StyledEventsViewerLower.EventsViewer.ScrollViewer.Focus();

            StyledEventsViewerUpper.EventsViewer.ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            StyledEventsViewerUpper.EventsViewer.ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            StyledEventsViewerLower.EventsViewer.ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            StyledEventsViewerUpper.EventsViewer.HorizontalRuler.Visibility = Visibility.Hidden;
            StyledEventsViewerUpper.TimeFramePanel.Visibility = Visibility.Hidden;

            StyledEventsViewerUpper.IsEnabled = false;

            StyledEventsViewerUpper.EventsViewer.LinedEventsCanvas.RemoveVerticalGuidelinePermanently();
            StyledEventsViewerLower.EventsViewer.LinedEventsCanvas.RemoveVerticalGuidelinePermanently();

            BindScrollViewers(StyledEventsViewerLower.EventsViewer.ScrollViewer, StyledEventsViewerUpper.EventsViewer.ScrollViewer);

            ButtonSynchronizeGraphs.IsEnabled = false;
            ButtonLinkScrollBars.IsEnabled = false;
            ButtonAppend.IsEnabled = false;

            Mouse.OverrideCursor = null;

        }

        int GetSecondsOffsetEndTimeView(EventsViewer eventsViewer, Events events) {
            double percentElapsed = ((eventsViewer.ScrollViewer.HorizontalOffset + eventsViewer.ScrollViewer.ViewportWidth) / eventsViewer.LinedEventsCanvas.Width);
            return (int)(percentElapsed * events.Duration.TotalSeconds);
        }
        
        DateTime GetEndTimeView(StyledEventsViewer eventsViewer, Analysis analysis) {
            int secondsOffset = GetSecondsOffsetEndTimeView(eventsViewer.EventsViewer, analysis.Events);
            return analysis.Events.StartTime.Add(new TimeSpan(0, 0, secondsOffset));
        }
        
        void ButtonAppend_Click(object sender, RoutedEventArgs e) {
            double percentElapsed = (StyledEventsViewerUpper.EventsViewer.ScrollViewer.HorizontalOffset / StyledEventsViewerUpper.EventsViewer.LinedEventsCanvas.Width);
            int secondsOffset = (int)(percentElapsed * AnalysisUpper.Events.Duration.TotalSeconds);
            var dateTimeUpperToStartCopyingAt = GetEndTimeView(StyledEventsViewerUpper, AnalysisUpper);
            var dateTimeLowerToEndCopyingAt = GetEndTimeView(StyledEventsViewerLower, AnalysisLower);

            if (Events.HasEventInProgress(AnalysisUpper.Events, dateTimeUpperToStartCopyingAt)
                || Events.HasEventInProgress(AnalysisLower.Events, dateTimeLowerToEndCopyingAt)) {
                MessageBox.Show("In order to append, please scroll the upper and lower graphs such that no event is in progress at the end of the view shown.",TwAssembly.TitleTraceWizard(),MessageBoxButton.OK,MessageBoxImage.Warning);
            } else {

                var analysisAppended = Analysis.Append(AnalysisLower, AnalysisUpper, dateTimeLowerToEndCopyingAt, dateTimeUpperToStartCopyingAt);

                string fileName = analysisAppended.KeyCode;

                Mouse.OverrideCursor = Cursors.Wait;

                MainTwWindow mainWindow = (MainTwWindow)Application.Current.MainWindow;

                var analysisPanel = mainWindow.CreateAnalysisPanel(analysisAppended, fileName, false);
                mainWindow.AddTab(analysisPanel, fileName);

                Mouse.OverrideCursor = null;
            }
        }

        void BindScrollViewers(ScrollViewer source, ScrollViewer target) {
            var translateTransform = new TranslateTransform();

            Binding binding = new Binding("HorizontalOffset");
            binding.Source = source;
            binding.Converter = new ScrollViewerHorizontalConverter();
            BindingOperations.SetBinding(translateTransform, TranslateTransform.XProperty, binding);

            Canvas canvas = (Canvas)target.Content;
            canvas.RenderTransform = translateTransform;

            //translateTransform = new TranslateTransform();
            //binding = new Binding("VerticalOffset");
            //binding.Source = source;
            //binding.Converter = new ScrollViewerVerticalConverter();
            //BindingOperations.SetBinding(translateTransform, TranslateTransform.YProperty, binding);

            //canvas = (Canvas)target.Content;
            //canvas.RenderTransform = translateTransform;
        }
    }
}
