using System;
using TraceWizard.Data;

namespace TraceWizard.Entities
{
    public class Flow : IComparable<Flow> {
        public Flow() { }
        public Flow(TimeFrame timeFrame, double rate)
            : this() {
            TimeFrame = timeFrame;
            Rate = rate;
        }

        public Flow(double volume, TimeFrame timeFrame)
            : this() {
            TimeFrame = timeFrame;
            SetVolume(volume);
        }

        public Flow(DateTime startTime, TimeSpan duration, double rate)
            : this(new TimeFrame(startTime, duration), rate) { }

        public Flow(int startYear, int startMonth, int startDay, int startHour, int startMinute, int startSecond,
            int durationDays, int durationHours, int durationMinutes, int durationSeconds,
            double rate)
            : this(
                new TimeFrame(
                    new DateTime(startYear, startMonth, startDay, startHour, startMinute, startSecond),
                    new TimeSpan(durationDays, durationHours, durationMinutes, durationSeconds)),
                    rate) { }

        public Flow(int startYear, int startMonth, int startDay, int startHour, int startMinute, int startSecond,
            int endYear, int endMonth, int endDay, int endHour, int endMinute, int endSecond,
            double rate)
            : this(
                new TimeFrame(
                    new DateTime(startYear, startMonth, startDay, startHour, startMinute, startSecond),
                    new DateTime(endYear, endMonth, endDay, endHour, endMinute, endSecond)),
                    rate) { }

        public TimeFrame TimeFrame { get; set; }
        public double Peak { get { return Rate; } }
        protected double rate;
        public virtual double Rate {
            get { return rate; }
            set { SetRate(value); }
        }

        public void ApplyConversionFactor(double oldConversionFactor, double newConversionFactor) {
            SetRate(rate * newConversionFactor / oldConversionFactor);
        }
        
        public double Volume { get; protected set; }
        protected void SetRate(double value) {
            rate = RoundRate(value);
            if (rate < 0.0D)
                throw new ArgumentException("negative rate");
            Volume = RoundVolume(rate * ((double)(TimeFrame.Duration.Ticks / (double)TimeSpan.TicksPerMinute)));
        }

        protected void SetVolume(double volume) {
            Volume = RoundVolume(volume);
            if (volume < 0.0D)
                throw new ArgumentException("negative volume");
            rate = RoundRate(volume * ((double)(TimeSpan.TicksPerMinute / (double)TimeFrame.Duration.Ticks)));
        }

        public TimeSpan Duration { get { return TimeFrame.Duration; } }
        public DateTime StartTime { get { return TimeFrame.StartTime; } }
        public DateTime EndTime { get { return TimeFrame.EndTime; } }

        double RoundRate(double rate) { return Math.Round(rate, 2); }
        double RoundVolume(double volume) { return Math.Round(volume, 3); }

        public override string ToString() {
            return string.Format("{0}, {1:0.00###}, {2:0.00###}", TimeFrame, Rate, Volume);
        }

        public int CompareTo(Flow other) {
            return StartTime.CompareTo(other.StartTime);
        }

    }
}
