using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.ComponentModel;

using TraceWizard.Entities;
using TraceWizard.Services;
using TraceWizard.Notification;

namespace TraceWizard.TwApp {

    public partial class FixturesPanel : UserControl {

        public Analysis Analysis;

        public FixturesPanel() {
            InitializeComponent();
        }

        public void Initialize() {

            InitializeFixturePanelLabels();

            grid.HorizontalAlignment = HorizontalAlignment.Left;

            int i = 0;
            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values) {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                InitializeFixturePanels(fixtureClass,i++);
            }
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            FixturePanelTotal.Label = "Total";
            FixturePanelTotal.Initialize();
            Grid.SetColumn(FixturePanelTotal, FixtureClasses.Items.Count);

            UpdateFixturesSummaryPanels(Analysis.FixtureSummaries, Analysis.FixtureSummaries);

            UpdateSelectedCountLabels();
            UpdateButtons();

            CollapseFixturePanels();

            Analysis.Events.PropertyChanged += new PropertyChangedEventHandler(Events_PropertyChanged);
            Properties.Settings.Default.PropertyChanged += new PropertyChangedEventHandler(Default_PropertyChanged);
            Visibility = SetVisibility();
        }

        Visibility SetVisibility() {
            return Properties.Settings.Default.ShowSummaryPanel ? Visibility.Visible : Visibility.Collapsed;
        }

