using System;
using System.Collections.Generic;
using System.Reflection;

using TraceWizard.Entities;
using TraceWizard.Analyzers;
using TraceWizard.Classification.Classifiers.J48;
using TraceWizard.Classification.Classifiers.Null;
using TraceWizard.Classification.Classifiers.Leak;
using TraceWizard.Classification.Classifiers.OneRulePeak;
using TraceWizard.Classification.Classifiers.RepTree;
using TraceWizard.Classification.Classifiers.FixtureList;
using TraceWizard.Environment;

namespace TraceWizard.Classification {

    public static class TwClassifiers {

        public static bool CanLoad(Classifier classifier) {
            if (classifier is Classification.Classifiers.NearestNeighbor.NearestNeighborClassifier
                && !System.IO.File.Exists(TwEnvironment.TwExemplars))
                return false;

            return true;
        }

        public static List<Classifier> CreateClassifiers() {
            var classifiers = new List<Classifier>();

            classifiers.Add(new TraceWizard.Classification.Classifiers.J48.J48Classifier());

            classifiers.Add(new TraceWizard.Classification.Classifiers.FixtureList.FixtureListClassifier());

            classifiers.Add(new TraceWizard.Classification.Classifiers.J48.J48ClothesWasherFrontClassifier());
            classifiers.Add(new TraceWizard.Classification.Classifiers.J48.J48ClothesWasherTopClassifier());
            classifiers.Add(new TraceWizard.Classification.Classifiers.J48.J48IndoorClassifier());
            classifiers.Add(new TraceWizard.Classification.Classifiers.J48.J48IsolatedLeakClassifier());

            classifiers.Add(new TraceWizard.Classification.Classifiers.NearestNeighbor.NearestNeighborv15p18d25m18Classifier());

            classifiers.Add(new TraceWizard.Classification.Classifiers.Null.NullClassifier());
            classifiers.Add(new TraceWizard.Classification.Classifiers.Leak.LeakClassifier());

            classifiers.Add(new TraceWizard.Classification.Classifiers.OneRulePeak.OneRulePeakClassifier());

            classifiers.Add(new TraceWizard.Classification.Classifiers.RepTree.RepTreeClassifier());

            return classifiers;
        }
    }

    public abstract class Classifier : Analyzer {

        public Analysis Analysis { get; set; }

        public Classifier() { }
        public Classifier(Analysis analysis) { Analysis = analysis; }

        public string GetName() { return Name; }
        public string GetDescription() { return Description; }

        public abstract string Name { get; }
        public abstract string Description { get; }

        public virtual void PreClassify() { 
        }

        public abstract FixtureClass Classify(Event @event);
        public Analysis Classify(Analysis analysis) {
            Analysis = analysis; return Classify();
        }
        public virtual Analysis Classify() {
            Analysis.ClearFirstCycle();
            Analysis.ClearManuallyClassified();
            foreach (Event @event in Analysis.Events) {
                @event.FixtureClass = Classify(@event);
            }
            return Analysis;
        }
    }
}

