using System;
using System.Data;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;

using TraceWizard.Logging;
using TraceWizard.Entities;

using TraceWizard.Logging.Adapters;

using TraceWizard.Environment;

namespace TraceWizard.Logging.Adapters {

    public class TelematicsLog : Log {
        public TelematicsLog(string dataSource) : base(dataSource) { }
        public TelematicsLog() : base() { }

        public const string FactoryIdLabel = "FactoryID";
        public string FactoryId { get; set; }

        public const string StartDateTimeLabel = "dt";
        public DateTime StartDateTime { get; set; }

        public const string FileVersionLabel = "FileVersion";
        public Version FileVersion { get; set; }

        public const string StorageIntervalLabel = "Interval";
        public int StorageInterval { get; set; }

        public const string UnitLabel = "Unit";
        public string Unit { get; set; }

        public const string StartFlowsLabel = "miu";
        public const string FlowLabel = "r";
        public const string CounterLabel = "n";

        public double ConversionFactorFromCubicMetersToGallons = 264.172052;

        public double BaselineReading = 0;

        public int FirstCounter = 0;
        public int LastCounter = 0;
    }
}

namespace TraceWizard.Logging.Adapters.Telematics {

    public class TelematicsLogAdapter : LogAdapter {
        public TelematicsLogAdapter() : base() { }

        double currentVolumeRunningTotal = 0.0;
        double previousVolumeRunningTotal = 0.0;
        int counterFlow = 0;
        int lastCounterFlow = 0;
        int previousCounterFlow = 0;
        double baselineReading = 0;

        public class Discontinuity {
            public int CounterStart;
            public int CounterEnd;
        }
        
        public class IntegrityData {
            public DateTime FileNameStartTime;
            public DateTime HeaderStartTime;
            public DateTime StartTime;
            public string FileName;
            public DateTime EndTime;
            public long FirstCounter; 
            public long LastCounter;
            public TimeSpan DurationByCounters;
            public List<Discontinuity> Discontinuities;
        }

        public IntegrityData GetIntegrityData(string fileName) {
            var data = new IntegrityData();
            data.FileName = fileName;

            var fileNameStartTime = GetStartTime(fileName);
            data.FileNameStartTime = fileNameStartTime;

            var log = ReadHeader(fileName);
            data.HeaderStartTime = log.StartDateTime;

            data.StartTime = (fileNameStartTime == DateTime.MinValue) ? log.StartDateTime : fileNameStartTime;

            data.FirstCounter = log.FirstCounter;
            data.FirstCounter = log.LastCounter;
            data.DurationByCounters = new TimeSpan(0,0,((log.LastCounter - log.FirstCounter + 1) * log.StorageInterval));

            data.Discontinuities = GetDiscontinuities(fileName);

            return data;    
        }
        
        DateTime GetStartTime(string fileName) {
            DateTime dateTime = DateTime.MinValue;

            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fileName);
            if (fileNameWithoutExtension.Length != "01_22oct2011_31oct2011".Length)
                return dateTime;

            int day = 0;
            if (!int.TryParse(fileNameWithoutExtension.Substring(3, 2), out day))
                return dateTime;

            int month = 0;
            switch (fileNameWithoutExtension.Substring(5, 3).ToLower()) {
                case "jan" :
                    month = 1;
                    break;
                case "feb":
                    month = 2;
                    break;
                case "mar":
                    month = 3;
                    break;
                case "apr":
                    month = 4;
                    break;
                case "may":
                    month = 5;
                    break;
                case "jun":
                    month = 6;
                    break;
                case "jul":
                    month = 7;
                    break;
                case "aug":
                    month = 8;
                    break;
                case "sep":
                    month = 9;
                    break;
                case "oct":
                    month = 10;
                    break;
                case "nov":
                    month = 11;
                    break;
                case "dec":
                    month = 12;
                    break;
                default:
                    return dateTime;
            }

            int year = 0;
            if (!int.TryParse(fileNameWithoutExtension.Substring(8, 4), out year))
                return dateTime;

            try {
                dateTime = new DateTime(year, month, day);
            } catch { }

            return dateTime;
        }

        TelematicsLog log = null;

        public TelematicsLog ReadHeader(string dataSource) {
            try {
                log = new TelematicsLog(dataSource);

                string[] lines = System.IO.File.ReadAllLines(dataSource);

                log.FileName = dataSource;

                ReadHeader(log, lines);

                log.FirstCounter =  GetFirstCounter(lines);
                log.LastCounter = GetLastCounter(lines);

                return log;
            } catch (System.Data.OleDb.OleDbException ex) {
                if (ex.Message.Contains("Could not find file")) throw new Exception("Could not find file");
                else throw;
            }
        }

