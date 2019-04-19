using System;
using System.Collections.Generic;
using System.Windows;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Threading;
using System.Text;
using System.Windows.Navigation;

using TraceWizard.Services;
using TraceWizard.Entities;
using TraceWizard.Adoption;
using TraceWizard.Classification;
using TraceWizard.Classification.Classifiers.Null;
using TraceWizard.Disaggregation;
using TraceWizard.Disaggregation.Disaggregators.Tw4;
using TraceWizard.Logging;
using TraceWizard.Logging.Adapters;
using TraceWizard.Entities.Adapters.Tw4Jet;
using TraceWizard.Entities.Adapters.Arff;
using TraceWizard.Entities.Adapters;
using TraceWizard.Environment;
using TraceWizard.FeatureLevels;
using TraceWizard.Notification;

using TraceWizard.Classification.Classifiers.J48;
using TraceWizard.Classification.Classifiers.OneRulePeak;
using TraceWizard.Classification.Classifiers.Leak;
using TraceWizard.Classification.Classifiers.FixtureList;

namespace TraceWizard.TwApp {
    public partial class MainTwWindow : Window {
        /*
         * This message will appear left of the version number in the menu bar. Set it = "" if you don't want to bake this into the build.
         */
        const string MenuBarNag = "Trace Wizard™";

        const string dirtyString = "*";
        DispatcherTimer dispatcherTimer;
        public FeatureLevel featureLevel;

        public bool IsFullScreened = false;

        public MainTwWindow() {

            InitializeFeatureLevel();
            InitializeCommands();
            InitializeComponent();
            InitializeToolBar();
            InitializeVersionDisplay();
            InitializeWindow();
            InitializeTab();
            InitializeTimer();

            this.Loaded += new RoutedEventHandler(window_Loaded);
            Application.Current.Exit += Application_Exit;
        }

        void window_Loaded(object sender, RoutedEventArgs e) {
            LoadWindowPosition();

            DispatchLoad(TwFile.GetCommandLineArgs());

            TwEnvironment.SetCurrentDirectoryToExecutableDirectory();

            KeyDown += new KeyEventHandler(window_KeyDown);
        }

        void window_KeyDown(object sender, KeyEventArgs e) {

            switch(e.Key) {
                case Key.System:
                    switch(e.SystemKey) {
                        case Key.R:
                            if (e.KeyboardDevice.Modifiers == ModifierKeys.Alt) {
                                MainToolBar.OpenContextMenu(MainToolBar.ButtonReports);
                                e.Handled = true;
                            }
                            break;
                        case Key.T:
                            if (e.KeyboardDevice.Modifiers == ModifierKeys.Alt) {
                                MainToolBar.OpenContextMenu(MainToolBar.ButtonTools);
                                e.Handled = true;
                            }
                            break;
                        case Key.H:
                            if (e.KeyboardDevice.Modifiers == ModifierKeys.Alt) {
                                MainToolBar.OpenContextMenu(MainToolBar.ButtonHelp);
                                e.Handled = true;
                            }
                            break;
                    }
                    break;
            }
        }

        public double GetViewportSeconds() {
            return EventsViewer.GetViewportSeconds(Properties.Settings.Default.ViewportSeconds);
        }

        public void DispatchLoad(string[] files) {
            var list = new List<string>();
            foreach(string file in files)
                list.Add(file);
            DispatchLoad(list);
        }

        void DispatchLoad(List<string> files) {

            var listNormalized = new List<string>();
            for (int i = 0; i < files.Count; i++) {
                string fileSpec = System.IO.Path.GetFileName(files[i]);
                string directory = System.IO.Path.GetDirectoryName(files[i]);
                if (System.IO.Directory.Exists(directory)) {
                    string[] filesDirectory = System.IO.Directory.GetFiles(directory, fileSpec);
                    foreach (string file in filesDirectory)
                        listNormalized.Add(file);
                }
            }

            foreach (string fileName in listNormalized) {
                if (TwServices.IsLog(fileName)) {
                    AddTabLoadLog(fileName);
                } else if (TwServices.IsAnalysis(fileName)) {
                    AddTabLoadAnalysis(fileName);
                }
            }
        }

        void InitializeFeatureLevel() {
            featureLevel = new FeatureLevel();
            featureLevel.Initialize();
        }

        void SaveWindowPosition() {
//            Properties.Settings.Default.WindowState = WindowState;
        }

        void Application_Exit(object sender, ExitEventArgs e) {
            SaveWindowPosition();

            Properties.Settings.Default.Save();
            TwFile.DeleteExtractedDirectory();
        }

        private void InitializeTimer() {
            if (Properties.Settings.Default.MinutesBetweenBackgroundSaves > 0) {
                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, Properties.Settings.Default.MinutesBetweenBackgroundSaves, 0);
                dispatcherTimer.Start();
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            BackupSave();
        }

        private void BackupSave() {
            foreach (TabItem tabItem in TabControl.Items) {
                Events events = EventsFromTabItem(tabItem);
                if (events != null && events.IsBackupDirty)
                    BackupSave(tabItem);
            }
        }

        void BackupSave(TabItem tabItem) {
            AnalysisPanel analysisPanel = tabItem.Content as AnalysisPanel;
            if (analysisPanel == null)
                return;

            Analysis analysis = analysisPanel.Analysis;
            AnalysisDatabase analysisDatabase = analysis as AnalysisDatabase;
            if (analysisDatabase == null)
                return;

            BackupSave(analysisDatabase);
        }