        public void Default_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "ShowUnusedFixtures":
                    CollapseFixturePanels();
                    break;
                case "ShowSummaryPanel":
                    Visibility = SetVisibility();
                    break;
            }
        }

        void InitializeFixturePanels(FixtureClass fixtureClass, int i) {
            var fixturePanel = new FixturePanel();
            fixturePanel.FixtureClass = fixtureClass;
            fixturePanel.Analysis = Analysis;
            fixturePanel.Initialize();
            Grid.SetRow(fixturePanel, 0);
            Grid.SetColumn(fixturePanel, i);
            grid.Children.Add(fixturePanel);
        }

        void CollapseFixturePanels() {
            CollapseFixturePanels(null);
        }

        void CollapseFixturePanels(FixtureSummaries fixtureSummariesOld) {
            bool showUnusedFixtures = Properties.Settings.Default.ShowUnusedFixtures;

            foreach (var item in grid.Children) {
                var panel = item as FixturePanel;
                if (panel != null && panel.FixtureClass != null && Analysis != null && Analysis.FixtureSummaries != null)
                    if (Analysis.FixtureSummaries[panel.FixtureClass].Volume == 0 && (fixtureSummariesOld == null || fixtureSummariesOld[panel.FixtureClass].Volume == 0))
                        panel.Visibility = showUnusedFixtures ? Visibility.Visible : Visibility.Collapsed;
                    else
                        panel.Visibility = Visibility.Visible;
            }
        }

        void InitializeFixturePanelLabels() {
            FixturePanelLabels.FixtureLabel.ManuallyClassified = false;
//            FixturePanelLabels.Button.Visibility = Visibility.Hidden;
            FixturePanelLabels.FixtureLabel.Visibility = Visibility.Hidden;

            FixturePanelLabels.FixtureSummaryPanel.textBlockInstancesCount.Text = "Events:";
            FixturePanelLabels.FixtureSummaryPanel.textBlockInstancesCount.HorizontalAlignment = HorizontalAlignment.Right;
            FixturePanelLabels.FixtureSummaryPanel.textBlockInstancesCount.ToolTip = "Number of Events";
            FixturePanelLabels.FixtureSummaryPanel.textBlockInstancesCount.Foreground = Brushes.DarkGray;

            FixturePanelLabels.FixtureSummaryPanel.textBlockInstancesDelta.Text = "Delta:";
            FixturePanelLabels.FixtureSummaryPanel.textBlockInstancesDelta.HorizontalAlignment = HorizontalAlignment.Right;
            FixturePanelLabels.FixtureSummaryPanel.textBlockInstancesDelta.FontWeight = FontWeights.Normal;
            FixturePanelLabels.FixtureSummaryPanel.textBlockInstancesDelta.ToolTip = "Increase or Decrease in Number of Events";
            FixturePanelLabels.FixtureSummaryPanel.textBlockInstancesDelta.Foreground = Brushes.DarkGray;

            FixturePanelLabels.FixtureSummaryPanel.textBlockVolume.Text = "Vol:";
            FixturePanelLabels.FixtureSummaryPanel.textBlockVolume.Foreground = Brushes.DarkGray;
            FixturePanelLabels.FixtureSummaryPanel.textBlockVolume.HorizontalAlignment = HorizontalAlignment.Right;
            FixturePanelLabels.FixtureSummaryPanel.textBlockVolume.ToolTip = "Volume";

            FixturePanelLabels.FixtureSummaryPanel.textBlockVolumePercent.Text = "Vol %:";
            FixturePanelLabels.FixtureSummaryPanel.textBlockVolumePercent.Foreground = Brushes.DarkGray;
            FixturePanelLabels.FixtureSummaryPanel.textBlockVolumePercent.HorizontalAlignment = HorizontalAlignment.Right;
            FixturePanelLabels.FixtureSummaryPanel.textBlockVolumePercent.ToolTip = "Percentage of Volume";

            FixturePanelLabels.FixtureSummaryPanel.textBlockSelected.Text = "Sel:";
            FixturePanelLabels.FixtureSummaryPanel.textBlockSelected.Foreground = Brushes.DarkGray;
            FixturePanelLabels.FixtureSummaryPanel.textBlockSelected.HorizontalAlignment = HorizontalAlignment.Right;
            FixturePanelLabels.FixtureSummaryPanel.textBlockSelected.ToolTip = "Number of Events Selected";
        }
        
        void UpdateButtons() {
            //foreach (FixturePanel fixturePanel in grid.Children) {
            //    fixturePanel.Button.IsEnabled = analysis.Events.SelectedEvents.Count == 0 ? false : true;
            //}
        }
        
        FixtureSummaries fixtureSummariesOld { get; set; }
        
        public void Events_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case TwNotificationProperty.OnStartClassify:
                case TwNotificationProperty.OnStartMergeSplit:
                case TwNotificationProperty.OnStartApplyConversionFactor:
                    fixtureSummariesOld = new FixtureSummaries(Analysis.Events);
                    fixtureSummariesOld.Update();
                    break;
                case TwNotificationProperty.OnEndMergeSplit:
                    UpdateFixturesSummaryPanels(fixtureSummariesOld, Analysis.FixtureSummaries);
                    CollapseFixturePanels(fixtureSummariesOld);
                    UpdateButtons();
                    break;
                case TwNotificationProperty.OnEndClassify:
                    UpdateFixturesSummaryPanels(fixtureSummariesOld, Analysis.FixtureSummaries);
                    CollapseFixturePanels(fixtureSummariesOld);
                    UpdateButtons();
                    break;
                case TwNotificationProperty.OnEndSelect:
                    UpdateSelectedCountLabels();
                    UpdateButtons();
                    break;
                case TwNotificationProperty.OnEndApplyConversionFactor:
                    UpdateFixturesSummaryPanels(fixtureSummariesOld, Analysis.FixtureSummaries);
                    UpdateButtons();
                    break;
            }
        }

        void UpdateSelectedCountLabels() {
            UpdateSelectLabels(Analysis.FixtureSummaries);
        }

        string WrapInParentheses(string s) { return " (" + s + ")"; }

        void UpdateFixtureSummaryInstanceCount(TextBlock textBlock, FixtureSummaries fixtureSummariesNew, FixtureClass fixtureClass) {
            int countNew = fixtureSummariesNew[fixtureClass].Count;
            if (countNew == 0) {
                textBlock.Text = string.Empty;
            } else {
                textBlock.Text = countNew.ToString();

                ToolTipService.SetShowDuration(textBlock, 60000);
                ToolTipService.SetInitialShowDelay(textBlock, 000);

                if (fixtureClass.CanHaveCycles) {
                    textBlock.Text += WrapInParentheses(fixtureSummariesNew[fixtureClass].FirstCycles.ToString());
                }
            }
        }

        void UpdateFixtureSummaryInstanceDelta(TextBlock textBlock, FixtureSummaries fixtureSummariesNew, FixtureSummaries fixtureSummariesOld, FixtureClass fixtureClass) {
            int countNew = fixtureSummariesNew[fixtureClass].Count;
            int countOld = fixtureSummariesOld[fixtureClass].Count;
            int deltaCount = countNew - countOld;

            int countFirstCyclesNew = fixtureSummariesNew[fixtureClass].FirstCycles;
            int countFirstCyclesOld = fixtureSummariesOld[fixtureClass].FirstCycles;
            int deltaFirstCyclesCount = countFirstCyclesNew - countFirstCyclesOld;

            textBlock.Text = string.Empty;
            textBlock.ToolTip = string.Empty;

            if (deltaCount == 0 && deltaFirstCyclesCount == 0)
                return;

            if (deltaCount != 0) {
                textBlock.ToolTip += "This action ";

                if (deltaCount > 0) {
                    textBlock.Text = "+" + deltaCount.ToString();
                    textBlock.Foreground = TwBrushes.GetPlusBrush();
                    textBlock.FontWeight = FontWeights.Bold;
                    textBlock.ToolTip += "increased ";
                } else {
                    textBlock.Text = deltaCount.ToString();
                    textBlock.Foreground = TwBrushes.GetMinusBrush();
                    textBlock.FontWeight = FontWeights.Bold;
                    textBlock.ToolTip += "decreased ";
                }
                textBlock.ToolTip += "the number of " + fixtureClass.FriendlyName + " events by " + Math.Abs(deltaCount) + ".";
                if (deltaFirstCyclesCount != 0)
                    textBlock.ToolTip += "\r\n";
            }

            if (deltaFirstCyclesCount != 0) {
                textBlock.ToolTip += "This action ";

                if (deltaFirstCyclesCount > 0) {
                    textBlock.Text += WrapInParentheses("+" + deltaFirstCyclesCount.ToString());
                    textBlock.Foreground = TwBrushes.GetPlusBrush();
                    textBlock.FontWeight = FontWeights.Bold;
                    textBlock.ToolTip += "increased ";
                } else if (deltaFirstCyclesCount < 0) {
                    textBlock.Text += WrapInParentheses(deltaFirstCyclesCount.ToString());
                    textBlock.Foreground = TwBrushes.GetMinusBrush();
                    textBlock.FontWeight = FontWeights.Bold;
                    textBlock.ToolTip += "decreased ";
                }
                textBlock.ToolTip += "the number of " + fixtureClass.FriendlyName + " 1st cycles by " + Math.Abs(deltaFirstCyclesCount) + ".";
            }
        }

        void UpdateFixtureSummaryVolumeCount(TextBlock textBlock, FixtureSummaries fixtureSummariesNew, FixtureClass fixtureClass) {
            double volume = fixtureSummariesNew[fixtureClass].Volume;
            if (volume == 0) {
                textBlock.Text = string.Empty;
            } else {
                textBlock.Text = volume.ToString("0.0");
                textBlock.ToolTip = "Volume of " + fixtureClass.FriendlyName + " Events";
            }
        }

        void UpdateFixtureSummaryVolumePercent(TextBlock textBlock, FixtureSummaries fixtureSummariesNew, FixtureClass fixtureClass) {
            double percentVolume = fixtureSummariesNew[fixtureClass].PercentVolume;
            if (percentVolume == 0) {
                textBlock.Text = string.Empty;
            } else {
                textBlock.Text = (percentVolume * 100).ToString("0.0") + "%";
                textBlock.ToolTip = "Percentage of Total Volume Attributed to " + fixtureClass.FriendlyName + " Events";
            }
        }

        void UpdateFixtureSummarySelectedCountLabels(TextBlock textBlock, FixtureSummaries fixtureSummariesNew, FixtureClass fixtureClass) {
            int countNew = fixtureSummariesNew[fixtureClass].SelectedCount;
            if (countNew == 0) {
                textBlock.Text = string.Empty;
            } else {
                textBlock.Text = countNew.ToString();
                textBlock.ToolTip = "Number of " + fixtureClass.FriendlyName + " Events Selected";
            }
        }

        void UpdateFixtureSummary(FixtureSummaryPanel fixtureSummaryPanel, FixtureSummaries fixtureSummariesOld, FixtureSummaries fixtureSummariesNew) {
            FixtureClass fixtureClass = fixtureSummaryPanel.FixtureClass;

            UpdateFixtureSummaryInstanceCount(fixtureSummaryPanel.textBlockInstancesCount, fixtureSummariesNew, fixtureClass);

            UpdateFixtureSummaryInstanceDelta(fixtureSummaryPanel.textBlockInstancesDelta, fixtureSummariesNew, fixtureSummariesOld, fixtureClass);

            UpdateFixtureSummaryVolumeCount(fixtureSummaryPanel.textBlockVolume, fixtureSummariesNew, fixtureClass);

            UpdateFixtureSummaryVolumePercent(fixtureSummaryPanel.textBlockVolumePercent, fixtureSummariesNew, fixtureClass);

            UpdateFixtureSummarySelectedCountLabels(fixtureSummaryPanel.textBlockSelected, fixtureSummariesNew, fixtureClass);

        }

        void UpdateSelectLabels(FixtureSummaryPanel fixtureSummaryPanel, FixtureSummaries fixtureSummariesNew) {
            UpdateFixtureSummarySelectedCountLabels(fixtureSummaryPanel.textBlockSelected, fixtureSummariesNew, fixtureSummaryPanel.FixtureClass);
        }

        void InitializeFixtureSummaries(FixtureSummaries fixtureSummaries) {
            for (int i = 0; i < FixtureClasses.Items.Count; i++) {
                FixturePanel fixturePanel = (FixturePanel)grid.Children[i];
                FixtureClass fixtureClass = fixturePanel.FixtureClass;
            }
        }

        void UpdateFixturesSummaryPanels(FixtureSummaries fixtureSummariesOld, FixtureSummaries fixtureSummariesNew) {
            for (int i = 0; i < FixtureClasses.Items.Count; i++) {
                FixturePanel fixturePanel = (FixturePanel)grid.Children[i];
                UpdateFixtureSummary(fixturePanel.FixtureSummaryPanel, fixtureSummariesOld, fixtureSummariesNew);
            }
            UpdateFixtureSummaryPanelTotal(Analysis.Events, fixtureSummariesOld, fixtureSummariesNew);
        }

        void UpdateFixtureSummaryPanelTotal(Events events, FixtureSummaries fixtureSummariesOld, FixtureSummaries fixtureSummariesNew) {
            var fixtureSummaryPanel = FixturePanelTotal.FixtureSummaryPanel;

            fixtureSummaryPanel.textBlockInstancesCount.Text = events.Count.ToString();
            fixtureSummaryPanel.textBlockInstancesCount.ToolTip = "Total Number of Events";

            UpdateFixtureSummaryInstanceDeltaTotals(fixtureSummaryPanel.textBlockInstancesDelta, fixtureSummariesOld, fixtureSummariesNew);
            fixtureSummaryPanel.textBlockInstancesDelta.ToolTip = "Total Increase or Decrease in Number of Events";

            fixtureSummaryPanel.textBlockVolume.Text = events.Volume.ToString("0.0");
            fixtureSummaryPanel.textBlockVolume.FontWeight = FontWeights.Bold;
            fixtureSummaryPanel.textBlockVolume.ToolTip = "Total Volume of Events";

            double percentInitialVolume = events.InitialVolume / events.Volume;
            fixtureSummaryPanel.textBlockVolumePercent.Text = (percentInitialVolume * 100).ToString("0.0") + "%";
            if (events.CheckVolume()) {
                fixtureSummaryPanel.textBlockVolumePercent.ToolTip = "Initial Total Percent Volume";
                fixtureSummaryPanel.textBlockVolumePercent.Background = fixtureSummaryPanel.textBlockVolume.Background;
            } else {
                fixtureSummaryPanel.textBlockVolumePercent.ToolTip = "Warning: Initial Total Percent Volume differs significantly from Current Total Percent Volume";
                fixtureSummaryPanel.textBlockVolumePercent.Background = new SolidColorBrush(Colors.Red);
            }
        }

        void UpdateFixtureSummaryInstanceDeltaTotals(TextBlock textBlock, FixtureSummaries fixtureSummariesOld, FixtureSummaries fixtureSummariesNew) {
            int countNew = fixtureSummariesNew.CountEvents();
            int countOld = fixtureSummariesOld.CountEvents();
            int deltaCount = countNew - countOld;
            if (deltaCount == 0) {
                textBlock.Text = string.Empty;
            } else {
                if (deltaCount > 0) {
                    textBlock.Text = "+" + deltaCount.ToString();
                    textBlock.Foreground = TwBrushes.GetPlusBrush();
                    textBlock.FontWeight = FontWeights.Bold;
                } else {
                    textBlock.Text = deltaCount.ToString();
                    textBlock.Foreground = TwBrushes.GetMinusBrush();
                    textBlock.FontWeight = FontWeights.Bold;
                }
            }
        }

        void UpdateSelectLabels(FixtureSummaries fixtureSummariesNew) {
            for (int i = 0; i < FixtureClasses.Items.Count; i++) {
                FixturePanel fixturePanel = (FixturePanel)grid.Children[i];
                UpdateSelectLabels(fixturePanel.FixtureSummaryPanel, fixtureSummariesNew);
            }
            UpdateSelectedVolume(fixtureSummariesNew);
        }

        void UpdateSelectedVolume(FixtureSummaries fixtureSummariesNew) {
            FixtureSummaryPanel fixtureSummaryPanel = FixturePanelTotal.FixtureSummaryPanel;

            if (fixtureSummariesNew.SelectedVolume == 0)
                fixtureSummaryPanel.textBlockSelected.Text = string.Empty;
            else
                fixtureSummaryPanel.textBlockSelected.Text = fixtureSummariesNew.SelectedVolume.ToString("0.0");
            fixtureSummaryPanel.textBlockSelected.ToolTip = "Total Volume of Selected Events";
        }
    }
}
