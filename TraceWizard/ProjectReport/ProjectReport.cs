using System;
using System.Collections.Generic;

using TraceWizard.Entities;

namespace TraceWizard.TwApp {

    public class ProjectReportProperties {
        public string KeyCode;
        public double TotalVolume;
        public DateTime TraceBegins;
        public DateTime TraceEnds;
        public int TraceLengthDays;
        public double TotalGpd;
        public double IndoorGpd;
        public double OutdoorGpd;

        public double IndoorTotalGal;
        public double OutdoorTotalGal;

        public double BathtubTotalGal;
        public double ClotheswasherTotalGal;
        public double CoolerTotalGal;
        public double DishwasherTotalGal;
        public double FaucetTotalGal;
        public double LeakTotalGal;
        public double OtherTotalGal;
        public double ShowerTotalGal;
        public double ToiletTotalGal;
        public double TreatmentTotalGal;

        public int BathtubEvents;
        public int ClotheswasherEvents;
        public int CoolerEvents;
        public int DishwasherEvents;
        public int FaucetEvents;
        public int LeakEvents;
        public int OtherEvents;
        public int ShowerEvents;
        public int ToiletEvents;
        public int TreatmentEvents;

        public double BathtubGpd;
        public double ClotheswasherGpd;
        public double CoolerGpd;
        public double DishwasherGpd;
        public double FaucetGpd;
        public double LeakGpd;
        public double OtherGpd;
        public double ShowerGpd;
        public double ToiletGpd;
        public double TreatmentGpd;

        public double AverageClotheswasherLoadGal;
        public double ClotheswasherLoadsPerDay;

        public double TotalShowerMinutes;
        public double AverageShowerSeconds;
        public double TotalShowerGal;
        public double AverageShowerGal;
        public double AverageShowerModeFlowGpm;
        public double ShowersPerDay;
        public double ShowerMinutesPerDay;

        public double AverageToiletFlushVolume;
        public double ToiletFlushStDev;
        public int NumberOfToiletFlushesLessThan2Point2Gal;
        public int NumberOfToiletFlushesGreaterThan2Point2Gal;
        public double FlushesPerDay;
    }

    public class ProjectReportCalculator {  
    
        FixtureSummaries FixtureSummaries;
        Events Events;

        public ProjectReportProperties CalculateProjectReportProperties(Events events, List<FixtureClass> fixtureClassesOutdoor) {
            Events = events;
            FixtureSummaries = new FixtureSummaries(Events);
            FixtureSummaries.Update();

            List<FixtureClass> fixtureClassesIndoor = new List<FixtureClass>();
            foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values)
                if (!fixtureClassesOutdoor.Contains(fixtureClass))
                    fixtureClassesIndoor.Add(fixtureClass);

            var ProjectReportProperties = new ProjectReportProperties();

            ProjectReportProperties.TotalVolume = Events.Volume;
            ProjectReportProperties.TraceBegins = Events.StartTime;
            ProjectReportProperties.TraceEnds = Events.EndTime;

            ProjectReportProperties.TraceLengthDays = Events.EndTime.Date.Subtract(Events.StartTime.Date).Days + 1;

            ProjectReportProperties.TotalGpd = Events.Volume / ProjectReportProperties.TraceLengthDays;

            ProjectReportProperties.IndoorTotalGal = CalculateVolume(fixtureClassesIndoor);
            ProjectReportProperties.OutdoorTotalGal = CalculateVolume(fixtureClassesOutdoor);

            ProjectReportProperties.BathtubTotalGal = FixtureSummaries[FixtureClasses.Bathtub].Volume;
            ProjectReportProperties.ClotheswasherTotalGal = FixtureSummaries[FixtureClasses.Clotheswasher].Volume;
            ProjectReportProperties.CoolerTotalGal = FixtureSummaries[FixtureClasses.Cooler].Volume;
            ProjectReportProperties.DishwasherTotalGal = FixtureSummaries[FixtureClasses.Dishwasher].Volume;
            ProjectReportProperties.FaucetTotalGal = FixtureSummaries[FixtureClasses.Faucet].Volume;
            ProjectReportProperties.LeakTotalGal = FixtureSummaries[FixtureClasses.Leak].Volume;
            ProjectReportProperties.ShowerTotalGal = FixtureSummaries[FixtureClasses.Shower].Volume;
            ProjectReportProperties.ToiletTotalGal = FixtureSummaries[FixtureClasses.Toilet].Volume;
            ProjectReportProperties.TreatmentTotalGal = FixtureSummaries[FixtureClasses.Treatment].Volume;

