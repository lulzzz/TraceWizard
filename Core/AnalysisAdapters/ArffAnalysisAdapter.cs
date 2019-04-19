using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Text;

using TraceWizard.Entities;
using TraceWizard.Logging;
using TraceWizard.Logging.Adapters;
using TraceWizard.Entities.Adapters;
using TraceWizard.Environment;

namespace TraceWizard.Entities.Adapters.Arff {

    public class ArffAttributes {
        public List<FixtureClass> FixtureClasses { get; set; }

        public bool IsKeyCodeEnabled { get; set; }
        public bool IsEventIdEnabled { get; set; }

        public bool IsStartTimeEnabled { get; set; }
        public bool IsEndTimeEnabled { get; set; }
        public bool IsDurationEnabled { get; set; }

        public bool IsFirstCycleEnabled { get; set; }

        public bool IsManuallyClassifiedEnabled { get; set; }

        public bool IsChannelEnabled { get; set; }

        public bool IsFixtureClassEnabled { get { return true; } }

        public bool IsVolumeEnabled { get; set; }
        public bool IsPeakEnabled { get; set; }
        public bool IsModeEnabled { get; set; }

        public bool IsModeFrequencyEnabled { get; set; }

        public bool IsHourEnabled { get; set; }
        public bool IsIsWeekendEnabled { get; set; }
        public bool IsTimeToLongerEventEnabled { get; set; }

        public bool IsUserNotesEnabled { get; set; }

        public bool IsClassifiedUsingFixtureListEnabled { get; set; }
        public bool IsManuallyApprovedEnabled { get; set; }
        public bool IsManuallyClassifiedFirstCycleEnabled { get; set; }
    }

    public class NativeArffAttributes : ArffAttributes {
        public NativeArffAttributes() {

            IsEventIdEnabled = true;

            IsStartTimeEnabled = true;
            IsEndTimeEnabled = true;
            IsDurationEnabled = true;

            IsFirstCycleEnabled = true;

            IsManuallyClassifiedEnabled = true;

            IsChannelEnabled = true;

            IsVolumeEnabled = true;
            IsPeakEnabled = true;
            IsModeEnabled = true;

            IsUserNotesEnabled = true;

            FixtureClasses = new List<FixtureClass>();
            foreach (FixtureClass fixtureClass in TraceWizard.Entities.FixtureClasses.Items.Values) {
                FixtureClasses.Add(fixtureClass);
            }

            IsClassifiedUsingFixtureListEnabled = true;
            IsManuallyApprovedEnabled = true;
            IsManuallyClassifiedFirstCycleEnabled = true;
        }
    }

    public class EventsArff : Events {

        public void CalculateProperties() {
            foreach (ArffEvent eventArff in this)
                eventArff.CalculateProperties();
        }
        
    }

    public class ArffEvent : Event {

        public ArffEvent(Event @event, TimeSpan maxDuration)
            : base() {
            Volume = @event.Volume;
            Peak = @event.Peak;
            TimeFrame = @event.TimeFrame;
            Mode = @event.Mode;
            ModeFrequency = @event.ModeFrequency;

            Channel = @event.Channel;

            FixtureClass = @event.FixtureClass;

            MaxDuration = maxDuration;

            UserNotes = @event.UserNotes;

            ClassifiedUsingFixtureList = @event.ClassifiedUsingFixtureList;
            ManuallyApproved = @event.ManuallyApproved;
            ManuallyClassifiedFirstCycle = @event.ManuallyClassifiedFirstCycle;

        }

        public void CalculateProperties() {
            hour = CalculateHour();
            isWeekend = CalculateIsWeekend();
            timeToLongerEvent = CalculateTimeToLongerEvent();
        }

        public TimeSpan MaxDuration { get; protected set; }

        private int hour;
        public int Hour { get { return hour; } }
        public int CalculateHour() {
            return StartTime.Hour;
        }

        private bool isWeekend;
        public bool IsWeekend { get { return isWeekend; } }
        public bool CalculateIsWeekend() {
            return (StartTime.DayOfWeek == DayOfWeek.Saturday) || (StartTime.DayOfWeek == DayOfWeek.Sunday);
        }

        private TimeSpan timeToLongerEvent;
        public TimeSpan TimeToLongerEvent { get { return timeToLongerEvent; } }
        public TimeSpan CalculateTimeToLongerEvent() {
            ArffEvent previousWork = (ArffEvent)Previous;
            while (previousWork != null)
                if (previousWork.Duration > Duration)
                    break;
                else
                    previousWork = (ArffEvent)previousWork.Previous;

            ArffEvent nextWork = (ArffEvent)Next;
            while (nextWork != null)
                if (nextWork.Duration > Duration)
                    break;
                else
                    nextWork = (ArffEvent)nextWork.Next;

            if (previousWork == null && nextWork == null)
                return MaxDuration;
            if (previousWork == null)
                return nextWork.StartTime.Subtract(StartTime);
            if (nextWork == null)
                return StartTime.Subtract(previousWork.StartTime);
            else
                return (StartTime.Subtract(previousWork.StartTime) < nextWork.StartTime.Subtract(StartTime)) ?
                    StartTime.Subtract(previousWork.StartTime)
                    : nextWork.StartTime.Subtract(StartTime);
        }
        //public ArffEvent Previous { get; set; }
        //public ArffEvent Next { get; set; }
    }

