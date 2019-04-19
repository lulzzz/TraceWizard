using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Globalization;

using System.ComponentModel;

using TraceWizard.Entities;
using TraceWizard.Adoption;
using TraceWizard.Classification;
using TraceWizard.Disaggregation;

namespace TraceWizard.TwHelper {

    public class LongTimeConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            DateTime dateTime = (DateTime)value;

            return dateTime.ToLongTimeString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class ShortDateConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            DateTime dateTime = (DateTime)value;

            return dateTime.ToShortDateString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class DurationConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            TimeSpan timeSpan = (TimeSpan)value;
            string result = null;

            result += timeSpan.Minutes.ToString("00") + ":" + timeSpan.Seconds.ToString("00");

            if (timeSpan.Hours > 0 || timeSpan.Days > 0) {
                result = timeSpan.Hours.ToString("00") + ":" + result;
            }

            if (timeSpan.Days > 0) {
                result = timeSpan.Days.ToString("0") + ":" + result;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class LimitedDurationConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            TimeSpan timeSpan = (TimeSpan)value;
            string result = null;

            int minutes = Math.Min(timeSpan.Minutes + (timeSpan.Hours * 60), 99);

            result += minutes.ToString("00") + ":" + timeSpan.Seconds.ToString("00");

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class FriendlyDurationConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            TimeSpan timeSpan = (TimeSpan)value;
            string result = null;

            if (timeSpan.Days > 0)
                result += timeSpan.Days.ToString("0") + "d";
            if (timeSpan.Hours > 0 || result != null)
                result += timeSpan.Hours.ToString("00") + "h";
            if (timeSpan.Minutes > 0 || result != null)
                result += timeSpan.Minutes.ToString("00") + "m";
            if (timeSpan.Seconds > 0 || result != null)
                result += timeSpan.Seconds.ToString("00") + "s";

            return result == null ? string.Empty : result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            try {
                string s = value as string;
                s = s.Trim().ToLower();

                int days = 0;
                int hours = 0;
                int minutes = 0;
                int seconds = 0;
                int index = -1;

                string[] tokens = s.Split(':');

                if (tokens.Length > 1) {
                    seconds = int.Parse(tokens[tokens.Length - 1]);
                    minutes = int.Parse(tokens[tokens.Length - 2]);
                    if (tokens.Length > 2)
                        hours = int.Parse(tokens[tokens.Length - 3]);
                        if (tokens.Length > 3)
                            days = int.Parse(tokens[tokens.Length - 4]);
                } else {
                    index = s.IndexOf('d');
                    if (index > -1)
                        if (index - 3 < 0 || !int.TryParse(s.Substring(index - 3, 3), out days))
                            if (index - 2 < 0 || !int.TryParse(s.Substring(index - 2, 2), out days))
                                if (index - 1 < 0 || !int.TryParse(s.Substring(index - 1, 1), out days))
                                    return TimeSpan.MinValue;

                    index = s.IndexOf('h');
                    if (index > -1)
                        if (index - 3 < 0 || !int.TryParse(s.Substring(index - 3, 3), out hours))
                            if (index - 2 < 0 || !int.TryParse(s.Substring(index - 2, 2), out hours))
                                if (index - 1 < 0 || !int.TryParse(s.Substring(index - 1, 1), out hours))
                                    return TimeSpan.MinValue;

                    index = s.IndexOf('m');
                    if (index > -1)
                        if (index - 3 < 0 || !int.TryParse(s.Substring(index - 3, 3), out minutes))
                            if (index - 2 < 0 || !int.TryParse(s.Substring(index - 2, 2), out minutes))
                                if (index - 1 < 0 || !int.TryParse(s.Substring(index - 1, 1), out minutes))
                                    return TimeSpan.MinValue;

                    index = s.IndexOf('s');
                    if (index > -1)
                        if (index - 3 < 0 || !int.TryParse(s.Substring(index - 3, 3), out seconds))
                            if (index - 2 < 0 || !int.TryParse(s.Substring(index - 2, 2), out seconds))
                                if (index - 1 < 0 || !int.TryParse(s.Substring(index - 1, 1), out seconds))
                                    return TimeSpan.MinValue;
                }
            
                if (days == 0 && hours == 0 && minutes == 0 && seconds == 0)
                    return TimeSpan.MinValue;
                else 
                    return new TimeSpan(days, hours, minutes, seconds);
            } catch {
                return TimeSpan.MinValue;
            }
        }
    }

    public class FriendlierDurationConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            TimeSpan timeSpan = (TimeSpan)value;
            string result = null;

            timeSpan = NormalizeViewportRoundingBug(timeSpan);

            if (timeSpan.Days > 0) {
                if (timeSpan.Days == 1)
                    result += "1 day ";
                else
                    result += timeSpan.Days.ToString("0") + " days ";
            }
            if (timeSpan.Hours > 0 || (timeSpan.Days > 0 && timeSpan.Minutes > 0)) {
                if (timeSpan.Hours == 1)
                    result += "1 hour ";
                else
                    result += timeSpan.Hours.ToString("0") + " hours ";
            }
            if (timeSpan.Minutes > 0 || (timeSpan.Hours > 0 && timeSpan.Seconds > 0)) {
                if (timeSpan.Minutes == 1)
                    result += "1 minute ";
                else
                    result += timeSpan.Minutes.ToString("0") + " minutes ";
            }
            if (timeSpan.Seconds > 0) {
                if (timeSpan.Seconds == 1)
                    result += "1 second ";
                else
                    result += timeSpan.Seconds.ToString("0") + " seconds ";
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        TimeSpan NormalizeViewportRoundingBug(TimeSpan timeSpan) {
            if (timeSpan.Milliseconds > 500)
                return timeSpan.Add(new TimeSpan(0, 0, 0, 0, 1000 - timeSpan.Milliseconds));
            else
                return timeSpan;
        }
    }

    public class ViewportStartTimeConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture) {

            double HorizontalOffset = (double)values[0];
            double ViewportWidth = (double)values[1];
            double Width = (double)values[2];

            double percentElapsed = HorizontalOffset / Width;

            if (double.IsNaN(percentElapsed) || double.IsInfinity(percentElapsed))
                return null;

            TimeFrame timeFrame = (TimeFrame)parameter;

            DateTime dateTime = timeFrame.StartTime.Add(new TimeSpan((long)(percentElapsed * timeFrame.Duration.Ticks)));

            return dateTime.ToLongTimeString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class ViewportStartDateConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture) {

            double HorizontalOffset = (double)values[0];
            double ViewportWidth = (double)values[1];
            double Width = (double)values[2];

            double percentElapsed = HorizontalOffset / Width;

            if (double.IsNaN(percentElapsed) || double.IsInfinity(percentElapsed))
                return null;

            TimeFrame timeFrame = (TimeFrame)parameter;

            DateTime dateTime = timeFrame.StartTime.Add(new TimeSpan((long)(percentElapsed * timeFrame.Duration.Ticks)));

            return dateTime.ToShortDateString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class ViewportLongStartDateConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture) {

            double HorizontalOffset = (double)values[0];
            double ViewportWidth = (double)values[1];
            double Width = (double)values[2];

            double percentElapsed = HorizontalOffset / Width;

            if (double.IsNaN(percentElapsed) || double.IsInfinity(percentElapsed))
                return null;

            TimeFrame timeFrame = (TimeFrame)parameter;

            DateTime dateTime = timeFrame.StartTime.Add(new TimeSpan((long)(percentElapsed * timeFrame.Duration.Ticks)));

            return dateTime.ToLongDateString() + " at " + dateTime.ToLongTimeString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class ViewportEndTimeConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture) {

            double HorizontalOffset = (double)values[0];
            double ViewportWidth = (double)values[1];
            double Width = (double)values[2];

            double percentElapsed = (HorizontalOffset + ViewportWidth) / Width;

            if (double.IsNaN(percentElapsed) || double.IsInfinity(percentElapsed))
                return null;

            TimeFrame timeFrame = (TimeFrame)parameter;

            DateTime dateTime = timeFrame.StartTime.Add(new TimeSpan((long)(percentElapsed * timeFrame.Duration.Ticks)));

            return dateTime.ToLongTimeString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class ViewportEndDateConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture) {

            double HorizontalOffset = (double)values[0];
            double ViewportWidth = (double)values[1];
            double Width = (double)values[2];

            double percentElapsed = (HorizontalOffset + ViewportWidth) / Width;

            if (double.IsNaN(percentElapsed) || double.IsInfinity(percentElapsed))
                return null;

            TimeFrame timeFrame = (TimeFrame)parameter;

            DateTime dateTime = timeFrame.StartTime.Add(new TimeSpan((long)(percentElapsed * timeFrame.Duration.Ticks)));

            return dateTime.ToShortDateString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class ViewportLongEndDateConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture) {

            double HorizontalOffset = (double)values[0];
            double ViewportWidth = (double)values[1];
            double Width = (double)values[2];

            double percentElapsed = (HorizontalOffset + ViewportWidth) / Width;

            if (double.IsNaN(percentElapsed) || double.IsInfinity(percentElapsed))
                return null;

            TimeFrame timeFrame = (TimeFrame)parameter;

            DateTime dateTime = timeFrame.StartTime.Add(new TimeSpan((long)(percentElapsed * timeFrame.Duration.Ticks)));

            return dateTime.ToLongDateString() + " at " + dateTime.ToLongTimeString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class ViewportDurationConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture) {

            double HorizontalOffset = (double)values[0];
            double ViewportWidth = (double)values[1];
            double Width = (double)values[2];

            double percentViewport = ViewportWidth / Width;

            if (double.IsNaN(percentViewport) || double.IsInfinity(percentViewport))
                return null;

            TimeFrame timeFrame = (TimeFrame)parameter;

            TimeSpan duration = new TimeSpan((long)(timeFrame.Duration.Ticks * percentViewport));

            return (new FriendlierDurationConverter()).Convert(duration, null, null, null);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
