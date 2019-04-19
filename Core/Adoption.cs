using System;
using System.Linq;
using System.Collections.Generic;

using TraceWizard.Entities;
using TraceWizard.Analyzers;
using TraceWizard.Adoption.Adopters.Naive;
using TraceWizard.Adoption.Adopters.Null;

namespace TraceWizard.Adoption {

    public sealed class TwAdopters {
        static readonly TwAdopters instance = new TwAdopters();
        static TwAdopters() { }
        TwAdopters() { }
        public static TwAdopters Instance { get { return instance; } }

        Adopter nullAdopter { get; set; }
        public Adopter GetNullAdopter() {
            if (nullAdopter == null) {
                nullAdopter = new NullAdopter();
            }
            return nullAdopter;
        }

        Adopter defaultAdopter { get; set; }
        public Adopter GetDefaultAdopter() {
            if (defaultAdopter == null) {
                defaultAdopter = new v15p18d25m18NaiveAdopter();
            }
            return defaultAdopter;
        }

        public Adopter GetDefaultAdopter(Events events) {
            defaultAdopter = new v15p18d25m18NaiveAdopter(events);
            return defaultAdopter;
        }
    }

    public abstract class Adopter : Analyzer {

        public double? VolumePercent { get; set; }
        public double? PeakPercent { get; set; }
        public double? DurationPercent { get; set; }
        public double? ModePercent { get; set; }

        public Events Events { get; set; }

        public Events AdoptionSourcesPredicted { get; set; }
        public Events AdoptionSourcesActual { get; set; }

        public Events AdoptionTargetsPredicted { get; set; }
        public Events AdoptionTargetsActual { get; set; }

        protected Events PredictedEventsSorted { get; set; }
        protected Events ActualEventsSorted { get; set; }

        public Adopter() { }
        public Adopter(Events events) { Events = events; }

        public string GetName() { return Name; }
        public string GetDescription() { return Description; }

        public abstract string Name { get; }
        public abstract string Description { get; }

        public abstract void Adopt(Event @event);
        public abstract void Adopt(Event @event, List<UndoTaskClassify> undoTasks);
        public abstract void AdoptWithStatistics(Event @event);

        public void Adopt(Events predicted, Events actual) {

            AdoptionSourcesPredicted = new Events();
            AdoptionSourcesActual = new Events();

            AdoptionTargetsPredicted = new Events();
            AdoptionTargetsActual = new Events();

            IEnumerable<Event> predictedSorted = Enumerable.OrderBy(predicted, n => n.StartTime);
            IEnumerable<Event> actualSorted = Enumerable.OrderBy(actual, n => n.StartTime);

            PredictedEventsSorted = new Events();
            foreach (Event @event in predictedSorted) {
                Event evt = @event.CopyShallow();
                PredictedEventsSorted.Add(evt);
            }

            ActualEventsSorted = new Events();
            foreach(Event @event in actualSorted) {
                ActualEventsSorted.Add(@event);
            }

            for (int i = 0; i < PredictedEventsSorted.Count; i++) {
                Event actualEvent = ActualEventsSorted[i];
                Event predictedEvent = PredictedEventsSorted[i];

                if (predictedEvent.FixtureClass != actualEvent.FixtureClass) {
                    AdoptionSourcesPredicted.Add(predictedEvent.CopyShallow());
                    AdoptionSourcesActual.Add(actualEvent);

                    predictedEvent.FixtureClass = actualEvent.FixtureClass;
                    predictedEvent.ManuallyClassified = true;

                    AdoptWithStatistics(predictedEvent);
                }
            }
        }

        protected void SetFixtureClass(Event source, Event targetPredicted, Event targetActual) {
            targetPredicted.FixtureClass = source.FixtureClass;

            if (targetActual != null) {
                AdoptionTargetsPredicted.Add(targetPredicted.CopyShallow());
                AdoptionTargetsActual.Add(targetActual);
            }
        }

        protected virtual bool IsSimilar(Event eventSource, Event eventTarget, double? volumePercent, double? peakPercent, double? durationPercent, double? modePercent) {
            return eventTarget.IsSimilar(eventSource, volumePercent, peakPercent, durationPercent, modePercent);
        }
    }

    public abstract class NaiveAdopter : Adopter {
        public NaiveAdopter() : base() {}
        public NaiveAdopter(Events events) : base(events) { }

        public override void Adopt(Event eventSource, List<UndoTaskClassify> undoTasks) {
            for (int i = 0; i < Events.Count; i++) {
                Event eventTargetPredicted = Events[i];
                var eventTargetPredictedFixtureClassOld = eventTargetPredicted.FixtureClass;
                if (AdoptLow(eventSource, eventTargetPredicted, null)) {
                    var undoTask = UndoTaskClassify.CreateUndoTask(
                        eventTargetPredicted, 
                        eventTargetPredictedFixtureClassOld, 
                        eventTargetPredicted.FixtureClass, 
                        eventTargetPredicted.ManuallyClassified, 
                        eventTargetPredicted.ManuallyClassified,
                        eventTargetPredicted.ClassifiedUsingFixtureList,
                        eventTargetPredicted.ClassifiedUsingFixtureList);
                    undoTasks.Add(undoTask);
                }
            }
        }

        public override void Adopt(Event eventSource) {
            for (int i = 0; i < Events.Count; i++) {
                Event eventTargetPredicted = Events[i];
                AdoptLow(eventSource, eventTargetPredicted, null);
            }
        }

        public override void AdoptWithStatistics(Event eventSource) {
            for (int i = 0; i < PredictedEventsSorted.Count; i++) {
                Event eventTargetPredicted = PredictedEventsSorted[i];
                Event eventTargetActual = ActualEventsSorted[i];
                AdoptLow(eventSource, eventTargetPredicted, eventTargetActual);
            }
        }

        protected virtual bool IsGenerallyAdoptable(Event eventSource, Event eventTargetPredicted) {
            if (eventTargetPredicted.ManuallyClassified)
                return false;

            if (eventTargetPredicted.FixtureClass == eventSource.FixtureClass)
                return false;

            if (eventTargetPredicted.StartTime <= eventSource.StartTime)
                return false;

            if (!IsSimilar(eventSource, eventTargetPredicted, VolumePercent, PeakPercent, DurationPercent, ModePercent))
                return false;

            if (eventTargetPredicted.ManuallyClassified)
                return false;

            return true;
        }
        
        protected virtual bool AdoptLow(Event eventSource, Event eventTargetPredicted, Event eventTargetActual) {
            if (IsGenerallyAdoptable(eventSource, eventTargetPredicted)) {
                SetFixtureClass(eventSource, eventTargetPredicted, eventTargetActual);
                return true;
            }
            return false;
        }
   }
}

