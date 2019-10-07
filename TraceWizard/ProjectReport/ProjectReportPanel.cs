using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Controls; // has DataGrid
using System.Windows.Media;
using System.Data;

// using Microsoft.Windows.Controls; // has DataGrid

using TraceWizard.Entities;
using TraceWizard.Services;
using TraceWizard.Reporters;

using Com.StellmanGreene.CSVReader;

namespace TraceWizard.TwApp {
    public class ProjectReportPanel : System.Windows.Controls.DataGrid {

        public ProjectReportPanel(string fileName) {
            Initialize();

            DataTable table = CSVReader.ReadCSVFile(fileName, true);
            this.ItemsSource = table.DefaultView;
        }

        void Initialize() {
            this.Margin = new Thickness(6, 6, 6, 6);
            this.VerticalAlignment = VerticalAlignment.Top;

            this.AutoGenerateColumns = true;
        }
    }

    public class ProjectReportReporter : IProgressOperation, Reporter {

        string s = string.Empty;

        List<string> analysisFiles;
        string aggregateFile;

        ProjectReportSelector reportSelector;
        ProjectReportAttributes attributes;

        ProjectReportPanel panel = null;

        public UIElement Report() {

            reportSelector = new ProjectReportSelector(attributes = new ProjectReportAttributes());
            reportSelector.Owner = Application.Current.MainWindow;
            reportSelector.ShowDialog();

            if (!reportSelector.DialogResult.HasValue || reportSelector.DialogResult.Value == false)
                return panel;

            analysisFiles = TwFile.GetAnalysisFilesIncludingZipped();
            if (analysisFiles.Count == 0)
                return panel;

            aggregateFile = TwFile.GetProjectReportFileToSave("TraceWizardProjectReport");
            if (aggregateFile == null || aggregateFile.Length == 0)
                return panel;

            System.IO.File.Delete(aggregateFile);

            InitProgressWindow();

            ReportDone();

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

            var exporter = new ProjectReportExporter();
            exporter.DataSource = aggregateFile;
            exporter.Attributes = reportSelector.Attributes;

            this.Total = analysisFiles.Count;

            foreach (string analysisFile in analysisFiles) {
                if (this._isCancelationPending == true) break;

                ++this.Current;
                this.KeyCode = GetKeyCode(analysisFile);

                Analysis analysis = Services.TwServices.CreateAnalysis(analysisFile);
                Events eventsFiltered = new Events();

                List<FixtureClass> fixtureClassesToExclude = new List<FixtureClass>();

                if (reportSelector.Attributes.IsExcludeNoiseEnabled)
                    fixtureClassesToExclude.Add(FixtureClasses.Noise);

                if (reportSelector.Attributes.IsExcludeDuplicateEnabled)
                    fixtureClassesToExclude.Add(FixtureClasses.Duplicate);

                List<FixtureClass> fixtureClassesOutdoor = new List<FixtureClass>();
                fixtureClassesOutdoor.Add(FixtureClasses.Irrigation);

                if (reportSelector.Attributes.IsOutdoorPoolEnabled)
                    fixtureClassesOutdoor.Add(FixtureClasses.Pool);

                if (reportSelector.Attributes.IsOutdoorCoolerEnabled)
                    fixtureClassesOutdoor.Add(FixtureClasses.Cooler);

                bool partialDays = reportSelector.Attributes.IsPartialDaysEnabled;

                analysis.FilterEvents(eventsFiltered, partialDays, fixtureClassesToExclude);

                var projectReportProperties = (new ProjectReportCalculator()).CalculateProjectReportProperties(eventsFiltered, fixtureClassesOutdoor);
                exporter.Properties = projectReportProperties;
                exporter.KeyCode = analysis.KeyCode;

                exporter.Export();
            }
            TwFile.Launch(aggregateFile);
        }

        void ReportDone() {
            panel = new ProjectReportPanel(aggregateFile);
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