    public class ArffAnalysisAdapter : AnalysisAdapter {

        public ArffAnalysisAdapter() {}

        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        public ArffAttributes Attributes { get; set; }

        private double GetTwConversionFactor(string[] lines) {
            double conversionFactor = 1.00;
            foreach (string line in lines) {
                if (line.Contains("@DATA"))
                    break;

                if (line.Contains("% TwConversionFactor: ")) {
                    conversionFactor = double.Parse(line.Split(':')[1].Trim());
                }
            }
            return conversionFactor;
        }

        private Version GetCurrentFileVersion(string[] lines) {
            Version version = TwAssembly.Version();

            foreach (string line in lines) {
                if (line.Contains("@DATA"))
                    break;

                if (line.Contains("% Version: ")) {
                    version = new Version(line.Split(':')[1].Trim());
                }
            }
            return version;
        }
 
        private Version MinimumFileVersion() {
            return new Version(5, 0, 30, 1);
        }

        private bool IsOldVersion(string[] lines) {
            Version fileVersion = GetCurrentFileVersion(lines);
            Version appVersion = MinimumFileVersion(); ;
            if (fileVersion == appVersion || TwAssembly.IsVersionNewer(appVersion, fileVersion))
                return false; 
            else
                return true;
        }
        
        public Analysis Load(string dataSource) {
            string[] lines = System.IO.File.ReadAllLines(dataSource);

            if (IsOldVersion(lines))
                throw new Exception("Please migrate the analysis file " + dataSource + " to version " + TwAssembly.Version() +  " or later.");

            double twConversionFactor = GetTwConversionFactor(lines);
            
            Attributes = LoadAttributes(lines);

            LogMeter log = LoadLog(lines);

            FixtureProfiles fixtureProfiles = LoadFixtureProfiles(lines);

            Events events = LoadEvents(lines);

            LoadFlows(lines, events);

            events.UpdateVolume();

            var analysis = new AnalysisDatabase(dataSource,events, log, fixtureProfiles);

            analysis.Events.ConversionFactor = twConversionFactor;
            analysis.Events.UpdateChannel();
            analysis.Events.UpdateSuperPeak();
            analysis.Events.UpdateLinkedList();
            analysis.Events.UpdateOriginalVolume();

            return analysis;
        }

        bool ContainsAttribute(string line, string attribute) {
            return line.Contains("@ATTRIBUTE " + attribute);
        }

        ArffAttributes LoadAttributes(string[] lines) {
            var attributes = new ArffAttributes();

            foreach (string line in lines) {
                if (line.Contains("@DATA"))
                    break;

                if (ContainsAttribute(line, "keycode"))
                    attributes.IsKeyCodeEnabled = true;
                if (ContainsAttribute(line, "eventid"))
                    attributes.IsEventIdEnabled = true;

                if (ContainsAttribute(line, "starttime"))
                    attributes.IsStartTimeEnabled = true;
                if (ContainsAttribute(line, "endtime"))
                    attributes.IsEndTimeEnabled = true;
                if (ContainsAttribute(line, "duration"))
                    attributes.IsDurationEnabled = true;

                if (ContainsAttribute(line, "firstcycle"))
                    attributes.IsFirstCycleEnabled = true;

                if (ContainsAttribute(line, "preserved"))
                    attributes.IsManuallyClassifiedEnabled = true;

                if (ContainsAttribute(line, "channel"))
                    attributes.IsChannelEnabled = true;

                if (ContainsAttribute(line, "volume"))
                    attributes.IsVolumeEnabled = true;
                if (ContainsAttribute(line, "peak"))
                    attributes.IsPeakEnabled = true;
                if (ContainsAttribute(line, "mode"))
                    attributes.IsModeEnabled = true;

                if (ContainsAttribute(line, "modefrequency"))
                    attributes.IsModeFrequencyEnabled = true;

                if (ContainsAttribute(line, "hour"))
                    attributes.IsHourEnabled = true;
                if (ContainsAttribute(line, "isweekend"))
                    attributes.IsIsWeekendEnabled = true;
                if (ContainsAttribute(line, "timetolongerevent"))
                    attributes.IsTimeToLongerEventEnabled = true;

                if (ContainsAttribute(line, "notes"))
                    attributes.IsUserNotesEnabled = true;

                if (ContainsAttribute(line, "class")) {
                    attributes.FixtureClasses = new List<FixtureClass>();
                    foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values)
                        attributes.FixtureClasses.Add(fixtureClass);
                }

                if (ContainsAttribute(line, "classifiedusingfixturelist"))
                    attributes.IsClassifiedUsingFixtureListEnabled = true;
                if (ContainsAttribute(line, "manuallyapproved"))
                    attributes.IsManuallyApprovedEnabled = true;
                if (ContainsAttribute(line, "manuallyclassifiedfirstcycle"))
                    attributes.IsManuallyClassifiedFirstCycleEnabled = true;
            }

