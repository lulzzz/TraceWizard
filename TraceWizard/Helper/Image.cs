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

using System.Windows.Input;
using System.IO;

using System.ComponentModel;
using System.Windows.Media.Imaging;


using TraceWizard.Entities;
using TraceWizard.Analyzers;
using TraceWizard.Adoption;
using TraceWizard.Adoption.Adopters.Null;
using TraceWizard.Adoption.Adopters.Naive;
using TraceWizard.Classification;
using TraceWizard.Classification.Classifiers.FixtureList;
using TraceWizard.Classification.Classifiers.J48;
using TraceWizard.Disaggregation;
using TraceWizard.Disaggregation.Disaggregators.Tw4;
using TraceWizard.Notification;

namespace TraceWizard.TwApp {

    public static class ResourceLocator {
        public static object FindResource(object key) {
            if (Application.Current != null)
                return Application.Current.FindResource(key);
            else
                return null;
        }
    }

    [Obsolete]
    public class GradientBrushFromFixtureClass : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return TwBrushes.GradientBrush(((FixtureClass)value).Color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class FrozenSolidColorBrushFromFixtureClass : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return TwBrushes.FrozenSolidColorBrush(((FixtureClass)value).Color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class ImageFromFixtureClass : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return TwGui.GetImage(((FixtureClass)value).ImageFilename);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
    public class ScrollViewerHorizontalConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return -1 * (double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class ScrollViewerVerticalConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return -1 * (double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class StyledFixtureLabel : UserControl {
        public FixtureClass FixtureClass;

        UIElement BuildToolTip(Event @event) {
            var panel = new StackPanel();

            var text = new TextBlock();
            text.Text = "Selected Event is " + @event.FixtureClass.FriendlyName;
            panel.Children.Add(text);

            {
                text = new TextBlock();
                var tick = new ClassificationTick();
                tick.VerticalAlignment = VerticalAlignment.Center;
                tick.Padding = new Thickness(0, 0, 3, 0);
                if (@event.ManuallyClassified) {
                    tick.TickClassifiedUsingFixtureList.Visibility = Visibility.Collapsed;
                    tick.TickClassifiedUsingMachineLearning.Visibility = Visibility.Collapsed;
                    text.Text = "Manually Classified";
                } else if (@event.ClassifiedUsingFixtureList) {
                    tick.TickManuallyClassified.Visibility = Visibility.Collapsed;
                    tick.TickClassifiedUsingMachineLearning.Visibility = Visibility.Collapsed;
                    text.Text = "Classified Using Fixture List";
                } else {
                    tick.TickManuallyClassified.Visibility = Visibility.Collapsed;
                    tick.TickClassifiedUsingFixtureList.Visibility = Visibility.Collapsed;
                    text.Text = "Classified Using Machine Learning";
                }

                var panelInner = new StackPanel();
                panelInner.Orientation = Orientation.Horizontal;
                panelInner.Children.Add(tick);
                panelInner.Children.Add(text);

                panel.Children.Add(panelInner);
            }

            if (@event.ManuallyApproved) {
                var panelInner = new StackPanel();
                panelInner.Orientation = Orientation.Horizontal;

                var tick = new ApprovalTick();
                tick.VerticalAlignment = VerticalAlignment.Center;
                tick.Padding = new Thickness(0, 0, 3, 0);
                panelInner.Children.Add(tick);
                text = new TextBlock();
                text.Text = "Manually Approved";
                panelInner.Children.Add(text);

                panel.Children.Add(panelInner);
            }

            if (@event.FirstCycle) {
                text = new TextBlock();
                if (@event.ManuallyClassifiedFirstCycle)
                    text.Text = "Manually Classified As 1st Cycle";
                else
                    text.Text = "Classified As 1st Cycle Using Machine Learning";
                panel.Children.Add(text);
            } else if (@event.ManuallyClassifiedFirstCycle) {
                text = new TextBlock();
                text.Text = "Manually Classified As Not 1st Cycle";
                panel.Children.Add(text);
            }

            return panel;
        }

        public StyledFixtureLabel(Event @event, bool showFriendlyName, bool activateCommand) {
            this.Focusable = false;
            this.IsTabStop = false;

            if (activateCommand)
                this.InputBindings.Add(new MouseBinding(AnalysisPanel.BringSelectedEventIntoViewCommand, new MouseGesture(MouseAction.LeftDoubleClick, ModifierKeys.None)));

            FixtureClass = @event.FixtureClass;

            this.ToolTip = BuildToolTip(@event);
            
            var panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;


            var panelTicks = new StackPanel();

            {
                var tick = new ClassificationTick();
                tick.Padding = new Thickness(0, 1, 4, 1);
                tick.VerticalAlignment = VerticalAlignment.Center;

                if (@event.ManuallyClassified) {
                    tick.TickClassifiedUsingFixtureList.Visibility = Visibility.Collapsed;
                    tick.TickClassifiedUsingMachineLearning.Visibility = Visibility.Collapsed;
                } else if (@event.ClassifiedUsingFixtureList) {
                    tick.TickManuallyClassified.Visibility = Visibility.Collapsed;
                    tick.TickClassifiedUsingMachineLearning.Visibility = Visibility.Collapsed;
                } else {
                    tick.TickManuallyClassified.Visibility = Visibility.Collapsed;
                    tick.TickClassifiedUsingFixtureList.Visibility = Visibility.Collapsed;
                }
                panelTicks.Children.Add(tick);
            }

            if (@event.ManuallyApproved) {
                var tick = new ApprovalTick();
                tick.Padding = new Thickness(0, 1, 4, 1);
                tick.VerticalAlignment = VerticalAlignment.Center;
                panelTicks.Children.Add(tick);
            }

            panel.Children.Add(panelTicks);

            Image image = new Image();
            image.Style = (Style)ResourceLocator.FindResource("ImageStyle");
            image.Source = TwGui.GetImage(@event.FixtureClass.ImageFilename);

            Border border = new Border();
            border.Margin = new Thickness(3, 0, 0, 0);
            border.Style = (Style)ResourceLocator.FindResource("FixtureBorderStyle");
            border.Background = TwBrushes.FrozenSolidColorBrush(@event.FixtureClass.Color);
            border.VerticalAlignment = VerticalAlignment.Top;
            border.Child = image;
            panel.Children.Add(border);

            var label = new TextBlock();
            label.Text = showFriendlyName ? @event.FixtureClass.FriendlyName : @event.FixtureClass.ShortName;
            label.Padding = new Thickness(3, 0, 0, 0);
            panel.Children.Add(label);

            if (@event.FirstCycle) {
                label = new TextBlock();
                label.FontSize = 8;
                label.BaselineOffset = 10;
                label.Text = "1";
                if (!@event.ManuallyClassifiedFirstCycle)
                    label.FontStyle = FontStyles.Italic;
                label.Padding = new Thickness(3, 0, 0, 0);
                panel.Children.Add(label);
            } else if (@event.ManuallyClassifiedFirstCycle) {
                label = new TextBlock();
                label.FontSize = 8;
                label.BaselineOffset = 10;
                label.Text = "0";
                label.Padding = new Thickness(3, 0, 0, 0);
                panel.Children.Add(label);
            }

            //if (!string.IsNullOrEmpty(@event.UserNotes)) {
            //    label = new TextBlock();
            //    label.Padding = new Thickness(3, 0, 0, 0);
            //    label.Text = "N";
            //    panel.Children.Add(label);
            //}

            this.Content = panel;
        }

        public StyledFixtureLabel(FixtureClass fixtureClass, FontWeight fontWeight, bool showKey, bool manuallyClassified, bool firstCycle, bool firstCycleManuallyClassified, bool showFriendlyName, bool singleRow, bool showHasNotes) {
            this.Focusable = false;
            this.IsTabStop = false;

            this.InputBindings.Add(new MouseBinding(AnalysisPanel.BringSelectedEventIntoViewCommand, new MouseGesture(MouseAction.LeftDoubleClick, ModifierKeys.None)));

            FixtureClass = fixtureClass;

            Grid grid = new Grid();

            ColumnDefinition coldefText = new ColumnDefinition();
            coldefText.Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(coldefText);

            ColumnDefinition coldefImage = new ColumnDefinition();
            coldefImage.Width = new GridLength(1, GridUnitType.Star);
            grid.ColumnDefinitions.Add(coldefImage);

            Image image = new Image();
            image.Style = (Style)ResourceLocator.FindResource("ImageStyle");
            image.Source = TwGui.GetImage(fixtureClass.ImageFilename);

            Border border = new Border();
            border.Style = (Style)ResourceLocator.FindResource("FixtureBorderStyle");
            border.Background = TwBrushes.FrozenSolidColorBrush(fixtureClass.Color);
            border.VerticalAlignment = VerticalAlignment.Top;
            border.Child = image;

            Grid.SetColumn(border, 0);
            grid.Children.Add(border);

            var stackPanel = new StackPanel();
            
            var label = new TextBlock();

            var name = showFriendlyName ? fixtureClass.FriendlyName : fixtureClass.ShortName;
            if (showKey)
                label.Text = name + " (" + fixtureClass.Character + ")";
            else
                label.Text = name;

            if (singleRow)
                grid.ToolTip = "Selected Event is " + fixtureClass.FriendlyName;

            if (manuallyClassified) {
                label.Text += "*";
                if (singleRow)
                    grid.ToolTip += "\r\n* = Manually classified";
            }

            label.Margin = new Thickness(6, 0, 0, 0);
            label.FontWeight = fontWeight;
            label.HorizontalAlignment = HorizontalAlignment.Left;
            label.VerticalAlignment = VerticalAlignment.Center;

            stackPanel.Children.Add(label);

            if (firstCycle) {
                label = new TextBlock();
                label.Text = "(1st Cycle)";
                label.Margin = new Thickness(6, 0, 0, 0);
                label.FontWeight = fontWeight;
                label.HorizontalAlignment = HorizontalAlignment.Left;
                label.VerticalAlignment = VerticalAlignment.Center;
                stackPanel.Children.Add(label);

                if (singleRow) {
                    stackPanel.Orientation = Orientation.Horizontal;
                    label.Text = "1";
                    if (singleRow)
                        grid.ToolTip += "\r\n1 = 1st Cycle";
                }

                if (firstCycleManuallyClassified) {
                    label.Text += "*";
                }
            }

            if (showHasNotes) {
                label = new TextBlock();
                label.Text = "Notes";
                label.Margin = new Thickness(6, 0, 0, 0);
                label.FontWeight = fontWeight;
                label.HorizontalAlignment = HorizontalAlignment.Left;
                label.VerticalAlignment = VerticalAlignment.Center;
                stackPanel.Children.Add(label);

                if (singleRow) {
                    stackPanel.Orientation = Orientation.Horizontal;
                    label.Text = "N";
                    grid.ToolTip += "\r\nN = Has User Notes";
                }
            }

            Grid.SetColumn(stackPanel, 1);
            grid.Children.Add(stackPanel);

            this.Content = grid;
        }
    }

    public static class TwGui {

        public static ImageSource GetIcon32() {
            string uri = @"pack://application:,,,/Images/" + "TraceWizard.ico";
            using (Stream iconStream = Application.GetResourceStream(new Uri(uri)).Stream) {
                using (System.Drawing.Icon icon = new System.Drawing.Icon(iconStream, 32, 32)) {
                    using (System.Drawing.Bitmap bitmap = icon.ToBitmap()) {
                        MemoryStream memoryStream = new MemoryStream();
                        bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        PngBitmapDecoder pbd = new PngBitmapDecoder(memoryStream,

                        BitmapCreateOptions.None, BitmapCacheOption.Default);
                        return pbd.Frames[0];
                    }
                }
            }
        }

        public static ImageSource GetImage(string fileName) {
            if (string.IsNullOrEmpty(fileName))
                return null;
            Uri uri = new Uri("pack://application:,,,/Images/" + fileName);
            return BitmapFrame.Create(uri);
        }

        public static UIElement CreateAnalyzerToolTip(Type type, Analyzer analyzer) {
            if (typeof(FixtureListClassifier).IsAssignableFrom(type))
                return CreateTitledFixtureProfiles((FixtureListClassifier)(analyzer));
            else {
                TextBlock textBlock = new TextBlock();
                textBlock.TextWrapping = TextWrapping.Wrap;
                textBlock.Text = analyzer.GetDescription();
                return textBlock;
            }
        }
        static UIElement CreateTitledFixtureProfiles(FixtureListClassifier classifier) {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Vertical;

            TextBlock textBlock = new TextBlock();
            textBlock.Text = classifier.GetDescription();
            stackPanel.Children.Add(textBlock);

            var fixtureProfilesPanel = new FixtureProfilesPanel();
            fixtureProfilesPanel.ItemsSource = classifier.FixtureProfiles;
            stackPanel.Children.Add(fixtureProfilesPanel);

            return stackPanel;
        }

        public static Grid FixtureWithImageLeftMenu(FixtureClass fixtureClass) {
            Grid grid = new Grid();
            grid.Margin = new Thickness(1, 0, 1, 0);

            grid.Tag = fixtureClass;

            ColumnDefinition columnDefinition = new ColumnDefinition();
            columnDefinition.Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(columnDefinition);

            columnDefinition = new ColumnDefinition();
            columnDefinition.Width = new GridLength(1, GridUnitType.Star);
            grid.ColumnDefinitions.Add(columnDefinition);

            Image image = new Image();
            image.Style = (Style)ResourceLocator.FindResource("ImageStyle");
            image.Source = TwGui.GetImage(fixtureClass.ImageFilename);

            Border border = new Border();
            border.Style = (Style)ResourceLocator.FindResource("FixtureBorderStyle");
            border.Background = TwBrushes.FrozenSolidColorBrush(fixtureClass.Color);
            border.Child = image;

            Grid.SetColumn(border, 0);
            grid.Children.Add(border);

            var label = new Label();
            label.Content = fixtureClass.LabelName;
            label.FontWeight = FontWeights.Normal;
            label.HorizontalAlignment = HorizontalAlignment.Left;
            label.VerticalAlignment = VerticalAlignment.Top;
            label.Padding = new Thickness(1, 0, 1, 0);
            label.Margin = new Thickness(5, 0, 5, 0);

            Grid.SetColumn(label, 1);
            grid.Children.Add(label);

            return grid;
        }

        public static StyledFixtureLabel FixtureWithImageLeft(FixtureClass fixtureClass) {
            return FixtureWithImageLeft(fixtureClass, FontWeights.Normal);
        }

        public static StyledFixtureLabel FixtureWithImageLeft(FixtureClass fixtureClass, bool showKey) {
            return FixtureWithImageLeft(fixtureClass, FontWeights.Normal, showKey);
        }

        public static StyledFixtureLabel FixtureWithImageLeft(FixtureClass fixtureClass, FontWeight fontWeight) {
            return FixtureWithImageLeft(fixtureClass, fontWeight, false);
        }

        public static Border FixtureImage(FixtureClass fixtureClass) {
            Image image = new Image();
            image.Style = (Style)ResourceLocator.FindResource("ImageStyle");
            image.Source = TwGui.GetImage(fixtureClass.ImageFilename);

            Border border = new Border();
            border.Style = (Style)ResourceLocator.FindResource("FixtureBorderStyle");
            border.Background = TwSingletonBrushes.Instance.FrozenSolidColorBrush(fixtureClass.Color); 
            border.Child = image;

            return border;
        }

        public static StyledFixtureLabel FixtureWithImageLeft(FixtureClass fixtureClass, FontWeight fontWeight, bool showKey) {
            return FixtureWithImageLeft(fixtureClass, fontWeight, showKey, false, false);
        }

        public static StyledFixtureLabel FixtureWithImageLeft(FixtureClass fixtureClass, FontWeight fontWeight, bool showKey, bool manuallyClassified, bool firstCycleManuallyClassified) {
            return new StyledFixtureLabel(fixtureClass, fontWeight, showKey, manuallyClassified, false, false, true, false, false);
        }
        
        public static Grid FixtureWithImageRight(FixtureClass fixtureClass) {
            Grid grid = new Grid();
            grid.Margin = new Thickness(1, 1, 1, 1);

            var coldefImage = new ColumnDefinition();
            coldefImage.Width = new GridLength(1, GridUnitType.Star);
            grid.ColumnDefinitions.Add(coldefImage);

            var coldefText = new ColumnDefinition();
            coldefText.Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(coldefText);

            Image image = new Image();
            image.Style = (Style)ResourceLocator.FindResource("ImageStyle");
            image.Source = TwGui.GetImage(fixtureClass.ImageFilename);

            Border border = new Border();
            border.Style = (Style)ResourceLocator.FindResource("FixtureBorderStyle");
            border.Background = TwBrushes.FrozenSolidColorBrush(fixtureClass.Color);
            border.Child = image;

            Grid.SetColumn(border, 1);
            grid.Children.Add(border);

            TextBlock textBlock = new TextBlock();
            textBlock.Text = fixtureClass.FriendlyName;
            textBlock.Margin = new Thickness(0, 0, 6, 0);
            textBlock.TextAlignment = TextAlignment.Right;
            textBlock.HorizontalAlignment = HorizontalAlignment.Right;
            textBlock.VerticalAlignment = VerticalAlignment.Center;

            Grid.SetColumn(textBlock, 0);
            grid.Children.Add(textBlock);

            return grid;
        }

        public static Grid FixtureWithNoImageRight(string text) {
            Grid grid = new Grid();
            grid.Margin = new Thickness(1, 1, 1, 1);

            var coldefImage = new ColumnDefinition();
            coldefImage.Width = new GridLength(1, GridUnitType.Star);
            grid.ColumnDefinitions.Add(coldefImage);

            var coldefText = new ColumnDefinition();
            coldefText.Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(coldefText);

            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Margin = new Thickness(0, 0, 6, 0);
            textBlock.TextAlignment = TextAlignment.Right;
            textBlock.HorizontalAlignment = HorizontalAlignment.Right;
            textBlock.VerticalAlignment = VerticalAlignment.Center;

            Grid.SetColumn(textBlock, 0);
            grid.Children.Add(textBlock);

            return grid;
        }

        public static Grid FixtureWithNoImageBottom(string text) {
            Grid grid = new Grid();
            grid.Margin = new Thickness(1, 1, 1, 1);

            RowDefinition rowdef = new RowDefinition();
            rowdef.Height = new GridLength(1, GridUnitType.Star);
            grid.RowDefinitions.Add(rowdef);

            rowdef = new RowDefinition();
            rowdef.Height = GridLength.Auto;
            grid.RowDefinitions.Add(rowdef);

            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Margin = new Thickness(0, 0, 0, 6);
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Bottom;

            textBlock.LayoutTransform = new RotateTransform(-90);

            Grid.SetRow(textBlock, 0);
            Grid.SetColumn(textBlock, 0);
            grid.Children.Add(textBlock);

            return grid;
        }

        public static Grid FixtureWithNoLabel(FixtureClass fixtureClass) {
            Grid grid = new Grid();
            grid.Margin = new Thickness(1, 1, 1, 1);

            RowDefinition rowdef = new RowDefinition();
            rowdef.Height = new GridLength(1, GridUnitType.Star);
            grid.RowDefinitions.Add(rowdef);

            rowdef = new RowDefinition();
            rowdef.Height = GridLength.Auto;
            grid.RowDefinitions.Add(rowdef);

            Image image = new Image();
            image.Style = (Style)ResourceLocator.FindResource("ImageStyle");
            image.Source = TwGui.GetImage(fixtureClass.ImageFilename);

            Border border = new Border();
            border.Style = (Style)ResourceLocator.FindResource("FixtureBorderStyle");
            border.Background = TwBrushes.FrozenSolidColorBrush(fixtureClass.Color);
            border.Child = image;

            Grid.SetRow(border, 1);
            grid.Children.Add(border);

            grid.ToolTip = fixtureClass.FriendlyName;

            return grid;
        }
    }
}
