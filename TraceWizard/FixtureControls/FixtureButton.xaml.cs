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
using TraceWizard.Notification;

using TraceWizard.Entities;

namespace TraceWizard.TwApp {

    public partial class FixtureButton : Button, INotifyPropertyChanged {

        public FixtureClass FixtureClass;

        public FixtureButton() {
            InitializeComponent();

            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(FixtureButton_PreviewMouseLeftButtonDown);
            this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(FixtureButton_PreviewMouseLeftButtonUp);
            this.PreviewMouseMove += new MouseEventHandler(FixtureButton_PreviewMouseMove);
        }

        public void Initialize() {
            Image.Source = TwGui.GetImage(FixtureClass.ImageFilename);
            this.Content = Image;
            this.Tag = FixtureClass;
            this.Background = TwBrushes.FrozenSolidColorBrush(FixtureClass.Color);
            this.Style = (Style)ResourceLocator.FindResource(FixtureClass.LowFrequency ? "ToolBarFixtureLowFrequencyButtonStyle" : "ToolBarFixtureButtonStyle");
            this.ToolTip = "Classify as " + FixtureClass.FriendlyName + " (" + FixtureClass.Character + ")";
            this.ContextMenu = FixtureButtonContextMenu();
            this.CommandParameter = FixtureClass;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        Point startPoint = new Point();
        void FixtureButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            startPoint = e.GetPosition(null);
            originatedMouseDown = true;
        }
        void FixtureButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            originatedMouseDown = false;
        }

        bool originatedMouseDown = false;

        public bool CanStartDragging = true;

        void FixtureButton_PreviewMouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed && CanStartDragging && originatedMouseDown) {
                Point mousePos = e.GetPosition(null);
                Vector diff = startPoint - mousePos;
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    OnPropertyChanged(TwNotificationProperty.OnStartDrag);
                    DragDrop.DoDragDrop(this, new DataObject(typeof(FixtureButton), this), DragDropEffects.All);
                    OnPropertyChanged(TwNotificationProperty.OnEndDrag);
                    originatedMouseDown = false;
                }
            }
            e.Handled = true;
        }

        ContextMenu FixtureButtonContextMenu() {
            var contextMenu = new ContextMenu();
            contextMenu.Items.Add(FixtureButtonContextMenuItem("Find Next", AnalysisPanel.FindNextCommand));
            contextMenu.Items.Add(FixtureButtonContextMenuItem("Find Previous", AnalysisPanel.FindPreviousCommand));
            return contextMenu;
        }

        MenuItem FixtureButtonContextMenuItem(string label, RoutedUICommand command) {
            var menuItem = new MenuItem();
            menuItem.Header = label + " " + FixtureClass.FriendlyName;
            var image = new Image();
            image.Source = TwGui.GetImage(FixtureClass.ImageFilename);
            var border = new Border();
            border.Padding = new Thickness(2);
            border.Background = TwBrushes.FrozenSolidColorBrush(FixtureClass.Color);
            border.Child = image;
            menuItem.Icon = border;
            menuItem.Command = command;
            menuItem.CommandParameter = FixtureClass;

            return menuItem;
        }
    }
}
