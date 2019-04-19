using System;
using System.Collections.Generic;

using System.ComponentModel;

using TraceWizard.Analyzers;
using TraceWizard.Logging;
using TraceWizard.Environment;
using TraceWizard.Data;
using TraceWizard.Notification;

using TraceWizard.Entities.Adapters.Arff;
using TraceWizard.Entities.Adapters.Tw4Jet;

namespace TraceWizard.Entities {
    public class Analysis {

        public Analysis() : this(null, null, null) { }
        public Analysis(Events events, FixtureProfiles fixtureProfiles) : this(events, fixtureProfiles, null) { }
        public Analysis(Events events) : this(events, null, null) { }
        public Analysis(Events events, string keyCode) : this(events, null, null) { KeyCode = keyCode; }
        public Analysis(Events events, Log log) : this(events, null, log) { }
        public Analysis(Log log) : this(null, null, log) { }
        public Analysis(Events events, FixtureProfiles fixtureProfiles, Log log) {
            Events = events;
            if (fixtureProfiles != null)
                FixtureProfiles = fixtureProfiles;

            Log = log;

            if (log != null) {
                KeyCode = log.DataSourceFileNameWithoutExtension;
            }
        }

        Events events;
        public Events Events { 
            get { return events; } 
            set { 
                events = value;
                FixtureSummaries = new FixtureSummaries(events);
                if (events != null) {
                    events.PropertyChanged += new PropertyChangedEventHandler(events_PropertyChanged);
                    events.UndoManager = UndoManager;
                    UndoManager.Events = events;
                }
            } 
        }

        public UndoManager UndoManager = new UndoManager();

