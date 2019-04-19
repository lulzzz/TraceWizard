using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Globalization;

using System.ComponentModel;

using TraceWizard.Entities;
using TraceWizard.Adoption;
using TraceWizard.Adoption.Adopters.Null;
using TraceWizard.Adoption.Adopters.Naive;
using TraceWizard.Classification;
using TraceWizard.Classification.Classifiers.FixtureList;
using TraceWizard.Classification.Classifiers.J48;
using TraceWizard.Disaggregation;
using TraceWizard.Disaggregation.Disaggregators.Tw4;

namespace TraceWizard.TwApp {

    public class TwSingletonBrushes {
        static readonly TwSingletonBrushes instance = new TwSingletonBrushes();
        static TwSingletonBrushes() { }
        TwSingletonBrushes() { }
        public static TwSingletonBrushes Instance { get { return instance; } }

        List<Color> colors = new List<Color>();
        List<Brush> brushes = new List<Brush>();
        
        public Brush FrozenSolidColorBrush(Color color) {
            if (colors.Contains(color))
                return brushes[colors.IndexOf(color)];
            colors.Add(color);
            brushes.Add(TwBrushes.FrozenSolidColorBrush(color));
            return brushes[colors.IndexOf(color)];
        }

        public Brush FrozenSolidColorBrush(FixtureClass fixtureClass) {
            return FrozenSolidColorBrush(fixtureClass.Color);
        }
    }
    
    public static class TwBrushes {

        public static Brush GetPlusBrush() { return Brushes.Black; }
        public static Brush GetMinusBrush() { return Brushes.Red; }

        [Obsolete]
        public static Brush GradientBrush(Color color, double offset) {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.GradientStops.Add(new GradientStop(Colors.White, 0.0));
            brush.GradientStops.Add(new GradientStop(color, offset));
            brush.Freeze();
            return brush;
        }

        [Obsolete]
        public static Brush GradientBrush(Color color) {
            return GradientBrush(color, 1.0);
        }

        [Obsolete]
        public static Brush GradientBrush(FixtureClass fixtureClass) {
            return GradientBrush(fixtureClass.Color);
        }

        public static Brush FrozenSolidColorBrush(Color color) {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        public static Brush FrozenSolidColorBrush(FixtureClass fixtureClass) {
            var brush = new SolidColorBrush(fixtureClass.Color);
            brush.Freeze();
            return brush;
        }

        public static Brush DiagonalLowerLeftStripedColorBrush(Color color) {
            var brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(5, 5);
            brush.MappingMode = BrushMappingMode.Absolute;
            brush.SpreadMethod = GradientSpreadMethod.Repeat;

            if (color == Colors.Yellow)
                brush.GradientStops.Add(new GradientStop(Colors.White, 0.40));
            else
                brush.GradientStops.Add(new GradientStop(Colors.White, 0.20));
            brush.GradientStops.Add(new GradientStop(color, 0.50));

            return brush;
        }

        public static Brush DiagonalUpperLeftStripedColorBrush(Color color) {
            var brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 5);
            brush.EndPoint = new Point(5, 0);
            brush.MappingMode = BrushMappingMode.Absolute;
            brush.SpreadMethod = GradientSpreadMethod.Repeat;

            brush.GradientStops.Add(new GradientStop(Colors.LightGray, 0.20));
            brush.GradientStops.Add(new GradientStop(color, 0.30));

            return brush;
        }

        public static Brush HorizontalStripedColorBrush(Color color) {
            var brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(0, 10);
            brush.MappingMode = BrushMappingMode.Absolute;
            brush.SpreadMethod = GradientSpreadMethod.Repeat;
            brush.GradientStops.Add(new GradientStop(Colors.White, 0.00));
            brush.GradientStops.Add(new GradientStop(color, 0.50));

            return brush;
        }

        [Obsolete]
        public static Brush PatternedColorBrush(Color color) {
            VisualBrush visualBrush = new VisualBrush();

            visualBrush.TileMode = TileMode.Tile;

            visualBrush.Viewport = new Rect(0, 0, 9, 9);
            visualBrush.ViewportUnits = BrushMappingMode.Absolute;

            visualBrush.Viewbox = new Rect(0, 0, 9, 9);
            visualBrush.ViewboxUnits = BrushMappingMode.Absolute;

            Rectangle rectangle = new Rectangle();
            rectangle.Fill = new SolidColorBrush(color);
            rectangle.Width = 9;
            rectangle.Height = 8;
            visualBrush.Visual = rectangle;

            return visualBrush;
        }

        [Obsolete]
        public static Brush DarkBrush() {
            var brush = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));
            brush.Freeze();
            return brush;
        }

        [Obsolete]
        public static Brush MediumBrush() {
            var brush = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
            brush.Freeze();
            return brush;
        }

        [Obsolete]
        public static Brush LightBrush() {
            var brush = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
            brush.Freeze();
            return brush;
        }

        public static Brush BrushFromColor(string color) {
            return (Brush)(new BrushConverter()).ConvertFromString(color);
        }
    }

    public class BrushFromString : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return new BrushConverter().ConvertFromString((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

}
