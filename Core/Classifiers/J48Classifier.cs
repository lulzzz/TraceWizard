using System;

using TraceWizard.Entities;
using TraceWizard.Classification;
using TraceWizard.Entities.Adapters.Arff;

namespace TraceWizard.Classification.Classifiers.J48 {

    public class J48IsolatedLeakClassifier : J48Classifier {
        public override string Name { get { return "J48 Isolated Leak Classifier"; } }
        public override string Description { get { return "Classifies using J48 algorithm and also classifies isolated predicted Leaks as Faucets"; } }

        public override FixtureClass Classify(Event @event) {
            FixtureClass fixtureClass = base.Classify(@event);
            if (fixtureClass == FixtureClasses.Leak && Analysis.Events.IsIsolated(@event, new TimeSpan(0, 20, 0)))
                fixtureClass = FixtureClasses.Faucet;

            return fixtureClass;
        }
    }

    public class J48IndoorClassifier : J48Classifier {
        public override string Name { get { return "J48 Indoor Classifier"; } }
        public override string Description { get { return "Classifies using J48 algorithm but no Irrigations are predicted"; } }

        public override FixtureClass Classify(Event @event) {
            FixtureClass fixtureClass = base.Classify(@event);
            if (fixtureClass == FixtureClasses.Irrigation)
                fixtureClass = FixtureClasses.Unclassified;

            return fixtureClass;
        }
    }

    public class J48ClothesWasherTopClassifier : J48Classifier {
        public override string Name { get { return "J48 Clothes Washer Top Classifier"; } }
        public override string Description { get { return "Classifies using J48 algorithm for Top-Loading Clothes Washers"; } }

        public override FixtureClass Classify(Event @event) {

            FixtureClass fixtureClass;

            if (@event.Peak <= 0.3) {
                fixtureClass = FixtureClasses.Leak;
            } else if (@event.Peak <= 2.07) {
                if (@event.Volume <= 1.237) {
                    fixtureClass = FixtureClasses.Faucet;
                } else if (@event.Volume <= 1.819) {
                    if (@event.Peak <= 1.15) {
                        fixtureClass = FixtureClasses.Dishwasher;
                    } else if (@event.Peak <= 1.86) {
                        fixtureClass = FixtureClasses.Faucet;
                    } else {
                        fixtureClass = FixtureClasses.Toilet;
                    }
                } else if (@event.Volume <= 4.424) {
                    if (@event.Duration.TotalSeconds <= 100) {
                        if (@event.Mode <= 1.71) {
                            fixtureClass = FixtureClasses.Faucet;
                        } else {
                            fixtureClass = FixtureClasses.Toilet;
                        }
                    } else {
                        fixtureClass = FixtureClasses.Faucet;
                    }
                } else if (@event.Volume <= 7.081) {
                    fixtureClass = FixtureClasses.Faucet;
                } else {
                    fixtureClass = FixtureClasses.Shower;
                }
            } else {
                if (@event.Volume <= 1.285) {
                    if (@event.Peak <= 2.88) {
                        fixtureClass = FixtureClasses.Faucet;
                    } else {
                        fixtureClass = FixtureClasses.Clotheswasher;
                    }
                } else if (@event.Volume <= 5.992) {
                    if (@event.Duration.TotalSeconds <= 90) {
                        fixtureClass = FixtureClasses.Toilet;
                    } else {
                        if (@event.Volume <= 3.391) {
                            fixtureClass = FixtureClasses.Toilet;
                        } else {
                            fixtureClass = FixtureClasses.Faucet;
                        }
                    }
                } else if (@event.Volume <= 31.949) {
                    if (@event.Mode <= 2.8) {
                        fixtureClass = FixtureClasses.Shower;
                    } else {
                        fixtureClass = FixtureClasses.Clotheswasher;
                    }
                } else {
                    fixtureClass = FixtureClasses.Irrigation;
                }
            }
            return fixtureClass;
        }
    }

    public class J48ClothesWasherFrontClassifier : J48Classifier {
        public override string Name { get { return "J48 Clothes Washer Front Classifier"; } }
        public override string Description { get { return "Classifies using J48 algorithm for Front-Loading Clothes Washers"; } }