            return attributes;
        }

        string ReadStringValue(string line) {
            return line.Split(',')[1];
        }

        DateTime ReadDateTimeValue(string line) {
            string s = line.Split(',')[1];
            if (string.IsNullOrEmpty(s) || s == "\"\"")
                return DateTime.MinValue;
            else
                return DateTime.Parse(TrimQuotes(s));
        }
        
        double? ReadDoubleValue(string line) {
            string s = line.Split(',')[1];
            if (string.IsNullOrEmpty(s))
                return null;
            else
                return double.Parse(s);
        }

        int? ReadIntegerValue(string line) {
            string s = line.Split(',')[1];
            if (string.IsNullOrEmpty(s))
                return null;
            else
                return int.Parse(s);
        }

        bool? ReadBoolValue(string line) {
            string s = line.Split(',')[1];
            if (string.IsNullOrEmpty(s))
                return null;
            else
                return bool.Parse(s);
        }

        string[] ReadAttributeValues(string line) {
            return line.Split(',');
        }

        string TrimQuotes(string s) {
            return s.Trim('\"');
        }
        
        Event ReadEvent(string[] attributeValues) {
            if (attributeValues == null || attributeValues[0] == string.Empty)
                return null;

            var @event = new Event();

            int i = 0;

            if (Attributes.IsKeyCodeEnabled)
                i++;

            if (Attributes.IsEventIdEnabled)
                i++;

            DateTime? startTime = null;
            if (Attributes.IsStartTimeEnabled)
                startTime = DateTime.Parse(TrimQuotes(attributeValues[i++]));

            DateTime? endTime = null;
            if (Attributes.IsEndTimeEnabled)
                endTime = DateTime.Parse(TrimQuotes(attributeValues[i++]));

            if (startTime != null && endTime != null)
                @event.TimeFrame = new TimeFrame(startTime.Value, endTime.Value);

            TimeSpan? duration = null;
            if (Attributes.IsDurationEnabled)
                duration = new TimeSpan(0,0,int.Parse(TrimQuotes(attributeValues[i++])));

            if (duration != null) {
                if (startTime == null)
                    @event.TimeFrame = new TimeFrame(DateTime.MinValue,duration.Value);
                else if (endTime == null)
                    @event.TimeFrame = new TimeFrame(startTime.Value,duration.Value);
            }

            if (Attributes.IsFirstCycleEnabled)
                @event.FirstCycle = Convert.ToBoolean(attributeValues[i++]);

            if (Attributes.IsManuallyClassifiedEnabled)
                @event.ManuallyClassified = Convert.ToBoolean(attributeValues[i++]);
            
            if (Attributes.IsChannelEnabled)
                @event.Channel = (Channel)Enum.Parse(typeof(Channel), attributeValues[i++]);

            if (Attributes.IsVolumeEnabled)
                @event.Volume = double.Parse(attributeValues[i++]);
            if (Attributes.IsPeakEnabled)
                @event.Peak = double.Parse(attributeValues[i++]);
            if (Attributes.IsModeEnabled)
                @event.Mode = double.Parse(attributeValues[i++]);
            if (Attributes.IsModeFrequencyEnabled)
                @event.ModeFrequency = int.Parse(attributeValues[i++]);

            if (Attributes.IsHourEnabled)
                i++;
            if (Attributes.IsIsWeekendEnabled)
                i++;
            if (Attributes.IsTimeToLongerEventEnabled)
                i++;

            if (Attributes.IsUserNotesEnabled)
                @event.UserNotes = attributeValues[i++].ToString();

            if (Attributes.IsFixtureClassEnabled)
                @event.FixtureClass = FixtureClasses.Items[attributeValues[i++]];

            if (Attributes.IsClassifiedUsingFixtureListEnabled)
                @event.ClassifiedUsingFixtureList = Convert.ToBoolean(attributeValues[i++]);

            if (Attributes.IsManuallyApprovedEnabled)
                @event.ManuallyApproved = Convert.ToBoolean(attributeValues[i++]);

            if (Attributes.IsManuallyClassifiedFirstCycleEnabled)
                @event.ManuallyClassifiedFirstCycle = Convert.ToBoolean(attributeValues[i++]);

            return @event;
        }
        
        Events LoadEvents(string[] lines) {
            Events events = new Events();
            int i = 0;

            bool dataFound = false;
            for (; i < lines.Length; i++) {
                if (lines[i].Contains("@DATA")) {
                    i++;
                    dataFound = true;
                    break;
                }
            }

            if (dataFound) {
                for (; i < lines.Length; i++) {
                    if (lines[i].Contains("@FLOW"))
                        break;
                    Event @event = ReadEvent(ReadAttributeValues(lines[i]));
                    if (@event != null)
                        events.Add(@event);
                }
            }
            return events;
        }

