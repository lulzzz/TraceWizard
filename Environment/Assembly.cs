using System;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace TraceWizard.Environment {

    public static class TwAssembly {

        public static string TitleTraceWizard() { return Title(TwEnvironment.TwExecutable); }

        public static string CompanyAndTitle() { return Company() + " " + Title(); }
        public static string Title() { return GetEntryAssemblyAttribute<AssemblyTitleAttribute>().Title; }
        public static string Product() { return GetEntryAssemblyAttribute<AssemblyProductAttribute>().Product; }
        public static string Description() { return GetEntryAssemblyAttribute<AssemblyDescriptionAttribute>().Description; }
        public static string Copyright() { return GetEntryAssemblyAttribute<AssemblyCopyrightAttribute>().Copyright; }
        public static string Company() { return GetEntryAssemblyAttribute<AssemblyCompanyAttribute>().Company; }
        public static Version Version() { return System.Reflection.Assembly.GetEntryAssembly().GetName().Version; }
        public static string CompleteVersion() { return
            Version().Major + "." + Version().Minor 
            + "." + Version().Build + "." + Version().Revision;
        }
        public static string Name() { return System.Reflection.Assembly.GetEntryAssembly().GetName().Name; }

        public static string Path() { return System.Reflection.Assembly.GetEntryAssembly().Location; }

        public static string Title(string assemblyFile) { return GetAssemblyAttribute<AssemblyTitleAttribute>(assemblyFile).Title; }
        public static string Product(string assemblyFile) { return GetAssemblyAttribute<AssemblyProductAttribute>(assemblyFile).Product; }
        public static string Description(string assemblyFile) { return GetAssemblyAttribute<AssemblyDescriptionAttribute>(assemblyFile).Description; }
        public static string Copyright(string assemblyFile) { return GetAssemblyAttribute<AssemblyCopyrightAttribute>(assemblyFile).Copyright; }
        public static string Company(string assemblyFile) { return GetAssemblyAttribute<AssemblyCompanyAttribute>(assemblyFile).Company; }
        public static Version Version(string assemblyFile) { return GetAssemblyVersion(assemblyFile).Version; }
        public static string Name(string assemblyFile) { return GetAssemblyVersion(assemblyFile).Name; }

        public static string FileName(string assemblyFile) {
            return GetAssemblyVersion(assemblyFile).Name + System.IO.Path.GetExtension(assemblyFile);
        }

        private static T GetEntryAssemblyAttribute<T>() where T : Attribute {
            Assembly assembly = System.Reflection.Assembly.GetEntryAssembly();
            return GetAssemblyAttribute<T>(assembly);
        }

        private static T GetCallingAssemblyAttribute<T>() where T : Attribute {
            Assembly assembly = System.Reflection.Assembly.GetCallingAssembly();
            return GetAssemblyAttribute<T>(assembly);
        }

        private static T GetExecutingAssemblyAttribute<T>() where T : Attribute {
            Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            return GetAssemblyAttribute<T>(assembly);
        }

        private static T GetAssemblyAttribute<T>(Assembly assembly) where T : Attribute {
            if (assembly == null) return null;
            object[] attributes = assembly.GetCustomAttributes(typeof(T), true);
            if (attributes == null) return null;
            if (attributes.Length == 0) return null;
            return (T)attributes[0];
        }

        public static T GetAssemblyAttribute<T>(string assemblyFile) where T : Attribute {
            return GetAssemblyAttribute<T>(Assembly.LoadFrom(assemblyFile));
        }

        public static AssemblyName GetAssemblyVersion(string assemblyFile) {
            return Assembly.LoadFrom(assemblyFile).GetName();
        }

        static Version GetVersion(string fileName) {
            if (!System.IO.File.Exists(fileName))
                return null;
            System.IO.StreamReader file = new System.IO.StreamReader(fileName);
            string version = file.ReadLine();

            file.Close();

            return CreateVersion(version);
        }

        static Version CreateVersion(string s) {
            string[] newVersionComponents = s.Split('.');

            return new Version(
                int.Parse(newVersionComponents[0]),
                int.Parse(newVersionComponents[1]),
                int.Parse(newVersionComponents[2]),
                int.Parse(newVersionComponents[3]));
        }
        

        public static bool IsVersionNewer(Version test, Version @new) {
            if (@new.Major > test.Major)
                return true;
            else if (@new.Major == test.Major) {
                if (@new.Minor > test.Minor)
                    return true;
                else if (@new.Minor == test.Minor) {
                    if (@new.Build > test.Build)
                        return true;
                    else if (@new.Build == test.Build) {
                        if (@new.Revision > test.Revision)
                            return true;
                    }
                }
            }

            return false;
        }
    }
}