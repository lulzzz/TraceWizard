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

using System.ComponentModel;

using TraceWizard.Entities;
using TraceWizard.Notification;

namespace TraceWizard.TwApp {
    public partial class ShortFixtureLabel : UserControl {

        public bool CanDrag = false;
        
        bool manuallyClassified;
        public bool ManuallyClassified {
            get { return manuallyClassified; }
            set {
                manuallyClassified = value;
                Initialize();
            }
        }

        bool firstCycle;
        public bool FirstCycle {
            get { return firstCycle; }
            set {
                firstCycle = value;
                Initialize();
                LabelFirstCycle.Visibility = firstCycle ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        bool firstCycleManuallyClassified;
        public bool FirstCycleManuallyClassified {
            get { return firstCycleManuallyClassified; }
            set {
                firstCycleManuallyClassified = value;
                Initialize();
            }
        }

        FixtureClass fixtureClass;
        public FixtureClass FixtureClass {
            get { return fixtureClass; }
            set {
                fixtureClass = value;
                Initialize();
            }
        }

        string label;
        public string Label {
            get { return label; }
            set {
                label = value;
                Initialize();
            }
        }

        HorizontalAlignment horizontalImageAlignment = HorizontalAlignment.Center;
        public HorizontalAlignment HorizontalImageAlignment {
            get { return horizontalImageAlignment; }
            set {
                horizontalImageAlignment = value;
                Initialize();
            }
        }

        public ShortFixtureLabel() {
            InitializeComponent();
        }

        public ShortFixtureLabel(FixtureClass fixtureClass) 
            : this(fixtureClass, false, false, false) {}

        public ShortFixtureLabel(FixtureClass fixtureClass, bool manuallyClassified, bool firstCycle, bool firstCycleManuallyClassified) {
            InitializeComponent();
            ManuallyClassified = manuallyClassified;
            FixtureClass = fixtureClass;
            FirstCycle = firstCycle;
            FirstCycleManuallyClassified = firstCycleManuallyClassified;
        }

        void Initialize() {
            if (FixtureClass != null) {
                if (CanDrag) {
                    this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(previewMouseLeftButtonDown);
                    this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(previewMouseLeftButtonUp);
                    this.PreviewMouseMove += new MouseEventHandler(previewMouseMove);
                }

                Image.Source = TwGui.GetImage(FixtureClass.ImageFilename);
                Border.Background = TwBrushes.FrozenSolidColorBrush(FixtureClass.Color);
                Tag = FixtureClass;
                LabelFixtureName.Text = FixtureClass.ShortName;

                if (manuallyClassified) {
                    LabelFixtureName.Text += "*";
                    LabelFixtureName.ToolTip = "* Manually classified as " + fixtureClass.FriendlyName + " by user. Will not be overridden by machine classification.";
                }

                LabelFirstCycle.ToolTip = "Manually classified as 1st Cycle";

                if (firstCycleManuallyClassified) {
                    LabelFirstCycle.Text += "*";
                    LabelFirstCycle.ToolTip = "* Manually classified as 1st Cycle by user. Will not be overridden by machine classification.";
                }

            } else if (!string.IsNullOrEmpty(Label))
                LabelFixtureName.Text = Label;
            LabelFirstCycle.Visibility = firstCycle ? Visibility.Visible : Visibility.Collapsed;
            Border.HorizontalAlignment = HorizontalImageAlignment;
            LabelFixtureName.HorizontalAlignment = HorizontalImageAlignment;
        }

        Point startPoint = new Point();
        void previewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            startPoint = e.GetPosition(null);
            originatedMouseDown = true;
        }
        void previewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            originatedMouseDown = false;
        }

        bool originatedMouseDown = false;

        public bool CanStartDragging = true;

        void previewMouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed && CanStartDragging && originatedMouseDown) {
                Point mousePos = e.GetPosition(null);
                Vector diff = startPoint - mousePos;
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    OnPropertyChanged(TwNotificationProperty.OnStartDrag);
                    DragDrop.DoDragDrop(this, new DataObject(this.GetType(), this), DragDropEffects.All);
                    OnPropertyChanged(TwNotificationProperty.OnEndDrag);
                    originatedMouseDown = false;
                }
            }
            e.Handled = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