        public EventsArff Load(Events events) {
            EventsArff eventsArff = new EventsArff();

            IEnumerable<Event> eventsSortedChannel = Enumerable.OrderBy(events, n => n.Channel);
            IEnumerable<Event> eventsSorted = Enumerable.OrderBy(eventsSortedChannel, n => n.StartTime);
            
            foreach (Event @event in eventsSorted) {
                ArffEvent eventArff = new ArffEvent(@event,events.Duration);
                eventsArff.Add(eventArff);
            }

            eventsArff.UpdateLinkedList();
            eventsArff.CalculateProperties();
            eventsArff.UpdateOriginalVolume();

            return eventsArff;
        }

        public FixtureProfiles LoadFixtureProfiles(string dataSource) {
            throw new NotImplementedException();
        }

        public Events LoadEvents(string dataSource, FixtureProfiles fixtureProfiles) {
            throw new NotImplementedException();
        }

        public void Save(string dataSource, Analysis analysis, bool overWrite) {
            System.Text.StringBuilder text = new System.Text.StringBuilder();

            if (Attributes == null)
                Attributes = new NativeArffAttributes();
            if (overWrite && System.IO.File.Exists(dataSource))
                System.IO.File.Delete(dataSource);
            if (!System.IO.File.Exists(dataSource)) {
                WriteHeader(text, dataSource, analysis.Events.ConversionFactor);

                WriteFixtureProfiles(text, analysis.FixtureProfiles);
                WriteLog(text, analysis.Log);
                WriteEventsHeader(text);
            }
            WriteEvents(text, analysis.Events, analysis.KeyCode);
            if (FlowsExist(analysis))
                WriteFlows(text, analysis.Events);
            System.IO.File.AppendAllText(dataSource, text.ToString());
        }

        bool FlowsExist(Analysis analysis) {
            return (analysis.Events.Count > 0 && analysis.Events[0].Count > 0);
        }
        
        void WriteHeader(System.Text.StringBuilder text, string dataSource, double conversionFactor) {

            text.AppendLine("% Format: Trace Wizard Analysis");
            text.AppendLine("% Version: " + TwAssembly.Version());
            text.AppendLine("% TwConversionFactor: " + conversionFactor.ToString("0.000000"));
            text.AppendLine("%");
            text.AppendLine("@RELATION event");
            text.AppendLine();

            if (Attributes.IsKeyCodeEnabled)
                text.AppendLine("@ATTRIBUTE keycode           STRING");

            if (Attributes.IsEventIdEnabled)
                text.AppendLine("@ATTRIBUTE eventid           NUMERIC");

            if (Attributes.IsStartTimeEnabled)
                text.AppendLine("@ATTRIBUTE starttime         DATE \"yyyy-MM-dd HH:mm:ss\"");

            if (Attributes.IsEndTimeEnabled)
                text.AppendLine("@ATTRIBUTE endtime           DATE \"yyyy-MM-dd HH:mm:ss\"");

            if (Attributes.IsDurationEnabled)
                text.AppendLine("@ATTRIBUTE duration          NUMERIC");

            if (Attributes.IsFirstCycleEnabled)
                text.AppendLine("@ATTRIBUTE firstcycle        {True,False}");

            if (Attributes.IsManuallyClassifiedEnabled)
                text.AppendLine("@ATTRIBUTE preserved         {True,False}");

            if (Attributes.IsChannelEnabled)
                text.AppendLine("@ATTRIBUTE channel           {None,Runt,Trickle,Base,Super}");

            if (Attributes.IsVolumeEnabled)
                text.AppendLine("@ATTRIBUTE volume            NUMERIC");

            if (Attributes.IsPeakEnabled)
                text.AppendLine("@ATTRIBUTE peak              NUMERIC");

            if (Attributes.IsModeEnabled)
                text.AppendLine("@ATTRIBUTE mode              NUMERIC");

            if (Attributes.IsModeFrequencyEnabled)
                text.AppendLine("@ATTRIBUTE modefrequency     NUMERIC");

            if (Attributes.IsHourEnabled)
                text.AppendLine("@ATTRIBUTE hour              NUMERIC");

            if (Attributes.IsIsWeekendEnabled)
                text.AppendLine("@ATTRIBUTE isweekend         {True,False}");

            if (Attributes.IsTimeToLongerEventEnabled)
                text.AppendLine("@ATTRIBUTE timetolongerevent NUMERIC");

            if (Attributes.IsUserNotesEnabled)
                text.AppendLine("@ATTRIBUTE notes             STRING");

            if (Attributes.IsFixtureClassEnabled)
                text.AppendLine("@ATTRIBUTE class             {" + BuildFixtureClasses() + "}");

            if (Attributes.IsClassifiedUsingFixtureListEnabled)
                text.AppendLine("@ATTRIBUTE classifiedusingfixturelist {True,False}");

            if (Attributes.IsManuallyApprovedEnabled)
                text.AppendLine("@ATTRIBUTE manuallyapproved  {True,False}");

            if (Attributes.IsManuallyClassifiedFirstCycleEnabled)
                text.AppendLine("@ATTRIBUTE manuallyclassifiedfirstcycle  {True,False}");

            text.AppendLine();
        }

