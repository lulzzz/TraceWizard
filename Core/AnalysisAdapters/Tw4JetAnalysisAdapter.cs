using System;
using System.Data.OleDb;
using System.Text;

using TraceWizard.Entities;
using TraceWizard.Entities.Adapters;
using TraceWizard.Data.Jet;
using TraceWizard.Environment;

namespace TraceWizard.Entities.Adapters.Tw4Jet {

    public class Tw4JetAnalysisAdapter : AnalysisAdapter {
        public Tw4JetAnalysisAdapter() : base() { }

        public Analysis Load(string dataSource) {
            var analysis = new AnalysisDatabase(dataSource);
            try {
                using (OleDbConnection connection = new OleDbConnection(DataServices.BuildJetConnectionString(dataSource, true))) {
                    connection.Open();
                    analysis.FixtureProfiles = LoadFixtureProfiles(connection);
                    analysis.Events = LoadEvents(connection,analysis.FixtureProfiles, GetInterval(connection));
                }
            } catch (System.Data.OleDb.OleDbException ex) {
                if (ex.Message.Contains("Could not find file")) throw new Exception("Could not find file");
                else if (ex.Message.Contains("Unrecognized database format")) throw new Exception("Unrecognized analysis format");
                else throw;
           } catch (InvalidOperationException ex) {
                if (ex.Message.Contains("provider is not registered")) throw new Exception("The MS 2007 Office System Driver is not installed on this system.\r\n\r\nPlease review System Requirements and Installation Troubleshooting.\r\n\r\n(" + ex.Message + ")");
                else throw;
            }
             return analysis;
        }

        FixtureProfiles LoadFixtureProfiles(OleDbConnection connection) {
            FixtureProfiles fixtureProfiles = new FixtureProfiles();
            using (OleDbCommand command = new OleDbCommand(BuildLoadFixtureProfilesCommand(), connection)) {
                using (OleDbDataReader reader = command.ExecuteReader()) {
                    while (reader.Read())
                        fixtureProfiles.Add(BuildFixtureProfile(reader).Normalize());
                }
            }
            return fixtureProfiles;
        }

        TimeSpan GetInterval(OleDbConnection connection) {
            Events events = new Events();
            int seconds = 0;
            using (OleDbCommand command = new OleDbCommand(BuildLoadDurationFlowCommand(), connection)) {
                using (OleDbDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        seconds = GetInterval(connection, reader);
                        break;
                    }
                }
            }
            return new TimeSpan(0, 0, seconds);
        }

        Events LoadEvents(OleDbConnection connection, FixtureProfiles fixtureProfiles, TimeSpan interval) {
            Events events = new Events();
            using (OleDbCommand command = new OleDbCommand(BuildLoadEventsCommand(), connection)) {
                using (OleDbDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        events.Add(BuildEvent(connection, reader, fixtureProfiles, interval));
                    }
                }
            }
            events.UpdateChannel();
            events.UpdateSuperPeak();
            events.UpdateLinkedList();
            events.UpdateOriginalVolume();

