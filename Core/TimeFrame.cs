using System;

using TraceWizard.Entities;

namespace TraceWizard.Entities
{
    public struct TimeFrame
    {
        public TimeFrame(DateTime StartTime, DateTime EndTime) : this(StartTime, EndTime - StartTime) { }

        public TimeFrame(DateTime StartTime, TimeSpan Duration)
            : this() {
            this.StartTime = StartTime;
            this.Duration = Duration;
        }

        private DateTime startTime;
        public DateTime StartTime { 
            get {return startTime;}  
            private set { startTime = Round(value); } 
        }

        public DateTime EndTime { get; private set; }

        private TimeSpan duration;
        public TimeSpan Duration { 
            get {return duration; } 
            private set {
                duration = Round(value);
                if (duration < TimeSpan.Zero) {
                    throw new ArgumentException("StartTime > EndTime");
                }
                try {
                    EndTime = StartTime.Add(duration);
                } catch (ArgumentOutOfRangeException) {
                    throw new ArgumentException("overflow duration");
                }
            }
        }
        static private string DateStringFormat { get { return "yyyy\\/MM\\/dd HH\\:mm\\:ss"; } }

        public override string ToString() {
            return string.Format("{0} - {1}", this.StartTime.ToString(TimeFrame.DateStringFormat), this.EndTime.ToString(TimeFrame.DateStringFormat));
        }

        internal static DateTime Round(DateTime dateTime) {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
        }

        internal static TimeSpan Round(TimeSpan timeSpan) {
            return new TimeSpan(timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }
    }
}
