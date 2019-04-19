using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Globalization;

using System.ComponentModel;

using TraceWizard.Entities;

namespace TraceWizard.TwApp {

    public class ConfusionMatrixAggregatePanel : ConfusionMatrixPanel {
        protected override Trace Trace { get { return Traces.TraceAggregate; } }
    }
   
    public class ConfusionMatrixPanel : UserControl {
        protected Traces Traces { get; set; }
        protected virtual Trace Trace { get { return Traces.CurrentTrace; } }

        Grid gridOuter;
        UIElement aggregatePanel;
        Grid aggregateGrid;

        public void Load(Traces traces) {
            
            Traces = traces;

            InitUserControl();

            DockPanel dockPanel = new DockPanel();
            dockPanel.Background = Brushes.White;

            DockPanel.SetDock(aggregatePanel = CreateAggregatePanel(), Dock.Bottom);
            dockPanel.Children.Add(aggregatePanel);

            DockPanel.SetDock(gridOuter = CreateGrid(), Dock.Bottom);
            dockPanel.Children.Add(gridOuter);

            this.Content = WrapWithLabel(dockPanel, 
                traces.Classifier.Name, TwGui.CreateAnalyzerToolTip(traces.Classifier.GetType(), traces.Classifier),
                traces.Adopter.Name, TwGui.CreateAnalyzerToolTip(traces.Adopter.GetType(), traces.Adopter),
                System.IO.Path.GetDirectoryName(traces.FilesLoaded[0]), 
                CreateElementFiles(traces.FilesLoaded));

            PopulateRowLabel();
            PopulateColumnLabel();

            CreateDisplayModeSelector();

            CreateRowAndColumnHeaders();
            PopulateRowHeaders();
            PopulateColumnHeaders();

            PopulateRows();
            RenderRows();

            PopulateFixturesColumnNoText();
            PopulateFixturesRowNoText();

            PopulateStatistics();

            PopulateAggregatePanel();
        }

        Border CreateAggregatePanel() {
            Border border = new Border();
            border.Padding = new Thickness(4);
            border.BorderThickness = new Thickness(0, 1, 0, 0);
            border.BorderBrush = Brushes.DarkGray;

            aggregateGrid = new Grid();

            for (int i = 0; i < 2; i++) {
                var rowDefinition = new RowDefinition();
                rowDefinition.Height = new GridLength(1, GridUnitType.Star);
                aggregateGrid.RowDefinitions.Add(rowDefinition);
            }

            for (int i = 0; i < 8; i++) {
                var columnDefinition = new ColumnDefinition();
                columnDefinition.Width = GridLength.Auto;
                aggregateGrid.ColumnDefinitions.Add(columnDefinition);
            }

            aggregateGrid.HorizontalAlignment = HorizontalAlignment.Left;
            aggregateGrid.VerticalAlignment = VerticalAlignment.Center;

            border.Child = aggregateGrid;
            return border;
        }

        Grid CreateGrid() {
            Grid grid = new Grid();
            grid.HorizontalAlignment = HorizontalAlignment.Stretch;
            grid.VerticalAlignment = VerticalAlignment.Stretch;
            return grid;
        }

        void InitUserControl() {
            FontSize = 11;
        }

        enum DisplayMode {
            None = 0,
            By_Events,
            By_Volume,
            Adoption_Sources,
            Adoption_Targets,
        }
        DisplayMode displayMode = DisplayMode.By_Events;

        RadioButton CreateRadioButton(DisplayMode displayMode, bool isChecked) {
            var button = new RadioButton();
            button.Padding = new Thickness(3,0,0,0);
            button.Margin = new Thickness(2);
            button.Content = NormalizeString(displayMode.ToString());
            button.Tag = displayMode;
            button.Click += new RoutedEventHandler(buttonDisplayMode_Click);
            button.IsChecked = isChecked;
            return button;
        }
        
        void CreateDisplayModeSelector() {
            GroupBox groupBox = new GroupBox();
            groupBox.Header = "Show";
            groupBox.HorizontalAlignment = HorizontalAlignment.Center;
            groupBox.VerticalAlignment = VerticalAlignment.Center;
            groupBox.Margin = new Thickness(0);
            groupBox.Padding = new Thickness(3);

            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(CreateRadioButton(DisplayMode.By_Events,true));
            stackPanel.Children.Add(CreateRadioButton(DisplayMode.By_Volume, false));
            stackPanel.Children.Add(CreateRadioButton(DisplayMode.Adoption_Sources, false));
            stackPanel.Children.Add(CreateRadioButton(DisplayMode.Adoption_Targets, false));

            groupBox.Content = stackPanel;
            Grid.SetRow(groupBox, 0);
            Grid.SetRowSpan(groupBox, 2);
            Grid.SetColumn(groupBox, 0);
            Grid.SetColumnSpan(groupBox, 2);
            gridOuter.Children.Add(groupBox);
        }

        string NormalizeString(string s) { return s.Replace("_", " "); }
        
