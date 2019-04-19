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
using TraceWizard.Logging.Adapters;
using TraceWizard.Classification;
using TraceWizard.Classification.Classifiers.FixtureList;
using TraceWizard.Classification.Classifiers.J48;
using TraceWizard.Classification.Classifiers.Composite;
using TraceWizard.Notification;
using TraceWizard.FeatureLevels;
using TraceWizard.Commanding;

namespace TraceWizard.TwApp {
    public partial class AnalysisPanel : UserControl, INotifyPropertyChanged {

        public FeatureLevel FeatureLevel;
        public Event SelectedEventView = null;
        
        public Analysis Analysis { get;set;}
        public double ViewportSeconds { get; set; }
        public double ViewportVolume { get; set; }
        public UndoPosition UndoPosition {
            get { return StyledEventsViewer.EventsViewer.UndoPosition; }
        }

        Classifier ClassifierMachineLearning;
        CompositeClassifier ClassifierComposite;

        public UndoPosition BookmarkPosition {
            get { return StyledEventsViewer.EventsViewer.BookmarkPosition; }
        }

        public AnalysisPanel() {
            InitializeTwCommandSelected();
            InitializeCommands();
            InitializeComponent();
        }

        public void Initialize() {

            ClassifierMachineLearning = GetClassifierMachineLearning();
            var classifierFixtureList = new FixtureListClassifier();
            classifierFixtureList.FixtureProfiles = Analysis.FixtureProfiles;

            ClassifierComposite = new CompositeClassifier();
            ClassifierComposite.ClassifierMachineLearning = ClassifierMachineLearning;
            ClassifierComposite.ClassifierFixtureList = classifierFixtureList;
            
            Analysis.UndoManager.RenderEvent = StyledEventsViewer.EventsViewer.LinedEventsCanvas.RenderEvent;
            Analysis.UndoManager.RemovePolygon = StyledEventsViewer.EventsViewer.LinedEventsCanvas.Remove;

            StyledEventsViewer.EventsViewer.LinedEventsCanvas.PropertyChanged += new PropertyChangedEventHandler(EventsCanvas_PropertyChanged);
            GraphToolBar.PropertyChanged += new PropertyChangedEventHandler(GraphToolBar_PropertyChanged);
            CommandPanel.PropertyChanged += new PropertyChangedEventHandler(CommandPanel_PropertyChanged);

            GraphToolBar.Analysis = Analysis;
            GraphToolBar.FeatureLevel = FeatureLevel;
            GraphToolBar.UndoManager = Analysis.UndoManager;
            GraphToolBar.ClassifierMachineLearning = ClassifierMachineLearning;
            //GraphToolBar.ClassifierFixtureList = classifierComposite;
            GraphToolBar.Initialize();

            CommandPanel.AnalysisPanel = this;
            CommandPanel.Initialize();

            FixtureProfilesEditor.Analysis = this.Analysis;
            FixtureProfilesEditor.Initialize();

            Binding binding;

            if (Properties.Settings.Default.ShowEventPanelsOnLeft) {

                dockEventPanelsHorizontal.Visibility = Visibility.Collapsed;

                SelectedEventPanelVertical.Analysis = Analysis;
                Analysis.Events.PropertyChanged += new PropertyChangedEventHandler(SelectedEventPanelVertical.Events_PropertyChanged);
                SelectedEventPanelVertical.Initialize();
                binding = new Binding("Visibility");
                binding.Source = SelectedEventPanelVertical;
                BindingOperations.SetBinding(SeparatorSelectedEventPanelVertical, VisibilityProperty, binding);

                CurrentEventPanelVertical.Analysis = Analysis;
                StyledEventsViewer.EventsViewer.LinedEventsCanvas.PropertyChanged += new PropertyChangedEventHandler(CurrentEventPanelVertical.EventsCanvas_PropertyChanged);
                CurrentEventPanelVertical.ShowSelectedEvent = false;
                CurrentEventPanelVertical.Initialize();
            } else {

                dockEventPanelsVertical.Visibility = Visibility.Collapsed;

                SelectedEventPanel.Analysis = Analysis;
                Analysis.Events.PropertyChanged += new PropertyChangedEventHandler(SelectedEventPanel.Events_PropertyChanged);
                SelectedEventPanel.Initialize();
                binding = new Binding("Visibility");
                binding.Source = SelectedEventPanel;
                BindingOperations.SetBinding(SeparatorSelectedEventPanel, VisibilityProperty, binding);

                CurrentEventPanel.Analysis = Analysis;
                StyledEventsViewer.EventsViewer.LinedEventsCanvas.PropertyChanged += new PropertyChangedEventHandler(CurrentEventPanel.EventsCanvas_PropertyChanged);
                CurrentEventPanel.ShowSelectedEvent = false;
                CurrentEventPanel.Initialize();
                binding = new Binding("Visibility");
                binding.Source = CurrentEventPanel;
                BindingOperations.SetBinding(SeparatorCurrentEventPanel, VisibilityProperty, binding);
            }

            LogMeter log = Analysis.Log as LogMeter;
            if (log == null)
                LogPropertiesPanel.Visibility = Visibility.Collapsed;
            else {
                LogPropertiesPanel.Analysis = Analysis;
                LogPropertiesPanel.Initialize();
            }

            binding = new Binding("Visibility");
            binding.Source = LogPropertiesPanel;
            BindingOperations.SetBinding(SeparatorLogPanel, VisibilityProperty, binding);

            PieChartsPanel.Analysis = Analysis;
            PieChartsPanel.Initialize();

            binding = new Binding("Visibility");
            binding.Source = PieChartsPanel;
            BindingOperations.SetBinding(SeparatorPieChartsPanel, VisibilityProperty, binding);

            ReportsPanel.Analysis = Analysis;
            ReportsPanel.Initialize();

            binding = new Binding("Visibility");
            binding.Source = ReportsPanel;
            BindingOperations.SetBinding(SeparatorReportsPanel, VisibilityProperty, binding);

            FixturesPanel.Analysis = Analysis;
            FixturesPanel.Initialize();

            StyledEventsViewer.EventsViewer.LinedEventsCanvas.ClassifierDisaggregation = ClassifierComposite;            
            StyledEventsViewer.Events = Analysis.Events;
            StyledEventsViewer.ViewportSeconds = ViewportSeconds;
            StyledEventsViewer.ViewportVolume = ViewportVolume;
            StyledEventsViewer.Initialize();

        }

