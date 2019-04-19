using System;
using System.Data.OleDb;

using System.Collections.Generic;

using TraceWizard.Entities;
using TraceWizard.TwApp;

namespace TraceWizard.Commanding {

    public class CommandParser {

        public class TwCommandParseResult {
            public string SuccessMessage = null;
            public string ErrorMessage = null;
            public TwCommand Command;

            public TwCommandParseResult() {
                Command = new TwCommand();
            }
        }
        
        public CommandParser() {
        }

        TwCommandAction GetAction(string[] tokens, ref int index, ref string error) {
            if (index >= tokens.Length) {
                error = "No Action specified";
                return TwCommandAction.None;
            }
           
            switch (tokens[index]) {
                case "select":
                case "sel":
                case "s":
                case "find":
                case "f":
                    if (++index < tokens.Length) {
                        switch (tokens[index]) {
                            case ("next"):
                            case ("n"):
                                index++;
                                return TwCommandAction.SelectNext;
                            case ("previous"):
                            case ("prev"):
                            case ("p"):
                                index++;
                                return TwCommandAction.SelectPrevious;
                            case ("all"):
                            case ("a"):
                                index++;
                                return TwCommandAction.SelectAll;
                            default:
                                error = "Invalid Action: Invalid Select direction: " + BuildTokens(tokens, index);
                                return TwCommandAction.None;
                        }
                    } else {
                        error = "Invalid Action: Select action direction not specified";
                        return TwCommandAction.None;
                    }
                case "selectnext":
                case "selectn":
                case "selnext":
                case "seln":
                case "snext":
                case "sn":
                case "next":
                case "n":
                case "findnext":
                case "findn":
                case "fnext":
                case "fn":
                    index++;
                    return TwCommandAction.SelectNext;
                case "selectprevious":
                case "selectprev":
                case "selectp":
                case "selprevious":
                case "selprev":
                case "selp":
                case "sprevious":
                case "sprev":
                case "sp":
                case "previous":
                case "prev":
                case "p":
                case "findprevious":
                case "findprev":
                case "findp":
                case "fprevious":
                case "fprev":
                case "fp":
                    index++;
                    return TwCommandAction.SelectPrevious;
                case "selectall":
                case "selecta":
                case "selall":
                case "sela":
                case "sall":
                case "sa":
                case "all":
                case "a":
                case "findall":
                case "finda":
                case "fall":
                case "fa":
                    index++;
                    return TwCommandAction.SelectAll;
                case "view":
                case "v":
                    if (++index < tokens.Length) {
                        switch(tokens[index]) {
                            case ("next"):
                            case ("n"):
                                index++;
                                return TwCommandAction.ViewNext;
                            case ("previous"):
                            case ("prev"):
                            case ("p"):
                                index++;
                                return TwCommandAction.ViewPrevious;
                            default:
                                error = "Invalid Action: Invalid View direction: " + BuildTokens(tokens, index);
                                return TwCommandAction.None;
                        }
                    } else {
                        error = "Invalid Action: View action direction not specified";
                        return TwCommandAction.None;
                    }
                case "viewnext":
                case "viewn":
                case "vnext":
                case "vn":
                    index++;
                    return TwCommandAction.ViewNext;
                case "viewprevious":
                case "viewprev":
                case "viewp":
                case "vprevious":
                case "vprev":
                case "vp":
                    index++;
                    return TwCommandAction.ViewPrevious;
                case "approve":
                case "app":
                    index++;
                    return TwCommandAction.Approve;
                case "unapprove":
                case "unapp":
                case "-approve":
                case "-app":
                    index++;
                    return TwCommandAction.Unapprove;
                case "classify":
                case "class":
                case "c":
                    if (++index < tokens.Length) {
                        switch (tokens[index]) {
                            case ("similar"):
                            case ("sim"):
//                            case ("s"): // conflicts with shower
                                if (++index < tokens.Length) {
                                    switch (tokens[index]) {
                                        case "as":
                                            break;
                                        default:
                                            index--;
                                            break;
                                    }
                                }
                                index++;
                                return TwCommandAction.ClassifySimilarAs;
                            case ("as"):
                                break;
                            default:
                                index--;
                                break;
                        }
                    }
                    index++;
                    return TwCommandAction.ClassifyAs;
                case "classifysimilar":
                case "classifysim":
                case "classsimilar":
                case "classsim":
                case "csimilar":
                case "csim":
                case "cs":
                    if (++index < tokens.Length) {
                        switch (tokens[index]) {
                            case "as":
                                break;
                            default:
                                index--;
                                break;
                        }
                    }
                    index++;
                    return TwCommandAction.ClassifySimilarAs;
                case "time":
                case "t":
                case "date":
                case "d":
                case "datetime":
                case "dt":
                    index++;
                    return TwCommandAction.Time;
                case "undo":
                    index++;
                    return TwCommandAction.Undo;
                case "redo":
                    index++;
                    return TwCommandAction.Redo;
                case "refresh":
                    index++;
                    return TwCommandAction.Refresh;
                case "home":
                case "left":
                case "start":
                    index++;
                    return TwCommandAction.GoToStart;
                case "end":
                case "right":
                    index++;
                    return TwCommandAction.GoToEnd;
                case "set":
                case "setbookmark":
                case "setbook":
                case "setb":
                    if (++index < tokens.Length) {
                        switch (tokens[index]) {
                            case "bookmark":
                            case "book":
                            case "b":
                                break;
                            default:
                                index--;
                                break;
                        }
                    }
                    index++;
                    return TwCommandAction.SetBookmark;
                case "go":
                case "goto":
                    if (++index < tokens.Length) {
                        switch (tokens[index]) {
                            case "to":
                                if (++index < tokens.Length) {
                                    switch (tokens[index]) {
                                        case "bookmark":
                                        case "book":
                                        case "b":
                                            index++;
                                            return TwCommandAction.GoToBookmark;
                                        case "start":
                                        case "s":
                                        case "home":
                                        case "h":
                                        case "left":
                                        case "l":
                                            index++;
                                            return TwCommandAction.GoToStart;
                                        case "end":
                                        case "e":
                                        case "right":
                                        case "r":
                                            index++;
                                            return TwCommandAction.GoToEnd;
                                        default:
                                            index--;
                                            break;
                                    }
                                }
                                break;
                            case "bookmark":
                            case "book":
                            case "b":
                                index++;
                                return TwCommandAction.GoToBookmark;
                            case "start":
                            case "s":
                            case "home":
                            case "h":
                            case "left":
                            case "l":
                                index++;
                                return TwCommandAction.GoToStart;
                            case "end":
                            case "e":
                            case "right":
                            case "r":
                                index++;
                                return TwCommandAction.GoToEnd;
                            default:
                                index--;
                                break;
                        }
                    }
                    error = "Invalid Action: Go To action not specified";
                    return TwCommandAction.None;
                default:
                    error = "Invalid Action: " + BuildTokens(tokens, index);
                    return TwCommandAction.None;
            }
        }

