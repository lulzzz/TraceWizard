using System;

using TraceWizard.Entities;
using TraceWizard.Classification;

namespace TraceWizard.Classification.Classifiers.RepTree {
    public class RepTreeClassifier : Classifier {
        public override FixtureClass Classify(Event @event) {

            if (@event.Peak < 0.31) return FixtureClasses.Leak;
            if (@event.Volume < 1.25) {
                if (@event.Peak < 2.85) return FixtureClasses.Faucet;
                return FixtureClasses.Clotheswasher;
            } else {
                if (@event.Volume < 4.65) {
                    if (@event.Peak < 2.08) return FixtureClasses.Faucet;
                    return FixtureClasses.Toilet;
                } else {
                    if (@event.Volume < 31.88) {
                        if (@event.Mode < 2.41) {
                            if (@event.Volume < 7.08) return FixtureClasses.Faucet;
                            return FixtureClasses.Shower;
                        } 
                        return FixtureClasses.Clotheswasher;
                    }
                    return FixtureClasses.Irrigation;
                }
            }
        }
        public override string Name { get { return "REPTree Classifier"; } }
        public override string Description { get { return "Classifies using REPTree algorithm"; } }
    }
}
