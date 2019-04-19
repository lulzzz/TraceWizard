using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Text;

using TraceWizard.Entities;
using TraceWizard.Environment;
using TraceWizard.Reporters;

namespace TraceWizard.TwApp {

    public class ProjectReportAttributes {
        public bool IsPartialDaysEnabled { get; set; }
        public bool IsExcludeNoiseEnabled { get; set; }
        public bool IsExcludeDuplicateEnabled { get; set; }
        public bool IsOutdoorPoolEnabled { get; set; }
        public bool IsOutdoorCoolerEnabled { get; set; }

        public List<FixtureClass> FixtureClasses { get; set; }

        public bool IsKeyCodeEnabled { get; set; }
        public bool IsTotalVolumeEnabled { get; set; }

        public bool IsTraceBeginsEnabled { get; set; }
        public bool IsTraceEndsEnabled { get; set; }
        public bool IsTraceLengthDaysEnabled { get; set; }

        public bool IsTotalGpdEnabled { get; set; }
        public bool IsIndoorGpdEnabled { get; set; }
        public bool IsOutdoorGpdEnabled { get; set; }

        public bool IsIndoorTotalGalEnabled { get; set; }
        public bool IsOutdoorTotalGalEnabled { get; set; }

        public bool IsTotalGallonsColumnsEnabled { get; set; }
        public bool IsEventsColumnsEnabled { get; set; }
        public bool IsGallonsPerDayColumnsEnabled { get; set; }

        public bool IsClotheswasherDetailColumnsEnabled { get; set; }
        public bool IsShowerDetailColumnsEnabled { get; set; }
        public bool IsToiletDetailColumnsEnabled { get; set; }
    }

    public class ProjectReportExporter : Exporter {

        public ProjectReportAttributes Attributes;
        public string DataSource;
        public ProjectReportProperties Properties;
        public string KeyCode;

        public const string PartialDaysLabel = "Partial Days";

        public const string ExcludeNoiseLabel = "Exclude Noise";
        public const string ExcludeDuplicateLabel = "Exclude Duplicate";
        public const string OutdoorPoolLabel = "Outdoor Pool";
        public const string OutdoorCoolerLabel = "Outdoor Cooler";

        public const string KeyCodeLabel = "Keycode";
        public const string TotalVolumeLabel = "Total Volume";

        public const string TraceBeginsLabel = "TraceBegins";
        public const string TraceEndsLabel = "TraceEnds";
        public const string TraceLengthDaysLabel = "Trace Length Days";

        public const string TotalGpdLabel = "Total GPD";
        public const string IndoorGpdLabel = "Indoor GPD";
        public const string OutdoorGpdLabel = "Outdoor GPD";

        public const string IndoorTotalGalLabel = "Indoor total gal";
        public const string OutdoorTotalGalLabel = "Outdoor total gal";

        public const string BathtubTotalGalLabel = "Bathtub total gal";
        public const string ClotheswasherTotalGalLabel = "Clotheswasher total gal";
        public const string CoolerTotalGalLabel = "Cooler total gal";
        public const string DishwasherTotalGalLabel = "Dishwasher total gal";
        public const string FaucetTotalGalLabel = "Faucet total gal";
        public const string LeakTotalGalLabel = "Leak total gal";
        public const string OtherTotalGalLabel = "Other total gal";
        public const string ShowerTotalGalLabel = "Shower total gal";
        public const string ToiletTotalGalLabel = "Toilet total gal";
        public const string TreatmentTotalGalLabel = "Treatment total gal";

        public const string BathtubEventsLabel = "Bathtub events";
        public const string ClotheswasherEventsLabel = "Clotheswasher events";
        public const string CoolerEventsLabel = "Cooler events";
        public const string DishwasherEventsLabel = "Dishwasher events";
        public const string FaucetEventsLabel = "Faucet events";
        public const string LeakEventsLabel = "Leak events";
        public const string OtherEventsLabel = "Other events";
        public const string ShowerEventsLabel = "Shower events";
        public const string ToiletEventsLabel = "Toilet events";
        public const string TreatmentEventsLabel = "Treatment events";
       
        public const string BathtubGpdLabel = "Bathtub gpd";
        public const string ClotheswasherGpdLabel = "Clotheswasher gpd";
        public const string CoolerGpdLabel = "Cooler gpd";
        public const string DishwasherGpdLabel = "Dishwasher gpd";
        public const string FaucetGpdLabel = "Faucet gpd";
        public const string LeakGpdLabel = "Leak gpd";
        public const string OtherGpdLabel = "Other gpd";
        public const string ShowerGpdLabel = "Shower gpd";
        public const string ToiletGpdLabel = "Toilet gpd";
        public const string TreatmentGpdLabel = "Treatment gpd";
        
