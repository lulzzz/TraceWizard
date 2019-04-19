using System;

using TraceWizard.Entities;
using TraceWizard.Classification;

namespace TraceWizard.Classification.Classifiers.Leak {
    public class LeakClassifier : Classifier {
        public override FixtureClass Classify(Event @event) {

            if (@event.Peak < 0.305) return FixtureClasses.Leak;
            return FixtureClasses.Unclassified;
        }

        public override string Name { get { return "Leak Classifier"; } }
        public override string Description { get { return "Classifies as either Leak or Unclassified"; } }
    }
}
