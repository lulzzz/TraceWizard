using System;
using System.Collections.Generic;
using System.Text;

namespace TraceWizard.Entities {

    public class FixtureProfiles : List<FixtureProfile> {
        public FixtureProfile this[string name] {
            get {
                FixtureProfile fixtureProfile = null;
                foreach (FixtureProfile fp in this)
                    if (fp.Name == name)
                        fixtureProfile = fp;
                return fixtureProfile;
            }
        }

        static double CalculateDifference(FixtureProfile fixtureProfile, FixtureProfile fixtureProfileEvent, Event @event) {
            double score = 0;

            if (fixtureProfile.MinVolume.HasValue && @event.Volume > fixtureProfile.MinVolume.Value)
                score++;

            if (fixtureProfile.MaxVolume.HasValue && @event.Volume < fixtureProfile.MaxVolume.Value)
                score++;

            if (fixtureProfile.MinPeak.HasValue && @event.Peak > fixtureProfile.MinPeak.Value)
                score++;

            if (fixtureProfile.MaxPeak.HasValue && @event.Peak < fixtureProfile.MaxPeak.Value)
                score++;

            if (fixtureProfile.MinMode.HasValue && @event.Mode > fixtureProfile.MinMode.Value)
                score++;

            if (fixtureProfile.MaxMode.HasValue && @event.Mode < fixtureProfile.MaxMode.Value)
                score++;

            if (fixtureProfile.MinDuration.HasValue && @event.Duration > fixtureProfile.MinDuration.Value)
                score++;

            if (fixtureProfile.MaxDuration.HasValue && @event.Duration < fixtureProfile.MaxDuration.Value)
                score++;

            return score;
        }
        
        static public FixtureProfile GetMostSimilar(List<FixtureProfile> fixtureProfiles, Event @event) {
            var fixtureProfileEvent = FixtureProfile.CreateFixtureProfile(@event);
            double scoreCurrent = 0;
            double scoreBest = 0;
            FixtureProfile fixtureProfileBest = null;
            foreach (FixtureProfile fixtureProfile in fixtureProfiles) {
                scoreCurrent = CalculateDifference(fixtureProfile, fixtureProfileEvent, @event);
                if (scoreCurrent >= scoreBest) {
                    scoreBest = scoreCurrent;
                    fixtureProfileBest = fixtureProfile;
                }
            }
            return fixtureProfileBest;
        }
       
        public FixtureProfiles Adopt(Analysis analysis) {
            FixtureProfiles fixtureProfilesAnalyst = analysis.FixtureProfiles;
            return Adopt(fixtureProfilesAnalyst, analysis.Events);
        }

        FixtureProfiles Adopt(FixtureProfiles fixtureProfilesAnalyst, Events events) {
            foreach (FixtureProfile fixtureProfileAnalyst in fixtureProfilesAnalyst) {
                FixtureProfile fixtureProfile = this[fixtureProfileAnalyst.Name];
                if (fixtureProfile != null && !fixtureProfile.MostlyNull()) {
                    fixtureProfile.Copy(fixtureProfile.Name, Adopt(fixtureProfileAnalyst, fixtureProfile, events));
                }
            }

            return this;
        }

        FixtureProfile Adopt(FixtureProfile fixtureProfileSource, FixtureProfile fixtureProfileTarget, Events events) {
            FixtureProfile fixtureProfileTest = fixtureProfileSource.CreateForAdoptedTest(fixtureProfileSource);
            foreach (Event @event in events) {
                if (fixtureProfileTest.IsAdopted(@event)) {
                    fixtureProfileTarget = fixtureProfileSource;
                    return fixtureProfileTarget;
                }
            }
            return fixtureProfileTarget;
        }
    }
    
    public class FixtureProfile {

        public static readonly string UnclassifiedName = "Other";