            ProjectReportProperties.OtherTotalGal = ProjectReportProperties.IndoorTotalGal
                - FixtureSummaries[FixtureClasses.Bathtub].Volume
                - FixtureSummaries[FixtureClasses.Clotheswasher].Volume
                - FixtureSummaries[FixtureClasses.Cooler].Volume
                - FixtureSummaries[FixtureClasses.Dishwasher].Volume
                - FixtureSummaries[FixtureClasses.Faucet].Volume
                - FixtureSummaries[FixtureClasses.Leak].Volume
                - FixtureSummaries[FixtureClasses.Shower].Volume
                - FixtureSummaries[FixtureClasses.Toilet].Volume
                - FixtureSummaries[FixtureClasses.Treatment].Volume
                ;

            ProjectReportProperties.IndoorGpd = ProjectReportProperties.IndoorTotalGal / ProjectReportProperties.TraceLengthDays;
            ProjectReportProperties.OutdoorGpd = ProjectReportProperties.OutdoorTotalGal / ProjectReportProperties.TraceLengthDays;
            ProjectReportProperties.BathtubGpd = ProjectReportProperties.BathtubTotalGal / ProjectReportProperties.TraceLengthDays;
            ProjectReportProperties.ClotheswasherGpd = ProjectReportProperties.ClotheswasherTotalGal / ProjectReportProperties.TraceLengthDays;
            ProjectReportProperties.CoolerGpd = ProjectReportProperties.CoolerTotalGal / ProjectReportProperties.TraceLengthDays;
            ProjectReportProperties.DishwasherGpd = ProjectReportProperties.DishwasherTotalGal / ProjectReportProperties.TraceLengthDays;
            ProjectReportProperties.FaucetGpd = ProjectReportProperties.FaucetTotalGal / ProjectReportProperties.TraceLengthDays;
            ProjectReportProperties.LeakGpd = ProjectReportProperties.LeakTotalGal / ProjectReportProperties.TraceLengthDays;
            ProjectReportProperties.ShowerGpd = ProjectReportProperties.ShowerTotalGal / ProjectReportProperties.TraceLengthDays;
            ProjectReportProperties.ToiletGpd = ProjectReportProperties.ToiletTotalGal / ProjectReportProperties.TraceLengthDays;
            ProjectReportProperties.TreatmentGpd = ProjectReportProperties.TreatmentTotalGal / ProjectReportProperties.TraceLengthDays;
            ProjectReportProperties.OtherGpd = ProjectReportProperties.OtherTotalGal / ProjectReportProperties.TraceLengthDays;

            ProjectReportProperties.BathtubEvents = FixtureSummaries[FixtureClasses.Bathtub].Count;
            ProjectReportProperties.ClotheswasherEvents = FixtureSummaries[FixtureClasses.Clotheswasher].FirstCycles;
            ProjectReportProperties.CoolerEvents = FixtureSummaries[FixtureClasses.Cooler].Count;
            ProjectReportProperties.DishwasherEvents = FixtureSummaries[FixtureClasses.Dishwasher].FirstCycles;
            ProjectReportProperties.FaucetEvents = FixtureSummaries[FixtureClasses.Faucet].Count;
            ProjectReportProperties.LeakEvents = FixtureSummaries[FixtureClasses.Leak].Count;
            ProjectReportProperties.ShowerEvents = FixtureSummaries[FixtureClasses.Shower].Count;
            ProjectReportProperties.ToiletEvents = FixtureSummaries[FixtureClasses.Toilet].Count;
            ProjectReportProperties.TreatmentEvents = FixtureSummaries[FixtureClasses.Treatment].Count;

            ProjectReportProperties.OtherEvents = CalculateEvents(fixtureClassesIndoor)
                - FixtureSummaries[FixtureClasses.Bathtub].Count
                - FixtureSummaries[FixtureClasses.Clotheswasher].Count
                - FixtureSummaries[FixtureClasses.Cooler].Count
                - FixtureSummaries[FixtureClasses.Dishwasher].Count
                - FixtureSummaries[FixtureClasses.Faucet].Count
                - FixtureSummaries[FixtureClasses.Leak].Count
                - FixtureSummaries[FixtureClasses.Shower].Count
                - FixtureSummaries[FixtureClasses.Toilet].Count
                - FixtureSummaries[FixtureClasses.Treatment].Count
                ;
            