        FixtureClass GetFixture(string token, ref string error, ref bool exclude) {
            exclude = token[0] == '-' ? true : false;

            string s = null;
            if (exclude && token.Length == 1) {
                error = "Invalid Fixture: " + token[0];
                return null;
            }
            else 
                s = exclude ? token.Substring(1) : token;

            FixtureClass fixtureClass = FixtureClasses.GetByName(s);
            if (fixtureClass == null)
                fixtureClass = FixtureClasses.GetByShortName(s);
            if (fixtureClass == null)
                fixtureClass = FixtureClasses.GetByCharacter(s);

            return fixtureClass;
        }
        
        TwCommandFixtures GetFixtures(string[] tokens, ref int index, ref string error) {
            var fixtures = new TwCommandFixtures();

            FixtureClass fixtureClass;
            bool exclude = false;
            while ((index < tokens.Length) && (fixtureClass = GetFixture(tokens[index], ref error, ref exclude)) != null) {
                if (fixtures.Count > 0 && fixtures.Exclude != exclude) {
                    error = "Fixtures must be either all included or all excluded: " + BuildTokens(tokens, index);
                    return null;
                }
                fixtures.Add(fixtureClass);
                fixtures.Exclude = exclude;
                index++;
            }

            return fixtures;
        }

        TwCommandConstraintRelation GetRelation(string token) {
            switch (token) {
                case "<":
                    return TwCommandConstraintRelation.LessThan;
                case ">":
                    return TwCommandConstraintRelation.GreaterThan;
                case "=":
                    return TwCommandConstraintRelation.Equal;
                default:
                    return TwCommandConstraintRelation.None;
            }
        }

