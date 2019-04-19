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

    public partial class FixtureClassSelector : UserControl {
        public FixtureClassSelector() {
            InitializeComponent();

            ComboBoxFixtureClass.SelectionChanged +=new SelectionChangedEventHandler(ComboBoxFixtureClass_SelectionChanged);

            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values) {
                ComboBoxFixtureClass.Items.Add(new StyledFixtureLabel(fixtureClass,FontWeights.Normal,false,false,false,false,false,true,false));
            }
        }

        void ComboBoxFixtureClass_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            FixtureClass = ((StyledFixtureLabel)(ComboBoxFixtureClass.SelectedItem)).FixtureClass;
        }
        
        public FixtureClass FixtureClass {
            get { return ((StyledFixtureLabel)(ComboBoxFixtureClass.SelectedItem)).FixtureClass; }
            set { ComboBoxFixtureClass.SelectedItem = ItemFromFixtureClass(value); 
            }
        }

        FrameworkElement ItemFromFixtureClass(FixtureClass fixtureClass) {
            foreach (StyledFixtureLabel item in ComboBoxFixtureClass.Items) {
                if (item.FixtureClass == fixtureClass) {
                    return item;
                }
            }
            return null;
        }
    }
}
