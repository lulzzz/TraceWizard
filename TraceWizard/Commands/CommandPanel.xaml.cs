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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.ComponentModel;

using TraceWizard.Entities;
using TraceWizard.Commanding;
using TraceWizard.Notification;

namespace TraceWizard.TwApp {
    public partial class CommandPanel : UserControl, INotifyPropertyChanged {

        CommandEngine engine = null;
        public AnalysisPanel AnalysisPanel;

        public CommandPanel() {
            InitializeComponent();
        }
        
        public void Initialize() {

            engine = new CommandEngine(AnalysisPanel);

            ButtonExecute.Click += new RoutedEventHandler(button_Click);
            TextBox.PreviewKeyUp += new KeyEventHandler(textBox_PreviewKeyUp);

            ToolTipService.SetShowDuration(TextBlockResult, 60000);
            ToolTipService.SetInitialShowDelay(TextBlockResult, 500);

            AnalysisPanel.Analysis.Events.PropertyChanged += new PropertyChangedEventHandler(analysisPanel_PropertyChanged);

            TextBox.MouseEnter +=new MouseEventHandler(TextBox_MouseEnter);
            TextBox.MouseLeave += new MouseEventHandler(TextBox_MouseLeave);

            Properties.Settings.Default.PropertyChanged +=new PropertyChangedEventHandler(Default_PropertyChanged);

            Visibility = SetVisibility();

            this.LostFocus +=new RoutedEventHandler(CommandPanel_LostFocus);
        }

        void CommandPanel_LostFocus(object sender, RoutedEventArgs e) {
            ;
        }
        
        Visibility SetVisibility() {
            return Properties.Settings.Default.ShowCommandPanel ? Visibility.Visible : Visibility.Collapsed;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Default_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "ShowCommandPanel":
//                    Visibility = SetVisibility();
                    break;}
        }
       
        void button_Click(object sender, System.Windows.RoutedEventArgs e) {
            ExecuteCommand(TextBox.Text);
        }

        void textBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            switch (e.Key) {
                case Key.Enter:
                    e.Handled = true;
                    ExecuteCommand(TextBox.Text);
                    break;
                //case Key.PageUp:
                //case Key.PageDown:
                //case Key.Home:
                //case Key.End:
                //case Key.Right:
                //case Key.Left:
                //case Key.Up:
                //case Key.Down:
                //    e.Handled = true;
                //    break;
            }
        }

        void ExecuteCommand(string command) {
            var parser = new CommandParser();
            var parsedResult = parser.Parse(command);

            if (!string.IsNullOrEmpty(parsedResult.ErrorMessage)) {
                TextBlockResult.Foreground = Brushes.Red;
                TextBlockResult.Text = parsedResult.ErrorMessage;
                TextBlockResult.ToolTip = parsedResult.ErrorMessage;
                OnPropertyChanged(TwNotificationProperty.OnCommandError);
            } else {
                var commandResult = engine.Execute(parsedResult.Command);

                if (!string.IsNullOrEmpty(commandResult.ErrorMessage)) {
                    TextBlockResult.Foreground = Brushes.Red;
                    TextBlockResult.Text = commandResult.ErrorMessage;
                    TextBlockResult.ToolTip = commandResult.ErrorMessage;
                    OnPropertyChanged(TwNotificationProperty.OnCommandError);
                } else {
                    TextBlockResult.Foreground = Brushes.Black;
                    TextBlockResult.Text = string.IsNullOrEmpty(commandResult.SuccessMessage)? parsedResult.SuccessMessage : commandResult.SuccessMessage;
                    TextBlockResult.ToolTip = string.IsNullOrEmpty(commandResult.SuccessMessage) ? parsedResult.SuccessMessage : commandResult.SuccessMessage;
                    if (string.IsNullOrEmpty(commandResult.SuccessMessage))
                        OnPropertyChanged(TwNotificationProperty.OnCommandSuccess);
                    else
                        OnPropertyChanged(TwNotificationProperty.OnCommandError);
                }
            }
        }

        void Clear() {
            TextBlockResult.Text = string.Empty;
        }
        
        public void analysisPanel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case TwNotificationProperty.OnEndSelect:
                    Clear();
                    break;
            }
        }

        void TextBox_MouseEnter(object sender, MouseEventArgs e) {
            OnPropertyChanged(TwNotificationProperty.OnMouseEnterCommand);
        }

        void TextBox_MouseLeave(object sender, MouseEventArgs e) {
            OnPropertyChanged(TwNotificationProperty.OnMouseLeaveCommand);
        }

    }
}
