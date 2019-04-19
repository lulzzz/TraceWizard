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
using System.Windows.Shapes;
using System.Globalization;

using System.ComponentModel;

using TraceWizard.Environment;

namespace TraceWizard.TwApp {

    public partial class Preferences : Window {

        bool cancel = true;

        bool? PreviousShowStatusBar;
        bool? PreviousShowClassifyFirstCyclesUsingMachineLearningButton;
        bool? PreviousShowGraphToolBar;
        bool? PreviousShowFixturesInGraphToolBar;
        bool? PreviousSingleLineGraphToolBar;
        bool? PreviousShowSelectedEventPanel;
        bool? PreviousShowCurrentEventPanel;
        bool? PreviousShowSummaryPanel;
        bool? PreviousShowEventPanelsOnLeft;
        bool? PreviousShowLogPanel;
        bool? PreviousShowPieChartsPanel;
        bool? PreviousShowReportsPanel;
        bool? PreviousShowCommandPanel;
        bool? PreviousShowSelectionRulerAboveGraph;
        bool? PreviousShowSelectionRulerByDefault;
        bool? PreviousShowApprovalRulerAboveGraph;
        bool? PreviousShowApprovalRulerByDefault;
        bool? PreviousShowClassificationRulerAboveGraph;
        bool? PreviousShowClassificationRulerByDefault;
        bool? PreviousShowFixturesRulerByDefault;
        bool? PreviousSelectFirstEvent;
        bool? PreviousShowFixturesRulerAboveGraph;
        bool? PreviousShowVerticalGuideline;
        bool? PreviousShowHorizontalGuideline;
        bool? PreviousShowGraphToolBarAboveGraph;
        bool? PreviousShowTimeFrameAboveGraph;
        bool? PreviousShowViewTimeFrame;
        bool? PreviousShowTraceTimeFrame;
        bool? PreviousShowUnusedFixtures;
        bool? PreviousShowEventToolTips;
        bool? PreviousShowDetailedEventToolTips;
        bool? PreviousShowSimilarCountInEventToolTips;

        bool? PreviousCycleCommandActions;

        string PreviousGraphVerticalGuidelineColor;
        string PreviousGraphHorizontalGuidelineColor;
        string PreviousSelectionTickColor;
        string PreviousGraphBackgroundColor;
        string PreviousAnalysisPanelBackgroundColor;

        string PreviousViewportSeconds;
        string PreviousDefaultClassifier;
        string PreviousFindMode;
        string PreviousClassificationActionMode;
        string PreviousManualClassificationMode;

        int PreviousMinutesToStartFirstCycle;
        int PreviousMinutesBetweenBackgroundSaves;

        public Preferences() {
            InitializeComponent();
            Initialize();
        }