        void WriteLine(System.Text.StringBuilder text) {
            text.AppendLine();
        }

        void WriteEventsHeader(System.Text.StringBuilder text) {
            text.Append("@DATA");
            text.AppendLine();
        }

        void WriteEvents(System.Text.StringBuilder text, Events events, string keyCode) {
            int i = 0;
            foreach (Event @event in events) {
                if (Attributes.FixtureClasses.Contains(@event.FixtureClass))
                    WriteEvent(text, @event, events.Duration, keyCode, i++);
            }
        }

        string WrapInQuotes(string s) {
            return "\"" + s + "\"";
        }
        
        void WriteEvent(System.Text.StringBuilder text, Event @event, TimeSpan maxDuration, string keyCode, int eventId) {

            if (Attributes.IsKeyCodeEnabled) {
                text.Append(keyCode);
                text.Append(",");
            }

            if (Attributes.IsEventIdEnabled) {
                text.Append(eventId);
                text.Append(",");
            }

            if (Attributes.IsStartTimeEnabled) {
                text.Append(WrapInQuotes(@event.StartTime.ToString(DateTimeFormat)));
                text.Append(",");
            }

            if (Attributes.IsEndTimeEnabled) {
                text.Append(WrapInQuotes(@event.EndTime.ToString(DateTimeFormat)));
                text.Append(",");
            }

            if (Attributes.IsDurationEnabled) {
                text.Append(@event.Duration.TotalSeconds);
                text.Append(",");
            }

            if (Attributes.IsFirstCycleEnabled) {
                text.Append(@event.FirstCycle == true ? true.ToString() : false.ToString());
                text.Append(",");
            }

            if (Attributes.IsManuallyClassifiedEnabled) {
                text.Append(@event.ManuallyClassified == true ? true.ToString() : false.ToString());
                text.Append(",");
            }

            if (Attributes.IsChannelEnabled) {
                text.Append(@event.Channel);
                text.Append(",");
            }

            if (Attributes.IsVolumeEnabled) {
                text.Append(@event.Volume);
                text.Append(",");
            }

            if (Attributes.IsPeakEnabled) {
                text.Append(@event.Peak);
                text.Append(",");
            }

            if (Attributes.IsModeEnabled) {
                text.Append(@event.Mode); 
                text.Append(",");
            }

            if (Attributes.IsModeFrequencyEnabled) {
                text.Append(@event.ModeFrequency);
                text.Append(",");
            }

            if (typeof(ArffEvent).IsAssignableFrom(@event.GetType())) {
                var eventArff = @event as ArffEvent;

                if (Attributes.IsHourEnabled) {
                    text.Append(@eventArff.Hour.ToString());
                    text.Append(",");
                }

                if (Attributes.IsIsWeekendEnabled) {
                    text.Append(@eventArff.IsWeekend == true ? true.ToString() : false.ToString());
                    text.Append(",");
                }

                if (Attributes.IsTimeToLongerEventEnabled) {
                    text.Append(@eventArff.TimeToLongerEvent.TotalSeconds.ToString());
                    text.Append(",");
                }
            }

            if (Attributes.IsUserNotesEnabled) {
                text.Append(@event.UserNotes);
                text.Append(",");
            }

            if (Attributes.IsFixtureClassEnabled) {
                text.Append(@event.FixtureClass.Name);
                text.Append(",");
            }

            if (Attributes.IsClassifiedUsingFixtureListEnabled) {
                text.Append(@event.ClassifiedUsingFixtureList);
                text.Append(",");
            }

            if (Attributes.IsManuallyApprovedEnabled) {
                text.Append(@event.ManuallyApproved);
                text.Append(",");
            }

            if (Attributes.IsManuallyClassifiedFirstCycleEnabled) {
                text.Append(@event.ManuallyClassifiedFirstCycle);
            }

// WARNING: When adding new attributes, make sure that penultimate appends comma, but that last one does not!
            
            text.AppendLine();
        }

        FixtureProfiles LoadFixtureProfiles(string[] lines) {
            FixtureProfiles fixtureProfiles = new FixtureProfiles();
            bool fixtureProfilesFound = false;
            foreach (string line in lines) {
                if (line.Contains("@FIXTURE PROFILES")) {
                    fixtureProfilesFound = true;
                } else if (fixtureProfilesFound){
                    if (string.IsNullOrEmpty(line) && !line.StartsWith("% ") || line.Contains("@DATA"))
                        break;
                    else if (!line.StartsWith("% FixtureClass")) {
                        FixtureProfile fixtureProfile = new FixtureProfile(ToStringArray(line,','));
                        fixtureProfiles.Add(fixtureProfile);
                    }
                }
            }
            return fixtureProfiles;
        }

