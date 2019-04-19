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
using TraceWizard.Notification;

namespace TraceWizard.TwApp {

    public partial class StyledEventsViewer : UserControl {

        public Events Events { get; set; }
        public double ViewportSeconds { get; set; }
        public double ViewportVolume { get; set; }

        public StyledEventsViewer() {
            InitializeComponent();
        }

        public void Initialize() {

            EventsViewer.Events = Events;
            EventsViewer.ViewportSeconds = ViewportSeconds;
            EventsViewer.ViewportVolume = ViewportVolume;
            EventsViewer.HorizontalRuler = HorizontalRuler;
            EventsViewer.VerticalRuler = VerticalRuler;
            EventsViewer.SelectionRuler = SelectionRuler;
            EventsViewer.ApprovalRuler = ApprovalRuler;
            EventsViewer.ClassificationRuler = ClassificationRuler;
            EventsViewer.FixturesRuler = FixturesRuler;

            EventsViewer.Initialize();

            TimeFramePanel.Events = Events;
            TimeFramePanel.Initialize();

            SelectionRuler.Events = Events;
            SelectionRuler.Initialize();

            ApprovalRuler.Events = Events;
            ApprovalRuler.Initialize();

            ClassificationRuler.Events = Events;
            ClassificationRuler.Initialize();

            FixturesRuler.Events = Events;
            FixturesRuler.ViewportSeconds = ViewportSeconds;
            FixturesRuler.Initialize();

            BindTimeFrameView();
            ScrollViewersSetBindings();

            Events.PropertyChanged += new PropertyChangedEventHandler(Events_PropertyChanged);
        }

        void ScrollViewersSetBindings() {
            BindHorizontalScrollViewers(EventsViewer.ScrollViewer, EventsViewer.HorizontalRuler.ScrollViewer);
            BindVerticalScrollViewers(EventsViewer.ScrollViewer, EventsViewer.VerticalRuler.ScrollViewer);

            BindHorizontalScrollViewers(EventsViewer.ScrollViewer, SelectionRuler.ScrollViewer);
            BindHorizontalScrollViewers(EventsViewer.ScrollViewer, ApprovalRuler.ScrollViewer);
            BindHorizontalScrollViewers(EventsViewer.ScrollViewer, ClassificationRuler.ScrollViewer);
            BindHorizontalScrollViewers(EventsViewer.ScrollViewer, FixturesRuler.ScrollViewer);
        }

        public void Events_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case TwNotificationProperty.OnChangeStartTime:
                    WorkaroundForBindingProblem();
                    break;
                case TwNotificationProperty.OnEndApplyConversionFactor:
                    RedrawVolumesChanged();
                    break;
            }
        }

        void RedrawVolumesChanged() {
            EventsViewer.ScrollChanged(true);
            EventsViewer.RestoreHorizontalScrollPosition(EventsViewer.UndoPosition);
        }

        void WorkaroundForBindingProblem() {
            TimeFramePanel.ClearBindings();
            TimeFramePanel.SetBindings();
            BindTimeFrameView();
            this.EventsViewer.RenderRowsColumnsRulers(ViewportSeconds, ViewportVolume);
        }

        void BindHorizontalScrollViewers(ScrollViewer source, ScrollViewer target) {
            target.IsEnabled = false;
            target.IsTabStop = false;
            target.Focusable = false;

            TranslateTransform translateTransform = new TranslateTransform();

            Binding bindingHorizontal = new Binding("HorizontalOffset");
            bindingHorizontal.Source = source;
            bindingHorizontal.Converter = new ScrollViewerHorizontalConverter();
            BindingOperations.SetBinding(translateTransform, TranslateTransform.XProperty, bindingHorizontal);

            Canvas canvas = (Canvas)target.Content;
            canvas.RenderTransform = translateTransform;
        }

        void BindVerticalScrollViewers(ScrollViewer source, ScrollViewer target) {
            target.IsEnabled = false;
            target.IsTabStop = false;
            target.Focusable = false;

            TranslateTransform translateTransform = new TranslateTransform();

            Binding bindingVertical = new Binding("VerticalOffset");
            bindingVertical.Source = source;
            bindingVertical.Converter = new ScrollViewerHorizontalConverter();
            BindingOperations.SetBinding(translateTransform, TranslateTransform.YProperty, bindingVertical);

            Canvas canvas = (Canvas)target.Content;
            canvas.RenderTransform = translateTransform;
        }

        void BindTimeFrameView() {
            Binding bindingHorizontalOffset = new Binding("HorizontalOffset");
            bindingHorizontalOffset.Source = EventsViewer.ScrollViewer;

            Binding bindingViewportWidth = new Binding("ViewportWidth");
            bindingViewportWidth.Source = EventsViewer.ScrollViewer;

            Binding bindingWidth = new Binding("Width");
            bindingWidth.Source = EventsViewer.LinedEventsCanvas;

            MultiBinding multiBinding;

            multiBinding = new MultiBinding();
            multiBinding.Bindings.Add(bindingHorizontalOffset);
            multiBinding.Bindings.Add(bindingViewportWidth);
            multiBinding.Bindings.Add(bindingWidth);
            multiBinding.Converter = new TwHelper.ViewportStartTimeConverter();
            multiBinding.ConverterParameter = Events.TimeFrame;
            BindingOperations.SetBinding(TimeFramePanel.textViewStartTime, TextBlock.TextProperty, multiBinding);

            multiBinding = new MultiBinding();
            multiBinding.Bindings.Add(bindingHorizontalOffset);
            multiBinding.Bindings.Add(bindingViewportWidth);
            multiBinding.Bindings.Add(bindingWidth);
            multiBinding.Converter = new TwHelper.ViewportStartDateConverter();
            multiBinding.ConverterParameter = Events.TimeFrame;
            BindingOperations.SetBinding(TimeFramePanel.textViewStartDate, TextBlock.TextProperty, multiBinding);

            multiBinding = new MultiBinding();
            multiBinding.Bindings.Add(bindingHorizontalOffset);
            multiBinding.Bindings.Add(bindingViewportWidth);
            multiBinding.Bindings.Add(bindingWidth);
            multiBinding.Converter = new TwHelper.ViewportEndTimeConverter();
            multiBinding.ConverterParameter = Events.TimeFrame;
            BindingOperations.SetBinding(TimeFramePanel.textViewEndTime, TextBlock.TextProperty, multiBinding);

            multiBinding = new MultiBinding();
            multiBinding.Bindings.Add(bindingHorizontalOffset);
            multiBinding.Bindings.Add(bindingViewportWidth);
            multiBinding.Bindings.Add(bindingWidth);
            multiBinding.Converter = new TwHelper.ViewportEndDateConverter();
            multiBinding.ConverterParameter = Events.TimeFrame;
            BindingOperations.SetBinding(TimeFramePanel.textViewEndDate, TextBlock.TextProperty, multiBinding);

            multiBinding = new MultiBinding();
            multiBinding.Bindings.Add(bindingHorizontalOffset);
            multiBinding.Bindings.Add(bindingViewportWidth);
            multiBinding.Bindings.Add(bindingWidth);
            multiBinding.Converter = new TwHelper.ViewportDurationConverter();
            multiBinding.ConverterParameter = Events.TimeFrame;
            BindingOperations.SetBinding(TimeFramePanel.textViewDuration, TextBlock.TextProperty, multiBinding);
        }
    }
}
