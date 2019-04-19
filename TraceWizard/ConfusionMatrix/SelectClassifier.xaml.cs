using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using TraceWizard.Entities;
using TraceWizard.Classification;
using TraceWizard.Classification.Classifiers.FixtureList;

using TraceWizard.Environment;

namespace TraceWizard.TwApp {
    public partial class ClassifierSelector : Window {

        public List<Classifier> Classifiers = new List<Classifier>();
        
        public ClassifierSelector() {
            InitializeComponent();
            Loaded += new RoutedEventHandler(loaded);
            OkButton.Click +=new RoutedEventHandler(okButton_Click);
        }

        void loaded(object sender, RoutedEventArgs e) {
            Populate();
        }

        void Populate() {
            var classifiers = TwClassifiers.CreateClassifiers();

            for (int i = 0; i < classifiers.Count; i++){
                var classifier = classifiers[i];

                int column = 0;
                grid.RowDefinitions.Add(new RowDefinition());
                
                var button = new CheckBox();
                button.Padding = new Thickness(4,0,4,4);
                button.Margin = new Thickness(4);
                button.VerticalAlignment = VerticalAlignment.Top;
                button.VerticalContentAlignment = VerticalAlignment.Top;
                button.Tag = classifier;
                button.Content = classifier.Name;

                button.ToolTip = TwGui.CreateAnalyzerToolTip(classifier.GetType(), classifier);

                if (!TwClassifiers.CanLoad(classifier) || classifier is FixtureListClassifier)
                    button.IsEnabled = false;

                Grid.SetRow(button, i);
                Grid.SetColumn(button, column++);
                grid.Children.Add(button);

                var description = new TextBlock();
                description.TextWrapping = TextWrapping.Wrap;
                description.MaxWidth = 200;
                description.Padding = new Thickness(4,0,4,4);
                description.Margin = new Thickness(4);
                description.VerticalAlignment = VerticalAlignment.Top;
                description.Text = classifier.Description;
                
                Grid.SetRow(description, i);
                Grid.SetColumn(description, column++);
                grid.Children.Add(description);
            }
        }

        void okButton_Click(object sender, System.Windows.RoutedEventArgs e) {

            foreach (var item in grid.Children) {
                var button = item as CheckBox;
                if (button != null && button.IsChecked.Value == true)
                    Classifiers.Add((Classifier)button.Tag);
            }

            Close();
        }
    }
}
