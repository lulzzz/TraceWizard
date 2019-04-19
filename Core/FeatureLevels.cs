using System;
using System.IO;

using TraceWizard.Environment;

namespace TraceWizard.FeatureLevels {

    public class FeatureLevel {
        public bool IsDemo { get; protected set; }
        public bool IsPro { get; protected set; }

        public void Initialize() {
            if (ProKeyFound()) {
                IsPro = true;
            } else {
                IsDemo = true;
            }
        }

        public string Text { 
            get {
                if (IsPro) {
                    return "Pro";
                } else if (IsDemo) {
                    return "Demo";
                } else {
                    return string.Empty;
                }
            }
        }

        bool KeyFound(string filename) {
            if (File.Exists(Path.GetDirectoryName(TwAssembly.Path()) + "\\" + filename))
                return true;
            else
                return false;
        }

        public bool ProKeyFound() {
            return KeyFound("TraceWizardProKey.dll");
        }

        public static bool IsKeyFile(string filename) {
            if (filename.EndsWith("Key.dll", StringComparison.InvariantCultureIgnoreCase))
                return true;
            else
                return false;
        }
    }
}