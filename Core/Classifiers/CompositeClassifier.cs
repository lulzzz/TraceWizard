using System;

using TraceWizard.Entities;

namespace TraceWizard.Classification.Classifiers.Composite {

    public class CompositeClassifier : Classifier {

        public override string Name { get { return "Composite Classifier"; } }
        public override string Description { get { return "Classifies using Fixture List, then using Machine Learning"; } }

        public Classifier ClassifierMachineLearning;
        public Classifier ClassifierFixtureList;
        
    public override FixtureClass Classify(Event @event) {
            var fixtureClass = ClassifierFixtureList.Classify(@event);
            //if (fixtureClass == @event.FixtureClass)
            //    fixtureClass = ClassifierMachineLearning.Classify(@event);
            return fixtureClass;
        }
    }
}