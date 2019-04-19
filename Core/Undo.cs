using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TraceWizard.Entities;
using TraceWizard.Notification;

namespace TraceWizard.Entities {
    public class UndoManager {

        public Events Events;
        public Events.RemovePolygon RemovePolygon;
        public Events.RenderEvent RenderEvent;

        public List<UndoTask> UndoTasks = new List<UndoTask>();
        int Cursor = -1;

        public bool CanUndo() {
            return Cursor > -1;
        }

        public bool CanRedo() {
            return (UndoTasks.Count - 1) - Cursor > 0;
        }

        public void Clear() {
            UndoTasks.Clear();
            Cursor = -1;
        }

        public void Add(UndoTask task) {
            for (int i = UndoTasks.Count - 1; i > Cursor; i--) {
                UndoTasks.RemoveAt(i);
            }
            UndoTasks.Add(task);
            Cursor = UndoTasks.Count - 1;
        }

        public UndoTask CurrentUndoTask() {
            return UndoTasks[Cursor];
        }

        public UndoTask CurrentRedoTask() {
            return UndoTasks[Cursor+1];
        }

        public UndoPosition Undo() {
            var position = new UndoPosition();

            UndoTask task = UndoTasks[Cursor];
            if (task is UndoTaskClassifyMultiple)
                position = DispatchUndoClassifyMultiple((UndoTaskClassifyMultiple)task);
            else if (task is UndoTaskClassifyFirstCycleMultiple)
                position = DispatchUndoClassifyFirstCycleMultiple((UndoTaskClassifyFirstCycleMultiple)task);
            else if (task is UndoTaskHorizontalSplitMultiple)
                position = DispatchUndoHorizontalSplitMultiple((UndoTaskHorizontalSplitMultiple)task);
            else if (task is UndoTaskVerticalSplitMultiple)
                position = DispatchUndoVerticalSplitMultiple((UndoTaskVerticalSplitMultiple)task);
            else if (task is UndoTaskHorizontalMergeMultiple)
                position = DispatchUndoHorizontalMergeMultiple((UndoTaskHorizontalMergeMultiple)task);
            else if (task is UndoTaskVerticalMergeMultiple)
                position = DispatchUndoVerticalMergeMultiple((UndoTaskVerticalMergeMultiple)task);
            else if (task is UndoTaskApproveMultiple)
                position = DispatchUndoApproveMultiple((UndoTaskApproveMultiple)task);
            Cursor--;

            return position;
        }

        public UndoPosition Redo() {

            var position = new UndoPosition();
            UndoTask task = UndoTasks[Cursor + 1];
            if (task is UndoTaskClassifyMultiple)
                position = DispatchRedoClassifyMultiple((UndoTaskClassifyMultiple)task);
            else if (task is UndoTaskClassifyFirstCycleMultiple)
                position = DispatchRedoClassifyFirstCycleMultiple((UndoTaskClassifyFirstCycleMultiple)task);
            else if (task is UndoTaskHorizontalSplitMultiple)
                position = DispatchRedoHorizontalSplitMultiple((UndoTaskHorizontalSplitMultiple)task);
            else if (task is UndoTaskVerticalSplitMultiple)
                position = DispatchRedoVerticalSplitMultiple((UndoTaskVerticalSplitMultiple)task);
            else if (task is UndoTaskHorizontalMergeMultiple)
                position = DispatchRedoHorizontalMergeMultiple((UndoTaskHorizontalMergeMultiple)task);
            else if (task is UndoTaskVerticalMergeMultiple)
                position = DispatchRedoVerticalMergeMultiple((UndoTaskVerticalMergeMultiple)task);
            else if (task is UndoTaskApproveMultiple)
                position = DispatchRedoApproveMultiple((UndoTaskApproveMultiple)task);
            Cursor++;

            return position;
        }

        UndoPosition DispatchUndoHorizontalSplit(UndoTaskHorizontalSplit task) {
            Events.MergeHorizontallyLow(task.EventSource, task.EventTarget, true, null, RemovePolygon, RenderEvent, new UndoPosition(true));
            task.EventTarget.FixtureClass = task.FixtureClassTarget;
            task.EventSource.FixtureClass = task.FixtureClassSource;
            return task.Position;
        }

