using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using TraceWizard.FeatureLevels;
using TraceWizard.Environment;

namespace TraceWizard.TwApp {
    public partial class MainToolBar : UserControl {

        public FeatureLevel FeatureLevel;

        public MainToolBar() {
            InitializeComponent();
        }

        public void Initialize() {

            InitializeControls();

            if (!FeatureLevel.IsPro) {
                ButtonTools.Visibility = Visibility.Collapsed;

                foreach (Control item in ButtonHelp.ContextMenu.Items)
                    if (item.Name != "AboutHelpMenu")
                        item.Visibility = Visibility.Collapsed;
            }

            AboutHelpMenu.Header = "About " + TwAssembly.CompanyAndTitle();

            InitializeMouse();
        }

        void InitializeControls() {

            InitializeMenuButton(OpenDropDownButton);

            InitializeMenuButton(ButtonHelp);
            CommandManager.InvalidateRequerySuggested();

            InitializeMenuButton(ButtonReports);
            InitializeMenuButton(ButtonTools);

//            InitializeFullScreenButton();

        }

        void InitializeMouse() {
            this.AllowDrop = true;
            this.Drop += new DragEventHandler(dragDrop);
        }

        public MainTwWindow.Dispatch DispatchLoad;
        
        void dragDrop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (DispatchLoad != null)
                    DispatchLoad(files);
            }
        }

        [Obsolete]
        void InitializeFullScreenButton() {
            FullScreenButton.Click += new RoutedEventHandler(FullScreenButton_Click);
        }

        [Obsolete]
        void FullScreenButton_Click(object sender, RoutedEventArgs e) {
//            ((MainTwWindow)(Application.Current.MainWindow)).ToggleFullScreen();
        }


        void InitializeMenuButton(ButtonBase button) {
            button.ContextMenuOpening += new ContextMenuEventHandler(menuButton_ContextMenuOpening);
            button.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(menuButton_PreviewMouseLeftButtonUp);
        }

        void menuItem_ContextMenuOpening(object sender, ContextMenuEventArgs e) {
            var menuItem = sender as MenuItem;

            if (menuItem.ContextMenu != null)
                menuItem.ContextMenu.IsOpen = false;
            e.Handled = true;
        }

        void menuButton_ContextMenuOpening(object sender, ContextMenuEventArgs e) {
            var button = sender as Button;

            if (button.ContextMenu != null)
                button.ContextMenu.IsOpen = false;
            e.Handled = true;
        }

        void menuButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var button = sender as Button;
            OpenContextMenu(button);
        }

        public void OpenContextMenu(Button button) {
            if (button.ContextMenu != null) {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                ContextMenuService.SetPlacement(button, System.Windows.Controls.Primitives.PlacementMode.Bottom);
                button.ContextMenu.IsOpen = true;
            }
        }
    }
}
