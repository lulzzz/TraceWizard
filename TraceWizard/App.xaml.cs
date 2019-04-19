using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Diagnostics;

using System.IO;

using System.Windows.Threading;

using TraceWizard.TwApp;
using TraceWizard.Environment;
using TraceWizard.Services;
using TraceWizard.Disaggregation.Disaggregators.Tw4;

namespace TraceWizardApp {
    public enum TwShutdownCode {
        Success = 0,
        VersionExpired = 1,
        CommandLineError = 2,
        ProgrammingError = 3
    }

    public partial class App : Application {

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            try {
                if (TwServices.DispatchUtilities(System.Environment.GetCommandLineArgs(), new Tw4PostTrickleMergeMidnightSplitDisaggregator())) {
                    Shutdown((int)TwShutdownCode.Success);
                    return;
                }
            } catch(Exception ex) {
                Console.WriteLine(ex.Message);
                Shutdown((int)TwShutdownCode.CommandLineError);
                return;
            }

            Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(AppDispatcherUnhandledException);
            base.Exit += new ExitEventHandler(App_Exit);
            this.StartupUri = new Uri(@"MainWindow\MainWindow.xaml", UriKind.Relative);
        }
        
        void App_Exit(object sender, ExitEventArgs e) {
            TraceWizard.TwApp.Properties.Settings.Default.Save();
        }

        void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
#if DEBUG // In debug mode do not custom-handle the exception, let Visual Studio handle it 
            ShowUnhandledException(e);
#else
            ShowUnhandledException(e);
#endif
        }

        void ShowUnhandledException(DispatcherUnhandledExceptionEventArgs e) {
            string message = "Trace Wizard has detected a problem. "; 
            if (e.Exception != null) {
                if (e.Exception.GetType() == typeof(System.OutOfMemoryException)) {
                    message += "Trace Wizard is exiting due to an out-of-memory condition.";
                } else {
                    message += "Exception Message: " + e.Exception.Message;
                    message += "\r\n\r\nIf you would like to report this problem to the developer, please make a note of the following information, especially the first file name and line number: ";
                    message += "\r\n\r\n";
                    message += "Exception Stack Trace: \r\n" + e.Exception.StackTrace;
                }
            }
            MessageBox.Show(message, "Trace Wizard Problem");

            e.Handled = true;
            Application.Current.Shutdown((int)TwShutdownCode.ProgrammingError);
        }
    }
}
