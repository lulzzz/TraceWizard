using System;
using System.Collections.Generic;

using TraceWizard.Entities;
using TraceWizard.Adoption;

namespace TraceWizard.Adoption.Adopters.Naive {

    public abstract class TightAdopter : v15p18d25m18NaiveAdopter {
        protected double? volumeTightPercent { get; set; }
        protected double? peakTightPercent { get; set; }
        protected double? durationTightPercent { get; set; }
        protected double? modeTightPercent { get; set; }

        public TightAdopter()
            : this(null) {
        }
        public TightAdopter(Events events)
            : base(events) {
        }
    }
    
    public class LeakFaucetTightAdopter : TightAdopter {
        public LeakFaucetTightAdopter()
            : this(null) {
        }
        public LeakFaucetTightAdopter(Events events)
            : base(events) {
            volumeTightPercent = 0.12;
            peakTightPercent = 0.15;
            durationTightPercent = 0.20;
            modeTightPercent = 0.15;
        }

        protected override bool IsSimilar(Event eventSource, Event eventTarget, double? volumePercent, double? peakPercent, double? durationPercent, double? modePercent) {
            if (eventSource.FixtureClass == FixtureClasses.Leak || eventSource.FixtureClass == FixtureClasses.Faucet)
                return eventTarget.IsSimilar(eventSource, volumeTightPercent, peakTightPercent, durationTightPercent, modeTightPercent);
            else            
                return eventTarget.IsSimilar(eventSource, volumePercent, peakPercent, durationPercent, modePercent);
        }

        public override string Name { get { return "Leak Faucet Tight Adopter"; } }
        public override string Description { get { return "Adopts if volume within 15%, peak within 18%, duration within 25%, and mode within 18%; However, if source is Leak or Faucet, tighter tolerances are used";  } }

    }

    public abstract class ThreeOutOfFourNaiveAdopter : NaiveAdopter {
        public ThreeOutOfFourNaiveAdopter() : base() {}
        public ThreeOutOfFourNaiveAdopter(Events events) : base(events) { }

        protected override bool IsSimilar(Event eventSource, Event eventTarget, double? volumePercent, double? peakPercent, double? durationPercent, double? modePercent) {
            return eventTarget.IsThreeOutOfFourSimilar(eventSource, volumePercent, peakPercent, durationPercent, modePercent);
        }
    }

    public class v15p18d25m18ThreeOutOfFourNaiveAdopter : ThreeOutOfFourNaiveAdopter {
        public v15p18d25m18ThreeOutOfFourNaiveAdopter() : this(null) {
        }
        public v15p18d25m18ThreeOutOfFourNaiveAdopter(Events events) : base(events) {
            VolumePercent = 0.15;
            PeakPercent = 0.18;
            DurationPercent = 0.25;
            ModePercent = 0.18;
        }

        public override string Name { get { return "v15p18d25m18 ThreeOutOfFour Naive Adopter"; } }
        public override string Description { get { return "Adopts if 3 out of 4 are satisfied: volume within 15%, peak within 18%, duration within 25%, mode within 18%"; } }
    }

    public abstract class ProximityAdopter : v15p18d25m18NaiveAdopter {
        public ProximityAdopter()
            : this(null) {
        }
        public ProximityAdopter(Events events)
            : base(events) {
        }

        protected int minutes = 0;
        protected FixtureClass fixtureClass = FixtureClasses.Unclassified;

        protected override bool AdoptLow(Event eventSource, Event eventTargetPredicted, Event eventTargetActual) {
            if (IsGenerallyAdoptable(eventSource, eventTargetPredicted)
                && IsSpecificallyAdoptable(eventSource, eventTargetPredicted, eventTargetActual)) {
                SetFixtureClass(eventSource, eventTargetPredicted, eventTargetActual);
                return true;
            } else
                return false;
        }

        bool IsSpecificallyAdoptable(Event eventSource, Event eventTargetPredicted, Event eventTargetActual) {
            if (eventSource.FixtureClass != fixtureClass)
                return true;
            else if (eventTargetPredicted.FixtureClass != FixtureClasses.Leak && eventTargetPredicted.FixtureClass != FixtureClasses.Faucet)
                return true;
            else {
                List<Event> eventsInRecentPast = PredictedEventsSorted.InRecentPast(eventTargetPredicted, minutes, fixtureClass);
                if (eventsInRecentPast != null && eventsInRecentPast.Count > 0)
                    return true;
            }
            return false;
        }
    }