        void Initialize() {
            PreviousShowStatusBar = Properties.Settings.Default.ShowStatusBar;
            PreviousShowClassifyFirstCyclesUsingMachineLearningButton = Properties.Settings.Default.ShowClassifyFirstCyclesUsingMachineLearningButton;
            PreviousShowGraphToolBar = Properties.Settings.Default.ShowGraphToolBar;
            PreviousShowFixturesInGraphToolBar = Properties.Settings.Default.ShowFixturesInGraphToolBar;
            PreviousSingleLineGraphToolBar = Properties.Settings.Default.SingleLineGraphToolBar;
            PreviousShowSelectedEventPanel = Properties.Settings.Default.ShowSelectedEventPanel;
            PreviousShowCurrentEventPanel = Properties.Settings.Default.ShowCurrentEventPanel;
            PreviousShowSummaryPanel = Properties.Settings.Default.ShowSummaryPanel;
            PreviousShowEventPanelsOnLeft = Properties.Settings.Default.ShowEventPanelsOnLeft;
            PreviousShowLogPanel = Properties.Settings.Default.ShowLogPanel;
            PreviousShowPieChartsPanel = Properties.Settings.Default.ShowPieChartsPanel;
            PreviousShowReportsPanel = Properties.Settings.Default.ShowReportsPanel;
            PreviousShowCommandPanel = Properties.Settings.Default.ShowCommandPanel;
            PreviousShowSelectionRulerAboveGraph = Properties.Settings.Default.ShowSelectionRulerAboveGraph;
            PreviousShowSelectionRulerByDefault = Properties.Settings.Default.ShowSelectionRulerByDefault;
            PreviousShowApprovalRulerAboveGraph = Properties.Settings.Default.ShowApprovalRulerAboveGraph;
            PreviousShowApprovalRulerByDefault = Properties.Settings.Default.ShowApprovalRulerByDefault;
            PreviousShowClassificationRulerAboveGraph = Properties.Settings.Default.ShowClassificationRulerAboveGraph;
            PreviousShowClassificationRulerByDefault = Properties.Settings.Default.ShowClassificationRulerByDefault;
            PreviousShowFixturesRulerByDefault = Properties.Settings.Default.ShowFixturesRulerByDefault;
            PreviousSelectFirstEvent = Properties.Settings.Default.SelectFirstEvent;
            PreviousShowFixturesRulerAboveGraph = Properties.Settings.Default.ShowFixturesRulerAboveGraph;
            PreviousShowVerticalGuideline = Properties.Settings.Default.ShowVerticalGuideline;
            PreviousShowHorizontalGuideline = Properties.Settings.Default.ShowHorizontalGuideline;
            PreviousShowVerticalGuideline = Properties.Settings.Default.ShowVerticalGuideline;
            PreviousShowGraphToolBarAboveGraph = Properties.Settings.Default.ShowGraphToolBarAboveGraph;
            PreviousShowTimeFrameAboveGraph = Properties.Settings.Default.ShowTimeFrameAboveGraph;
            PreviousShowViewTimeFrame = Properties.Settings.Default.ShowViewTimeFrame;
            PreviousShowTraceTimeFrame = Properties.Settings.Default.ShowTraceTimeFrame;
            PreviousShowUnusedFixtures = CheckBoxShowUnusedFixtures.IsChecked;
            PreviousShowEventToolTips = Properties.Settings.Default.ShowEventToolTips;
            PreviousShowDetailedEventToolTips = Properties.Settings.Default.ShowDetailedEventToolTips;
            PreviousShowSimilarCountInEventToolTips = Properties.Settings.Default.ShowSimilarCountInEventToolTips;

            PreviousCycleCommandActions = Properties.Settings.Default.CycleCommandActions;

            PreviousGraphVerticalGuidelineColor = Properties.Settings.Default.GraphVerticalGuidelineColor;
            PreviousGraphHorizontalGuidelineColor = Properties.Settings.Default.GraphHorizontalGuidelineColor;
            PreviousSelectionTickColor = Properties.Settings.Default.SelectionTickColor;
            PreviousGraphBackgroundColor = Properties.Settings.Default.GraphBackgroundColor;
            PreviousAnalysisPanelBackgroundColor = Properties.Settings.Default.AnalysisPanelBackgroundColor;

            PreviousViewportSeconds = Properties.Settings.Default.ViewportSeconds;
            PreviousDefaultClassifier = Properties.Settings.Default.Classifier;
            PreviousFindMode = Properties.Settings.Default.FindMode;
            PreviousClassificationActionMode = Properties.Settings.Default.ClassificationActionMode;
            PreviousManualClassificationMode = Properties.Settings.Default.ManualClassificationMode;

            PreviousMinutesToStartFirstCycle = Properties.Settings.Default.MinutesToStartFirstCycle;
            PreviousMinutesBetweenBackgroundSaves = Properties.Settings.Default.MinutesBetweenBackgroundSaves;

            this.Closing += new System.ComponentModel.CancelEventHandler(Preferences_Closing);

            this.ButtonOk.Click += new RoutedEventHandler(ButtonOk_Click);
            this.ButtonCancel.Click += new RoutedEventHandler(ButtonCancel_Click);
        }

        void Preferences_Closing(object sender, CancelEventArgs e) {
            if (cancel)
                Rollback();
        }

