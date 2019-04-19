//using System;
//using System.Windows;
//using System.Speech.Recognition;
//using System.Speech.Recognition.SrgsGrammar;
//using System.ComponentModel;

//using TraceWizard.Entities;
//using TraceWizard.Services;

//namespace TraceWizard.TwApp {

//    public class SpeechEnabledEventsViewer : EventsViewer {

//        public SpeechEnabledEventsViewer() : base() {
//            IsSpeechRecognitionEnabled = Properties.Settings.Default.EnableSpeechRecognition;

////            Properties.Settings.Default.PropertyChanged += new PropertyChangedEventHandler(PropertyChanged);
//        }

//        public void PropertyChanged(object sender, PropertyChangedEventArgs e) {
//            //switch (e.PropertyName) {
//            //    case Properties.Settings.Default.IsSpeechRecognitionEnabled:
//            //        break;
//            //}
//        }

//        SpeechRecognizer speechRecognizer { get; set; }

//        public bool IsSpeechRecognitionEnabled {
//            get { return (bool)GetValue(IsSpeechRecognitionEnabledProperty); }
//            set {
//                SetValue(IsSpeechRecognitionEnabledProperty, value);

//                if (value && speechRecognizer == null)
//                    InitializeSpeechRecognition();
//                else if (!value && speechRecognizer != null)
//                    TerminateSpeechRecognition();
//            }
//        }

//        public static readonly DependencyProperty IsSpeechRecognitionEnabledProperty =
//            DependencyProperty.Register("IsSpeechRecognitionEnabled", typeof(bool), null,
//            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsSpeechRecognitionEnabledChanged)));

//        private static void OnIsSpeechRecognitionEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
//            Properties.Settings.Default.EnableSpeechRecognition = (bool)args.NewValue;

//            Properties.Settings.Default.Save();
//        }
        
//    //public bool IsSpeechEnabled {
//        //    get { return (speechRecognizer == null); }
//        //    set {
//        //        if (value == true && speechRecognizer == null) {
//        //            InitializeSpeechRecognition();
//        //        } else if (value == false && speechRecognizer != null) {
//        //            TerminateSpeechRecognition();
//        //        }
//        //    }
//        //}

//        void InitializeSpeechRecognition() {
//            try {
//                speechRecognizer = new SpeechRecognizer();
//                speechRecognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(speechRecognizer_SpeechRecognized);
//                speechRecognizer.SpeechHypothesized += new EventHandler<SpeechHypothesizedEventArgs>(speechRecognizer_SpeechHypothesized);

//                Choices choices = new Choices();
//                foreach (FixtureClass fixtureClass in FixtureClasses.Items.Values)
//                    choices.Add(fixtureClass.FriendlyName);

//                GrammarBuilder builder = new GrammarBuilder(choices);
//                speechRecognizer.LoadGrammar(new Grammar(builder));
//            } catch {
//                ;
//            }
//        }

//        void TerminateSpeechRecognition() {
//            speechRecognizer = null;
//        }

//        void speechRecognizer_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e) {
//            ; ; 
//        }

//        void speechRecognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e) {

//            FixtureClass fixtureClass = FixtureClasses.GetByFriendlyName(e.Result.Text);
//            if (fixtureClass == null)
//                return;
            
//            LinedEventsCanvas.Events.UserClassify(LinedEventsCanvas.Events.SelectedEvents, fixtureClass, false, false);
//        }
//    }
//}
