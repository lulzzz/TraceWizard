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
using TraceWizard.Notification;

namespace TraceWizard.TwApp {
    public partial class FixtureProfilesEditor : UserControl {

        public Analysis Analysis;
        public FixtureProfileRow RowSelected = null;
        public UndoPosition UndoPosition = new UndoPosition();

        public FixtureProfilesEditor() {
            InitializeComponent();
        }

        public void Initialize() {

            this.AllowDrop = true;
            this.Drop += new DragEventHandler(DragDrop);

            InitializeHeader();
            PopulateRows();

            Analysis.Events.PropertyChanged += new PropertyChangedEventHandler(Events_PropertyChanged);
        }

        public void ApplyFixture(List<Event> events) {
            int i = 0;
            foreach (Event @event in events)
                if (i++ < 100)
                    Apply(@event);
                else
                    break;
        }

        public void AddFixture(List<Event> events) {
            int i = 0;
            foreach (Event @event in events)
                if (i++ < 100)
                    AddFixture(@event);
                else
                    break;
        }
        
        public void MoveUp(FixtureProfileRow row) {
            if (RowSelected == null)
                return;

            var fixtureProfile = row.FixtureProfile;
            int index = Analysis.FixtureProfiles.IndexOf(fixtureProfile);

            if (index == 0) {
                return;
            }

            Analysis.FixtureProfiles.Remove(fixtureProfile);
            Analysis.FixtureProfiles.Insert(index - 1, fixtureProfile);

            StackPanelRows.Children.Remove(row);
            StackPanelRows.Children.Insert(index - 1, row);
        }

        public void MoveDown(FixtureProfileRow row) {
            if (RowSelected == null)
                return;

            var fixtureProfile = row.FixtureProfile;
            int index = Analysis.FixtureProfiles.IndexOf(fixtureProfile);
            if (index == Analysis.FixtureProfiles.Count - 1) {
                return;
            }

            Analysis.FixtureProfiles.Remove(fixtureProfile);
            Analysis.FixtureProfiles.Insert(index + 1,fixtureProfile);

            StackPanelRows.Children.Remove(row);
            StackPanelRows.Children.Insert(index + 1, row);
        }

        public void Delete(FixtureProfileRow row) {
            if (RowSelected == null)
                return;

            Analysis.FixtureProfiles.Remove(row.FixtureProfile);

            int index = StackPanelRows.Children.IndexOf(row);
            StackPanelRows.Children.Remove(row);

            if (index == 0) {
                if (StackPanelRows.Children.Count > 0)
                    index = 0;
                else
                    return;
            } else 
                index = index - 1;

            SelectRow((FixtureProfileRow)(StackPanelRows.Children[index]));
        }

        public void UnselectAll() {
            foreach(FixtureProfileRow row in StackPanelRows.Children)
                row.ClearSelection();
            RowSelected = null;
        }
        
        public void SelectRow(FixtureProfileRow rowToSelect) {
            bool clearSelection = true;
            foreach (FixtureProfileRow row in StackPanelRows.Children) {
                if (row == rowToSelect) {
                    if (row == RowSelected)
                        row.ClearSelection();
                    else {
                        row.Select();
                        RowSelected = row;
                        if (row.Button.IsChecked != true)
                            row.Button.IsChecked = true;
                        row.Button.Focus();
                        clearSelection = false;

                        BringRowIntoView(row);
                    }
                } else
                    row.ClearSelection();
            }

            if (clearSelection)
                RowSelected = null;
        }

        void BringRowIntoView(FixtureProfileRow row) {
            if (!IsAtLeastPartiallyInView(row))
                BringRowIntoViewLow(row);
        }

        bool IsAtLeastPartiallyInView(FixtureProfileRow row) {
            double percentElapsed = ((double)StackPanelRows.Children.IndexOf(row) + 1) / (double)StackPanelRows.Children.Count;
            double percentOneRow = (double)1.0 / (double)StackPanelRows.Children.Count;
            return ((percentElapsed + percentOneRow) * StackPanelRows.ActualHeight) > ScrollViewer.VerticalOffset
                && (percentElapsed * StackPanelRows.ActualHeight) < ScrollViewer.VerticalOffset + ScrollViewer.ViewportHeight;
        }

