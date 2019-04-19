using System;

using TraceWizard.Entities;
using TraceWizard.Classification;

namespace TraceWizard.Classification.Classifiers.Null {
    public class NullClassifier : Classifier {
        public override FixtureClass Classify(Event @event) {
            return FixtureClasses.Unclassified;
        }

        public override string Name { get { return "Null Classifier"; } }
        public override string Description { get { return "Classifies all events as Unclassified"; } }
    }
}