        ProjectReportProperties.AverageClotheswasherLoadGal = ProjectReportProperties.ClotheswasherTotalGal / ProjectReportProperties.ClotheswasherEvents;
        ProjectReportProperties.ClotheswasherLoadsPerDay = ((double) ProjectReportProperties.ClotheswasherEvents) / ProjectReportProperties.TraceLengthDays;

        ProjectReportProperties.TotalShowerMinutes = CalculateTotalMinutes(FixtureClasses.Shower);
        ProjectReportProperties.AverageShowerSeconds = (ProjectReportProperties.TotalShowerMinutes / ProjectReportProperties.ShowerEvents) * 60;
        ProjectReportProperties.TotalShowerGal = ProjectReportProperties.ShowerTotalGal;
        ProjectReportProperties.AverageShowerGal = ProjectReportProperties.ShowerTotalGal / ProjectReportProperties.ShowerEvents;
        ProjectReportProperties.AverageShowerModeFlowGpm = CalculateMeanMode(FixtureClasses.Shower);
        ProjectReportProperties.ShowersPerDay = ((double)FixtureSummaries[FixtureClasses.Shower].Count) / ProjectReportProperties.TraceLengthDays ;
        ProjectReportProperties.ShowerMinutesPerDay = ProjectReportProperties.TotalShowerMinutes / ProjectReportProperties.TraceLengthDays;

        ProjectReportProperties.AverageToiletFlushVolume = ProjectReportProperties.ToiletTotalGal / ProjectReportProperties.ToiletEvents;
        ProjectReportProperties.ToiletFlushStDev = CalculateVolumeStandardDeviation(FixtureClasses.Toilet);
        ProjectReportProperties.NumberOfToiletFlushesLessThan2Point2Gal = CalculateToiletEventsLessThan(2.2); ;
        ProjectReportProperties.NumberOfToiletFlushesGreaterThan2Point2Gal = ProjectReportProperties.ToiletEvents - ProjectReportProperties.NumberOfToiletFlushesLessThan2Point2Gal;
        ProjectReportProperties.FlushesPerDay = ((double)ProjectReportProperties.ToiletEvents) / ProjectReportProperties.TraceLengthDays;

        return ProjectReportProperties;
        }

        double CalculateVolume(List<FixtureClass> fixtureClasses) {
            double total = 0;
            foreach (FixtureClass fixtureClass in fixtureClasses) {
                total += FixtureSummaries[fixtureClass].Volume;
            }
            return total;
        }

        int CalculateEvents(List<FixtureClass> fixtureClasses) {
            int total = 0;
            foreach (FixtureClass fixtureClass in fixtureClasses) {
                total += FixtureSummaries[fixtureClass].Count;
            }
            return total;
        }

        double CalculateTotalMinutes(FixtureClass fixtureClass) {
            if (FixtureSummaries[fixtureClass].Count == 0)
                return double.NaN;

            TimeSpan duration = new TimeSpan();
            foreach (Event @event in Events) {
                if (@event.FixtureClass == fixtureClass) {
                    duration += @event.Duration;
                }
            }
            return duration.TotalMinutes;
        }

        double CalculateVolumeStandardDeviation(FixtureClass fixtureClass) {
            List<double> list = new List<double>();
            foreach (Event @event in Events) {
                if (@event.FixtureClass == fixtureClass) {
                    list.Add(@event.Volume);
                }
            }
            return GetStdev(list);
        }

        public static double GetAverage(List<double> list) {
            if (list.Count == 0)
                return 0;

            double sum = 0;
            foreach (double d in list) {
                sum += d;
            }
            return sum / list.Count;
        }
        
        static double GetVariance(List<double> list) {
            double avg = GetAverage(list);

            double sum = 0;
            foreach (double d in list) {
                sum += Math.Pow((d - avg), 2);
            }
            return sum / list.Count;
        }

        static double GetStdev(List<double> list) {
            return Math.Sqrt(GetVariance(list));
        } 

        double CalculateMeanMode(FixtureClass fixtureClass) {
            double modeSum = 0;
            int count = 0;
            foreach (Event @event in Events) {
                if (@event.FixtureClass == fixtureClass) {
                    modeSum += @event.Mode;
                    count++;
                }
            }
            return modeSum / count;
        }

        int CalculateToiletEventsLessThan(double threshold) {
            int count = 0;
            foreach (Event @event in Events) {
                if (@event.FixtureClass == FixtureClasses.Toilet && @event.Volume < threshold) {
                    count++;
                }
            }
            return count;
        }
    }
}
