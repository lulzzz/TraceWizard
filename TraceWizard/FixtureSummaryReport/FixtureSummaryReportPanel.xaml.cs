﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using TraceWizard.Entities;

namespace TraceWizard.TwApp {
    public partial class FixtureSummaryReportPanel : UserControl {

        public Analysis Analysis;
        public double Radius;
        public bool ByInstances;

        public FixtureSummaryReportPanel() {
            InitializeComponent();
        }

        public void Initialize() {

            PieChart.ByInstances = ByInstances;
            PieChart.Analysis = Analysis;
            PieChart.Radius = Radius;
            PieChart.IsEnlargeable = false;
            PieChart.Initialize();

            FixtureSummaryReportTable.ByInstances = ByInstances;
            FixtureSummaryReportTable.Analysis = Analysis;
            FixtureSummaryReportTable.Initialize();
        }
    }
}