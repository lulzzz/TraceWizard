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
    public partial class DateTimePickerWindow : Window {
        public DateTime DateTime;
        public DateTimePickerWindow(DateTime dateTime) {
            InitializeComponent();
            TextBoxDateTime.Text = dateTime.ToString();

            this.ButtonOk.Click += new RoutedEventHandler(ButtonOk_Click);
        }

        void ButtonOk_Click(object sender, System.Windows.RoutedEventArgs e) {
            if (DateTime.TryParse(TextBoxDateTime.Text.Trim(), out DateTime))
                Close();
        }
    }
}