        UndoPosition DispatchRedoHorizontalSplit(UndoTaskHorizontalSplit task) {
            task.EventSource.Clear();
            task.EventSource = Events.SplitHorizontally(task.EventTarget, task.DateTime, null, RemovePolygon, RenderEvent, false, new UndoPosition(true), false);
            task.EventTarget.FixtureClass = task.FixtureClassTargetNew;
            task.EventSource.FixtureClass = task.FixtureClassSource;
            return task.Position;
        }

        UndoPosition DispatchUndoVerticalSplit(UndoTaskVerticalSplit task) {
            Events.MergeVerticallyLow(task.EventSource, task.EventTarget, true, null, RemovePolygon, RenderEvent, new UndoPosition(true));
            task.EventTarget.FixtureClass = task.FixtureClassTargetOld;
            task.EventSource.FixtureClass = task.FixtureClassSource;
            return task.Position;
        }

        DateTime SlightlyInTheFuture(DateTime dateTime) {
            return dateTime.Add(new TimeSpan(1));
        }

        DateTime SlightlyInThePast(DateTime dateTime) {
            return dateTime.Add(new TimeSpan(-1));
        }

        double SlightlyMore(double d) {
            return d + 0.001;
        }

        UndoPosition DispatchRedoVerticalSplit(UndoTaskVerticalSplit task) {
            task.EventSource.Clear();
            task.EventSource = Events.SplitVertically(task.EventTarget, task.DateTime, task.Rate, null, RemovePolygon, RenderEvent, new UndoPosition(true), task.EventSource);

            task.EventTarget.FixtureClass = task.FixtureClassTargetNew;
            task.EventSource.FixtureClass = task.FixtureClassSource;
            return task.Position;
        }

        UndoPosition DispatchUndoHorizontalMerge(UndoTaskHorizontalMerge task) {
            if (task.LeftEventIsUserSource)
                task.EventLeft = Events.SplitHorizontally(task.EventRight, SlightlyInThePast(task.SplitTime), null, RemovePolygon, RenderEvent, false, new UndoPosition(true), true);
            else
                task.EventRight = Events.SplitHorizontally(task.EventLeft, SlightlyInThePast(task.SplitTime), null, RemovePolygon, RenderEvent, false, new UndoPosition(true), false);

            task.EventLeft.FixtureClass = task.FixtureClassLeftOld;
            task.EventRight.FixtureClass = task.FixtureClassRightOld;
            task.EventLeft.ClassifiedUsingFixtureList = task.ClassifiedUsingFixtureListLeftOld;
            task.EventRight.ClassifiedUsingFixtureList = task.ClassifiedUsingFixtureListRightOld;

            return task.Position;
        }

        UndoPosition DispatchRedoHorizontalMerge(UndoTaskHorizontalMerge task) {
            if (task.LeftEventIsUserSource)
                Events.MergeHorizontallyLow(task.EventLeft, task.EventRight, true, null, RemovePolygon, RenderEvent, new UndoPosition(true));
            else
                Events.MergeHorizontallyLow(task.EventRight, task.EventLeft, true, null, RemovePolygon, RenderEvent, new UndoPosition(true));

            task.EventLeft.FixtureClass = task.FixtureClassLeftNew;
            task.EventRight.FixtureClass = task.FixtureClassRightNew;
            task.EventLeft.ClassifiedUsingFixtureList = task.ClassifiedUsingFixtureListLeftNew;
            task.EventRight.ClassifiedUsingFixtureList = task.ClassifiedUsingFixtureListRightNew;

            return task.Position;
        }

        UndoPosition DispatchUndoVerticalMerge(UndoTaskVerticalMerge task) {
            task.EventSource.Clear();
//            task.EventSource = Events.SplitVertically(task.EventTarget, SlightlyInTheFuture(task.EventSource.StartTime), SlightlyMore(task.EventSource.Baseline), null, RemovePolygon, RenderEvent, new UndoPosition(true), task.EventSource);
//            task.EventSource = Events.SplitVertically(task.EventTarget, SlightlyInTheFuture(task.EventSource.StartTime), task.EventSource.Baseline, null, RemovePolygon, RenderEvent, new UndoPosition(true), task.EventSource);
            task.EventSource = Events.SplitVertically(task.EventTarget, task.EventSource.StartTime, task.EventSource.Baseline, null, RemovePolygon, RenderEvent, new UndoPosition(true), task.EventSource); 
            task.EventTarget.FixtureClass = task.FixtureClassTargetOld;
            task.EventSource.FixtureClass = task.FixtureClassSourceOld;
            return task.Position;
        }

