using System;
using System.Windows.Data;
using System.Collections.Generic;
using System.ComponentModel;

using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using TraceWizard.Adoption;
using TraceWizard.Classification;
using TraceWizard.Environment;

namespace TraceWizard.TwApp {

    public class Traces : List<Trace>, INotifyPropertyChanged, IProgressOperation 
    {
        public Trace TraceAggregate { get; set; }
        protected List<string> Files { get; set; }
        public List<string> FilesLoaded { get; set; }

        public Adopter Adopter { get; set; }
        public Classifier Classifier { get; set; }

        ClassificationFactoryActual ActualClassificationFactory;
        ClassificationFactoryPredicted PredictedClassificationFactory;

        public Traces(List<string> files) : this(files,null,null) { }

        public Traces(List<string> files, Classifier classifier, Adopter adopter) {
            Files = files;
            Classifier = classifier;
            Adopter = adopter;

            TraceAggregate = new Trace();

            ActualClassificationFactory = new ClassificationFactoryActual(files);
            PredictedClassificationFactory = new ClassificationFactoryPredicted(files,classifier);

            this._total = 0;
            this._current = 0;
            this._isCancelationPending = false;

            this._keyCode = null;
        }

        void worker_DoWork(object sender, DoWorkEventArgs e) {
            Load(Files);
        }

        private string GetKeyCode(string fileName) {
            return System.IO.Path.GetFileNameWithoutExtension(fileName); 
        }
        
        private void Load(List<string> fileNames) {
            this.Total = fileNames.Count;

            FilesLoaded = new List<string>();

            foreach (string fileName in fileNames) {
                try {
                    if (this._isCancelationPending == true) break;

                    ++this.Current;
                    this.KeyCode = GetKeyCode(fileName);

                    Trace trace = new Trace(this.KeyCode);

                    trace.ClassificationActual = ActualClassificationFactory.Create(fileName);
                    trace.ClassificationPredicted = PredictedClassificationFactory.Create(fileName);

                    trace.Adopt(Adopter);

                    trace.Load();
                    Add(trace);

                    TraceAggregate.Load(trace);

                    FilesLoaded.Add(fileName);
                } catch (Exception ex) {
                    if (ex.Message.Contains(TwEnvironment.TwExemplars)) {
                        string message = "This classifier requires the file " + TwEnvironment.TwExemplars + ", which is not installed by default due to its size.";
                        message += " (" + ex.Message + ")";
                        MessageBox.Show(message);
                        return;
                    } else 
                    throw new Exception(ex.Message + " KeyCode = " + this.KeyCode);
                }
            }

            TraceAggregate.CalculateConfusionMatrix();
        }

        public static List<string> GetKeyCodes(string path, string extension) {
            List<string> keyCodes = new List<string>();
            foreach (string fileName in System.IO.Directory.GetFiles(path, "*." + extension)) {
                keyCodes.Add(System.IO.Path.GetFileNameWithoutExtension(fileName));
            }
            return keyCodes;
        }

        private List<string> GetKeyCodes(string path, string extension, int count) {
            List<string> keyCodes = new List<string>();
            int i = 0;
            foreach (string fileName in System.IO.Directory.GetFiles(path, "*." + extension)) {
                keyCodes.Add(System.IO.Path.GetFileNameWithoutExtension(fileName));
                if (++i == count)
                    break;
            }
            return keyCodes;
        }

        public Trace CurrentTrace {
            get { return (Trace)(CollectionViewSource.GetDefaultView(this)).CurrentItem; }
            set {
                Notify("CurrentTrace");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propName) {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
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