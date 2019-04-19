using System;
using System.Data;
using System.Data.OleDb;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using TraceWizard.Logging;
using TraceWizard.Entities;

using TraceWizard.Logging.Adapters;

namespace TraceWizard.Logging.Adapters {

    public class LogMeter : Log {
        public LogMeter(string dataSource) : base(dataSource) { }
        public LogMeter() : base() { }

        public LogMeterMeter Meter { get; set; }
        public LogMeterCustomer Customer { get; set; }
    }

    public class LogMeterMeter {

        public const string CodeLabel = "Code";
        public int? Code { get; set; }

        public const string MakeLabel = "Make";
        public string Make { get; set; }

        public const string ModelLabel = "Model";
        public string Model { get; set; }

        public const string SizeLabel = "Size";
        public string Size { get; set; }

        public const string UnitLabel = "Unit";
        public string Unit { get; set; }

        public const string NutationLabel = "Nutation";
        public double? Nutation { get; set; }

        public const string LedLabel = "LED";
        public double? Led { get; set; }

        public const string StorageIntervalLabel = "StorageInterval";
        public int? StorageInterval { get; set; }

        public const string NumberOfIntervalsLabel = "NumberOfIntervals";
        public int? NumberOfIntervals { get; set; }

        public const string TotalTimeLabel = "TotalTime";
        public string TotalTime { get; set; }

        public const string TotalPulsesLabel = "TotalPulses";
        public int? TotalPulses { get; set; }

        public const string BeginReadingLabel = "BeginReading";
        public double? BeginReading { get; set; }

        public const string EndReadingLabel = "EndReading";
        public double? EndReading { get; set; }

        public const string RegisterVolumeLabel = "RegVolume";
        public double? RegisterVolume { get; set; }

        public const string MeterMasterVolumeLabel = "MMVolume";
        public double? MeterMasterVolume { get; set; }

        public const string ConversionFactorTypeLabel = "ConvFactorType";
        public int? ConversionFactorType { get; set; }

        public const string ConversionFactorLabel = "ConvFactor";
        public double? ConversionFactor { get; set; }

        public const string DatabaseMultiplierLabel = "DatabaseMultiplier";
        public double? DatabaseMultiplier { get; set; }

        public const string CombinedFileLabel = "CombinedFile";
        public int? CombinedFile { get; set; }

        public const string DoublePulseLabel = "DoublePulse";
        public bool? DoublePulse { get; set; }
    }

    public class LogMeterCustomer {

        public const string IdLabel = "CustomerID";
        public string ID { get; set; }

        public const string NameLabel = "CustomerName";
        public string Name { get; set; }

        public const string AddressLabel = "Address";
        public string Address { get; set; }

        public const string CityLabel = "City";
        public string City { get; set; }

        public const string StateLabel = "State";
        public string State { get; set; }

        public const string PostalCodeLabel = "PostalCode";
        public string PostalCode { get; set; }

        public const string PhoneNumberLabel = "PhoneNumber";
        public string PhoneNumber { get; set; }

        public const string NoteLabel = "Note";
        public string Note { get; set; }

    }
}
