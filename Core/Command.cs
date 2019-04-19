using System;
using System.Data.OleDb;

using System.Collections.Generic;

using TraceWizard.Entities;

namespace TraceWizard.Commanding {

    public class TwCommand {
        public TwCommandAction Action;
        public TwCommandFixtures Fixtures;
        public TwCommandConstraints Constraints;

        public TwCommand() {
            Fixtures = new TwCommandFixtures();
            Constraints = new TwCommandConstraints();
        }

        override public string ToString() {
            string s = null;
            s += ToString(Action);
            if (Fixtures != null)
                s += Fixtures.ToString();
            if (Constraints != null)
                s += Constraints.ToString();

            return s;
        }
        protected string ToString(TwCommandAction action) {
            switch (action) {
                case TwCommandAction.SelectNext:
                    return "Select Next";
                case TwCommandAction.SelectPrevious:
                    return "Select Previous";
                case TwCommandAction.SelectAll:
                    return "Select All";
                case TwCommandAction.ViewNext:
                    return "View Next";
                case TwCommandAction.ViewPrevious:
                    return "View Previous";
                case TwCommandAction.ClassifyAs:
                    return "Classify As";
                case TwCommandAction.ClassifySimilarAs:
                    return "Classify Similar As";
                case TwCommandAction.SetBookmark:
                    return "Set Bookmark";
                case TwCommandAction.GoToBookmark:
                    return "Go To Bookmark";
                case TwCommandAction.GoToStart:
                    return "Go To Start";
                case TwCommandAction.GoToEnd:
                    return "Go To End";
                default:
                    return action.ToString();
            }
        }
    }

    public class TwCommandConstraintDouble : TwCommandConstraint {
        public double Value;
        override public string ToString() {
            return ToString(Attribute) + " " + ToString(Relation) + " " + Value.ToString("0.00");
        }
    }

    public class TwCommandConstraintInteger : TwCommandConstraint {
        public int Value;
        override public string ToString() {
            return ToString(Attribute) + " " + ToString(Relation) + " " + Value.ToString("0");
        }
    }

    public enum TwCommandAction {
        None = 0,
        SelectNext = 1,
        SelectPrevious = 2,
        SelectAll = 3,
        ViewNext = 4,
        ViewPrevious = 5,
        ClassifyAs = 6,
        ClassifySimilarAs = 7,
        Time = 8,
        Undo = 9,
        Redo = 10,
        SetBookmark = 11,
        GoToBookmark = 12,
        GoToStart = 13,
        GoToEnd = 14,
        Refresh = 15,
        Approve = 16,
        Unapprove = 17
    };

    public class TwCommandFixtures : List<FixtureClass> {
        public bool Exclude;
        override public string ToString() {
            string s = null;
            foreach (FixtureClass fixtureClass in this) {
                s += " " + (Exclude ? "-" : string.Empty) + fixtureClass.FriendlyName;
            }
            return s;
        }
    }

    public class TwCommandConstraints : List<TwCommandConstraint> {
        override public string ToString() {
            string s = null;
            foreach (TwCommandConstraint constraint in this) {
                s += " " + constraint.ToString();
            }
            return s;
        }
    }

    public enum TwCommandConstraintAttribute {
        None = 0,
        Volume = 1,
        Peak = 2,
        Duration = 3,
        Mode = 4,
        FirstCycle = 5,
        ManuallyClassified = 6,
        ClassifiedUsingFixtureList = 7,
        ClassifiedUsingMachineLearning = 8,
        Approved = 9,
        Similar = 10,
        Selected = 11,
        DateTime = 12,
        Super = 13,
        Notes = 14,
        ListItem = 15
    };

    public enum TwCommandConstraintRelation {
        None = 0,
        LessThan = 1,
        LessThanOrEqual = 2,
        Equal = 3,
        GreaterThan = 4,
        GreaterThanOrEqual = 5
    };

    abstract public class TwCommandConstraint {
        public TwCommandConstraintAttribute Attribute;
        public TwCommandConstraintRelation Relation;
        override public abstract string ToString();
        protected string ToString(TwCommandConstraintRelation relation) {
            switch (relation) {
                case TwCommandConstraintRelation.LessThan:
                    return "<";
                case TwCommandConstraintRelation.LessThanOrEqual:
                    return "<=";
                case TwCommandConstraintRelation.GreaterThan:
                    return ">";
                case TwCommandConstraintRelation.GreaterThanOrEqual:
                    return ">=";
                case TwCommandConstraintRelation.Equal:
                    return "=";
                default:
                    return relation.ToString();
            }
        }
        protected string ToString(TwCommandConstraintAttribute attribute) {
            switch (attribute) {
                case TwCommandConstraintAttribute.FirstCycle:
                    return "1st";
                case TwCommandConstraintAttribute.ManuallyClassified:
                    return "Manual";
                case TwCommandConstraintAttribute.ClassifiedUsingFixtureList:
                    return "List";
                case TwCommandConstraintAttribute.ClassifiedUsingMachineLearning:
                    return "Machine";
                default:
                    return attribute.ToString();
            }
        }
    }

    public class TwCommandConstraintListItem : TwCommandConstraintBool {
        public FixtureProfiles FixtureProfiles;
        public FixtureClass ListItem;
        public int ListItemIndex;
        override public string ToString() {
            return (Value ? string.Empty : "-") + ListItem.Name.ToString() + ListItemIndex.ToString();
        }
    }

    public class TwCommandConstraintDateTime : TwCommandConstraint {
        public DateTime Value;
        override public string ToString() {
            return Value.ToString();
        }
    }

    public class TwCommandConstraintBool : TwCommandConstraint {
        public bool Value;
        override public string ToString() {
            return (Value ? string.Empty : "-") + ToString(Attribute);
        }
    }

    public class TwCommandConstraintBoolEvent : TwCommandConstraintBool {
        public Event EventSource;
    }

    public class TwCommandConstraintTimeSpan : TwCommandConstraint {
        public TimeSpan Value;
        override public string ToString() {
            return ToString(Attribute) + " " + ToString(Relation) + " " + ToString(Value);
        }
        public string ToString(TimeSpan value) {
            return (new TwHelper.FriendlyDurationConverter()).Convert(value, null, null, null).ToString();
        }
    }
}