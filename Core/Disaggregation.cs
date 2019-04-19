using System;
using System.Collections.Generic;

using TraceWizard.Entities;
using TraceWizard.Analyzers;
using TraceWizard.Logging;
using TraceWizard.Disaggregation;

namespace TraceWizard.Disaggregation {

    public abstract class Disaggregator : Analyzer {

        public Log Log {get; set;} 

        protected Parameters Parameters { get; set; }

        public string GetName() { return Name; }
        public string GetDescription() { return Description; }

        public abstract string Name { get; }
        public abstract string Description { get; }

        protected Disaggregator() { Parameters = new Parameters(); }

        protected Disaggregator(Log log, Parameters parameters) {
            Log = log; Parameters = parameters;
        }
        protected Disaggregator(Log log)
            : this(log, new Parameters()) { }

        public abstract Events Disaggregate();
        public Events Disaggregate(Log log) {
            Log = log;
            return Disaggregate();
        }
        public Events Disaggregate(Log log, Parameters parameters) {
            Log = log;
            Parameters = parameters;
            return Disaggregate();
        }
    }

    public class Parameters {
        public double TrickleFlowMax { get; set; }
        public double BaseFlowMinContinue { get; set; }
        public double BaseVolMinBeginSuper { get; set; }
        public int BaseDurMinBeginSuper { get; set; }
        public double BaseDeltaFlowMinBeginSuper { get; set; }
        public double BaseDeltaFlowPercentMinBeginSuper { get; set; }
        public double VolMinBeginEvent { get; set; }
        public int TrickleDurMinBeginAfterBase { get; set; }
        public double SuperFlowAboveBaseMinContinue { get; set; }
        public double SuperFlowVolMaxForMerge { get; set; }
        public int PostTrickleDurMaxForMerge { get; set; }

        public Parameters() {
            TrickleFlowMax = 0.3D;
            BaseFlowMinContinue = 0.2D;
            BaseVolMinBeginSuper = 0.25D;
            BaseDurMinBeginSuper = 30;
            BaseDeltaFlowMinBeginSuper = 1D;
            BaseDeltaFlowPercentMinBeginSuper = 1.1D;
            VolMinBeginEvent = 0.01D;
            TrickleDurMinBeginAfterBase = 10;
            SuperFlowAboveBaseMinContinue = 0.4D;
            SuperFlowVolMaxForMerge = 0.01D;
            PostTrickleDurMaxForMerge = 60;
        }

        public Parameters(
            double trickleFlowMax,
            double baseFlowMinContinue,
            double baseVolMinBeginSuper,
            int baseDurMinBeginSuper,
            double baseDeltaFlowMinBeginSuper,
            double baseDeltaFlowPercentMinBeginSuper,
            double volMinBeginEvent,
            int trickleDurMinBeginAfterBase,
            double superFlowAboveBaseMinContinue,
            double superFlowVolMaxForMerge,
            int postTrickleDurMaxForMerge
            ) {
            TrickleFlowMax = trickleFlowMax;
            BaseFlowMinContinue = baseFlowMinContinue;
            BaseVolMinBeginSuper = baseVolMinBeginSuper;
            BaseDurMinBeginSuper = baseDurMinBeginSuper;
            BaseDeltaFlowMinBeginSuper = baseDeltaFlowMinBeginSuper;
            BaseDeltaFlowPercentMinBeginSuper = baseDeltaFlowPercentMinBeginSuper;
            VolMinBeginEvent = volMinBeginEvent;
            TrickleDurMinBeginAfterBase = trickleDurMinBeginAfterBase;
            SuperFlowAboveBaseMinContinue = superFlowAboveBaseMinContinue;
            SuperFlowVolMaxForMerge = superFlowVolMaxForMerge;
            PostTrickleDurMaxForMerge = PostTrickleDurMaxForMerge;
        }
    }
}