        string[] ToStringArray(string data, char token) {
            return data.Trim().Substring(2).Split(token);
        }
        
        LogMeter LoadLog(string[] lines) {
            bool logFound = false;
            foreach (string line in lines) {
                if (line.Contains("@LOG")) {
                    logFound = true;
                    break;
                }
            }

            LogMeter log = null;
            if (logFound) {
                log = new LogMeter();

                ReadLogDates(lines, log);
                ReadLogFileName(lines, log);
                
                log.Customer = ReadCustomer(lines);
                log.Meter = ReadMeter(lines);
            }

            return log;
        }

        bool CommentStartsWith(string line, string label) {
            return line.StartsWith("% " + label);
        }
        
        void ReadLogDates(string[] lines, LogMeter log) {
            foreach (string line in lines) {
                if (line.StartsWith("@DATA"))
                    break;

                if (CommentStartsWith(line,LogMeter.StartTimeLabel))
                    log.StartTime = ReadDateTimeValue(line);
                if (CommentStartsWith(line, LogMeter.EndTimeLabel))
                    log.EndTime = ReadDateTimeValue(line);
            }
        }

        void ReadLogFileName(string[] lines, LogMeter log) {
            foreach (string line in lines) {
                if (line.StartsWith("@DATA"))
                    break;

                if (CommentStartsWith(line, LogMeter.FileNameLabel))
                    log.FileName = ReadStringValue(line);
            }
        }

        LogMeterMeter ReadMeter(string[] lines) {
            var meter = new LogMeterMeter();
            foreach (string line in lines) {
                if (CommentStartsWith(line, "@DATA"))
                    break;

                if (CommentStartsWith(line, LogMeterMeter.CodeLabel))
                    meter.Code = ReadIntegerValue(line);
                if (CommentStartsWith(line, LogMeterMeter.MakeLabel))
                    meter.Make = ReadStringValue(line);
                if (CommentStartsWith(line, LogMeterMeter.ModelLabel))
                    meter.Model = ReadStringValue(line);
                if (CommentStartsWith(line, LogMeterMeter.SizeLabel))
                    meter.Size = ReadStringValue(line);
                if (CommentStartsWith(line, LogMeterMeter.UnitLabel))
                    meter.Unit = ReadStringValue(line);
                if (CommentStartsWith(line, LogMeterMeter.LedLabel))
                    meter.Led = ReadDoubleValue(line);
                if (CommentStartsWith(line, LogMeterMeter.NutationLabel))
                    meter.Nutation = ReadDoubleValue(line);
                if (CommentStartsWith(line, LogMeterMeter.StorageIntervalLabel))
                    meter.StorageInterval = ReadIntegerValue(line);
                if (CommentStartsWith(line, LogMeterMeter.NumberOfIntervalsLabel))
                    meter.NumberOfIntervals = ReadIntegerValue(line);
                if (CommentStartsWith(line, LogMeterMeter.TotalTimeLabel))
                    meter.TotalTime = ReadStringValue(line);
                if (CommentStartsWith(line, LogMeterMeter.TotalPulsesLabel))
                    meter.TotalPulses = ReadIntegerValue(line);
                if (CommentStartsWith(line, LogMeterMeter.BeginReadingLabel))
                    meter.BeginReading = ReadDoubleValue(line);
                if (CommentStartsWith(line, LogMeterMeter.EndReadingLabel))
                    meter.EndReading = ReadDoubleValue(line);
                if (CommentStartsWith(line, LogMeterMeter.RegisterVolumeLabel))
                    meter.RegisterVolume = ReadDoubleValue(line);
                if (CommentStartsWith(line, LogMeterMeter.MeterMasterVolumeLabel))
                    meter.MeterMasterVolume = ReadDoubleValue(line);
                if (CommentStartsWith(line, LogMeterMeter.ConversionFactorTypeLabel))
                    meter.ConversionFactorType = ReadIntegerValue(line);
                if (CommentStartsWith(line, LogMeterMeter.ConversionFactorLabel + ","))
                    meter.ConversionFactor = ReadDoubleValue(line);
                if (CommentStartsWith(line, LogMeterMeter.DatabaseMultiplierLabel))
                    meter.DatabaseMultiplier = ReadDoubleValue(line);
                if (CommentStartsWith(line, LogMeterMeter.CombinedFileLabel))
                    meter.CombinedFile = ReadIntegerValue(line);
                if (CommentStartsWith(line, LogMeterMeter.DoublePulseLabel))
                    meter.DoublePulse = ReadBoolValue(line);
            }
            return meter;
        }

