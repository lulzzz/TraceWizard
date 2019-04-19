using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;

using TraceWizard.Entities;

namespace TraceWizard.TwApp {
    public partial class TimeFramePanel : UserControl {
        public Events Events;

        public TimeFramePanel() {
            InitializeComponent();
        }

        public void Initialize() {

            SetSettingTopAlignment();
            SetSettingShowView();
            SetSettingShowTrace();
            SetBindings();

            Properties.Settings.Default.PropertyChanged += new PropertyChangedEventHandler(Default_PropertyChanged);
        }

        public void ClearBindings() {
            BindingOperations.ClearBinding(textTraceStartTime, TextBlock.TextProperty);
            BindingOperations.ClearBinding(textTraceStartDate, TextBlock.TextProperty);
            BindingOperations.ClearBinding(textTraceDuration, TextBlock.TextProperty);
            BindingOperations.ClearBinding(textTraceDuration, TextBlock.ToolTipProperty);
            BindingOperations.ClearBinding(textTraceEndTime, TextBlock.TextProperty);
            BindingOperations.ClearBinding(textTraceEndDate, TextBlock.TextProperty);

            BindingOperations.ClearBinding(textViewStartTime, TextBlock.TextProperty);
            BindingOperations.ClearBinding(textViewStartDate, TextBlock.TextProperty);
            BindingOperations.ClearBinding(textViewDuration, TextBlock.TextProperty);
            BindingOperations.ClearBinding(textViewEndTime, TextBlock.TextProperty);
            BindingOperations.ClearBinding(textViewEndDate, TextBlock.TextProperty);
        }

        public void SetBindings() {
            Binding binding = new Binding("StartTime");
            binding.Source = Events;
            binding.Converter = new TwHelper.LongTimeConverter();
            BindingOperations.SetBinding(textTraceStartTime, TextBlock.TextProperty, binding);

            binding = new Binding("StartTime");
            binding.Source = Events;
            binding.Converter = new TwHelper.ShortDateConverter();
            BindingOperations.SetBinding(textTraceStartDate, TextBlock.TextProperty, binding);

            binding = new Binding("Duration");
            binding.Source = Events;
            binding.Converter = new TwHelper.DurationConverter();
            BindingOperations.SetBinding(textTraceDuration, TextBlock.TextProperty, binding);

            binding = new Binding("Duration");
            binding.Source = Events;
            binding.Converter = new TwHelper.FriendlierDurationConverter();
            BindingOperations.SetBinding(textTraceDuration, TextBlock.ToolTipProperty, binding);

            binding = new Binding("EndTime");
            binding.Source = Events;
            binding.Converter = new TwHelper.LongTimeConverter();
            BindingOperations.SetBinding(textTraceEndTime, TextBlock.TextProperty, binding);

            binding = new Binding("EndTime");
            binding.Source = Events;
            binding.Converter = new TwHelper.ShortDateConverter();
            BindingOperations.SetBinding(textTraceEndDate, TextBlock.TextProperty, binding);
        }

        public void Default_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "ShowTimeFrameAboveGraph":
                    SetTopAlignment();
                    SetSettingTopAlignment();
                    break;
                case "ShowViewTimeFrame":
                    //SetShowView();
                    //SetSettingShowView();
                    break;
                case "ShowTraceTimeFrame":
                    //SetShowTrace();
                    //SetSettingShowTrace();
                    break;
            }
        }

        public static readonly DependencyProperty TopAlignmentProperty =
          DependencyProperty.Register("TopAlignment", typeof(bool),
          typeof(TimeFramePanel), new UIPropertyMetadata(true));

        public bool TopAlignment {
            get { return (bool)GetValue(TopAlignmentProperty); }
            set {
                SetValue(TopAlignmentProperty, value);
                SetTopAlignment();
            }
        }

        void SetTopAlignment() {
            DockPanel.SetDock((DockPanel)this.Parent, TopAlignment ? Dock.Top : Dock.Bottom);

            foreach(FrameworkElement item in Grid.Children) {
                if (item.Name.ToLower().Contains("view"))
                    Grid.SetRow(item, TopAlignment ? 1 : 0);
                else if (item.Name.ToLower().Contains("trace"))
                    Grid.SetRow(item, TopAlignment ? 0 : 1);
            }
        }

        void SetSettingTopAlignment() {
            TopAlignment = Properties.Settings.Default.ShowTimeFrameAboveGraph;
        }

        public static readonly DependencyProperty ShowViewProperty =
          DependencyProperty.Register("ShowView", typeof(bool),
          typeof(TimeFramePanel), new UIPropertyMetadata(true));

        public bool ShowView {
            get { return (bool)GetValue(ShowViewProperty); }
            set {
                SetValue(ShowViewProperty, value);
                SetShowView();
            }
        }
        void SetShowView() {
            foreach(FrameworkElement item in Grid.Children) {
                if (item.Name.ToLower().Contains("view"))
                    item.Visibility = ShowView ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        void SetSettingShowView() {
            ShowView = Properties.Settings.Default.ShowViewTimeFrame;
        }
        
        public static readonly DependencyProperty ShowTraceProperty =
          DependencyProperty.Register("ShowTrace", typeof(bool),
          typeof(TimeFramePanel), new UIPropertyMetadata(true));

        public bool ShowTrace {
            get { return (bool)GetValue(ShowTraceProperty); }
            set {
                SetValue(ShowTraceProperty, value);
                SetShowTrace();
            }
        }
        void SetShowTrace() {
            foreach (FrameworkElement item in Grid.Children) {
                if (item.Name.ToLower().Contains("trace"))
                    item.Visibility = ShowTrace ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        void SetSettingShowTrace() {
            ShowTrace = Properties.Settings.Default.ShowTraceTimeFrame;
        }

    }
}
