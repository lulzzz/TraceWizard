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

using TraceWizard.Entities;

namespace TraceWizard.TwApp {
    public partial class FixtureProfileRow : UserControl {

        public FixtureProfile FixtureProfile;
        public FixtureProfilesEditor FixtureProfilesEditor;

        public FixtureProfileRow() {
            InitializeComponent();
        }

        public void Initialize() {
            InitializeTextBoxes();
            InitializeButton();
            BuildRow();
        }

        void InitializeButton() {
            Button.PreviewKeyDown +=new KeyEventHandler(Button_PreviewKeyDown);
            Button.Click += new RoutedEventHandler(Button_Click);
        }

        public void TextBox_PreviewDrop(object sender, DragEventArgs e) {
            FixtureProfilesEditor.DragDrop(sender, e, FixtureProfile);
        }

        void Button_PreviewKeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Up:
                    FixtureProfilesEditor.MoveUp(this);
                    break;
                case Key.Down:
                    FixtureProfilesEditor.MoveDown(this);
                    break;
                case Key.Delete:
                case Key.Back:
                    FixtureProfilesEditor.Delete(this);
                    break;
            }
        }

        void Button_Click(object sender, RoutedEventArgs e) {
            FixtureProfilesEditor.SelectRow(this);
        }
        
        public void Select() {
            SetBrush(Brushes.LightGray);
        }

        void SetBrush(Brush brush) {
            foreach (var child in Grid.Children) {
                var textBox = child as TextBox;
                if (textBox != null)
                    textBox.Background = brush;
            }
            FixtureClassSelector.ComboBoxFixtureClass.Background = brush;
        }

        public void ClearSelection() {
            if (Button.IsChecked != false)
                Button.IsChecked = false;
            SetBrush(Brushes.Transparent);
        }

        void InitializeTextBoxes() {
            foreach (var child in Grid.Children) {
                var textBox = child as TextBox;
                if (textBox != null) {
                    textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                    textBox.VerticalAlignment = VerticalAlignment.Stretch;
                    textBox.HorizontalContentAlignment = HorizontalAlignment.Right;
                    textBox.VerticalContentAlignment = VerticalAlignment.Center;
                    textBox.Padding = new Thickness(0, 0, 3, 0);
                    textBox.Background = Brushes.Transparent;
                    textBox.AllowDrop = true;
//                    textBox.Drop += new DragEventHandler(FixtureProfilesEditor.DragDrop);
                    textBox.PreviewDragOver += new DragEventHandler(FixtureProfilesEditor.TextBox_PreviewDragEnter);
                    textBox.PreviewDragEnter += new DragEventHandler(FixtureProfilesEditor.TextBox_PreviewDragEnter);
                    textBox.PreviewDrop += new DragEventHandler(TextBox_PreviewDrop);
                    textBox.PreviewMouseDown +=new MouseButtonEventHandler(textBox_PreviewMouseDown);
                }
            }
        }

        void textBox_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if (FixtureProfilesEditor.RowSelected == this) {
                ClearSelection();
                FixtureProfilesEditor.RowSelected = null;
            }

//            e.Handled = true;
        }
        
        string GetString(double? value) {
            return value.HasValue ? value.Value.ToString("0.00") : string.Empty;
        }

        string GetString(TimeSpan? value) {
            return value.HasValue ? new TwHelper.LimitedDurationConverter().Convert(value, null, null, null).ToString() : string.Empty;
        }

        void BuildRow() {
            this.FixtureClassSelector.FixtureClass = FixtureProfile.FixtureClass;
            this.MinVolume.Text = GetString(FixtureProfile.MinVolume);
            this.MaxVolume.Text = GetString(FixtureProfile.MaxVolume);
            this.MinPeak.Text = GetString(FixtureProfile.MinPeak);
            this.MaxPeak.Text = GetString(FixtureProfile.MaxPeak);
            this.MinMode.Text = GetString(FixtureProfile.MinMode);
            this.MaxMode.Text = GetString(FixtureProfile.MaxMode);
            this.MinDuration.Text = GetString(FixtureProfile.MinDuration);
            this.MaxDuration.Text = GetString(FixtureProfile.MaxDuration);
        }

        public FixtureClassSelector AddFixtureClassSelector(FixtureClass fixtureClass, int row, int column) {
            FixtureClassSelector fixtureClassSelector = new FixtureClassSelector();
            fixtureClassSelector.Margin = new Thickness(0, 0, 0, 0);
            fixtureClassSelector.Padding = new Thickness(0, 0, 0, 0);
            fixtureClassSelector.FixtureClass = fixtureClass;
            Grid.SetRow(fixtureClassSelector, row);
            Grid.SetColumn(fixtureClassSelector, column);
            return fixtureClassSelector;
        }
    }
}