        void SplitListItem(string listItem, ref FixtureClass fixtureClass, out int fixtureProfileIndex, out bool value) {

            if (listItem[0] == '-')
                value = false;
            else
                value = true;

            string listItemWithoutValue = listItem.Substring(value ? 0 : 1);
            
            int i = 0;
            fixtureProfileIndex = -1;
            bool indexFound = false;
            for (; i < listItemWithoutValue.Length; i++) {
                if (int.TryParse(listItemWithoutValue.Substring(i), out fixtureProfileIndex)) {
                    indexFound = true;
                    break;
                }
            }

            if (indexFound) {
                foreach (FixtureClass fixtureClassLoop in FixtureClasses.Items.Values) {
                    string fixtureName = listItemWithoutValue.Substring(0, i);
                        if (fixtureName.ToLower() == fixtureClassLoop.Name.ToLower() 
                            || fixtureName.ToLower() == fixtureClassLoop.ShortName.ToLower())
                            fixtureClass = fixtureClassLoop;
                }
            }
        }
        
        TwCommandConstraintListItem DispatchConstraintListItem(string[] tokens, ref int index) {
            string listItemToken = tokens[index];

            FixtureClass fixtureClass = null;
            int fixtureProfileIndex;
            bool value;
            SplitListItem(listItemToken, ref fixtureClass, out fixtureProfileIndex, out value);

            TwCommandConstraintListItem constraint = null;

            if (fixtureClass != null) {
                constraint = new TwCommandConstraintListItem();
                constraint.Attribute = TwCommandConstraintAttribute.ListItem;
                constraint.Relation = TwCommandConstraintRelation.Equal;
                constraint.Value = value;
                constraint.ListItemIndex = fixtureProfileIndex;
                constraint.ListItem = fixtureClass;
                index++;
            }
            return constraint;
        }

        TwCommandConstraintDateTime DispatchConstraintDateTime(string[] tokens, ref int index) {
            DateTime dateTime;
            string dateTimeToken = null;
            int i = 0;
            for (i = index; i < tokens.Length; i++) {
                dateTimeToken += tokens[i] + " ";
            }

            TwCommandConstraintDateTime constraint = null;
            
            if (DateTime.TryParse(dateTimeToken.Trim(), out dateTime)) {
                constraint = new TwCommandConstraintDateTime();
                constraint.Attribute = TwCommandConstraintAttribute.DateTime;
                constraint.Relation = TwCommandConstraintRelation.Equal;
                constraint.Value = dateTime;
                index = i;
            }
            return constraint;
        }
        