        void BackupSave(AnalysisDatabase analysis) {

//            string arffFile = TwFile.GetBackupArffFileToSave(analysis.DataSourceDirectoryName, analysis.KeyCode);
            string arffFile = TwFile.GetBackupArffFileToSave(".", analysis.KeyCode);
            if (arffFile == null || arffFile.Length == 0)
                return;

            var analysisAdapter = new ArffAnalysisAdapter();
            analysisAdapter.Save(arffFile, analysis,true);
            analysis.Events.IsBackupDirty = false;
        }

        void Exit() {
            Application.Current.Shutdown((int)TraceWizardApp.TwShutdownCode.VersionExpired);
        }

        public static RoutedUICommand EnterFullScreenCommand;
        public static RoutedUICommand ExitFullScreenCommand;
        public static RoutedUICommand ExportLogToTwdbCommand;
        public static RoutedUICommand ExportTdbToTwdbCommand;
        public static RoutedUICommand ExportMdbToCsvCommand;
        public static RoutedUICommand SaveAsCommand;
        public static RoutedUICommand ApplyConversionFactorCommand;
        public static RoutedUICommand ChangeStartTimeCommand;
        public static RoutedUICommand PreferencesHelpCommand;
        public static RoutedUICommand ShortcutsHelpCommand;
        public static RoutedUICommand CommandsHelpCommand;
        public static RoutedUICommand CommandLineHelpCommand;
        public static RoutedUICommand PreferencesCommand;
        public static RoutedUICommand AboutCommand;
        public static RoutedUICommand ReleaseNotesCommand;
        public static RoutedUICommand InstallationNotesCommand;
        public static RoutedUICommand TipsCommand;
        public static RoutedUICommand CompareAnalysesCommand;
        public static RoutedUICommand OpenRecoverCommand;
        public static RoutedUICommand RecoverTraceCommand;
        public static RoutedUICommand ConfusionMatrixCommand;
        public static RoutedUICommand ExportToWekaCommand;
        public static RoutedUICommand ExportToExemplarsCommand;
        public static RoutedUICommand HourlyReportCommand;
        public static RoutedUICommand DistributionReportCommand;
        public static RoutedUICommand FixtureSummaryByEventsReportCommand;
        public static RoutedUICommand FixtureSummaryByVolumeReportCommand;
        public static RoutedUICommand EventsDatabaseReportCommand;
        public static RoutedUICommand ProjectReportCommand;
        public static RoutedUICommand AggregateReportCommand;
        public static RoutedUICommand VerifyLogReportCommand;

        RoutedUICommand InitializeCommand(string commandText, string commandName, ExecutedRoutedEventHandler handlerExecute, CanExecuteRoutedEventHandler handlerCanExecute) {
            var command = new RoutedUICommand(commandText, commandName, typeof(MainTwWindow));
            CommandBinding commandBinding = new CommandBinding(command, handlerExecute, handlerCanExecute);
            CommandBindings.Add(commandBinding);
            return command;
        }

        RoutedUICommand InitializeCommand(string commandText, string commandName, ExecutedRoutedEventHandler handlerExecute) {
            return InitializeCommand(commandText, commandName, handlerExecute, CanExecute);
        }

        void InitializeCommands() {
            EnterFullScreenCommand = InitializeCommand("EnterFullScreen", "EnterFullScreenCommand", EnterFullScreenExecuted);
            this.InputBindings.Add(new KeyBinding(EnterFullScreenCommand, new KeyGesture(Key.F11)));

            ExitFullScreenCommand = InitializeCommand("ExitFullScreen", "ExitFullScreenCommand", ExitFullScreenExecuted);
            this.InputBindings.Add(new KeyBinding(ExitFullScreenCommand, new KeyGesture(Key.Escape)));

            SaveAsCommand = InitializeCommand("SaveAs", "SaveAsCommand", SaveAsExecuted, SaveAsCanExecute);
            this.InputBindings.Add(new KeyBinding(SaveAsCommand, new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Alt)));

            this.InputBindings.Add(new KeyBinding(ApplicationCommands.NotACommand, new KeyGesture(Key.S, ModifierKeys.Control)));
            this.InputBindings.Add(new KeyBinding(ApplicationCommands.Save, new KeyGesture(Key.S, ModifierKeys.Control)));

            ApplyConversionFactorCommand = InitializeCommand("ApplyConversionFactor", "ApplyConversionFactorCommand", ApplyConversionFactorExecuted);
            this.InputBindings.Add(new KeyBinding(ApplyConversionFactorCommand, new KeyGesture(Key.A, ModifierKeys.Control | ModifierKeys.Shift)));

            ChangeStartTimeCommand = InitializeCommand("ChangeStartTime", "ChangeStartTimeCommand", ChangeStartTimeExecuted);
            this.InputBindings.Add(new KeyBinding(ChangeStartTimeCommand, new KeyGesture(Key.T, ModifierKeys.Control)));

            ShortcutsHelpCommand = InitializeCommand("ShortcutsHelp", "ShortcutsHelpCommand", ShortcutsHelpExecuted);

            CommandsHelpCommand = InitializeCommand("CommandsHelp", "CommandsHelpCommand", CommandsHelpExecuted);

            CommandLineHelpCommand = InitializeCommand("CommandLineHelp", "CommandLineHelpCommand", CommandLineHelpExecuted);
                        
            ReleaseNotesCommand = InitializeCommand("ReleaseNotes", "ReleaseNotesCommand", ReleaseNotesExecuted,ReleaseNotesCanExecute);

            InstallationNotesCommand = InitializeCommand("InstallationNotes", "InstallationNotesCommand", InstallationNotesExecuted,InstallationNotesCanExecute);

