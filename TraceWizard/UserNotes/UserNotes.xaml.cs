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

using TraceWizard.Entities;

namespace TraceWizard.TwApp {
    public partial class UserNotes : Window {
        public UserNotes() {
            InitializeComponent();
            textBox.PreviewTextInput +=new TextCompositionEventHandler(textBox_PreviewTextInput);
            OKButton.Click +=new RoutedEventHandler(OKButton_Click);
        }

        void OKButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
        }

        void Window_Loaded(object sender, RoutedEventArgs e) {
            textBox.Focus();
        }

        void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
        if (e.Text == ",")
            e.Handled = true;
        }
    }
}
