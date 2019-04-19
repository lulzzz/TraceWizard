using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;

using TraceWizard.Classification;
using TraceWizard.Adoption;
using TraceWizard.Logging;
using TraceWizard.Commanding;

using TraceWizard.Notification;

namespace TraceWizard.Entities {

    public class Events : List<Event>, INotifyPropertyChanged {

        public Events() { 
            SelectedEvents = new List<Event>();
            ConversionFactor = 1.0;
        }

        public UndoManager UndoManager { get; set; }
        
        public double ConversionFactor { get;set; }
        
        public TimeFrame TimeFrame { get; set; }
        public double Volume { get; private set; }
        public double InitialVolume { get; private set; }
        public double Peak { get; private set; }
        public TimeSpan Duration { get { return TimeFrame.Duration; } }
        public DateTime StartTime { get { return TimeFrame.StartTime; } }
        public DateTime EndTime { get { return TimeFrame.EndTime; } }

        public double SuperPeak { get; private set; }

        public double MinVolume { get; private set; }
        public double LowerLimitVolume { get; private set; }
        public double MedianVolume { get; private set; }
        public double UpperLimitVolume { get; private set; }
        public double MaxVolume { get; private set; }

        public double MinPeak { get; private set; }
        public double LowerLimitPeak { get; private set; }
        public double MedianPeak { get; private set; }
        public double UpperLimitPeak { get; private set; }
        public double MaxPeak { get; private set; }

        public TimeSpan MinDuration { get; private set; }
        public TimeSpan LowerLimitDuration { get; private set; }
        public TimeSpan MedianDuration { get; private set; }
        public TimeSpan UpperLimitDuration { get; private set; }
        public TimeSpan MaxDuration { get; private set; }

        public double MinMode { get; private set; }
        public double LowerLimitMode { get; private set; }
        public double MedianMode { get; private set; }
        public double UpperLimitMode { get; private set; }
        public double MaxMode { get; private set; }

        public List<Event> SelectedEvents { get; set; }
        public Event CurrentEvent { get; set; }

        public bool IsBackupDirty { get; set; }

