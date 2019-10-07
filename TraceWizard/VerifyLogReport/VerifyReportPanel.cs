using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data;

// using Microsoft.Windows.Controls;

using TraceWizard.Entities;
using TraceWizard.Logging;
using TraceWizard.Logging.Adapters.Telematics;
using TraceWizard.Services;
using TraceWizard.Reporters;

namespace TraceWizard.TwApp {

    public class VerifyLogReporter : IProgressOperation, Reporter {

        string s = string.Empty;

        List<string> files;

        VerifyLogReportPanel panel = new VerifyLogReportPanel();

        public UIElement Report() {

            files = TwFile.GetLogFilesIncludingZipped();
            if (files.Count == 0)
                return panel;

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

            this.Total = files.Count;

            foreach (string file in files) {
                if (this._isCancelationPending == true) break;

                ++this.Current;
                this.KeyCode = GetKeyCode(file);

                var adapter = TwServices.CreateLogAdapter(file) as TelematicsLogAdapter;

                if (adapter != null) {
                    var integrityData = adapter.GetIntegrityData(file);
                    panel.IntegrityDatas.Add(integrityData);
                }
                
            }
        }

        void ReportDone() {
            panel.Initialize();
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

