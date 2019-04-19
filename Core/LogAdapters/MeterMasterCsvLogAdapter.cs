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

namespace TraceWizard.Logging.Adapters.MeterMasterCsv {

    public class MeterMasterCsvLogAdapter : LogAdapter {
        public MeterMasterCsvLogAdapter() : base() { }

        public override bool CanLoad(string dataSource) {
            string[] lines = System.IO.File.ReadAllLines(dataSource);
            return (lines[0].StartsWith("MM100"));
        }

        public override Log Load(string dataSource) {
            try {
                var log = new LogMeter(dataSource);

                string[] lines = System.IO.File.ReadAllLines(dataSource);

                log.FileName = dataSource;

                log.Customer = ReadCustomer(lines);
                log.Meter = ReadMeter(lines);
                log.Flows = ReadFlows(lines, TimeSpan.FromSeconds(log.Meter.StorageInterval.GetValueOrDefault()));
        
                if (log.Flows.Count > 0) {
                    log.StartTime = log.Flows[0].StartTime;
                    log.EndTime = log.Flows[log.Flows.Count - 1].EndTime;
                }

                log.Update();
                return log;
            } catch (System.Data.OleDb.OleDbException ex) {
                if (ex.Message.Contains("Could not find file")) throw new Exception("Could not find file");
                else throw;
            }
        }

        List<Flow>ReadFlows(string[] lines, TimeSpan duration) {
            List<Flow> flows = new List<Flow>();
            int i = 0;

            for (; i < lines.Length; i++) {
                if (lines[i].Contains("DateTimeStamp,RateData")) {
                    i++;
                    break;
                }
            }

            for (; i < lines.Length; i++) {
                Flow flow = ReadFlow(ReadAttributeValues(lines[i]), duration);
                if (flow != null)
                    flows.Add(flow);
            }

            return flows;
        }

        string[] ReadAttributeValues(string line) {
            return line.Split(',');
        }

        Flow ReadFlow(string[] values, TimeSpan duration) {
            var timeFrame = new TimeFrame(DateTime.Parse(values[0]), duration);
            double rate = double.Parse(values[1]);
            return new Flow(timeFrame, rate);
        }

        string ReadStringAttribute(string line, string label) {
            string result = null;
            string[] values = ReadAttributeValues(line);
            for (int i = 1; i < values.Length; i++) {
                result += values[i];
            }
            if (result != null) {
                result = result.Trim('"');
                result = result.Trim();
            }

            return result;
        }

        double? ReadDoubleAttribute(string line, string label) {
            string[] values = ReadAttributeValues(line);
            if (values.Length > 0) {
                return double.Parse(values[1]);
            } else
                return null;
        }

        double? ReadDoubleAttributeBrainardBug(string line, string label) {
            int posDelimiter = line.IndexOf(',');
            if (posDelimiter == 0)
                return null;

            string s = line.Substring(posDelimiter);

            if (s.Length > 0) {
                // clean up data to deal with Brainard bug
                s = s.Replace("\"", "");
                s = s.Replace("\'", "");
                s = s.Replace(",", "");
                return double.Parse(s);
            } else
                return null;
        }

        int? ReadIntegerAttribute(string line, string label) {
            string[] values = ReadAttributeValues(line);
            if (values.Length > 0)
                return int.Parse(values[1]);
            else
                return null;
        }

        bool? ReadBooleanAttribute(string line, string label) {
            string[] values = ReadAttributeValues(line);
            if (values.Length > 0)
                return bool.Parse(values[1]);
            else
                return null;
        }

        LogMeterCustomer ReadCustomer(string[] lines) {
            LogMeterCustomer customer = new LogMeterCustomer();

            foreach (string line in lines) {
                if (line.StartsWith("DateTimeStamp"))
                    break;

                if (line.StartsWith(LogMeterCustomer.IdLabel))
                    customer.ID = ReadStringAttribute(line, LogMeterCustomer.IdLabel);
                else if (line.StartsWith(LogMeterCustomer.NameLabel))
                    customer.Name = ReadStringAttribute(line, LogMeterCustomer.NameLabel);
                else if (line.StartsWith(LogMeterCustomer.AddressLabel))
                    customer.Address = ReadStringAttribute(line, LogMeterCustomer.AddressLabel);
                else if (line.StartsWith(LogMeterCustomer.CityLabel))
                    customer.City = ReadStringAttribute(line, LogMeterCustomer.CityLabel);
                else if (line.StartsWith(LogMeterCustomer.StateLabel))
                    customer.State = ReadStringAttribute(line, LogMeterCustomer.StateLabel);
                else if (line.StartsWith(LogMeterCustomer.PostalCodeLabel))
                    customer.PostalCode = ReadStringAttribute(line, LogMeterCustomer.PostalCodeLabel);
                else if (line.StartsWith(LogMeterCustomer.PhoneNumberLabel))
                    customer.PhoneNumber = ReadStringAttribute(line, LogMeterCustomer.PhoneNumberLabel);
                else if (line.StartsWith(LogMeterCustomer.NoteLabel))
                    customer.Note = ReadStringAttribute(line, LogMeterCustomer.NoteLabel);
            }
            return customer;
        }