        public const string AverageClotheswasherLoadGalLabel = "Average clotheswasher load gal";
        public const string ClotheswasherLoadsPerDayLabel = "Clotheswasher loads per day";

        public const string TotalShowerMinutesLabel = "Total shower minutes";
        public const string AverageShowerSecondsLabel = "Average shower seconds";
        public const string TotalShowerGalLabel = "Total shower gal";
        public const string AverageShowerGalLabel = "Average shower gal";
        public const string AverageShowerModeFlowGpmLabel = "Average shower mode flow gpm";
        public const string ShowersPerDayLabel = "Showers per day";
        public const string ShowerMinutesPerDayLabel = "Shower minutes per day";

        public const string AverageToiletFlushVolumeLabel = "Average toilet flush volume";
        public const string ToiletFlushStDevLabel = "Toilet flush stdev";
        public const string NumberOfFlushesLessThan2_2GalLabel = "Number of flushes less than 2_2 Gal";
        public const string NumberOfFlushesGreaterThan2_2GalLabel = "Number of flushes greater than 2_2 Gal";
        public const string FlushesPerDayLabel = "Flushes per day";

        public const string TotalGallonsColumnsLabel = "Total gallons columns";
        public const string EventsColumnsLabel = "Events columns";
        public const string GallonsPerDayColumnsLabel = "Gallons per day columns";

        public const string ClotheswasherDetailColumnsLabel = "Clotheswasher detail columns";
        public const string ShowerDetailColumnsLabel = "Shower detail columns";
        public const string ToiletDetailColumnsLabel = "Toilet detail columns";

        const string separator = "\t";
//        const string separator = ",";

        public ProjectReportExporter() { }

        public void Export() {

            if (!System.IO.File.Exists(DataSource)) {
                WriteHeader(DataSource);
            }
            WriteData(DataSource, Properties, KeyCode);
        }

        void WriteData(string dataSource, ProjectReportProperties properties, string keyCode) {
            System.Text.StringBuilder text = new System.Text.StringBuilder();
            WriteAnalysis(text, properties, keyCode);
            System.IO.File.AppendAllText(dataSource, text.ToString());
        }

        void AppendStringTab(StringBuilder text, string input) {
            text.Append(input + separator);
        }

        void AppendStringTab(StringBuilder text, double input) {
            if (double.IsNaN(input) || double.IsInfinity(input))
                text.Append(separator);
            else
                text.Append(input.ToString("0.00") + separator);
        }

        void AppendStringTab(StringBuilder text, int input) {
            text.Append(input.ToString() + separator);
        }

        void AppendStringTab(StringBuilder text, DateTime input) {
            text.Append(input.ToString("MM'/'dd'/'yyyy") + separator);
        }

