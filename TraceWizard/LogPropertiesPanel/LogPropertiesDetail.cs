using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.Windows.Shapes;

using System.Collections.Generic;
using System.ComponentModel;

using TraceWizard.Entities;
using TraceWizard.Classification;
using TraceWizard.Logging;
using TraceWizard.Logging.Adapters;

namespace TraceWizard.TwApp {

    public class LogPropertiesDetail : Grid {

        public LogMeter Log;

        void BuildRow(int row, string label, string value) {
            BuildRow(row, label, value, true, false); 
        }

        void BuildRow(int row, string label, string value, bool showSeparator, bool bold) {
            TextBlock txt;

            txt = new TextBlock();
            txt.Padding = new Thickness(0, 0, 6, 0);
            txt.Text = label;
            txt.FontWeight = bold ? FontWeights.Bold : FontWeights.Normal;
            if (showSeparator) {
                txt.Text += ":";
            }
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 0);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Text = value;
            txt.FontWeight = bold ? FontWeights.Bold : FontWeights.Normal;
            Grid.SetRow(txt, row++);
            Grid.SetColumn(txt, 1);
            this.Children.Add(txt);
        }

        void BuildRow(int row, string label, int? value) {
            TextBlock txt;

            txt = new TextBlock();
            txt.Padding = new Thickness(0, 0, 6, 0);
            txt.Text = label + ":";
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 0);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Text = value.HasValue ? value.Value.ToString() : string.Empty;
            Grid.SetRow(txt, row++);
            Grid.SetColumn(txt, 1);
            this.Children.Add(txt);
        }

        void BuildRow(int row, string label, double? value) {
            BuildRow(row, label, value, 2);
        }

        void BuildRow(int row, string label, double? value, int precision) {
            TextBlock txt;

            txt = new TextBlock();
            txt.Padding = new Thickness(0, 0, 6, 0);
            txt.Text = label + ":";
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 0);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Text = value.HasValue ? value.Value.ToString("0.".PadRight(precision + 2,'0')) : string.Empty;
            Grid.SetRow(txt, row++);
            Grid.SetColumn(txt, 1);
            this.Children.Add(txt);
        }

        void BuildRow(int row, string label, bool? value) {
            TextBlock txt;

            txt = new TextBlock();
            txt.Padding = new Thickness(0, 0, 6, 0);
            txt.Text = label + ":";
            txt.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(txt, row);
            Grid.SetColumn(txt, 0);
            this.Children.Add(txt);

            txt = new TextBlock();
            txt.Text = value.HasValue ? value.Value.ToString() : string.Empty;
            Grid.SetRow(txt, row++);
            Grid.SetColumn(txt, 1);
            this.Children.Add(txt);
        }

        public LogPropertiesDetail() {
            HorizontalAlignment = HorizontalAlignment.Left;
        }

        public void Initialize() {
 
            for (int i = 0; i < 35; i++) this.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < 2; i++) this.ColumnDefinitions.Add(new ColumnDefinition());

            int row = 0;

            BuildRow(row++, "Log Property", "Log Value", false, true);

            BuildRow(row++, Logging.Log.StartTimeLabel, Log.StartTime == DateTime.MinValue ? string.Empty : Log.StartTime.ToString());
            BuildRow(row++, Logging.Log.EndTimeLabel, Log.EndTime == DateTime.MinValue ? string.Empty : Log.EndTime.ToString());

            BuildRow(row++, Logging.Log.FileNameLabel, System.IO.Path.GetFileName(Log.FileName));

            BuildRow(row++, LogMeterCustomer.IdLabel, Log.Customer.ID);
            BuildRow(row++, LogMeterCustomer.NameLabel, Log.Customer.Name);
            BuildRow(row++, LogMeterCustomer.AddressLabel, Log.Customer.Address);
            BuildRow(row++, LogMeterCustomer.CityLabel, Log.Customer.City);
            BuildRow(row++, LogMeterCustomer.StateLabel, Log.Customer.State);
            BuildRow(row++, LogMeterCustomer.PostalCodeLabel, Log.Customer.PostalCode);
            BuildRow(row++, LogMeterCustomer.PhoneNumberLabel, Log.Customer.PhoneNumber);
            BuildRow(row++, LogMeterCustomer.NoteLabel, Log.Customer.Note);

            BuildRow(row++, LogMeterMeter.CodeLabel, Log.Meter.Code);
            BuildRow(row++, LogMeterMeter.MakeLabel, Log.Meter.Make);
            BuildRow(row++, LogMeterMeter.ModelLabel, Log.Meter.Model);
            BuildRow(row++, LogMeterMeter.SizeLabel, Log.Meter.Size);
            BuildRow(row++, LogMeterMeter.UnitLabel, Log.Meter.Unit);
            BuildRow(row++, LogMeterMeter.NutationLabel, Log.Meter.Nutation,8);
            BuildRow(row++, LogMeterMeter.LedLabel, Log.Meter.Led);
            BuildRow(row++, LogMeterMeter.StorageIntervalLabel, Log.Meter.StorageInterval);
            BuildRow(row++, LogMeterMeter.NumberOfIntervalsLabel, Log.Meter.NumberOfIntervals);
            BuildRow(row++, LogMeterMeter.TotalTimeLabel, Log.Meter.TotalTime);
            BuildRow(row++, LogMeterMeter.TotalPulsesLabel, Log.Meter.TotalPulses);

            BuildRow(row++, LogMeterMeter.BeginReadingLabel, Log.Meter.BeginReading);
            BuildRow(row++, LogMeterMeter.EndReadingLabel, Log.Meter.EndReading);
            BuildRow(row++, LogMeterMeter.RegisterVolumeLabel, Log.Meter.RegisterVolume);
            BuildRow(row++, LogMeterMeter.MeterMasterVolumeLabel, Log.Meter.MeterMasterVolume);

            BuildRow(row++, LogMeterMeter.ConversionFactorTypeLabel, Log.Meter.ConversionFactorType);
            BuildRow(row++, LogMeterMeter.ConversionFactorLabel, Log.Meter.ConversionFactor);

            BuildRow(row++, LogMeterMeter.DatabaseMultiplierLabel, Log.Meter.DatabaseMultiplier);
            BuildRow(row++, LogMeterMeter.CombinedFileLabel, Log.Meter.CombinedFile);

            BuildRow(row++, LogMeterMeter.DoublePulseLabel, Log.Meter.DoublePulse);
        }
    }
}

