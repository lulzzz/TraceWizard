using System;
using System.Collections.Generic;

using TraceWizard.Entities;
using TraceWizard.Adoption;

namespace TraceWizard.Adoption.Adopters.Null {
    public class NullAdopter : Adopter {
        public NullAdopter() : base() { }
        public NullAdopter(Events events) : base(events) { }

        public override void Adopt(Event @event) {
            ;
        }

        public override void Adopt(Event @event, List<UndoTaskClassify> undoTasks) {
            ;
        }

        public override void AdoptWithStatistics(Event @event) {
            ;
        }

        public override string Name { get { return "Null Adopter"; } }
        public override string Description { get { return "Performs no Adoption"; } }
    }
}