        LogMeterCustomer ReadCustomer(string[] lines) {
            var customer = new LogMeterCustomer();

            foreach (string line in lines) {
                if (CommentStartsWith(line, "@DATA"))
                    break;

                if (CommentStartsWith(line, LogMeterCustomer.IdLabel))
                    customer.ID = ReadStringValue(line);
                if (CommentStartsWith(line, LogMeterCustomer.NameLabel))
                    customer.Name = ReadStringValue(line);
                if (CommentStartsWith(line, LogMeterCustomer.AddressLabel))
                    customer.Address = ReadStringValue(line);
                if (CommentStartsWith(line, LogMeterCustomer.CityLabel))
                    customer.City = ReadStringValue(line);
                if (CommentStartsWith(line, LogMeterCustomer.StateLabel))
                    customer.State = ReadStringValue(line);
                if (CommentStartsWith(line, LogMeterCustomer.PostalCodeLabel))
                    customer.PostalCode = ReadStringValue(line);
                if (CommentStartsWith(line, LogMeterCustomer.PhoneNumberLabel))
                    customer.PhoneNumber = ReadStringValue(line);
                if (CommentStartsWith(line, LogMeterCustomer.NoteLabel))
                    customer.Note = ReadStringValue(line);
            }
            return customer;
        }
    
        void LoadFlows(string[] lines, Events events) {
            int i = 0;
            bool flowFound = false;

            for (; i < lines.Length; i++) {
                if (lines[i].Contains("@FLOW")) {
                    i++;
                    flowFound = true;
                    break;
                }
            }

            if (flowFound) {
                for (; i < lines.Length; i++) {
                    int id;
                    Flow flow = ReadFlow(ReadAttributeValues(lines[i]), out id);
                    if (flow != null)
                        events[id].AddWithoutVolume(flow);
                }
            }
        }

        Flow ReadFlow(string[] attributes, out int id) {
            if (attributes == null) {
                id = -1;
                return null;
            }

            id = int.Parse(attributes[0].Substring(2));
            DateTime startTime = DateTime.Parse(TrimQuotes(attributes[1]));
            TimeSpan duration = new TimeSpan(0, 0, int.Parse(attributes[2]));
            double rate = double.Parse(attributes[3]);

            Flow flow = new Flow(startTime, duration, rate);

            return flow;
        }

        void WriteFixtureProfiles(System.Text.StringBuilder text, FixtureProfiles fixtureProfiles) {
            if (fixtureProfiles != null) {
                WriteFixtureProfilesHeader(text);
                WriteFixtureProfilesData(text, fixtureProfiles);
            }
        }

        void WriteFixtureProfilesHeader(System.Text.StringBuilder text) {
            text.Append("% @FIXTURE PROFILES");
            WriteLine(text);
            text.Append("% FixtureClass,MinVolume,MaxVolume,MinPeak,MaxPeak,MinDuration,MaxDuration,MinMode,MaxMode");
            WriteLine(text);
        }

        void WriteFixtureProfilesData(System.Text.StringBuilder text, FixtureProfiles fixtureProfiles) {
            foreach (FixtureProfile fixtureProfile in fixtureProfiles) {
                WriteFixtureProfile(text, fixtureProfile);
                WriteLine(text);
            }
            WriteLine(text);
        }

        void WriteFixtureProfile(System.Text.StringBuilder text, FixtureProfile fixtureProfile) {
            string s = "% "
                + fixtureProfile.FixtureClass.Name + ","
                + fixtureProfile.MinVolume + ","
                + fixtureProfile.MaxVolume + ","
                + fixtureProfile.MinPeak + ","
                + fixtureProfile.MaxPeak + ","
                + (fixtureProfile.MinDuration.HasValue ? fixtureProfile.MinDuration.Value.TotalSeconds.ToString() : null) + ","
                + (fixtureProfile.MaxDuration.HasValue ? fixtureProfile.MaxDuration.Value.TotalSeconds.ToString() : null) + ","
                + fixtureProfile.MinMode + ","
                + fixtureProfile.MaxMode;

            text.Append(s);
        }

        void WriteLog(System.Text.StringBuilder text, Log log) {
            LogMeter meterMasterLog = log as LogMeter;

            if (meterMasterLog != null) {
                WriteLine(text);
                WriteLogHeader(text);
                WriteLogData(text, meterMasterLog);
                WriteLine(text);
            }
        }

        void WriteLogHeader(System.Text.StringBuilder text) {
            text.Append("% @LOG");
            text.AppendLine();
        }

        void WriteLine(System.Text.StringBuilder text, string label, string value) {
            text.Append("% ");
            text.Append(label);
            text.Append(",");
            text.Append(value);
            text.AppendLine();
        }

        void WriteLine(System.Text.StringBuilder text, string label, double? value) {
            text.Append("% ");
            text.Append(label);
            text.Append(",");
            text.Append(value.HasValue? value.ToString() : string.Empty);
            text.AppendLine();
        }

        void WriteLine(System.Text.StringBuilder text, string label, int? value) {
            text.Append("% ");
            text.Append(label);
            text.Append(",");
            text.Append(value.HasValue? value.ToString() : string.Empty);
            text.AppendLine();
        }