        LogMeterMeter ReadMeter(string[] lines) {
            LogMeterMeter meter = new LogMeterMeter();

            foreach (string line in lines) {
                if (line.StartsWith("DateTimeStamp"))
                    break;

                if (line.StartsWith(LogMeterMeter.CodeLabel))
                    meter.Code = ReadIntegerAttribute(line, LogMeterMeter.CodeLabel);
                else if (line.StartsWith(LogMeterMeter.MakeLabel))
                    meter.Make = ReadStringAttribute(line, LogMeterMeter.MakeLabel);
                else if (line.StartsWith(LogMeterMeter.ModelLabel))
                    meter.Model = ReadStringAttribute(line, LogMeterMeter.ModelLabel);
                else if (line.StartsWith(LogMeterMeter.SizeLabel))
                    meter.Size = ReadStringAttribute(line, LogMeterMeter.SizeLabel);
                else if (line.StartsWith(LogMeterMeter.UnitLabel))
                    meter.Unit = ReadStringAttribute(line, LogMeterMeter.UnitLabel);
                else if (line.StartsWith(LogMeterMeter.NutationLabel))
                    meter.Nutation = ReadDoubleAttribute(line, LogMeterMeter.NutationLabel);
                else if (line.StartsWith(LogMeterMeter.LedLabel))
                    meter.Led = ReadDoubleAttribute(line, LogMeterMeter.LedLabel);
                else if (line.StartsWith(LogMeterMeter.StorageIntervalLabel))
                    meter.StorageInterval = ReadIntegerAttribute(line, LogMeterMeter.StorageIntervalLabel);
                else if (line.StartsWith(LogMeterMeter.NumberOfIntervalsLabel))
                    meter.NumberOfIntervals = ReadIntegerAttribute(line, LogMeterMeter.NumberOfIntervalsLabel);
                else if (line.StartsWith(LogMeterMeter.TotalTimeLabel))
                    meter.TotalTime = ReadStringAttribute(line, LogMeterMeter.TotalTimeLabel);
                else if (line.StartsWith(LogMeterMeter.TotalPulsesLabel))
                    meter.TotalPulses = ReadIntegerAttribute(line, LogMeterMeter.TotalPulsesLabel);
                else if (line.StartsWith(LogMeterMeter.BeginReadingLabel))
                    meter.BeginReading = ReadDoubleAttributeBrainardBug(line, LogMeterMeter.BeginReadingLabel);
                else if (line.StartsWith(LogMeterMeter.EndReadingLabel))
                    meter.EndReading = ReadDoubleAttributeBrainardBug(line, LogMeterMeter.EndReadingLabel);
                else if (line.StartsWith(LogMeterMeter.RegisterVolumeLabel))
                    meter.RegisterVolume = ReadDoubleAttribute(line, LogMeterMeter.RegisterVolumeLabel);
                else if (line.StartsWith(LogMeterMeter.MeterMasterVolumeLabel))
                    meter.MeterMasterVolume = ReadDoubleAttribute(line, LogMeterMeter.MeterMasterVolumeLabel);
                else if (line.StartsWith(LogMeterMeter.ConversionFactorTypeLabel))
                    meter.ConversionFactorType = ReadIntegerAttribute(line, LogMeterMeter.ConversionFactorTypeLabel);
                else if (line.StartsWith(AppendComma(LogMeterMeter.ConversionFactorLabel)))
                    meter.ConversionFactor = ReadDoubleAttribute(line, LogMeterMeter.ConversionFactorLabel);
                else if (line.StartsWith(LogMeterMeter.DatabaseMultiplierLabel))
                    meter.DatabaseMultiplier = ReadIntegerAttribute(line, LogMeterMeter.DatabaseMultiplierLabel);
                else if (line.StartsWith(LogMeterMeter.CombinedFileLabel))
                    meter.CombinedFile = ReadIntegerAttribute(line, LogMeterMeter.CombinedFileLabel);
                else if (line.StartsWith(LogMeterMeter.DoublePulseLabel))
                    meter.DoublePulse = ReadBooleanAttribute(line, LogMeterMeter.DoublePulseLabel);
            }
            return meter;
        }

        string AppendComma(string s) { return s + ","; }

        double? Round(double? value) {
            if (value.HasValue)
                return Math.Round(value.Value, 2);
            else
                return null;
        }

        public void Save(string dataSource, LogMeter log) {
            if (System.IO.File.Exists(dataSource))
                System.IO.File.Delete(dataSource);
            StringBuilder text = new System.Text.StringBuilder();
            WriteHeader(text, log);
            WriteData(text, log.Flows);
            System.IO.File.WriteAllText(dataSource, text.ToString());
        }