        void WriteHeader(string dataSource) {
            System.Text.StringBuilder text = new System.Text.StringBuilder();

            if (Attributes.IsKeyCodeEnabled)
                AppendStringTab(text, KeyCodeLabel);

            if (Attributes.IsTotalVolumeEnabled)
                AppendStringTab(text, TotalVolumeLabel);

            if (Attributes.IsTraceBeginsEnabled)
                AppendStringTab(text, TraceBeginsLabel);

            if (Attributes.IsTraceEndsEnabled)
                AppendStringTab(text, TraceEndsLabel);

            if (Attributes.IsTraceLengthDaysEnabled)
                AppendStringTab(text, TraceLengthDaysLabel);

            if (Attributes.IsTotalGpdEnabled)
                AppendStringTab(text, TotalGpdLabel);

            if (Attributes.IsIndoorGpdEnabled)
                AppendStringTab(text, IndoorGpdLabel);

            if (Attributes.IsOutdoorGpdEnabled)
                AppendStringTab(text, OutdoorGpdLabel);
           
            if (Attributes.IsIndoorTotalGalEnabled)
                AppendStringTab(text, IndoorTotalGalLabel);

            if (Attributes.IsOutdoorTotalGalEnabled)
                AppendStringTab(text, OutdoorTotalGalLabel);

            if (Attributes.IsTotalGallonsColumnsEnabled) {
                AppendStringTab(text, BathtubTotalGalLabel);
                AppendStringTab(text, ClotheswasherTotalGalLabel);
                AppendStringTab(text, CoolerTotalGalLabel);
                AppendStringTab(text, DishwasherTotalGalLabel);
                AppendStringTab(text, FaucetTotalGalLabel);
                AppendStringTab(text, LeakTotalGalLabel);
                AppendStringTab(text, OtherTotalGalLabel);
                AppendStringTab(text, ShowerTotalGalLabel);
                AppendStringTab(text, ToiletTotalGalLabel);
                AppendStringTab(text, TreatmentTotalGalLabel);
            }

            if (Attributes.IsEventsColumnsEnabled) {
                AppendStringTab(text, BathtubEventsLabel);
                AppendStringTab(text, ClotheswasherEventsLabel);
                AppendStringTab(text, CoolerEventsLabel);
                AppendStringTab(text, DishwasherEventsLabel);
                AppendStringTab(text, FaucetEventsLabel);
                AppendStringTab(text, LeakEventsLabel);
                AppendStringTab(text, OtherEventsLabel);
                AppendStringTab(text, ShowerEventsLabel);
                AppendStringTab(text, ToiletEventsLabel);
                AppendStringTab(text, TreatmentEventsLabel);
            }

            if (Attributes.IsGallonsPerDayColumnsEnabled) {
                AppendStringTab(text, BathtubGpdLabel);
                AppendStringTab(text, ClotheswasherGpdLabel);
                AppendStringTab(text, CoolerGpdLabel);
                AppendStringTab(text, DishwasherGpdLabel);
                AppendStringTab(text, FaucetGpdLabel);
                AppendStringTab(text, LeakGpdLabel);
                AppendStringTab(text, OtherGpdLabel);
                AppendStringTab(text, ShowerGpdLabel);
                AppendStringTab(text, ToiletGpdLabel);
                AppendStringTab(text, TreatmentGpdLabel);
            }

            if (Attributes.IsClotheswasherDetailColumnsEnabled) {
                AppendStringTab(text, AverageClotheswasherLoadGalLabel);
                AppendStringTab(text, ClotheswasherLoadsPerDayLabel);
            }

            if (Attributes.IsShowerDetailColumnsEnabled) {
                AppendStringTab(text, TotalShowerMinutesLabel);
                AppendStringTab(text, AverageShowerSecondsLabel);
                AppendStringTab(text, TotalShowerGalLabel);
                AppendStringTab(text, AverageShowerGalLabel);
                AppendStringTab(text, AverageShowerModeFlowGpmLabel);
                AppendStringTab(text, ShowersPerDayLabel);
                AppendStringTab(text, ShowerMinutesPerDayLabel);
            }

            if (Attributes.IsToiletDetailColumnsEnabled) {
                AppendStringTab(text, AverageToiletFlushVolumeLabel);
                AppendStringTab(text, ToiletFlushStDevLabel);
                AppendStringTab(text, NumberOfFlushesLessThan2_2GalLabel);
                AppendStringTab(text, NumberOfFlushesGreaterThan2_2GalLabel);
                AppendStringTab(text, FlushesPerDayLabel);
            }

            WriteLine(text);

            System.IO.File.AppendAllText(dataSource, text.ToString());
        }
        
        void WriteLine(System.Text.StringBuilder text) {
            text.AppendLine();
        }

