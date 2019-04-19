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
using System.Windows.Controls.Primitives;

namespace TraceWizard.TwApp {
    public partial class TwStatusBar : UserControl {

        public TwStatusBar() {
            InitializeComponent();

            Properties.Settings.Default.PropertyChanged +=new PropertyChangedEventHandler(Default_PropertyChanged);

            Visibility = SetVisibility();
        }

        Visibility SetVisibility() {
            return Properties.Settings.Default.ShowStatusBar ? Visibility.Visible : Visibility.Collapsed;
        }

        public void Default_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "ShowStatusBar":
//                    Visibility = SetVisibility();
                    break;}
        }
    }
}
