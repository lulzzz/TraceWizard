using System;
using System.Windows.Data;
using System.Collections.Generic;
using System.ComponentModel;

using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TraceWizard.TwApp {

    public interface IProgressOperation {
        int Total { get; }
        int Current { get; }
        string KeyCode { get; }
        void Start();
        void CancelAsync();
        event EventHandler ProgressChanged;
        event EventHandler ProgressTotalChanged;
        event EventHandler Complete;
    }
}