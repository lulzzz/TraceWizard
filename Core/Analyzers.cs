using System;
using TraceWizard.Entities;

namespace TraceWizard.Analyzers {

    public interface Analyzer {
        string GetName();
        string GetDescription();
    }

    public abstract class AdapterFactory {
        public abstract Adapter GetAdapter(string dataSource);
    }

    public interface Adapter {
    }
}
