using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Media.Imaging;

using TraceWizard.Entities;

namespace TraceWizard.TwApp {

    public partial class FixtureProfilesPanel : ListView {

        protected string KeyCode { get; set; }
        public FixtureProfilesPanel() {

            InitializeComponent();
        }
    }
}