        void WriteLine(System.Text.StringBuilder text, string label, bool? value) {
            text.Append("% ");
            text.Append(label);
            text.Append(",");
            text.Append(value.HasValue ? value.ToString() : string.Empty);
            text.AppendLine();
        }

        void WriteLogData(System.Text.StringBuilder text, LogMeter log) {
            WriteLogDates(text, log);
            WriteLogFileName(text, log);
            WriteCustomer(text, log.Customer);
            WriteMeter(text, log.Meter);
        }

        void WriteLogDates(System.Text.StringBuilder text, Log log) {
            WriteLine(text, Log.StartTimeLabel, WrapInQuotes(log.StartTime == DateTime.MinValue ? string.Empty : log.StartTime.ToString(DateTimeFormat)));
            WriteLine(text, Log.EndTimeLabel, WrapInQuotes(log.EndTime == DateTime.MinValue ? string.Empty : log.EndTime.ToString(DateTimeFormat)));
        }

        void WriteLogFileName(System.Text.StringBuilder text, Log log) {
            WriteLine(text, Log.FileNameLabel, log.FileName);
        }

        void WriteCustomer(System.Text.StringBuilder text, LogMeterCustomer customer) {
            WriteLine(text, LogMeterCustomer.IdLabel, customer.ID);
            WriteLine(text, LogMeterCustomer.NameLabel, customer.Name);
            WriteLine(text, LogMeterCustomer.AddressLabel, customer.Address);
            WriteLine(text, LogMeterCustomer.CityLabel, customer.City);
            WriteLine(text, LogMeterCustomer.StateLabel, customer.State);
            WriteLine(text, LogMeterCustomer.PostalCodeLabel, customer.PostalCode);
            WriteLine(text, LogMeterCustomer.PhoneNumberLabel, customer.PhoneNumber);
            WriteLine(text, LogMeterCustomer.NoteLabel, customer.Note);
        }

        void WriteMeter(System.Text.StringBuilder text, LogMeterMeter meter) {
            WriteLine(text, LogMeterMeter.CodeLabel, meter.Code);
            WriteLine(text, LogMeterMeter.MakeLabel, meter.Make);
            WriteLine(text, LogMeterMeter.ModelLabel, meter.Model);
            WriteLine(text, LogMeterMeter.SizeLabel, meter.Size);
            WriteLine(text, LogMeterMeter.UnitLabel, meter.Unit);
            WriteLine(text, LogMeterMeter.NutationLabel, meter.Nutation);
            WriteLine(text, LogMeterMeter.LedLabel, meter.Led);
            WriteLine(text, LogMeterMeter.StorageIntervalLabel, meter.StorageInterval);
            WriteLine(text, LogMeterMeter.NumberOfIntervalsLabel, meter.NumberOfIntervals);
            WriteLine(text, LogMeterMeter.TotalTimeLabel, meter.TotalTime);
            WriteLine(text, LogMeterMeter.TotalPulsesLabel, meter.TotalPulses);
            WriteLine(text, LogMeterMeter.BeginReadingLabel, meter.BeginReading);
            WriteLine(text, LogMeterMeter.EndReadingLabel, meter.EndReading);
            WriteLine(text, LogMeterMeter.RegisterVolumeLabel, meter.RegisterVolume);
            WriteLine(text, LogMeterMeter.MeterMasterVolumeLabel, meter.MeterMasterVolume);
            WriteLine(text, LogMeterMeter.ConversionFactorTypeLabel, meter.ConversionFactorType);
            WriteLine(text, LogMeterMeter.ConversionFactorLabel, meter.ConversionFactor);
            WriteLine(text, LogMeterMeter.DatabaseMultiplierLabel, meter.DatabaseMultiplier);
            WriteLine(text, LogMeterMeter.CombinedFileLabel, meter.CombinedFile);
            WriteLine(text, LogMeterMeter.DoublePulseLabel, meter.DoublePulse);
        }

        void WriteFlows(System.Text.StringBuilder text, Events events) {
            WriteLine(text);
            WriteFlowHeader(text);
            WriteFlowData(text, events);
        }

        void WriteFlowData(System.Text.StringBuilder text, Events events) {
            int i = 0;
            foreach (Event @event in events) {
                foreach (Flow flow in @event)
                    WriteFlow(text, flow, i);
                i++;
            }
        }

        void WriteFlowHeader(System.Text.StringBuilder text) {
            text.Append("% @FLOW");
            text.AppendLine();
        }

        void WriteFlow(System.Text.StringBuilder text, Flow flow, int id) {
            text.Append("% ");

            text.Append(id);
            text.Append(",");

            text.Append(WrapInQuotes(flow.StartTime.ToString(DateTimeFormat)));
            text.Append(",");

            text.Append(flow.Duration.TotalSeconds);
            text.Append(",");

            text.Append(flow.Rate);

            text.AppendLine();
        }

        string BuildFixtureClasses() {
            string result = null;

            bool first = true;
            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values) {
                if (!first) {
                    result += ",";
                } else
                    first = false;
                result += fixtureClass.Name;
            }

            return result;
        }
    }
}