        double? ApplyMax(double? valFixtureProfile, double valEvent, double tolerance) {
            var valEventWithTolerance = valEvent * (1 + tolerance);
            if (!valFixtureProfile.HasValue)
                return valEventWithTolerance;
            else
                return Math.Max(valFixtureProfile.Value, valEventWithTolerance);
        }

        double? ApplyMin(double? valFixtureProfile, double valEvent, double tolerance) {
            var valEventWithTolerance = valEvent * (1 - tolerance);
            if (!valFixtureProfile.HasValue)
                return valEventWithTolerance;
            else
                return Math.Min(valFixtureProfile.Value, valEventWithTolerance);
        }

        TimeSpan? ApplyMax(TimeSpan? valFixtureProfile, TimeSpan valEvent, double tolerance) {
            var valEventWithTolerance = valEvent.Ticks * (1 + tolerance);
            if (!valFixtureProfile.HasValue)
                return RoundUp(new TimeSpan((long)valEventWithTolerance));
            else
                return RoundUp(new TimeSpan((long)Math.Max(valFixtureProfile.Value.Ticks, valEventWithTolerance)));
        }

        TimeSpan? ApplyMin(TimeSpan? valFixtureProfile, TimeSpan valEvent, double tolerance) {
            var valEventWithTolerance = valEvent.Ticks * (1 - tolerance);
            if (!valFixtureProfile.HasValue)
                return RoundDown(new TimeSpan((long)valEventWithTolerance));
            else
                return RoundDown(new TimeSpan((long)Math.Min(valFixtureProfile.Value.Ticks, valEventWithTolerance)));
        }

        public void Apply(Event @event) {
            var fixtureClass = @event.FixtureClass;
            if (fixtureClass == FixtureClasses.Leak) {
                MaxPeak = ApplyMax(MaxPeak, @event.Peak, 0.18);
            } else if (fixtureClass == FixtureClasses.Toilet) {
                MinVolume = ApplyMin(MinVolume, @event.Volume, 0.15);
                MaxVolume = ApplyMax(MaxVolume, @event.Volume, 0.15);
                MinPeak = ApplyMin(MinPeak, @event.Peak, 0.18);
                MaxPeak = ApplyMax(MaxPeak, @event.Peak, 0.18);
                MinDuration = ApplyMin(MinDuration, @event.Duration, 0.25);
                MaxDuration = ApplyMax(MaxDuration, @event.Duration, 0.25);
            } else if (fixtureClass == FixtureClasses.Shower) {
                MinVolume = ApplyMin(MaxVolume, @event.Volume, 0.15);
                MaxVolume = ApplyMax(MaxVolume, @event.Volume, 0.15);
                MaxPeak = ApplyMax(MaxPeak, @event.Peak, 0.18);
                MinMode = ApplyMin(MaxMode, @event.Mode, 0.18);
                MaxMode = ApplyMax(MaxMode, @event.Mode, 0.18);
                MinDuration = ApplyMin(MaxDuration, @event.Duration, 0.25);
                MaxDuration = ApplyMax(MaxDuration, @event.Duration, 0.25);
            } else if (fixtureClass == FixtureClasses.Faucet) {
                MaxVolume = ApplyMax(MaxVolume, @event.Volume, 0.15);
                MaxPeak = ApplyMax(MaxPeak, @event.Peak, 0.18);
            } else if (fixtureClass == FixtureClasses.Bathtub) {
                MinVolume = ApplyMin(MinVolume, @event.Volume, 0.15);
                MaxVolume = ApplyMax(MaxVolume, @event.Volume, 0.15);
                MinPeak = ApplyMin(MinPeak, @event.Peak, 0.18);
                MaxPeak = ApplyMax(MaxPeak, @event.Peak, 0.18);
            } else if (fixtureClass == FixtureClasses.Irrigation) {
                MaxVolume = ApplyMax(MaxVolume, @event.Volume, 0.15);
            } else {
                MinVolume = ApplyMin(MinVolume, @event.Volume, 0.15);
                MaxVolume = ApplyMax(MaxVolume, @event.Volume, 0.15);
                MinPeak = ApplyMin(MinPeak, @event.Peak, 0.18);
                MaxPeak = ApplyMax(MaxPeak, @event.Peak, 0.18);
                MinMode = ApplyMin(MaxMode, @event.Mode, 0.18);
                MaxMode = ApplyMax(MaxMode, @event.Mode, 0.18);
                MinDuration = ApplyMin(MaxDuration, @event.Duration, 0.25);
                MaxDuration = ApplyMax(MaxDuration, @event.Duration, 0.25);
            }
        }

