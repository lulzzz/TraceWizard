using System;
using System.IO;
using System.Diagnostics;

using System.Data;
using System.Data.OleDb;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using TraceWizard.Data.Jet;
using TraceWizard.Logging;
using TraceWizard.Entities;

using TraceWizard.Logging.Adapters;

using TraceWizard.Environment;

namespace TraceWizard.Logging.Adapters.MeterMasterJet
{
    public class MeterMasterJetFlow : Flow {
        public int RawData { get; protected set; }

        public MeterMasterJetFlow(TimeFrame timeFrame, double rate, int rawData) : base(timeFrame, rate) { RawData = rawData; }
    }

    public class MeterMasterJetLogAdapter : LogAdapter
    {
        public MeterMasterJetLogAdapter() : base() { }

        string ConvertFromAccess97(string dataSource) {
            string dataSourceConverted = ConvertFileName(dataSource);
            LaunchConverter(dataSource, dataSourceConverted);
            //Microsoft.Office.Interop.Access.Application app = new Microsoft.Office.Interop.Access.Application();
            //app.ConvertAccessProject(dataSource, dataSourceConverted, AcFileFormat.acFileFormatAccess2007);
            return dataSourceConverted;
        }

        void LaunchConverter(string oldFileName, string newFileName) {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "MsAccess.EXE";
            startInfo.Arguments = oldFileName + " /Convert " + newFileName;
            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            process.Close();
        }


        string ConvertFileName(string filename) {
            return System.IO.Path.GetDirectoryName(filename) + "\\" + System.IO.Path.GetFileNameWithoutExtension(filename) + "-converted" + System.IO.Path.GetExtension(filename);
        }

        public override bool CanLoad(string dataSource) {
            return (dataSource.EndsWith(TwEnvironment.MeterMasterJetLogExtension));
        }

        public override Log Load(string dataSource) {

            //dataSource = ConvertFromAccess97(dataSource);

            var log = new LogMeter(dataSource);
            try {
                using (OleDbConnection connection = new OleDbConnection(DataServices.BuildJetConnectionString(dataSource, true))) {
                    connection.Open();
                    using (OleDbCommand command = new OleDbCommand()) {
                        command.Connection = connection;

                        log.FileName = dataSource;
                        log.Customer = AddCustomer(command);
                        log.Meter = AddMeter(command);
                        log.Flows = AddFlows(command, TimeSpan.FromSeconds(log.Meter.StorageInterval.GetValueOrDefault()),log);

                        if (log.Flows.Count > 0) {
                            log.StartTime = log.Flows[0].StartTime;
                            log.EndTime = log.Flows[log.Flows.Count - 1].EndTime;
                        }
                    }

                    log.Update();
                    return log;
                }
            } catch (System.Data.OleDb.OleDbException ex) {
                if (ex.Message.Contains("Could not find file")) throw new Exception("Could not find file");
                else if (ex.Message.Contains("Unrecognized database format")) throw new Exception("Unrecognized logger format");
                else if (ex.Message.Contains("Cannot open a database created with a previous version of your application")) throw new Exception("The Microsoft Access 2007 Runtime is not properly installed on this system. Historically, this error has occured when Access 2010 or later has been run on this system.\r\n\r\nPlease review the " + TwAssembly.Title() + " System Requirements and reinstall/repair the Access 2007 Runtime.\r\n\r\n The original error message was: " + ex.Message);
                else throw;
            } catch (InvalidOperationException ex) {
                if (ex.Message.Contains("provider is not registered")) throw new Exception("The Microsoft Access 2007 Runtime is not installed on this system.\r\n\r\nPlease review the " + TwAssembly.Title() + " System Requirements and install the Access 2007 Runtime.\r\n\r\n(" + ex.Message + ")");
                else throw;
            }
        }

        List<Flow> AddFlows(OleDbCommand command, TimeSpan duration, LogMeter log) {
            command.CommandText = BuildFlowsCommandText();
            List<Flow> flows = new List<Flow>();
            using (OleDbDataReader reader = command.ExecuteReader()) {

                while (reader.Read())
                    AddFlow(flows, reader, duration, log);
            }
            return flows;
        }