        public override bool CanLoad(string dataSource) {
            string[] lines = System.IO.File.ReadAllLines(dataSource);
            return (lines.Length > 2 && lines[1].StartsWith("<LoggerDataMIU"));
        }

        public override Log Load(string dataSource) {
            try {
                log = new TelematicsLog(dataSource);

                string[] lines = System.IO.File.ReadAllLines(dataSource);

                log.FileName = dataSource;

                ReadHeader(log, lines);

                log.StartTime = GetStartTime(log.FileName);
                if (log.StartTime == DateTime.MinValue) {
                    log.StartTime = log.StartDateTime;
                }

                lastCounterFlow = GetLastCounter(lines);
                
                log.Flows = ReadFlows(lines, log.StartTime, TimeSpan.FromSeconds(log.StorageInterval), log.ConversionFactorFromCubicMetersToGallons);

                if (log.Flows.Count > 0) {
                    log.EndTime = log.Flows[log.Flows.Count - 1].EndTime;
                }

                log.Update();
                return log;
            } catch (System.Data.OleDb.OleDbException ex) {
                if (ex.Message.Contains("Could not find file")) throw new Exception("Could not find file");
                else throw;
            }
        }

        List<Flow> ReadFlows(string[] lines, DateTime dateTime, TimeSpan duration, double conversionFactor) {
            List<Flow> flows = new List<Flow>();
            int i = 0;

            for (; i < lines.Length; i++) {
                if (ContainsOpenTag(lines[i],TelematicsLog.StartFlowsLabel)) {
                    i++;
                    break;
                }
            }

            for (; i < lines.Length; i++) {
                Flow flow = ReadFlow(lines, ref i, ref dateTime, duration, conversionFactor);
                if (counterFlow != 1 && flow != null)
                    flows.Add(flow);
            }

            return flows;
        }

        List<Discontinuity> GetDiscontinuities(string dataSource) {

            log = new TelematicsLog(dataSource);

            string[] lines = System.IO.File.ReadAllLines(dataSource);

            var discontinuities = new List<Discontinuity>();

            int previousCounter = 0;
            int currentCounter = 0;
            bool firstTime = true;

            Discontinuity discontinuity = null;
            for (int i = 0; i < lines.Length; i++) {
                if (ContainsOpenTag(lines[i], TelematicsLog.CounterLabel)) {
                    if (firstTime) {
                        currentCounter = int.Parse(ReadAttributeValue(lines[i]));
                        firstTime = false;
                    } else {
                        previousCounter = currentCounter;
                        currentCounter = int.Parse(ReadAttributeValue(lines[i]));
                        if (currentCounter != previousCounter + 1 && discontinuity == null) {
                            discontinuity = new Discontinuity();
                            discontinuity.CounterStart = currentCounter;
                        } else if (currentCounter == previousCounter + 1 && discontinuity != null) {
                            discontinuity.CounterEnd = currentCounter;
                            discontinuities.Add(discontinuity);
                            discontinuity = null;
                        }
                    }
                }
            }
            return discontinuities;
        }

        int GetLastCounter(string[] values) {
            int counter = 0;

            for (int i =0; i < values.Length; i++) {
                if (ContainsOpenTag(values[i], TelematicsLog.CounterLabel)) {
                    counter = int.Parse(ReadAttributeValue(values[i]));
                }
            }
            
            return counter;
        }

        int GetFirstCounter(string[] values) {
            int counter = 0;

            for (int i = 0; i < values.Length; i++) {
                if (ContainsOpenTag(values[i], TelematicsLog.CounterLabel)) {
                    counter = int.Parse(ReadAttributeValue(values[i]));
                    break;
                }
            }

            return counter;
        }

