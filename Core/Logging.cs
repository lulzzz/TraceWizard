using System;
using System.Collections.Generic;

using TraceWizard.Entities;
using TraceWizard.Data;
using TraceWizard.Analyzers;
using TraceWizard.Environment;

using TraceWizard.Logging.Adapters.MeterMasterCsv;
using TraceWizard.Logging.Adapters.MeterMasterJet;
using TraceWizard.Logging.Adapters.Telematics;
using TraceWizard.Logging.Adapters.ManuFlo;

namespace TraceWizard.Logging {

    public class Log : IDatabase {

        public Log(string dataSource) { DataSource = dataSource; }
        public Log() {}

        public virtual List<Flow> Flows { get; set; }

        public double Volume { get; protected set; }

        public double Peak { get; protected set; }

        public const string StartTimeLabel = "LogStartTime";
        public DateTime StartTime { get; set; }

        public const string EndTimeLabel = "LogEndTime";
        public DateTime EndTime { get; set; }

        public TimeFrame TimeFrame { get { return new TimeFrame(StartTime, EndTime); } }

        public const string FileNameLabel = "LogFileName";
        public string FileName { get; set; }

        public string DataSource { get; set; }
        public string DataSourceFileNameWithoutExtension { get { return System.IO.Path.GetFileNameWithoutExtension(DataSource); } set { } }
        public string DataSourceWithFullPath { get { return System.IO.Path.GetFullPath(DataSource); } set { } }

        public double[] HoursVolume;
        public Dictionary<DateTime,double> DailyVolume;
        
        public void UpdateHourlyTotals() {
            HoursVolume = new double[24];
            foreach (Flow flow in Flows) {
                HoursVolume[flow.StartTime.Hour] += flow.Volume;
            }
        }

        public void UpdateDailyTotals() {
            DailyVolume = GetFullDays();
            foreach (Flow flow in Flows) {
                if (DailyVolume.ContainsKey(flow.StartTime.Date))
                    DailyVolume[flow.StartTime.Date] += flow.Volume;
            }
        }

        Dictionary<DateTime, double> GetFullDays() {
            var dictionary = new Dictionary<DateTime, double>();
            var startDate = StartTime.Date;
            var endDate = EndTime.Date;
            var runningDate = startDate.AddDays(1);

            while (runningDate < endDate) {
                dictionary.Add(runningDate, 0.0);
                runningDate = runningDate.AddDays(1);
            }
            return dictionary;
        }

        public void Update() {
            Volume = CalcVolume();
            Peak = CalcPeak();
        }

        private double CalcVolume() {
            double vol = 0.0;
            foreach (Flow flow in Flows)
                vol += flow.Volume;
            return vol;
        }

        private double CalcPeak() {
            double peak = 0.0;
            foreach (Flow flow in Flows)
                if (flow.Peak > peak)
                    peak = flow.Peak;
            return peak;
        }
    }
}
namespace TraceWizard.Logging.Adapters {
    public class LogAdapterFactory : AdapterFactory {
        public override Adapter GetAdapter(string dataSource) {
            string extension = System.IO.Path.GetExtension(dataSource).ToLower().Trim('.');
            switch (extension) {
                case (TwEnvironment.MeterMasterJetLogExtension):
                    return new MeterMasterJetLogAdapter();
                case (TwEnvironment.CsvLogExtension):
                case (TwEnvironment.TelematicsLogExtension):
                case (TwEnvironment.TextLogExtension): 
                    {
                        var adapter = new MeterMasterCsvLogAdapter();
                        if (adapter.CanLoad(dataSource))
                            return adapter;
                    }
                    {
                        var adapter = new ManuFloLogAdapter();
                        if (adapter.CanLoad(dataSource))
                            return adapter;
                    }
                    {
                        var adapter = new TelematicsLogAdapter();
                        if (adapter.CanLoad(dataSource))
                            return adapter;
                    }

                    return null;
            }
            return null;
        }
    }

    public abstract class LogAdapter : Adapter {
        public LogAdapter() { }
        public abstract bool CanLoad(string dataSource);
        public abstract Logging.Log Load(string dataSource);
    }
}