        UndoPosition DispatchRedoVerticalMerge(UndoTaskVerticalMerge task) {
            Events.MergeVerticallyLow(task.EventSource, task.EventTarget, true, null, RemovePolygon, RenderEvent, new UndoPosition(true));
            task.EventTarget.FixtureClass = task.FixtureClassTargetNew;
            task.EventSource.FixtureClass = task.FixtureClassSourceOld;
            return task.Position;
        }

        UndoPosition DispatchUndoClassifyFirstCycle(UndoTaskClassifyFirstCycle task) {
            task.Event.FirstCycle = task.FirstCycleOld;
            task.Event.ManuallyClassifiedFirstCycle = task.ManuallyClassifiedFirstCycleOld;
            return task.Position;
        }

        UndoPosition DispatchRedoClassifyFirstCycle(UndoTaskClassifyFirstCycle task) {
            task.Event.FirstCycle = task.FirstCycleNew;
            task.Event.ManuallyClassifiedFirstCycle = task.ManuallyClassifiedFirstCycleNew;
            return task.Position;
        }

        UndoPosition DispatchUndoClassify(UndoTaskClassify task) {
            task.Event.FixtureClass = task.FixtureClassTarget;
            task.Event.ManuallyClassified = task.ManuallyClassifiedOld;
            task.Event.ClassifiedUsingFixtureList = task.ClassifiedUsingFixtureListOld;
            return task.Position;
        }

        UndoPosition DispatchRedoClassify(UndoTaskClassify task) {
            task.Event.FixtureClass = task.FixtureClassSource;
            task.Event.ManuallyClassified = task.ManuallyClassifiedNew;
            task.Event.ClassifiedUsingFixtureList = task.ClassifiedUsingFixtureListNew;
            return task.Position;
        }

        UndoPosition DispatchUndoApprove(UndoTaskApprove task) {
            task.Event.ManuallyApproved = task.ManuallyApprovedOld;
            return task.Position;
        }

        UndoPosition DispatchRedoApprove(UndoTaskApprove task) {
            task.Event.ManuallyApproved = task.ManuallyApprovedNew;
            return task.Position;
        }

        UndoPosition DispatchUndoApproveMultiple(UndoTaskApproveMultiple task) {
            Events.OnPropertyChanged(TwNotificationProperty.OnStartClassify);
            foreach (var taskSingle in task.UndoTasks.Reverse<UndoTaskApprove>()) {
                DispatchUndoApprove(taskSingle);
            }
            Events.OnPropertyChanged(TwNotificationProperty.OnPreviewEndClassify);
            Events.OnPropertyChanged(TwNotificationProperty.OnEndClassify);

            return task.Position;
        }

        UndoPosition DispatchRedoApproveMultiple(UndoTaskApproveMultiple task) {
            Events.OnPropertyChanged(TwNotificationProperty.OnStartClassify);
            foreach (var taskSingle in task.UndoTasks) {
                DispatchRedoApprove(taskSingle);
            }
            Events.OnPropertyChanged(TwNotificationProperty.OnPreviewEndClassify);
            Events.OnPropertyChanged(TwNotificationProperty.OnEndClassify);

            return task.Position;
        }

        UndoPosition DispatchUndoClassifyMultiple(UndoTaskClassifyMultiple task) {
            Events.OnPropertyChanged(TwNotificationProperty.OnStartClassify);
            foreach (var taskSingle in task.UndoTasks.Reverse<UndoTaskClassify>()) {
                DispatchUndoClassify(taskSingle);
            }
            Events.OnPropertyChanged(TwNotificationProperty.OnPreviewEndClassify);
            Events.OnPropertyChanged(TwNotificationProperty.OnEndClassify);

            return task.Position;
        }

        UndoPosition DispatchRedoClassifyMultiple(UndoTaskClassifyMultiple task) {
            Events.OnPropertyChanged(TwNotificationProperty.OnStartClassify);
            foreach (var taskSingle in task.UndoTasks) {
                DispatchRedoClassify(taskSingle);
            }
            Events.OnPropertyChanged(TwNotificationProperty.OnPreviewEndClassify);
            Events.OnPropertyChanged(TwNotificationProperty.OnEndClassify);

            return task.Position;
        }

