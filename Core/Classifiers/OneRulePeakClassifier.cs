using System;

using TraceWizard.Entities;
using TraceWizard.Classification;

namespace TraceWizard.Classification.Classifiers.OneRulePeak {
    public class OneRulePeakClassifier : Classifier {
        public override FixtureClass Classify(Event @event) {

            if (@event.Peak < 0.305) return FixtureClasses.Leak;
            if (@event.Peak < 2.225) return FixtureClasses.Faucet;
            if (@event.Peak < 5.545) return FixtureClasses.Toilet;
            return FixtureClasses.Irrigation;
        }
 
        public override string Name { get { return "One Rule Peak Classifier"; } }
        public override string Description { get { return "Classifies using One Rule algorithm based on Peak attribute"; } }
    }
}