        Flow ReadFlow(string[] values, ref int i, ref DateTime dateTime, TimeSpan duration, double conversionFactor) {

            const double RateForTriggeringEventCreation = 0.1;        

            for (; i < values.Length; i++) {
                if (ContainsOpenTag(values[i], TelematicsLog.CounterLabel)) {
                    counterFlow = int.Parse(ReadAttributeValue(values[i]));
                }
                else if (ContainsOpenTag(values[i], TelematicsLog.FlowLabel)) {
                    if (counterFlow == 1) {
                        baselineReading = double.Parse(ReadAttributeValue(values[i])) * conversionFactor;
                        baselineReading -= RateForTriggeringEventCreation;
                        previousCounterFlow = counterFlow;
                        return null;
                    } else {
                        var timeFrame = new TimeFrame(dateTime, duration);
                        dateTime = timeFrame.EndTime;

                        double volumeRead = (double.Parse(ReadAttributeValue(values[i])) * conversionFactor) - baselineReading;
                        if (counterFlow == lastCounterFlow)
                            volumeRead += RateForTriggeringEventCreation;

                        currentVolumeRunningTotal = Math.Max(previousVolumeRunningTotal, volumeRead);

                        double volume = currentVolumeRunningTotal - previousVolumeRunningTotal;

                        if (counterFlow != previousCounterFlow + 1)
                            throw new Exception(log.DataSource + ": Gap in counter (n=" + counterFlow + ").");

                        previousCounterFlow = counterFlow;
                        previousVolumeRunningTotal = currentVolumeRunningTotal;

                        return new Flow(volume, timeFrame);
                    }
                }
            }

            return null;
        }

        string ReadAttributeValue(string line) {
            int valueStart = line.IndexOf(">") + 1;
            int valueEnd = line.LastIndexOf("<") - 1;

            return line.Substring(valueStart, (valueEnd - valueStart) + 1);
        }

        string ReadStringAttribute(string line, string label) {
            return ReadAttributeValue(line);
        }

        Version ReadVersionAttribute(string line, string label) {
            string value = ReadAttributeValue(line);
            return new Version(NormalizeVersion(value));
        }

        string NormalizeVersion(string value) {
            string normalizedValue = value;
            if (!value.Contains("."))
                normalizedValue = value + ".0";
            return normalizedValue;

        }

        DateTime ReadDateTimeAttribute(string line, string label) {
            string value = ReadAttributeValue(line);
            string valueNoTimeZone = TrimTimeZone(value);
            return DateTime.Parse(valueNoTimeZone);
        }

        string TrimTimeZone(string value) {
            string sPlus = string.Empty;
            if (value.Contains("+"))
                sPlus = value.Substring(0, value.LastIndexOf("+"));

            string sMinus = string.Empty;
            if (value.Contains("-"))
                sMinus = value.Substring(0, value.LastIndexOf("-"));

            if (string.IsNullOrEmpty(sPlus) && string.IsNullOrEmpty(sMinus))
                return value;
            else if (sPlus.Length > sMinus.Length)
                return sPlus;
            else 
                return sMinus;
        }

        double ReadDoubleAttribute(string line, string label) {
            string value = ReadAttributeValue(line);
            return double.Parse(value);
        }

        int ReadIntegerAttribute(string line, string label) {
            string value = ReadAttributeValue(line);
            return int.Parse(value);
        }

        bool ReadBooleanAttribute(string line, string label) {
            string value = ReadAttributeValue(line);
            return bool.Parse(value);
        }

        string BuildOpenTag(string tag) {
            return "<" + tag + ">";
        }

        bool ContainsOpenTag(string s, string tag) {
            if (s.Contains(BuildOpenTag(tag)))
                return true;
            else
                return false;
        }
        
        void ReadHeader(TelematicsLog log, string[] lines) {

            foreach (string line in lines) {
                if (ContainsOpenTag(line, TelematicsLog.StartFlowsLabel))
                    break;

                if (ContainsOpenTag(line, TelematicsLog.FileVersionLabel))
                    log.FileVersion = ReadVersionAttribute(line, TelematicsLog.FileVersionLabel);
                else if (ContainsOpenTag(line, TelematicsLog.FactoryIdLabel))
                    log.FactoryId = ReadStringAttribute(line, TelematicsLog.FactoryIdLabel);
                else if (ContainsOpenTag(line, TelematicsLog.StorageIntervalLabel))
                    log.StorageInterval = ReadIntegerAttribute(line, TelematicsLog.StorageIntervalLabel);
                else if (ContainsOpenTag(line, LogMeterMeter.UnitLabel))
                    log.Unit = ReadStringAttribute(line, TelematicsLog.UnitLabel);
                else if (ContainsOpenTag(line, TelematicsLog.StartDateTimeLabel))
                    log.StartDateTime = ReadDateTimeAttribute(line, TelematicsLog.StartDateTimeLabel);
            }
        }

        double Round(double value) {
            return Math.Round(value, 2);
        }
    }
}