        void buttonDisplayMode_Click(object sender, RoutedEventArgs e) {
            ToggleButton button = sender as ToggleButton;

            displayMode = (DisplayMode)button.Tag;
            RenderRows();
        }

        Collection<TElement> GetElements<TElement>(Grid grid, int row, int column)
           where TElement : UIElement {
            var elements = from UIElement element in grid.Children
               where element is TElement &&
                     Grid.GetRow(element) == row &&
                     Grid.GetColumn(element) == column
               select element as TElement;
            return new Collection<TElement>(elements.ToList());
        }

        void RenderRows() {
            for (int iRow = FirstMatrixRow; iRow < FixtureClasses.Items.Count + FirstMatrixRow; iRow++)
                for (int iCol = FirstMatrixColumn; iCol < FixtureClasses.Items.Count + FirstMatrixColumn; iCol++) {
                    RenderCell(iRow, iCol);
                }
        }

        void RenderCell(int iRow, int iCol) {
            Collection<TextBlock> TextBlocks = GetElements<TextBlock>(gridOuter, iRow, iCol);
            foreach (TextBlock textBlock in TextBlocks)
                RenderCellTextBlock(textBlock);
        }

        void RenderCellTextBlock(TextBlock textBlock) {
            if ((DisplayMode)textBlock.Tag == displayMode)
                textBlock.Visibility = Visibility.Visible;
            else
                textBlock.Visibility = Visibility.Hidden;

        }
    
        void PopulateRowLabel() {
            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = new GridLength(1, GridUnitType.Star); ;
            gridOuter.RowDefinitions.Add(rowDefinition);

            TextBlock textBlock = new TextBlock();
            textBlock.Text = "Actual Class";
            textBlock.FontSize = 12;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.Padding = new Thickness(0, 1, 0, 0);

            Grid.SetRow(textBlock, gridOuter.RowDefinitions.Count - 1);
            Grid.SetColumn(textBlock, 2);
            Grid.SetColumnSpan(textBlock, FixtureClasses.Items.Values.Count);
            gridOuter.Children.Add(textBlock);
        }

        void PopulateColumnLabel() {
            ColumnDefinition columnDefinition = new ColumnDefinition();
            columnDefinition.Width = new GridLength(1, GridUnitType.Star); ;
            gridOuter.ColumnDefinitions.Add(columnDefinition);

            TextBlock textBlock = new TextBlock();
            textBlock.Text = "Predicted Class";
            textBlock.FontSize = 12;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Padding = new Thickness(0, 1, 0, 0);

            textBlock.LayoutTransform = new RotateTransform(-90);

            Grid.SetRow(textBlock, 2);
            Grid.SetColumn(textBlock, gridOuter.ColumnDefinitions.Count - 1);
            Grid.SetRowSpan(textBlock, FixtureClasses.Items.Values.Count);
            gridOuter.Children.Add(textBlock);
        }

        void CreateRowAndColumnHeaders() {
            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = GridLength.Auto;

            gridOuter.RowDefinitions.Add(rowDefinition);
            FirstMatrixRow = gridOuter.RowDefinitions.Count;

            ColumnDefinition columnDefinition = new ColumnDefinition();
            columnDefinition.Width = GridLength.Auto;
            //columnDefinition.Width = new GridLength(1, GridUnitType.Star);
            gridOuter.ColumnDefinitions.Add(columnDefinition);
            FirstMatrixColumn = gridOuter.ColumnDefinitions.Count;
        }
        
        void PopulateRowHeaders() {
            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values)
                PopulateRowHeader(fixtureClass);
        }

        void PopulateRowHeader(FixtureClass fixtureClass) {
            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = new GridLength(1,GridUnitType.Star);
            gridOuter.RowDefinitions.Add(rowDefinition);

            Grid grid = TwGui.FixtureWithImageRight(fixtureClass);

            Grid.SetRow(grid, gridOuter.RowDefinitions.Count - 1);
            Grid.SetColumn(grid, 1);
            gridOuter.Children.Add(grid);
        }