        void WriteHeader(StringBuilder text, LogMeter log) {
            AppendKeyValuePair(text,"Format","TraceWizardLog");
            AppendKeyValuePair(text, "Version", TwAssembly.Version().ToString());

            WriteCustomerInfo(text, log.Customer);
            WriteMeterInfo(text, log.Meter);
        }

        void WriteCustomerInfo(StringBuilder text, LogMeterCustomer customer) {
            AppendKeyValuePair(text,LogMeterCustomer.IdLabel,customer.ID);
            AppendKeyValuePair(text, LogMeterCustomer.NameLabel, customer.Name);
            AppendKeyValuePair(text, LogMeterCustomer.AddressLabel, customer.Address);
            AppendKeyValuePair(text, LogMeterCustomer.CityLabel, customer.City);
            AppendKeyValuePair(text, LogMeterCustomer.StateLabel, customer.State);
            AppendKeyValuePair(text, LogMeterCustomer.PostalCodeLabel, customer.PostalCode);
            AppendKeyValuePair(text, LogMeterCustomer.PhoneNumberLabel, customer.PhoneNumber);
            AppendKeyValuePair(text, LogMeterCustomer.NoteLabel, customer.Note);
        }

        void WriteMeterInfo(StringBuilder text, LogMeterMeter meter) {
            if (meter.Code.HasValue)
                AppendKeyValuePair(text, LogMeterMeter.CodeLabel, meter.Code.ToString());
            AppendKeyValuePair(text, LogMeterMeter.MakeLabel, meter.Make);
            AppendKeyValuePair(text, LogMeterMeter.ModelLabel, meter.Model);
            AppendKeyValuePair(text, LogMeterMeter.SizeLabel, meter.Size);
            AppendKeyValuePair(text, LogMeterMeter.UnitLabel, meter.Unit);
            
            if (meter.Nutation.HasValue)
                AppendKeyValuePair(text, LogMeterMeter.NutationLabel, meter.Nutation.ToString());
            if (meter.Led.HasValue)
                AppendKeyValuePair(text, LogMeterMeter.LedLabel, meter.Led.ToString());
            if (meter.StorageInterval.HasValue)
                AppendKeyValuePair(text, LogMeterMeter.StorageIntervalLabel, meter.StorageInterval.ToString());
            if (meter.NumberOfIntervals.HasValue)
                AppendKeyValuePair(text, LogMeterMeter.NumberOfIntervalsLabel, meter.NumberOfIntervals.ToString());
            AppendKeyValuePair(text, LogMeterMeter.TotalTimeLabel, meter.TotalTime);
            if (meter.TotalPulses.HasValue)
                AppendKeyValuePair(text, LogMeterMeter.TotalPulsesLabel, meter.TotalPulses.ToString());
            if (meter.BeginReading.HasValue)
                AppendKeyValuePair(text, LogMeterMeter.BeginReadingLabel, meter.BeginReading.ToString());
            if (meter.EndReading.HasValue)
                AppendKeyValuePair(text, LogMeterMeter.EndReadingLabel, meter.EndReading.ToString());
            if (meter.RegisterVolume.HasValue)
                AppendKeyValuePair(text, LogMeterMeter.RegisterVolumeLabel, meter.RegisterVolume.ToString());
            if (meter.MeterMasterVolume.HasValue)
                AppendKeyValuePair(text, LogMeterMeter.MeterMasterVolumeLabel, meter.MeterMasterVolume.ToString());
            if (meter.ConversionFactorType.HasValue)
                AppendKeyValuePair(text, LogMeterMeter.ConversionFactorTypeLabel, meter.ConversionFactorType.ToString());
            if (meter.ConversionFactor.HasValue)
                AppendKeyValuePair(text, LogMeterMeter.ConversionFactorLabel, meter.ConversionFactor.ToString());
            if (meter.DatabaseMultiplier.HasValue)
                AppendKeyValuePair(text, LogMeterMeter.DatabaseMultiplierLabel, meter.DatabaseMultiplier.ToString());
            if (meter.CombinedFile.HasValue)
                AppendKeyValuePair(text, LogMeterMeter.CombinedFileLabel, meter.CombinedFile.ToString());
            if (meter.DoublePulse.HasValue)
                AppendKeyValuePair(text, LogMeterMeter.DoublePulseLabel, meter.DoublePulse.ToString());
        }

        void WriteData(StringBuilder text, List<Flow> flows) {
            AppendKeyValuePair(text,"DateTimeStamp","RateData");

            foreach (Flow flow in flows) {
                AppendKeyValuePair(text, flow.StartTime.ToString(), flow.Rate.ToString());
            }
        }

        void AppendKeyValuePair(StringBuilder text, string key, string value) {
            text.AppendLine(key + "," + value);
        }

//TotalTime,12 days + 21:52:00
//BeginReading,13922.7802734375
//EndReading,24814.599609375
//RegVolume,10891.8193359375
//MMVolume,81496.147
//ConvFactor,1
//,

    }
}
