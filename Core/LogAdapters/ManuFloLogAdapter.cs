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

namespace TraceWizard.Logging.Adapters.ManuFlo {

    public class ManuFloLogAdapter : LogAdapter {
        public ManuFloLogAdapter() : base() { }

        int seconds;
        string dateTimeFormat = @"dd\/MM\/yy HH:mm:ss";
        double nutationRate = 1.0/72.0;
        double multiplier;

        public override bool CanLoad(string dataSource) {
            string format = GetDateTimeFormat(dataSource, dateTimeFormat);
            string[] lines = System.IO.File.ReadAllLines(dataSource);
            var dateTime = new DateTime();
            double pulseCount;
            if (lines.Length == 0)
                return true;

            string[] values = ReadAttributeValues(lines[0]);
            if ((values.Length >=3 && values.Length <= 4)
                && DateTime.TryParseExact(values[1], format, null, System.Globalization.DateTimeStyles.None, out dateTime)
                && double.TryParse(values[values.Length-1],out pulseCount)) 
                return true;
            else
                return false;
        }


        
        public override Log Load(string dataSource) {
            try {
                var log = new LogMeter(dataSource);
                log.Customer = new LogMeterCustomer();
                log.Meter = new LogMeterMeter();

                string[] lines = System.IO.File.ReadAllLines(dataSource);

                log.FileName = dataSource;

                dateTimeFormat = GetDateTimeFormat(dataSource, dateTimeFormat);
                seconds = GetInterval(lines);
                nutationRate = GetNutationRate(dataSource, nutationRate);
                log.Meter.Nutation = nutationRate;
                multiplier = (60.0 / (double)seconds) * nutationRate;
                log.Customer.ID = GetSiteId(lines);

                log.Flows = ReadFlows(lines, TimeSpan.FromSeconds(seconds));
        
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
                if (lines[i][0] == '=')
                    break;
                Flow flow = ReadFlow(ReadAttributeValues(lines[i]), duration);
                if (flow != null)
                    flows.Add(flow);
            }

            return flows;
        }

        string[] ReadAttributeValues(string line) {
            return line.Split(',');
        }
       
        string GetConfigFileThisFile(string dataSource) {
            return System.IO.Path.GetDirectoryName(dataSource) + @"\" + System.IO.Path.GetFileNameWithoutExtension(dataSource) + ".config";
        }

        string GetConfigFileThisFolder(string dataSource) {
            return System.IO.Path.GetDirectoryName(dataSource) + @"\logger.config";
        }

        string GetDateTimeFormat(string dataSource, string defaultFormat) {
            string format = GetDateTimeFormatLow(GetConfigFileThisFile(dataSource));
            if (string.IsNullOrEmpty(format)) {
                format = GetDateTimeFormatLow(GetConfigFileThisFolder(dataSource));
            }
            if (string.IsNullOrEmpty(format))
                format = defaultFormat;
            return format;
        }

        double GetNutationRate(string dataSource, double defaultRate) {
            double rate = GetNutationRateLow(GetConfigFileThisFile(dataSource));
            if (rate == 0) {
                rate = GetNutationRateLow(GetConfigFileThisFolder(dataSource));
            }
            if (rate == 0)
                rate = defaultRate;
            return rate;
        }

        double GetNutationRateLow(string dataSource) {
            if (System.IO.File.Exists(dataSource)) {
                string[] lines = System.IO.File.ReadAllLines(dataSource);

                foreach (string line in lines) {
                    string lineClean = CleanLine(line);
                    if (!string.IsNullOrEmpty(lineClean) && ReadAttributeValues(lineClean)[0].ToLower() == "nutationrate") {
                        return double.Parse(ReadAttributeValues(lineClean)[1]);
                    }
                }
            }

            return 0.0;
        }

        string GetSiteId(string[] values) {
            if (values.Length == 0)
                return string.Empty;

            return ReadAttributeValues(values[0])[0];
        }

        int GetInterval(string[] values) {
            DateTime dateTime1;
            DateTime dateTime2;

            if (values.Length < 2)
                return 0;

            dateTime1 = DateTime.ParseExact(ReadAttributeValues(values[0])[1], dateTimeFormat, null);
            dateTime2 = DateTime.ParseExact(ReadAttributeValues(values[1])[1], dateTimeFormat, null);
                
            return (int)(dateTime2.Subtract(dateTime1).TotalSeconds);
        }

        //string GetDateTimeFormat(string[] values) {
        //    var dateTime = new DateTime();

        //    string[] dateTimeFormats = new string[] { 
        //        @"dd\/MM\/yy HH:mm:ss",
        //        @"yy\/MM\/dd HH:mm:ss"
        //    };

        //    if (values.Length < 2)
        //        return string.Empty;

        //    foreach (string format in dateTimeFormats) {
        //        if (DateTime.TryParseExact(values[1], format, null, System.Globalization.DateTimeStyles.None, out dateTime))
        //            return format;
        //    }
        //    return string.Empty;
        //}

        string GetDateTimeFormatLow(string dataSource) {

            if (System.IO.File.Exists(dataSource)) {
                string[] lines = System.IO.File.ReadAllLines(dataSource);

                foreach (string line in lines) {
                    string lineClean = CleanLine(line);
                    if (!string.IsNullOrEmpty(lineClean) && ReadAttributeValues(lineClean)[0].ToLower() == "datetimeformat") {
                        return ReadAttributeValues(lineClean)[1].Trim().Replace("/", @"\/");
                    }
                }
            }

            return string.Empty;
        }

        string CleanLine(string line) {
            string clean = line;
            int indexComment = line.IndexOf(';');
            if (indexComment >= 0) {
                clean = line.Substring(0, indexComment).Trim();
            }
            return clean;
        }
        
        Flow ReadFlow(string[] values, TimeSpan duration) {
            var timeFrame = new TimeFrame(DateTime.ParseExact(values[1],dateTimeFormat,null), duration);
            int indexRate = values.Length - 1; 
            double rate = double.Parse(values[indexRate]) * multiplier;
            return new Flow(timeFrame, rate);
        }
    }
}