            TipsCommand = InitializeCommand("Tips", "TipsCommand", TipsExecuted, ReleaseNotesCanExecute);

            PreferencesCommand = InitializeCommand("Preferences", "PreferencesCommand", PreferencesExecuted);
            this.InputBindings.Add(new KeyBinding(PreferencesCommand, new KeyGesture(Key.F4, ModifierKeys.Shift)));
            
            AboutCommand = InitializeCommand("About", "AboutCommand", AboutExecuted);
            
            CompareAnalysesCommand = InitializeCommand("CompareAnalyses", "CompareAnalysesCommand", CompareAnalysesExecuted); 
            this.InputBindings.Add(new KeyBinding(CompareAnalysesCommand, new KeyGesture(Key.O, ModifierKeys.Control | ModifierKeys.Alt)));

            OpenRecoverCommand = InitializeCommand("Open Recover", "OpenRecoverCommand", OpenRecoverExecuted);

            ConfusionMatrixCommand = InitializeCommand("ConfusionMatrix", "ConfusionMatrixCommand", ConfusionMatrixExecuted);
            
            ExportLogToTwdbCommand = InitializeCommand("ExportLogToTwdb", "ExportLogToTwdbCommand", ExportLogToTwdbExecuted);
            
            ExportTdbToTwdbCommand = InitializeCommand("ExportTdbToTwdb", "ExportTdbToTwdbCommand", ExportTdbToTwdbExecuted);
            
            ExportMdbToCsvCommand = InitializeCommand("ExportMdbToCsv", "ExportMdbToCsvCommand", ExportMdbToCsvExecuted);
            
            ExportToWekaCommand = InitializeCommand("ExportToWeka", "ExportToWekaCommand", ExportToWekaExecuted);
            
            ExportToExemplarsCommand = InitializeCommand("ExportToExemplars", "ExportToExemplarsCommand", ExportToExemplarsExecuted);
            
            HourlyReportCommand = InitializeCommand("HourlyReport", "HourlyReportCommand", HourlyReportExecuted);
            
            FixtureSummaryByEventsReportCommand = InitializeCommand("FixtureSummaryByEventsReport", "FixtureSummaryByEventsReportCommand", FixtureSummaryByEventsReportExecuted);
            
            FixtureSummaryByVolumeReportCommand = InitializeCommand("FixtureSummaryByVolumeReport", "FixtureSummaryByVolumeReportCommand", FixtureSummaryByVolumeReportExecuted);
            
            DistributionReportCommand = InitializeCommand("DistributionReport", "DistributionCommand", DistributionReportExecuted);
            
            AggregateReportCommand = InitializeCommand("Aggregate", "Aggregate", AggregateReportExecuted);
            
            EventsDatabaseReportCommand = InitializeCommand("EventsDatabaseReport", "EventsDatabaseReportCommand", ExportToEventsDatabaseExecuted);
            
            ProjectReportCommand = InitializeCommand("ProjectReport", "ProjectReportCommand", ExportToProjectReportExecuted);

            VerifyLogReportCommand = InitializeCommand("VerifyLogReport", "VerifyLogReportCommand", VerifyLogReportExecuted);

        }

        public delegate void Dispatch(string[] files);
        
        void InitializeToolBar() {
            MainToolBar.FeatureLevel = featureLevel;
            MainToolBar.DispatchLoad = DispatchLoad;
            MainToolBar.Initialize();
        }

        void InitializeVersionDisplay() {
            LabelVersion.Content =  (MenuBarNag != null ? MenuBarNag + " | " : "") + "Ver " + TwAssembly.CompleteVersion() + " " + featureLevel.Text;
            LabelVersion.ToolTip = "Trace Wizard version: " + TwAssembly.CompleteVersion() + "\r\n" + "Feature level: " + featureLevel.Text;
        }
    
        void InitializeWindow() {
            Width = SystemParameters.WorkArea.Width - 96;
            Height = SystemParameters.WorkArea.Height - 96;
            Left = (SystemParameters.WorkArea.Width - Width) / 2 + SystemParameters.WorkArea.Left;
            Top = (SystemParameters.WorkArea.Height - Height) / 2 + SystemParameters.WorkArea.Top;
            Title = TwAssembly.CompanyAndTitle();
        }

