using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using TraceWizard.Entities;
using TraceWizard.Entities.Adapters.Arff;

namespace TraceWizard.TwApp {
    public partial class ArffSelector : Window {

        public ArffAttributes Attributes {get;set;}

        public ArffSelector(ArffAttributes attributes) : this() {
            Attributes = attributes;
        }

        public ArffSelector() {
            InitializeComponent();
            Loaded += new RoutedEventHandler(ArffSelector_Loaded);

        }

        void ArffSelector_Loaded(object sender, RoutedEventArgs e) {
            CreateFixtureClasses();
            CreateAttributes();
        }

        void okButton_Click(object sender, System.Windows.RoutedEventArgs e) {

            Attributes.FixtureClasses = new List<FixtureClass>();
            foreach (CheckBox checkBox in stackPanelFixtureClasses.Children) {
                if (checkBox.IsChecked.Value == true)
                    Attributes.FixtureClasses.Add((FixtureClass)checkBox.Tag);
            }

            Attributes.IsKeyCodeEnabled = IsAttributeEnabled("Key Code");

            Attributes.IsEventIdEnabled = IsAttributeEnabled("Event ID");

            Attributes.IsStartTimeEnabled = IsAttributeEnabled("Start Time");
            Attributes.IsEndTimeEnabled = IsAttributeEnabled("End Time");
            Attributes.IsDurationEnabled = IsAttributeEnabled("Duration");

            Attributes.IsFirstCycleEnabled = IsAttributeEnabled("First Cycle?");

            Attributes.IsManuallyClassifiedEnabled = IsAttributeEnabled("Manually Classified?");

            Attributes.IsChannelEnabled = IsAttributeEnabled("Channel");

            Attributes.IsVolumeEnabled = IsAttributeEnabled("Volume");
            Attributes.IsPeakEnabled = IsAttributeEnabled("Peak");
            Attributes.IsModeEnabled = IsAttributeEnabled("Mode");
            Attributes.IsModeFrequencyEnabled = IsAttributeEnabled("Mode Frequency");

            Attributes.IsHourEnabled = IsAttributeEnabled("Hour");
            Attributes.IsIsWeekendEnabled = IsAttributeEnabled("Is Weekend");
            Attributes.IsTimeToLongerEventEnabled = IsAttributeEnabled("Time to Longer Event");
            
            DialogResult = true;
            this.Close();
        }

        bool IsAttributeEnabled(string attribute) {
            foreach (CheckBox checkBox in stackPanelAttributes.Children) {
                if ((string)checkBox.Tag == attribute)
                    return checkBox.IsChecked.Value;
            }
            return false;
        }

        void CreateFixtureClasses() {

            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values) {
                stackPanelFixtureClasses.Children.Add(CreateFixtureClass(fixtureClass));
            }
        }

        UIElement CreateFixtureClass(FixtureClass fixtureClass) {
            var checkBox = new CheckBox();
            checkBox.IsChecked = true;
//            checkBox.Content = fixtureClass.FriendlyName;
            checkBox.Content = TwGui.FixtureWithImageLeft(fixtureClass);
            checkBox.Tag = fixtureClass;
            checkBox.Margin = new Thickness(3);

            return checkBox;
        }

        void CreateAttributes() {
            stackPanelAttributes.Children.Add(CreateAttribute("Key Code"));

            stackPanelAttributes.Children.Add(CreateAttribute("Event ID"));

            stackPanelAttributes.Children.Add(CreateAttribute("Start Time"));
            stackPanelAttributes.Children.Add(CreateAttribute("End Time"));
            stackPanelAttributes.Children.Add(CreateAttribute("Duration"));

            stackPanelAttributes.Children.Add(CreateAttribute("First Cycle?"));

            stackPanelAttributes.Children.Add(CreateAttribute("Manually Classified?"));

            stackPanelAttributes.Children.Add(CreateAttribute("Channel"));

            stackPanelAttributes.Children.Add(CreateAttribute("Volume"));
            stackPanelAttributes.Children.Add(CreateAttribute("Peak"));
            stackPanelAttributes.Children.Add(CreateAttribute("Mode"));
            stackPanelAttributes.Children.Add(CreateAttribute("Mode Frequency"));
            stackPanelAttributes.Children.Add(CreateAttribute("Hour"));
            stackPanelAttributes.Children.Add(CreateAttribute("Is Weekend"));
            stackPanelAttributes.Children.Add(CreateAttribute("Time to Longer Event"));
        }

        UIElement CreateAttribute(string label) {
            var checkBox = new CheckBox();
            checkBox.IsChecked = true;
            checkBox.Content = label;
            checkBox.Tag = label;
            checkBox.Margin = new Thickness(3);

            return checkBox;
        }
    }
}