        void WriteAnalysis(System.Text.StringBuilder text, ProjectReportProperties ProjectReportProperties, string keyCode) {

            if (Attributes.IsKeyCodeEnabled)
                AppendStringTab(text, keyCode);

            if (Attributes.IsTotalVolumeEnabled)
                AppendStringTab(text, ProjectReportProperties.TotalVolume);

            if (Attributes.IsTraceBeginsEnabled)
                AppendStringTab(text, ProjectReportProperties.TraceBegins);

            if (Attributes.IsTraceEndsEnabled)
                AppendStringTab(text, ProjectReportProperties.TraceEnds);

            if (Attributes.IsTraceLengthDaysEnabled)
                AppendStringTab(text, ProjectReportProperties.TraceLengthDays);

            if (Attributes.IsTotalGpdEnabled)
                AppendStringTab(text, ProjectReportProperties.TotalGpd);

            if (Attributes.IsIndoorGpdEnabled)
                AppendStringTab(text, ProjectReportProperties.IndoorGpd);

            if (Attributes.IsOutdoorGpdEnabled)
                AppendStringTab(text, ProjectReportProperties.OutdoorGpd);

            if (Attributes.IsIndoorTotalGalEnabled)
                AppendStringTab(text, ProjectReportProperties.IndoorTotalGal);

            if (Attributes.IsOutdoorTotalGalEnabled)
                AppendStringTab(text, ProjectReportProperties.OutdoorTotalGal);

            if (Attributes.IsTotalGallonsColumnsEnabled) {
                AppendStringTab(text, ProjectReportProperties.BathtubTotalGal);
                AppendStringTab(text, ProjectReportProperties.ClotheswasherTotalGal);
                AppendStringTab(text, ProjectReportProperties.CoolerTotalGal);
                AppendStringTab(text, ProjectReportProperties.DishwasherTotalGal);
                AppendStringTab(text, ProjectReportProperties.FaucetTotalGal);
                AppendStringTab(text, ProjectReportProperties.LeakTotalGal);
                AppendStringTab(text, ProjectReportProperties.OtherTotalGal);
                AppendStringTab(text, ProjectReportProperties.ShowerTotalGal);
                AppendStringTab(text, ProjectReportProperties.ToiletTotalGal);
                AppendStringTab(text, ProjectReportProperties.TreatmentTotalGal);
            }

            if (Attributes.IsEventsColumnsEnabled) {
                AppendStringTab(text, ProjectReportProperties.BathtubEvents);
                AppendStringTab(text, ProjectReportProperties.ClotheswasherEvents);
                AppendStringTab(text, ProjectReportProperties.CoolerEvents);
                AppendStringTab(text, ProjectReportProperties.DishwasherEvents);
                AppendStringTab(text, ProjectReportProperties.FaucetEvents);
                AppendStringTab(text, ProjectReportProperties.LeakEvents);
                AppendStringTab(text, ProjectReportProperties.OtherEvents);
                AppendStringTab(text, ProjectReportProperties.ShowerEvents);
                AppendStringTab(text, ProjectReportProperties.ToiletEvents);
                AppendStringTab(text, ProjectReportProperties.TreatmentEvents);
            }

            if (Attributes.IsGallonsPerDayColumnsEnabled) {
                AppendStringTab(text, ProjectReportProperties.BathtubGpd);
                AppendStringTab(text, ProjectReportProperties.ClotheswasherGpd);
                AppendStringTab(text, ProjectReportProperties.CoolerGpd);
                AppendStringTab(text, ProjectReportProperties.DishwasherGpd);
                AppendStringTab(text, ProjectReportProperties.FaucetGpd);
                AppendStringTab(text, ProjectReportProperties.LeakGpd);
                AppendStringTab(text, ProjectReportProperties.OtherGpd);
                AppendStringTab(text, ProjectReportProperties.ShowerGpd);
                AppendStringTab(text, ProjectReportProperties.ToiletGpd);
                AppendStringTab(text, ProjectReportProperties.TreatmentGpd);
            }

            if (Attributes.IsClotheswasherDetailColumnsEnabled) {
                AppendStringTab(text, ProjectReportProperties.AverageClotheswasherLoadGal);
                AppendStringTab(text, ProjectReportProperties.ClotheswasherLoadsPerDay);
            }

            if (Attributes.IsShowerDetailColumnsEnabled) {
                AppendStringTab(text, ProjectReportProperties.TotalShowerMinutes);
                AppendStringTab(text, ProjectReportProperties.AverageShowerSeconds);
                AppendStringTab(text, ProjectReportProperties.TotalShowerGal);
                AppendStringTab(text, ProjectReportProperties.AverageShowerGal);
                AppendStringTab(text, ProjectReportProperties.AverageShowerModeFlowGpm);
                AppendStringTab(text, ProjectReportProperties.ShowersPerDay);
                AppendStringTab(text, ProjectReportProperties.ShowerMinutesPerDay);
            }

            if (Attributes.IsToiletDetailColumnsEnabled) {
                AppendStringTab(text, ProjectReportProperties.AverageToiletFlushVolume);
                AppendStringTab(text, ProjectReportProperties.ToiletFlushStDev);
                AppendStringTab(text, ProjectReportProperties.NumberOfToiletFlushesLessThan2Point2Gal);
                AppendStringTab(text, ProjectReportProperties.NumberOfToiletFlushesGreaterThan2Point2Gal);
                AppendStringTab(text, ProjectReportProperties.FlushesPerDay);
            }

            WriteLine(text);
        }
    }
}
