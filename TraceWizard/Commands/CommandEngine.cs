using System;
using System.Data.OleDb;

using System.Collections.Generic;

using TraceWizard.Entities;
using TraceWizard.TwApp;

namespace TraceWizard.Commanding {

    public class CommandEngine {

        public class TwCommandResult {
            public string SuccessMessage = null;
            public string ErrorMessage = null;
        }

        AnalysisPanel analysisPanel;
        public CommandEngine(AnalysisPanel analysisPanel) {
            this.analysisPanel = analysisPanel;
        }

        public TwCommandResult Execute(TwCommand command) {
            TwCommandResult commandResult = null;
            switch (command.Action) {
                case TwCommandAction.SelectNext:
                    commandResult = DispatchActionFind(command, true);
                    break;
                case TwCommandAction.SelectPrevious:
                    commandResult = DispatchActionFind(command, false);
                    break;
                case TwCommandAction.SelectAll:
                    commandResult = DispatchActionSelectAll(command);
                    break;
                case TwCommandAction.ViewNext:
                    commandResult = DispatchActionView(command, true);
                    break;
                case TwCommandAction.ViewPrevious:
                    commandResult = DispatchActionView(command, false);
                    break;
                case TwCommandAction.ClassifyAs:
                    commandResult = DispatchActionClassifyAs(command, false);
                    break;
                case TwCommandAction.ClassifySimilarAs:
                    commandResult = DispatchActionClassifyAs(command, true);
                    break;
                case TwCommandAction.Time:
                    commandResult = DispatchActionTime(command);
                    break;
                case TwCommandAction.Undo:
                    commandResult = DispatchActionUndo(command);
                    break;
                case TwCommandAction.Redo:
                    commandResult = DispatchActionRedo(command);
                    break;
                case TwCommandAction.SetBookmark:
                    commandResult = DispatchActionSetBookmark(command);
                    break;
                case TwCommandAction.GoToBookmark:
                    commandResult = DispatchActionGoToBookmark(command);
                    break;
                case TwCommandAction.GoToStart:
                    commandResult = DispatchActionGoToStart(command);
                    break;
                case TwCommandAction.GoToEnd:
                    commandResult = DispatchActionGoToEnd(command);
                    break;
                case TwCommandAction.Refresh:
                    commandResult = DispatchActionRefresh(command);
                    break;
                case TwCommandAction.Approve:
                    commandResult = DispatchActionApprove(command);
                    break;
                case TwCommandAction.Unapprove:
                    commandResult = DispatchActionUnapprove(command);
                    break;
            }
            return commandResult;
        }

        bool IsSimilarConstraintExists(TwCommandConstraints constraints) {
            foreach (TwCommandConstraint constraint in constraints) {
                if (constraint.Attribute == TwCommandConstraintAttribute.Similar && constraint.Relation == TwCommandConstraintRelation.None)
                    return true;
            }
            return false;
        }

        [Obsolete]
        bool FixtureExists(TwCommandFixtures fixtures) {
            return (fixtures != null && fixtures.Count > 0);
        }

        void PackageConstraints(TwCommandConstraints constraints, Event @event) {
                foreach (var constraint in constraints) {
                    var constraintBoolEvent = constraint as TwCommandConstraintBoolEvent;
                    if (constraintBoolEvent != null)
                        constraintBoolEvent.EventSource = @event;

                    var constraintListItem = constraint as TwCommandConstraintListItem;
                    if (constraintListItem != null)
                        constraintListItem.FixtureProfiles = analysisPanel.Analysis.FixtureProfiles;

                }
        }
        
        TwCommandResult DispatchActionFind(TwCommand command, bool next) {
            analysisPanel.SelectedEventView = null;
            var commandResult = new TwCommandResult();

            if (analysisPanel.Analysis.Events.SelectedEvents.Count != 1 && IsSimilarConstraintExists(command.Constraints)) {
                commandResult.ErrorMessage = "First select one event.";
            } else {
                Event @event = null;

                if (IsSimilarConstraintExists(command.Constraints))
                    @event = analysisPanel.Analysis.Events.SelectedEvents[0];
                else if (analysisPanel.StyledEventsViewer.EventsViewer.SingleSelectedInView())
                    @event = analysisPanel.Analysis.Events.SelectedEvents[0];
                else if (next)
                    @event = analysisPanel.StyledEventsViewer.EventsViewer.GetNextFromPosition();
                else
                    @event = analysisPanel.StyledEventsViewer.EventsViewer.GetPreviousFromPosition();

                PackageConstraints(command.Constraints, @event);

                if (next)
                    analysisPanel.StyledEventsViewer.EventsViewer.FindNextAndSelect(@event, command);
                else
                    analysisPanel.StyledEventsViewer.EventsViewer.FindPreviousAndSelect(@event, command);
            }
            return commandResult;
        }