        TwCommandConstraint GetConstraint(string[] tokens, ref int index, ref string error) {
            error = null;

            var constraintDateTime = DispatchConstraintDateTime(tokens, ref index);
            if (constraintDateTime != null)
                return constraintDateTime;

            var constraintListItem = DispatchConstraintListItem(tokens, ref index);
            if (constraintListItem != null)
                return constraintListItem;

            switch (tokens[index]) {
                case "volume":
                case "vol": 
                    try {
                        var constraint = new TwCommandConstraintDouble();
                        constraint.Attribute = TwCommandConstraintAttribute.Volume;
                        constraint.Relation = GetRelation(tokens[index + 1]);
                        constraint.Value = double.Parse(tokens[index + 2]);
                        index = index + 3;
                        return constraint;
                    } catch {
                        error = "Invalid Volume constraint";
                        return null;
                    }
                case "peak":
                case "pe":
                case "pk":
                    try {
                        var constraint = new TwCommandConstraintDouble();
                        constraint.Attribute = TwCommandConstraintAttribute.Peak;
                        constraint.Relation = GetRelation(tokens[index + 1]);
                        constraint.Value = double.Parse(tokens[index + 2]);
                        index = index + 3;
                        return constraint;
                    } catch {
                        error = "Invalid Peak constraint";
                        return null;
                    }
                case "duration":
                case "dur": 
                    try {
                        var constraint = new TwCommandConstraintTimeSpan();
                        constraint.Attribute = TwCommandConstraintAttribute.Duration;
                        constraint.Relation = GetRelation(tokens[index + 1]);
                        constraint.Value = (TimeSpan)(new TwHelper.FriendlyDurationConverter().ConvertBack(tokens[index + 2],null, null, null));
                        if (constraint.Value == TimeSpan.MinValue)
                            throw new Exception();
                        index = index + 3;
                        return constraint;
                    } catch {
                        error = "Invalid Duration constraint";
                        return null;
                    }
                case "mode":
                case "md": 
                    try {
                        var constraint = new TwCommandConstraintDouble();
                        constraint.Attribute = TwCommandConstraintAttribute.Mode;
                        constraint.Relation = GetRelation(tokens[index + 1]);
                        constraint.Value = Double.Parse(tokens[index + 2]);
                        index = index + 3;
                        return constraint;
                    } catch {
                        error = "Invalid Mode constraint";
                        return null;
                    }
                case "1stcycle":
                case "-1stcycle":
                case "1st":
                case "-1st":
                case "1":
                case "-1":
                    try {
                        var constraint = new TwCommandConstraintBool();
                        constraint.Attribute = TwCommandConstraintAttribute.FirstCycle;
                        constraint.Value = tokens[index][0] == '-' ? false :true;
                        index = index + 1;
                        return constraint;
                    } catch { return null; }
                case "manuallyclassified":
                case "-manuallyclassified":
                case "manual":
                case "-manual":
                case "man":
                case "-man":
                case "preserved":
                case "-preserved":
                    try {
                        var constraint = new TwCommandConstraintBool();
                        constraint.Attribute = TwCommandConstraintAttribute.ManuallyClassified;
                        constraint.Value = tokens[index][0] == '-' ? false : true;
                        index = index + 1;
                        return constraint;
                    } catch { return null; }
                case "classifiedusingmachinelearning":
                case "-classifiedusingmachinelearning":
                case "classifiedbymachinelearning":
                case "-classifiedbymachinelearning":
                case "machinelearning":
                case "-machinelearning":
                case "machine":
                case "-machine":
                    try {
                        var constraint = new TwCommandConstraintBool();
                        constraint.Attribute = TwCommandConstraintAttribute.ClassifiedUsingMachineLearning;
                        constraint.Value = tokens[index][0] == '-' ? false : true;
                        index = index + 1;
                        return constraint;
                    } catch { return null; }
                case "classifedusingfixturelist":
                case "-classifiedusingfixturelist":
                case "classifedbyfixturelist":
                case "-classifiedbyfixturelist":
                case "fixturelist":
                case "-fixturelist":
                case "list":
                case "-list":
                    try {
                        var constraint = new TwCommandConstraintBool();
                        constraint.Attribute = TwCommandConstraintAttribute.ClassifiedUsingFixtureList;
                        constraint.Value = tokens[index][0] == '-' ? false : true;
                        index = index + 1;
                        return constraint;
                    } catch { return null; }
                case "manuallyapproved":
                case "-manuallyapproved":
                case "manuallyapprove":
                case "-manuallyapprove":
                case "approved":
                case "-approved":
                case "approve":
                case "-approve":
                case "app":
                case "-app":
                    try {
                        var constraint = new TwCommandConstraintBool();
                        constraint.Attribute = TwCommandConstraintAttribute.Approved;
                        constraint.Value = tokens[index][0] == '-' ? false : true;
                        index = index + 1;
                        return constraint;
                    } catch { return null; }
                case "similar":
                case "-similar":
                case "sim":
                case "-sim":
                    try {
                        var constraint = new TwCommandConstraintInteger();
                        constraint.Attribute = TwCommandConstraintAttribute.Similar;
                        constraint.Relation = GetRelation(tokens[index + 1]);
                        constraint.Value = int.Parse(tokens[index + 2]);
                        index = index + 3;
                        return constraint;
                    } catch {
                        try {
                            var constraint = new TwCommandConstraintBoolEvent();
                            constraint.Attribute = TwCommandConstraintAttribute.Similar;
                            constraint.Value = tokens[index][0] == '-' ? false : true;
                            index = index + 1;
                            return constraint;
                        } catch { return null; }
                    }
                case "selected":
                case "-selected":
                case "sel":
                case "-sel":
                    try {
                        var constraint = new TwCommandConstraintBool();
                        constraint.Attribute = TwCommandConstraintAttribute.Selected;
                        constraint.Value = tokens[index][0] == '-' ? false : true;
                        index = index + 1;
                        return constraint;
                    } catch { return null; }
                case "super":
                    try {
                        var constraint = new TwCommandConstraintBool();
                        constraint.Attribute = TwCommandConstraintAttribute.Super;
                        constraint.Value = tokens[index][0] == '-' ? false : true;
                        index = index + 1;
                        return constraint;
                    } catch { return null; }
                case "notes":
                    try {
                        var constraint = new TwCommandConstraintBool();
                        constraint.Attribute = TwCommandConstraintAttribute.Notes;
                        constraint.Value = tokens[index][0] == '-' ? false : true;
                        index = index + 1;
                        return constraint;
                    } catch { return null; }
                default:
                    error = "Invalid constraint: " + BuildTokens(tokens,index);
                    return null;
            }
        }

