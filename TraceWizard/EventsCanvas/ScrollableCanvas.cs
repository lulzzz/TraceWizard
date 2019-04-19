using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace TraceWizard.TwApp {

    public class ScrollableCanvas : Canvas {

        // NOT SURE IF THIS IS NEEDED, TW COMPILES WITHOUT IT
        static ScrollableCanvas() {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ScrollableCanvas), new FrameworkPropertyMetadata(typeof(ScrollableCanvas)));
        }

        // THIS IS THE ORIGINAL VERSION, NOT SURE IF STILL NEEDED, DON'T SEE THE EFFECT OF INCLUDING IT
        protected override Size MeasureOverride(Size constraint) {
            double bottomMost = 0d;
            double rightMost = 0d;

            foreach (object obj in Children) {
                FrameworkElement child = obj as FrameworkElement;
                if (child != null) {
                    child.Measure(constraint);

                    bottomMost = Math.Max(bottomMost, GetTop(child) + child.DesiredSize.Height);

                    rightMost = Math.Max(rightMost, GetLeft(child) + child.DesiredSize.Width);
                }
            }

            if (double.IsNaN(bottomMost) || double.IsInfinity(bottomMost))
                bottomMost = 0d;
            if (double.IsNaN(rightMost) || double.IsInfinity(rightMost))
                rightMost = 0d;

            return new Size(rightMost, bottomMost);
        }

        // THIS LINQ VERSION CRASHES

        //protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint) {
        //    base.MeasureOverride(constraint);
        //    double width = base
        //        .InternalChildren
        //        .OfType<UIElement>()
        //        .Max(i => i.DesiredSize.Width + (double)i.GetValue(Canvas.LeftProperty));

        //    double height = base
        //        .InternalChildren
        //        .OfType<UIElement>()
        //        .Max(i => i.DesiredSize.Height + (double)i.GetValue(Canvas.TopProperty));

        //    return new Size(width, height);
        //}

        // NOT SURE IF THIS VERSION WORKS
        
        //protected override Size MeasureOverride(Size constraint) {
        //    Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

        //    double maxHeight = 0;
        //    double maxWidth = 0;

        //    foreach (UIElement element in base.InternalChildren)
        //    {
        //        if (element != null)
        //        {
        //            element.Measure(availableSize);
        //            double left = Canvas.GetLeft(element);
        //            double top = Canvas.GetTop(element);
        //            left += element.DesiredSize.Width;
        //            top += element.DesiredSize.Height;

        //            maxWidth = maxWidth < left ? left : maxWidth;
        //            maxHeight = maxHeight < top ? top : maxHeight;
        //        }
        //    }

        //    return new Size { Height = maxHeight, Width = maxWidth };
        //}
    }
}