        static public FixtureProfile FilterFixtureProfile(FixtureProfile fixtureProfile) {
            if (fixtureProfile.FixtureClass == FixtureClasses.Leak) {
                fixtureProfile.MinVolume = null;
                fixtureProfile.MaxVolume = null;
                fixtureProfile.MinPeak = null;
                fixtureProfile.MinMode = null;
                fixtureProfile.MaxMode = null;
                fixtureProfile.MinDuration = null;
                fixtureProfile.MaxDuration = null;
            } else if (fixtureProfile.FixtureClass == FixtureClasses.Toilet) {
                fixtureProfile.MinMode = null;
                fixtureProfile.MaxMode = null;
            } else if (fixtureProfile.FixtureClass == FixtureClasses.Shower) {
                fixtureProfile.MinPeak = null;
            } else if (fixtureProfile.FixtureClass == FixtureClasses.Faucet) {
                fixtureProfile.MinVolume = null;
                fixtureProfile.MinPeak = null;
                fixtureProfile.MinMode = null;
                fixtureProfile.MaxMode = null;
                fixtureProfile.MinDuration = null;
                fixtureProfile.MaxDuration = null;
            } else if (fixtureProfile.FixtureClass == FixtureClasses.Bathtub) {
                fixtureProfile.MinMode = null;
                fixtureProfile.MaxMode = null;
                fixtureProfile.MinDuration = null;
                fixtureProfile.MaxDuration = null;
            } else if (fixtureProfile.FixtureClass == FixtureClasses.Irrigation) {
                fixtureProfile.MinVolume = null;
                fixtureProfile.MinPeak = null;
                fixtureProfile.MaxPeak = null;
                fixtureProfile.MinMode = null;
                fixtureProfile.MaxMode = null;
                fixtureProfile.MinDuration = null;
                fixtureProfile.MaxDuration = null;
            }
            return fixtureProfile;
        }

        static TimeSpan RoundDown(TimeSpan timeSpan) {
            return timeSpan.Add(new TimeSpan(0, 0, 0, 0, 1000 - timeSpan.Milliseconds));
        }

        static TimeSpan RoundUp(TimeSpan timeSpan) {
            return timeSpan.Add(new TimeSpan(0, 0, 0, 1, 1000 - timeSpan.Milliseconds));
        }

        static public FixtureProfile CreateFixtureProfile(Event @event) {
            var fixtureProfile = new FixtureProfile(@event.FixtureClass.FriendlyName, @event.FixtureClass,
                @event.Volume * (1 - 0.15),
                @event.Volume * (1 + 0.15),
                @event.Peak * (1 - 0.18),
                @event.Peak * (1 + 0.18),
                RoundDown(new TimeSpan((long)(@event.Duration.Ticks * (1 - 0.25)))),
                RoundUp(new TimeSpan((long)(@event.Duration.Ticks * (1 + 0.25)))),
                @event.Mode * (1 - 0.18),
                @event.Mode * (1 + 0.18)
                );
            return fixtureProfile;
        }

        public void Copy(string name, FixtureProfile fixtureProfile) {
            Name = name;
            FixtureClass = fixtureProfile.FixtureClass;
            MinVolume = fixtureProfile.MinVolume;
            MaxVolume = fixtureProfile.MaxVolume;
            MinPeak = fixtureProfile.MinPeak;
            MaxPeak = fixtureProfile.MaxPeak;
            MinDuration = fixtureProfile.MinDuration;
            MaxDuration = fixtureProfile.MaxDuration;
            MinMode = fixtureProfile.MinMode;
            MaxMode = fixtureProfile.MaxMode;
        }


