using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using TraceWizard.Entities;

namespace TraceWizard.TwApp {
    public partial class ProjectReportSelector : Window {

        public ProjectReportAttributes Attributes { get; set; }

        public ProjectReportSelector(ProjectReportAttributes attributes)
            : this() {
            Attributes = attributes;
        }

        public ProjectReportSelector() {
            InitializeComponent();
            Loaded += new RoutedEventHandler(ArffSelector_Loaded);

        }

        void ArffSelector_Loaded(object sender, RoutedEventArgs e) {
            CreateAttributes();
        }

        void okButton_Click(object sender, System.Windows.RoutedEventArgs e) {

            Attributes.IsPartialDaysEnabled = IsAttributeEnabled(ProjectReportExporter.PartialDaysLabel);
            Attributes.IsExcludeNoiseEnabled = IsAttributeEnabled(ProjectReportExporter.ExcludeNoiseLabel);
            Attributes.IsExcludeDuplicateEnabled = IsAttributeEnabled(ProjectReportExporter.ExcludeDuplicateLabel);
            Attributes.IsOutdoorPoolEnabled = IsAttributeEnabled(ProjectReportExporter.OutdoorPoolLabel);
            Attributes.IsOutdoorCoolerEnabled = IsAttributeEnabled(ProjectReportExporter.OutdoorCoolerLabel);

            Attributes.IsKeyCodeEnabled = IsAttributeEnabled(ProjectReportExporter.KeyCodeLabel);

            Attributes.IsTotalVolumeEnabled = IsAttributeEnabled(ProjectReportExporter.TotalVolumeLabel);

            Attributes.IsTraceBeginsEnabled = IsAttributeEnabled(ProjectReportExporter.TraceBeginsLabel);
            Attributes.IsTraceEndsEnabled = IsAttributeEnabled(ProjectReportExporter.TraceEndsLabel);
            Attributes.IsTraceLengthDaysEnabled = IsAttributeEnabled(ProjectReportExporter.TraceLengthDaysLabel);

            Attributes.IsTotalGpdEnabled = IsAttributeEnabled(ProjectReportExporter.TotalGpdLabel);
            Attributes.IsIndoorGpdEnabled = IsAttributeEnabled(ProjectReportExporter.IndoorGpdLabel);
            Attributes.IsOutdoorGpdEnabled = IsAttributeEnabled(ProjectReportExporter.OutdoorGpdLabel);

            Attributes.IsIndoorTotalGalEnabled = IsAttributeEnabled(ProjectReportExporter.IndoorTotalGalLabel);
            Attributes.IsOutdoorTotalGalEnabled = IsAttributeEnabled(ProjectReportExporter.OutdoorTotalGalLabel);

            Attributes.IsTotalGallonsColumnsEnabled = IsAttributeEnabled(ProjectReportExporter.TotalGallonsColumnsLabel);
            Attributes.IsEventsColumnsEnabled = IsAttributeEnabled(ProjectReportExporter.EventsColumnsLabel);
            Attributes.IsGallonsPerDayColumnsEnabled = IsAttributeEnabled(ProjectReportExporter.GallonsPerDayColumnsLabel);

            Attributes.IsClotheswasherDetailColumnsEnabled = IsAttributeEnabled(ProjectReportExporter.ClotheswasherDetailColumnsLabel);
            Attributes.IsShowerDetailColumnsEnabled = IsAttributeEnabled(ProjectReportExporter.ShowerDetailColumnsLabel);
            Attributes.IsToiletDetailColumnsEnabled = IsAttributeEnabled(ProjectReportExporter.ToiletDetailColumnsLabel);

            DialogResult = true;
            this.Close();
        }

        bool IsAttributeEnabled(string attribute) {
            foreach (CheckBox checkBox in stackPanelAttributes.Children) {
                if ((string)checkBox.Tag == attribute)
                    return checkBox.IsChecked.Value;
            }
            foreach (CheckBox checkBox in stackPanelSettings.Children) {
                if ((string)checkBox.Tag == attribute)
                    return checkBox.IsChecked.Value;
            }
            return false;
        }

        void CreateAttributes() {
            stackPanelSettings.Children.Add(CreateAttribute(ProjectReportExporter.PartialDaysLabel, false));
            stackPanelSettings.Children.Add(CreateAttribute(ProjectReportExporter.ExcludeNoiseLabel, true));
            stackPanelSettings.Children.Add(CreateAttribute(ProjectReportExporter.ExcludeDuplicateLabel, true));
            stackPanelSettings.Children.Add(CreateAttribute(ProjectReportExporter.OutdoorPoolLabel, true));
            stackPanelSettings.Children.Add(CreateAttribute(ProjectReportExporter.OutdoorCoolerLabel, true));

            stackPanelAttributes.Children.Add(CreateAttribute(ProjectReportExporter.KeyCodeLabel, true));

            stackPanelAttributes.Children.Add(CreateAttribute(ProjectReportExporter.TotalVolumeLabel, true));

            stackPanelAttributes.Children.Add(CreateAttribute(ProjectReportExporter.TraceBeginsLabel, true));
            stackPanelAttributes.Children.Add(CreateAttribute(ProjectReportExporter.TraceEndsLabel, true));
            stackPanelAttributes.Children.Add(CreateAttribute(ProjectReportExporter.TraceLengthDaysLabel, true));

            stackPanelAttributes.Children.Add(CreateAttribute(ProjectReportExporter.TotalGpdLabel, true));
            stackPanelAttributes.Children.Add(CreateAttribute(ProjectReportExporter.IndoorGpdLabel, true));
            stackPanelAttributes.Children.Add(CreateAttribute(ProjectReportExporter.OutdoorGpdLabel, true));

            stackPanelAttributes.Children.Add(CreateAttribute(ProjectReportExporter.IndoorTotalGalLabel, true));
            stackPanelAttributes.Children.Add(CreateAttribute(ProjectReportExporter.OutdoorTotalGalLabel, true));

            stackPanelAttributes.Children.Add(CreateAttribute(ProjectReportExporter.TotalGallonsColumnsLabel, true));
            stackPanelAttributes.Children.Add(CreateAttribute(ProjectReportExporter.EventsColumnsLabel, true));
            stackPanelAttributes.Children.Add(CreateAttribute(ProjectReportExporter.GallonsPerDayColumnsLabel, true));

            stackPanelAttributes.Children.Add(CreateAttribute(ProjectReportExporter.ClotheswasherDetailColumnsLabel, true));
            stackPanelAttributes.Children.Add(CreateAttribute(ProjectReportExporter.ShowerDetailColumnsLabel, true));
            stackPanelAttributes.Children.Add(CreateAttribute(ProjectReportExporter.ToiletDetailColumnsLabel, true));
        }

        UIElement CreateAttribute(string label, bool isChecked) {
            var checkBox = new CheckBox();
            checkBox.IsChecked = isChecked;
            checkBox.Content = label;
            checkBox.Tag = label;
            checkBox.Margin = new Thickness(3);

            return checkBox;
        }
    }
}
