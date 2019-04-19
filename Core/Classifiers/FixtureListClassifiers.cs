using System;
using System.Collections.Generic;
using System.IO;

using TraceWizard.Entities;
using TraceWizard.Entities.Adapters;
using TraceWizard.Classification;
using TraceWizard.Classification.Classifiers.J48;

using TraceWizard.Environment;

namespace TraceWizard.Classification.Classifiers.FixtureList {

    public class FixtureListClassifier : Classifier {
        public FixtureListClassifier() : base() { }
        public FixtureListClassifier(Analysis analysis) : base(analysis) { }

        public override string Name { get { return "Fixture List Classifier"; } }
        public override string Description { get { return "Classifies using fixture list"; } }

        public FixtureProfiles FixtureProfiles { get; set; }

        public override FixtureClass Classify(Event @event) {
            foreach (FixtureProfile fixtureProfile in FixtureProfiles) {
                if (Event.MatchesFixture(@event, fixtureProfile)) {
                    @event.ClassifiedUsingFixtureList = true;
                    return fixtureProfile.FixtureClass;
                }
            }
//            return FixtureClasses.Unclassified;
            @event.ClassifiedUsingFixtureList = false;
            if (@event.FixtureClass == null)
                return FixtureClasses.Unclassified;
            else
                return @event.FixtureClass;
        }
    }
}