        public FixtureProfile Normalize() {
            if (Name.ToLower().Contains("other"))
                FixtureClass = FixtureClasses.Unclassified;
            else if (Name.ToLower().Contains("bad") || Name.ToLower().Contains("delete"))
                FixtureClass = FixtureClasses.Noise;
            else if (Name.ToLower().Contains("duplicate"))
                FixtureClass = FixtureClasses.Duplicate;
            else if (Name.ToLower().Contains("treatment"))
                FixtureClass = FixtureClasses.Treatment;

            return this;
        }

        public FixtureProfile(string data, char token) : this(ToStringArray(data,token)) {
        }

        static string[] ToStringArray(string data, char token) {
            return data.Split(token);
        }
        
        public FixtureProfile(string[] data) : this(null, null, null, null, null, null, null, null, null, null) {
            FixtureClass = FixtureClasses.Items[data[0].Trim('"')];

            Name = FixtureClass.FriendlyName;

            MinVolume = NormalizeDouble(data[1].Trim('"'));
            MaxVolume = NormalizeDouble(data[2].Trim('"'));
            MinPeak = NormalizeDouble(data[3].Trim('"'));
            MaxPeak = NormalizeDouble(data[4].Trim('"'));
            MinDuration = NormalizeTimeSpan(data[5].Trim('"'));
            MaxDuration = NormalizeTimeSpan(data[6].Trim('"'));
            MinMode = NormalizeDouble(data[7].Trim('"'));
            MaxMode = NormalizeDouble(data[8].Trim('"'));
        }

        double? NormalizeDouble(string s) {
            if (string.IsNullOrEmpty(s))
                return null;
            else
                return Convert.ToDouble(s);
        }

        TimeSpan? NormalizeTimeSpan(string s) {
            if (string.IsNullOrEmpty(s))
                return null;
            else {

                return new TimeSpan(0,0,Convert.ToInt32(s));
            }
        }

        public FixtureProfile() : this(null, null, null, null, null, null, null, null, null, null) { }

        public FixtureProfile(
            string name,
            FixtureClass fixtureClass,
            double? minVolume, double? maxVolume,
            double? minPeak, double? maxPeak,
            TimeSpan? minDuration, TimeSpan? maxDuration,
            double? minMode, double? maxMode
            ) {
            Name = name;
            FixtureClass = fixtureClass;
            MinVolume = minVolume; MaxVolume = maxVolume;
            MinPeak = minPeak; MaxPeak = maxPeak;
            MinDuration = minDuration; MaxDuration = maxDuration;
            MinMode = minMode; MaxMode = maxMode;
        }

        public string Name { get; set; }
        public FixtureClass FixtureClass { get; set; }

        double? minVolume = null;
        public double? MinVolume { get { return minVolume; } set { minVolume = Round(value); } }

        double? maxVolume = null;
        public double? MaxVolume { get { return maxVolume; } set { maxVolume = Round(value); } }

        double? minPeak = null;
        public double? MinPeak { get { return minPeak; } set { minPeak = Round(value); } }

        double? maxPeak = null;
        public double? MaxPeak { get { return maxPeak; } set { maxPeak = Round(value); } }

        TimeSpan? minDuration = null;
        public TimeSpan? MinDuration { get { return minDuration; } set { minDuration = Round(value); } }

        TimeSpan? maxDuration = null;
        public TimeSpan? MaxDuration { get { return maxDuration; } set { maxDuration = Round(value); } }

        double? minMode = null;
        public double? MinMode { get { return minMode; } set { minMode = Round(value); } }

        double? maxMode = null;
        public double? MaxMode { get { return maxMode; } set { maxMode = Round(value); } }

