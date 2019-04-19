using System.Windows;
using System.Collections.Generic;

namespace TraceWizard.Reporters {
    public interface Exporter {
        void Export();
    }

    public interface Reporter {
        UIElement Report();
    }
    public interface MultiReporter {
        List<UIElement> Report();
    }
}