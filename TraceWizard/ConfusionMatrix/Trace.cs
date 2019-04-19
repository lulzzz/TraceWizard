using System;
using System.Windows.Data;
using System.Collections.Generic;
using System.ComponentModel;

using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using TraceWizard.Services;
using TraceWizard.Entities;
using TraceWizard.Logging;
using TraceWizard.Adoption;
using TraceWizard.Classification;
using TraceWizard.Disaggregation;
using TraceWizard.Disaggregation.Disaggregators.Tw4;
using TraceWizard.Logging.Adapters.MeterMasterJet;
using TraceWizard.Entities.Adapters.Tw4Jet;

namespace TraceWizard.TwApp {

    public class Trace{
        public string KeyCode { get; set; }

        public Trace() {}

        public Trace(string keyCode) { KeyCode = keyCode; }

        public Events DisaggregationActual { get; set; }
        public Events DisaggregationPredicted { get; set; }

        public Analysis ClassificationActual { get; set; }
        public Analysis ClassificationPredicted { get; set; }

        public ClassificationAggregation ClassificationAggregationActual { get; set; }
        public ClassificationAggregation ClassificationAggregationPredicted { get; set; }

        public Events AdoptionSourcesActual { get; set; }
        public Events AdoptionSourcesPredicted { get; set; }

        public Events AdoptionTargetsActual { get; set; }
        public Events AdoptionTargetsPredicted { get; set; }

        public ConfusionMatrix ConfusionMatrix { get; set; }
        public ConfusionMatrixStatistics ConfusionMatrixStatistics { get; set; }

        public int Count { get; set; }
        public int AdoptionCount { get; set; }

        public void Load() {

            ClassificationAggregationActual = new ClassificationAggregation(ClassificationActual);
            ClassificationAggregationPredicted = new ClassificationAggregation(ClassificationPredicted);

            CalculateConfusionMatrix();
        }

        public void Adopt(Adopter adopter) {
            adopter.Adopt(ClassificationPredicted.Events, ClassificationActual.Events);

            AdoptionCount = adopter.AdoptionSourcesActual.Count;
                
            AdoptionSourcesPredicted = adopter.AdoptionSourcesPredicted;
            AdoptionSourcesActual = adopter.AdoptionSourcesActual;

            AdoptionTargetsPredicted = adopter.AdoptionTargetsPredicted;
            AdoptionTargetsActual = adopter.AdoptionTargetsActual;
        }
        
        public void Load(Trace trace) {
            Count++;
            AdoptionCount += trace.AdoptionCount;

            LoadClassificationAggregationActual(trace);
            LoadClassificationAggregationPredicted(trace);

            LoadAdoptionSources(trace);
            LoadAdoptionTarget(trace);
        }

        void LoadClassificationAggregationActual(Trace trace) {
            if (ClassificationAggregationActual == null)
                ClassificationAggregationActual = new ClassificationAggregation();

            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values)
                ClassificationAggregationActual.FixtureSummaries[fixtureClass].Update(trace.ClassificationAggregationActual.Events);
            foreach (Event @event in trace.ClassificationAggregationActual.Events)
                ClassificationAggregationActual.Events.Add(@event);

        }

