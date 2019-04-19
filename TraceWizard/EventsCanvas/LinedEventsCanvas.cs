using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

using System.Windows.Input;
using System.ComponentModel;

using TraceWizard.Entities;
using TraceWizard.Notification;

namespace TraceWizard.TwApp {

    public class LinedEventsCanvas : EventsCanvas {

        public bool HorizontalSplitMode;
        public bool VerticalSplitMode;
        public bool MergeAllIntoBaseMode;

        public bool CanStartDragging = true;

        public LinedEventsCanvas() {}

        public new void Initialize() {

            base.Initialize();

            this.MouseEnter += new MouseEventHandler(mouseEnter);
            this.MouseLeave += new MouseEventHandler(mouseLeave);
        }
        
        void mouseEnter(object sender, MouseEventArgs e) {
            OnPropertyChanged(TwNotificationProperty.OnMouseEnterEventsCanvas);
        }
        void mouseLeave(object sender, MouseEventArgs e) {
            OnPropertyChanged(TwNotificationProperty.OnMouseLeaveEventsCanvas);
        }

        public void RenderRowsColumns(bool showOneDayTicks, bool showOneDayLabel, bool showOneHourTicks, bool showOneHourLabel,
            bool showTenMinutesTicks, bool showTenMinutesLabel, bool showOneMinuteTicks, bool showOneMinuteLabel,
            bool showTwentyFiveVolumeRows, bool showTwentyFiveVolumeTicks, bool showTwentyFiveVolumeLabel,
            bool showFiveVolumeRows, bool showFiveVolumeTicks, bool showFiveVolumeLabel,
            bool showOneVolumeRows, bool showOneVolumeTicks, bool showOneVolumeLabel, int blankRows) {

            RenderRows(showTwentyFiveVolumeRows, showFiveVolumeRows, showOneVolumeRows);
            RenderVerticalGuideline();
        }

        Path pathVerticalGuideline;
        List<Path> pathHorizontalGuidelines = new List<Path>();

        public void RenderVerticalGuideline() {
            RemoveVerticalGuideline();

            if (Properties.Settings.Default.ShowVerticalGuideline && !hideVerticalGuideline)
                ShowVerticalGuideline(-2);
        }

        public void ShowVerticalGuideline(int zIndex) {

            int numRows = MaximumVolumeInTrace;
            Brush brush = TwBrushes.BrushFromColor(Properties.Settings.Default.GraphVerticalGuidelineColor);
            double width = EventsViewer.ScrollViewer.HorizontalOffset + EventsViewer.ViewportWidth / 2;
            double height = MaximumVolumeInTrace * HeightMultiplier;

            RectangleGeometry rect = new RectangleGeometry(new Rect(width, 0, 0, height));
            rect.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            rect.Freeze();

            pathVerticalGuideline = new Path();
            pathVerticalGuideline.Stroke = brush;
            pathVerticalGuideline.StrokeThickness = 1;
            pathVerticalGuideline.SnapsToDevicePixels = false;

            pathVerticalGuideline.Data = rect;
            Canvas.SetZIndex(pathVerticalGuideline, zIndex);
            this.Children.Add(pathVerticalGuideline);
        }

        void RemoveVerticalGuideline() {
            if (this.Children.Contains(pathVerticalGuideline))
                this.Children.Remove(pathVerticalGuideline);
            pathVerticalGuideline = null;
        }

        bool hideVerticalGuideline = false;
        public void RemoveVerticalGuidelinePermanently() {
            hideVerticalGuideline = true;
        }

        public void RenderRows(bool showTwentyFiveVolumeRows, bool showFiveVolumeRows, bool showOneVolumeRows) {
            RemoveRows();

            if (Properties.Settings.Default.ShowHorizontalGuideline)
                ShowRows(showTwentyFiveVolumeRows,showFiveVolumeRows,showOneVolumeRows);
        }

        void RemoveRows() {
            foreach (var path in pathHorizontalGuidelines) {
                if (this.Children.Contains(path)) {
                    this.Children.Remove(path);
                }
            }
            pathHorizontalGuidelines.Clear();
        }
        
        void ShowRows(bool showTwentyFiveVolumeRows, bool showFiveVolumeRows, bool showOneVolumeRows) {
            int numRows = MaximumVolumeInTrace;

            Brush brush = TwBrushes.BrushFromColor(Properties.Settings.Default.GraphHorizontalGuidelineColor);
            
            for (int i = 1; i < numRows; i++) {
                if (showTwentyFiveVolumeRows && i % 25 == 0)
                    pathHorizontalGuidelines.Add(RenderRow(i, brush, 3.0, -1));
                else if (showFiveVolumeRows && i % 5 == 0)
                    pathHorizontalGuidelines.Add(RenderRow(i, brush, 2.0, -1));
                else if (showOneVolumeRows)
                    pathHorizontalGuidelines.Add(RenderRow(i, brush, 1.0, -1));
            }
        }

        Path RenderRow(int row, Brush brush, double thickness, int zIndex) {
            double width = MaximumSecondsInTrace * WidthMultiplier;
            double height = ((MaximumVolumeInTrace - row) * HeightMultiplier);

            RectangleGeometry rect = new RectangleGeometry(new Rect(0, height, width, 0));
            rect.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            rect.Freeze();

            Path path = new Path();
            path.Stroke = brush;
            path.StrokeThickness = thickness;
            path.SnapsToDevicePixels = false;

            path.Data = rect;
            Canvas.SetZIndex(path, zIndex);
            this.Children.Add(path);

            return path;
        }
    }
}
