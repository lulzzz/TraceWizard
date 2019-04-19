using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.ComponentModel;

using TraceWizard.Entities;
using TraceWizard.Classification;
using TraceWizard.Classification.Classifiers.FixtureList;
using TraceWizard.Classification.Classifiers.J48;
using TraceWizard.FeatureLevels;
using TraceWizard.Notification;

namespace TraceWizard.TwApp {
    public partial class GraphToolBar : UserControl, INotifyPropertyChanged {

        public Classifier ClassifierMachineLearning;
        //public Classifier ClassifierFixtureList;
        public Analysis Analysis;
        public FeatureLevel FeatureLevel;
        public UndoManager UndoManager;

        bool showingMoreFixtures = true;

        public GraphToolBar() {
            InitializeComponent();
        }

        public void Initialize() {

            Properties.Settings.Default.PropertyChanged += new PropertyChangedEventHandler(Default_PropertyChanged);

            Visibility = SetVisibility();
            SetSettingTopAlignment();
            SetSettingShowFixtures();
            SetSettingSingleLine();

            InitializeApplyConversionFactorButton();
            InitializeUndoButtons();
            InitializeFixtureButtons();
            InitializeClassifyButtons();
            InitializeMergeSplitButtons();
            InitializeFindModeButtons();
            InitializeClassificationActionModeButtons();
            InitializeManualClassificationModeButtons();
            InitializeRulerButtons();

            CanStartDragging = true;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        void ButtonMoreFixtures_Click(object sender, RoutedEventArgs e) {
            if (showingMoreFixtures) {
//                AnalysisPanel.SetVisibility(panelFixturesLowFrequency, Visibility.Collapsed);
                panelFixturesLowFrequency.Visibility = Visibility.Collapsed;
                ButtonMoreFixtures.ToolTip = "Show More Fixtures";
                ImageMoreFixtures.Source = TwGui.GetImage("arrowright.png");
            } else {
                panelFixturesLowFrequency.Visibility = Visibility.Visible;
                ButtonMoreFixtures.ToolTip = "Hide More Fixtures";
                ImageMoreFixtures.Source = TwGui.GetImage("arrowleft.png");
            }

            showingMoreFixtures = !showingMoreFixtures;
        }

        void InitializeRulerButtons() {
            ButtonSelectionRuler.Visibility = Properties.Settings.Default.ShowSelectionRulerByDefault ? Visibility.Visible : Visibility.Collapsed;
            ButtonClassificationRuler.Visibility = Properties.Settings.Default.ShowClassificationRulerByDefault ? Visibility.Visible : Visibility.Collapsed;
            ButtonApprovalRuler.Visibility = Properties.Settings.Default.ShowApprovalRulerByDefault ? Visibility.Visible : Visibility.Collapsed;
            ButtonFixturesRuler.Visibility = Properties.Settings.Default.ShowFixturesRulerByDefault ? Visibility.Visible : Visibility.Collapsed;
            if (ButtonSelectionRuler.Visibility != Visibility.Visible && ButtonClassificationRuler.Visibility != Visibility.Visible && ButtonFixturesRuler.Visibility != Visibility.Visible) {
                SeparatorRulers.Visibility = Visibility.Collapsed;
                dockRulers.Visibility = Visibility.Collapsed;
            }

            ButtonSelectionRuler.IsChecked = Properties.Settings.Default.ShowSelectionRulerByDefault;
            ButtonClassificationRuler.IsChecked = Properties.Settings.Default.ShowClassificationRulerByDefault;
            ButtonApprovalRuler.IsChecked = Properties.Settings.Default.ShowApprovalRulerByDefault;
            ButtonFixturesRuler.IsChecked = Properties.Settings.Default.ShowFixturesRulerByDefault;

            ButtonSelectionRuler.Click += new RoutedEventHandler(ButtonSelectionRuler_Click);
            ButtonClassificationRuler.Click += new RoutedEventHandler(ButtonClassificationRuler_Click);
            ButtonApprovalRuler.Click += new RoutedEventHandler(ButtonApprovalRuler_Click);
            ButtonFixturesRuler.Click += new RoutedEventHandler(ButtonFixturesRuler_Click);
        }

        void ButtonSelectionRuler_Click(object sender, RoutedEventArgs e) {
            if (ButtonSelectionRuler.IsChecked == true)
                OnPropertyChanged(TwNotificationProperty.OnShowSelectionRuler);
            else
                OnPropertyChanged(TwNotificationProperty.OnHideSelectionRuler);
        }

        void ButtonClassificationRuler_Click(object sender, RoutedEventArgs e) {
            if (ButtonClassificationRuler.IsChecked == true)
                OnPropertyChanged(TwNotificationProperty.OnShowClassificationRuler);
            else
                OnPropertyChanged(TwNotificationProperty.OnHideClassificationRuler);
        }

        void ButtonApprovalRuler_Click(object sender, RoutedEventArgs e) {
            if (ButtonApprovalRuler.IsChecked == true)
                OnPropertyChanged(TwNotificationProperty.OnShowApprovalRuler);
            else
                OnPropertyChanged(TwNotificationProperty.OnHideApprovalRuler);
        }

        void ButtonFixturesRuler_Click(object sender, RoutedEventArgs e) {
            if (ButtonFixturesRuler.IsChecked == true)
                OnPropertyChanged(TwNotificationProperty.OnShowFixturesRuler);
            else
                OnPropertyChanged(TwNotificationProperty.OnHideFixturesRuler);
        }

        void InitializeFindModeButtons() {
            switch(Properties.Settings.Default.FindMode) {
                case "Any":
                    ToggleFindMode(ButtonFindModeAny, ButtonFindModeAny);
                    break;
                case "Selected":
                    ToggleFindMode(ButtonFindModeSelected, ButtonFindModeSelected);
                    break;
                case "Similar":
                    ToggleFindMode(ButtonFindModeSimilar, ButtonFindModeSimilar);
                    break;
            }

            ButtonFindModeAny.Click +=new RoutedEventHandler(ButtonFindMode_Click);
            ButtonFindModeSelected.Click += new RoutedEventHandler(ButtonFindMode_Click);
            ButtonFindModeSimilar.Click += new RoutedEventHandler(ButtonFindMode_Click);
        }

        void ButtonFindMode_Click(object sender, RoutedEventArgs e) {
            var source = sender as ToggleButton;
            
            ToggleFindMode(source, ButtonFindModeAny);
            ToggleFindMode(source, ButtonFindModeSelected);
            ToggleFindMode(source, ButtonFindModeSimilar);
        }

        void ToggleFindMode(ToggleButton source, ToggleButton target) {
            if (source == target) {
                target.IsChecked = true;
                if (source == ButtonFindModeAny)
                    OnPropertyChanged(TwNotificationProperty.OnFindModeAnyFixture);
                else if (source == ButtonFindModeSelected)
                    OnPropertyChanged(TwNotificationProperty.OnFindModeSelectedFixture);
                else if (source == ButtonFindModeSimilar)
                    OnPropertyChanged(TwNotificationProperty.OnFindModeSimilarFixture);
            } else
                target.IsChecked = false;
        }

        void InitializeClassificationActionModeButtons() {
            switch (Properties.Settings.Default.ClassificationActionMode) {
                case "Selected":
                    ToggleClassificationActionMode(ButtonClassificationActionModeSelected, ButtonClassificationActionModeSelected);
                    break;
                case "All":
                    ToggleClassificationActionMode(ButtonClassificationActionModeAll, ButtonClassificationActionModeAll);
                    break;
                case "Forward":
                    ToggleClassificationActionMode(ButtonClassificationActionModeForward, ButtonClassificationActionModeForward);
                    break;
            }

            ButtonClassificationActionModeSelected.Click += new RoutedEventHandler(ButtonClassificationActionMode_Click);
            ButtonClassificationActionModeAll.Click += new RoutedEventHandler(ButtonClassificationActionMode_Click);
            ButtonClassificationActionModeForward.Click += new RoutedEventHandler(ButtonClassificationActionMode_Click);
        }

        void ButtonClassificationActionMode_Click(object sender, RoutedEventArgs e) {
            var source = sender as ToggleButton;

            ToggleClassificationActionMode(source, ButtonClassificationActionModeSelected);
            ToggleClassificationActionMode(source, ButtonClassificationActionModeAll);
            ToggleClassificationActionMode(source, ButtonClassificationActionModeForward);
        }

        void ToggleClassificationActionMode(ToggleButton source, ToggleButton target) {
            if (source == target) {
                target.IsChecked = true;
                if (source == ButtonClassificationActionModeSelected)
                    OnPropertyChanged(TwNotificationProperty.OnClassificationActionModeSelected);
                else if (source == ButtonClassificationActionModeAll)
                    OnPropertyChanged(TwNotificationProperty.OnClassificationActionModeAll);
                else if (source == ButtonClassificationActionModeForward)
                    OnPropertyChanged(TwNotificationProperty.OnClassificationActionModeForward);
            } else
                target.IsChecked = false;
        }

        void InitializeManualClassificationModeButtons() {
            switch (Properties.Settings.Default.ManualClassificationMode) {
                case "Without Adoption":
                    ToggleManualClassificationMode(ButtonManualClassificationModeWithoutAdoption, ButtonManualClassificationModeWithoutAdoption);
                    break;
                case "With Adoption":
                    ToggleManualClassificationMode(ButtonManualClassificationModeWithAdoption, ButtonManualClassificationModeWithAdoption);
                    break;
            }

            ButtonManualClassificationModeWithoutAdoption.Click += new RoutedEventHandler(ButtonManualClassificationMode_Click);
            ButtonManualClassificationModeWithAdoption.Click += new RoutedEventHandler(ButtonManualClassificationMode_Click);
        }

        void ButtonManualClassificationMode_Click(object sender, RoutedEventArgs e) {
            var source = sender as ToggleButton;

            ToggleManualClassificationMode(source, ButtonManualClassificationModeWithoutAdoption);
            ToggleManualClassificationMode(source, ButtonManualClassificationModeWithAdoption);
        }

        void ToggleManualClassificationMode(ToggleButton source, ToggleButton target) {
            if (source == target) {
                target.IsChecked = true;
                if (source == ButtonManualClassificationModeWithoutAdoption)
                    OnPropertyChanged(TwNotificationProperty.OnManualClassificationModeWithoutAdoption);
                else if (source == ButtonManualClassificationModeWithAdoption)
                    OnPropertyChanged(TwNotificationProperty.OnManualClassificationModeWithAdoption);
            } else
                target.IsChecked = false;
        }

        void InitializeMergeSplitButtons() {
            ButtonSplitHorizontally.Click += new RoutedEventHandler(ButtonSplitHorizontally_Click);
            ButtonSplitVertically.Click += new RoutedEventHandler(ButtonSplitVertically_Click);
            ButtonMergeAllIntoBase.Click += new RoutedEventHandler(ButtonMergeAllIntoBase_Click);
        }

        void ButtonMergeAllIntoBase_Click(object sender, RoutedEventArgs e) {
            if (ButtonMergeAllIntoBase.IsChecked == true) {
                ButtonSplitHorizontally.IsChecked = false;
                ButtonSplitVertically.IsChecked = false;
                if (Mouse.OverrideCursor != Cursors.Arrow)
                    Mouse.OverrideCursor = null;
                OnPropertyChanged(TwNotificationProperty.OnEnterMergeAllIntoBaseMode);
                OnPropertyChanged(TwNotificationProperty.OnLeaveHorizontalSplitMode);
                OnPropertyChanged(TwNotificationProperty.OnLeaveVerticalSplitMode);
            } else {
                if (Mouse.OverrideCursor != Cursors.Arrow)
                    Mouse.OverrideCursor = null;
                OnPropertyChanged(TwNotificationProperty.OnLeaveMergeAllIntoBaseMode);
            }
        }

        void ButtonSplitHorizontally_Click(object sender, RoutedEventArgs e) {
            if (ButtonSplitHorizontally.IsChecked == true) {
                ButtonSplitVertically.IsChecked = false;
                ButtonMergeAllIntoBase.IsChecked = false;
                Mouse.OverrideCursor = Cursors.IBeam;
                OnPropertyChanged(TwNotificationProperty.OnEnterHorizontalSplitMode);
                OnPropertyChanged(TwNotificationProperty.OnLeaveVerticalSplitMode);
                OnPropertyChanged(TwNotificationProperty.OnLeaveMergeAllIntoBaseMode);
            } else {
                if (Mouse.OverrideCursor != Cursors.Arrow)
                    Mouse.OverrideCursor = null;
                OnPropertyChanged(TwNotificationProperty.OnLeaveHorizontalSplitMode);
            }
        }

        void ButtonSplitVertically_Click(object sender, RoutedEventArgs e) {
            if (ButtonSplitVertically.IsChecked == true) {
                ButtonSplitHorizontally.IsChecked = false;
                ButtonMergeAllIntoBase.IsChecked = false;
                Mouse.OverrideCursor = Cursors.Cross;
                OnPropertyChanged(TwNotificationProperty.OnEnterVerticalSplitMode);
                OnPropertyChanged(TwNotificationProperty.OnLeaveHorizontalSplitMode);
                OnPropertyChanged(TwNotificationProperty.OnLeaveMergeAllIntoBaseMode);
            } else {
                if (Mouse.OverrideCursor != Cursors.Arrow)
                    Mouse.OverrideCursor = null;
                OnPropertyChanged(TwNotificationProperty.OnLeaveVerticalSplitMode);
            }
        }

        public void ClearMergeSplitButtons() {
            if (ButtonSplitHorizontally.IsChecked == true) {
                ButtonSplitHorizontally.IsChecked = false;
                OnPropertyChanged(TwNotificationProperty.OnLeaveHorizontalSplitMode);
            }
            if (ButtonSplitVertically.IsChecked == true) {
                ButtonSplitVertically.IsChecked = false;
                OnPropertyChanged(TwNotificationProperty.OnLeaveVerticalSplitMode);
            }
            if (ButtonMergeAllIntoBase.IsChecked == true) {
                ButtonMergeAllIntoBase.IsChecked = false;
                OnPropertyChanged(TwNotificationProperty.OnLeaveMergeAllIntoBaseMode);
            }
            if (Mouse.OverrideCursor == Cursors.IBeam || Mouse.OverrideCursor == Cursors.Cross)
                Mouse.OverrideCursor = null;
        }
        
        void SetCanStartDragging(Panel panel, bool value) {
            foreach(var control in panel.Children) {
                if (control is FixtureButton) {
                    var fixtureButton = control as FixtureButton;
                    fixtureButton.CanStartDragging = value;
                }
            }
        }

        bool canStartDragging = true;
        public bool CanStartDragging {
            get { return canStartDragging; }
            set {
                canStartDragging = value;
                SetCanStartDragging(panelFixturesHighFrequency, value);
                SetCanStartDragging(panelFixturesLowFrequency, value);
                SetCanStartDragging(panelFirstCycle, value);
            }
        }
        
        void InitializeFixtureButtons() {
            foreach (var fixtureClass in FixtureClasses.Items.Values) {
                var button = new FixtureButton();
                button.FixtureClass = fixtureClass;
                button.Initialize();
                button.PropertyChanged +=new PropertyChangedEventHandler(buttonFixture_PropertyChanged);

                if (fixtureClass.LowFrequency)
                    panelFixturesLowFrequency.Children.Add(button);
                else
                    panelFixturesHighFrequency.Children.Add(button);
            }

            ButtonMoreFixtures_Click(null,null);
            ButtonMoreFixtures.Click += new RoutedEventHandler(ButtonMoreFixtures_Click);
            
            ButtonUserClassifyFirstCycles.PropertyChanged += new PropertyChangedEventHandler(buttonFixture_PropertyChanged);
        }

        public void buttonFixture_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case TwNotificationProperty.OnStartDrag:
                    OnPropertyChanged(TwNotificationProperty.OnStartDrag);
                    break;
                case TwNotificationProperty.OnEndDrag:
                    OnPropertyChanged(TwNotificationProperty.OnEndDrag);
                    break;
            }
        }