        UndoPosition DispatchUndoClassifyFirstCycleMultiple(UndoTaskClassifyFirstCycleMultiple task) {
            Events.OnPropertyChanged(TwNotificationProperty.OnStartClassify);
            foreach (var taskSingle in task.UndoTasks.Reverse<UndoTaskClassifyFirstCycle>()) {
                DispatchUndoClassifyFirstCycle(taskSingle);
            }
            Events.OnPropertyChanged(TwNotificationProperty.OnPreviewEndClassify);
            Events.OnPropertyChanged(TwNotificationProperty.OnEndClassify);

            return task.Position;
        }

        UndoPosition DispatchRedoClassifyFirstCycleMultiple(UndoTaskClassifyFirstCycleMultiple task) {
            Events.OnPropertyChanged(TwNotificationProperty.OnStartClassify);
            foreach (var taskSingle in task.UndoTasks) {
                DispatchRedoClassifyFirstCycle(taskSingle);
            }
            Events.OnPropertyChanged(TwNotificationProperty.OnPreviewEndClassify);
            Events.OnPropertyChanged(TwNotificationProperty.OnEndClassify);

            return task.Position;
        }

        UndoPosition DispatchUndoHorizontalSplitMultiple(UndoTaskHorizontalSplitMultiple task) {
            Events.OnPropertyChanged(TwNotificationProperty.OnStartMergeSplit);
            foreach (var taskSingle in task.UndoTasks.Reverse<UndoTaskHorizontalSplit>()) {
                DispatchUndoHorizontalSplit(taskSingle);
            }
            Events.OnPropertyChanged(TwNotificationProperty.OnPreviewEndMergeSplit);
            Events.OnPropertyChanged(TwNotificationProperty.OnEndMergeSplit);

            return task.Position;
        }

        UndoPosition DispatchRedoHorizontalSplitMultiple(UndoTaskHorizontalSplitMultiple task) {
            Events.OnPropertyChanged(TwNotificationProperty.OnStartMergeSplit);
            foreach (var taskSingle in task.UndoTasks) {
                DispatchRedoHorizontalSplit(taskSingle);
            }
            Events.OnPropertyChanged(TwNotificationProperty.OnPreviewEndMergeSplit);
            Events.OnPropertyChanged(TwNotificationProperty.OnEndMergeSplit);

            return task.Position;
        }

        UndoPosition DispatchUndoVerticalSplitMultiple(UndoTaskVerticalSplitMultiple task) {
            Events.OnPropertyChanged(TwNotificationProperty.OnStartMergeSplit);
            foreach (var taskSingle in task.UndoTasks.Reverse<UndoTaskVerticalSplit>()) {
                DispatchUndoVerticalSplit(taskSingle);
            }
            Events.OnPropertyChanged(TwNotificationProperty.OnPreviewEndMergeSplit);
            Events.OnPropertyChanged(TwNotificationProperty.OnEndMergeSplit);

            return task.Position;
        }

        UndoPosition DispatchRedoVerticalSplitMultiple(UndoTaskVerticalSplitMultiple task) {
            Events.OnPropertyChanged(TwNotificationProperty.OnStartMergeSplit);
            foreach (var taskSingle in task.UndoTasks) {
                DispatchRedoVerticalSplit(taskSingle);
            }
            Events.OnPropertyChanged(TwNotificationProperty.OnPreviewEndMergeSplit);
            Events.OnPropertyChanged(TwNotificationProperty.OnEndMergeSplit);

            return task.Position;
        }

        UndoPosition DispatchUndoHorizontalMergeMultiple(UndoTaskHorizontalMergeMultiple task) {
            Events.OnPropertyChanged(TwNotificationProperty.OnStartMergeSplit);
            foreach (var taskSingle in task.UndoTasks.Reverse<UndoTaskHorizontalMerge>()) {
                DispatchUndoHorizontalMerge(taskSingle);
            }
            Events.OnPropertyChanged(TwNotificationProperty.OnPreviewEndMergeSplit);
            Events.OnPropertyChanged(TwNotificationProperty.OnEndMergeSplit);

            return task.Position;
        }

