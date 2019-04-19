using System;
using System.Collections.Generic;
using System.ComponentModel;

using TraceWizard.Adoption;

using TraceWizard.Notification;

namespace TraceWizard.Entities {

    public class EventComparer : IComparer<Event>{
        public int Compare(Event event1, Event event2) {
            if (event1 == event2)
                return 0;
            if (event1.TimeFrame.StartTime == event2.TimeFrame.StartTime) {
                if (event1.Channel == Channel.Base)
                    return -1;
                else
                    return 1;
            }
            return event1.TimeFrame.StartTime.CompareTo(event2.TimeFrame.StartTime);
        }
    }
    
    public enum Channel {
        None = 0,
        Runt = 1,
        Trickle = 2,
        Base = 3,
        Super = 4
    };

    public class Event : List<Flow>, INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) 
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Tag { get; set; }

        string userNotes;
        public string UserNotes {
            get { return userNotes; }
            set {
                if (!string.IsNullOrEmpty(value) && !value.Equals(UserNotes)) {
                    userNotes = value;
                    OnPropertyChanged(TwNotificationProperty.OnUserNotesChanged);
                }
            }
        }
        public bool HasNotes { get { return !String.IsNullOrEmpty(userNotes); } }

        public Event Previous { get; set; }
        public Event Next { get; set; }

        FixtureClass fixtureClass;
        public FixtureClass FixtureClass { get {return fixtureClass;}
            set {
                fixtureClass = value;
                OnPropertyChanged(TwNotificationProperty.OnEndClassify);
            }
        }

        bool selected;
        public bool Selected {
            get { return selected; }
            set {
                selected = value;
                OnPropertyChanged(TwNotificationProperty.OnEndSelect);
            }
        }

        bool firstCycle;
        public bool FirstCycle {
            get { return firstCycle; }
            set {
                firstCycle = value;
                OnPropertyChanged(TwNotificationProperty.OnEndClassify);
            }
        }

        bool classifiedUsingFixtureList;
        public bool ClassifiedUsingFixtureList {
            get { return classifiedUsingFixtureList; }
            set {
                classifiedUsingFixtureList = value;
                OnPropertyChanged(TwNotificationProperty.OnEndClassify);
            }
        }

        bool manuallyClassified;
        public bool ManuallyClassified {
            get { return manuallyClassified; }
            set {
                manuallyClassified = value;
                if (manuallyClassified == true && manuallyApproved == false)
                    manuallyApproved = true;
                OnPropertyChanged(TwNotificationProperty.OnEndClassify);
            }
        }

        bool manuallyApproved;
        public bool ManuallyApproved {
            get { return manuallyApproved; }
            set {
                manuallyApproved = value;
                OnPropertyChanged(TwNotificationProperty.OnEndClassify);
            }
        }

        bool manuallyClassifiedFirstCycle;
        public bool ManuallyClassifiedFirstCycle {
            get { return manuallyClassifiedFirstCycle; }
            set {
                manuallyClassifiedFirstCycle = value;
                OnPropertyChanged(TwNotificationProperty.OnEndClassify);
            }
        }

        public TimeFrame TimeFrame { get; set; }
        public double Volume { get; set; }
        public double Peak { get; set; }
        public double Mode { get; set; }

        public int ModeFrequency { get; set; }

        public double SuperPeak { get; set; }

        public int SimilarForwardCount { get; set; }
        public int SimilarBackwardCount { get; set; }
        public int SimilarCount { get { return SimilarForwardCount + SimilarBackwardCount; } }

        public TimeSpan Duration { get { return TimeFrame.Duration; } }
        public DateTime StartTime { get { return TimeFrame.StartTime; } }
        public DateTime EndTime { get { return TimeFrame.EndTime; } }

        public TimeSpan Interval { get; set; }

        public Channel Channel { get; set; }
        public double Baseline { get; set; }
        public double BaselineWithTolerance {get;set;}
        public Event BaseEvent { get; set; }
        public Events SuperEvents { get; set; }

        public bool IsSimilarDefault(Event eventSource) {
            Adopter adopter = TwAdopters.Instance.GetDefaultAdopter();
            return IsSimilar(eventSource,
                adopter.VolumePercent,
                adopter.PeakPercent,
                adopter.DurationPercent,
                adopter.ModePercent);
        }
    
        public bool IsSimilar(FixtureClass fixtureClass,
            double? minVolume, double? maxVolume,
            double? minPeak, double? maxPeak,
            TimeSpan? minDuration, TimeSpan? maxDuration,
            double? minMode, double? maxMode
            ) {
            var fixtureProfile = new FixtureProfile(fixtureClass.Name,
                fixtureClass,
                minVolume,
                maxVolume,
                minPeak,
                maxPeak,
                minDuration,
                maxDuration,
                minMode,
                maxMode
                );

            return Event.MatchesFixture(this, fixtureProfile);
        }

        public bool IsSimilar(Event @event,
            double? minVolumePercent, double? maxVolumePercent,
            double? minPeakPercent, double? maxPeakPercent,
            double? minDurationPercent, double? maxDurationPercent,
            double? minModePercent, double? maxModePercent
            ) {
            var fixtureProfile = new FixtureProfile(@event.FixtureClass.Name,
                @event.FixtureClass,
                Decrease(@event.Volume, minVolumePercent),
                Increase(@event.Volume, maxVolumePercent),
                Decrease(@event.Peak, minPeakPercent),
                Increase(@event.Peak, maxPeakPercent),
                DecreaseDuration(@event.Duration, minDurationPercent),
                IncreaseDuration(@event.Duration, maxDurationPercent),
                Decrease(@event.Mode, minModePercent),
                Increase(@event.Mode, maxModePercent)
                );

            return Event.MatchesFixture(this, fixtureProfile);
        }

        public bool IsSimilar(Event @event, double? volumePercent, double? peakPercent, double? durationPercent, double? modePercent) {
            var fixtureProfile = new FixtureProfile(@event.FixtureClass.Name,
                @event.FixtureClass,
                Decrease(@event.Volume, volumePercent),
                Increase(@event.Volume, volumePercent),
                Decrease(@event.Peak, peakPercent),
                Increase(@event.Peak, peakPercent),
                DecreaseDuration(@event.Duration, durationPercent),
                IncreaseDuration(@event.Duration, durationPercent),
                Decrease(@event.Mode, modePercent),
                Increase(@event.Mode, modePercent)
                );

            return Event.MatchesFixture(this, fixtureProfile);
        }

        public bool IsThreeOutOfFourSimilar(Event @event, double? volumePercent, double? peakPercent, double? durationPercent, double? modePercent) {
            var fixtureProfile = new FixtureProfile(@event.FixtureClass.Name,
                @event.FixtureClass,
                Decrease(@event.Volume, volumePercent),
                Increase(@event.Volume, volumePercent),
                Decrease(@event.Peak, peakPercent),
                Increase(@event.Peak, peakPercent),
                DecreaseDuration(@event.Duration, durationPercent),
                IncreaseDuration(@event.Duration, durationPercent),
                Decrease(@event.Mode, modePercent),
                Increase(@event.Mode, modePercent)
                );

            return Event.MatchesFixtureThreeOutOfFour(this, fixtureProfile);
        }

        public double? Increase(double value, double? factor) {
            if (!factor.HasValue) return null;
            return value * (1+factor);
        }

        public double? Decrease(double value, double? factor) {
            if (!factor.HasValue) return null;
            return value * (1 - factor);
        }

        public TimeSpan? IncreaseDuration(TimeSpan value, double? factor) {
            if (!factor.HasValue) return null;
            return new TimeSpan(0, 0, (int)(value.TotalSeconds * (1 + factor)));
        }

        public TimeSpan? DecreaseDuration(TimeSpan value, double? factor) {
            if (!factor.HasValue) return null;
            return new TimeSpan(0, 0, (int)(value.TotalSeconds * (1 - factor)));
        }

        public Event() { }

        public Event(FixtureClass fixtureClass) { FixtureClass = fixtureClass; }

        public Event(Channel channel, TimeFrame timeFrame, params double[] rate) : this(timeFrame, rate){
            Channel = channel;
            FixtureClass = FixtureClasses.Unclassified;
        }

        public Event(TimeFrame timeFrame, params double[] rate) {
            TimeSpan offset = TimeSpan.Zero;
            for (int index = 0; index < rate.Length; index++) {
                this.Add(new Flow(new TimeFrame(timeFrame.StartTime.Add(offset), timeFrame.Duration), rate[index]));
                offset += timeFrame.Duration;
            }
        }

        public Event(int startYear, int startMonth, int startDay, int startHour, int startMinute, int startSecond,
            int durationDays, int durationHours, int durationMinutes, int durationSeconds,
            params double[] rate) : 
            this(
                new TimeFrame(new DateTime(startYear, startMonth, startDay, startHour, startMinute, startSecond),
                new TimeSpan(durationDays, durationHours, durationMinutes, durationSeconds)),
                rate)
            { }

        public Event(FixtureClass fixtureClass, double volume, double peak, TimeFrame timeFrame, double mode, int modeFrequency) :
            this(volume,peak,timeFrame,mode,modeFrequency)
        {
            FixtureClass = fixtureClass;
        }

        public Event(double volume, double peak, TimeFrame timeFrame, double mode, int modeFrequency) {
            Volume = volume; Peak = peak; TimeFrame = timeFrame; Mode = mode; ModeFrequency = modeFrequency;
        }

        public void UpdateMode() {
            int modeFrequency;
            Mode = CalcMode(out modeFrequency);
            ModeFrequency = modeFrequency;
        }

        public TimeFrame CalcTimeFrame() {
            if (Count == 0) return new TimeFrame();

            DateTime startTime = DateTime.MaxValue;
            DateTime endTime = DateTime.MinValue;
            foreach (Flow flow in this) {
                if (flow.StartTime < startTime) startTime = flow.StartTime;
                if (flow.EndTime > endTime) endTime = flow.EndTime;
            }
            return new TimeFrame(startTime, endTime);
        }

        public double CalcVolume() {
            double volume = 0.0D;
            foreach (Flow flow in this)
                volume += flow.Volume;
            return volume;
        }

        public double CalcPeak() {
            double peak = 0.0D;
            foreach (Flow flow in this)
                if (flow.Peak > peak) peak = flow.Peak;
            return peak;
        }

        public double CalcSuperPeak() {
            double basePeak = 0.0D;

            if (Channel == Channel.Super)
                return 0.0;

            foreach (Flow flow in this) {
                if (flow.Peak > basePeak)
                    basePeak = flow.Peak;
            }

            if (SuperEvents == null || SuperEvents.Count == 0)
                return basePeak;

            double supersPeak = 0.0D;
            double superPeak = 0.0D;
            foreach (Event super in SuperEvents) {
                foreach(Flow flow in super) {
                    if (flow.Peak > superPeak)
                        superPeak = flow.Peak;
                }
                if (superPeak > supersPeak)
                    supersPeak = superPeak;
            }

            return basePeak + supersPeak;
        }

        public void UpdateSimilarCounts() {
            SimilarForwardCount = CalcSimilarForwardCount(this);
            SimilarBackwardCount = CalcSimilarBackwardCount(this);
        }
        
        int CalcSimilarForwardCount(Event @eventStart) {
            int similarCount = 0;
            if (eventStart != null)
                eventStart = eventStart.Next;
            while (@eventStart != null) {
                if (@eventStart.IsSimilarDefault(this))
                    similarCount++;
                @eventStart = @eventStart.Next;
            }
            return similarCount;
        }

        int CalcSimilarBackwardCount(Event @eventStart) {
            int similarCount = 0;
                if (eventStart != null)
                    eventStart = eventStart.Previous;
                while (@eventStart != null) {
                    if (@eventStart.IsSimilarDefault(this))
                        similarCount++;
                    @eventStart = @eventStart.Previous;
            }
            return similarCount;
        }

        public double CalcMode(out int modeFrequency) {
            return CalcMode(ToPeakArray(), out modeFrequency);
        }

        public double[] ToPeakArray() {
            double[] array = new double[this.Count];
            int index = 0;
            foreach (Flow flow in this)
                array[index++] = flow.Peak;
            return array;
        }

        static double[] ToSort(double[] source) {
            double[] dest = new double[source.Length];
            source.CopyTo(dest, 0);
            Array.Sort(dest);
            return dest;
        }

        public static double CalcMode(double[] items, out int modeFrequency)
        {
            modeFrequency = 0;
            double valMode = 0.0;

            if (items.Length == 0) return valMode;
            
            double[] array = ToSort(items);
            double helpValMode = (valMode = array[0]);
            int oldCounter = 0, newCounter = 0, index = 0;

            for (; index <= array.Length - 1; index++)
                if (array[index] == helpValMode) newCounter++;
                else if (newCounter >= oldCounter) {
                    oldCounter = newCounter;
                    newCounter = 1;
                    helpValMode = array[index];
                    valMode = array[index - 1];
                } else {
                    helpValMode = array[index];
                    newCounter = 1;
                }

            if (newCounter >= oldCounter) {
                valMode = array[index - 1];
                modeFrequency = newCounter;
            } else
                modeFrequency = oldCounter;

            return valMode;
        }

        public void UpdateWithoutVolume(Flow flow) {
            if (flow.Peak > Peak) 
                Peak = flow.Peak;
            UpdateTimeFrame(flow);
        }

        public void Update(Flow flow) {
            if (flow.Peak > Peak) 
                Peak = flow.Peak;
            Volume += flow.Volume;
            Volume = Tw4CompatibilityRoundVolume3(Volume);
            UpdateTimeFrame(flow);
        }

        public void Update() {
            Peak = CalcPeak();
            Volume = CalcVolume();
            int modeFrequency = 0;
            Mode = CalcMode(out modeFrequency);
            TimeFrame = CalcTimeFrame();
            SuperPeak = CalcSuperPeak();
        }

        double Tw4CompatibilityRoundVolume3(double volume) {
            return Math.Round(volume, 3);
        }

        public void ApplyConversionFactor(double oldConversionFactor, double newConversionFactor) {
            foreach (Flow flow in this) {
                flow.ApplyConversionFactor(oldConversionFactor, newConversionFactor);
            }
            Volume *= newConversionFactor / oldConversionFactor;
            Peak *= newConversionFactor / oldConversionFactor;
            Mode *= newConversionFactor / oldConversionFactor;
            SuperPeak *= newConversionFactor / oldConversionFactor;
            Baseline *= newConversionFactor / oldConversionFactor;
            BaselineWithTolerance *= newConversionFactor / oldConversionFactor;
        }

        public void ClearTimeFrame() {
            TimeFrame = new TimeFrame();
        }

        public void UpdateTimeFrame(Flow flow) {
            if ((StartTime == default(DateTime)) || (EndTime == default(DateTime)))
                TimeFrame = new TimeFrame(flow.StartTime, flow.EndTime);
            else {
                if (flow.StartTime < StartTime) TimeFrame = new TimeFrame(flow.StartTime, EndTime);
                if (flow.EndTime > EndTime) TimeFrame = new TimeFrame(StartTime, flow.EndTime);
            }
        }

        public new void Add(Flow item) {
            if (BinarySearch(item) >= 0) return;
            base.Add(item);
            Update(item);
        }

        public void AddWithoutVolume(Flow item) {
            if (BinarySearch(item) >= 0) return;
            base.Add(item);
            UpdateWithoutVolume(item);
        }

        public new void Remove(Flow item) {
            base.Remove(item);
            UpdateRemove(item);
        }

        public new void Insert(int index, Flow item) {
            base.Insert(index,item);
            Update(item);
        }
    
        void UpdateRemove(Flow item) {
            Volume -= item.Volume;
        }
       
       public bool IsSubsetOf(Event other) {
            foreach (Flow flow in this)
                if (!other.Contains(flow)) 
                    return false;
            return true;
        }

       public Event CopyShallow() {
           return new Event(FixtureClass,Volume,Peak,TimeFrame,Mode,ModeFrequency);
       }
       
       public Event Copy() {
            Event @event = this.CopyShallow();
            foreach(Flow flow in this)
                @event.Add(flow);
            return @event;
       }

        public bool CanSplitHorizontally(DateTime dateTime) {
            return !IsASuperEventInProgressThatIsNotAlmostDone(dateTime);
        }

        public bool CanSplitHorizontally(double x, double y, double widthMultiplier, double heightMultiplier) {
            DateTime splitTime = GetSplitDate(x, widthMultiplier);
            Flow flow = GetFlow(splitTime);

            return flow != null && CanSplitHorizontally(splitTime);
        }

        public double GetSplitRate(double y, double heightMultiplier) {
            return -y / heightMultiplier;
        }

        public DateTime GetSplitDate(double x, double widthMultiplier) {
            //int secondsOffset = (int)(x / widthMultiplier);
            //return StartTime.Add(new TimeSpan(0, 0, secondsOffset));

            double secondsOffset = x / widthMultiplier;
            double remainder = secondsOffset - (int)Math.Floor(secondsOffset);

            DateTime dateTime = StartTime.Add(new TimeSpan(0, 0, 0, (int)Math.Floor(secondsOffset), (int)(remainder * 1000.0)));
            return dateTime;
        }

        public bool IsASuperEventInProgressThatIsNotAlmostDone(DateTime dateTime) {
            Event super = GetSuperEvent(dateTime);
            if (super == null)
                return false;
            if (dateTime > super[super.Count - 1].StartTime)
                return false;
            else
                return true;
        }

        public bool HasSuperEvents() {
            return Channel == Channel.Base && SuperEvents != null && SuperEvents.Count > 0;
        }
        
        public Event GetSuperEvent(DateTime dateTime) {
            if (!HasSuperEvents())
                return null;

            foreach (Event superEvent in SuperEvents) {
                if (superEvent.StartTime <= dateTime && dateTime <= superEvent.EndTime)
                    return superEvent;
            }
            return null;
        }

        public bool IsLegal(DateTime dateTime, double rate) {
            Flow flow = GetFlow(dateTime, rate);
            return (flow != null);
        }
        
        public Flow GetFlow(DateTime startTime, double rate) {
            int index = FindLastIndex(delegate(Flow f) {
                return f.StartTime <= startTime;
            });
            if (index < 0 || index >= Count)
                return null;
            if (this[index].Rate <= rate)
                return null;
            else
                return this[index];
        }

        public Flow GetFlow(DateTime startTime) {
            int index = FindLastIndex(delegate(Flow f) {
                return f.StartTime <= startTime;
            });
            if (index < 0 || index >= Count)
                return null;
            else
                return this[index];
        }

        public int GetFlowIndex(DateTime startTime) {
            return FindLastIndex(delegate(Flow f) {
                return f.StartTime <= startTime;
            });
        }

        public Flow GetPreviousFlowWithLowerRate(DateTime startTime, double rate) {
            int index = FindLastIndex(delegate(Flow f) {
                return f.Rate < rate && f.StartTime < startTime;
            });

            return index == -1 ? null : this[index];
        }

        public Flow GetNextFlowWithLowerRate(DateTime startTime, double rate) {
            int index = FindIndex(delegate(Flow f) {
                return f.Rate < rate && f.StartTime >= startTime;
            });
            return index == -1 ? null : this[index];
        }

        public override string ToString() {
            return string.Format("{0}, {1:0.000}, {2:0.000}", TimeFrame, Peak, Volume);
        }

        public double GetRate(DateTime startTime) {
            foreach (Flow flow in this)
                if (flow.StartTime == startTime)
                    return flow.Peak;
            return 0;
        }

        public static bool MatchesFixtureThreeOutOfFour(Event @event, FixtureProfile fixtureProfile) {
            bool similarVolume = WithinTolerance(@event.Volume, fixtureProfile.MinVolume, fixtureProfile.MaxVolume);
            bool similarPeak = WithinTolerance(@event.Peak, fixtureProfile.MinPeak, fixtureProfile.MaxPeak);
            bool similarDuration = WithinTolerance(@event.Duration, fixtureProfile.MinDuration, fixtureProfile.MaxDuration);
            bool similarMode = WithinTolerance(@event.Mode, fixtureProfile.MinMode, fixtureProfile.MaxMode);

            return (
                (similarVolume && similarPeak && similarDuration) ||
                (similarVolume && similarPeak && similarMode) ||
                (similarVolume && similarDuration && similarMode) ||
                (similarPeak && similarDuration && similarMode)
                );
        }

        public static bool MatchesFixture(Event @event, FixtureProfile fixtureProfile) {
            bool matchesVolume = WithinTolerance(@event.Volume, fixtureProfile.MinVolume, fixtureProfile.MaxVolume);
            bool matchesPeak = WithinTolerance(@event.Peak, fixtureProfile.MinPeak, fixtureProfile.MaxPeak);
            bool matchesDuration = WithinTolerance(@event.Duration, fixtureProfile.MinDuration, fixtureProfile.MaxDuration);
            bool matchesMode = WithinTolerance(@event.Mode, fixtureProfile.MinMode, fixtureProfile.MaxMode);

            return matchesVolume && matchesPeak && matchesDuration && matchesMode;
        }

        static bool WithinTolerance(double value, double? min, double? max) {
            return AboveMinTolerance(value, min) && BelowMaxTolerance(value, max);
        }

        static bool AboveMinTolerance(double value, double? min) {
            return !min.HasValue || (min.Value.CompareTo(value) <= 0);
        }

        static bool BelowMaxTolerance(double value, double? max) {
            return !max.HasValue || (value.CompareTo(max.Value) <= 0);
        }

        static bool WithinTolerance(TimeSpan value, TimeSpan? min, TimeSpan? max) {
            return AboveMinTolerance(value, min) && BelowMaxTolerance(value, max);
        }

        static bool AboveMinTolerance(TimeSpan value, TimeSpan? min) {
            return !min.HasValue || (min.Value.CompareTo(value) <= 0);
        }

        static bool BelowMaxTolerance(TimeSpan value, TimeSpan? max) {
            return !max.HasValue || (value.CompareTo(max.Value) <= 0);
        }

        static bool WithinTolerance(int value, int? min, int? max) {
            return AboveMinTolerance(value, min) && BelowMaxTolerance(value, max);
        }

        static bool AboveMinTolerance(int value, int? min) {
            return !min.HasValue || (min.Value.CompareTo(value) <= 0);
        }

        static bool BelowMaxTolerance(int value, int? max) {
            return !max.HasValue || (value.CompareTo(max.Value) <= 0);
        }

        static public Event DeepCopy(Event @event, TimeSpan offset) {
            var timeFrame = new TimeFrame(@event.StartTime.Add(offset), @event.Duration);
            return DeepCopy(@event, timeFrame);
        }

        static public Event DeepCopy(Event @event) {
            var timeFrame = new TimeFrame(@event.StartTime, @event.Duration);
            return DeepCopy(@event, timeFrame);
        }

        static Event DeepCopy(Event @event, TimeFrame timeFrame) {
            var eventNew = new Event();
            eventNew.TimeFrame = timeFrame;

            eventNew.FirstCycle = @event.FirstCycle;
            eventNew.ManuallyClassified = @event.ManuallyClassified;
            eventNew.Channel = @event.Channel;
            eventNew.Volume = @event.Volume;
            eventNew.Peak = @event.Peak;
            eventNew.Mode = @event.Mode;
            eventNew.ModeFrequency = @event.ModeFrequency;

            eventNew.UserNotes = @event.UserNotes;
            eventNew.FixtureClass = @event.FixtureClass;

            return eventNew;
        }
    }
}
