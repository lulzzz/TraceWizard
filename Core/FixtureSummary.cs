using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

using System.ComponentModel;

using TraceWizard.Notification;

namespace TraceWizard.Entities {

    public struct DateTimeVolume {
        public DateTime DateTime;
        public double Volume;
    }
    
    public class FixtureSummaries : Dictionary<FixtureClass, FixtureSummary> {

        public double SelectedVolume;
        public double[] HourlyVolume;
        public Dictionary<DateTime,double> DailyVolume;
        public Events Events { get; set; }

        public FixtureSummaries(Events events) : this() { Events = events; }
        private FixtureSummaries() {
            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values)
                Add(fixtureClass, new FixtureSummary(fixtureClass));
            ClearHourlyTotals();
            ClearDailyTotals();
        }

        public void ClearDailyTotals() {
            DailyVolume = Analysis.GetFullDays(Events);
            foreach (var fixtureClass in FixtureClasses.Items.Values) {
                this[fixtureClass].DailyVolume = Analysis.GetFullDays(Events);
                this[fixtureClass].DailyVolumeTotal = 0.0;
            }
        }

        public void ClearHourlyTotals() {
            HourlyVolume = new double[24];
            foreach (var fixtureClass in FixtureClasses.Items.Values) {
                this[fixtureClass].HourlyVolume = new double[24];
            }
        }

        public FixtureClass MaximumFixtureClass() {
            FixtureSummary fixtureSummaryMaximum = this[FixtureClasses.Unclassified];
            foreach (FixtureSummary fixtureSummary in this.Values) {
                if (fixtureSummary.Count > fixtureSummaryMaximum.Count)
                    fixtureSummaryMaximum = fixtureSummary;
            }
            return fixtureSummaryMaximum.FixtureClass;
        }
     
        public void Update() {
            Update(Events);
        }

        public void Update(Events events) {
            foreach (FixtureSummary fixtureSummary in this.Values) {
                fixtureSummary.Update(events);
            }
        }

        public void UpdateSelectedCounts(Events events) {

            foreach (FixtureSummary fixtureSummary in this.Values)
                fixtureSummary.SelectedCount = 0;
            SelectedVolume = 0;

            foreach (Event @event in events) {
                if (@event.Selected) {
                    this[@event.FixtureClass].SelectedCount++;
                    SelectedVolume += @event.Volume;
                }
            }
        }

        public void UpdateMedians() {
            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values)
                this[fixtureClass].Events.UpdateMedians();
        }

        public int CountEvents() {
            int count = 0;
            foreach (FixtureSummary fixtureSummary in this.Values) {
                count += fixtureSummary.Count;
            }
            return count;
        }
    }

    public class FixtureSummary {
        public FixtureClass FixtureClass { get; set; }

        public FixtureSummary() { }
        public FixtureSummary(FixtureClass fixtureClass) {
            FixtureClass = fixtureClass; 
        }

        public int Count { get; set; }
        public int SelectedCount { get; set; }
        public int FirstCycles { get; set; }
        public int TraceCount { get; set; }
        public double PercentCount { get; set; }
        public double Volume { get; set; }
        public double PercentVolume { get; set; }
        public int ManuallyClassified { get; set; }
        public double PercentManuallyClassified { get; set; }

        public double[] HourlyVolume = new double[24];
        public Dictionary<DateTime, double> DailyVolume;
        public double DailyVolumeTotal;

        public Events Events = new Events();

        public void UpdateSelectedCount(Events events) {
            SelectedCount = 0;
            foreach (Event @event in events) {
                if (@event.FixtureClass == FixtureClass) {
                    if (@event.Selected)
                        SelectedCount++;
                }
            }
        }
        
        public void Update(Events events) {
            bool found = false;
            TraceCount = 0;
            Count = 0;
            Volume = 0;
            ManuallyClassified = 0;
            FirstCycles = 0;
            SelectedCount = 0;

            Events.Clear();

            foreach (Event @event in events) {
                if (@event.FixtureClass == FixtureClass) {
                    if (!found) {
                        found = true;
                        TraceCount++;
                    }

                    Events.Add(@event);

                    Count++;
                    Volume += @event.Volume;
                    if (@event.ManuallyClassified)
                        ManuallyClassified++;
                    if (@event.FirstCycle)
                        FirstCycles++;
                    if (@event.Selected)
                        SelectedCount++;
                }
            }

            PercentCount = (double)Count / (double)events.Count;
            PercentVolume = Volume / events.Volume;
            PercentManuallyClassified = Count == 0 ? 0 : (double)ManuallyClassified / (double)Count;
        }
    }
}