        UndoPosition DispatchRedoHorizontalMergeMultiple(UndoTaskHorizontalMergeMultiple task) {
            Events.OnPropertyChanged(TwNotificationProperty.OnStartMergeSplit);
            foreach (var taskSingle in task.UndoTasks) {
                DispatchRedoHorizontalMerge(taskSingle);
            }
            Events.OnPropertyChanged(TwNotificationProperty.OnPreviewEndMergeSplit);
            Events.OnPropertyChanged(TwNotificationProperty.OnEndMergeSplit);

            return task.Position;
        }

        UndoPosition DispatchUndoVerticalMergeMultiple(UndoTaskVerticalMergeMultiple task) {
            Events.OnPropertyChanged(TwNotificationProperty.OnStartMergeSplit);
            foreach (var taskSingle in task.UndoTasks.Reverse<UndoTaskVerticalMerge>()) {
                DispatchUndoVerticalMerge(taskSingle);
            }
            Events.OnPropertyChanged(TwNotificationProperty.OnPreviewEndMergeSplit);
            Events.OnPropertyChanged(TwNotificationProperty.OnEndMergeSplit);

            return task.Position;
        }

        UndoPosition DispatchRedoVerticalMergeMultiple(UndoTaskVerticalMergeMultiple task) {
            Events.OnPropertyChanged(TwNotificationProperty.OnStartMergeSplit);
            foreach (var taskSingle in task.UndoTasks) {
                DispatchRedoVerticalMerge(taskSingle);
            }
            Events.OnPropertyChanged(TwNotificationProperty.OnPreviewEndMergeSplit);
            Events.OnPropertyChanged(TwNotificationProperty.OnEndMergeSplit);

            return task.Position;
        }

    }

    public struct UndoPosition {
        public double PercentElapsedHorizontal;
        public double PercentElapsedVertical;
        public double ViewportSeconds;
        public double ViewportVolume;

        public bool IsNull;        
        public UndoPosition(bool initializeToNull) {
        PercentElapsedHorizontal = 0;
        PercentElapsedVertical = 0;
        ViewportSeconds = 0;
        ViewportVolume = 0;

        IsNull = true;
        }
    }

    public abstract class UndoTask {
        public UndoPosition Position;
    }

    public class UndoTaskClassifyFirstCycle : UndoTask {
        public Event Event;
        public bool FirstCycleOld;
        public bool FirstCycleNew;
        public bool ManuallyClassifiedFirstCycleOld;
        public bool ManuallyClassifiedFirstCycleNew;

        static public UndoTaskClassifyFirstCycle CreateUndoTask(Event @event, bool firstCycleOld, bool firstCycleNew, bool manuallyClassifiedFirstCycleOld, bool manuallyClassifiedFirstCycleNew) {
            var task = new UndoTaskClassifyFirstCycle();
            task.Event = @event;
            task.FirstCycleOld = firstCycleOld;
            task.FirstCycleNew = firstCycleNew;
            task.ManuallyClassifiedFirstCycleOld = manuallyClassifiedFirstCycleOld;
            task.ManuallyClassifiedFirstCycleNew = manuallyClassifiedFirstCycleNew;
            return task;
        }
    }

    public class UndoTaskApprove : UndoTask {
        public Event Event;
        public bool ManuallyApprovedOld;
        public bool ManuallyApprovedNew;

        static public UndoTaskApprove CreateUndoTask(Event @event, bool manuallyApprovedOld, bool manuallyApprovedNew) {
            var task = new UndoTaskApprove();
            task.Event = @event;
            task.ManuallyApprovedOld = manuallyApprovedOld;
            task.ManuallyApprovedNew = manuallyApprovedNew;
            return task;
        }
    }

    public class UndoTaskClassify : UndoTask {
        public Event Event;
        public FixtureClass FixtureClassTarget;
        public FixtureClass FixtureClassSource;
        public bool ManuallyClassifiedOld;
        public bool ManuallyClassifiedNew;
        public bool ClassifiedUsingFixtureListOld;
        public bool ClassifiedUsingFixtureListNew;