        TwCommandResult DispatchActionApprove(TwCommand command) {
            return DispatchActionApprove(command,true);
        }

        TwCommandResult DispatchActionUnapprove(TwCommand command) {
            return DispatchActionApprove(command, false);
        }

        TwCommandResult DispatchActionApprove(TwCommand command, bool approve) {
            var commandResult = new TwCommandResult();

            if (analysisPanel.Analysis.Events.SelectedEvents.Count == 0) {
                commandResult.ErrorMessage = "First select one or more events.";
            } else if (command.Fixtures.Count != 0) {
                commandResult.ErrorMessage = "Do not specify a fixture.";
            } else if (command.Constraints.Count > 0) {
                commandResult.ErrorMessage = "Do not specify constraints: " + command.Constraints[0].ToString();
            } else {
                analysisPanel.Analysis.Events.Approve(analysisPanel.Analysis.Events.SelectedEvents, analysisPanel.UndoPosition, approve);
            }

            return commandResult;
        }

        TwCommandResult DispatchActionClassifyAs(TwCommand command, bool withAdoption) {
            var commandResult = new TwCommandResult();
 
            if (analysisPanel.Analysis.Events.SelectedEvents.Count == 0) {
                commandResult.ErrorMessage = "First select one or more events.";
            } else if(command.Fixtures.Count == 0) {
                commandResult.ErrorMessage = "Specify a fixture.";
            } else if(command.Fixtures.Count > 1) {
                commandResult.ErrorMessage = "Specify only one fixture.";
            } else if(command.Constraints.Count > 0) {
                commandResult.ErrorMessage = "Do not specify constraints: " + command.Constraints[0].ToString();
            } else {
                analysisPanel.Analysis.Events.ManuallyClassify(analysisPanel.Analysis.Events.SelectedEvents,command.Fixtures[0],withAdoption, 
                    analysisPanel.UndoPosition);
            }

            return commandResult;
        }

        TwCommandResult DispatchActionUndo(TwCommand command) {
            var commandResult = new TwCommandResult();

            if (command.Fixtures.Count > 0) {
                commandResult.ErrorMessage = "Do not specify fixture.";
            } else if (command.Constraints.Count > 0) {
                commandResult.ErrorMessage = "Do not specify constraint.";
            } else if (!analysisPanel.Analysis.UndoManager.CanUndo()) {
                commandResult.ErrorMessage = "Cannot Undo.";
            } else {
                var position = analysisPanel.Analysis.UndoManager.Undo();
                analysisPanel.StyledEventsViewer.EventsViewer.RestoreScrollPosition(position);
            }
            return commandResult;
        }

        TwCommandResult DispatchActionRedo(TwCommand command) {
            var commandResult = new TwCommandResult();

            if (command.Fixtures.Count > 0) {
                commandResult.ErrorMessage = "Do not specify fixture.";
            } else if (command.Constraints.Count > 0) {
                commandResult.ErrorMessage = "Do not specify constraint.";
            } else if (!analysisPanel.Analysis.UndoManager.CanRedo()) {
                commandResult.ErrorMessage = "Cannot Redo.";
            } else {
                var position = analysisPanel.Analysis.UndoManager.Redo();
                analysisPanel.StyledEventsViewer.EventsViewer.RestoreScrollPosition(position);
            }
            return commandResult;
        }

        TwCommandResult DispatchActionSetBookmark(TwCommand command) {
            var commandResult = new TwCommandResult();

            if (command.Fixtures.Count > 0) {
                commandResult.ErrorMessage = "Do not specify fixture.";
            } else if (command.Constraints.Count > 0) {
                commandResult.ErrorMessage = "Do not specify constraint.";
            } else if (!analysisPanel.StyledEventsViewer.EventsViewer.BookmarkIsSet()) {
                commandResult.ErrorMessage = "Cannot Go To Bookmark.";
            } else {
                analysisPanel.StyledEventsViewer.EventsViewer.SetBookmark();
            }
            return commandResult;
        }