        public void events_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case TwNotificationProperty.OnPreviewEndSelect:
                    UpdateSelectedCounts();
                    break;
                case TwNotificationProperty.OnPreviewEndClassify:
                    UpdateFixtureSummaries();
                    break;
                case TwNotificationProperty.OnPreviewEndMergeSplit:
                    Events.UpdateLinkedList();
                    UpdateFixtureSummaries();
                    UndoManager.Clear();
                    break;
                case TwNotificationProperty.OnChangeStartTime:
                    UndoManager.Clear();
                    break;
                case TwNotificationProperty.OnEndApplyConversionFactor:
                    UpdateFixtureSummaries();
                    UndoManager.Clear();
                    break;
            }
        }

        public FixtureProfiles FixtureProfiles = new FixtureProfiles();
        public FixtureSummaries FixtureSummaries;
        public Log Log { get; set; }

        public string KeyCode { get; set; }

        public static Dictionary<DateTime,double> GetFullDays(Events events) {

            if (events == null)
                return null;

            var dictionary = new Dictionary<DateTime, double>();
            var startDate = events.StartTime.Date;
            var endDate = events.EndTime.Date;
            var runningDate = startDate.AddDays(1);

            while (runningDate < endDate) {
                dictionary.Add(runningDate,0.0);
                runningDate = runningDate.AddDays(1);
            }
            return dictionary;
        }


        public void FilterEvents(Events events, bool partialDays, List<FixtureClass> fixtureClassesToExclude) {
            foreach (Event @event in Events) {
                if ((partialDays ||
                        (@event.StartTime.Date != Events.StartTime.Date
                        && @event.EndTime.Date != Events.EndTime.Date))
                    && !fixtureClassesToExclude.Contains(@event.FixtureClass)
                    ) {
                    events.Add(@event);
                }
            }
        }

        public void ClearFirstCycle() {
            foreach (Event @event in Events)
                @event.FirstCycle = false;
        }

        public void ClearManuallyClassified() {
            foreach (Event @event in Events)
                @event.ManuallyClassified = false;
        }

        public void UpdateSelectedCounts() {
            FixtureSummaries.UpdateSelectedCounts(Events);
        }

        void ClearHourlyTotals() {
            FixtureSummaries.ClearHourlyTotals();
        }

        void ClearDailyTotals() {
            FixtureSummaries.ClearDailyTotals();
        }

        public void UpdateDailyTotals() {
            ClearDailyTotals();
            foreach (Event @event in Events) {
                foreach (Flow flow in @event) {
                    if (FixtureSummaries.DailyVolume.ContainsKey(flow.StartTime.Date)) {
                        FixtureSummaries.DailyVolume[flow.StartTime.Date] += flow.Volume;
                        FixtureSummaries[@event.FixtureClass].DailyVolume[flow.StartTime.Date] += flow.Volume;
                        FixtureSummaries[@event.FixtureClass].DailyVolumeTotal += flow.Volume;
                    }
                }
            }
        }

        public void UpdateHourlyTotals() {
            ClearHourlyTotals();
            foreach (Event @event in Events) {
                foreach (Flow flow in @event) {
                    FixtureSummaries.HourlyVolume[flow.StartTime.Hour] += flow.Volume;
                    FixtureSummaries[@event.FixtureClass].HourlyVolume[flow.StartTime.Hour] += flow.Volume;
                }
            }
        }

        public void UpdateFixtureSummaries() {
            FixtureSummaries.Update();
        }

        public bool VolumeToleranceExceeded(double? volume, double tolerance) {
            if (!volume.HasValue || volume == 0)
                return true;
            return Math.Abs(Events.Volume - volume.Value) / volume.Value >= tolerance;
        }

        static public Analysis Append(Analysis analysisBase, Analysis analysisAppend, DateTime dateTimeEndBase, DateTime dateTimeStartAppend) {
            var events = new Events();

            foreach (Event eventSource in analysisBase.Events) {
                if (eventSource.EndTime <= dateTimeEndBase && !(eventSource.Channel == Channel.Super && eventSource.BaseEvent.EndTime > dateTimeEndBase )) {
                    var eventTarget = Event.DeepCopy(eventSource);
                    events.Add(eventTarget);

                    foreach (Flow flow in eventSource) {
                        var newFlow = new Flow(flow.StartTime, flow.Duration, flow.Rate);
                        eventTarget.AddWithoutVolume(newFlow);
                    }
                }
            }

            TimeSpan offset = dateTimeEndBase.Subtract(dateTimeStartAppend);
            
            foreach (Event eventSource in analysisAppend.Events) {
                if (eventSource.StartTime >= dateTimeStartAppend && !(eventSource.Channel == Channel.Super && eventSource.BaseEvent.StartTime < dateTimeStartAppend)) {
                    var eventTarget = Event.DeepCopy(eventSource, offset);
                    events.Add(eventTarget);

                    foreach (Flow flow in eventSource) {
                        var newFlow = new Flow(flow.StartTime.Add(offset), flow.Duration, flow.Rate);
                        eventTarget.AddWithoutVolume(newFlow);
                    }
                }
            }


            events.UpdateVolume();

            string fileNameBeginning = GetBeginning(analysisBase.KeyCode);
            string fileNameEnd = GetEnd(analysisAppend.KeyCode);
            string fileNameDefault = null;
            if (!string.IsNullOrEmpty(fileNameBeginning) && !string.IsNullOrEmpty(fileNameEnd))
                fileNameDefault = fileNameBeginning + "_" + fileNameEnd;
            else
                fileNameDefault = analysisBase.KeyCode + "_" + analysisAppend.KeyCode;

            var analysis = new Analysis(events, fileNameDefault);

            events.UpdateChannel();
            events.UpdateSuperPeak();
            events.UpdateLinkedList();
            events.UpdateOriginalVolume();

            analysis.UpdateFixtureSummaries();

            return analysis;
        }

        //expected format: 01_22oct2011_31oct2011
        static string GetBeginning(string fileName) {
            string[] tokens = fileName.Split('_');
            if (tokens.Length >= 3) {
                return tokens[0] + '_' + tokens[1];
            } else 
                return null;
        }
        static string GetEnd(string fileName) {
            string[] tokens = fileName.Split('_');
            if (tokens.Length >= 3) {
                return tokens[tokens.Length - 1];
            } else
                return null;
        }
    }

    public class Analyses : List<Analysis> {

        public double VolumeAverage { get; set; }

        public new void Add(Analysis item) {
            base.Add(item);
        }

        public void Update() {
            if (Count == 0)
                return;

            foreach (Analysis analysis in this) {
                VolumeAverage += analysis.Events.Volume;
            }
            VolumeAverage = VolumeAverage / Count;
        }
    }

    public class AnalysisDatabase : Analysis, IDatabase {

        public AnalysisDatabase(string dataSource) : base() { DataSource = dataSource; KeyCode = DataSourceFileNameWithoutExtension; }
        public AnalysisDatabase(string dataSource, Events events) : base(events) { DataSource = dataSource; KeyCode = DataSourceFileNameWithoutExtension; }
        public AnalysisDatabase(string dataSource, Events events, Log log) : base(events, log) { DataSource = dataSource; KeyCode = DataSourceFileNameWithoutExtension; }
        public AnalysisDatabase(string dataSource, Events events, Log log, FixtureProfiles fixtureProfiles) : base(events, fixtureProfiles, log) { DataSource = dataSource; KeyCode = DataSourceFileNameWithoutExtension; }

        public string DataSource { get; set; }
        public string DataSourcePath { get; set; }
        public string DataSourceFileNameWithoutExtension { get { return System.IO.Path.GetFileNameWithoutExtension(DataSource); } set { } }
        public string DataSourceWithFullPath { get { return System.IO.Path.GetFullPath(DataSource); } set { } }
        public string DataSourceDirectoryName { get { return System.IO.Path.GetDirectoryName(DataSource); } set { } }

    }
}

namespace TraceWizard.Entities.Adapters {
    public class AnalysisAdapterFactory : AdapterFactory {
        public override Adapter GetAdapter(string dataSource) {
            string extension = System.IO.Path.GetExtension(dataSource).Trim('.');
            switch (extension) {
                case (TwEnvironment.ArffAnalysisExtension):
                    return new ArffAnalysisAdapter();
                case (TwEnvironment.Tw4JetAnalysisExtension):
                    return new Tw4JetAnalysisAdapter();
            }
            return null;
        }
    }

    public interface AnalysisAdapter : Adapter {

        Analysis Load(string dataSource);
        Events LoadEvents(string dataSource, FixtureProfiles fixtureProfiles);
        FixtureProfiles LoadFixtureProfiles(string dataSource);
        void Save(string dataSource, Analysis analysis, bool overWrite);
    }
}