        protected override void OnClosing(CancelEventArgs e) {
            try {
                foreach (TabItem tabItem in TabControl.Items) {
                    Events events = EventsFromTabItem(tabItem);
                    if (events != null && events.IsDirty) {
                        if (ShouldCancelClose(string.Empty)) {
                            e.Cancel = true;
                        }
                        return;
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show("Trace Wizard error during OnClosing: " + ex.Message);
            }
        }

        void InitializeTab() {
            this.AddHandler(CloseableTabItem.CloseTabEvent, new RoutedEventHandler(this.tabItem_Close));
        }

        void LoadWindowPosition() {
//            WindowState = Properties.Settings.Default.WindowState;
        } 
       
        Visibility BoolToVisibility(bool full) {
            return full ? Visibility.Collapsed : Visibility.Visible;
        }

        WindowState BoolToWindowState(bool full) {
            if (full) {
                WindowStateSaved = WindowState;
                return WindowState.Maximized;
            } else {
                return WindowStateSaved;
            }
        }

        WindowState WindowStateSaved = WindowState.Maximized;
        
        void DoFullScreen(bool full, AnalysisPanel analysisPanel) {
            analysisPanel.StyledEventsViewer.TimeFramePanel.Visibility = BoolToVisibility(full);
            analysisPanel.StyledEventsViewer.dockFixturesRuler.Visibility = BoolToVisibility(full);
            analysisPanel.StyledEventsViewer.dockApprovalRuler.Visibility = BoolToVisibility(full);
            analysisPanel.StyledEventsViewer.dockClassificationRuler.Visibility = BoolToVisibility(full);
            analysisPanel.StyledEventsViewer.dockSelectionRuler.Visibility = BoolToVisibility(full);
            analysisPanel.dockGraphToolBar.Visibility = BoolToVisibility(full);
            analysisPanel.dockCommandPanel.Visibility = BoolToVisibility(full);
            analysisPanel.dockSummaryPanel.Visibility = BoolToVisibility(full);
            analysisPanel.StatusBar.Visibility = BoolToVisibility(full);
        }   
        
        void DoFullScreen(bool enterFullScreen) {

            if (enterFullScreen)
                Title = TwAssembly.CompanyAndTitle() + " - " + "Press Escape key to exit Full Screen mode";
            else
                Title = TwAssembly.CompanyAndTitle();
            
            MainToolBar.Visibility = BoolToVisibility(enterFullScreen);

            foreach (TabItem tabItem in TabControl.Items) {
                var analysisPanel = tabItem.Content as AnalysisPanel;

                if (analysisPanel != null)
                    DoFullScreen(enterFullScreen, analysisPanel);
            }
            WindowState = BoolToWindowState(enterFullScreen);
        }

        void ShowMessageBox(string message) {
            MessageBox.Show(message, TwAssembly.Title());
            Mouse.OverrideCursor = null;
        }

        private void InitProgressWindow(Traces traces) {
            ProgressWindow progressWindow = new ProgressWindow(traces);
            progressWindow.Topmost = false;
            progressWindow.Owner = this;
            progressWindow.ShowInTaskbar = false;
            progressWindow.ShowDialog();
        }

        void ExecuteConfusionMatrix(List<Classifier> classifiers, Adopter adopter) {
            List<string> files = TwFile.GetAnalysisFilesIncludingZipped();
            if (files.Count > 0) {
                foreach (var classifier in classifiers) {
                    var panel = LoadConfusionMatrix(files, classifier, adopter);
                    AddTab(CreateTabItemHeader("Confusion Matrix: " + classifier.Name,
                        TwGui.GetImage("confusionmatrix.png")), panel, null,true);
                }
            }
        }

        UIElement LoadConfusionMatrix(List<string> analysisFilenames, Classifier classifier, Adopter adopter) {
            Traces traces = new Traces(analysisFilenames,classifier, adopter);
            InitProgressWindow(traces);

            ConfusionMatrixAggregatePanel confusionMatrixAggregatePanel = new ConfusionMatrixAggregatePanel();

            if (traces.Count > 0)
                confusionMatrixAggregatePanel.Load(traces);

            return confusionMatrixAggregatePanel;
        }

        void tabItem_Close(object source, RoutedEventArgs args)
        {
            TabItem tabItem = args.Source as TabItem;
            if (tabItem != null) {
                Events events = EventsFromTabItem(tabItem);

                if (events != null) {
                    if (!events.IsDirty || !ShouldCancelClose(KeyCodeFromTabItem(tabItem))) {
                        CloseTab(tabItem);
                    }
                } else {
                    CloseTab(tabItem);
                }
            }
        }

        void CloseTab(TabItem tabItem) {
            TabControl tabControl = tabItem.Parent as TabControl;
            if (tabControl != null)
                tabControl.Items.Remove(tabItem);
        }

        bool ShouldCancelClose(string keyCode) {
            string message = "You have made unsaved changes";
            if (!string.IsNullOrEmpty(keyCode))            
                message += " to " + keyCode;
            message += ".\r\n\r\n To close and lose these changes, press OK.\r\n\r\n To cancel the close, press Cancel.";
                MessageBoxResult messageBoxResult = MessageBox.Show(
                    message,
                    TwAssembly.Title(),
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning,
                    MessageBoxResult.Cancel);
                if (messageBoxResult == MessageBoxResult.Cancel)
                    return true;
            return false;
        }

        void tabItem_GotFocus(object sender, RoutedEventArgs e) {
            TabItem tabItem = sender as TabItem;

            var scrollViewer = tabItem.Tag as ScrollViewer;

            if (scrollViewer != null)
                scrollViewer.Focus();
        }

        void AddTab(UIElement header, UIElement content, UIElement focus, bool padding) {
            CloseableTabItem tabItem = new CloseableTabItem();

            tabItem.SnapsToDevicePixels = true;
            tabItem.Margin = new Thickness(0);
            tabItem.Padding = new Thickness(0);
//            tabItem.Height = 19;
            tabItem.IsTabStop = false;
            tabItem.Header = header;

            tabItem.Content = content;
            var contentControl = content as Control;
            if (contentControl != null && padding)
                contentControl.Padding = new Thickness(20);

            if (focus != null)
                tabItem.Tag = focus;

            TabControl.Items.Add(tabItem);

            tabItem.IsSelected = true;
        }

        UIElement CreateTabItemHeader(string text, ImageSource imageSource) {
            return CreateTabItemHeader(text, imageSource, null);
        }

        UIElement CreateTabItemHeader(AnalysisPanel analysisPanel, string fileNameIn) {
            string fileName = string.IsNullOrEmpty(analysisPanel.SaveFileName) ? fileNameIn : analysisPanel.SaveFileName;
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Margin = new Thickness(2,1,0,0);

            Image image = new Image();
            image.Style = (Style)FindResource("ImageStyle");

            image.Source = TwGui.GetImage("trace.png");
            image.Margin = new Thickness(0);
            stackPanel.Children.Add(image);

            TextBlock textBlock = new TextBlock();
            textBlock.Margin = new Thickness(4, 0, 0, 0);
            textBlock.Text = System.IO.Path.GetFileNameWithoutExtension(fileName);
            stackPanel.Children.Add(textBlock);

            stackPanel.ToolTip = fileName;
            
            return stackPanel;
        }

        UIElement CreateTabItemHeader(string text, ImageSource imageSource, object toolTip) {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Margin = new Thickness(2,1,0,0);

            Image image = new Image();
            image.Style = (Style)FindResource("ImageStyle");

            image.Source = imageSource;
            image.Margin = new Thickness(0);
            stackPanel.Children.Add(image);

            TextBlock textBlock = new TextBlock();
            textBlock.Margin = new Thickness(4, 0, 0, 0);
            textBlock.Text = text;
            stackPanel.Children.Add(textBlock);

            stackPanel.ToolTip = toolTip;
            
            return stackPanel;
        }

        TabItem TabItemFromEvents(Events events) {
            foreach (TabItem tabItem in TabControl.Items) {
                Events eventsTabItem = EventsFromTabItem(tabItem);
                if (eventsTabItem != null && eventsTabItem == events)
                    return tabItem;
            }
            return null;
        }

        Events EventsFromTabItem(TabItem tabItem) {
            ScrollViewer scrollviewer = tabItem.Tag as ScrollViewer;
            if (scrollviewer != null) {
                var eventsCanvas = scrollviewer.Content as EventsCanvas;

                if (eventsCanvas != null)
                    return eventsCanvas.Events;
            }
            return null;
        }

        string KeyCodeFromTabItem(TabItem tabItem) {
            string keyCode = string.Empty;
            
            StackPanel stackPanel = tabItem.Header as StackPanel;
            if (stackPanel == null || stackPanel.Children.Count < 2)
                return keyCode;
            
            TextBlock textBlock = stackPanel.Children[1] as TextBlock;
            if (textBlock == null)
                return keyCode;

            if (textBlock.Text.EndsWith(dirtyString)) {
                string keyCodeDirty = textBlock.Text;
                keyCode = keyCodeDirty.Remove(textBlock.Text.Length - dirtyString.Length);
            }

            return keyCode;
        }

        public void analysisPanel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case TwNotificationProperty.OnChangeFileName:
                    var analysisPanel = sender as AnalysisPanel;
                    if (analysisPanel != null) {
                        TabItem tabItem = analysisPanel.Parent as TabItem;
                        if (tabItem != null)
                            tabItem.Header = CreateTabItemHeader(analysisPanel, analysisPanel.SaveFileName);
                    }
                    break;
            }
        }