    public class ProximityTreatmentAdopter : ProximityAdopter {
        public ProximityTreatmentAdopter()
            : this(null) {
        }
        public ProximityTreatmentAdopter(Events events)
            : base(events) {
            minutes = 5;
            fixtureClass = FixtureClasses.Treatment;
        }
        
        public override string Name { get { return "Proximity Treatment Adopter"; } }
        public override string Description { get { return "Adopts if volume within 15%, peak within 18%, duration within 25%, and mode within 18%; If Adoption Source is Treatment, then Adoption Targets which are Predicted as Leaks or Faucets are only adopted if there is Treament within 5 minutes earlier";  } }
    }

    public class ProximityClothesWasherAdopter : ProximityAdopter {
        public ProximityClothesWasherAdopter()
            : this(null) {
        }
        public ProximityClothesWasherAdopter(Events events)
            : base(events) {
            minutes = 20;
            fixtureClass = FixtureClasses.Clotheswasher;
        }

        public override string Name { get { return "Proximity Clothes Washer Adopter"; } }
        public override string Description { get { return "Adopts if volume within 15%, peak within 18%, duration within 25%, and mode within 18%; If Adoption Source is Treatment, then Adoption Targets which are Predicted as Leaks or Faucets are only adopted if there is Clothes Washer within 20 minutes earlier"; } }
    }

    public class v15p18d25m18NaiveAdopter : NaiveAdopter {
        public v15p18d25m18NaiveAdopter()
            : this(null) {
        }
        public v15p18d25m18NaiveAdopter(Events events) : base(events) {
            VolumePercent = 0.15;
            PeakPercent = 0.18;
            DurationPercent = 0.25;
            ModePercent = 0.18;
        }

        public override string Name { get { return "v15p18d25m18 Naive Adopter"; } }
        public override string Description { get { return "Adopts if volume within 15%, peak within 18%, duration within 25%, and mode within 18%"; } }
    }

    public class v15p18d25m18HighVolumeNaiveAdopter : v15p18d25m18NaiveAdopter {
        public v15p18d25m18HighVolumeNaiveAdopter() : this(null) {
        }

        public v15p18d25m18HighVolumeNaiveAdopter(Events events)
            : base(events) { }

        public override void Adopt(Event @event) {
            if (@event.Volume < 1)
                return;
            base.Adopt(@event);
        }

        public override void AdoptWithStatistics(Event @event) {
            if (@event.Volume < 1)
                return;
            base.AdoptWithStatistics(@event);
        }

        public override string Name { get { return "v15p18d25m18 High Volume Naive Adopter"; } }
        public override string Description { get { return "If source is not low-volume (<1), use v15p18d25m18 Adopter"; } }
    }

    public class v15p18d25m18NoFaucetNaiveAdopter : v15p18d25m18NaiveAdopter {
        public v15p18d25m18NoFaucetNaiveAdopter()
            : this(null) {
        }

        public v15p18d25m18NoFaucetNaiveAdopter(Events events)
            : base(events) { }

        public override void Adopt(Event @event) {
            if (@event.FixtureClass == FixtureClasses.Faucet)
                return;
            base.Adopt(@event);
        }

        public override void AdoptWithStatistics(Event @event) {
            if (@event.FixtureClass == FixtureClasses.Faucet)
                return;
            base.AdoptWithStatistics(@event);
        }

        public override string Name { get { return "v15p18d25m18 No Faucet Naive Adopter"; } }
        public override string Description { get { return "If source is not Faucet, use v15p18d25m18 Adopter"; } }
    }

    public class v12p15d20m15NaiveAdopter : NaiveAdopter {
        public v12p15d20m15NaiveAdopter() : this(null) { }
        public v12p15d20m15NaiveAdopter(Events events) : base(events) {
            VolumePercent = 0.12;
            PeakPercent = 0.15;
            DurationPercent = 0.20;
            ModePercent = 0.15;
        }

