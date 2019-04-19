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

using System.Windows.Navigation;

using TraceWizard.Environment;
using TraceWizard.FeatureLevels;

namespace TraceWizard.TwApp {
    public partial class About : Window {
        public About(FeatureLevel featureLevel) {
            InitializeComponent();
            Initialize(featureLevel);
        }

        void HyperlinkAbout_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            TwFile.Launch(e.Uri.AbsoluteUri);
        }

        void Initialize(FeatureLevel featureLevel) {
            Title = "About " + TwAssembly.CompanyAndTitle();

            ImageIcon.Source = TwGui.GetIcon32();

            LabelTitle.Text = TwAssembly.CompanyAndTitle();
            LabelVersion.Text = TwAssembly.CompleteVersion() + " " + featureLevel.Text;
            LabelCopyright.Text = TwAssembly.Copyright();

            var hyperlink = new Hyperlink();
            hyperlink.NavigateUri = new System.Uri(TwEnvironment.WebSite);
            hyperlink.RequestNavigate += new RequestNavigateEventHandler(HyperlinkAbout_RequestNavigate);
            hyperlink.Inlines.Add(TwEnvironment.WebSite);
            LabelHyperlink.Inlines.Add(hyperlink);
        }
    }
}