        TwCommandConstraints GetConstraints(string[] tokens, ref int index, ref string error) {
            var constraints = new TwCommandConstraints();

            TwCommandConstraint constraint;
            while ((index < tokens.Length) && (constraint = GetConstraint(tokens, ref index, ref error)) != null)
                constraints.Add(constraint);

            return constraints;
        }

        string BuildTokens(string[] tokens, int index) {
            string s = null;
            for (; index < tokens.Length; index++)
                s += tokens[index] + " ";
                return s;
        }

        string Normalize(string s){
            s = s.Replace("<"," < ");
            s = s.Replace("="," = ");
            s = s.Replace(">"," > ");
            return s;
        }
        
        public TwCommandParseResult Parse(string command) {

            command = Normalize(command);
            
            char[] delimiters = new char[] { ' ', };
            string[] tokens = command.Trim().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            int index = 0;
            var result = new TwCommandParseResult();
            string error = null;

            if (string.IsNullOrEmpty(error))
                result.Command.Action = GetAction(tokens, ref index, ref error);

            if (string.IsNullOrEmpty(error))
                result.Command.Fixtures = GetFixtures(tokens, ref index, ref error);

            if (string.IsNullOrEmpty(error))
                result.Command.Constraints = GetConstraints(tokens, ref index, ref error);

            if (string.IsNullOrEmpty(error)) {
                result.SuccessMessage = result.Command.ToString();
            } else {
                result.ErrorMessage = error;
            }
            return result;
        }
    }
}