        void PopulateFixturesColumnNoText() {
            ColumnDefinition columnDefinition = new ColumnDefinition();
            columnDefinition.Width = new GridLength(1, GridUnitType.Star);
            gridOuter.ColumnDefinitions.Add(columnDefinition);

            int iRow = FirstMatrixRow;
            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values)
                PopulateColumnHeaderNoText(fixtureClass, iRow++);
        }

        void PopulateColumnHeaderNoText(FixtureClass fixtureClass, int iRow) {
            Grid grid = TwGui.FixtureWithNoLabel(fixtureClass);

            Grid.SetRow(grid, iRow);
            Grid.SetColumn(grid, gridOuter.ColumnDefinitions.Count - 1);
            gridOuter.Children.Add(grid);
        }

        void PopulateFixturesRowNoText() {
            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = new GridLength(1, GridUnitType.Star);
            gridOuter.RowDefinitions.Add(rowDefinition);

            int iColumn = FirstMatrixColumn;
            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values)
                PopulateRowHeaderNoText(fixtureClass, iColumn++);
        }

        void PopulateRowHeaderNoText(FixtureClass fixtureClass, int iColumn) {
            Grid grid = TwGui.FixtureWithNoLabel(fixtureClass);

            Grid.SetRow(grid, gridOuter.RowDefinitions.Count - 1);
            Grid.SetColumn(grid, iColumn);
            gridOuter.Children.Add(grid);
        }

        void PopulateColumnHeaders() {
            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values)
                PopulateColumnHeader(fixtureClass);
        }

        void PopulateColumnHeader(FixtureClass fixtureClass) {
            ColumnDefinition columnDefinition = new ColumnDefinition();
            columnDefinition.Width = new GridLength(1,GridUnitType.Star);
            gridOuter.ColumnDefinitions.Add(columnDefinition);

            var fixtureImage = new ShortFixtureLabel(fixtureClass);

            Grid.SetRow(fixtureImage, 1);
            Grid.SetColumn(fixtureImage, gridOuter.ColumnDefinitions.Count - 1);
            gridOuter.Children.Add(fixtureImage);
        }

        void PopulateRows() {
            int iRow = FirstMatrixRow;
            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values) {
                PopulateRow(iRow++, fixtureClass);
            }
        }

        void PopulateRow(int iRow, FixtureClass fixtureClassRow) {
            int iCol = FirstMatrixColumn;

            foreach (FixtureClass fixtureClassColumn in FixtureClasses.Items.Values)
                PopulateCell(iRow, iCol++, fixtureClassRow, fixtureClassColumn);
        }

        string FormatVolume { get { return "0"; } }
        string FormatPercent { get { return "0.0%"; } }
        string FormatRatio { get { return "0.00"; } }
        string FormatCount { get { return "0"; } }

        void SetPredictedTotalInstancesToolTip(FrameworkElement element, FixtureClass fixtureClass) {
            element.ToolTip = new EventsProperties(
                fixtureClass, null, Trace.ClassificationAggregationPredicted.FixtureSummaries[fixtureClass].Events,
                Trace.ClassificationAggregationPredicted.FixtureSummaries[fixtureClass].Events.Count);
            SetToolTipService(element);
        }

        void SetActualTotalInstancesToolTip(FrameworkElement element, FixtureClass fixtureClass) {
            element.ToolTip = new EventsProperties(
                null, fixtureClass, Trace.ClassificationAggregationActual.FixtureSummaries[fixtureClass].Events, 
                Trace.ClassificationAggregationActual.FixtureSummaries[fixtureClass].Events.Count);
            SetToolTipService(element);
        }

        void SetCellToolTip(FrameworkElement element, FixtureClass fixtureClassRow, FixtureClass fixtureClassColumn) {
            element.ToolTip = new EventsProperties(
                fixtureClassRow, fixtureClassColumn, Trace.ConfusionMatrix[fixtureClassRow][fixtureClassColumn].Events, Trace.ConfusionMatrix[fixtureClassRow][fixtureClassColumn].Count);
            SetToolTipService(element);
        }

        void SetToolTipService(FrameworkElement element) {
            ToolTipService.SetShowDuration(element, 30000);
            ToolTipService.SetInitialShowDelay(element, 0);
        }
        
        void SetAdoptionSourcesCellToolTip(FrameworkElement element, FixtureClass fixtureClassRow, FixtureClass fixtureClassColumn) {
            element.ToolTip = Trace.ConfusionMatrix[fixtureClassRow][fixtureClassColumn].AdoptionSources.Count.ToString() + " " + fixtureClassColumn.FriendlyName + " events predicted by Classifier+Adopter as " + 
                fixtureClassRow.FriendlyName + " were Sources during Adoption.";
            SetToolTipService(element);
        }

        void SetAdoptionTargetsCellToolTip(FrameworkElement element, FixtureClass fixtureClassRow, FixtureClass fixtureClassColumn) {
            element.ToolTip = Trace.ConfusionMatrix[fixtureClassRow][fixtureClassColumn].AdoptionTargets.Count.ToString() + " " + fixtureClassColumn.FriendlyName + " events predicted by Classifier+Adopter as " +
                fixtureClassRow.FriendlyName + " were Targets during Adoption.";
            SetToolTipService(element);
        }

        void PopulateCell(int iRow, int iColumn, FixtureClass fixtureClassRow, FixtureClass fixtureClassColumn) {

            TextBlock textBlock;

            textBlock = FormatCell(Trace.ConfusionMatrix[fixtureClassRow][fixtureClassColumn].Count);
            textBlock.FontWeight = iRow == iColumn ? FontWeights.Bold : FontWeights.Normal;
            textBlock.Tag = DisplayMode.By_Events;
            textBlock.Visibility = Visibility.Hidden;

            if (Trace.ConfusionMatrix[fixtureClassRow][fixtureClassColumn].Count > 0)
                SetCellToolTip(textBlock, fixtureClassRow, fixtureClassColumn);
            
            Grid.SetRow(textBlock, iRow);
            Grid.SetColumn(textBlock, iColumn);
            gridOuter.Children.Add(textBlock);

            textBlock = FormatCell(Trace.ConfusionMatrix[fixtureClassRow][fixtureClassColumn].Volume, FormatVolume);
            textBlock.FontWeight = iRow == iColumn ? FontWeights.Bold : FontWeights.Normal;
            textBlock.Tag = DisplayMode.By_Volume;
            textBlock.Visibility = Visibility.Hidden;

            if (Trace.ConfusionMatrix[fixtureClassRow][fixtureClassColumn].Count > 0)
                SetCellToolTip(textBlock, fixtureClassRow, fixtureClassColumn);

            Grid.SetRow(textBlock, iRow);
            Grid.SetColumn(textBlock, iColumn);
            gridOuter.Children.Add(textBlock);

            textBlock = FormatCell(Trace.ConfusionMatrix[fixtureClassRow][fixtureClassColumn].AdoptionSources.Count);
            textBlock.FontWeight = iRow == iColumn ? FontWeights.Bold : FontWeights.Normal;
            textBlock.Tag = DisplayMode.Adoption_Sources;
            textBlock.Visibility = Visibility.Hidden;

            if (Trace.ConfusionMatrix[fixtureClassRow][fixtureClassColumn].AdoptionSources.Count > 0)
                SetAdoptionSourcesCellToolTip(textBlock, fixtureClassRow, fixtureClassColumn);

            Grid.SetRow(textBlock, iRow);
            Grid.SetColumn(textBlock, iColumn);
            gridOuter.Children.Add(textBlock);

            textBlock = FormatCell(Trace.ConfusionMatrix[fixtureClassRow][fixtureClassColumn].AdoptionTargets.Count);
            textBlock.FontWeight = iRow == iColumn ? FontWeights.Bold : FontWeights.Normal;
            textBlock.Tag = DisplayMode.Adoption_Targets;
            textBlock.Visibility = Visibility.Hidden;

            if (Trace.ConfusionMatrix[fixtureClassRow][fixtureClassColumn].AdoptionTargets.Count > 0)
                SetAdoptionTargetsCellToolTip(textBlock, fixtureClassRow, fixtureClassColumn);

            Grid.SetRow(textBlock, iRow);
            Grid.SetColumn(textBlock, iColumn);
            gridOuter.Children.Add(textBlock);

        }

        TextBlock FormatCell(int value) {
            return FormatCell(value, string.Empty);
        }

        TextBlock FormatCell(double value, string format) {
            return FormatCell(value, format, FontWeights.Normal);
        }

        TextBlock FormatCell(double value, string format, FontWeight fontWeight) {
            TextBlock textBlock = new TextBlock();

            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.FontWeight = fontWeight;
            textBlock.Foreground = (value == 0) ? Brushes.LightGray : Brushes.Black;
            textBlock.Padding = new Thickness(2);
            textBlock.Text = double.IsNaN(value) ? string.Empty : value.ToString(format);

            return textBlock;
        }

        int FirstMatrixRow { get; set; }
        int FirstMatrixColumn { get; set; }
        int FinalRow { get { return gridOuter.RowDefinitions.Count - 1; } }

        void PopulateStatistics() {
            PopulateRowHeaderStatistic("Events");
            PopulateColumnHeaderStatistic("Events", CreateToolTipStatisticLabel("Events", "The number of events predicted as this fixture."));
            PopulateInstances();

            PopulateRowHeaderStatistic("Traces");
            PopulateColumnHeaderStatistic("Traces", CreateToolTipStatisticLabel("Traces", "The number of traces in which at least one event was predicted as this fixture."));
            PopulateTraces();

            PopulateColumnHeaderStatistic("True Positives", CreateToolTipStatisticLabel("True Positives", "The number of events correctly predicted as this fixture."));
            PopulateTruePositives();
            PopulateColumnHeaderStatistic("True Positive Rate", CreateToolTipStatisticLabel("True Positive Rate", "True Positives", "True Positives + False Negatives", "Out of all events which are actually this fixture, the percentage correctly predicted as this fixture. Also known as Sensitivity or Recall."));
            PopulateTruePositiveRate();

            PopulateColumnHeaderStatistic("False Positives", CreateToolTipStatisticLabel("False Positives", "The number of events incorrectly predicted as this fixture."));
            PopulateFalsePositives();

            PopulateColumnHeaderStatistic("False Negatives", CreateToolTipStatisticLabel("False Negatives", "The number of events incorrectly predicted as not this fixture."));
            PopulateFalseNegatives();

            PopulateRowHeaderStatistic("Volume");
            PopulateColumnHeaderStatistic("Volume", CreateToolTipStatisticLabel("Volume", "The volume of events predicted as this fixture."));
            PopulateVolumes();

            PopulateRowHeaderStatistic("Volume Percent");
            PopulateColumnHeaderStatistic("Volume Percent", CreateToolTipStatisticLabel("Volume Percent", "Out of the volume of all events, the percentage (of volume) predicted as this fixture."));
            PopulateVolumePercents();

            PopulateColumnHeaderStatistic("True Pos Vol Rate", CreateToolTipStatisticLabel("True Positive Rate Volume", "Out of the volume of all events which are actually this fixture, the percentage (of volume) correctly predicted as this fixture."));
            PopulateTruePositiveRateVolume();
        }

        void PopulateRowHeaderStatistic(string text) {
            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = new GridLength(1, GridUnitType.Star);
            gridOuter.RowDefinitions.Add(rowDefinition);

            Grid grid = TwGui.FixtureWithNoImageRight(text);
            
            Border borderOuter = new Border();
            borderOuter.BorderBrush = Brushes.LightGray;

            borderOuter.BorderThickness = new Thickness(0, 0, 1, 0);

            borderOuter.Child = grid;

            Grid.SetRow(borderOuter, gridOuter.RowDefinitions.Count - 1);
            Grid.SetColumn(borderOuter, 1);
            gridOuter.Children.Add(borderOuter);
        }

        void PopulateColumnHeaderStatistic(string text) {
            PopulateColumnHeaderStatistic(text,null);
        }

        void PopulateColumnHeaderStatistic(string text, UIElement toolTip) {
            ColumnDefinition columnDefinition = new ColumnDefinition();

            gridOuter.ColumnDefinitions.Add(columnDefinition);

            Grid grid = TwGui.FixtureWithNoImageBottom(text);

            Border borderOuter = new Border();
            borderOuter.BorderBrush = Brushes.LightGray;

            borderOuter.BorderThickness = new Thickness(0, 0, 0, 1);
            
            borderOuter.Child = grid;
            borderOuter.ToolTip = toolTip;

            Grid.SetRow(borderOuter, 1);
            Grid.SetColumn(borderOuter, gridOuter.ColumnDefinitions.Count - 1);
            gridOuter.Children.Add(borderOuter);
        }

        void PopulateAggregatePanel() {
            PopulateClassifierCorrectInstancesAccuracy(0, 0);
            PopulateClassifierIncorrectInstancesAccuracy(0, 2);
            PopulateClassifierCorrectVolumeAccuracy(0, 4);
            PopulateClassifierIncorrectVolumeAccuracy(0, 6);

            PopulateAdopterCorrectInstancesAccuracy(1, 0);
            PopulateAdopterIncorrectInstancesAccuracy(1, 2);
        }

        void PopulateTotalStatisticShowBorder(double statistic, string format, int row) {
            PopulateTotalStatistic(statistic, format, true, row);
        }

        void PopulateInstances() {
            int iRow = 2;
            FrameworkElement element;
            foreach (FixtureClass fixtureClassRow in FixtureClasses.Items.Values) {
                element = PopulateRowInstances(iRow++, fixtureClassRow);
                SetPredictedTotalInstancesToolTip(element, fixtureClassRow);
            }

            int iCol = 2;
            foreach (FixtureClass fixtureClassColumn in FixtureClasses.Items.Values) {
                element = PopulateColumnStatistic(iCol++, Trace.ClassificationAggregationActual.FixtureSummaries[fixtureClassColumn].Count, string.Empty);
                SetActualTotalInstancesToolTip(element, fixtureClassColumn); 
            }

            PopulateTotalStatistic(Trace.ConfusionMatrixStatistics.TotalInstanceCount, string.Empty, FinalRow);
        }

        FrameworkElement PopulateRowInstances(int iRow, FixtureClass fixtureClass) {
            TextBlock textBlock = FormatCell(Trace.ClassificationAggregationPredicted.FixtureSummaries[fixtureClass].Count);

            Border border = new Border();
            border.BorderBrush = Brushes.LightGray;
            border.BorderThickness = new Thickness(0);
            border.Child = textBlock;

            Grid.SetRow(border, iRow);
            Grid.SetColumn(border, gridOuter.ColumnDefinitions.Count - 1);
            gridOuter.Children.Add(border);

            return border;
        }
        
        void PopulateTraces() {
            int iRow = 2;
            foreach (FixtureClass fixtureClassRow in FixtureClasses.Items.Values)
                PopulateRowStatistic(iRow++, Trace.ClassificationAggregationPredicted.FixtureSummaries[fixtureClassRow].TraceCount, string.Empty);

            int iCol = 2;
            foreach (FixtureClass fixtureClassColumn in FixtureClasses.Items.Values)
                PopulateColumnStatistic(iCol++, Trace.ClassificationAggregationActual.FixtureSummaries[fixtureClassColumn].TraceCount, string.Empty);

            PopulateTotalStatistic(Trace.Count, string.Empty,FinalRow);
        }

        void PopulateVolumes() {
            int iRow = 2;
            foreach (FixtureClass fixtureClassRow in FixtureClasses.Items.Values)
                PopulateRowStatistic(iRow++, Trace.ClassificationAggregationPredicted.FixtureSummaries[fixtureClassRow].Volume, FormatVolume);

            int iCol = 2;
            foreach (FixtureClass fixtureClassColumn in FixtureClasses.Items.Values)
                PopulateColumnStatistic(iCol++, Trace.ClassificationAggregationActual.FixtureSummaries[fixtureClassColumn].Volume, FormatVolume);

            PopulateTotalStatistic(Math.Round(Trace.ClassificationAggregationPredicted.Events.Volume,1), FormatVolume, FinalRow);
        }

        void PopulateVolumePercents() {
            int iRow = 2;
            foreach (FixtureClass fixtureClassRow in FixtureClasses.Items.Values)
                PopulateRowStatistic(iRow++, Trace.ClassificationAggregationPredicted.FixtureSummaries[fixtureClassRow].Volume
                    / Trace.ClassificationAggregationPredicted.Events.Volume, FormatPercent);

            int iCol = 2;
            foreach (FixtureClass fixtureClassColumn in FixtureClasses.Items.Values)
                PopulateColumnStatistic(iCol++, Trace.ClassificationAggregationActual.FixtureSummaries[fixtureClassColumn].Volume
                    / Trace.ClassificationAggregationActual.Events.Volume, FormatPercent);
        }

        void PopulateRowStatistic(int iRow, double value, string format) {
            TextBlock textBlock = FormatCell(value,format);

            Border border = new Border();
            border.BorderBrush = Brushes.LightGray;
            border.BorderThickness = new Thickness(0);
            border.Child = textBlock;

            Grid.SetRow(border, iRow);
            Grid.SetColumn(border, gridOuter.ColumnDefinitions.Count - 1);
            gridOuter.Children.Add(border);
        }

        FrameworkElement PopulateColumnStatistic(int iColumn, double value, string format) {
            TextBlock textBlock = FormatCell(value, format);

            Border border = new Border();
            border.BorderBrush = Brushes.LightGray;

            border.BorderThickness = new Thickness(0);
            
            border.Child = textBlock;

            Grid.SetRow(border, gridOuter.RowDefinitions.Count - 1);
            Grid.SetColumn(border, iColumn);
            gridOuter.Children.Add(border);

            return border;
        }

        void PopulateTruePositives() {
            int iRow = 2;
            foreach (FixtureClass fixtureClassRow in FixtureClasses.Items.Values)
                PopulateRowStatistic(iRow++, Trace.ConfusionMatrixStatistics.BinaryClassifiersStatistics[fixtureClassRow].TruePositives, FormatCount);
        }

        void PopulateTruePositiveRate() {
            int iRow = 2;
            foreach (FixtureClass fixtureClassRow in FixtureClasses.Items.Values)
                PopulateRowStatistic(iRow++, Trace.ConfusionMatrixStatistics.BinaryClassifiersStatistics[fixtureClassRow].TruePositiveRate, FormatPercent); 
        }

        void PopulateTruePositiveRateVolume() {
            int iRow = 2;
            foreach (FixtureClass fixtureClassRow in FixtureClasses.Items.Values)
                PopulateRowStatistic(iRow++, Trace.ConfusionMatrixStatistics.BinaryClassifiersStatistics[fixtureClassRow].TruePositiveRateVolume, FormatPercent);
        }

        void PopulateFalsePositives() {
            int iRow = 2;
            foreach (FixtureClass fixtureClassRow in FixtureClasses.Items.Values)
                PopulateRowStatistic(iRow++, Trace.ConfusionMatrixStatistics.BinaryClassifiersStatistics[fixtureClassRow].FalsePositives, FormatCount);
        }

        void PopulateFalseNegatives() {
            int iRow = 2;
            foreach (FixtureClass fixtureClassRow in FixtureClasses.Items.Values)
                PopulateRowStatistic(iRow++, Trace.ConfusionMatrixStatistics.BinaryClassifiersStatistics[fixtureClassRow].FalseNegatives, FormatCount);
        }

        void PopulatePositivePredictiveValue() {
            int iRow = 2;
            foreach (FixtureClass fixtureClassRow in FixtureClasses.Items.Values)
                PopulateRowStatistic(iRow++, Trace.ConfusionMatrixStatistics.BinaryClassifiersStatistics[fixtureClassRow].PositivePredictiveValue, FormatPercent);
        }

        void PopulateNegativePredictiveValue() {
            int iRow = 2;
            foreach (FixtureClass fixtureClassRow in FixtureClasses.Items.Values)
                PopulateRowStatistic(iRow++, Trace.ConfusionMatrixStatistics.BinaryClassifiersStatistics[fixtureClassRow].NegativePredictiveValue, FormatPercent);
        }

        void PopulatePositiveLikelihoodRatio() {
            int iRow = 2;
            foreach (FixtureClass fixtureClassRow in FixtureClasses.Items.Values)
                PopulateRowStatistic(iRow++, Trace.ConfusionMatrixStatistics.BinaryClassifiersStatistics[fixtureClassRow].PositiveLikelihoodRatio, FormatRatio);
        }

        void PopulateNegativeLikelihoodRatio() {
            int iRow = 2;
            foreach (FixtureClass fixtureClassRow in FixtureClasses.Items.Values)
                PopulateRowStatistic(iRow++, Trace.ConfusionMatrixStatistics.BinaryClassifiersStatistics[fixtureClassRow].NegativeLikelihoodRatio, FormatRatio);
        }

        void PopulateFMeasure() {
            int iRow = 2;
            foreach (FixtureClass fixtureClassRow in FixtureClasses.Items.Values)
                PopulateRowStatistic(iRow++, Trace.ConfusionMatrixStatistics.BinaryClassifiersStatistics[fixtureClassRow].FMeasure, FormatPercent);
        }

        void PopulateMatthewsCorrelationCoefficient() {
            int iRow = 2;
            foreach (FixtureClass fixtureClassRow in FixtureClasses.Items.Values)
                PopulateRowStatistic(iRow++, Trace.ConfusionMatrixStatistics.BinaryClassifiersStatistics[fixtureClassRow].MatthewsCorrelationCoefficient, FormatRatio);
        }

        void PopulateTotalStatistic(double statistic, string format, int row) {
            PopulateTotalStatistic(statistic, format, false, row, FontWeights.Normal);
        }

        void PopulateTotalStatistic(double statistic, string format, int row, FontWeight fontWeight) {
            PopulateTotalStatistic(statistic, format, false, row, fontWeight);
        }

        void PopulateTotalStatistic(double statistic, string format, bool showBorder, int row) {
            PopulateTotalStatistic(statistic, format, showBorder, row, FontWeights.Normal);
        }

        void PopulateTotalStatistic(double statistic, string format, bool showBorder, int row, FontWeight fontWeight) {
            TextBlock textBlock = FormatCell(statistic, format, fontWeight);

            Border border = new Border();
            border.BorderBrush = Brushes.LightGray;

            if (showBorder)
                border.BorderThickness = new Thickness(1, 1, 0, 0);
            else
                border.BorderThickness = new Thickness(0, 0, 0, 0);

            border.Child = textBlock;

            Grid.SetRow(border, row);
            Grid.SetColumn(border, gridOuter.ColumnDefinitions.Count - 1);
            gridOuter.Children.Add(border);
        }

        void PopulateAggregateStatistic(int row, int column, string header, string content) {
            PopulateAggregateStatistic(row, column, header, content, null);
        }

        void PopulateAggregateStatistic(int row, int column, string header, string content, UIElement toolTip) {
            TextBlock textBlock;

            textBlock = new TextBlock();
            textBlock.Text = header + ": ";
            textBlock.Padding = new Thickness(2,2,2,2);
            textBlock.HorizontalAlignment = HorizontalAlignment.Right;

            if (toolTip != null) {
                textBlock.ToolTip = toolTip;
                SetToolTipService(textBlock);
            }
            
            Grid.SetRow(textBlock, row);
            Grid.SetColumn(textBlock, column);
            aggregateGrid.Children.Add(textBlock);

            textBlock = new TextBlock();
            textBlock.Text = content;
            textBlock.Padding = new Thickness(0, 2, 16, 2);

            if (toolTip != null) {
                textBlock.ToolTip = toolTip;
                SetToolTipService(textBlock);
            }

            Grid.SetRow(textBlock, row);
            Grid.SetColumn(textBlock, column+1);
            aggregateGrid.Children.Add(textBlock);
        }

        void PopulateAggregateStatistic(int row, int column, string header, double value, string format) {
            PopulateAggregateStatistic(row, column, header, value.ToString(format));
        }

        void PopulateClassifierCorrectInstancesAccuracy(int row, int column) {
            PopulateAggregateStatistic(row, column, "Classifier Correct Events", Trace.ConfusionMatrixStatistics.TruePositives.ToString() 
                + " (" +
                Math.Round(Trace.ConfusionMatrixStatistics.CorrectInstanceAccuracy, 4).ToString(FormatPercent)
                + ")");
        }

        void PopulateClassifierIncorrectInstancesAccuracy(int row, int column) {
            PopulateAggregateStatistic(row, column, "Classifier Incorrect Events", Trace.ConfusionMatrixStatistics.IncorrectInstanceCount.ToString()
                + " (" +
                Math.Round(Trace.ConfusionMatrixStatistics.IncorrectInstanceAccuracy, 4).ToString(FormatPercent)
                + ")");
        }

        void PopulateClassifierCorrectVolumeAccuracy(int row, int column) {
            PopulateAggregateStatistic(row, column, "Classifier Correct Volume", Trace.ConfusionMatrixStatistics.TruePositivesVolume.ToString(FormatCount)
                + " (" +
                Math.Round(Trace.ConfusionMatrixStatistics.VolumeAccuracy, 4).ToString(FormatPercent)
                + ")");
        }

        void PopulateClassifierIncorrectVolumeAccuracy(int row, int column) {
            PopulateAggregateStatistic(row, column, "Classifier Incorrect Volume", (Trace.ConfusionMatrixStatistics.TotalVolume - Trace.ConfusionMatrixStatistics.TruePositivesVolume).ToString(FormatCount)
                + " (" +
                Math.Round(1.0 - Trace.ConfusionMatrixStatistics.VolumeAccuracy, 4).ToString(FormatPercent)
                + ")");
        }

        void PopulateAdopterCorrectInstancesAccuracy(int row, int column) {
            var textBlock = new TextBlock();
            textBlock.Text = "Events correctly classified by Classifier+Adopter";
            PopulateAggregateStatistic(row, column, "Classifier+Adopter Correct Events", Trace.ConfusionMatrixStatistics.AdopterCorrectInstanceCount.ToString(FormatCount)
                + " (" +
                Math.Round(Trace.ConfusionMatrixStatistics.AdopterCorrectInstanceAccuracy, 4).ToString(FormatPercent)
                + ")",textBlock);
        }

        void PopulateAdopterIncorrectInstancesAccuracy(int row, int column) {
            var textBlock = new TextBlock();
            textBlock.Text = "Events that must be manually classified by Analyst using this Classifier+Adopter";
            PopulateAggregateStatistic(row, column, "Classifier+Adopter Incorrect Events", Trace.ConfusionMatrixStatistics.AdopterIncorrectInstanceCount.ToString(FormatCount)
                + " (" +
                Math.Round(Trace.ConfusionMatrixStatistics.AdopterIncorrectInstanceAccuracy, 4).ToString(FormatPercent)
                + ")",textBlock);
        }

        UIElement CreateToolTipStatisticLabel(string title, string detail) {
            return CreateToolTipStatisticLabel(title, null, null, detail);
        }

        UIElement CreateToolTipStatisticLabel(string title, string numerator, string denominator, string detail) {
            DockPanel panel = new DockPanel();
            panel.MinWidth = 192;
            SetToolTipService(panel);

            TextBlock textBlock;

            textBlock = new TextBlock();
            textBlock.Text = title;
            textBlock.FontWeight = FontWeights.Bold;
            textBlock.Margin = new Thickness(2, 0, 2, 6);
            DockPanel.SetDock(textBlock, Dock.Top);
            panel.Children.Add(textBlock);

            if (numerator != null && denominator != null) {
                textBlock = new TextBlock();
                textBlock.Text = numerator;
                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                DockPanel.SetDock(textBlock, Dock.Top);
                panel.Children.Add(textBlock);

                Rectangle rectangle = new Rectangle();
                rectangle.Fill = Brushes.DarkGray;
                rectangle.Height = 1;
                rectangle.Margin = new Thickness(6, 2, 6, 2);
                rectangle.HorizontalAlignment = HorizontalAlignment.Stretch;
                DockPanel.SetDock(rectangle, Dock.Top);
                panel.Children.Add(rectangle);

                textBlock = new TextBlock();
                textBlock.Text = denominator;
                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                DockPanel.SetDock(textBlock, Dock.Top);
                panel.Children.Add(textBlock);
            }

            textBlock = new TextBlock();
            textBlock.Text = detail;
            textBlock.MaxWidth = 192;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Margin = new Thickness(2, 6, 0, 2);
            DockPanel.SetDock(textBlock, Dock.Top);
            panel.Children.Add(textBlock);

            return panel;
        }

        DockPanel WrapWithLabel(UIElement content, string analyzer1, UIElement analyzer1ToolTip, string analyzer2, UIElement analyzer2ToolTip, string fileName, UIElement filesToolTip) {
            DockPanel dockPanelOuter = new DockPanel();

            DockPanel dockPanelInner = new DockPanel();
            dockPanelInner.LastChildFill = false;
            dockPanelInner.Margin = new Thickness(0);

            TextBlock textBlock;

            textBlock = new TextBlock();
            textBlock.Text = analyzer1;
            textBlock.ToolTip = analyzer1ToolTip;
            SetToolTipService(textBlock);
            textBlock.HorizontalAlignment = HorizontalAlignment.Left;

            DockPanel.SetDock(textBlock, Dock.Left);
            dockPanelInner.Children.Add(textBlock);

            textBlock = new TextBlock();
            textBlock.Text = analyzer2;
            textBlock.ToolTip = analyzer2ToolTip;
            SetToolTipService(textBlock);
            textBlock.HorizontalAlignment = HorizontalAlignment.Left;

            textBlock.Padding = new Thickness(16,0,0,0);

            DockPanel.SetDock(textBlock, Dock.Left);
            dockPanelInner.Children.Add(textBlock);
            textBlock = new TextBlock();
            textBlock.Text = fileName;
            textBlock.ToolTip = filesToolTip;
            SetToolTipService(textBlock);
            textBlock.HorizontalAlignment = HorizontalAlignment.Right;

            DockPanel.SetDock(textBlock, Dock.Right);
            dockPanelInner.Children.Add(textBlock);

            DockPanel.SetDock(dockPanelInner, Dock.Top);
            dockPanelOuter.Children.Add(dockPanelInner);

            DockPanel.SetDock(content, Dock.Top);
            dockPanelOuter.Children.Add(content);

            return dockPanelOuter;
        }

        UIElement CreateElementFiles(List<string> files) {
            UniformGrid grid = new UniformGrid();
            grid.Columns = 5;

            foreach (string file in files) {
                TextBlock textBlock = new TextBlock();
                textBlock.Padding = new Thickness(3);
                textBlock.Text = System.IO.Path.GetFileNameWithoutExtension(file);
                grid.Children.Add(textBlock);
            }
            return grid;
        }
    }
}
