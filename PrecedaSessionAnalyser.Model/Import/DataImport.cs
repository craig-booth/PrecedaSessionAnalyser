using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

using System.Data.OleDb;
using System.Data.SQLite;

namespace PrecedaSessionAnalyser.Model.Import
{
    interface IDataImport
    {
        int ImportData(DateTime fromDate, DateTime toDate, SQLiteConnection sessionDatabase, IProgress<DataImportProgress> progress);
    }

    abstract class DataImport
    {
        public string Dataset { get; private set; }
        public string Server { get; private set; }
        public string User { get; private set; }
        public string Password { get; private set; }
        public string Library { get; private set; }

        public DataImport(string dataset, string server, string user, string password, string library)
        {
            Dataset = dataset;
            Server = server;
            User = user;
            Password = password;
            Library = library;
        }

        public OleDbConnection ConnectToServer()
        {
            // Setup OLEDB connection string
            var connectionStringBuilder = new OleDbConnectionStringBuilder();
            connectionStringBuilder["Provider"] = "IBMDA400";
            connectionStringBuilder["Data Source"] = Server;
            connectionStringBuilder["User Id"] = User;
            connectionStringBuilder["Password"] = Password;
            connectionStringBuilder["Default Collection"] = Library;
            connectionStringBuilder["Force Translate"] = "0";
            var connectionString = connectionStringBuilder.ConnectionString;

            var connection = new OleDbConnection(connectionString);
            connection.Open();

            return connection;
        }


        public void OnImportStarted(IProgress<DataImportProgress> progress, DateTime initialTime)
        {
            Report(progress, initialTime, 0, false);
        }

        public void OnImportProgress(IProgress<DataImportProgress> progress, DateTime lastTime, int records)
        {
            Report(progress, lastTime, records, false);
        }

        public void OnImportComplete(IProgress<DataImportProgress> progress, DateTime lastTime, int records)
        {
            Report(progress, lastTime, records, true);
        }

        private void Report(IProgress<DataImportProgress> progress, DateTime lastTime, int records, bool complete)
        {
            var importProgress = new DataImportProgress()
            {
                Dataset = Dataset,
                Complete = complete,
                LastTimeProcessed = lastTime,
                RecordsProcessed = records
            };

            progress.Report(importProgress);
        }
    }

    public class DataImportProgress
    {
        public string Dataset;
        public bool Complete;
        public DateTime LastTimeProcessed;
        public int RecordsProcessed;
    }


}