        bool isDirty { get; set; }
        public bool IsDirty { 
            get { return isDirty; }
            set {
                IsBackupDirty = value;
                if (isDirty != value) {
                    isDirty = value;
                    OnPropertyChanged(TwNotificationProperty.OnIsDirtyChanged);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public new void Add(Event item) {
            Add(item, true);
        }

        public void Add(Event item, bool updateVolume) {
            base.Add(item);
            Update(item, updateVolume);
            OnPropertyChanged(TwNotificationProperty.OnAddEvent);
            IsDirty = true;
        }

        public new void Remove(Event item) {
            if (SelectedEvents.Contains(item))
                SelectedEvents.Remove(item);
            base.Remove(item);
            item.OnPropertyChanged(TwNotificationProperty.OnRemoveEvent);
            IsDirty = true;
        }

        public new void Insert(int index, Event item) {
            throw new NotImplementedException("Events: Insert Event: not implemented");
        }

        public void ManuallyToggleFirstCycle(Event @event, UndoPosition position) {
            if (!@event.FixtureClass.CanHaveCycles)
                return;

            OnPropertyChanged(TwNotificationProperty.OnStartClassify);

            var taskMultiple = new UndoTaskClassifyFirstCycleMultiple(position);
            var task = UndoTaskClassifyFirstCycle.CreateUndoTask(@event, @event.FirstCycle, !@event.FirstCycle, @event.ManuallyClassifiedFirstCycle, !@event.FirstCycle);
            taskMultiple.UndoTasks.Add(task);
            UndoManager.Add(taskMultiple);

            @event.FirstCycle = !@event.FirstCycle;
            @event.ManuallyClassifiedFirstCycle = true;
            OnPropertyChanged(TwNotificationProperty.OnPreviewEndClassify);
            OnPropertyChanged(TwNotificationProperty.OnEndClassify);
            IsDirty = true;
        }

        Event FirstCanHaveCycles(IEnumerable<Event> events) {
            foreach (var @event in events)
                if (@event.FixtureClass.CanHaveCycles)
                    return @event;
            return null;
        }

        public void ManuallyToggleFirstCycle(List<Event> events, bool clear, bool? currentValue, UndoPosition position) {

            if (events.Count == 0)
                return;

            OnPropertyChanged(TwNotificationProperty.OnStartClassify);

            IEnumerable<Event> sorted = Enumerable.OrderBy(events, n => n.StartTime);
            var firstEvent = FirstCanHaveCycles(sorted);
            if (firstEvent == null)
                return;

            bool firstCycleFirstEvent = FirstCanHaveCycles(sorted).FirstCycle;

            bool valueToSet = false;
            if (currentValue.HasValue) 
                valueToSet = !currentValue.Value;
            else
                valueToSet = !firstCycleFirstEvent;

            var taskMultiple = new UndoTaskClassifyFirstCycleMultiple(position);
            foreach (Event @event in sorted) {
                if (@event.FixtureClass.CanHaveCycles) {

                    var task = UndoTaskClassifyFirstCycle.CreateUndoTask(@event, @event.FirstCycle, valueToSet, @event.ManuallyClassifiedFirstCycle, valueToSet);
                    taskMultiple.UndoTasks.Add(task);

                    @event.FirstCycle = valueToSet;
                    @event.ManuallyClassifiedFirstCycle = true;
                }
            }

            if (taskMultiple.UndoTasks.Count > 0)
                UndoManager.Add(taskMultiple);

            OnPropertyChanged(TwNotificationProperty.OnPreviewEndClassify);
            OnPropertyChanged(TwNotificationProperty.OnEndClassify);
            IsDirty = true;

            if (clear)
                ClearSelectedEventsLow(null);

        }

        public void ApproveAllPrevious(UndoPosition position) {
            var startTime = this.StartTime;
            var endTime = StartTimeFromPosition(position);
            Approve(startTime, endTime, position);
        }

        public void ApproveInView(double percentInView, UndoPosition position) {
            var startTime = StartTimeFromPosition(position);
            var endTime = EndTimeFromPosition(position, percentInView);
            Approve(startTime, endTime, position);
        }

        void Approve(DateTime startTime, DateTime endTime, UndoPosition position) {
            OnPropertyChanged(TwNotificationProperty.OnStartClassify);

            var taskMultiple = new UndoTaskApproveMultiple(position);
            foreach (Event @event in this) {

                if (@event.StartTime >= startTime && @event.StartTime <= endTime) {
                    var task = UndoTaskApprove.CreateUndoTask(@event, @event.ManuallyApproved, true);
                    taskMultiple.UndoTasks.Add(task);

                    @event.ManuallyApproved = true;
                }
            }

            if (taskMultiple.UndoTasks.Count > 0)
                UndoManager.Add(taskMultiple);

            OnPropertyChanged(TwNotificationProperty.OnPreviewEndClassify);
            OnPropertyChanged(TwNotificationProperty.OnEndClassify);
            IsDirty = true;

        }

        public void Approve(List<Event> events, UndoPosition position, bool approve) {

            if (events.Count == 0)
                return;

            OnPropertyChanged(TwNotificationProperty.OnStartClassify);

            var taskMultiple = new UndoTaskApproveMultiple(position);
            foreach (Event @event in events) {

                var task = UndoTaskApprove.CreateUndoTask(@event, @event.ManuallyApproved, approve);
                taskMultiple.UndoTasks.Add(task);

                @event.ManuallyApproved = approve;
            }

            if (taskMultiple.UndoTasks.Count > 0)
                UndoManager.Add(taskMultiple);

            OnPropertyChanged(TwNotificationProperty.OnPreviewEndClassify);
            OnPropertyChanged(TwNotificationProperty.OnEndClassify);
            IsDirty = true;

        }

        public static DateTime GetMinStartDateTime(Event event1, Event event2) {
            return event1.StartTime < event2.StartTime? event1.StartTime : event2.StartTime;
        }

        public static DateTime GetMaxEndDateTime(Event event1, Event event2) {
            return event1.EndTime > event2.EndTime ? event1.EndTime : event2.EndTime;
        }

        public void SelectAll() {
            if (Count > 1)
                SelectRange(this.StartTime, this.EndTime);
        }

        public void SelectRange(Event event1, Event event2) {
            DateTime startTime = GetMinStartDateTime(event1,event2);
            DateTime endTime = GetMaxEndDateTime(event1, event2);

            SelectRange(startTime, endTime);
        }

        public void SelectRange(DateTime startTime, DateTime endTime) {

            foreach (Event @event in this) {
                if (@event.StartTime >= startTime && @event.EndTime <= endTime && !@event.Selected) {
                    @event.Selected = true;
                    SelectedEvents.Add(@event);
                }
            }
            OnPropertyChanged(TwNotificationProperty.OnPreviewEndSelect);
            OnPropertyChanged(TwNotificationProperty.OnEndSelect);
        }

        public void Select(List<Event> events) {
            foreach (Event @event in events) {
                @event.Selected = true;
                SelectedEvents.Add(@event);
            }
            OnPropertyChanged(TwNotificationProperty.OnPreviewEndSelect);
            OnPropertyChanged(TwNotificationProperty.OnEndSelect);
        }

        public void ToggleSelected(Event @event) {
            @event.Selected = !@event.Selected;
            if (@event.Selected)
                SelectedEvents.Add(@event);
            else
                SelectedEvents.Remove(@event);

            OnPropertyChanged(TwNotificationProperty.OnPreviewEndSelect);
            OnPropertyChanged(TwNotificationProperty.OnEndSelect);
        }

        public void MachineClassifyFirstCycles(bool overrideFirstCycleManuallyClassified, int minutesThreshold) {
            OnPropertyChanged(TwNotificationProperty.OnStartClassify);
            MachineClassifyFirstCyclesLow(overrideFirstCycleManuallyClassified, minutesThreshold);
            OnPropertyChanged(TwNotificationProperty.OnPreviewEndClassify);
            OnPropertyChanged(TwNotificationProperty.OnEndClassify);
        }

        void MachineClassifyFirstCyclesLow(bool overrideFirstCycleManuallyClassified, int minutesThreshold) {
            foreach (Event @event in this) {
                if (overrideFirstCycleManuallyClassified || !@event.ManuallyClassifiedFirstCycle) {
                    if (@event.FixtureClass.CanHaveCycles) {
                        int countWithinThreshold = InRecentPastWithSameFixtureClass(@event, minutesThreshold).Count;
                        if (countWithinThreshold == 0 && !@event.FirstCycle) {
                            @event.FirstCycle = true;
                            @event.ManuallyClassifiedFirstCycle = false;
                        } else if (countWithinThreshold > 0 && @event.FirstCycle) {
                            @event.FirstCycle = false;
                            @event.ManuallyClassifiedFirstCycle = false;
                        }
                    }
                }
            }
        }

        void ClearTimeFrame() {
            TimeFrame = new TimeFrame();
        }

        public void ApplyConversionFactor(double oldConversionFactor, double newConversionFactor) {
            OnPropertyChanged(TwNotificationProperty.OnStartApplyConversionFactor);

            ConversionFactor = newConversionFactor;

            foreach (Event @event in this) {
                @event.ApplyConversionFactor(oldConversionFactor, newConversionFactor);
            }

            Volume *= newConversionFactor / oldConversionFactor;
            InitialVolume *= newConversionFactor / oldConversionFactor;
            Peak *= newConversionFactor / oldConversionFactor;
            SuperPeak *= newConversionFactor / oldConversionFactor;

            MinVolume *= newConversionFactor / oldConversionFactor;
            LowerLimitVolume *= newConversionFactor / oldConversionFactor;
            MedianVolume *= newConversionFactor / oldConversionFactor;
            UpperLimitVolume *= newConversionFactor / oldConversionFactor;
            MaxVolume *= newConversionFactor / oldConversionFactor;

            MinPeak *= newConversionFactor / oldConversionFactor;
            LowerLimitPeak *= newConversionFactor / oldConversionFactor;
            MedianPeak *= newConversionFactor / oldConversionFactor;
            UpperLimitPeak *= newConversionFactor / oldConversionFactor;
            MaxPeak *= newConversionFactor / oldConversionFactor;

            MinMode *= newConversionFactor / oldConversionFactor;
            LowerLimitMode *= newConversionFactor / oldConversionFactor;
            MedianMode *= newConversionFactor / oldConversionFactor;
            UpperLimitMode *= newConversionFactor / oldConversionFactor;
            MaxMode *= newConversionFactor / oldConversionFactor;
                
            IsDirty = true;
            OnPropertyChanged(TwNotificationProperty.OnEndApplyConversionFactor);
        }

        public void ChangeStartTime(DateTime newStartTime) {
            DateTime oldStartTime = StartTime;
            var timeSpanOffset = newStartTime.Subtract(oldStartTime);
            ClearTimeFrame();
            foreach (Event @event in this) {
                @event.ClearTimeFrame();
                foreach (Flow flow in @event) {
                    flow.TimeFrame = new TimeFrame(flow.StartTime.Add(timeSpanOffset),flow.Duration);
                    @event.UpdateTimeFrame(flow);
                }
                UpdateTimeFrame(@event);
            }
            IsDirty = true;
            OnPropertyChanged(TwNotificationProperty.OnChangeStartTime);
        }

        public List<Event> GetEventsForward(UndoPosition position) {
            var events = new List<Event>();
            var startTime = StartTimeFromPosition(position);
            foreach (Event @event in this) {
                if (@event.StartTime >= startTime)
                    events.Add(@event);
            }
            return events;
        }
        
        public void MachineClassify(List<Event> events, Classifier classifier, bool overrideManuallyClassified, UndoPosition position) {
            OnPropertyChanged(TwNotificationProperty.OnStartClassify);
            MachineClassifyLow(events, classifier, overrideManuallyClassified, position);
            OnPropertyChanged(TwNotificationProperty.OnPreviewEndClassify);
            OnPropertyChanged(TwNotificationProperty.OnEndClassify);
        }

        void MachineClassifyLow(List<Event> events, Classifier classifier, bool overrideManuallyClassified, UndoPosition position) {
            var taskMultiple = new UndoTaskClassifyMultiple(position);

            classifier.PreClassify();
            foreach (Event @event in events) {
                if (overrideManuallyClassified || (!@event.ManuallyClassified && !@event.ManuallyApproved) ) {

                    var fixtureClassOld = @event.FixtureClass;
                    var manuallyClassifiedOld = @event.ManuallyClassified;
                    var classifiedUsingFixtureListOld = @event.ClassifiedUsingFixtureList;
                    @event.FixtureClass = classifier.Classify(@event);
                    if (fixtureClassOld != @event.FixtureClass) {
                        @event.ManuallyClassified = false;
                    }

                    var task = UndoTaskClassify.CreateUndoTask(@event, fixtureClassOld, @event.FixtureClass, manuallyClassifiedOld, @event.ManuallyClassified, classifiedUsingFixtureListOld, @event.ClassifiedUsingFixtureList);
                    taskMultiple.UndoTasks.Add(task);
                }
            }

            if (taskMultiple.UndoTasks.Count > 0)
                UndoManager.Add(taskMultiple);

        }

        public void ToggleManuallyClassified(List<Event> events) {
            OnPropertyChanged(TwNotificationProperty.OnStartClassify);

            foreach (Event @event in events)
                @event.ManuallyClassified = !@event.ManuallyClassified;

            IsDirty = true;

            OnPropertyChanged(TwNotificationProperty.OnPreviewEndClassify);
            OnPropertyChanged(TwNotificationProperty.OnEndClassify);
        }

        public void ManuallyClassify(List<Event> events, FixtureClass fixtureClass, bool adopt, UndoPosition position) {
            ManuallyClassify(events, fixtureClass, false, adopt, position);
        }

        public void ManuallyClassify(List<Event> events, FixtureClass fixtureClass, bool clear, bool adopt, UndoPosition position) {
            OnPropertyChanged(TwNotificationProperty.OnStartClassify);

            IEnumerable<Event> sorted = Enumerable.OrderBy(events, n => n.StartTime);

            var taskMultiple = new UndoTaskClassifyMultiple(position);
            foreach (Event @event in sorted) {
                var classifiedUsingFixtureListOld = @event.ClassifiedUsingFixtureList;
                var fixtureClassOld = @event.FixtureClass;
                var manuallyClassifiedOld = @event.ManuallyClassified;

                ManuallyClassifyLow(@event, fixtureClass, adopt, taskMultiple.UndoTasks);

                var task = UndoTaskClassify.CreateUndoTask(@event, fixtureClassOld, @event.FixtureClass, manuallyClassifiedOld, true, classifiedUsingFixtureListOld, @event.ClassifiedUsingFixtureList);
                taskMultiple.UndoTasks.Add(task);
            }

            if (taskMultiple.UndoTasks.Count > 0)
                UndoManager.Add(taskMultiple);

            if (clear)
                ClearSelectedEventsLow(null);
            OnPropertyChanged(TwNotificationProperty.OnPreviewEndClassify);
            OnPropertyChanged(TwNotificationProperty.OnEndClassify);
        }

        public void ManuallyClassify(Event @event, FixtureClass fixtureClass, bool adopt) {
            ManuallyClassify(@event, fixtureClass, adopt, new UndoPosition());
        }

        public void ManuallyClassify(Event @event, FixtureClass fixtureClass, bool adopt, UndoPosition position) {
            OnPropertyChanged(TwNotificationProperty.OnStartClassify);

            var taskMultiple = new UndoTaskClassifyMultiple(position);

            var classifiedUsingFixtureListOld = @event.ClassifiedUsingFixtureList;
            var fixtureClassOld = @event.FixtureClass;
            var manuallyClassifiedOld = @event.ManuallyClassified;

            ManuallyClassifyLow(@event, fixtureClass, adopt, taskMultiple.UndoTasks);

            var task = UndoTaskClassify.CreateUndoTask(@event, fixtureClassOld, @event.FixtureClass, manuallyClassifiedOld, true, classifiedUsingFixtureListOld, @event.ClassifiedUsingFixtureList);
            taskMultiple.UndoTasks.Add(task);

            UndoManager.Add(taskMultiple);

            OnPropertyChanged(TwNotificationProperty.OnPreviewEndClassify);
            OnPropertyChanged(TwNotificationProperty.OnEndClassify);
        }

        void ManuallyClassifyLow(Event @event, FixtureClass fixtureClass, bool adopt, List<UndoTaskClassify> undoTasks) {

            @event.FixtureClass = fixtureClass;
            @event.ManuallyClassified = true;

            if (adopt)
                TwAdopters.Instance.GetDefaultAdopter(this).Adopt(@event, undoTasks);

            IsDirty = true;
        }

        public void ClearSelectedEvents(Event eventIgnore) {
            ClearSelectedEventsLow(eventIgnore);
            //if (ClearSelectedEventsLow(eventIgnore)) {
                OnPropertyChanged(TwNotificationProperty.OnPreviewEndSelect);
                OnPropertyChanged(TwNotificationProperty.OnEndSelect);
//            }
        }

        public bool ClearSelectedEventsLow(Event eventIgnore) {
            List<Event> toRemove = new List<Event>();
            foreach (Event @event in SelectedEvents) {
                if (@event != eventIgnore) {
                    @event.Selected = false;
                    toRemove.Add(@event);
                }
            }
            bool eventsRemoved = toRemove.Count > 0 ? true : false;
            foreach (Event @event in toRemove)
                SelectedEvents.Remove(@event);
            return eventsRemoved;
        }

        List<FixtureClass> Exclude(List<FixtureClass> fixtureClasses) {
            var classes = new List<FixtureClass>();

            foreach(FixtureClass fixtureClass in FixtureClasses.Items.Values)
                if (!fixtureClasses.Contains(fixtureClass))
                    classes.Add(fixtureClass);
            return classes;
        }
        
        public Event FindLast(TwCommand command) {
            List<FixtureClass> classes = BuildFixtureClasses(command.Fixtures);
            if (Count > 0) {
                Event eventTarget = this[Count - 1];
                while (eventTarget != null && !SatisfiesConstraints(eventTarget, classes, command.Constraints))
                    eventTarget = eventTarget.Previous;
                return eventTarget;
            } else
                return null;
        }

        bool SatisfiesConstraints(TwCommandConstraints constraints, Event eventTarget) {
            foreach (TwCommandConstraint constraint in constraints) {
                if (!SatisfiesConstraint(eventTarget, constraint))
                    return false;
            }
            return true;
        }

        bool SatisfiesConstraint(Event eventTarget, TwCommandConstraint constraint) {
            switch(constraint.Attribute) {
                case TwCommandConstraintAttribute.Volume:
                    return SatisfiesConstraint(eventTarget.Volume, constraint as TwCommandConstraintDouble);
                case TwCommandConstraintAttribute.Peak:
                    return SatisfiesConstraint(eventTarget.Peak, constraint as TwCommandConstraintDouble);
                case TwCommandConstraintAttribute.Mode:
                    return SatisfiesConstraint(eventTarget.Mode, constraint as TwCommandConstraintDouble);
                case TwCommandConstraintAttribute.Duration:
                    return SatisfiesConstraint(eventTarget.Duration, constraint as TwCommandConstraintTimeSpan);
                case TwCommandConstraintAttribute.FirstCycle:
                    return SatisfiesConstraint(eventTarget.FirstCycle, constraint as TwCommandConstraintBool);
                case TwCommandConstraintAttribute.ManuallyClassified:
                    return SatisfiesConstraint(eventTarget.ManuallyClassified, constraint as TwCommandConstraintBool);
                case TwCommandConstraintAttribute.ClassifiedUsingFixtureList:
                    return SatisfiesConstraint(eventTarget.ClassifiedUsingFixtureList, constraint as TwCommandConstraintBool);
                case TwCommandConstraintAttribute.ClassifiedUsingMachineLearning:
                    return SatisfiesConstraint(!eventTarget.ManuallyClassified && !eventTarget.ClassifiedUsingFixtureList, constraint as TwCommandConstraintBool);
                case TwCommandConstraintAttribute.Approved:
                    return SatisfiesConstraint(eventTarget.ManuallyApproved, constraint as TwCommandConstraintBool);
                case TwCommandConstraintAttribute.Super:
                    return SatisfiesConstraint(eventTarget.Channel == Channel.Super, constraint as TwCommandConstraintBool);
                case TwCommandConstraintAttribute.Notes:
                    return SatisfiesConstraint(eventTarget.HasNotes, constraint as TwCommandConstraintBool);
                case TwCommandConstraintAttribute.Similar:
                    if (constraint.Relation == TwCommandConstraintRelation.None)
                        return SatisfiesConstraint(eventTarget, constraint as TwCommandConstraintBoolEvent);
                    else
                        return SatisfiesConstraint(eventTarget.SimilarCount, constraint as TwCommandConstraintInteger);
                case TwCommandConstraintAttribute.Selected:
                    return SatisfiesConstraint(eventTarget.Selected, constraint as TwCommandConstraintBool);
                case TwCommandConstraintAttribute.ListItem:
                    return SatisfiesConstraint(eventTarget, constraint as TwCommandConstraintListItem);
                default:
                    return false;
            }
        }

        bool SatisfiesConstraint(double value, TwCommandConstraintDouble constraint) {
            switch(constraint.Relation) {
                case(TwCommandConstraintRelation.LessThan):
                    return value < constraint.Value;
                case (TwCommandConstraintRelation.GreaterThan):
                    return value > constraint.Value;
                case (TwCommandConstraintRelation.Equal):
                    return value == constraint.Value;
                case (TwCommandConstraintRelation.None):
                default:
                    return false;
            }
        }

        bool SatisfiesConstraint(double value, TwCommandConstraintInteger constraint) {
            switch (constraint.Relation) {
                case (TwCommandConstraintRelation.LessThan):
                    return value < constraint.Value;
                case (TwCommandConstraintRelation.GreaterThan):
                    return value > constraint.Value;
                case (TwCommandConstraintRelation.Equal):
                    return value == constraint.Value;
                case (TwCommandConstraintRelation.None):
                default:
                    return false;
            }
        }

        bool SatisfiesConstraint(TimeSpan value, TwCommandConstraintTimeSpan constraint) {
            switch (constraint.Relation) {
                case (TwCommandConstraintRelation.LessThan):
                    return value < constraint.Value;
                case (TwCommandConstraintRelation.GreaterThan):
                    return value > constraint.Value;
                case (TwCommandConstraintRelation.Equal):
                    return value == constraint.Value;
                case (TwCommandConstraintRelation.None):
                default:
                    return false;
            }
        }

        FixtureProfile GetFixtureProfile(TwCommandConstraintListItem constraint) {
            int i = 0;
            foreach (FixtureProfile fixtureProfile in constraint.FixtureProfiles) {
                if (fixtureProfile.FixtureClass == constraint.ListItem)
                    if (++i == constraint.ListItemIndex)
                        return fixtureProfile;
            }
            return null;
        }
        
        bool SatisfiesConstraint(Event eventTarget, TwCommandConstraintListItem constraint) {
            FixtureProfile fixtureProfile = GetFixtureProfile(constraint);
            if (fixtureProfile != null && Event.MatchesFixture(eventTarget, fixtureProfile))
                return true;
            else
                return false;

        }

        bool SatisfiesConstraint(Event eventTarget, TwCommandConstraintBoolEvent constraint) {
            return constraint.Value ? eventTarget.IsSimilarDefault(constraint.EventSource) : !constraint.EventSource.IsSimilarDefault(eventTarget);
        }

        bool SatisfiesConstraint(bool value, TwCommandConstraintBool constraint) {
            return value == constraint.Value;
        }

        public Event FindFirst(TwCommand command) {
            List<FixtureClass> classes = BuildFixtureClasses(command.Fixtures);
            if (Count > 0) {
                Event eventTarget = this[0];
                while (eventTarget != null && !SatisfiesConstraints(eventTarget, classes, command.Constraints))
                    eventTarget = eventTarget.Next;
                return eventTarget;
            } else
                return null;
        }

        public Event GetLast() {
            if (Count > 0)
                return this[Count - 1];
            else
                return null;
        }

        public Event GetFirst() {
            if (Count > 0)
                return this[0];
            else
                return null;
        }

        public Event GetNext(Event eventSource) {
            return eventSource == null ? this[0] : eventSource.Next;
        }
        
        public Event GetPrevious(Event eventSource) {
            return eventSource == null ? this[this.Count - 1] : eventSource.Previous;
        }

        DateTime StartTimeFromPosition(UndoPosition undoPosition) {
            return this.StartTime.AddTicks((long)(((double)(this.Duration.Ticks)) * undoPosition.PercentElapsedHorizontal));
        }

        DateTime EndTimeFromPosition(UndoPosition undoPosition, double percentCanvasWidth) {
            return this.StartTime.AddTicks((long)(((double)(this.Duration.Ticks)) * (undoPosition.PercentElapsedHorizontal + percentCanvasWidth)));
        }

        public Event FindNext(UndoPosition undoPosition) {
            var startTime = StartTimeFromPosition(undoPosition);
            return FindNext(startTime);
        }

        public Event FindPrevious(UndoPosition undoPosition, double percentCanvasWidth) {
            var endTime = EndTimeFromPosition(undoPosition, percentCanvasWidth);
            return FindPrevious(endTime);
        }

        public Event FindPrevious(DateTime endTime) {
            var @event = this[this.Count - 1];
            while (@event != null && (@event.StartTime >= endTime))
                @event = @event.Previous;
            return @event;
        }

        public Event FindNext(DateTime startTime) {
            var @event = this[0];
            while (@event != null && (@event.StartTime < startTime))
                @event = @event.Next;
            return @event;
        }

        bool IsNotNull(Event @event) { return @event != null; }
        public Event FindNextFirstCycle(Event eventSource, FixtureClass fixtureClass) {
            Event eventTarget = eventSource.Next;

            while (eventTarget != null && (eventTarget.FixtureClass != fixtureClass || !eventTarget.FirstCycle)) {
                eventTarget = eventTarget.Next;
            }

            return eventTarget;
        }

        public Event FindPreviousFirstCycle(Event eventSource, FixtureClass fixtureClass) {
            Event eventTarget = eventSource.Previous;

            while (eventTarget != null && (eventTarget.FixtureClass != fixtureClass || !eventTarget.FirstCycle)) {
                eventTarget = eventTarget.Previous;
            }

            return eventTarget;
        }

        bool SatisfiesConstraints(Event eventTarget,  List<FixtureClass> fixtureClasses, TwCommandConstraints constraints) {
                if (fixtureClasses.Contains(eventTarget.FixtureClass))
                    if (SatisfiesConstraints(constraints, eventTarget))
                        return true;
            return false;
        }

        List<FixtureClass> BuildFixtureClasses(TwCommandFixtures fixtureClasses) {
            var list = new List<FixtureClass>();
            if (fixtureClasses.Count == 0) {
                foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values)
                    list.Add(fixtureClass);
            } else if (fixtureClasses.Exclude) {
                foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values)
                    if (!fixtureClasses.Contains(fixtureClass))
                        list.Add(fixtureClass);
            } else {
                list = fixtureClasses;
            }
            return list;
        }

        public List<Event> Select(TwCommand command) {
            var events = new List<Event>();
            List<FixtureClass> classes = BuildFixtureClasses(command.Fixtures);

            Event eventTarget = null;
            if (Count > 0)
                eventTarget = this[0];

            while (eventTarget != null) {
                if (SatisfiesConstraints(eventTarget, classes, command.Constraints))
                    events.Add(eventTarget);
                eventTarget = eventTarget.Next;
            }

            return events;
        }

        public Event FindPrevious(Event eventSource, TwCommand command, bool cycle) {
            Event eventTarget = eventSource == null ? this[this.Count - 1] : eventSource.Previous;
            List<FixtureClass> classes = BuildFixtureClasses(command.Fixtures);

            while (eventTarget != null && !SatisfiesConstraints(eventTarget, classes, command.Constraints))
                eventTarget = eventTarget.Previous;

            if (cycle) {
                if (eventTarget == null) {
                    eventTarget = GetLast();
                    while (eventTarget != null
                    && !SatisfiesConstraints(eventTarget, classes, command.Constraints))
                        eventTarget = eventTarget.Previous;
                }
                if (eventTarget == eventSource)
                    eventTarget = null;
            }

            return eventTarget;
        }

        public Event FindNext(Event eventSource, TwCommand command, bool cycle) {
            Event eventTarget = eventSource == null ? this[0] : eventSource.Next;
            List<FixtureClass> classes = BuildFixtureClasses(command.Fixtures);

            while (eventTarget != null && !SatisfiesConstraints(eventTarget, classes, command.Constraints))
                eventTarget = eventTarget.Next;

            if (cycle) {
                if (eventTarget == null) {
                    eventTarget = GetFirst();
                    while (eventTarget != null
                    && !SatisfiesConstraints(eventTarget, classes, command.Constraints))
                        eventTarget = eventTarget.Next;
                }
                if (eventTarget == eventSource)
                    eventTarget = null;
            }
 
            return eventTarget;
        }

        public Event FindNext(Event eventSource, FixtureClass fixtureClass) {
            Event eventTarget = eventSource == null ? this[0] : eventSource.Next;

            while (eventTarget != null && eventTarget.FixtureClass != fixtureClass) {
                eventTarget = eventTarget.Next;
            }

            return eventTarget;
        }

        public Event FindNext(DateTime startTime, FixtureClass fixtureClass) {
            var @event = this[0];
            while (@event != null && (@event.StartTime < startTime || @event.FixtureClass != fixtureClass))
                @event = @event.Next;

            return @event;
        }

        public Event FindPrevious(DateTime endTime, FixtureClass fixtureClass) {
            var @event = this[this.Count - 1];
            while (@event != null && (@event.StartTime >= endTime || @event.FixtureClass != fixtureClass))
                @event = @event.Previous;

            return @event;
        }

        public Event FindPrevious(Event eventSource, FixtureClass fixtureClass) {
            Event eventTarget = eventSource == null ? this[this.Count - 1] : eventSource.Previous;

            while (eventTarget != null && eventTarget.FixtureClass != fixtureClass) {
                eventTarget = eventTarget.Previous;
            }

            return eventTarget;
        }

        public Event FindFirst(FixtureClass fixtureClass) {
            Event eventTarget = this[0];

            while (eventTarget != null && eventTarget.FixtureClass != fixtureClass) {
                eventTarget = eventTarget.Next;
            }

            return eventTarget;
        }

        public Event FindLast(FixtureClass fixtureClass) {
            Event eventTarget = this[Count - 1];

            while (eventTarget != null && eventTarget.FixtureClass != fixtureClass) {
                eventTarget = eventTarget.Previous;
            }

            return eventTarget;
        }

        public Event FindLastSimilar(Event eventSource) {
            Event eventTarget = this[Count - 1];

            while (eventTarget != null && !eventTarget.IsSimilarDefault(eventSource)) {
                eventTarget = eventTarget.Previous;
            }

            return eventTarget;
        }

        public Event FindFirstSimilar(Event eventSource) {
            Event eventTarget = this[0];

            while (eventTarget != null && !eventTarget.IsSimilarDefault(eventSource)) {
                eventTarget = eventTarget.Next;
            }

            return eventTarget;
        }

        public Event FindNextSimilar(Event eventSource) {
            Event eventTarget = eventSource.Next;

            while (eventTarget != null && !eventTarget.IsSimilarDefault(eventSource)) {
                eventTarget = eventTarget.Next;
            }

            return eventTarget;
        }

        public Event FindPreviousSimilar(Event eventSource) {
            Event eventTarget = eventSource.Previous;

            while (eventTarget != null && !eventTarget.IsSimilarDefault(eventSource)) {
                eventTarget = eventTarget.Previous;
            }

            return eventTarget;
        }

        public Event FindNextNote(Event eventSource) {
            Event eventTarget = eventSource.Next;

            while (eventTarget != null && string.IsNullOrEmpty(eventTarget.UserNotes)) {
                eventTarget = eventTarget.Next;
            }

            return eventTarget;
        }

        public Event FindPreviousNote(Event eventSource) {
            Event eventTarget = eventSource.Previous;

            while (eventTarget != null && string.IsNullOrEmpty(eventTarget.UserNotes)) {
                eventTarget = eventTarget.Previous;
            }

            return eventTarget;
        }

        [Obsolete]
        List<Event> InNearFuture(Event @event, TimeSpan timeSpan) {
           DateTime window = @event.EndTime.Add(timeSpan);
           return this.FindAll(delegate(Event evt) {
               return @event.EndTime <= @evt.StartTime
              && evt.StartTime <= window;
           });
       }

        [Obsolete]
        List<Event> InNearPast(Event @event, TimeSpan timeSpan) {
            DateTime window = @event.StartTime.Subtract(timeSpan);
            return this.FindAll(delegate(Event evt) {
                return @evt.EndTime <= @event.StartTime
               && window <= evt.EndTime;
            });
        }

        public bool IsIsolated(Event @event, TimeSpan timeSpan) {
            Event eventPrevious = @event.Previous;
            Event eventNext = @event.Next;

            if (eventPrevious != null
                && @event.StartTime.Subtract(eventPrevious.EndTime) <= timeSpan)
                return false;
            else if (eventNext != null
                && eventNext.StartTime.Subtract(@event.EndTime) <= timeSpan)
                return false;
            else
                return true;
        }
        
        List<Event> InRecentPastWithSameFixtureClass(Event @event, int minutes) {
            return InRecentPast(@event, minutes, @event.FixtureClass);
       }

        public List<Event> InRecentPast(Event @event, int minutes, FixtureClass fixtureClass) {
            DateTime window = @event.StartTime.Subtract(new TimeSpan(0, minutes, 0));
            return this.FindAll(delegate(Event evt) {
                return evt.EndTime <= @event.StartTime
               && evt.EndTime >= window
               && evt.FixtureClass == fixtureClass;
            });
        }
        
       void Update(Event @event, bool updateVolume) {
            if (@event.Peak > Peak) 
                Peak = @event.Peak;
            if (updateVolume)
                Volume += @event.Volume;
            UpdateTimeFrame(@event);
            if (@event.Mode == 0)
                @event.UpdateMode();
        }

       public void UpdateSuperPeak(Log log) {
           if (log != null)
               SuperPeak = log.Peak;
           else
               UpdateSuperPeak();
       }

       public void UpdateSuperPeak() {

           SuperPeak = 0.0;
           foreach (Event @event in this) {
               if (@event.Channel != Channel.Super) {
                   @event.SuperPeak = @event.CalcSuperPeak();
                   if (@event.SuperPeak > SuperPeak)
                       SuperPeak = @event.SuperPeak;
               }
           }
       }
       
       public void UpdateVolume() {
           Volume = 0.0;
           foreach (Event @event in this)
               Volume += @event.Volume;
       }

       public void UpdateVolumeByFlow() {
           Volume = 0.0;
           foreach (Event @event in this) {
               foreach (Flow flow in @event)
                   Volume += flow.Volume;
           }
       }

        public void UpdateOriginalVolume() {
           InitialVolume = Volume;
       }

       public bool CheckVolume() {
           if (Volume != 0 && Math.Abs((Volume / InitialVolume) - 1.0) > 0.001)
               return false;
           else
               return true;
       }

       public void UpdateSimilarCounts() {
           foreach (Event @event in this)
               @event.UpdateSimilarCounts();
       }
        
        public void UpdateChannel() {
           foreach (Event @event in this)
               UpdateChannel(@event);
       }

        private void UpdateTimeFrame(Event @event) {
            if ((StartTime == default(DateTime)) || (EndTime == default(DateTime)))
                TimeFrame = new TimeFrame(@event.StartTime, @event.EndTime);
            else {
                if (@event.StartTime < StartTime) TimeFrame = new TimeFrame(@event.StartTime, EndTime);
                if (@event.EndTime > EndTime) TimeFrame = new TimeFrame(StartTime, @event.EndTime);
            }
        }

        void UpdateChannel(Event @event) {
            @event.BaseEvent = GetBase(@event);
            @event.SuperEvents = GetSuperEvents(@event);

            if (@event.BaseEvent != null) {
                @event.Baseline = @event.BaseEvent.GetRate(@event.StartTime);
            } else {
                @event.Baseline = 0.0;
            }
        }

        private Events GetSuperEvents(Event @event) {
            Events superEvents = null;
            if (@event.Channel != Channel.Base)
                return null;

            foreach (Event evt in this) {
                if (evt.StartTime >= @event.StartTime &&
                    evt.EndTime <= @event.EndTime &&
                    evt.Channel == Channel.Super) {
                    if (superEvents == null)
                        superEvents = new Events();
                    superEvents.Add(evt);
                }
            }
            return superEvents;
        }

        private Event GetBase(Event @event) {
            if (@event.Channel != Channel.Super)
                return null;

            foreach (Event evt in this) {
                if (evt.StartTime <= @event.StartTime &&
                    evt.EndTime >= @event.EndTime &&
                    evt.Channel == Channel.Base)
                    return evt;
            }
            return null;
        }

        public void UpdateMedians() {
            if (this.Count == 0)
                return;

            var listVolume = new List<double>();
            var listPeak = new List<double>();
            var listDuration = new List<TimeSpan>();
            var listMode = new List<double>();

            foreach (Event @event in this) {
                listVolume.Add(@event.Volume);
                listPeak.Add(@event.Peak);
                listDuration.Add(@event.Duration);
                listMode.Add(@event.Mode);
            }

            listVolume.Sort();
            listPeak.Sort();
            listDuration.Sort();
            listMode.Sort();

            double tolerance = 0.10;

            MinVolume = CalcLimit(listVolume, 0);
            LowerLimitVolume = CalcLimit(listVolume, tolerance);
            MedianVolume = CalcLimit(listVolume,0.50);
            UpperLimitVolume = CalcLimit(listVolume, 1-tolerance);
            MaxVolume = CalcLimit(listVolume, 1.0);

            MinPeak = CalcLimit(listPeak, 0);
            LowerLimitPeak = CalcLimit(listPeak, tolerance);
            MedianPeak = CalcLimit(listPeak,0.50);
            UpperLimitPeak = CalcLimit(listPeak, 1 - tolerance);
            MaxPeak = CalcLimit(listPeak, 1.0);

            MinDuration = CalcLimit(listDuration, 0);
            LowerLimitDuration = CalcLimit(listDuration, tolerance);
            MedianDuration = CalcLimit(listDuration,0.50);
            UpperLimitDuration = CalcLimit(listDuration, 1 - tolerance);
            MaxDuration = CalcLimit(listDuration, 1.0);

            MinMode = CalcLimit(listMode, 0);
            LowerLimitMode = CalcLimit(listMode, tolerance);
            MedianMode = CalcLimit(listMode,0.50);
            UpperLimitMode = CalcLimit(listMode, 1 - tolerance);
            MaxMode = CalcLimit(listMode, 1.0);

        }

        int CalcEquals(List<double> values, int limit) {
            int count = 0;
            foreach (double value in values)
                if (value == limit)
                    count++;
            return count;
        }

        double CalcLimit(List<double> values, double tolerance) {
            return values[(int)((values.Count - 1) * tolerance)];
        }

        TimeSpan CalcLimit(List<TimeSpan> values, double tolerance) {
            values.Sort();
            return values[(int)((values.Count - 1) * tolerance)];
        }

        public bool CanMerge(Event source, Event target) {
            return CanMergeHorizontally(source,target) || CanMergeVertically(source, target);
        }

        static public bool CanMergeHorizontally(Event source, Event target) {
             return CanMergeHorizontallyByTimeConstraint(source,target) && CanMergeHorizontallyByChannelConstraint(source,target);
        }

        static public bool CanMergeHorizontallyByTimeConstraint(Event source, Event target) {
            return source != null && target != null && 
                (source.EndTime == target.StartTime || source.StartTime == target.EndTime);
        }

        static public bool CanMergeHorizontallyByChannelConstraint(Event source, Event target) {
            return source != null && target != null && (
                (source.Channel == target.Channel && source.Channel == Channel.Base)
                || (source.Channel == target.Channel && source.Channel == Channel.Trickle)
                || (source.Channel == target.Channel && source.Channel == Channel.Super 
                    && source.BaseEvent == target.BaseEvent && source.BaseEvent != null)
                || (source.Channel == Channel.Trickle && target.Channel == Channel.Base)
                );
        }

        static public bool CanMergeVertically(Event source, Event target) {
            if (source != null && target != null 
                && source.Channel == Channel.Super && target.Channel == Channel.Base
                && source.BaseEvent == target)
                return true;
            else
                return false;
        }

        static bool HasAtLeastOneSuperEvent(Event source) {
            return (source != null && source.Channel == Channel.Base && source.SuperEvents != null && source.SuperEvents.Count > 0);
        }

        static public bool CanMergeAllVerticallyIntoBase(Event eventTarget, List<Event> events) {
            if (events == null || events.Count == 0)
                return false;
            else if (eventTarget.Channel != Channel.Base)
                return false;
            else {
                foreach (Event @event in events)
                    if (@event.Channel != Channel.Super || @event.BaseEvent != eventTarget)
                        return false;
            }
            return true;
        }

        public bool CanMergeVerticallyIntoBase(Event source) {
            return source != null && (source.Channel == Channel.Super && HasAtLeastOneSuperEvent(source.BaseEvent));
        }

        static public bool CanMergeVerticallyIntoThisBase(Event target) {
            return HasAtLeastOneSuperEvent(target);
        }

        public bool CanMergeAllHorizontally(Event source) {
            return CanMergePreviousHorizontally(source) || CanMergeNextHorizontally(source);
        }

        static public bool CanMergeNextHorizontally(Event source) {
//            return CanMergeHorizontally(GetNextContiguous(source, source.Channel), source);
            return CanMergeHorizontally(GetNextContiguous(source), source);
        }

        public bool CanMergePreviousHorizontally(Event source) {
            return CanMergeHorizontally(GetPreviousContiguous(source), source);
        }

        void InsertBefore(Event source, Event target) {
            int i = 0;
            foreach (Flow flow in source)
                target.Insert(i++, flow);
        }

        void InsertAfter(Event source, Event target) {
            foreach (Flow flow in source)
                target.Add(flow);
        }

        public int MergeAllHorizontallyWithNotification(Event target, Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent, UndoPosition position) {
            OnPropertyChanged(TwNotificationProperty.OnStartMergeSplit);
            int count = MergeAllHorizontallyLow(target, classifier, removePolygon, renderEvent, position);
            OnPropertyChanged(TwNotificationProperty.OnPreviewEndMergeSplit);
            OnPropertyChanged(TwNotificationProperty.OnEndMergeSplit);
            return count; }

        public int MergeAllHorizontallyLow(Event target, Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent, UndoPosition position) {
            int count = 0;
            var taskMultiple = new UndoTaskHorizontalMergeMultiple(position);
            while (this.MergePreviousHorizontally(target, classifier, removePolygon, renderEvent, position, taskMultiple)) { count++; }
            while (this.MergeNextHorizontally(target, classifier, removePolygon, renderEvent, position, taskMultiple)) { count++; }
            UndoManager.Add(taskMultiple);
            return count;
        }

        internal bool MergePreviousHorizontally(Event target, Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent, UndoPosition position, UndoTaskHorizontalMergeMultiple taskMultiple) {
            bool merged = false;
            Event source = null;
            source = this.GetPreviousContiguousSameChannel(target);

            if (source == null)
                source = this.GetPreviousContiguousTrickleForBase(target);
            if (source != null) {
                MergeHorizontally(source, target, true, classifier, position, taskMultiple);
                merged = true;

                removePolygon(source);
                removePolygon(target);
                renderEvent(target);
            }
            return merged;
        }

        public bool MergePreviousHorizontallyWithNotification(Event target, Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent, UndoPosition position) {
            OnPropertyChanged(TwNotificationProperty.OnStartMergeSplit);
            bool merged = MergePreviousHorizontally(target, classifier, removePolygon, renderEvent, position, null);
            OnPropertyChanged(TwNotificationProperty.OnPreviewEndMergeSplit);
            OnPropertyChanged(TwNotificationProperty.OnEndMergeSplit);
            return merged;
        }

        internal bool MergeNextHorizontally(Event target, Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent, UndoPosition position, UndoTaskHorizontalMergeMultiple taskMultiple) {
            bool merged = false;
            Event source = null;
            source = this.GetNextContiguousSameChannel(target);

            if (source == null)
                source = this.GetNextContiguousTrickleForBase(target);
            if (source != null) {
                MergeHorizontally(source, target, true, classifier, position, taskMultiple);
                merged = true;

                removePolygon(source);
                removePolygon(target);
                renderEvent(target);
            }
            return merged;
        }

        public bool MergeNextHorizontallyWithNotification(Event target, Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent, UndoPosition position) {
            OnPropertyChanged(TwNotificationProperty.OnStartMergeSplit);
            bool merged = MergeNextHorizontally(target, classifier, removePolygon, renderEvent, position, null);
            OnPropertyChanged(TwNotificationProperty.OnPreviewEndMergeSplit);
            OnPropertyChanged(TwNotificationProperty.OnEndMergeSplit);
            return merged;
        }

        public void MergeHorizontallyWithNotification(Event source, Event target, bool remove, 
            Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent, UndoPosition position) {
            OnPropertyChanged(TwNotificationProperty.OnStartMergeSplit);
            MergeHorizontallyLow(source, target, remove, classifier, removePolygon, renderEvent, position);
            OnPropertyChanged(TwNotificationProperty.OnPreviewEndMergeSplit);
            OnPropertyChanged(TwNotificationProperty.OnEndMergeSplit);
        }


        public void MergeHorizontallyLow(Event source, Event target, bool remove,
            Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent, UndoPosition position) {

            MergeHorizontally(source, target, remove, classifier, position, null);
            if (removePolygon != null && renderEvent != null) {
                removePolygon(source);
                removePolygon(target);
                renderEvent(target);
            }
        }

        internal void MergeHorizontally(Event source, Event target, bool remove, Classifier classifier, UndoPosition position, UndoTaskHorizontalMergeMultiple taskMultiple) {
            if (source.StartTime < target.StartTime) {
                InsertBefore(source, target);
            } else {
                InsertAfter(source, target);
            }

            MoveSuperEvents(source, target);
            
            target.Update();
            target.OnPropertyChanged(TwNotificationProperty.OnChangeStartTime);

            var fixtureClassTargetOld = target.FixtureClass;
            var classifiedUsingFixtureListOld = target.ClassifiedUsingFixtureList;
            if (!target.ManuallyClassified && classifier != null) {
                target.FixtureClass = classifier.Classify(target);
            }

            if (!position.IsNull) {
                bool addTaskMultiple = false;
                if (taskMultiple == null) {
                    taskMultiple = new UndoTaskHorizontalMergeMultiple(position);
                    addTaskMultiple = true;
                }
                var task = UndoTaskHorizontalMerge.CreateUndoTask(target, source, fixtureClassTargetOld, source.FixtureClass, classifiedUsingFixtureListOld, source.ClassifiedUsingFixtureList);
                taskMultiple.UndoTasks.Add(task);

                if (addTaskMultiple)
                    UndoManager.Add(taskMultiple);
            }

            if (remove)
                Remove(source);
        }

        void MoveSuperEvents(Event source, Event target) {
            MoveSuperEvents(source, target, source.StartTime, source.EndTime);
        }

        void MoveSuperEvents(Event source, Event target, DateTime startTime, DateTime endTime) {
            if (source.HasSuperEvents()) {
                if (target.SuperEvents == null)
                    target.SuperEvents = new Events();
                var superEventsToMove = new List<Event>();
                foreach (Event super in source.SuperEvents) {
                    if (super.StartTime >= startTime && super.EndTime <= endTime ) {
                        super.BaseEvent = target;
                        target.SuperEvents.Add(super);
                        superEventsToMove.Add(super);
                    }
                }
                foreach (Event superEventToMove in superEventsToMove) {
                    source.SuperEvents.Remove(superEventToMove);
                }
            }
        }

        public void MergeVertically(Event source, Event target, bool remove, Classifier classifier) {

            foreach (Flow flowTarget in target) {
                Flow flowSource = source.Find(delegate(Flow flow) { return flow.StartTime == flowTarget.StartTime; });
                if (flowSource != null) {
                    flowTarget.Rate += flowSource.Rate;
                }
            }
            target.Update();

            if (!target.ManuallyClassified && classifier != null)
                target.FixtureClass = classifier.Classify(target);

            if (remove) {
                target.SuperEvents.Remove(source);
                Remove(source);
            }
        }

        public Event GetPreviousContiguousSameChannel(Event @event) {
            foreach (Event evt in this) {
                if (evt.EndTime == @event.StartTime && evt.Channel == @event.Channel)
                    return evt;
            }
            return null;
        }

        public Event GetPreviousContiguousTrickleForBase(Event @event) {
            if (@event.Channel != Channel.Base)
                return null;
            foreach (Event evt in this) {
                if (evt.EndTime == @event.StartTime && evt.Channel == Channel.Trickle)
                    return evt;
            }
            return null;
        }

        public Event GetPreviousContiguous(Event @event, Channel channel) {
            foreach (Event evt in this) {
                if (evt.EndTime == @event.StartTime && evt.Channel == channel)
                    return evt;
            }
            return null;
        }

        static public Event GetPreviousContiguous(Event @event) {
            if (@event.Previous != null && @event.Previous.EndTime == @event.StartTime)
                return @event.Previous;
            return null;
        }

        public Event GetNextContiguousSameChannel(Event @event) {
            foreach (Event evt in this) {
                if (evt.StartTime == @event.EndTime && evt.Channel == @event.Channel)
                    return evt;
            }
            return null;
        }

        public Event GetNextContiguousTrickleForBase(Event @event) {
            if (@event.Channel != Channel.Base)
                return null;
            foreach (Event evt in this) {
                if (evt.StartTime == @event.EndTime && evt.Channel == Channel.Trickle)
                    return evt;
            }
            return null;
        }

        static public Event GetNextContiguous(Event @event, Channel channel) {
            Event evt = @event;
            while (evt != null && (evt.StartTime <= @event.EndTime) ) {
                if (evt.StartTime == @event.EndTime && evt.Channel == channel)
                    return evt;
                evt = evt.Next;
            }
            return null;
        }

        static public Event GetNextContiguous(Event @event) {
            if (@event.Next != null && @event.Next.StartTime == @event.EndTime)
                return @event.Next;
            return null;
        }

        public delegate void RemovePolygon(Event @event);
        public delegate void RenderEvent(Event @event);

        public void MergeVerticallyLow(Event eventSource, Event eventTarget, bool remove,
            Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent, UndoPosition position) {

            var fixtureClassSourceOld = eventSource.FixtureClass;
            var fixtureClassTargetOld = eventTarget.FixtureClass;
            MergeVertically(eventSource, eventTarget, remove, classifier);

            if (!position.IsNull) {
                var taskMultiple = new UndoTaskVerticalMergeMultiple(position);
                var task = UndoTaskVerticalMerge.CreateUndoTask(eventTarget, eventSource, fixtureClassTargetOld, fixtureClassSourceOld);
                taskMultiple.UndoTasks.Add(task);
                UndoManager.Add(taskMultiple);
            }

            removePolygon(eventSource);
            removePolygon(eventTarget);
            renderEvent(eventTarget);
        }

        public void MergeVerticallyWithNotification(Event eventSource, Event eventTarget, bool remove, 
            Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent, UndoPosition position) {
            OnPropertyChanged(TwNotificationProperty.OnStartMergeSplit);

            MergeVerticallyLow(eventSource, eventTarget, remove, classifier, removePolygon, renderEvent, position);

            OnPropertyChanged(TwNotificationProperty.OnPreviewEndMergeSplit);
            OnPropertyChanged(TwNotificationProperty.OnEndMergeSplit);
        }

        public class Split {
            public Event Event;
            public DateTime DateTime;
            public Split(Event @event, DateTime dateTime) { Event = @event; DateTime = dateTime; }
        }
        
        public bool MidnightSplitLow(bool supers) {
            Split split = null;
            foreach (Event @event in this) {
                if ((supers && @event.Channel == Channel.Super)
                    || (!supers && @event.Channel != Channel.Super))
                    if (@event.StartTime.DayOfYear != @event.EndTime.DayOfYear)
                        if (@event.EndTime.DayOfYear - @event.StartTime.DayOfYear > 1 || @event.EndTime.Hour != 0) {
                            foreach (Flow flow in @event) {
                                if (flow.StartTime.DayOfYear != flow.EndTime.DayOfYear) {
                                    split = new Split(@event, flow.EndTime);
                                    break;
                                }
                            }
                        }
            }
            if (split != null) {
                SplitHorizontallyWithNotification(split.Event, split.DateTime, null, null, null, true);
                return true;
            } else {
                return false;
            }
        }

        public void MidnightSplit() {
            while (MidnightSplitLow(true));
            while (MidnightSplitLow(false)) ;
        }

        public int MergeAllPostTrickleHorizontally() {
            return MergeAllPostTrickleHorizontallyWithNotification(null, null, null);
        }
        
        public int MergeAllPostTrickleHorizontallyWithNotification(Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent) {

            OnPropertyChanged(TwNotificationProperty.OnStartMergeSplit);

            Events trickles = new Events();
            Events tricklesMerged = new Events();
            int count = 0;
            foreach (Event @event in this)
                if (@event.Channel == Channel.Trickle && @event.Duration.TotalSeconds <= 60) {
                    Event previous = this.GetPreviousContiguous(@event, Channel.Base);
                    if (previous != null) {
                        this.MergeHorizontallyLow(@event, previous, false, classifier, removePolygon, renderEvent, new UndoPosition(true));
                        tricklesMerged.Add(@event);
                        count++;
                    }
                }

            foreach (Event trickleMerged in tricklesMerged)
                this.Remove(trickleMerged);

            OnPropertyChanged(TwNotificationProperty.OnPreviewEndMergeSplit);
            OnPropertyChanged(TwNotificationProperty.OnEndMergeSplit);

            return count;
        }

        int MergeAllVerticallyIntoBase(Event eventTarget, DateTime startTime, DateTime endTime, Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent, UndoPosition position) {
            return MergeAllVerticallyIntoBase(eventTarget, eventTarget.SuperEvents, startTime, endTime, classifier, removePolygon, renderEvent, position);
        }

        int MergeAllVerticallyIntoBase(Event eventTarget, List<Event> events, DateTime startTime, DateTime endTime, Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent, UndoPosition position) {
            if (events == null || events.Count == 0)
                return 0;

            int count = 0;
            Events superEventsToDelete = new Events();

            var taskMultiple = new UndoTaskVerticalMergeMultiple(position);
            foreach (Event eventSuper in events) {
                if (eventSuper.StartTime >= startTime && eventSuper.EndTime <= endTime) {

                    var fixtureClassTargetOld = eventTarget.FixtureClass;
                    MergeVertically(eventSuper, eventTarget, false, classifier);

                    var task = UndoTaskVerticalMerge.CreateUndoTask(eventTarget, eventSuper, fixtureClassTargetOld, eventSuper.FixtureClass);
                    taskMultiple.UndoTasks.Add(task);

                    if (removePolygon != null) {
                        removePolygon(eventSuper);
                        removePolygon(eventTarget);
                    }
                    if (renderEvent != null)
                        renderEvent(eventTarget);
                    superEventsToDelete.Add(eventSuper);
                    count++;
                }
            }

            if (taskMultiple.UndoTasks.Count > 0)
                UndoManager.Add(taskMultiple);

            RemoveSuperEvents(eventTarget, superEventsToDelete);

            return count;
        }

        public int MergeAllVerticallyIntoBaseWithNotification(Event eventTarget, List<Event> events, Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent, UndoPosition position) {

            OnPropertyChanged(TwNotificationProperty.OnStartMergeSplit);
            int count = MergeAllVerticallyIntoBase(eventTarget, events, eventTarget.StartTime, eventTarget.EndTime, classifier, removePolygon, renderEvent, position);

            OnPropertyChanged(TwNotificationProperty.OnPreviewEndMergeSplit);
            OnPropertyChanged(TwNotificationProperty.OnEndMergeSplit);

            return count;
        }

        void RemoveSuperEvents(Event @event, Events eventsToDelete) {
            foreach (Event super in eventsToDelete) {
                @event.SuperEvents.Remove(super);
                this.Remove(super);
            }
        }
        
        public Flow GetPreviousFlow(Event @event) {
            Event previousEvent = GetPreviousContiguous(@event);
            if (previousEvent != null)
                return previousEvent[previousEvent.Count - 1];
            else
                return null;
        }

        public Flow GetNextFlow(Event @event) {
            Event nextEvent = GetNextContiguous(@event);
            if (nextEvent != null)
                return nextEvent[0];
            else
                return null;
        }

        public Event SplitVertically(Event @base, DateTime startTime, DateTime endTime, double baseline, Event super) {
            if (super == null)
                super = new Event();

            super.Channel = Channel.Super;
            super.Baseline = baseline;
            super.BaseEvent = @base;

            if (@base.SuperEvents == null)
                @base.SuperEvents = new Events();
            @base.SuperEvents.Add(super);

            foreach (Flow flow in @base) {
                if (flow.StartTime >= startTime && flow.EndTime <= endTime) {
                    Flow flowSuper = new Flow(new TimeFrame(flow.StartTime, flow.EndTime), flow.Rate - baseline);
                    super.Add(flowSuper);

                    flow.Rate = baseline;
                }
            }
            @base.Update();
            super.Update();
            return super;
        }

        public Event SplitVerticallyWithNotification(Event @event, DateTime dateTime, double rate,
            Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent, UndoPosition undoPosition) {
            OnPropertyChanged(TwNotificationProperty.OnStartMergeSplit);

            Event super = SplitVertically(@event, dateTime, rate, classifier, removePolygon, renderEvent, undoPosition, null);

            OnPropertyChanged(TwNotificationProperty.OnPreviewEndMergeSplit);
            OnPropertyChanged(TwNotificationProperty.OnEndMergeSplit);

            return super;
        }

        public Event SplitVertically(Event @event, DateTime dateTime, double rate,
            Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent, UndoPosition position, Event super) {

            if (super == null)
                super = this.SplitVerticallyLow(@event, dateTime, rate, classifier, removePolygon, renderEvent, super, new UndoPosition(true));
            else
                this.SplitVerticallyLow(@event, dateTime, rate, classifier, removePolygon, renderEvent, super, new UndoPosition(true));

            if (super == null)
                return null;

            var fixtureClassOld = @event.FixtureClass;
            
            if (classifier != null)
                SetFixtureClasses(@event, super, classifier);
            else {
                @event.FixtureClass = FixtureClasses.Unclassified;
                super.FixtureClass = FixtureClasses.Unclassified;
            }

            if (!position.IsNull) {
                var taskMultiple = new UndoTaskVerticalSplitMultiple(position);
                var task = UndoTaskVerticalSplit.CreateUndoTask(@event, super, fixtureClassOld, dateTime, rate);
                taskMultiple.UndoTasks.Add(task);
                UndoManager.Add(taskMultiple);
            }

            this.Add(super, false);

            if (renderEvent != null)
                renderEvent(super);

            if (removePolygon != null)
                removePolygon(@event);

            if (renderEvent != null)
                renderEvent(@event);

            return super;
        }

        public Event SplitVerticallyLow(Event @event, DateTime dateTime, double rate,
            Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent, Event super, UndoPosition position) {

            DateTime startTime;
            double startRate;
            DateTime endTime;
            double endRate;

            GetVerticalSplitData(@event, dateTime, rate,
                out startTime, out startRate,
                out endTime, out endRate);

            double baseline = Math.Max(startRate, endRate);
            //if (baseline <= 0.1)
            if (!MeetsBaselineCondition(baseline))
                return null;

            this.MergeAllVerticallyIntoBase(@event, startTime, endTime, classifier, removePolygon, renderEvent, position);

            return this.SplitVertically(@event, startTime, endTime, baseline, super);
        }

        bool MeetsBaselineCondition(double baseline) {
            return baseline > 0.0;
//            return baseline > 0.1;
        }

        void GetVerticalSplitData(Event @event, DateTime dateTime, double rate,
            out DateTime startTime, out double startRate,
            out DateTime endTime, out double endRate) {

            Flow thisFlow = @event.GetFlow(dateTime, rate);
            
            Flow previousFlowWithLowerRate = @event.GetPreviousFlowWithLowerRate(thisFlow.StartTime, rate);
            Flow nextFlowWithLowerRate = @event.GetNextFlowWithLowerRate(thisFlow.StartTime, rate);

            startTime = @event.StartTime;
            startRate = 0.0;
            if (previousFlowWithLowerRate != null) {
                startTime = previousFlowWithLowerRate.EndTime;
                startRate = previousFlowWithLowerRate.Rate;
            } else {
                Flow flow = this.GetPreviousFlow(@event);
                if (flow != null && flow.Rate < rate) {
                    startTime = flow.EndTime;
                    startRate = flow.Rate;
                }
            }

            endTime = @event.EndTime;
            endRate = 0.0;
            if (nextFlowWithLowerRate != null) {
                endTime = nextFlowWithLowerRate.StartTime;
                endRate = nextFlowWithLowerRate.Rate;
            } else {
                Flow flow = this.GetNextFlow(@event);
                if (flow != null && flow.Rate < rate) {
                    endTime = flow.StartTime;
                    endRate = flow.Rate;
                }
            }
        }

        public static bool HasEventInProgress(Events events, DateTime dateTime) {
            foreach (Event @event in events) {
                if (@event.StartTime < dateTime && @event.EndTime > dateTime)
                    return true;
            }
            return false;
        }

        public Event SplitHorizontallyLow(Event @event, DateTime dateTime, bool splitSuper, bool newIsPrevious) {
            if (!splitSuper) {
                if (@event.IsASuperEventInProgressThatIsNotAlmostDone(dateTime))
                    return null;
            } else {
                Event super = @event.GetSuperEvent(dateTime);
                if (super != null)
                    SplitHorizontallyLow(@event.GetSuperEvent(dateTime), dateTime, false, newIsPrevious);
            }

            Event eventNew = new Event();

            var flowsToDelete = new List<Flow>();
            foreach (Flow flow in @event) {
                if (newIsPrevious && flow.StartTime < dateTime) {
                    eventNew.Add(flow);
                    flowsToDelete.Add(flow);
                } else if (!newIsPrevious && flow.StartTime >= dateTime) {
                    eventNew.Add(flow);
                    flowsToDelete.Add(flow);
                }
            }

            foreach(Flow flow in flowsToDelete) {
                @event.Remove(flow);
            }
            @event.Update();
            eventNew.Update();
            if (newIsPrevious)
                @event.OnPropertyChanged(TwNotificationProperty.OnChangeStartTime);

            if (@event.Channel == Channel.Super) {
                eventNew.Channel = Channel.Super;
                eventNew.BaseEvent = @event.BaseEvent;
                @event.BaseEvent.SuperEvents.Add(eventNew);
            } else if (@event.Channel == Channel.Trickle) {
                eventNew.Channel = Channel.Trickle;
            } else if (@event.Channel == Channel.Base) {
                eventNew.Channel = Channel.Base;
                MoveSuperEvents(@event, eventNew, eventNew.StartTime, eventNew.EndTime);            
            }
            eventNew.Baseline = @event.Baseline;
            eventNew.FirstCycle = @event.FirstCycle;
            eventNew.ManuallyClassified = false;

            if (eventNew.Count == 0)
                eventNew = null;

            return eventNew;
        }

        static public bool CanSplitHorizontally(Event @event) { return true; }
        static public bool CanSplitVertically(Event @event) { return (@event.Channel == Channel.Base); }

        public Event SplitHorizontallyWithNotification(Event @event, DateTime date, Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent, bool splitSuper) {
            return SplitHorizontallyWithNotification(@event, date, classifier, removePolygon, renderEvent, splitSuper, new UndoPosition(true));
        }

        public Event SplitHorizontallyWithNotification(Event @event, DateTime date, Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent, bool splitSuper, UndoPosition position) {
            OnPropertyChanged(TwNotificationProperty.OnStartMergeSplit);

            var @eventNew = SplitHorizontally(@event, date, classifier, removePolygon, renderEvent, splitSuper, position,false);
            
            OnPropertyChanged(TwNotificationProperty.OnPreviewEndMergeSplit);
            OnPropertyChanged(TwNotificationProperty.OnEndMergeSplit);

            return eventNew;
        }

        public Event SplitHorizontally(Event @event, DateTime date, Classifier classifier, RemovePolygon removePolygon, RenderEvent renderEvent, bool splitSuper, UndoPosition position, bool newIsPrevious) {
            Event eventNew = this.SplitHorizontallyLow(@event, date, splitSuper, newIsPrevious);

            if (eventNew == null)
                return null;

            var fixtureClassOld = @event.FixtureClass;

            if (classifier != null)
                SetFixtureClasses(@event, eventNew, classifier);
            else {
                @event.FixtureClass = FixtureClasses.Unclassified;
                eventNew.FixtureClass = FixtureClasses.Unclassified;
            }

            if (!position.IsNull) {
                var taskMultiple = new UndoTaskHorizontalSplitMultiple(position);
                var task = UndoTaskHorizontalSplit.CreateUndoTask(@event, fixtureClassOld, eventNew, date);
                taskMultiple.UndoTasks.Add(task);
                UndoManager.Add(taskMultiple);
            }

            if (removePolygon != null) {
                removePolygon(@event);
                renderEvent(@event);
            }

            this.Add(eventNew,false);

            if (renderEvent != null)
                renderEvent(eventNew);

            return eventNew;
    }

        void SetFixtureClasses(Event oldEvent, Event newEvent, Classifier classifier) {
            var oldEventClassifiedUsingFixtureList = oldEvent.ClassifiedUsingFixtureList;
            var newEventClassifiedUsingFixtureList = newEvent.ClassifiedUsingFixtureList;

            FixtureClass fixtureClassOldEvent = classifier.Classify(oldEvent);
            FixtureClass fixtureClassNewEvent = classifier.Classify(newEvent);

            if (!oldEvent.ManuallyClassified) {
                oldEvent.FixtureClass = fixtureClassOldEvent;
                newEvent.FixtureClass = fixtureClassNewEvent;
            } else if (oldEvent.Volume > newEvent.Volume) {
                newEvent.FixtureClass = fixtureClassNewEvent;
                oldEvent.ClassifiedUsingFixtureList = oldEventClassifiedUsingFixtureList;
            } else {
                newEvent.FixtureClass = oldEvent.FixtureClass;
                newEvent.ManuallyClassified = true;
                newEvent.ClassifiedUsingFixtureList = oldEvent.ClassifiedUsingFixtureList;

                oldEvent.FixtureClass = fixtureClassOldEvent;
                oldEvent.ManuallyClassified = false;
            }
        }

        public void UpdateLinkedList() {
            if (this.Count == 0) return;

            var eventComparer = new EventComparer();
            this.Sort(eventComparer);
            
            for (int i = 0; i < this.Count; i++) {
                var @event = this[i] as Event;

                if (i > 0)
                    @event.Previous = (Event)this[i - 1];
                else
                    @event.Previous = null;

                if (i < this.Count - 1)
                    @event.Next = (Event)this[i + 1];
                else
                    @event.Next = null;
            }
        }

        public static bool CanClassifyFirstCycles(List<Event> events) {
            foreach (Event @event in events)
                if (!@event.FixtureClass.CanHaveCycles)
                    return false;
            return true;
        }
    }
}