        double? Round(double? d) {
            if (d.HasValue && !double.IsNaN(d.Value))
                return Math.Round(d.Value, 2);
            else
                return null;
        }

        TimeSpan? Round(TimeSpan? value) { return value; }
        int? Round(int? value) { return value; }

        public FixtureProfile CreateForAdoptedTest(FixtureProfile fixtureProfile) {
            double? minVolume;
            if (MinVolume.HasValue) minVolume = MinVolume.Value / (1 - 0.15);
            else minVolume = null;

            double? maxVolume;
            if (MaxVolume.HasValue) maxVolume = MaxVolume.Value / (1 + 0.15);
            else maxVolume = null;

            double? minPeak;
            if (MinPeak.HasValue) minPeak = MinPeak.Value / (1 - 0.18);
            else minPeak = null;

            double? maxPeak;
            if (MaxPeak.HasValue) maxPeak = MaxPeak.Value / (1 + 0.18);
            else maxPeak = null;

            TimeSpan? minDuration;
            if (MinDuration.HasValue) minDuration = new TimeSpan((long)(MinDuration.Value.Ticks / (1 - 0.25)));
            else minDuration = null;

            TimeSpan? maxDuration;
            if (MaxDuration.HasValue) maxDuration = new TimeSpan((long)(MaxDuration.Value.Ticks / (1 + 0.25)));
            else maxDuration = null;

            double? minMode;
            if (MinMode.HasValue) minMode = MinMode.Value / (1 - 0.18);
            else minMode = null;

            double? maxMode;
            if (MaxMode.HasValue) maxMode = MaxMode.Value / (1 + 0.18);
            else maxMode = null;

            return new FixtureProfile(
                fixtureProfile.Name, fixtureProfile.FixtureClass,
                minVolume, maxVolume,
                minPeak, maxPeak,
                minDuration, maxDuration,
                minMode, maxMode
                );
        }

        public bool IsAdopted(Event @event) {
            if (!AreRoughlyEqual(@event.Volume, MinVolume)) return false;
            if (!AreRoughlyEqual(@event.Volume, MaxVolume)) return false;
            if (!AreRoughlyEqual(@event.Peak, MinPeak)) return false;
            if (!AreRoughlyEqual(@event.Peak, MaxPeak)) return false;
            if (!AreRoughlyEqual(@event.Duration, MinDuration)) return false;
            if (!AreRoughlyEqual(@event.Duration, MaxDuration)) return false;
            if (!AreRoughlyEqual(@event.Mode, MinMode)) return false;
            if (!AreRoughlyEqual(@event.Mode, MaxMode)) return false;
            return true;
        }

        private bool AreRoughlyEqual(double a, double? b) {
            if (!b.HasValue) return true;
            if ((b - 0.01 <= a) && (a <= b + 0.01))
                return true;
            else
                return false;
        }

        private bool AreRoughlyEqual(TimeSpan a, TimeSpan? b) {
            if (!b.HasValue) return true;
            if ((b.Value.Seconds - 10 <= a.Seconds) && (a.Seconds <= b.Value.Seconds + 10))
                return true;
            else
                return false;
        }

        private bool AreRoughlyEqual(int a, int? b) {
            if (!b.HasValue) return true;
            if ((b - 1 <= a) && (a <= b + 1))
                return true;
            else
                return false;
        }

        public bool MostlyNull() {
            int count = 0;
            count += MinVolume.HasValue ? 1 : 0;
            count += MaxVolume.HasValue ? 1 : 0;
            count += MinPeak.HasValue ? 1 : 0;
            count += MaxPeak.HasValue ? 1 : 0;
            count += MinDuration.HasValue ? 1 : 0;
            count += MaxDuration.HasValue ? 1 : 0;
            count += MinMode.HasValue ? 1 : 0;
            count += MaxMode.HasValue ? 1 : 0;

            return count < 2 ? true : false;
        }
    }
}
