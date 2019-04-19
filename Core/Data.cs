using System;
using System.Data.OleDb;

using TraceWizard.Entities;

namespace TraceWizard.Data {

    public interface IDatabase {
        string DataSource { get; set; }
        string DataSourceFileNameWithoutExtension { get; set; }
        string DataSourceWithFullPath { get; set;}
    }
    namespace Jet {
        public static class DataServices {
            public static string BuildJetConnectionString(string dataSource, bool ReadOnly) {
                OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder();
                //builder["Provider"] = "Microsoft.ACE.OLEDB.12.0";
                builder["Provider"] = "Microsoft.Jet.OLEDB.4.0";
                builder["Data Source"] = dataSource;
                if (ReadOnly) {
                    builder["Mode"] = "Share Deny Write";
                }
                return builder.ConnectionString;
            }
        }
    }
}