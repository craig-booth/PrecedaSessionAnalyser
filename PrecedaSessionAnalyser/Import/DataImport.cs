using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;


namespace PrecedaSessionAnalyser.Import
{
    interface IDataImport
    {
        int ImportData(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken, IProgress<DataImportProgress> progress);
    }

    class DataImport
    {
        private List<IDataImport> _DataImportors;

        public DataImport(string server, string library, string user, string password, string databasePath)
        {
            _DataImportors = new List<Import.IDataImport>();
            _DataImportors.Add(new PrecedaSessionImport(server, library, user, password, databasePath));
            _DataImportors.Add(new LansaJobDataImport(server, user, password, databasePath));
            _DataImportors.Add(new WebServerDataImport(server, user, password, databasePath));
        }

        public Task<int> ImportData(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken, IProgress<DataImportProgress> progress)
        {
            var task = Task.Factory.StartNew(() => ImportDataTask(fromDate, toDate, CancellationToken.None, progress));

            return task;
        }

        public int ImportDataTask(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken, IProgress<DataImportProgress> progress)
        {
            int records = 0;

            foreach (var importer in _DataImportors)
            {
                records += importer.ImportData(fromDate, toDate, cancellationToken, progress);
            }
            
            return records;
        }

    }

    public class DataImportProgress
    {
        public bool Complete;
        public DateTime LastTimeProcessed { get; }
        public int RecordsProcessed { get; }

        public DataImportProgress(DateTime lastTime, int records, bool complete)
        {
            Complete = complete;
            LastTimeProcessed = lastTime;
            RecordsProcessed = records;
        }
    }


}