        void LoadClassificationAggregationPredicted(Trace trace) {
            if (ClassificationAggregationPredicted == null)
                ClassificationAggregationPredicted = new ClassificationAggregation();

            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values)
                ClassificationAggregationPredicted.FixtureSummaries[fixtureClass].Update(trace.ClassificationAggregationPredicted.Events);
            foreach (Event @event in trace.ClassificationAggregationPredicted.Events)
                ClassificationAggregationPredicted.Events.Add(@event);
        }

        void LoadAdoptionSources(Trace trace) {
            if (AdoptionSourcesPredicted == null)
                AdoptionSourcesPredicted = new Events();
            if (AdoptionSourcesActual == null)
                AdoptionSourcesActual = new Events();

            foreach (Event @event in trace.AdoptionSourcesPredicted)
                AdoptionSourcesPredicted.Add(@event);
            foreach (Event @event in trace.AdoptionSourcesActual)
                AdoptionSourcesActual.Add(@event);
        }

        void LoadAdoptionTarget(Trace trace) {
            if (AdoptionTargetsPredicted == null)
                AdoptionTargetsPredicted = new Events();
            if (AdoptionTargetsActual == null)
                AdoptionTargetsActual = new Events();

            foreach (Event @event in trace.AdoptionTargetsPredicted)
                AdoptionTargetsPredicted.Add(@event);
            foreach (Event @event in trace.AdoptionTargetsActual)
                AdoptionTargetsActual.Add(@event);
        }

        public void CalculateConfusionMatrix() {
            ConfusionMatrix = new ConfusionMatrix();
            ConfusionMatrix.Populate(ClassificationAggregationPredicted, ClassificationAggregationActual);
            ConfusionMatrix.PopulateAdoptionSources(AdoptionSourcesPredicted, AdoptionSourcesActual);
            ConfusionMatrix.PopulateAdoptionTargets(AdoptionTargetsPredicted, AdoptionTargetsActual);

            ConfusionMatrixStatistics = new ConfusionMatrixStatistics(this);

            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values)
                ClassificationAggregationPredicted.FixtureSummaries[fixtureClass].Events.UpdateMedians();
            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values)
                ClassificationAggregationActual.FixtureSummaries[fixtureClass].Events.UpdateMedians();
        }        
    }

    public class ConfusionMatrix : Dictionary<FixtureClass, ConfusionMatrixRow> {
        public ConfusionMatrix() {
            foreach (FixtureClass fixtureClassRow in FixtureClasses.Items.Values) {
                ConfusionMatrixRow confusionMatrixRow = new ConfusionMatrixRow(fixtureClassRow);
                Add(fixtureClassRow, confusionMatrixRow);
            }
        }

        public void Populate(Analysis analysisPredicted, Analysis analysisActual) {
            for (int i = 0; i < analysisActual.Events.Count; i++) {
                FixtureClass fixtureClassActual = analysisActual.Events[i].FixtureClass;
                FixtureClass fixtureClassPredicted = analysisPredicted.Events[i].FixtureClass;
                (this[fixtureClassPredicted])[fixtureClassActual].Events.Add(analysisPredicted.Events[i]);
                (this[fixtureClassPredicted])[fixtureClassActual].Count++;
                (this[fixtureClassPredicted])[fixtureClassActual].Volume += analysisActual.Events[i].Volume;
            }
            foreach (FixtureClass fixtureClassRow in FixtureClasses.Items.Values) {
                foreach (FixtureClass fixtureClassColumn in FixtureClasses.Items.Values) {
                    (this[fixtureClassRow])[fixtureClassColumn].Events.UpdateMedians();
                }
            }
        }

        public void PopulateAdoptionSources(Events adoptionsPredicted, Events adoptionsActual) {
            for (int i = 0; i < adoptionsActual.Count; i++) {
                FixtureClass fixtureClassActual = adoptionsActual[i].FixtureClass;
                FixtureClass fixtureClassPredicted = adoptionsPredicted[i].FixtureClass;
                (this[fixtureClassPredicted])[fixtureClassActual].AdoptionSources.Add(adoptionsPredicted[i]);
            }
        }

        public void PopulateAdoptionTargets(Events adoptionsPredicted, Events adoptionsActual) {
            for (int i = 0; i < adoptionsActual.Count; i++) {
                FixtureClass fixtureClassActual = adoptionsActual[i].FixtureClass;
                FixtureClass fixtureClassPredicted = adoptionsPredicted[i].FixtureClass;
                (this[fixtureClassPredicted])[fixtureClassActual].AdoptionTargets.Add(adoptionsPredicted[i]);
            }
        }

    }

    public class ConfusionMatrixRow : Dictionary<FixtureClass, ConfusionMatrixCell> {
        public FixtureClass FixtureClass {get;set;}
        public ConfusionMatrixRow(FixtureClass fixtureClassRow) {
            FixtureClass = fixtureClassRow;
            foreach (FixtureClass fixtureClassColumn in FixtureClasses.Items.Values) {
                ConfusionMatrixCell confusionMatrixCell = new ConfusionMatrixCell(fixtureClassRow, fixtureClassColumn);
                Add(fixtureClassColumn, confusionMatrixCell);
            }
        }
    }

    public class ConfusionMatrixCell {
        public Events Events { get; set; }
        public Events AdoptionSources { get; set; }
        public Events AdoptionTargets { get; set; }
        public FixtureClass FixtureClassPredicted { get; set; }
        public FixtureClass FixtureClassActual { get; set; }
        public int Count { get; set; }
        public int CountPercent { get; set; }
        public double Volume { get; set; }
        public double VolumePercent { get; set; }

        public ConfusionMatrixCell(FixtureClass fixtureClassPredicted, FixtureClass fixtureClassActual) {
            Events = new Events();
            AdoptionSources = new Events();
            AdoptionTargets = new Events();
            FixtureClassPredicted = fixtureClassPredicted;
            FixtureClassActual = fixtureClassActual;
        }
    }

    public class ConfusionMatrixStatistics {
        public Trace Trace {get;set;}

        public Dictionary<FixtureClass, BinaryClassifierStatistics> BinaryClassifiersStatistics { get; set; }

        public ConfusionMatrixStatistics(Trace trace) { 
            Trace = trace;

            BinaryClassifiersStatistics = new Dictionary<FixtureClass, BinaryClassifierStatistics>();

            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values) {
                BinaryClassifierStatistics binaryClassifierStatistics = new BinaryClassifierStatistics(trace,fixtureClass);
                BinaryClassifiersStatistics.Add(fixtureClass,binaryClassifierStatistics);
            }        

        }

        public int TruePositives {
            get {
                int count = 0;
                foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values)
                    count += Trace.ConfusionMatrix[fixtureClass][fixtureClass].Count;
                return count;
            }
        }

        public double TruePositivesVolume {
            get {
                double volume = 0;
                foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values)
                    volume += Trace.ConfusionMatrix[fixtureClass][fixtureClass].Volume;
                return volume;
            }
        }

        public double KappaCoefficient { get { return ((double)(TruePositives - ExpectedAgreement)) / ((double)(TotalInstanceCount - ExpectedAgreement)); } }

        private double ExpectedAgreement {
            get {
            double result = 0;
            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values) {
                double countPredicted = Trace.ClassificationAggregationPredicted.FixtureSummaries[fixtureClass].Count;
                double countActual = Trace.ClassificationAggregationActual.FixtureSummaries[fixtureClass].Count;
                result += (countPredicted * countActual) / (double) TotalInstanceCount;
            }
            return result;
            }
        }

        public int TotalInstanceCount { get { return Trace.ClassificationAggregationActual.Events.Count; } }

        public int CorrectInstanceCount { get { return TruePositives; } }
        public double CorrectInstanceAccuracy { get { return (double)TruePositives / (double)(TotalInstanceCount); } }

        public double TotalVolume { get { return Trace.ClassificationAggregationActual.Events.Volume; } }
        public double VolumeAccuracy { get { return (double)TruePositivesVolume / (double)(TotalVolume); } }

        public int IncorrectInstanceCount { get { return Trace.ConfusionMatrixStatistics.TotalInstanceCount - Trace.ConfusionMatrixStatistics.TruePositives; } }
        public double IncorrectInstanceAccuracy { get { return 1.0 - Trace.ConfusionMatrixStatistics.CorrectInstanceAccuracy; } }
      
        public int AdopterCorrectInstanceCount { get { return TotalInstanceCount - Trace.AdoptionCount; } }
        public double AdopterCorrectInstanceAccuracy { get { return (double)AdopterCorrectInstanceCount / (double)TotalInstanceCount; } }

        public int AdopterIncorrectInstanceCount { get { return Trace.AdoptionCount; } }
        public double AdopterIncorrectInstanceAccuracy { get { return (double)AdopterIncorrectInstanceCount / (double)TotalInstanceCount; } }
    }

    public class BinaryClassifierStatistics {
        public Trace Trace { get; protected set; }
        public FixtureClass FixtureClass { get; protected set; }
        public BinaryClassifierStatistics(Trace trace, FixtureClass fixtureClass) {
            Trace = trace; FixtureClass = fixtureClass; 
        }

        public int TotalInstanceCount { get { return Trace.ClassificationAggregationActual.Events.Count; } }
        public double TotalVolume { get { return Trace.ClassificationAggregationActual.Events.Volume; } } 

        public int TruePositives { get {return Trace.ConfusionMatrix[FixtureClass][FixtureClass].Count; } }
        public int FalsePositives {
            get {
                int count = 0;
                foreach (FixtureClass fixtureClassActual in FixtureClasses.Items.Values)
                    if (FixtureClass != fixtureClassActual)
                        count += Trace.ConfusionMatrix[FixtureClass][fixtureClassActual].Count;
                return count;
            }
        }

        public int Positives { get { return TruePositives + FalsePositives; } }
        public int TrueNegatives { get { return TotalInstanceCount - (Positives + FalseNegatives); } }

        public int FalseNegatives {
            get {
                int count = 0;
                foreach (FixtureClass fixtureClassActual in FixtureClasses.Items.Values)
                    if (FixtureClass != fixtureClassActual)
                        count += Trace.ConfusionMatrix[fixtureClassActual][FixtureClass].Count;
                return count;
            }            
        }
        public int Negatives { get { return TrueNegatives + FalseNegatives; } }

        public double TruePositiveRate { get { return (double)TruePositives / (double)(TruePositives + FalseNegatives); } }
        public double TrueNegativeRate { get { return (double)TrueNegatives / (double)(TrueNegatives + FalsePositives); } }
        public double Accuracy { get { return (double)(TruePositives + TrueNegatives) / (double)(Positives + Negatives); } }

        public double FalsePositiveRate { get { return (double)FalsePositives / (double)(TrueNegatives + FalsePositives); } }
        public double FalseDiscoveryRate { get { return (double)FalsePositives / (double)(Positives); } }

        public double Specificity { get { return TrueNegativeRate; } }
        public double Sensitivity { get { return TruePositiveRate; } }

        public double Recall { get { return TruePositiveRate; } }
        public double Precision { get { return (double)TruePositives / (double)(Positives); } }

        public double PositivePredictiveValue { get { return Precision; } }
        public double NegativePredictiveValue { get { return ((double)TrueNegatives) / (double)(Negatives); } }

        public double PositiveLikelihoodRatio { get { return TruePositiveRate / (1.0 - TrueNegativeRate); } }
        public double NegativeLikelihoodRatio { get { return (1.0 - TruePositiveRate) / TrueNegativeRate; } }

        public double YoudenIndex { get { return Sensitivity + Specificity - 1; } }
        
        public double FMeasure {
            get {
                return 2.0 * (
                    ((double)(Precision * Recall))
                    /
                    (double)(Precision + Recall));
            }
        }

        public double MatthewsCorrelationCoefficient {
            get {
                long factor1 = (TruePositives + FalsePositives) * (TruePositives + FalseNegatives);
                long factor2 = (TrueNegatives + FalsePositives) * (TrueNegatives + FalseNegatives);

                double sqrt1 = Math.Sqrt(factor1);
                double sqrt2 = Math.Sqrt(factor2);

                double denominator = sqrt1 * sqrt2;
                long numerator = (TruePositives * TrueNegatives) - (FalsePositives * FalseNegatives);

                return ((double)numerator) / denominator;
            }
        }

        public double TruePositivesVolume { get { return Trace.ConfusionMatrix[FixtureClass][FixtureClass].Volume; } }
        public double FalsePositivesVolume {
            get {
                double volume = 0;
                foreach (FixtureClass fixtureClassActual in FixtureClasses.Items.Values)
                    if (FixtureClass != fixtureClassActual)
                        volume += Trace.ConfusionMatrix[FixtureClass][fixtureClassActual].Volume;
                return volume;
            }
        }
        public double FalseNegativesVolume {
            get {
                double volume = 0;
                foreach (FixtureClass fixtureClassActual in FixtureClasses.Items.Values)
                    if (FixtureClass != fixtureClassActual)
                        volume += Trace.ConfusionMatrix[fixtureClassActual][FixtureClass].Volume;
                return volume;
            }
        }
        public double PositivesVolume { get { return TruePositivesVolume + FalsePositivesVolume; } }
        public double TrueNegativesVolume { get { return TotalVolume - (PositivesVolume + FalseNegativesVolume); } }

        public double TruePositiveRateVolume { get { return (double)TruePositivesVolume / (double)(TruePositivesVolume + FalseNegativesVolume); } }
        public double TrueNegativeRateVolume { get { return (double)TrueNegativesVolume / (double)(TrueNegativesVolume + FalsePositivesVolume); } }
    }

    public class ClassificationAggregation : Analysis {
        public ClassificationAggregation() {
            Events = new Events();
            this.FixtureSummaries = new FixtureSummaries(Events);
        }

        public ClassificationAggregation(Analysis analysis) : this() {
            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values)
                FixtureSummaries[fixtureClass].Update(analysis.Events);
            Events = analysis.Events;
            KeyCode = analysis.KeyCode;
        }
    }    

    public abstract class AnalysisFactory {
        public string Name { get; private set; }
        public List<string> Files { get; private set; }
        public AnalysisFactory(List<string> files) { Files = files; }
        protected abstract Analysis CreateAnalysis(string fileName);
        public virtual Analysis Create(string fileName) {
            Analysis analysis = CreateAnalysis(fileName);
            analysis.UpdateFixtureSummaries();
            return analysis;
        }
    }

    public class ClassificationFactoryPredicted : AnalysisFactory {
        public Classifier Classifier { get; set; }
        public ClassificationFactoryPredicted(List<string> files, Classifier classifier) : base(files) { Classifier = classifier; }
        public override Analysis Create(string keyCode) {
            Analysis analysis = base.Create(keyCode);
            return analysis;
        }
        protected override Analysis CreateAnalysis(string fileName) {
            Analysis analysis = TwServices.CreateAnalysis(fileName);
            return Classifier.Classify(analysis);
        }
    }

    public class ClassificationFactoryActual : AnalysisFactory {
        public ClassificationFactoryActual(List<string> files) : base(files) { }
        protected override Analysis CreateAnalysis(string fileName) {
            return TwServices.CreateAnalysis(fileName);
        }
    }
}