            return events;
        }

        public FixtureProfiles LoadFixtureProfiles(string dataSource) {
            FixtureProfiles fixtureProfiles;
            try {
                using (OleDbConnection connection = new OleDbConnection(DataServices.BuildJetConnectionString(dataSource, true))) {
                    connection.Open();
                    fixtureProfiles = LoadFixtureProfiles(connection);
                }
            } catch (System.Data.OleDb.OleDbException ex) {
                if (ex.Message.Contains("Could not find file")) throw new Exception("Could not find file");
                else if (ex.Message.Contains("Unrecognized database format")) throw new Exception("Unrecognized analysis format");
                else throw;
            } catch (InvalidOperationException ex) {
                if (ex.Message.Contains("provider is not registered")) throw new Exception("The MS 2007 Office System Driver is not installed on this system.\r\n\r\nPlease review System Requirements and Installation Troubleshooting.\r\n\r\n(" + ex.Message + ")");
                else throw;
            }
            return fixtureProfiles;
        }

        public Events LoadEvents(string dataSource, FixtureProfiles fixtureProfiles) {
            Events events;
            try {
                using (OleDbConnection connection = new OleDbConnection(DataServices.BuildJetConnectionString(dataSource, true))) {
                    connection.Open();
                    events = LoadEvents(connection,fixtureProfiles, new TimeSpan(0,0,10));
                }
            } catch (System.Data.OleDb.OleDbException ex) {
                if (ex.Message.Contains("Could not find file")) throw new Exception("Could not find file");
                else if (ex.Message.Contains("Unrecognized database format")) throw new Exception("Unrecognized analysis format");
                else throw;
            } catch (InvalidOperationException ex) {
                if (ex.Message.Contains("provider is not registered")) throw new Exception("The MS 2007 Office System Driver is not installed on this system.\r\n\r\nPlease review System Requirements and Installation Troubleshooting.\r\n\r\n(" + ex.Message + ")");
                else throw;
            }
            return events;
        }

        public void Save(string dataSource, Analysis analysis, bool overWrite) {
            throw new NotImplementedException();
        }

        string BuildLoadFixtureProfilesCommand() {
            return "SELECT VirtualFixtureID, Name, MinVol, MaxVol, MinPeak, MaxPeak, MinDur, MaxDur, MinMode, MaxMode, MinModeFreq, MaxModeFreq FROM Fixtures ORDER BY [Order]";
        }

        int GetInterval(OleDbConnection connection, OleDbDataReader reader) {
            int idEvent = reader.GetInt32(0);
            int duration = reader.GetInt32(1);

            int i = 0;
            using (OleDbCommand commandFlows = new OleDbCommand(BuildLoadFlowsCommand(idEvent), connection)) {
                using (OleDbDataReader readerFlows = commandFlows.ExecuteReader()) {
                    while (readerFlows.Read()) {
                        i++;
                    }
                }
            }
            return i > 0 ? duration / i : 0;
        }

        Event BuildEvent(OleDbConnection connection, OleDbDataReader reader, FixtureProfiles fixtureProfiles, TimeSpan interval) {
            int i = 0;
            int idEvent = reader.GetInt32(i++);

            FixtureClass fixtureClass;
            Boolean firstCycle = false;
            if (reader.IsDBNull(i))
                fixtureClass = FixtureClasses.Unclassified;
            else {
                fixtureClass = fixtureProfiles[reader.GetString(i)].FixtureClass;
                firstCycle = reader.GetString(i).Contains("@");
            }
            i++;

            Event @event = new Event(fixtureClass);
            @event.ManuallyClassified = reader.GetBoolean(i++);
            @event.Channel = (Channel)reader.GetInt16(i++);
            @event.FirstCycle = firstCycle;

            using (OleDbCommand commandFlows = new OleDbCommand(BuildLoadFlowsCommand(idEvent), connection)) {
                using (OleDbDataReader readerFlows = commandFlows.ExecuteReader()) {
                    while (readerFlows.Read()) {
                        @event.Add(BuildFlow(readerFlows, interval));
                    }
                }
            }
            return @event;
        }

        FixtureClass FixtureClassFromDatabaseId(int? id) {
            if (!id.HasValue)
                return FixtureClasses.Unclassified;

            switch (id) {
                case 1:
                    return FixtureClasses.Toilet;
                case 2:
                    return FixtureClasses.Dishwasher;
                case 3:
                    return FixtureClasses.Clotheswasher;
                case 4:
                    return FixtureClasses.Shower;
                case 5:
                    return FixtureClasses.Bathtub;
                case 6:
                    return FixtureClasses.Cooler;
                case 7:
                    return FixtureClasses.Pool;
                case 8:
                    return FixtureClasses.Leak;
                case 9:
                    return FixtureClasses.Irrigation;
                case 10:
                    return FixtureClasses.Faucet;
                case 11:
                    return FixtureClasses.Other;
                case 12:
                    return FixtureClasses.Humidifier;
                case 13:
                    return FixtureClasses.Hottub;
                default:
                    return FixtureClasses.Unclassified;
            }
        }

        FixtureProfile BuildFixtureProfile(OleDbDataReader reader) {
            int i = -1;
            FixtureClass fixtureClass;
            if (reader.IsDBNull(++i)) fixtureClass = FixtureClasses.Unclassified;
            else fixtureClass = FixtureClassFromDatabaseId(reader.GetInt32(i));

            string name;
            if (reader.IsDBNull(++i)) name = null;
            else name = reader.GetString(i);

            double? minVolume;
            if (reader.IsDBNull(++i)) minVolume = null;
            else minVolume = (double)reader.GetFloat(i);

            double? maxVolume;
            if (reader.IsDBNull(++i)) maxVolume = null;
            else maxVolume = (double)reader.GetFloat(i);

            double? minPeak;
            if (reader.IsDBNull(++i)) minPeak = null;
            else minPeak = (double)reader.GetFloat(i);

            double? maxPeak;
            if (reader.IsDBNull(++i)) maxPeak = null;
            else maxPeak = (double)reader.GetFloat(i);

            TimeSpan? minDuration;
            if (reader.IsDBNull(++i)) minDuration = null;
            else minDuration = new TimeSpan(0, 0, (reader.GetInt32(i)));

            TimeSpan? maxDuration;
            if (reader.IsDBNull(++i)) maxDuration = null;
            else maxDuration = new TimeSpan(0, 0, (reader.GetInt32(i)));

            double? minMode;
            if (reader.IsDBNull(++i)) minMode = null;
            else minMode = (double)reader.GetFloat(i);

            double? maxMode;
            if (reader.IsDBNull(++i)) maxMode = null;
            else maxMode = (double)reader.GetFloat(i);

            int? minModeFrequency;
            if (reader.IsDBNull(++i)) minModeFrequency = null;
            else minModeFrequency = reader.GetInt32(i);

            int? maxModeFrequency;
            if (reader.IsDBNull(++i)) maxModeFrequency = null;
            else maxModeFrequency = reader.GetInt32(i);

            return new FixtureProfile(name, fixtureClass, minVolume, maxVolume, minPeak, maxPeak,
                minDuration, maxDuration, minMode, maxMode);
        }

        string BuildLoadEventsCommand() {
            return "SELECT Events.ID, Fixtures.Name, EventFixtures.Preserved, Events.Class FROM Events INNER JOIN" +
                " (EventFixtures LEFT JOIN (Fixtures LEFT JOIN VirtualFixtures" +
                " On Fixtures.VirtualFixtureID=VirtualFixtures.ID) On EventFixtures.IDFixture=Fixtures.ID)" +
                " On Events.ID=EventFixtures.IDEvent ORDER BY Events.StartTime";
        }

        string BuildLoadFlowsCommand(int id) {
            return "SELECT StartTime, Rate FROM Flows WHERE EventID = " + id.ToString() + " ORDER BY StartTime";
        }

        string BuildLoadDurationFlowCommand() {
            return "SELECT Events.ID, Duration from Events";
        }

        Flow BuildFlow(OleDbDataReader reader, TimeSpan duration) {
            int i = 0;
            DateTime startTime = reader.GetDateTime(i++);
            Double rate = reader.GetFloat(i++);
            return new Flow(new TimeFrame(startTime, duration), rate);
        }
    }
}