        public void events_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case TwNotificationProperty.OnIsDirtyChanged:
                    Events events = sender as Events;
                    if (events == null)
                        return;
                    TabItem tabItem = TabItemFromEvents(events);

                    if (tabItem != null)
                        ShowTabDirty(tabItem, events.IsDirty);

                    break;
            }
        }

        void ShowTabDirty(TabItem tabItem, bool dirty) {
            StackPanel stackpanel = tabItem.Header as StackPanel;

            if (stackpanel == null || stackpanel.Children.Count < 2)
                return;
            TextBlock textBlock = stackpanel.Children[1] as TextBlock;
            if (textBlock == null)
                return;

            ShowTextDirty(textBlock, dirty);
        }

        void ShowTextDirty(TextBlock textBlock, bool dirty) {
            if (dirty && !textBlock.Text.EndsWith(dirtyString))
                textBlock.Text += dirtyString;
            else if (!dirty && textBlock.Text.EndsWith(dirtyString))
                textBlock.Text = textBlock.Text.Remove(textBlock.Text.Length - dirtyString.Length);
        }

        void AddTabLoadLog(string fileName) {
            if (featureLevel.IsDemo) 
                return;
            var analysisPanel = LoadLogAndClassify(fileName, false);
            AddTab(analysisPanel, fileName);
        }

        void AddTabLoadAnalysis(string fileName) {
            var analysisPanel = LoadAnalysis(fileName, false);
            AddTab(analysisPanel, fileName);
        }

        public void AddTab(AnalysisPanel analysisPanel, string fileName) {
            analysisPanel.PropertyChanged += new PropertyChangedEventHandler(analysisPanel_PropertyChanged);
            analysisPanel.Analysis.Events.PropertyChanged += new PropertyChangedEventHandler(events_PropertyChanged);
            analysisPanel.Analysis.Events.IsDirty = false;
            AddTab(CreateTabItemHeader(analysisPanel, fileName), analysisPanel, analysisPanel.StyledEventsViewer.EventsViewer.ScrollViewer,false);
        }

        public void AddTab(UIElement content, string iconFile, string keyCode) {
            AddTab(CreateTabItemHeader(keyCode, TwGui.GetImage(iconFile)), content, content,true);
        }

        AnalysisPanel LoadLogAndClassify(string fileName, bool showAnalyzer) {
            var classifier = new NullClassifier();
            var disaggregator = new Tw4PostTrickleMergeMidnightSplitDisaggregator();
            
            Mouse.OverrideCursor = Cursors.Wait;

            Log log = TwServices.CreateLog(fileName);

            disaggregator.Log = log;
            Events events = disaggregator.Disaggregate();
            events.UpdateLinkedList();

            var analysis = new AnalysisDatabase(fileName, events, log);
            classifier.Classify(analysis);

            analysis.UpdateFixtureSummaries();

            var analysisPanel = CreateAnalysisPanel(analysis, fileName, disaggregator, showAnalyzer, GetViewportSeconds(), EventsViewer.VolumeTen);

            Mouse.OverrideCursor = null;
            return analysisPanel;
        }

        AnalysisPanel LoadAnalysis(string fileName, bool showAnalyst) {
            Mouse.OverrideCursor = Cursors.Wait;

            Analysis analysis = TwServices.CreateAnalysis(fileName);

            analysis.UpdateFixtureSummaries();

            var analysisPanel = CreateAnalysisPanel(analysis, fileName, showAnalyst);
            analysisPanel.SaveFileName = fileName;
            
            Mouse.OverrideCursor = null;

            return analysisPanel;
        }

        AnalysisPanel CreateAnalysisPanel(Analysis analysis, string fileName, Disaggregator disaggregator, bool showAnalyzer,
            double viewportSeconds, double viewportVolume) {
            var toolTip = TwGui.CreateAnalyzerToolTip(disaggregator.GetType(),disaggregator);
            return CreateAnalysisPanel(analysis, fileName, showAnalyzer ? disaggregator.Name : null, toolTip);
        }

        public AnalysisPanel CreateAnalysisPanel(Analysis analysis, string fileName, bool showAnalyst) {
            return CreateAnalysisPanel(analysis, fileName, showAnalyst ? "Classifier: Analyst" : null, null);
        }

        AnalysisPanel CreateAnalysisPanel(Analysis analysis, string fileName, string analyzer, UIElement analyzerToolTip) {
            var analysisPanel = new AnalysisPanel();
            analysisPanel.ViewportSeconds = GetViewportSeconds();
            analysisPanel.ViewportVolume = EventsViewer.VolumeTen;
            analysisPanel.Analysis = analysis;
            analysisPanel.FeatureLevel = featureLevel;
            analysisPanel.Initialize();

            return analysisPanel;
        }

        void NewExecuted(object sender, ExecutedRoutedEventArgs e) {
            ExecuteNewLog();
        }
        void OpenExecuted(object sender, ExecutedRoutedEventArgs e) {
            ExecuteOpen();
        }
        void PrintExecuted(object sender, ExecutedRoutedEventArgs e) {
            ExecutePrint();
        }
        void SaveExecuted(object sender, ExecutedRoutedEventArgs e) {
            ExecuteSave();
        }
        void ShortcutsHelpExecuted(object sender, ExecutedRoutedEventArgs e) {
            ShortcutsHelpExecuted();
        }

        void ChangeStartTimeExecuted(object sender, ExecutedRoutedEventArgs e) {
            var analysis =(GetCurrentAnalysis());
            var window = new DateTimePickerWindow(analysis.Events.StartTime);
            window.Owner = this;
            window.ShowDialog();

            if (window.DateTime != DateTime.MinValue && window.DateTime != analysis.Events.StartTime) {
                analysis.Events.ChangeStartTime(window.DateTime);
            }
        }

        void ApplyConversionFactorExecuted(object sender, ExecutedRoutedEventArgs e) {
            var analysis = (GetCurrentAnalysis());
            double oldConversionFactor = analysis.Events.ConversionFactor;
            var window = new ConversionFactorWindow(oldConversionFactor);
            window.Owner = this;
            window.ShowDialog();

            if (window.ConversionFactor != oldConversionFactor) {
                analysis.Events.ApplyConversionFactor(oldConversionFactor, window.ConversionFactor);
            }
        }

        static void ShortcutsHelpExecuted() { TwFile.Launch(TwEnvironment.TwHelpShortcuts); }
        static public void ReleaseNotesExecuted(object sender, ExecutedRoutedEventArgs e) {
//            throw new Exception("Testing simplified bug reporter");}
            TwFile.Launch(TwEnvironment.TwReleaseNotes); }
        static public void InstallationNotesExecuted(object sender, ExecutedRoutedEventArgs e) { TwFile.Launch(TwEnvironment.TwReadMe); }
        static public void CommandLineHelpExecuted(object sender, ExecutedRoutedEventArgs e) { TwFile.Launch(TwEnvironment.TwHelpCommandLine); }
        static public void CommandsHelpExecuted(object sender, ExecutedRoutedEventArgs e) { TwFile.Launch(TwEnvironment.TwHelpCommands); }

        static public void TipsExecuted(object sender, ExecutedRoutedEventArgs e) {
            var tipsVersion = e.Parameter;
            TwFile.Launch(TwEnvironment.TwTipsBase + tipsVersion + "." + TwEnvironment.TwTipsExtension);
        }

        public void ExecuteSave() {
            var analysisPanel = GetCurrentAnalysisPanel();
            if (analysisPanel != null) {
                if (string.IsNullOrEmpty(analysisPanel.SaveFileName))
                    SaveAsExecuted(null, null);
                else
                    ExecuteSave(analysisPanel.Analysis, analysisPanel.SaveFileName);
            }
        }

        void SaveAsExecuted(object sender, ExecutedRoutedEventArgs e) {
            var analysisPanel = GetCurrentAnalysisPanel();
            if (analysisPanel != null)
                analysisPanel.SaveFileName = ExecuteSaveAs(analysisPanel.Analysis, 
                    string.IsNullOrEmpty(analysisPanel.SaveFileName) ? 
                    analysisPanel.Analysis.KeyCode 
                    : System.IO.Path.ChangeExtension(analysisPanel.SaveFileName,TwEnvironment.ArffAnalysisExtension));
        }

        void EnterFullScreenExecuted(object sender, ExecutedRoutedEventArgs e) {
            DoFullScreen(!IsFullScreened);
            IsFullScreened = !IsFullScreened;
        }

        void ExitFullScreenExecuted(object sender, ExecutedRoutedEventArgs e) {
            DoFullScreen(false);
            IsFullScreened = false;
        }

        Analysis GetCurrentAnalysis() {
            AnalysisPanel analysisPanel = GetCurrentAnalysisPanel();
            if (analysisPanel != null)
                return analysisPanel.Analysis;
            else
                return null;
        }

        public AnalysisPanel GetCurrentAnalysisPanel() {
            TabItem tabItem = null;
            foreach (TabItem item in TabControl.Items)
                if (item.IsSelected == true) {
                    tabItem = item;
                    break;
                }

            if (tabItem == null)
                return null;

            AnalysisPanel analysisPanel = tabItem.Content as AnalysisPanel;
            return analysisPanel;
        }

        void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            bool canExecute = false;

            var analysisPanel = GetCurrentAnalysisPanel();
            if (featureLevel.IsPro && analysisPanel != null 
                && (analysisPanel.Analysis.Events.IsDirty || string.IsNullOrEmpty(analysisPanel.SaveFileName)))
                canExecute = true;

            e.CanExecute = canExecute;
            e.Handled = true;
        }

        void SaveAsCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            bool canExecute = false;

            var analysisPanel = GetCurrentAnalysisPanel();
            if (featureLevel.IsPro && analysisPanel != null)
                canExecute = true;

            e.CanExecute = canExecute;
            e.Handled = true;
        }

        void ReleaseNotesCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = featureLevel.IsPro ? true : false;
            e.Handled = true;
        }

        void InstallationNotesCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
            e.Handled = true;
        }

        void CompareAnalysesExecuted(object sender, ExecutedRoutedEventArgs e) {
            string fileNameAnalysisLower = TwFile.GetAnalysisFile();
            if (fileNameAnalysisLower == null || fileNameAnalysisLower == string.Empty) return;

            string fileNameAnalysisUpper = TwFile.GetAnalysisFile();
            if (fileNameAnalysisUpper == null || fileNameAnalysisUpper == string.Empty) return;

            UpdateAnalysisFolder(fileNameAnalysisUpper);

            Mouse.OverrideCursor = Cursors.Wait;

            var analysisLower = TwServices.CreateAnalysis(fileNameAnalysisLower);
            var analysisUpper = TwServices.CreateAnalysis(fileNameAnalysisUpper);

            var compareAnalysesPanel = new CompareAnalysesPanel();
            compareAnalysesPanel.AnalysisLower = analysisLower;
            compareAnalysesPanel.AnalysisUpper = analysisUpper;
            compareAnalysesPanel.Initialize();

            AddTab(CreateTabItemHeader("Compare", TwGui.GetImage("traces2.png"), CreateCompareAnalysesHeaderToolTip(fileNameAnalysisUpper, fileNameAnalysisLower)), compareAnalysesPanel, compareAnalysesPanel.StyledEventsViewerUpper.EventsViewer.ScrollViewer,false);

            Mouse.OverrideCursor = null;
        }

        void ConfusionMatrixExecuted(object sender, ExecutedRoutedEventArgs e) {
            var window = new ClassifierSelector();
            window.Owner = this;
            window.ShowDialog();

            if (window.Classifiers != null && window.Classifiers.Count > 0)
                ExecuteConfusionMatrix(window.Classifiers, TwAdopters.Instance.GetDefaultAdopter());
        }

        void ExportLogToTwdbExecuted(object sender, ExecutedRoutedEventArgs e) {
            var files = TwFile.GetLogFilesIncludingZipped(Properties.Settings.Default.DirectoryLog);

            if (files != null && files.Count > 0) {
                var list = new List<string>();
                for (int i = 0; i < files.Count; i++)
                    list.Add(files[i]);
                var countFilesSaved = TwServices.ExportLogToTwdb(list, new Tw4PostTrickleMergeMidnightSplitDisaggregator());
                MessageBox.Show(countFilesSaved + " TWDB file(s) were created.", TwAssembly.TitleTraceWizard());
            }
        }

        void ExportTdbToTwdbExecuted(object sender, ExecutedRoutedEventArgs e) {
            var files = TwFile.GetTdbAnalysisFiles(Properties.Settings.Default.DirectoryAnalysis);

            if (files != null && files.Length > 0) {
                var list = new List<string>();
                for (int i = 0; i < files.Length; i++)
                    list.Add(files[i]);
                var countFilesSaved = TwServices.ExportTdbToTwdb(list);
                MessageBox.Show(countFilesSaved + " TWDB file(s) were created.", TwAssembly.TitleTraceWizard());
            }
        }

        void ExportMdbToCsvExecuted(object sender, ExecutedRoutedEventArgs e) {
            var files = TwFile.GetMdbLogFiles(Properties.Settings.Default.DirectoryLog);

            if (files != null && files.Length > 0) {
                var list = new List<string>();
                for (int i = 0; i < files.Length; i++)
                    list.Add(files[i]);
                var countFilesSaved = TwServices.ExportMdbToCsv(list);
                MessageBox.Show(countFilesSaved + " CSV file(s) were created.", TwAssembly.TitleTraceWizard());
            }
        }

        void ExportToWekaExecuted(object sender, ExecutedRoutedEventArgs e) {
            (new WekaExporter()).Export();
        }

        void ExportToExemplarsExecuted(object sender, ExecutedRoutedEventArgs e) {
            (new ExemplarsExporter()).Export();
        }

        void HourlyReportExecuted(object sender, ExecutedRoutedEventArgs e) {
            var reporter = new HourlyReporter();
            var reports = reporter.Report();
            if (reports != null && reports.Count > 0)
                foreach (var report in reports)
                    AddTab(report, "stats.png", "Daily and Hourly - " + ((HourlyReportPanel)report).Analysis.KeyCode);
                //foreach (var file in reporter.files) 
                //    TwFile.Launch(file);
        }

        void FixtureSummaryReport(bool byInstances) {
            var reporter = new FixtureSummaryReporter();
            reporter.ByInstances = byInstances;
            var reports = reporter.Report();
            if (reports != null && reports.Count > 0)
                foreach (var report in reports)
                    AddTab(report, "stats.png", "Fixture Summary - " + 
                        (byInstances ? " By Events" : " By Volume") + " - " + ((FixtureSummaryReportPanel)report).Analysis.KeyCode);
        }

        void FixtureSummaryByEventsReportExecuted(object sender, ExecutedRoutedEventArgs e) {
            FixtureSummaryReport(true);
        }

        void FixtureSummaryByVolumeReportExecuted(object sender, ExecutedRoutedEventArgs e) {
            FixtureSummaryReport(false);
        }

        void DistributionReportExecuted(object sender, ExecutedRoutedEventArgs e) {
            var reports = new DistributionReporter().Report();
            if (reports != null && reports.Count > 0)
                foreach (var report in reports)
                    AddTab(report, "stats.png", "Distribution - " + ((DistributionReportPanel)report).Analysis.KeyCode);
        }

        void VerifyLogReportExecuted(object sender, ExecutedRoutedEventArgs e) {
            var report = new VerifyLogReporter().Report();
            if (report != null)
                AddTab(report, "stats.png", "Log Integrity");
        }

        void AggregateReportExecuted(object sender, ExecutedRoutedEventArgs e) {
            var report = new AggregateReporter().Report();
            if (report != null)
                AddTab(report, "stats.png", "Aggregate");
        }

        void ExportToEventsDatabaseExecuted(object sender, ExecutedRoutedEventArgs e) {
            (new EventsDatabaseExporter()).Export();
        }

        void ExportToProjectReportExecuted(object sender, ExecutedRoutedEventArgs e) {
            var report = new ProjectReportReporter().Report();
            if (report != null)
                AddTab(report, "stats.png", "Project");
        }

        void PreferencesExecuted(object sender, ExecutedRoutedEventArgs e) {
            var window = new Preferences();
            window.Owner = this;
            window.ShowDialog();
        }

        void AboutExecuted(object sender, ExecutedRoutedEventArgs e) {
            var window = new About(featureLevel);
            window.Owner = this;
            window.ShowDialog();
        }

        void CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
            e.Handled = true;
        }

        void NewCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            if (featureLevel.IsDemo)
                e.CanExecute = false;
            else
                e.CanExecute = true;

            e.Handled = true;
        }

        object CreateCompareAnalysesHeaderToolTip(string fileNameAnalysisUpper, string fileNameAnalysisLower) {
            return "Upper: " + fileNameAnalysisUpper + "\r\nLower: " + fileNameAnalysisLower;
        }

        void UpdateLogFolder(string file) {
            Properties.Settings.Default.DirectoryLog = System.IO.Path.GetDirectoryName(file);
        }

        void UpdateAnalysisFolder(string file) {
            Properties.Settings.Default.DirectoryAnalysis = System.IO.Path.GetDirectoryName(file);
        }


        void ExecuteNewLog() {
            List<string> fileNames = TwFile.GetLogFilesIncludingZipped(Properties.Settings.Default.DirectoryLog);

            if (fileNames.Count > 0)
                UpdateLogFolder(fileNames[0]);

            foreach (string fileNameLog in fileNames)
                if (TwServices.IsLog(fileNameLog)) {
                    AddTabLoadLog(fileNameLog);
                }
        }

        void OpenRecoverExecuted(object sender, ExecutedRoutedEventArgs e) {
            List<string> fileNames = TwFile.GetArffAnalysisFilesRecover();

            foreach (string fileNameActualAnalysis in fileNames)
                if (TwServices.IsAnalysis(fileNameActualAnalysis)) {
                    AddTabLoadAnalysis(fileNameActualAnalysis);
                }
        }

        void ExecuteOpen() {
            List<string> fileNames = TwFile.GetAnalysisFilesIncludingZipped();

            if (fileNames.Count > 0)
                UpdateAnalysisFolder(fileNames[0]);

            foreach(string fileNameActualAnalysis in fileNames)
                if (TwServices.IsAnalysis(fileNameActualAnalysis)) {
                    AddTabLoadAnalysis(fileNameActualAnalysis);
                }
        }

        void ExecutePrint() {
            PrintDialog dialog = new PrintDialog();

            if (dialog.ShowDialog() == true) {
                dialog.PrintVisual(this, Title);
            }
        }

        void ExecuteSave(Analysis analysis, string arffFile) {
            var analysisAdapter = new ArffAnalysisAdapter();
            if (arffFile == null || arffFile.Length == 0)
                return;

            System.IO.File.Delete(arffFile);
            analysisAdapter.Save(arffFile, analysis,true);
            analysis.Events.IsDirty = false;
            return;
        }

        string ExecuteSaveAs(Analysis analysis, string saveFileName) {
            var analysisAdapter = new ArffAnalysisAdapter();
            string arffFile = TwFile.GetArffFileToSave(saveFileName);
            if (arffFile == null || arffFile.Length == 0)
                return saveFileName;

            System.IO.File.Delete(arffFile);
            analysisAdapter.Save(arffFile, analysis,true);
            analysis.Events.IsDirty = false;

            return arffFile;
        }
    }
}

