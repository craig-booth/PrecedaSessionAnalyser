using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace PrecedaSessionAnalyser.Model.Import
{
    public class DataImporter
    {
        public string DatabasePath;
        
        private List<IDataImport> _DataImporters;

        public DataImporter(string databasePath, string server, string user, string password, string library)
        {
            DatabasePath = databasePath;
            _DataImporters = new List<IDataImport>();
            _DataImporters.Add(new PrecedaSessionImport(server, user, password, library));
            _DataImporters.Add(new LansaJobDataImport(server, user, password));
            _DataImporters.Add(new WebServerDataImport(server, user, password));
        }

        public int ImportData(DateTime fromDate, DateTime toDate, IProgress<DataImportProgress> progress)
        {      
            int records = 0;

            var sqliteConnection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;");
            sqliteConnection.Open();

            using (var transaction = sqliteConnection.BeginTransaction())
            {
                foreach (var importer in _DataImporters)
                    records += importer.ImportData(fromDate, toDate, sqliteConnection, progress);

                transaction.Commit();
            }

            sqliteConnection.Close();

            return records;
        }

 

    }
}
