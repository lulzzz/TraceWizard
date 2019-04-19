using System;

using TraceWizard.Entities;
using TraceWizard.Classification;
using TraceWizard.Entities.Adapters.Arff;
using TraceWizard.Environment;

namespace TraceWizard.Classification.Classifiers.NearestNeighbor {

    public abstract class NearestNeighborClassifier : Classifier {

        public NearestNeighborClassifier() { }

        public Analysis Exemplars {get;set;}
        public string ExemplarsDataSource { get; set; }

        protected double? volumePercent { get; set; }
        protected double? peakPercent { get; set; }
        protected double? durationPercent { get; set; }
        protected double? modePercent { get; set; }

        public override FixtureClass Classify(Event @event) {

            LazyInitialize();

            if (Exemplars == null || Exemplars.Events == null)
                return FixtureClasses.Unclassified;

            FixtureClass fixtureClass;

            double? minVolume = @event.Decrease(@event.Volume, volumePercent);
            double? maxVolume = @event.Increase(@event.Volume, volumePercent);
            double? minPeak = @event.Decrease(@event.Peak, peakPercent);
            double? maxPeak = @event.Increase(@event.Peak, peakPercent);
            TimeSpan? minDuration = @event.DecreaseDuration(@event.Duration, durationPercent);
            TimeSpan? maxDuration = @event.IncreaseDuration(@event.Duration, durationPercent);
            double? minMode = @event.Decrease(@event.Mode, modePercent);
            double? maxMode = @event.Increase(@event.Mode, modePercent);

            var fixtureSummaries = new FixtureSummaries(Exemplars.Events);

            foreach (Event exemplar in Exemplars.Events) {
                if (exemplar.IsSimilar(@event.FixtureClass, minVolume, maxVolume, minPeak, maxPeak, minDuration, maxDuration, minMode, maxMode))
                    fixtureSummaries[exemplar.FixtureClass].Count++;
            }

            fixtureClass = fixtureSummaries.MaximumFixtureClass();

            return fixtureClass;
        }

        void LazyInitialize() {
            if (string.IsNullOrEmpty(ExemplarsDataSource))
                ExemplarsDataSource = TwEnvironment.TwExemplars;

            if (Exemplars == null && System.IO.File.Exists(ExemplarsDataSource)) {
                Exemplars = (new ArffAnalysisAdapter()).Load(ExemplarsDataSource);
            }
        }
    }
        
    public class NearestNeighborv15p18d25m18Classifier : NearestNeighborClassifier {

        public NearestNeighborv15p18d25m18Classifier()
            : base() {
            volumePercent = 0.15;
            peakPercent = 0.18;
            durationPercent = 0.25;
            modePercent = 0.18;
        }

        public override string Name { get { return "Nearest Neighbor Classifier"; } }
        public override string Description { get { return "Classifies using Nearest Neighbor algorithm (exemplars.twdb file must be installed)"; } }
    }
}