        public void BringRowIntoViewLow(FixtureProfileRow row) {
            double percentElapsed = ((double)StackPanelRows.Children.IndexOf(row) + 1)/ (double)StackPanelRows.Children.Count;
            if (((percentElapsed * StackPanelRows.ActualHeight) - ScrollViewer.ActualHeight / 2) >= (StackPanelRows.ActualHeight - ScrollViewer.ActualHeight))
                ScrollViewer.ScrollToEnd();
            else
                ScrollViewer.ScrollToVerticalOffset(EventsViewer.Between(0, StackPanelRows.ActualHeight - ScrollViewer.ActualHeight, ((percentElapsed * StackPanelRows.ActualHeight) - ScrollViewer.ActualHeight / 2)));
        }

        public void Events_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case TwNotificationProperty.OnStartClassify:
                    if (!Save())
                        MessageBox.Show("Unable to save Fixture List. Please check any recently-edited values in the Fixture List to make sure their format is valid.");
                    break;
            }
        }

        bool Save() {
            bool result = true;

            foreach (FixtureProfileRow row in StackPanelRows.Children) {

                row.FixtureProfile.FixtureClass = row.FixtureClassSelector.FixtureClass;

                double d;
                TimeSpan ts;
                if (double.TryParse(row.MinVolume.Text, out d))
                    row.FixtureProfile.MinVolume = d;
                else if (string.IsNullOrEmpty(row.MinVolume.Text))
                    row.FixtureProfile.MinVolume = null;
                else result= false;

                if (double.TryParse(row.MaxVolume.Text, out d))
                    row.FixtureProfile.MaxVolume = d;
                else if (string.IsNullOrEmpty(row.MaxVolume.Text))
                    row.FixtureProfile.MaxVolume = null;
                else result = false;

                if (result == double.TryParse(row.MinPeak.Text, out d))
                    row.FixtureProfile.MinPeak = d;
                else if (string.IsNullOrEmpty(row.MinPeak.Text))
                    row.FixtureProfile.MinPeak = null;
                else result = false;

                if (result == double.TryParse(row.MaxPeak.Text, out d))
                    row.FixtureProfile.MaxPeak = d;
                else if (string.IsNullOrEmpty(row.MaxPeak.Text))
                    row.FixtureProfile.MaxPeak = null;
                else result = false;

                if (result == double.TryParse(row.MinMode.Text, out d))
                    row.FixtureProfile.MinMode = d;
                else if (string.IsNullOrEmpty(row.MinMode.Text))
                    row.FixtureProfile.MinMode = null;
                else result = false;

                if (result == double.TryParse(row.MaxMode.Text, out d))
                    row.FixtureProfile.MaxMode = d;
                else if (string.IsNullOrEmpty(row.MaxMode.Text))
                    row.FixtureProfile.MaxMode = null;
                else result = false;

                if (string.IsNullOrEmpty(row.MinDuration.Text))
                    row.FixtureProfile.MinDuration = null;
                else if ((ts = (TimeSpan)(new TwHelper.FriendlyDurationConverter().ConvertBack(row.MinDuration.Text, null, null, null))) != TimeSpan.MinValue)
                    row.FixtureProfile.MinDuration = ts;
                else result = false;

                if (string.IsNullOrEmpty(row.MaxDuration.Text))
                    row.FixtureProfile.MaxDuration = null;
                else if ((ts = (TimeSpan)(new TwHelper.FriendlyDurationConverter().ConvertBack(row.MaxDuration.Text, null, null, null))) != TimeSpan.MinValue)
                    row.FixtureProfile.MaxDuration = ts;
                else result = false;
                
            }
            return result;
        }
        
        void InitializeHeader() {
            FixtureProfileHeader.AllowDrop = true;
            FixtureProfileHeader.Drop += new DragEventHandler(DragDrop);

            FixtureProfileHeader.Button.Focusable = false;
            FixtureProfileHeader.Button.IsTabStop = false;
            FixtureProfileHeader.Button.Visibility = Visibility.Collapsed;
            FixtureProfileHeader.FixtureClassSelector.Visibility = Visibility.Collapsed;
            FixtureProfileHeader.MinVolume.Visibility = Visibility.Hidden;
            FixtureProfileHeader.MaxVolume.Visibility = Visibility.Hidden;
            FixtureProfileHeader.BorderVolume.Visibility = Visibility.Collapsed;
            FixtureProfileHeader.MinPeak.Visibility = Visibility.Hidden;
            FixtureProfileHeader.MaxPeak.Visibility = Visibility.Hidden;
            FixtureProfileHeader.BorderPeak.Visibility = Visibility.Collapsed;
            FixtureProfileHeader.MinMode.Visibility = Visibility.Hidden;
            FixtureProfileHeader.MaxMode.Visibility = Visibility.Hidden;
            FixtureProfileHeader.BorderMode.Visibility = Visibility.Collapsed;
            FixtureProfileHeader.MinDuration.Visibility = Visibility.Hidden;
            FixtureProfileHeader.MaxDuration.Visibility = Visibility.Hidden;

            var textBlock = new TextBlock();
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.Style = (Style)ResourceLocator.FindResource("LabelStyle");
            Grid.SetColumn(textBlock, 2);
            Grid.SetColumnSpan(textBlock, 2);
            FixtureProfileHeader.Grid.Children.Add(textBlock);
            textBlock.Text = "Volume";

            textBlock = new TextBlock();
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.Style = (Style)ResourceLocator.FindResource("LabelStyle");
            Grid.SetColumn(textBlock, 5);
            Grid.SetColumnSpan(textBlock, 2);
            FixtureProfileHeader.Grid.Children.Add(textBlock);
            textBlock.Text = "Peak";

            textBlock = new TextBlock();
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.Style = (Style)ResourceLocator.FindResource("LabelStyle");
            Grid.SetColumn(textBlock, 8);
            Grid.SetColumnSpan(textBlock, 2);
            FixtureProfileHeader.Grid.Children.Add(textBlock);
            textBlock.Text = "Mode";

            textBlock = new TextBlock();
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.Style = (Style)ResourceLocator.FindResource("LabelStyle");
            Grid.SetColumn(textBlock, 11);
            Grid.SetColumnSpan(textBlock, 2);
            FixtureProfileHeader.Grid.Children.Add(textBlock);
            textBlock.Text = "Duration";
        }
        
        void PopulateRows() {
            ClearRows();
            foreach (FixtureProfile fixtureProfile in Analysis.FixtureProfiles) 
                AddRow(fixtureProfile);
        }

        void SetRowSelected(FixtureProfile fixtureProfile) {
            foreach (FixtureProfileRow row in StackPanelRows.Children) {
                if (fixtureProfile != null && row.FixtureProfile == fixtureProfile) {
                    SelectRow(row);
                    return;
                }
            }
        }

        void ClearRows() {
            StackPanelRows.Children.Clear();
        }

        public void DragDrop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(typeof(Polygon))) {
                DispatchDrop((Polygon)e.Data.GetData(typeof(Polygon)));
            } else if (e.Data.GetDataPresent(typeof(List<Polygon>))) {
                var polygons = (List<Polygon>)e.Data.GetData(typeof(List<Polygon>));
                foreach (Polygon polygon in polygons)
                    DispatchDrop(polygon);
            }
            e.Handled = true;
        }

        public void DragDrop(object sender, DragEventArgs e, FixtureProfile fixtureProfile) {
            if (e.Data.GetDataPresent(typeof(Polygon))) {
                DispatchDrop((Polygon)e.Data.GetData(typeof(Polygon)), fixtureProfile);
            } else if (e.Data.GetDataPresent(typeof(List<Polygon>))) {
                var polygons = (List<Polygon>)e.Data.GetData(typeof(List<Polygon>));
                foreach (Polygon polygon in polygons)
                    DispatchDrop(polygon, fixtureProfile);
            }
            e.Handled = true;
        }

        int GetInsertPosition(FixtureProfiles fixtureProfiles, FixtureClass fixtureClass) {
            for (int i = fixtureProfiles.Count - 1; i >=0; i--) {
                if (fixtureProfiles[i].FixtureClass == fixtureClass) {
                    return i + 1;
                } else if (fixtureClass == FixtureClasses.Irrigation) {
                    if (
                        fixtureProfiles[i].FixtureClass == FixtureClasses.Faucet
                        || fixtureProfiles[i].FixtureClass == FixtureClasses.Bathtub
                        || fixtureProfiles[i].FixtureClass == FixtureClasses.Shower
                        || fixtureProfiles[i].FixtureClass == FixtureClasses.Clotheswasher
                        || fixtureProfiles[i].FixtureClass == FixtureClasses.Toilet
                        || fixtureProfiles[i].FixtureClass == FixtureClasses.Leak
                        )
                        return i + 1;
                } else if (fixtureClass == FixtureClasses.Faucet) {
                    if (
                        fixtureProfiles[i].FixtureClass == FixtureClasses.Bathtub
                        || fixtureProfiles[i].FixtureClass == FixtureClasses.Shower
                        || fixtureProfiles[i].FixtureClass == FixtureClasses.Clotheswasher
                        || fixtureProfiles[i].FixtureClass == FixtureClasses.Toilet
                        || fixtureProfiles[i].FixtureClass == FixtureClasses.Leak
                        )
                        return i + 1;
                } else if (fixtureClass == FixtureClasses.Bathtub) {
                    if (
                        fixtureProfiles[i].FixtureClass == FixtureClasses.Shower
                        || fixtureProfiles[i].FixtureClass == FixtureClasses.Clotheswasher
                        || fixtureProfiles[i].FixtureClass == FixtureClasses.Toilet
                        || fixtureProfiles[i].FixtureClass == FixtureClasses.Leak
                        )
                        return i + 1;
                } else if (fixtureClass == FixtureClasses.Shower) {
                    if (
                        fixtureProfiles[i].FixtureClass == FixtureClasses.Clotheswasher
                        || fixtureProfiles[i].FixtureClass == FixtureClasses.Toilet
                        || fixtureProfiles[i].FixtureClass == FixtureClasses.Leak
                        )
                        return i + 1;
                } else if (fixtureClass == FixtureClasses.Clotheswasher) {
                    if (
                        fixtureProfiles[i].FixtureClass == FixtureClasses.Toilet
                        || fixtureProfiles[i].FixtureClass == FixtureClasses.Leak
                        )   
                        return i + 1;
                } else if (fixtureClass == FixtureClasses.Toilet) {
                    if (
                        fixtureProfiles[i].FixtureClass == FixtureClasses.Leak
                        )
                        return i + 1;
                } else if (fixtureClass == FixtureClasses.Leak) {
                    ;
                } else
                    return fixtureProfiles.Count;
            }

            return 0;
        }

        void DispatchDrop(Polygon polygon) {
            var @event = (polygon).Tag as Event;
            AddFixture(@event);
        }

        void DispatchDrop(Polygon polygon, FixtureProfile fixtureProfile) {
            var @event = (polygon).Tag as Event;
            if (fixtureProfile != null && fixtureProfile.FixtureClass == @event.FixtureClass)
                Apply(fixtureProfile,@event);
            else
                AddFixture(@event);
        }

        List<FixtureProfile> GetFixtureProfiles(FixtureClass fixtureClass) {
            var list = new List<FixtureProfile>();
            foreach (FixtureProfile fixtureProfile in Analysis.FixtureProfiles) {
                if (fixtureProfile.FixtureClass == fixtureClass)
                    list.Add(fixtureProfile);
            }
            return list;
        }

        void Apply(FixtureProfile fixtureProfile, Event @event) {
            fixtureProfile.Apply(@event);
            PopulateRows();
        }
        
        FixtureProfile Apply(Event @event) {
            var fixtureProfilesThisFixture = GetFixtureProfiles(@event.FixtureClass);
            FixtureProfile fixtureProfile = null;
            if (fixtureProfilesThisFixture.Count == 0) {
                fixtureProfile = AddFixture(@event);
                return fixtureProfile;
            } else if (fixtureProfilesThisFixture.Count == 1) {
                fixtureProfile = fixtureProfilesThisFixture[0];
                Apply(fixtureProfile, @event);
            } else {
                fixtureProfile = FixtureProfiles.GetMostSimilar(fixtureProfilesThisFixture, @event);
                Apply(fixtureProfile, @event);
            }
            SelectRow(RowFromProfile(fixtureProfile));
            return fixtureProfile;
        }

        FixtureProfileRow RowFromProfile(FixtureProfile fixtureProfile) {
            foreach (FixtureProfileRow row in StackPanelRows.Children) {
                if (row.FixtureProfile == fixtureProfile)
                    return row;
            } 
            return null;
        }

        FixtureProfile AddFixture(Event @event) {
            var fixtureProfile = FixtureProfile.FilterFixtureProfile(FixtureProfile.CreateFixtureProfile(@event));
            int position = GetInsertPosition(Analysis.FixtureProfiles, fixtureProfile.FixtureClass);
            Analysis.FixtureProfiles.Insert(position,fixtureProfile);
            PopulateRows();
            SelectRow(RowFromProfile(fixtureProfile));
            return fixtureProfile;
        }
        
        public void AddRow(FixtureProfile fixtureProfile) {
            var row = new FixtureProfileRow();
            row.FixtureProfile = fixtureProfile;
            row.FixtureProfilesEditor = this;
            row.Initialize();
            StackPanelRows.Children.Add(row);
        }

        public void TextBox_PreviewDragEnter(object sender, DragEventArgs e) {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }
    }
}