        LogMeterCustomer AddCustomer(OleDbCommand command) {
            LogMeterCustomer customer = null;
            command.CommandText = BuildCommandCustomerText();
            using (OleDbDataReader reader = command.ExecuteReader())
                if (reader.Read()) {
                    customer = new LogMeterCustomer();
                    customer.ID = reader.GetString(0).Trim();
                    customer.Name = reader.GetString(1).Trim();
                    customer.Address = reader.GetString(2).Trim();
                    customer.City = reader.GetString(3).Trim();
                    customer.State = reader.GetString(4).Trim();
                    customer.PostalCode = reader.GetString(5).Trim();
                    customer.PhoneNumber = reader.GetString(6).Trim();
                    customer.Note = reader.GetString(7).Trim();
                }
            return customer;
        }

        LogMeterMeter AddMeter(OleDbCommand command) {
            LogMeterMeter meter = null;
            command.CommandText = BuildCommandMeterText();
            using (OleDbDataReader reader = command.ExecuteReader())
                if (reader.Read()) {
                    meter = new LogMeterMeter();
                    meter.Code = reader.GetInt32(0);
                    meter.Make = reader.GetString(1).Trim();
                    meter.Model = reader.GetString(2).Trim();
                    meter.Size = reader.GetString(3).Trim();
                    meter.Unit = reader.GetString(4).Trim();
                    meter.Nutation = reader.GetDouble(5);
                    meter.Led = reader.GetDouble(6);
                    meter.StorageInterval = reader.GetInt32(7);
                    meter.NumberOfIntervals = reader.GetInt32(8);
                    meter.TotalPulses = reader.GetInt32(9);
                    meter.BeginReading = Math.Round(reader.GetDouble(10), 2);
                    meter.EndReading = Math.Round(reader.GetDouble(11),2);
                    meter.RegisterVolume = Math.Round(reader.GetDouble(12),2);
                    meter.MeterMasterVolume =  Math.Round(reader.GetDouble(13),2);
                    meter.ConversionFactorType = reader.GetInt32(14);
                    meter.ConversionFactor =  Math.Round(reader.GetDouble(15),2);
                    meter.DatabaseMultiplier = Math.Round(reader.GetDouble(16),2);
                    meter.CombinedFile =  reader.GetInt32(17);
                }
            return meter;
        }

        void AddFlow(List<Flow> flows, OleDbDataReader reader, TimeSpan duration, LogMeter log)
        {
            DateTime startTime = reader.GetDateTime(0);
            TimeFrame timeFrame = new TimeFrame(startTime, duration);
            double rate = reader.GetDouble(1);
            int rawData = reader.GetInt32(2);
            var flow = BuildFlow(timeFrame, rate, rawData, log);
            flows.Add(flow);
        }

        public virtual Flow BuildFlow(TimeFrame timeFrame, double rate, int rawData, LogMeter log) {
            return new MeterMasterJetFlow(timeFrame, rate, rawData);
        }
        
        string BuildFlowsCommandText() {
            return "SELECT DateTimeStamp, RateData, RawData FROM MMData ORDER BY DateTimeStamp";
        }

        string BuildCommandMeterText() {
            return "SELECT MeterCode, Make, Model, [Size], Unit, Nutation, LED, StorageInterval, NumberOfIntervals, TotalPulses, BeginReading, EndReading, RegVolume, MMVolume, ConvFactorType, ConvFactor, DatabaseMultiplier, CombinedFile FROM MeterInfo";
        }

        string BuildCommandCustomerText() {
            return "SELECT CustomerID, CustomerName, Address, City, State, PostalCode, PhoneNumber, Note FROM Customer";
        }

        TimeSpan GetDuration(OleDbCommand command) {
            TimeSpan duration = TimeSpan.Zero;
            using (OleDbDataReader reader = command.ExecuteReader()) {
                if (reader.Read()) {
                    DateTime startTime = reader.GetDateTime(0);
                    if (reader.Read()) {
                        DateTime endTime = reader.GetDateTime(0);
                        duration = new TimeFrame(startTime, endTime).Duration;
                    }
                }
            }
            return duration;
        }
    }
}