        static public UndoTaskClassify CreateUndoTask(Event @event, FixtureClass fixtureClassTarget, FixtureClass fixtureClassSource, bool manuallyClassifiedOld, bool manuallyClassifiedNew, bool classifiedUsingFixtureListOld, bool classifiedUsingFixtureListNew) {
            var task = new UndoTaskClassify();
            task.Event = @event;
            task.FixtureClassTarget = fixtureClassTarget;
            task.FixtureClassSource = fixtureClassSource;
            task.ManuallyClassifiedOld = manuallyClassifiedOld;
            task.ManuallyClassifiedNew = manuallyClassifiedNew;
            task.ClassifiedUsingFixtureListOld = classifiedUsingFixtureListOld;
            task.ClassifiedUsingFixtureListNew = classifiedUsingFixtureListNew;
            return task;
        }
    }

    public class UndoTaskClassifyMultiple : UndoTask {
        public UndoTaskClassifyMultiple(UndoPosition position) {
            Position = position;
        }
        override public string ToString() {
            return "Classify";
        }
        public List<UndoTaskClassify> UndoTasks = new List<UndoTaskClassify>();
    }

    public class UndoTaskApproveMultiple : UndoTask {
        public UndoTaskApproveMultiple(UndoPosition position) {
            Position = position;
        }
        override public string ToString() {
            return "Approve/Unapprove";
        }
        public List<UndoTaskApprove> UndoTasks = new List<UndoTaskApprove>();
    }

    public class UndoTaskClassifyFirstCycleMultiple : UndoTask {
        public UndoTaskClassifyFirstCycleMultiple(UndoPosition position) {
            Position = position;
        }
        override public string ToString() {
            return "Classify/Unclassify as 1st Cycle";
        }
        public List<UndoTaskClassifyFirstCycle> UndoTasks = new List<UndoTaskClassifyFirstCycle>();
    }

    public class UndoTaskHorizontalSplit : UndoTask {
        public Event EventTarget;
        public FixtureClass FixtureClassTarget;
        public FixtureClass FixtureClassTargetNew;
        public Event EventSource;
        public FixtureClass FixtureClassSource;
        public DateTime DateTime;

        static public UndoTaskHorizontalSplit CreateUndoTask(Event eventTarget, FixtureClass fixtureClassTarget, Event eventSource, DateTime dateTime) {
            var task = new UndoTaskHorizontalSplit();
            task.EventTarget = eventTarget;
            task.FixtureClassTarget = fixtureClassTarget;
            task.FixtureClassTargetNew = task.EventTarget.FixtureClass;
            task.EventSource = eventSource;
            task.FixtureClassSource = eventSource.FixtureClass;
            task.DateTime = dateTime;
            return task;
        }
    }

    public class UndoTaskHorizontalSplitMultiple : UndoTask {
        public UndoTaskHorizontalSplitMultiple(UndoPosition position) {
            Position = position;
        }
        override public string ToString() {
            return "Split into Contiguous Events";
        }
        public List<UndoTaskHorizontalSplit> UndoTasks = new List<UndoTaskHorizontalSplit>();
    }

    public class UndoTaskVerticalSplit : UndoTask {
        public Event EventTarget;
        public FixtureClass FixtureClassTargetOld;
        public FixtureClass FixtureClassTargetNew;
        public Event EventSource;
        public FixtureClass FixtureClassSource;
        public DateTime DateTime;
        public double Rate;
        static public UndoTaskVerticalSplit CreateUndoTask(Event eventTarget, Event eventSource, FixtureClass fixtureClassTargetOld, DateTime dateTime, double rate) {
            var task = new UndoTaskVerticalSplit();
            task.EventTarget = eventTarget;
            task.FixtureClassTargetOld = fixtureClassTargetOld;
            task.FixtureClassTargetNew = task.EventTarget.FixtureClass;
            task.EventSource = eventSource;
            task.FixtureClassSource = eventSource.FixtureClass;
            task.DateTime = dateTime;
            task.Rate = rate;
            return task;
        }
    }

    public class UndoTaskVerticalSplitMultiple : UndoTask {
        public UndoTaskVerticalSplitMultiple(UndoPosition position) {
            Position = position;
        }
        override public string ToString() {
            return "Split into Simultaneous Events";
        }
        public List<UndoTaskVerticalSplit> UndoTasks = new List<UndoTaskVerticalSplit>();
    }