        public override FixtureClass Classify(Event @event) {

            FixtureClass fixtureClass;

            if (@event.Peak <= 0.3) {
                if (@event.Volume <= 0.279) {
                    fixtureClass = FixtureClasses.Leak;
                } else {
                    if (@event.Mode <= 0.09) {
                        fixtureClass = FixtureClasses.Leak;
                    } else {
                        if (@event.Peak <= 0.16) {
                            fixtureClass = FixtureClasses.Cooler;
                        } else {
                            fixtureClass = FixtureClasses.Leak;
                        }
                    }
                }
            } else {
                if (@event.Volume <= 1.167) {
                    if (@event.Peak <= 0.75) {
                        fixtureClass = FixtureClasses.Faucet;
                    } else {
                        if (@event.Volume <= 0.237) {
                            if (@event.Peak <= 0.77) {
                                fixtureClass = FixtureClasses.Leak;
                            } else if (@event.Peak <= 0.84) {
                                fixtureClass = FixtureClasses.Faucet;
                            } else if (@event.Peak <= 0.86) {
                                fixtureClass = FixtureClasses.Leak;
                            } else {
                                fixtureClass = FixtureClasses.Faucet;
                            }
                        } else {
                            fixtureClass = FixtureClasses.Faucet;
                        }
                    }
                } else if (@event.Volume <= 3.697) {
                    if (@event.Peak <= 1.17) {
                        fixtureClass = FixtureClasses.Dishwasher;
                    } else if (@event.Peak <= 2.04) {
                        fixtureClass = FixtureClasses.Faucet;
                    } else {
                        fixtureClass = FixtureClasses.Toilet;
                    }
                } else {
                    if (@event.Peak <= 6.08){
                        if (@event.Volume <= 7.426) {
                            if (@event.Peak <= 1.86) {
                                fixtureClass = FixtureClasses.Faucet;
                            } else {
                                fixtureClass = FixtureClasses.Clotheswasher;
                            }
                        } else {
                            if (@event.Mode <= 2.36) {
                                fixtureClass = FixtureClasses.Shower;
                            } else {
                                fixtureClass = FixtureClasses.Irrigation;
                            }
                        }
                    } else {
                        fixtureClass = FixtureClasses.Irrigation;
                    }
                }
            }
            return fixtureClass;
        }
    }

    public class J48Classifier : Classifier {

        public override string Name { get { return "J48 Classifier"; } }
        public override string Description { get { return "Classifies using J48 algorithm"; } }

        //public override Analysis Classify() {
        //    foreach (Event @event in Analysis.Events)
        //        @event.FixtureClass = Classify(@event);
        //    return Analysis;
        //}

        // FIXUP COMMENTED OUT, this code is useful for classifiers using "extended" attributes
        // the kind that we don't calculate when loading analysis
        //public override Analysis Classify() {
        //    var analysisAdapterArff = new AnalysisAdapterArff();
        //    var analysis = new Analysis();
        //    EventsArff eventsArff = analysisAdapterArff.Load(Analysis.Events);
        //    foreach (EventArff eventArff in eventsArff) {
        //        eventArff.FixtureClass = Classify(eventArff);
        //        analysis.Events.Add(eventArff);
        //    }
        //    return analysis;
        //}

        public override FixtureClass Classify(Event @event) {

            FixtureClass fixtureClass;

            if (@event.Peak <= 0.3) {
                fixtureClass = FixtureClasses.Leak;
            } else {
                if (@event.Volume <= 1.237) {
                    if (@event.Mode <= 2.88) {
                        fixtureClass = FixtureClasses.Faucet;
                    } else {
                        fixtureClass = FixtureClasses.Clotheswasher;
                    }
                } else if (@event.Volume <= 4.659) {
                    if (@event.Peak <= 2.07) {
                        if (@event.Volume <= 1.819) {
                            if (@event.Peak <= 1.18) {
                                fixtureClass = FixtureClasses.Dishwasher;
                            } else {
                                fixtureClass = FixtureClasses.Faucet;
                            }
                        } else if (@event.Volume <= 3.595) {
                            if (@event.Mode <= 1.02) {
                                fixtureClass = FixtureClasses.Faucet;
                            } else if (@event.Mode <= 1.8) {
                                if (@event.Volume <= 2.528) {
                                    if (@event.Peak <= 1.4) {
                                        fixtureClass = FixtureClasses.Faucet;
                                    } else {
                                        if (@event.Volume <= 2.162) {
                                            fixtureClass = FixtureClasses.Faucet;
                                        } else {
                                            fixtureClass = FixtureClasses.Toilet;
                                        }
                                    }
                                } else {
                                    fixtureClass = FixtureClasses.Faucet;
                                }
                            } else {
                                fixtureClass = FixtureClasses.Toilet;
                            }
                        } else {
                            fixtureClass = FixtureClasses.Faucet;
                        }
                    } else if (@event.Volume <= 3.5) {
                        fixtureClass = FixtureClasses.Toilet;
                    } else {
                        if (@event.Duration.TotalSeconds <= 90) {
                            fixtureClass = FixtureClasses.Toilet;
                        } else {
                            fixtureClass = FixtureClasses.Clotheswasher;
                        }
                    }
                } else if (@event.Volume <= 31.949) {
                    if (@event.Mode <= 2.4) {
                        if (@event.Volume <= 7.081) {
                            fixtureClass = FixtureClasses.Faucet;
                        } else {
                            fixtureClass = FixtureClasses.Shower;
                        }
                    } else {
                        fixtureClass = FixtureClasses.Clotheswasher;
                    }
                } else {
                    fixtureClass = FixtureClasses.Irrigation;
                }
            }
            return fixtureClass;
        }
    }
}
