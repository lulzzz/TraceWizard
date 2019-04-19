using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using TraceWizard.Entities;

namespace TraceWizard.TwApp {
    public partial class VerticalRuler : UserControl {
        public Events Events;
        public string Units = null;

        public VerticalRuler() {
            InitializeComponent();
        }
 
        public void Initialize() {
            this.Background = TwBrushes.BrushFromColor(Properties.Settings.Default.GraphRulerColor);
        }

        public void Clear() {
            Canvas.Children.Clear();
        }

        public int MaximumVolumeRows { get { return (int)Math.Ceiling(Events.SuperPeak); } }

        string UnitsLabel() { return Units == null || Units == string.Empty ? null : Units.ToLower().Substring(0, 1); }

        public void RenderRows(double heightMultiplier,
            bool showTwentyFiveVolumeTicks, bool showTwentyFiveVolumeLabel, 
            bool showFiveVolumeTicks, bool showFiveVolumeLabel, 
            bool showOneVolumeTicks, bool showOneVolumeLabel, int blankRows) {

            int numRows = MaximumVolumeRows;

            Brush brush = Brushes.Black;

            for (int i = 1; i < numRows; i++) {
                if (showTwentyFiveVolumeTicks && i % 25 == 0) {
                    RenderRowTick(i, brush, 1.0, heightMultiplier, blankRows);
                    if (showTwentyFiveVolumeLabel)
                        RenderRowLabel(i, i.ToString() + UnitsLabel(), Brushes.Black, heightMultiplier, blankRows);
                } else if (showFiveVolumeTicks && i % 5 == 0) {
                    RenderRowTick(i, brush, 1.0, heightMultiplier, blankRows);
                    if (showFiveVolumeLabel)
                        RenderRowLabel(i, i.ToString() + UnitsLabel(), Brushes.Black, heightMultiplier, blankRows);
                } else if (showOneVolumeTicks) {
                    RenderRowTick(i, brush, 1.0, heightMultiplier, blankRows);
                    if (showOneVolumeLabel)
                        RenderRowLabel(i, i.ToString() + UnitsLabel(), Brushes.Black, heightMultiplier, blankRows);
                }
            }
        }

        private int RowsOffset() {
            return 0;
        }
        
        public void RenderRowTick(int row, Brush brush, double thickness, double heightMultiplier, int blankRows) {
            LineGeometry line = new LineGeometry();
            line.StartPoint = new Point(Canvas.Width - 4, (MaximumVolumeRows + blankRows - row) * heightMultiplier);
            line.EndPoint = new Point(Canvas.Width, (MaximumVolumeRows + blankRows - row) * heightMultiplier);
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            line.Freeze();

            Path path = new Path();
            path.Stroke = brush;
            path.StrokeThickness = thickness;
            path.SnapsToDevicePixels = false;

            path.Data = line;
            System.Windows.Controls.Canvas.SetZIndex(path, -1);
            this.Canvas.Children.Add(path);
        }

        public void RenderRowLabel(int row, string text, Brush foreground, double heightMultiplier, int blankRows) {

            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.FontFamily = new FontFamily("Courier New");
            textBlock.FontSize = 11;
            textBlock.Foreground = foreground;
            textBlock.Padding = new Thickness(2);

            System.Windows.Controls.Canvas.SetTop(textBlock, ((MaximumVolumeRows + blankRows - row) * heightMultiplier) - 8);
            System.Windows.Controls.Canvas.SetRight(textBlock, 4);
            this.Canvas.Children.Add(textBlock);
        }
    }
}