        Classifier GetClassifierMachineLearning() {
            var classifiers = TwClassifiers.CreateClassifiers();
            foreach (var classifier in classifiers) {
                    if (classifier.Name.ToLower() == Properties.Settings.Default.Classifier.ToLower().Trim())
                        return classifier;
            }
            return new J48Classifier();
        }
        
        public TwFindMode FindMode = TwFindMode.None;
        public enum TwFindMode {
            None = 0,
            AnyFixture = 1,
            SelectedFixture = 2,
            SimilarFixture = 3
        }

        public TwClassificationActionMode ClassificationActionMode = TwClassificationActionMode.None;
        public enum TwClassificationActionMode {
            None = 0,
            Selected = 1,
            All = 2,
            Forward = 3
        }

        public TwManualClassificationMode ManualClassificationMode = TwManualClassificationMode.None;
        public enum TwManualClassificationMode {
            None = 0,
            WithoutAdoption = 1,
            WithAdoption = 2
        }
        
        string saveFileName { get; set; }
        public string SaveFileName {
            get { return saveFileName; }
            set {
                saveFileName = value;
                if (!string.IsNullOrEmpty(saveFileName))
                    OnPropertyChanged(TwNotificationProperty.OnChangeFileName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void GraphToolBar_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case TwNotificationProperty.OnEnterHorizontalSplitMode:
                    StyledEventsViewer.EventsViewer.LinedEventsCanvas.HorizontalSplitMode = true;
                    break;
                case TwNotificationProperty.OnLeaveHorizontalSplitMode:
                    StyledEventsViewer.EventsViewer.LinedEventsCanvas.HorizontalSplitMode = false;
                    break;
                case TwNotificationProperty.OnEnterVerticalSplitMode:
                    StyledEventsViewer.EventsViewer.LinedEventsCanvas.VerticalSplitMode = true;
                    break;
                case TwNotificationProperty.OnLeaveVerticalSplitMode:
                    StyledEventsViewer.EventsViewer.LinedEventsCanvas.VerticalSplitMode = false;
                    break;
                case TwNotificationProperty.OnEnterMergeAllIntoBaseMode:
                    StyledEventsViewer.EventsViewer.LinedEventsCanvas.MergeAllIntoBaseMode = true;
                    break;
                case TwNotificationProperty.OnLeaveMergeAllIntoBaseMode:
                    StyledEventsViewer.EventsViewer.LinedEventsCanvas.MergeAllIntoBaseMode = false;
                    break;
                case TwNotificationProperty.OnStartDrag:
                    GraphToolBar.CanStartDragging = false;
                    StyledEventsViewer.EventsViewer.LinedEventsCanvas.CanStartDragging = false;
                    break;
                case TwNotificationProperty.OnEndDrag:
                    GraphToolBar.CanStartDragging = true;
                    StyledEventsViewer.EventsViewer.LinedEventsCanvas.CanStartDragging = true;
                    break;
                case TwNotificationProperty.OnFindModeAnyFixture:
                    FindMode = TwFindMode.AnyFixture;
                    break;
                case TwNotificationProperty.OnFindModeSelectedFixture:
                    FindMode = TwFindMode.SelectedFixture;
                    break;
                case TwNotificationProperty.OnFindModeSimilarFixture:
                    FindMode = TwFindMode.SimilarFixture;
                    break;
                case TwNotificationProperty.OnClassificationActionModeSelected:
                    ClassificationActionMode = TwClassificationActionMode.Selected;
                    break;
                case TwNotificationProperty.OnClassificationActionModeAll:
                    ClassificationActionMode = TwClassificationActionMode.All;
                    break;
                case TwNotificationProperty.OnClassificationActionModeForward:
                    ClassificationActionMode = TwClassificationActionMode.Forward;
                    break;
                case TwNotificationProperty.OnManualClassificationModeWithoutAdoption:
                    ManualClassificationMode = TwManualClassificationMode.WithoutAdoption;
                    StyledEventsViewer.EventsViewer.ManualClassificationMode = ManualClassificationMode;
                    break;
                case TwNotificationProperty.OnManualClassificationModeWithAdoption:
                    ManualClassificationMode = TwManualClassificationMode.WithAdoption;
                    StyledEventsViewer.EventsViewer.ManualClassificationMode = ManualClassificationMode;
                    break;
                case TwNotificationProperty.OnShowSelectionRuler:
                    StyledEventsViewer.SelectionRuler.VisibleRequested = true;
                    StyledEventsViewer.SelectionRuler.Render();
                    break;
                case TwNotificationProperty.OnHideSelectionRuler:
                    StyledEventsViewer.SelectionRuler.VisibleRequested = false;
                    StyledEventsViewer.SelectionRuler.Render();
                    break;
                case TwNotificationProperty.OnShowApprovalRuler:
                    StyledEventsViewer.ApprovalRuler.VisibleRequested = true;
                    StyledEventsViewer.ApprovalRuler.Render();
                    break;
                case TwNotificationProperty.OnHideApprovalRuler:
                    StyledEventsViewer.ApprovalRuler.VisibleRequested = false;
                    StyledEventsViewer.ApprovalRuler.Render();
                    break;
                case TwNotificationProperty.OnShowClassificationRuler:
                    StyledEventsViewer.ClassificationRuler.VisibleRequested = true;
                    StyledEventsViewer.ClassificationRuler.Render();
                    break;
                case TwNotificationProperty.OnHideClassificationRuler:
                    StyledEventsViewer.ClassificationRuler.VisibleRequested = false;
                    StyledEventsViewer.ClassificationRuler.Render();
                    break;
                case TwNotificationProperty.OnShowFixturesRuler:
                    StyledEventsViewer.FixturesRuler.VisibleRequested = true;
                    StyledEventsViewer.FixturesRuler.Render();
                    break;
                case TwNotificationProperty.OnHideFixturesRuler:
                    StyledEventsViewer.FixturesRuler.VisibleRequested = false;
                    StyledEventsViewer.FixturesRuler.Render();
                    break;
            }
        }

        PolygonHelp polygonHelp = new PolygonHelp();
        ZoomHelp zoomHelp = new ZoomHelp();
        CommandHelp commandHelp = new CommandHelp();

        public void CommandPanel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case TwNotificationProperty.OnMouseEnterCommand:
                    StatusBar.Content = commandHelp;
                    break;
                case TwNotificationProperty.OnMouseLeaveCommand:
                    StatusBar.Content = string.Empty;
                    break;
                case TwNotificationProperty.OnCommandSuccess:
                    StatusBar.Content = CreateCommandSuccessMessage(CommandPanel.TextBlockResult.Text);
                    break;
                case TwNotificationProperty.OnCommandError:
                    StatusBar.Content = CreateCommandErrorMessage(CommandPanel.TextBlockResult.Text);
                    break;
            }
        }