        TwCommandResult DispatchActionGoToBookmark(TwCommand command) {
            var commandResult = new TwCommandResult();

            if (command.Fixtures.Count > 0) {
                commandResult.ErrorMessage = "Do not specify fixture.";
            } else if (command.Constraints.Count > 0) {
                commandResult.ErrorMessage = "Do not specify constraint.";
            } else {
                analysisPanel.StyledEventsViewer.EventsViewer.GoToBookmark();
            }
            return commandResult;
        }

        TwCommandResult DispatchActionGoToStart(TwCommand command) {
            var commandResult = new TwCommandResult();

            if (command.Fixtures.Count > 0) {
                commandResult.ErrorMessage = "Do not specify fixture.";
            } else if (command.Constraints.Count > 0) {
                commandResult.ErrorMessage = "Do not specify constraint.";
            } else {
                analysisPanel.StyledEventsViewer.EventsViewer.GoToStart();
            }
            return commandResult;
        }

        TwCommandResult DispatchActionGoToEnd(TwCommand command) {
            var commandResult = new TwCommandResult();

            if (command.Fixtures.Count > 0) {
                commandResult.ErrorMessage = "Do not specify fixture.";
            } else if (command.Constraints.Count > 0) {
                commandResult.ErrorMessage = "Do not specify constraint.";
            } else {
                analysisPanel.StyledEventsViewer.EventsViewer.GoToEnd();
            }
            return commandResult;
        }

        TwCommandResult DispatchActionRefresh(TwCommand command) {
            var commandResult = new TwCommandResult();

            if (command.Fixtures.Count > 0) {
                commandResult.ErrorMessage = "Do not specify fixture.";
            } else if (command.Constraints.Count > 0) {
                commandResult.ErrorMessage = "Do not specify constraint.";
            } else {
                analysisPanel.RefreshExecuted(null,null);
            }
            return commandResult;
        }

        TwCommandResult DispatchActionTime(TwCommand command) {
            var commandResult = new TwCommandResult();

            if (command.Fixtures.Count > 0) {
                commandResult.ErrorMessage = "Do not specify fixture.";
            } else if (command.Constraints.Count != 1) {
                commandResult.ErrorMessage = "Specify exactly one constraint.";
            } else if (command.Constraints[0].Attribute != TwCommandConstraintAttribute.DateTime) {
                commandResult.ErrorMessage = "Constraint must be datetime.";
            } else {
                analysisPanel.StyledEventsViewer.EventsViewer.FindDateTime(((TwCommandConstraintDateTime)(command.Constraints[0])).Value);
            }
            return commandResult;
        }

        [Obsolete]
        bool ContainsAttribute(TwCommandConstraints constraints, TwCommandConstraintAttribute attribute) {
            foreach (var constraint in constraints)
                if (constraint.Attribute == attribute)
                    return true;
            return false;
        }
        
        TwCommandResult DispatchActionSelectAll(TwCommand command) {
            analysisPanel.SelectedEventView = null;
            var commandResult = new TwCommandResult();

            if (analysisPanel.Analysis.Events.SelectedEvents.Count != 1 && IsSimilarConstraintExists(command.Constraints)) {
                commandResult.ErrorMessage = "First select one event.";
            } else {
                Event @event = analysisPanel.Analysis.Events.SelectedEvents.Count > 0 ? analysisPanel.Analysis.Events.SelectedEvents[0] : null;
                foreach (var constraint in command.Constraints) {
                    var constraintBoolEvent = constraint as TwCommandConstraintBoolEvent;
                    if (constraintBoolEvent != null)
                        constraintBoolEvent.EventSource = @event;
                }

                analysisPanel.StyledEventsViewer.EventsViewer.Select(command);
            }
            return commandResult;
        }

        TwCommandResult DispatchActionView(TwCommand command, bool next) {
            var commandResult = new TwCommandResult();

            var constraint = new TwCommandConstraintBool();
            constraint.Attribute = TwCommandConstraintAttribute.Selected;
            constraint.Value = true;
            command.Constraints.Add(constraint);

            if (analysisPanel.Analysis.Events.SelectedEvents.Count == 0) {
                commandResult.ErrorMessage = "First select one or more events.";
            } else if (next) {
                analysisPanel.SelectedEventView = analysisPanel.StyledEventsViewer.EventsViewer.FindNext(command, analysisPanel.SelectedEventView);
            } else {
                analysisPanel.SelectedEventView = analysisPanel.StyledEventsViewer.EventsViewer.FindPrevious(command, analysisPanel.SelectedEventView);
            }

             return commandResult;
        }
    }
}