using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

using TraceWizard.Entities;
using TraceWizard.Services;
using TraceWizard.Reporters;

namespace TraceWizard.TwApp {
    public class AggregateReportPanel : UserControl {

        public AggregateReportPanel(Analyses analyses) {
            Initialize();

            int row = 0;
            int column = 0;
            DisplaySummary(analyses, row, column);
        }

        void Initialize() {
            this.Margin = new Thickness(6, 6, 6, 6);
            this.VerticalAlignment = VerticalAlignment.Top;
        }

        void DisplaySummary(Analyses analyses, int rowExternal, int columnExternal) {
            var stackPanel = new StackPanel();
            this.Content = stackPanel;

            var textBlock = new TextBlock();
            textBlock.Text = "In this unfinished report, we display aggregate statistics (e.g., average volume of traces).";
            textBlock.FontWeight = FontWeights.Bold;
            stackPanel.Children.Add(textBlock);
            
            var grid = new Grid();
            grid.HorizontalAlignment = HorizontalAlignment.Left;
            grid.Margin = new Thickness(6, 6, 6, 6);
            stackPanel.Children.Add(grid);

            int row = 0;

            ColumnDefinition columnDefinition = new ColumnDefinition();

            columnDefinition.Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(columnDefinition);

            columnDefinition = new ColumnDefinition();
            columnDefinition.Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(columnDefinition);

            RowDefinition rowDefinition;

            rowDefinition = new RowDefinition();
            grid.RowDefinitions.Add(rowDefinition);

            textBlock = new TextBlock();
            textBlock.Text = "KeyCodes" + ":";
            textBlock.HorizontalAlignment = HorizontalAlignment.Right;
            textBlock.Padding = new Thickness(0, 0, 6, 0);

            Grid.SetRow(textBlock, row);
            Grid.SetColumn(textBlock, 0);
            grid.Children.Add(textBlock);

            textBlock = new TextBlock();

            string keyCodes = string.Empty;
            foreach (Analysis analysis in analyses) {
                keyCodes += analysis.KeyCode + " ";
            }

            textBlock.Text = keyCodes;
            textBlock.FontWeight = FontWeights.Normal;
            textBlock.Padding = new Thickness(6, 0, 0, 0);

            Grid.SetRow(textBlock, row++);
            Grid.SetColumn(textBlock, 1);
            grid.Children.Add(textBlock);

            rowDefinition = new RowDefinition();
            grid.RowDefinitions.Add(rowDefinition);

            textBlock = new TextBlock();
            textBlock.Text = "Number of Traces" + ":";
            textBlock.HorizontalAlignment = HorizontalAlignment.Right;
            textBlock.Padding = new Thickness(0, 0, 6, 0);

            Grid.SetRow(textBlock, row);
            Grid.SetColumn(textBlock, 0);
            grid.Children.Add(textBlock);

            textBlock = new TextBlock();
            textBlock.Text = (analyses.Count).ToString();
            textBlock.FontWeight = FontWeights.Normal;
            textBlock.Padding = new Thickness(6, 0, 0, 0);

            Grid.SetRow(textBlock, row++);
            Grid.SetColumn(textBlock, 1);
            grid.Children.Add(textBlock);

            rowDefinition = new RowDefinition();
            grid.RowDefinitions.Add(rowDefinition);

            textBlock = new TextBlock();
            textBlock.Text = "Average volume" + ":";
            textBlock.HorizontalAlignment = HorizontalAlignment.Right;
            textBlock.Padding = new Thickness(0, 0, 6, 0);

            Grid.SetRow(textBlock, row);
            Grid.SetColumn(textBlock, 0);
            grid.Children.Add(textBlock);

            textBlock = new TextBlock();
            textBlock.Text = (analyses.VolumeAverage).ToString("0.0");
            textBlock.FontWeight = FontWeights.Normal;
            textBlock.Padding = new Thickness(6, 0, 0, 0);

            Grid.SetRow(textBlock, row++);
            Grid.SetColumn(textBlock, 1);
            grid.Children.Add(textBlock);

            Grid.SetRow(grid, rowExternal);
            Grid.SetColumn(grid, columnExternal);
        }
    }

    public class AggregateReporter : IProgressOperation, Reporter {

        List<string> analysisFiles;

        Analyses analyses;
        
        string s = string.Empty;

        AggregateReportPanel panel = null;

        public UIElement Report() {

            analysisFiles = TwFile.GetAnalysisFilesIncludingZipped();
            if (analysisFiles.Count != 0) {
                InitProgressWindow();
                ReportDone();
            }

            return panel;
        }

        void InitProgressWindow() {
            this._total = 0;
            this._current = 0;
            this._isCancelationPending = false;

            this._keyCode = null;

            ProgressWindow progressWindow = new ProgressWindow(this);
            progressWindow.Topmost = false;
            progressWindow.Owner = Application.Current.MainWindow;
            progressWindow.ShowInTaskbar = false;
            progressWindow.ShowDialog();
        }

        string GetKeyCode(string fileName) {
            return System.IO.Path.GetFileNameWithoutExtension(fileName);
        }

        void ReportLow() {

            this.Total = analysisFiles.Count;

            analyses = new Analyses();            
            
            foreach (string analysisFile in analysisFiles) {
                if (this._isCancelationPending == true) break;

                ++this.Current;
                this.KeyCode = GetKeyCode(analysisFile);

                Analysis analysis = Services.TwServices.CreateAnalysis(analysisFile);

                analyses.Add(analysis); 
            }

            analyses.Update();
        }

        void ReportDone() {
            panel = new AggregateReportPanel(analyses);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e) {
            ReportLow();
        }

        private int _total;
        private int _current;
        private bool _isCancelationPending;

        private string _keyCode;

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            OnComplete(EventArgs.Empty);
        }

        protected virtual void OnProgressChanged(EventArgs e) {
            if (this.ProgressChanged != null) {
                this.ProgressChanged(this, e);
            }
        }

        protected virtual void OnProgressTotalChanged(EventArgs e) {
            if (this.ProgressTotalChanged != null) {
                this.ProgressTotalChanged(this, e);
            }
        }

        protected virtual void OnComplete(EventArgs e) {
            if (this.Complete != null) {
                this.Complete(this, e);
            }
        }

        public int Total {
            get {
                return this._total;
            }
            private set {
                this._total = value;
                OnProgressTotalChanged(EventArgs.Empty);
            }
        }

        public int Current {
            get {
                return this._current;
            }
            private set {
                this._current = value;
                OnProgressChanged(EventArgs.Empty);
            }
        }

        public string KeyCode {
            get {
                return this._keyCode;
            }
            private set {
                this._keyCode = value;
                OnProgressChanged(EventArgs.Empty);
            }
        }

        public void Start() {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync();
        }

        public void CancelAsync() {
            this._isCancelationPending = true;
        }

        public event EventHandler ProgressChanged;
        public event EventHandler ProgressTotalChanged;
        public event EventHandler Complete;
    }
}