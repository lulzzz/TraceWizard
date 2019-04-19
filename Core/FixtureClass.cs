using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace TraceWizard.Entities {

    public class FixtureClass {
        public FixtureClass(string name, string friendlyName, string labelName, string shortName, char character, Color color, string imageFilename, bool lowFrequency, bool canHaveCycles) {
            Name = name; FriendlyName = friendlyName; LabelName = labelName; ShortName = shortName; Character = character;  Color = color; ImageFilename = imageFilename; LowFrequency = lowFrequency; CanHaveCycles = canHaveCycles;
        }

        public string Name { get; set; }
        public string LabelName { get; set; }
        public string FriendlyName { get; set; }
        public string ShortName { get; set; }
        public char Character { get; set; }
        public Color Color { get; set; }
        public string ImageFilename { get; set; }
        public bool LowFrequency { get; set; }
        public bool CanHaveCycles { get; set; }
    }

    public static class FixtureClasses {

        public static Dictionary<string,FixtureClass> Items = new Dictionary<string,FixtureClass>();
        public static Dictionary<string, FixtureClass> ItemsDescending = new Dictionary<string, FixtureClass>();

        public static FixtureClass Unclassified = new FixtureClass("Unclassified", "Unclassified", "_Unclassified", "Uncl", 'u', Colors.Olive, null, true, false);
        public static FixtureClass Toilet = new FixtureClass("Toilet", "Toilet", "_Toilet", "Toilet", 't', Colors.Lime, "toilet.png", false, false);
        public static FixtureClass Dishwasher = new FixtureClass("Dishwasher", "Dishwasher", "_Dishwasher", "DW", 'd', Colors.Fuchsia, "dishwasher.png", false, true);
        public static FixtureClass Clotheswasher = new FixtureClass("Clotheswasher", "Clothes Washer", "_Clothes Washer", "CW", 'c', Colors.Cyan, "clotheswasher.png", false, true);
        public static FixtureClass Shower = new FixtureClass("Shower", "Shower", "_Shower", "Shower", 's', Colors.Red, "shower.png", false, false);
        public static FixtureClass Bathtub = new FixtureClass("Bathtub", "Bathtub", "_Bathtub", "Bath", 'b', Colors.Orange, "bathtub.png", false, false);
        public static FixtureClass Cooler = new FixtureClass("Cooler", "Cooler", "Cool_er", "Cool", 'e', Colors.Green, "cooler.png", true, false);
        public static FixtureClass Pool = new FixtureClass("Pool", "Pool", "_Pool", "Pool", 'p', Colors.Navy, "pool.png", true, false);
        public static FixtureClass Leak = new FixtureClass("Leak", "Leak", "_Leak", "Leak", 'l', Colors.Blue, "leak.png", false, false);
        public static FixtureClass Irrigation = new FixtureClass("Irrigation", "Irrigation", "_Irrigation", "Irrig", 'i', Colors.Black, "irrigation.png", false, false);
        public static FixtureClass Faucet = new FixtureClass("Faucet", "Faucet", "_Faucet", "Faucet", 'f', Colors.Yellow, "faucet.png", false, false);
        public static FixtureClass Other = new FixtureClass("Other", "Other", "_Other", "Other", 'o', Colors.Brown, "other.png", true, false);
        public static FixtureClass Humidifier = new FixtureClass("Humidifier", "Humidifier", "Hu_midifier", "Humid", 'm', Colors.Teal, "humidifier.png", true, false);
        public static FixtureClass Hottub = new FixtureClass("Hottub", "Hot Tub", "_Hot Tub", "Hot", 'h', Colors.Maroon, "hottub.png", true, false);
//        public static FixtureClass Newfixture = new FixtureClass("NewFixture", "Ne_w Fixture", "New Fixture", "New", 'w', Colors.GreenYellow, "newfixture.png", true, false);
        public static FixtureClass Treatment = new FixtureClass("Treatment", "Treatment", "T_reatment", "Treat", 'r', Colors.Magenta, "treatment.png", true, true);
        public static FixtureClass Noise = new FixtureClass("Noise", "Noise", "_Noise", "Noise", 'n', Colors.Purple, "noise.png", true, false);
        public static FixtureClass Duplicate = new FixtureClass("Duplicate", "Duplicate", "Duplic_ate", "Dupe", 'a', Colors.Purple, "duplicate.png", true, false);

        static FixtureClasses() {

            Items.Add(Toilet.Name, Toilet);
            Items.Add(Clotheswasher.Name, Clotheswasher);
            Items.Add(Shower.Name, Shower);
            Items.Add(Dishwasher.Name, Dishwasher);
            Items.Add(Faucet.Name, Faucet);
            Items.Add(Irrigation.Name, Irrigation);
            Items.Add(Bathtub.Name, Bathtub);
            Items.Add(Leak.Name, Leak);
            Items.Add(Humidifier.Name, Humidifier);
            Items.Add(Treatment.Name, Treatment);
            Items.Add(Cooler.Name, Cooler);
            Items.Add(Pool.Name, Pool);
            Items.Add(Hottub.Name, Hottub);
//            Items.Add(Newfixture.Name, Newfixture);
            Items.Add(Noise.Name, Noise);
            Items.Add(Duplicate.Name, Duplicate);
            Items.Add(Other.Name, Other);
            Items.Add(Unclassified.Name, Unclassified);

            PopulateDescending();
        }

        static private void PopulateDescending() {
            List<FixtureClass> listAscending = new List<FixtureClass>();
            foreach (FixtureClass fixtureClass in Items.Values)
                listAscending.Add(fixtureClass);
            for (int i = listAscending.Count - 1; i >= 0; i--) {
                FixtureClass fixtureClass = listAscending[i];
                ItemsDescending.Add(fixtureClass.Name, fixtureClass);
            }
        }

        public static FixtureClass GetByIndex(int i) {
            int count = 0;
            foreach (FixtureClass fixtureClass in Items.Values) {
                if (count++ == i)
                    return fixtureClass;
            }
            return FixtureClasses.Unclassified;
        }

        static string Singularize(string s) {
            if (s.Length > 1 && s[s.Length - 1] == 's')
                return s.Substring(0,s.Length - "s".Length);
            if (s.Length > 2 && s.Substring(s.Length - 2,2) == "es")
                return s.Substring(0, s.Length - "es".Length);
            else
                return s;
        }

        static bool IsPluralMatch(string source, string target) {
            if (target.EndsWith("es")) {
                if ((target.Substring(0,target.Length - "es".Length) == source))
                    return true;
            }

            if (target.EndsWith("s")) {
                if ((target.Substring(0,target.Length - "s".Length) == source))
                    return true;
            }
            return false;
        }

        public static FixtureClass GetByFriendlyName(string friendlyName) {
            foreach (FixtureClass fixtureClass in Items.Values) {
                if (fixtureClass.FriendlyName.ToLower() == friendlyName.ToLower())
                    return fixtureClass;
            }
            return null;
        }

        public static FixtureClass GetByName(string name) {
            foreach (FixtureClass fixtureClass in Items.Values) {
                if (fixtureClass.Name.ToLower() == name.ToLower())
                    return fixtureClass;
                else if (IsPluralMatch(fixtureClass.Name.ToLower(), name.ToLower()))
                    return fixtureClass;
            }
            return null;
        }

        public static FixtureClass GetByShortName(string name) {
            foreach (FixtureClass fixtureClass in Items.Values) {
                if (fixtureClass.ShortName.ToLower() == name.ToLower())
                    return fixtureClass;
                else if (IsPluralMatch(fixtureClass.ShortName.ToLower(), name.ToLower()))
                    return fixtureClass;
            }
            return null;
        }

        public static FixtureClass GetByCharacter(char ch) {
            foreach (FixtureClass fixtureClass in Items.Values) {
                if (char.ToUpper(fixtureClass.Character) == char.ToUpper(ch))
                    return fixtureClass;
            }
            return null;
        }

        public static FixtureClass GetByCharacter(string s) {
            foreach (FixtureClass fixtureClass in Items.Values) {
                if (char.ToUpper(fixtureClass.Character).ToString() == s.ToUpper())
                    return fixtureClass;
            }
            return null;
        }

    }
}