        Visibility SetVisibility() {
            return Properties.Settings.Default.ShowGraphToolBar ? Visibility.Visible : Visibility.Collapsed;
        }

        public void Default_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "ShowGraphToolBarAboveGraph":
                    SetTopAlignment();
                    SetSettingTopAlignment();
                    break;
                case "ShowGraphToolBar":
                    Visibility = SetVisibility();
                    break;
                case "ShowFixturesInGraphToolBar":
                    SetShowFixtures();
                    SetSettingShowFixtures();
                    break;
                case "SingleLineGraphToolBar":
                    //SetSingleLine();
                    //SetSettingSingleLine();
                    break;
            }
        }

        public static readonly DependencyProperty TopAlignmentProperty =
          DependencyProperty.Register("TopAlignment", typeof(bool),
          typeof(GraphToolBar), new UIPropertyMetadata(false));

        public bool TopAlignment {
            get { return (bool)GetValue(TopAlignmentProperty); }
            set {
                SetValue(TopAlignmentProperty, value);
                SetTopAlignment();
            }
        }

        void SetTopAlignment() {
            var panel = this.Parent as DockPanel;
            DockPanel.SetDock((DockPanel)this.Parent, TopAlignment ? Dock.Top : Dock.Bottom);

            SetSingleLine();
        }

        void SetSettingTopAlignment() {
            TopAlignment = Properties.Settings.Default.ShowGraphToolBarAboveGraph;
        }

        void InitializeApplyConversionFactorButton() {
            ButtonApplyConversionFactor.MouseEnter += new MouseEventHandler(ButtonApplyConversionFactor_MouseEnter);
        }

        void ButtonApplyConversionFactor_MouseEnter(object sender, MouseEventArgs e) {
            ButtonApplyConversionFactor.ToolTip = "Apply Conversion Factor (Ctrl+Shift+A)" + "\r\n" + "Conversion Factor: " + Analysis.Events.ConversionFactor.ToString("0.0000");
        }

        void InitializeUndoButtons() {
            ButtonUndo.MouseEnter +=new MouseEventHandler(ButtonUndo_MouseEnter);
            ButtonRedo.MouseEnter += new MouseEventHandler(ButtonRedo_MouseEnter);
        }

        void ButtonUndo_MouseEnter(object sender, MouseEventArgs e) {
            if (UndoManager.CanUndo())
                ButtonUndo.ToolTip = "Undo " + UndoManager.CurrentUndoTask().ToString() + "\r\n(Ctrl+Z)";
        }

        void ButtonRedo_MouseEnter(object sender, MouseEventArgs e) {
            if (UndoManager.CanRedo())
                ButtonRedo.ToolTip = "Redo " + UndoManager.CurrentRedoTask().ToString() + "\r\n(Alt+Shift+Backspace)";
        }

        void InitializeClassifyButtons() {
            ButtonClassifyUsingMachineLearning.ToolTip += "\r\n(" + ClassifierMachineLearning.Name + ")";
            ButtonClassifyFirstCyclesUsingMachineLearning.Visibility = Properties.Settings.Default.ShowClassifyFirstCyclesUsingMachineLearningButton ? Visibility.Visible : Visibility.Collapsed;
        }

        public static readonly DependencyProperty ShowFixturesProperty =
          DependencyProperty.Register("ShowFixtures", typeof(bool),
          typeof(GraphToolBar), new UIPropertyMetadata(true));

        public bool ShowFixtures {
            get { return (bool)GetValue(ShowFixturesProperty); }
            set {
                SetValue(ShowFixturesProperty, value);
                SetShowFixtures();
            }
        }
        void SetShowFixtures() {
            panelFixtures.Visibility = ShowFixtures ? Visibility.Visible : Visibility.Collapsed;
        }

        void SetSettingShowFixtures() {
            ShowFixtures = Properties.Settings.Default.ShowFixturesInGraphToolBar;
        }

        public static readonly DependencyProperty SingleLineProperty =
          DependencyProperty.Register("SingleLine", typeof(bool),
          typeof(GraphToolBar), new UIPropertyMetadata(true));

        public bool SingleLine {
            get { return (bool)GetValue(SingleLineProperty); }
            set {
                SetValue(SingleLineProperty, value);
                SetSingleLine();
            }
        }
        void SetSingleLine() {
            if (TopAlignment) {
                DockPanel.SetDock(panelTools, SingleLine ? Dock.Left : Dock.Top);
                DockPanel.SetDock(panelFixtures, SingleLine ? Dock.Right : Dock.Bottom);
            } else {
                DockPanel.SetDock(panelTools, SingleLine ? Dock.Left : Dock.Bottom);
                DockPanel.SetDock(panelFixtures, SingleLine ? Dock.Right : Dock.Top);
            }

            separatorPanelFixtures.Visibility = SingleLine ? Visibility.Visible : Visibility.Collapsed;
        }

        void SetSettingSingleLine() {
            SingleLine = Properties.Settings.Default.SingleLineGraphToolBar;
        }

    }
}
