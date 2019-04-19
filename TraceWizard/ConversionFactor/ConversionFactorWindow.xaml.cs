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
using System.Windows.Shapes;

namespace TraceWizard.TwApp {
    public partial class ConversionFactorWindow : Window {
        public double ConversionFactor;
        public ConversionFactorWindow(double conversionFactor) {
            InitializeComponent();
            ConversionFactor = conversionFactor;
            TextBoxConversionFactor.Text = conversionFactor.ToString("0.0000");
            TextBoxConversionFactor.CaretIndex = TextBoxConversionFactor.Text.Length;

            this.ButtonOk.Click += new RoutedEventHandler(ButtonOk_Click);
        }

        void ButtonOk_Click(object sender, System.Windows.RoutedEventArgs e) {
            double value;
            if (double.TryParse(TextBoxConversionFactor.Text.Trim(), out value) && value > 0) {
                ConversionFactor = value;
                Close();
            } else {
                MessageBox.Show("Conversion Factor must be > 0","Invalid Conversion Factor",MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