        void ButtonOk_Click(object sender, System.Windows.RoutedEventArgs e) {
            if (IsValid(this)) {
                if (PreviousShowStatusBar != CheckBoxShowStatusBar.IsChecked.Value)
                    Properties.Settings.Default.ShowStatusBar = CheckBoxShowStatusBar.IsChecked.Value ? true : false;
                if (PreviousShowClassifyFirstCyclesUsingMachineLearningButton != CheckBoxShowClassifyFirstCyclesUsingMachineLearningButton.IsChecked.Value)
                    Properties.Settings.Default.ShowClassifyFirstCyclesUsingMachineLearningButton = CheckBoxShowClassifyFirstCyclesUsingMachineLearningButton.IsChecked.Value ? true : false;
                if (PreviousShowGraphToolBar != CheckBoxShowGraphToolBar.IsChecked.Value)
                    Properties.Settings.Default.ShowGraphToolBar = CheckBoxShowGraphToolBar.IsChecked.Value ? true : false;
                if (PreviousShowFixturesInGraphToolBar != CheckBoxShowFixturesInGraphToolBar.IsChecked.Value)
                    Properties.Settings.Default.ShowFixturesInGraphToolBar = CheckBoxShowFixturesInGraphToolBar.IsChecked.Value ? true : false;
                if (PreviousSingleLineGraphToolBar != CheckBoxSingleLineGraphToolBar.IsChecked.Value)
                    Properties.Settings.Default.SingleLineGraphToolBar = CheckBoxSingleLineGraphToolBar.IsChecked.Value ? true : false;
                if (PreviousShowSelectedEventPanel != CheckBoxShowSelectedEventPanel.IsChecked.Value)
                    Properties.Settings.Default.ShowSelectedEventPanel = CheckBoxShowSelectedEventPanel.IsChecked.Value ? true : false;
                if (PreviousShowCurrentEventPanel != CheckBoxShowCurrentEventPanel.IsChecked.Value)
                    Properties.Settings.Default.ShowCurrentEventPanel = CheckBoxShowCurrentEventPanel.IsChecked.Value ? true : false;
                if (PreviousShowSummaryPanel != CheckBoxShowSummaryPanel.IsChecked.Value)
                    Properties.Settings.Default.ShowSummaryPanel = CheckBoxShowSummaryPanel.IsChecked.Value ? true : false;
                if (PreviousShowEventPanelsOnLeft != CheckBoxShowEventPanelsOnLeft.IsChecked.Value)
                    Properties.Settings.Default.ShowEventPanelsOnLeft = CheckBoxShowEventPanelsOnLeft.IsChecked.Value ? true : false;
                if (PreviousShowLogPanel != CheckBoxShowLogPanel.IsChecked.Value)
                    Properties.Settings.Default.ShowLogPanel = CheckBoxShowLogPanel.IsChecked.Value ? true : false;
                if (PreviousShowPieChartsPanel != CheckBoxShowPieChartsPanel.IsChecked.Value)
                    Properties.Settings.Default.ShowPieChartsPanel = CheckBoxShowPieChartsPanel.IsChecked.Value ? true : false;
                if (PreviousShowReportsPanel != CheckBoxShowReportsPanel.IsChecked.Value)
                    Properties.Settings.Default.ShowReportsPanel = CheckBoxShowReportsPanel.IsChecked.Value ? true : false;
                if (PreviousShowCommandPanel != CheckBoxShowCommandPanel.IsChecked.Value)
                    Properties.Settings.Default.ShowCommandPanel = CheckBoxShowCommandPanel.IsChecked.Value ? true : false;
                if (PreviousShowSelectionRulerAboveGraph != CheckBoxShowSelectionRulerAboveGraph.IsChecked)
                    Properties.Settings.Default.ShowSelectionRulerAboveGraph = CheckBoxShowSelectionRulerAboveGraph.IsChecked.Value ? true : false;
                if (PreviousShowSelectionRulerByDefault != CheckBoxShowSelectionRulerByDefault.IsChecked)
                    Properties.Settings.Default.ShowSelectionRulerByDefault = CheckBoxShowSelectionRulerByDefault.IsChecked.Value ? true : false;
                if (PreviousShowApprovalRulerAboveGraph != CheckBoxShowApprovalRulerAboveGraph.IsChecked)
                    Properties.Settings.Default.ShowApprovalRulerAboveGraph = CheckBoxShowApprovalRulerAboveGraph.IsChecked.Value ? true : false;
                if (PreviousShowApprovalRulerByDefault != CheckBoxShowApprovalRulerByDefault.IsChecked)
                    Properties.Settings.Default.ShowApprovalRulerByDefault = CheckBoxShowApprovalRulerByDefault.IsChecked.Value ? true : false;
                if (PreviousShowClassificationRulerAboveGraph != CheckBoxShowClassificationRulerAboveGraph.IsChecked)
                    Properties.Settings.Default.ShowClassificationRulerAboveGraph = CheckBoxShowClassificationRulerAboveGraph.IsChecked.Value ? true : false;
                if (PreviousShowClassificationRulerByDefault != CheckBoxShowClassificationRulerByDefault.IsChecked)
                    Properties.Settings.Default.ShowClassificationRulerByDefault = CheckBoxShowClassificationRulerByDefault.IsChecked.Value ? true : false;
                if (PreviousShowFixturesRulerByDefault != CheckBoxShowFixturesRulerByDefault.IsChecked)
                    Properties.Settings.Default.ShowFixturesRulerByDefault = CheckBoxShowFixturesRulerByDefault.IsChecked.Value ? true : false;
                if (PreviousSelectFirstEvent != CheckBoxSelectFirstEvent.IsChecked)
                    Properties.Settings.Default.SelectFirstEvent = CheckBoxSelectFirstEvent.IsChecked.Value ? true : false;
                if (PreviousShowFixturesRulerAboveGraph != CheckBoxShowFixturesRulerAboveGraph.IsChecked)
                    Properties.Settings.Default.ShowFixturesRulerAboveGraph = CheckBoxShowFixturesRulerAboveGraph.IsChecked.Value ? true : false;
                if (PreviousShowVerticalGuideline != CheckBoxShowVerticalGuideline.IsChecked)
                    Properties.Settings.Default.ShowVerticalGuideline = CheckBoxShowVerticalGuideline.IsChecked.Value ? true : false;
                if (PreviousShowHorizontalGuideline != CheckBoxShowHorizontalGuideline.IsChecked)
                    Properties.Settings.Default.ShowHorizontalGuideline = CheckBoxShowHorizontalGuideline.IsChecked.Value ? true : false;
                if (PreviousShowGraphToolBarAboveGraph != CheckBoxShowGraphToolBarAboveGraph.IsChecked)
                    Properties.Settings.Default.ShowGraphToolBarAboveGraph = CheckBoxShowGraphToolBarAboveGraph.IsChecked.Value ? true : false;
                if (PreviousShowTimeFrameAboveGraph != CheckBoxShowTimeFrameAboveGraph.IsChecked)
                    Properties.Settings.Default.ShowTimeFrameAboveGraph = CheckBoxShowTimeFrameAboveGraph.IsChecked.Value ? true : false;
                if (PreviousShowViewTimeFrame != CheckBoxShowViewTimeFrame.IsChecked)
                    Properties.Settings.Default.ShowViewTimeFrame = CheckBoxShowViewTimeFrame.IsChecked.Value ? true : false;
                if (PreviousShowTraceTimeFrame != CheckBoxShowTraceTimeFrame.IsChecked)
                    Properties.Settings.Default.ShowTraceTimeFrame = CheckBoxShowTraceTimeFrame.IsChecked.Value ? true : false;
                if (PreviousShowUnusedFixtures != CheckBoxShowUnusedFixtures.IsChecked)
                    Properties.Settings.Default.ShowUnusedFixtures = CheckBoxShowUnusedFixtures.IsChecked.Value ? true : false;
                if (PreviousShowEventToolTips != CheckBoxShowEventToolTips.IsChecked)
                    Properties.Settings.Default.ShowEventToolTips = CheckBoxShowEventToolTips.IsChecked.Value ? true : false;
                if (PreviousShowDetailedEventToolTips != CheckBoxShowDetailedEventToolTips.IsChecked)
                    Properties.Settings.Default.ShowDetailedEventToolTips = CheckBoxShowDetailedEventToolTips.IsChecked.Value ? true : false;
                if (PreviousShowSimilarCountInEventToolTips != CheckBoxShowSimilarCountInEventToolTips.IsChecked)
                    Properties.Settings.Default.ShowSimilarCountInEventToolTips = CheckBoxShowSimilarCountInEventToolTips.IsChecked.Value ? true : false;

                if (PreviousCycleCommandActions != CheckBoxCycleCommandActions.IsChecked)
                    Properties.Settings.Default.CycleCommandActions = CheckBoxCycleCommandActions.IsChecked.Value ? true : false;

                if (PreviousGraphVerticalGuidelineColor != (string)(((FrameworkElement)(ComboBoxGraphVerticalGuidelineColor.SelectedItem)).Tag))
                    Properties.Settings.Default.GraphVerticalGuidelineColor = (string)(((FrameworkElement)(ComboBoxGraphVerticalGuidelineColor.SelectedItem)).Tag);
                if (PreviousGraphHorizontalGuidelineColor != (string)(((FrameworkElement)(ComboBoxGraphHorizontalGuidelineColor.SelectedItem)).Tag))
                    Properties.Settings.Default.GraphHorizontalGuidelineColor = (string)(((FrameworkElement)(ComboBoxGraphHorizontalGuidelineColor.SelectedItem)).Tag);
                if (PreviousSelectionTickColor != (string)(((FrameworkElement)(ComboBoxSelectionTickColor.SelectedItem)).Tag))
                    Properties.Settings.Default.SelectionTickColor = (string)(((FrameworkElement)(ComboBoxSelectionTickColor.SelectedItem)).Tag);
                if (PreviousGraphBackgroundColor != (string)(((FrameworkElement)(ComboBoxGraphBackgroundColor.SelectedItem)).Tag))
                    Properties.Settings.Default.GraphBackgroundColor = (string)(((FrameworkElement)(ComboBoxGraphBackgroundColor.SelectedItem)).Tag);
                if (PreviousAnalysisPanelBackgroundColor != (string)(((FrameworkElement)(ComboBoxAnalysisPanelBackgroundColor.SelectedItem)).Tag))
                    Properties.Settings.Default.AnalysisPanelBackgroundColor = (string)(((FrameworkElement)(ComboBoxAnalysisPanelBackgroundColor.SelectedItem)).Tag);

                if (PreviousViewportSeconds != (string)(((FrameworkElement)(ComboBoxViewportSeconds.SelectedItem)).Tag))
                    Properties.Settings.Default.ViewportSeconds = (string)(((FrameworkElement)(ComboBoxViewportSeconds.SelectedItem)).Tag);

                if (PreviousDefaultClassifier != (string)(((FrameworkElement)(ComboBoxDefaultClassifier.SelectedItem)).Tag))
                    Properties.Settings.Default.Classifier = (string)(((FrameworkElement)(ComboBoxDefaultClassifier.SelectedItem)).Tag);

                if (PreviousFindMode != (string)(((FrameworkElement)(ComboBoxFindMode.SelectedItem)).Tag))
                    Properties.Settings.Default.FindMode = (string)(((FrameworkElement)(ComboBoxFindMode.SelectedItem)).Tag);
                if (PreviousClassificationActionMode != (string)(((FrameworkElement)(ComboBoxClassificationActionMode.SelectedItem)).Tag))
                    Properties.Settings.Default.ClassificationActionMode = (string)(((FrameworkElement)(ComboBoxClassificationActionMode.SelectedItem)).Tag);
                if (PreviousManualClassificationMode != (string)(((FrameworkElement)(ComboBoxManualClassificationMode.SelectedItem)).Tag))
                    Properties.Settings.Default.ManualClassificationMode = (string)(((FrameworkElement)(ComboBoxManualClassificationMode.SelectedItem)).Tag);

                cancel = false;
                Close();
            }
        }

