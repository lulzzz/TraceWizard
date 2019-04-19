using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;

using TraceWizard.Reporters;
using TraceWizard.Entities;
using TraceWizard.Entities.Adapters.Arff;
using TraceWizard.Entities.Adapters.Tw4Jet;
using TraceWizard.Environment;

namespace TraceWizard.TwApp {

    public class EventsDatabaseExporter : WekaExporter {
        public class EventsDatabaseAttributes : ArffAttributes {
            public EventsDatabaseAttributes() {
                IsKeyCodeEnabled = true;
                IsEventIdEnabled = true;
                IsStartTimeEnabled = true;
                IsEndTimeEnabled = true;
                IsDurationEnabled = true;
                IsFirstCycleEnabled = true;
                IsVolumeEnabled = true;
                IsPeakEnabled = true;
                IsModeEnabled = true;

                FixtureClasses = new List<FixtureClass>();
                foreach (FixtureClass fixtureClass in TraceWizard.Entities.FixtureClasses.Items.Values) {
                    FixtureClasses.Add(fixtureClass);
                }
            }
        }

        public override void Export() {
            attributes = new EventsDatabaseAttributes();
            Export(TwEnvironment.TwEventsDatabase);
        }
    }

    public class ExemplarsExporter : WekaExporter {
        public class ExemplarsAttributes : ArffAttributes {
            public ExemplarsAttributes() {
                IsVolumeEnabled = true;
                IsPeakEnabled = true;
                IsDurationEnabled = true;
                IsModeEnabled = true;

                FixtureClasses = new List<FixtureClass>();
                foreach (FixtureClass fixtureClass in TraceWizard.Entities.FixtureClasses.Items.Values) {
                    FixtureClasses.Add(fixtureClass);
                }
            }
        }

        public override void Export() {
            attributes = new ExemplarsAttributes();
            Export(TwEnvironment.TwExemplars);
        }
    }
    
    public class WekaExporter : IProgressOperation, Exporter {

        protected List<string> analysisFiles = new List<string>(); 
        protected string arffFile;
        ArffSelector arffSelector;
        protected ArffAttributes attributes;
        protected bool launchTextEditor = false;

        public virtual void Export() {

            arffSelector = new ArffSelector(attributes = new ArffAttributes());
            arffSelector.Owner = Application.Current.MainWindow;
            arffSelector.ShowDialog();

            if (!arffSelector.DialogResult.HasValue || arffSelector.DialogResult.Value == false)
                return;

            Export(TwEnvironment.TwWeka);
        }

        protected void Export(string defaultFileName) {
            analysisFiles = TwFile.GetAnalysisFilesIncludingZipped();
            if (analysisFiles.Count == 0)
                return;

            arffFile = TwFile.GetArffFileToSave(defaultFileName);
            if (arffFile == null || arffFile.Length == 0)
                return;

            System.IO.File.Delete(arffFile);
            InitProgressWindow();
        }

        protected void InitProgressWindow() {
            this._total = 0;
            this._current = 0;
            this._isCancelationPending = false;

            this._keyCode = null;

            ProgressWindow progressWindow = new ProgressWindow(this);
            progressWindow.Topmost = false;
            progressWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            progressWindow.Owner = Application.Current.MainWindow;
            progressWindow.ShowInTaskbar = false;
            progressWindow.ShowDialog();
        }

         string GetKeyCode(string fileName) {
             return System.IO.Path.GetFileNameWithoutExtension(fileName);
         }

         void ExportLow() {

            var analysisAdapterTarget = new ArffAnalysisAdapter();
            analysisAdapterTarget.Attributes = attributes;

            this.Total = analysisFiles.Count;

            foreach (string analysisFile in analysisFiles) {
                if (this._isCancelationPending == true) break;

                ++this.Current;
                this.KeyCode = GetKeyCode(analysisFile);

                Analysis analysis = Services.TwServices.CreateAnalysis(analysisFile);

                EventsArff events = analysisAdapterTarget.Load(analysis.Events);
                analysisAdapterTarget.Save(arffFile, new Analysis(events, analysis.KeyCode),false);
            }

            if (launchTextEditor)
                TwFile.LaunchNotepad(arffFile);
            else
                MessageBox.Show("Export file successfully created: \r\n\r\n" + arffFile, TwAssembly.TitleTraceWizard());
        }

        void worker_DoWork(object sender, DoWorkEventArgs e) {
            ExportLow();
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