    public class UndoTaskHorizontalMerge : UndoTask {
        public Event EventLeft;
        public FixtureClass FixtureClassLeftOld;
        public FixtureClass FixtureClassLeftNew;
        public bool ClassifiedUsingFixtureListLeftOld;
        public bool ClassifiedUsingFixtureListLeftNew;
        public Event EventRight;
        public FixtureClass FixtureClassRightOld;
        public FixtureClass FixtureClassRightNew;
        public bool ClassifiedUsingFixtureListRightOld;
        public bool ClassifiedUsingFixtureListRightNew;

        public bool LeftEventIsUserSource;
        public DateTime SplitTime;

        static public UndoTaskHorizontalMerge CreateUndoTask(Event eventTarget, Event eventSource, FixtureClass fixtureClassTargetOld, FixtureClass fixtureClassSourceOld, bool classifiedUsingFixtureListTargetOld, bool classifiedUsingFixtureListSourceOld) {
            var task = new UndoTaskHorizontalMerge();

            task.LeftEventIsUserSource = (eventSource.StartTime == eventTarget.StartTime);

            if (task.LeftEventIsUserSource) {
                task.EventLeft = eventSource;
                task.FixtureClassLeftOld = fixtureClassSourceOld;
                task.FixtureClassLeftNew = eventSource.FixtureClass;
                task.ClassifiedUsingFixtureListLeftOld = classifiedUsingFixtureListSourceOld;
                task.ClassifiedUsingFixtureListLeftNew = eventSource.ClassifiedUsingFixtureList;

                task.EventRight = eventTarget;
                task.FixtureClassRightOld = fixtureClassTargetOld;
                task.FixtureClassRightNew = eventTarget.FixtureClass;
                task.ClassifiedUsingFixtureListRightOld = classifiedUsingFixtureListTargetOld;
                task.ClassifiedUsingFixtureListRightNew = eventTarget.ClassifiedUsingFixtureList;

                task.SplitTime = task.EventLeft.EndTime;
            } else {
                task.EventLeft = eventTarget;
                task.FixtureClassLeftOld = fixtureClassTargetOld;
                task.FixtureClassLeftNew = eventTarget.FixtureClass;
                task.ClassifiedUsingFixtureListLeftOld = classifiedUsingFixtureListTargetOld;
                task.ClassifiedUsingFixtureListLeftNew = eventTarget.ClassifiedUsingFixtureList;

                task.EventRight = eventSource;
                task.FixtureClassRightOld = fixtureClassSourceOld;
                task.FixtureClassRightNew = eventSource.FixtureClass;
                task.ClassifiedUsingFixtureListRightOld = classifiedUsingFixtureListSourceOld;
                task.ClassifiedUsingFixtureListRightNew = eventSource.ClassifiedUsingFixtureList;

                task.SplitTime = task.EventRight.StartTime;
            }

            return task;
        }
    }

    public class UndoTaskHorizontalMergeMultiple : UndoTask {
        public UndoTaskHorizontalMergeMultiple(UndoPosition position) {
            Position = position;
        }
        override public string ToString() {
            return "Merge of Contiguous Events";
        }
        public List<UndoTaskHorizontalMerge> UndoTasks = new List<UndoTaskHorizontalMerge>();
    }

    public class UndoTaskVerticalMerge : UndoTask {
        public Event EventSource;
        public FixtureClass FixtureClassSourceOld;
        public Event EventTarget;
        public FixtureClass FixtureClassTargetOld;
        public FixtureClass FixtureClassTargetNew;

        static public UndoTaskVerticalMerge CreateUndoTask(Event eventTarget, Event eventSource, FixtureClass fixtureClassTargetOld, FixtureClass fixtureClassSourceOld) {
            var task = new UndoTaskVerticalMerge();
            task.EventSource = eventSource;
            task.FixtureClassSourceOld = fixtureClassSourceOld;
            task.EventTarget = eventTarget;
            task.FixtureClassTargetOld = fixtureClassTargetOld;
            task.FixtureClassTargetNew = task.EventTarget.FixtureClass;

            return task;
        }
    }

    public class UndoTaskVerticalMergeMultiple : UndoTask {
        public UndoTaskVerticalMergeMultiple(UndoPosition position) {
            Position = position;
        }
        override public string ToString() {
            return "Merge of Simultaneous Events";
        }
        public List<UndoTaskVerticalMerge> UndoTasks = new List<UndoTaskVerticalMerge>();
    }
}