        public override string Name { get { return "v12p15d20m15 Naive Adopter"; } }
        public override string Description { get { return "Adopts if volume within 12%, peak within 15%, duration within 20%, and mode within 15%"; } }
    }

    public class v18p25d30m25NaiveAdopter : NaiveAdopter {
        public v18p25d30m25NaiveAdopter() : this(null) { }
        public v18p25d30m25NaiveAdopter(Events events) : base(events) {
            VolumePercent = 0.18;
            PeakPercent = 0.25;
            DurationPercent = 0.30;
            ModePercent = 0.25;
        }

        public override string Name { get { return "v18p25d30m25 Naive Adopter"; } }
        public override string Description { get { return "Adopts if volume within 18%, peak within 25%, duration within 30%, and mode within 25%"; } }
    }

    public class v05p05d05m05NaiveAdopter : NaiveAdopter {
        public v05p05d05m05NaiveAdopter() : this(null) { }
        public v05p05d05m05NaiveAdopter(Events events)
            : base(events) {
            VolumePercent = 0.05;
            PeakPercent = 0.05;
            DurationPercent = 0.05;
            ModePercent = 0.05;
        }

        public override string Name { get { return "v05p05d05m05 Naive Adopter"; } }
        public override string Description { get { return "Adopts if volume within 05%, peak within 05%, duration within 05%, and mode within 05%"; } }
    }

    public class v15p15d15m15NaiveAdopter : NaiveAdopter {
        public v15p15d15m15NaiveAdopter()
            : this(null) {
        }
        public v15p15d15m15NaiveAdopter(Events events)
            : base(events) {
            VolumePercent = 0.15;
            PeakPercent = 0.15;
            DurationPercent = 0.15;
            ModePercent = 0.15;
        }

        public override string Name { get { return "v15p15d15m15 Naive Adopter"; } }
        public override string Description { get { return "Adopts if volume within 15%, peak within 15%, duration within 15%, and mode within 15%"; } }
    }

    public class v10p10d10m10NaiveAdopter : NaiveAdopter {
        public v10p10d10m10NaiveAdopter()
            : this(null) {
        }
        public v10p10d10m10NaiveAdopter(Events events)
            : base(events) {
            VolumePercent = 0.10;
            PeakPercent = 0.10;
            DurationPercent = 0.10;
            ModePercent = 0.10;
        }

        public override string Name { get { return "v10p10d10m10 Naive Adopter"; } }
        public override string Description { get { return "Adopts if volume within 10%, peak within 10%, duration within 10%, and mode within 10%"; } }
    }

    public class v20p20d20m20NaiveAdopter : NaiveAdopter {
        public v20p20d20m20NaiveAdopter()
            : this(null) {
        }
        public v20p20d20m20NaiveAdopter(Events events)
            : base(events) {
            VolumePercent = 0.20;
            PeakPercent = 0.20;
            DurationPercent = 0.20;
            ModePercent = 0.20;
        }

        public override string Name { get { return "v20p20d20m20 Naive Adopter"; } }
        public override string Description { get { return "Adopts if volume within 20%, peak within 20%, duration within 20%, and mode within 20%"; } }
    }

    public class v15p20d50m20NaiveAdopter : NaiveAdopter {
        public v15p20d50m20NaiveAdopter()
            : this(null) {
        }
        public v15p20d50m20NaiveAdopter(Events events)
            : base(events) {
            VolumePercent = 0.15;
            PeakPercent = 0.20;
            DurationPercent = 0.50;
            ModePercent = 0.20;
        }

        public override string Name { get { return "v15p20d50m20 Naive Adopter"; } }
        public override string Description { get { return "Adopts if volume within 15%, peak within 20%, duration within 50%, and mode within 20%"; } }
    }

    public class m05NaiveAdopter : NaiveAdopter {
        public m05NaiveAdopter()
            : this(null) {
        }
        public m05NaiveAdopter(Events events)
            : base(events) {
            VolumePercent = null;
            PeakPercent = null;
            DurationPercent = null;
            ModePercent = 0.05;
        }

        public override string Name { get { return "m05 Naive Adopter"; } }
        public override string Description { get { return "Adopts if mode within 05%"; } }
    }

}