        void ButtonCancel_Click(object sender, System.Windows.RoutedEventArgs e) {
            Close();
        }

        void Rollback() {
            int dummy;
            if (!int.TryParse(TextBoxMinutesToStartFirstCycle.Text, out dummy) 
                || PreviousMinutesToStartFirstCycle != int.Parse(TextBoxMinutesToStartFirstCycle.Text))
                    Properties.Settings.Default.MinutesToStartFirstCycle = PreviousMinutesToStartFirstCycle;

            if (!int.TryParse(TextBoxMinutesBetweenBackgroundSaves.Text, out dummy) 
                || PreviousMinutesBetweenBackgroundSaves != int.Parse(TextBoxMinutesBetweenBackgroundSaves.Text))
                    Properties.Settings.Default.MinutesBetweenBackgroundSaves = PreviousMinutesBetweenBackgroundSaves;
        }

        public static bool IsChildValid(DependencyObject parent) {
            bool valid = true;

            System.Collections.IEnumerable children = LogicalTreeHelper.GetChildren(parent);
            foreach (object obj in children) {
                if (obj is DependencyObject) {
                    DependencyObject child = (DependencyObject)obj;
                    if (!IsValid(child)) { valid = false; }
                }
            }
            return valid;
        }

        public static bool IsValid(DependencyObject parent) {
            return IsParentValid(parent) && IsChildValid(parent);
        }
        public static bool IsParentValid(DependencyObject parent) {
            bool valid = true;
            LocalValueEnumerator localValues = parent.GetLocalValueEnumerator();
            while (localValues.MoveNext()) {
                LocalValueEntry entry = localValues.Current;
                if (BindingOperations.IsDataBound(parent, entry.Property)) {
                    Binding binding = BindingOperations.GetBinding(parent, entry.Property);
                    if (binding.ValidationRules.Count > 0) {
                        BindingExpression expression = BindingOperations.GetBindingExpression(parent, entry.Property);
                        expression.UpdateSource();

                        if (expression.HasError) {
                            valid = false;
                        }
                    }
                }
            }
            return valid;
        }
    }

    public class NumberRangeRule : ValidationRule {
        int min;
        public int Min {
            get { return min; }
            set { min = value; }
        }

        int max;
        public int Max {
            get { return max; }
            set { max = value; }
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo) {
            int number;
            if (!int.TryParse((string)value, out number)) {
                return new ValidationResult(false, "Invalid number format");
            }

            if (number < min || number > max) {
                return new ValidationResult(false, string.Format("Number out of range ({0}-{1})", min, max));
            }

            return ValidationResult.ValidResult;
        }
    }
    
    public class TagConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var p = parameter as string;
            var v = value as string;

            if (p != null && v != null)
                return p == v;
            return false;
        }

         public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
   }
}