        UIElement CreateCommandSuccessMessage(string message) {
            var textBlock = new TextBlock();
            textBlock.Text = "Executed: " + message;
            textBlock.Foreground = Brushes.DarkGray;
            textBlock.FontSize = 12;
            return textBlock;
        }

        UIElement CreateCommandErrorMessage(string message) {
            var textBlock = new TextBlock();
            textBlock.Text = "Command Error: " + message;
            textBlock.Foreground = Brushes.Red;
            textBlock.FontWeight = FontWeights.Bold;
            textBlock.FontSize = 12;
            return textBlock;
        }

        bool inPolygon = false;
        public void EventsCanvas_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case TwNotificationProperty.OnMouseEnterPolygon:
                    inPolygon = true;
                    StatusBar.Content = polygonHelp;
                    break;
                case TwNotificationProperty.OnMouseLeavePolygon:
                    inPolygon = false;
                    StatusBar.Content = zoomHelp;
                    break;
                case TwNotificationProperty.OnMouseEnterEventsCanvas:
                    if (!inPolygon)
                        StatusBar.Content = zoomHelp;
                    break;
                case TwNotificationProperty.OnMouseLeaveEventsCanvas:
                    StatusBar.Content = string.Empty;
                    break;
                case TwNotificationProperty.OnLeaveHorizontalSplitMode:
                case TwNotificationProperty.OnLeaveVerticalSplitMode:
                case TwNotificationProperty.OnLeaveMergeAllIntoBaseMode:
                    GraphToolBar.ClearMergeSplitButtons();
                    break;
                case TwNotificationProperty.OnAddFixtureRequested:
                    FixtureProfilesEditor.AddFixture(GetCurrentEventOrSelectedEvents());
                    break;
                case TwNotificationProperty.OnApplyFixtureRequested:
                    FixtureProfilesEditor.ApplyFixture(GetCurrentEventOrSelectedEvents());
                    break;
            }
        }

        List<Event> GetCurrentEventOrSelectedEvents() {
            if (Analysis.Events.CurrentEvent != null) {
                if (Analysis.Events.SelectedEvents.Contains(Analysis.Events.CurrentEvent))
                    return Analysis.Events.SelectedEvents;
                else {
                    var list = new List<Event>();
                    list.Add(Analysis.Events.CurrentEvent);
                    return list;
                }
            } else
                return Analysis.Events.SelectedEvents;
        }
        
        void UndoCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            var position = Analysis.UndoManager.Undo();
            StyledEventsViewer.EventsViewer.RestoreScrollPosition(position);
        }

        void RedoCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            var position = Analysis.UndoManager.Redo();
            StyledEventsViewer.EventsViewer.RestoreScrollPosition(position);
        }

        void UndoCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Analysis.UndoManager.CanUndo();
            e.Handled = true;
        }

        void RedoCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Analysis.UndoManager.CanRedo();
            e.Handled = true;
        }

        bool AdoptionRequested() {
            return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)
                || ManualClassificationMode == AnalysisPanel.TwManualClassificationMode.WithAdoption;
        }

        void ManuallyClassifyCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            var fixtureClass = e.Parameter as FixtureClass;
            if (fixtureClass != null) {
                Mouse.OverrideCursor = Cursors.Wait;

                if (AdoptionRequested())
                    Analysis.Events.ManuallyClassify(Analysis.Events.SelectedEvents, fixtureClass, true, UndoPosition);
                else
                    Analysis.Events.ManuallyClassify(Analysis.Events.SelectedEvents, fixtureClass, false, UndoPosition);
                Mouse.OverrideCursor = null;

            }
        }

        void FindPreviousCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            var fixtureClass = e.Parameter as FixtureClass;
            if (fixtureClass != null)
                StyledEventsViewer.EventsViewer.FindPrevious(StyledEventsViewer.EventsViewer.GetPreviousFromPosition(), fixtureClass);
            else {
                switch (FindMode) {
                    case TwFindMode.AnyFixture:
                        StyledEventsViewer.EventsViewer.FindPreviousAnyBySelectedInView();
                        break;
                    case TwFindMode.SelectedFixture:
                        StyledEventsViewer.EventsViewer.FindPreviousBySelectedInView();
                        break;
                    case TwFindMode.SimilarFixture:
                        StyledEventsViewer.EventsViewer.FindPreviousSimilarBySelectedInView();
                        break;
                }
            }
        }

        void FindNextCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            var fixtureClass = e.Parameter as FixtureClass;
            if (fixtureClass != null)
                StyledEventsViewer.EventsViewer.FindNext(StyledEventsViewer.EventsViewer.GetNextFromPosition(), fixtureClass);
            else {
                switch (FindMode) {
                    case TwFindMode.AnyFixture:
                        StyledEventsViewer.EventsViewer.FindNextAnyBySelectedInView();
                        break;
                    case TwFindMode.SelectedFixture:
                        StyledEventsViewer.EventsViewer.FindNextBySelectedInView();
                        break;
                    case TwFindMode.SimilarFixture:
                        StyledEventsViewer.EventsViewer.FindNextSimilarBySelectedInView();
                        break;
                }
            }
        }

        public delegate void BringEventIntoView(Event @event);

        public delegate void Finder();
        public delegate void FinderEvent(Event @event);
        public delegate void FinderFixture(FixtureClass fixtureClass);
        public delegate void FinderEventFixture(Event @event, FixtureClass fixtureClass);

        void ManuallyClassifyCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
            e.Handled = true;
        }

        Point GetZoomCenterPosition() {
            double x = StyledEventsViewer.EventsViewer.ScrollViewer.ViewportWidth / 2;
            double y = StyledEventsViewer.EventsViewer.ScrollViewer.ViewportHeight / 2;
            return new Point(x, y);
        }

        void DecreaseVerticalZoomCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = !StyledEventsViewer.EventsViewer.zoomInProgress && StyledEventsViewer.EventsViewer.IsZoomEnabled && StyledEventsViewer.EventsViewer.CanIncreaseViewportVolume();
            e.Handled = true;
        }

        void DecreaseVerticalZoomCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            StyledEventsViewer.EventsViewer.DecreaseVerticalZoomExecuted(GetZoomCenterPosition().X);
        }

        void IncreaseVerticalZoomCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = !StyledEventsViewer.EventsViewer.zoomInProgress && StyledEventsViewer.EventsViewer.IsZoomEnabled && StyledEventsViewer.EventsViewer.CanDecreaseViewportVolume();
            e.Handled = true;
        }

        void IncreaseVerticalZoomCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            StyledEventsViewer.EventsViewer.IncreaseVerticalZoomExecuted(GetZoomCenterPosition().X);
        }

        void DecreaseZoomCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = !StyledEventsViewer.EventsViewer.zoomInProgress && StyledEventsViewer.EventsViewer.IsZoomEnabled && StyledEventsViewer.EventsViewer.CanIncreaseViewportSeconds();
            e.Handled = true;
        }

        void DecreaseZoomCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            StyledEventsViewer.EventsViewer.DecreaseZoomExecuted(GetZoomCenterPosition().X);
        }

        void IncreaseZoomCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
            StyledEventsViewer.EventsViewer.IncreaseZoomExecuted(GetZoomCenterPosition().X);
        }

        void IncreaseZoomCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = !StyledEventsViewer.EventsViewer.zoomInProgress && StyledEventsViewer.EventsViewer.IsZoomEnabled && StyledEventsViewer.EventsViewer.CanDecreaseViewportSeconds();
            e.Handled = true;
        }

        RoutedUICommand InitializeCommand(string commandText, string commandName, ExecutedRoutedEventHandler handlerExecute, CanExecuteRoutedEventHandler handlerCanExecute) {
            var command = new RoutedUICommand(commandText, commandName, typeof(AnalysisPanel));
            CommandBinding commandBinding = new CommandBinding(command, handlerExecute, handlerCanExecute);
            CommandBindings.Add(commandBinding);
            return command;
        }

        RoutedUICommand InitializeCommand(string commandText, string commandName, ExecutedRoutedEventHandler handlerExecute) {
            return InitializeCommand(commandText, commandName, handlerExecute, CanExecute);
        }

        public static RoutedUICommand IncreaseZoomCenterCommand;
        public static RoutedUICommand DecreaseZoomCenterCommand;
        public static RoutedUICommand IncreaseVerticalZoomCenterCommand;
        public static RoutedUICommand DecreaseVerticalZoomCenterCommand;

        public static RoutedUICommand SelectAllCommand;

        public static RoutedUICommand FindPreviousCommand;
        public static RoutedUICommand FindNextCommand;

        public static RoutedUICommand ManuallyClassifyCommand;

        public static RoutedUICommand UndoCommand;
        public static RoutedUICommand RedoCommand;

        public static RoutedUICommand ClassifyUsingMachineLearningCommand;
        public static RoutedUICommand MachineClassifyFirstCyclesCommand;

        public static RoutedUICommand ClassifyUsingFixtureListCommand;

        public static RoutedUICommand ManuallyClassifyFirstCyclesCommand;

        public static RoutedUICommand SetBookmarkCommand;
        public static RoutedUICommand GoToBookmarkCommand;

        public static RoutedUICommand RefreshCommand;

        public static RoutedUICommand BringSelectedEventIntoViewCommand;
        public static RoutedUICommand BringNextSelectedEventIntoViewCommand;
        public static RoutedUICommand BringPreviousSelectedEventIntoViewCommand;

        public static RoutedUICommand UnapproveCommand;
        public static RoutedUICommand ApproveCommand;
        public static RoutedUICommand ApproveAllPreviousCommand;
        public static RoutedUICommand ApproveInViewCommand;

        public static RoutedUICommand AddFixtureCommand;
        public static RoutedUICommand ApplyFixtureCommand;

        void InitializeCommands() {
            IncreaseZoomCenterCommand = InitializeCommand("Horizontal Zoom In", "IncreaseHorizontalZoomCommand", IncreaseZoomCommandExecuted, IncreaseZoomCanExecute);
            DecreaseZoomCenterCommand = InitializeCommand("Horizontal Zoom Out", "DecreaseHorizontalZoomCommand", DecreaseZoomCommandExecuted, DecreaseZoomCanExecute);
            IncreaseVerticalZoomCenterCommand = InitializeCommand("Vertical Zoom In", "IncreaseVerticalZoomCommand", IncreaseVerticalZoomCommandExecuted, IncreaseVerticalZoomCanExecute);
            DecreaseVerticalZoomCenterCommand = InitializeCommand("Vertical Zoom Out", "DecreaseVerticalZoomCommand", DecreaseVerticalZoomCommandExecuted, DecreaseVerticalZoomCanExecute);

            SelectAllCommand = InitializeCommand("EnterFullScreen", "EnterFullScreenCommand", SelectAllExecuted);

            FindPreviousCommand = InitializeCommand("Find Previous", "FindPreviousCommand", FindPreviousCommandExecuted, CanExecute);
            FindNextCommand = InitializeCommand("Find Next", "FindNextCommand", FindNextCommandExecuted, CanExecute);

            ManuallyClassifyCommand = InitializeCommand("Manually Classify", "ManuallyClassifyCommand", ManuallyClassifyCommandExecuted, ManuallyClassifyCanExecute);

            UndoCommand = InitializeCommand("Undo", "UndoCommand", UndoCommandExecuted, UndoCanExecute);
            RedoCommand = InitializeCommand("Redo", "RedoCommand", RedoCommandExecuted, RedoCanExecute);

            MachineClassifyFirstCyclesCommand = InitializeCommand("ClassifyFirstCycles", "ClassifyFirstCyclesCommand", MachineClassifyFirstCyclesExecuted, CanExecute);
            this.InputBindings.Add(new KeyBinding(MachineClassifyFirstCyclesCommand, new KeyGesture(Key.F5, ModifierKeys.Control | ModifierKeys.Alt)));

            ClassifyUsingMachineLearningCommand = InitializeCommand("Classify", "ClassifyCommand", ClassifyUsingMachineLearningExecuted, ClassifyCanExecute);
            this.InputBindings.Add(new KeyBinding(ClassifyUsingMachineLearningCommand, new KeyGesture(Key.F5, ModifierKeys.None)));

            ClassifyUsingFixtureListCommand = InitializeCommand("Classify Using Fixture List", "ClassifyUsingFixtureListCommand", ClassifyUsingFixtureListExecuted, ClassifyCanExecute);
            this.InputBindings.Add(new KeyBinding(ClassifyUsingFixtureListCommand, new KeyGesture(Key.F6, ModifierKeys.None)));

            ManuallyClassifyFirstCyclesCommand = InitializeCommand("UserClassifyFirstCycles", "UserClassifyFirstCyclesCommand", ManuallyClassifyFirstCyclesExecuted, CanExecute);

            SetBookmarkCommand = InitializeCommand("SetBookmark", "SetBookmarkCommand", SetBookmarkExecuted, CanExecute);
            this.InputBindings.Add(new KeyBinding(SetBookmarkCommand, new KeyGesture(Key.F1, ModifierKeys.None)));

            GoToBookmarkCommand = InitializeCommand("GoToBookmark", "GoToBookmarkCommand", GoToBookmarkExecuted, GoToBookmarkCanExecute);
            this.InputBindings.Add(new KeyBinding(GoToBookmarkCommand, new KeyGesture(Key.F2, ModifierKeys.None)));

            RefreshCommand = InitializeCommand("Refresh", "RefreshCommand", RefreshExecuted, CanExecute);
            this.InputBindings.Add(new KeyBinding(RefreshCommand, new KeyGesture(Key.F5, ModifierKeys.Control)));

            BringSelectedEventIntoViewCommand = InitializeCommand("Bring Selected Event Into View", "BringSelectedEventIntoViewCommand", BringSelectedEventIntoViewExecuted, BringSelectedEventIntoViewCanExecute);

            BringPreviousSelectedEventIntoViewCommand = InitializeCommand("Bring Previous Selected Event Into View", "BringPreviousSelectedEventIntoViewCommand", BringPreviousSelectedEventIntoViewExecuted, BringPreviousSelectedEventIntoViewCanExecute);
            this.InputBindings.Add(new KeyBinding(BringPreviousSelectedEventIntoViewCommand, new KeyGesture(Key.F3, ModifierKeys.None)));

            BringNextSelectedEventIntoViewCommand = InitializeCommand("Bring Next Selected Event Into View", "BringNextSelectedEventIntoViewCommand", BringNextSelectedEventIntoViewExecuted, BringNextSelectedEventIntoViewCanExecute);
            this.InputBindings.Add(new KeyBinding(BringNextSelectedEventIntoViewCommand, new KeyGesture(Key.F4, ModifierKeys.None)));

            UnapproveCommand = InitializeCommand("Unapprove", "UnapproveCommand", UnapproveExecuted, ApproveCanExecute);
            this.InputBindings.Add(new KeyBinding(UnapproveCommand, new KeyGesture(Key.F12, ModifierKeys.Shift)));

            ApproveCommand = InitializeCommand("Approve", "ApproveCommand", ApproveExecuted, ApproveCanExecute);
            this.InputBindings.Add(new KeyBinding(ApproveCommand, new KeyGesture(Key.F12, ModifierKeys.None)));

            ApproveAllPreviousCommand = InitializeCommand("Approve All Previous", "ApproveAllPreviousCommand", ApproveAllPreviousExecuted, ApproveAllPreviousCanExecute);
            this.InputBindings.Add(new KeyBinding(ApproveAllPreviousCommand, new KeyGesture(Key.F12, ModifierKeys.Control)));

            ApproveInViewCommand = InitializeCommand("Approve In View", "ApproveInViewCommand", ApproveInViewExecuted, ApproveInViewCanExecute);
            this.InputBindings.Add(new KeyBinding(ApproveInViewCommand, new KeyGesture(Key.F12, ModifierKeys.Alt)));

            AddFixtureCommand = InitializeCommand("Add Fixture", "AddFixtureCommand", AddFixtureExecuted, AddFixtureCanExecute);
//            this.InputBindings.Add(new KeyBinding(AddFixtureCommand, new KeyGesture(Key.D2, ModifierKeys.None)));

            ApplyFixtureCommand = InitializeCommand("Apply Fixture", "ApplyFixtureCommand", ApplyFixtureExecuted, ApplyFixtureCanExecute);
//            this.InputBindings.Add(new KeyBinding(ApplyFixtureCommand, new KeyGesture(Key.D3, ModifierKeys.None)));
        }

        void BringSelectedEventIntoViewExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (Analysis.Events.SelectedEvents.Count == 1)
                StyledEventsViewer.EventsViewer.BringEventIntoView(Analysis.Events.SelectedEvents[0]);
        }

        void CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
            e.Handled = true;
        }

        void SelectAllExecuted(object sender, ExecutedRoutedEventArgs e) {
            Analysis.Events.SelectAll();
        }

        public void RefreshExecuted(object sender, ExecutedRoutedEventArgs e) {
            Mouse.OverrideCursor = Cursors.Wait;
            Analysis.Events.UpdateSimilarCounts();
            Mouse.OverrideCursor = null;
        }

        void BringSelectedEventIntoViewCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (Analysis.Events.SelectedEvents.Count == 1);
            e.Handled = true;
        }

        void ClassifyCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            switch (ClassificationActionMode) {
                case TwClassificationActionMode.Selected:
                    e.CanExecute = Analysis.Events.SelectedEvents.Count > 0;
                    break;
                case TwClassificationActionMode.All:
                case TwClassificationActionMode.Forward:
                    e.CanExecute = true;
                    break;
            }

            e.Handled = true;
        }

        void ClassifyUsingMachineLearningExecuted(object sender, ExecutedRoutedEventArgs e) {
            ClassifierMachineLearning.Analysis = Analysis;
            switch (ClassificationActionMode) {
                case TwClassificationActionMode.Selected:
                    if (Analysis.Events.SelectedEvents.Count > 0)
                        Analysis.Events.MachineClassify(Analysis.Events.SelectedEvents, ClassifierMachineLearning, false, UndoPosition);
                    break;
                case TwClassificationActionMode.All:
                    Analysis.Events.MachineClassify(Analysis.Events, ClassifierMachineLearning, false, UndoPosition);
                    break;
                case TwClassificationActionMode.Forward:
                    Analysis.Events.MachineClassify(Analysis.Events.GetEventsForward(UndoPosition), ClassifierMachineLearning, false, UndoPosition);
                    break;
            }
        }

        void ClassifyUsingFixtureListExecuted(object sender, ExecutedRoutedEventArgs e) {
            switch (ClassificationActionMode) {
                case TwClassificationActionMode.Selected:
                    if (Analysis.Events.SelectedEvents.Count > 0)
                        Analysis.Events.MachineClassify(Analysis.Events.SelectedEvents, ClassifierComposite, false, UndoPosition);
                    break;
                case TwClassificationActionMode.All:
                    Analysis.Events.MachineClassify(Analysis.Events, ClassifierComposite, false, UndoPosition);
                    break;
                case TwClassificationActionMode.Forward:
                    Analysis.Events.MachineClassify(Analysis.Events.GetEventsForward(UndoPosition), ClassifierComposite, false, UndoPosition);
                    break;
            }
        }

        void MachineClassifyFirstCyclesExecuted(object sender, ExecutedRoutedEventArgs e) {
            Analysis.Events.MachineClassifyFirstCycles(false, Properties.Settings.Default.MinutesToStartFirstCycle);
        }

        void ManuallyClassifyFirstCyclesExecuted(object sender, ExecutedRoutedEventArgs e) {
            Analysis.Events.ManuallyToggleFirstCycle(Analysis.Events.SelectedEvents, false, null,UndoPosition);
        }

        void SetBookmarkExecuted(object sender, ExecutedRoutedEventArgs e) {
            StyledEventsViewer.EventsViewer.SetBookmark();
        }

        void GoToBookmarkExecuted(object sender, ExecutedRoutedEventArgs e) {
            StyledEventsViewer.EventsViewer.GoToBookmark();
        }

        void GoToBookmarkCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = StyledEventsViewer.EventsViewer.BookmarkIsSet();
            e.Handled = true;
        }

        TwCommand twCommandSelected;

        void InitializeTwCommandSelected() {
            var constraint = new TwCommandConstraintBool();
            constraint.Attribute = TwCommandConstraintAttribute.Selected;
            constraint.Value = true;

            twCommandSelected = new TwCommand();
            twCommandSelected.Constraints.Add(constraint);
        }
        
        void BringNextSelectedEventIntoViewCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            //bool canFind = StyledEventsViewer.EventsViewer.CanFindNext(twCommandSelected, SelectedEventView);
            //e.CanExecute = Analysis.Events.SelectedEvents.Count > 0 && canFind;
            e.CanExecute = Analysis.Events.SelectedEvents.Count > 0;
            e.Handled = true;
        }

        void BringPreviousSelectedEventIntoViewCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            //bool canFind = StyledEventsViewer.EventsViewer.CanFindPrevious(twCommandSelected, SelectedEventView);
            //e.CanExecute = Analysis.Events.SelectedEvents.Count > 0 && canFind;
            e.CanExecute = Analysis.Events.SelectedEvents.Count > 0;
            e.Handled = true;
        }

        void BringNextSelectedEventIntoViewExecuted(object sender, ExecutedRoutedEventArgs e) {
            SelectedEventView = StyledEventsViewer.EventsViewer.FindNext(twCommandSelected, SelectedEventView);
        }

        void BringPreviousSelectedEventIntoViewExecuted(object sender, ExecutedRoutedEventArgs e) {
            SelectedEventView = StyledEventsViewer.EventsViewer.FindPrevious(twCommandSelected, SelectedEventView);
        }

        void UnapproveExecuted(object sender, ExecutedRoutedEventArgs e) {
            Analysis.Events.Approve(Analysis.Events.SelectedEvents, UndoPosition, false);
        }

        void ApproveExecuted(object sender, ExecutedRoutedEventArgs e) {
            Analysis.Events.Approve(Analysis.Events.SelectedEvents, UndoPosition, true);
        }

        void ApproveCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Analysis.Events.SelectedEvents.Count > 0;
            e.Handled = true;
        }

        void ApproveAllPreviousExecuted(object sender, ExecutedRoutedEventArgs e) {
            Analysis.Events.ApproveAllPrevious(UndoPosition);
        }

        void ApproveAllPreviousCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
            e.Handled = true;
        }

        void ApproveInViewExecuted(object sender, ExecutedRoutedEventArgs e) {
            Analysis.Events.ApproveInView(StyledEventsViewer.EventsViewer.PercentCanvasWidth(), UndoPosition);
        }

        void ApproveInViewCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
            e.Handled = true;
        }

        void AddFixtureCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Analysis.Events.SelectedEvents.Count > 0;
            e.Handled = true;
        }

        void AddFixtureExecuted(object sender, ExecutedRoutedEventArgs e) {
            FixtureProfilesEditor.AddFixture(Analysis.Events.SelectedEvents);
        }

        void ApplyFixtureCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Analysis.Events.SelectedEvents.Count > 0;
            e.Handled = true;
        }

        void ApplyFixtureExecuted(object sender, ExecutedRoutedEventArgs e) {
            FixtureProfilesEditor.ApplyFixture(Analysis.Events.SelectedEvents);
        }

    }
}
