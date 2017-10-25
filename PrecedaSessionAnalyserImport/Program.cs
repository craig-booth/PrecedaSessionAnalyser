using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using CommandLine;

using PrecedaSessionAnalyser.Model;
using PrecedaSessionAnalyser.Model.Import;

namespace PrecedaSessionAnalyserImport
{
    class Program
    {

        static void Main(string[] args)
        {
            var options = new ImportOptions();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                DataImport(options.FromDate, options.ToDate);
                Console.WriteLine("Complete");
            }
            else
            {
                Console.WriteLine("Invalid parameters");
            }
        }

        private static void DataImport(DateTime? requestedFromDate, DateTime? requestedToDate)
        {
            var databasePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrecedaSessions.db");
            var config = new SessionAnalyserConfig(databasePath);
            config.Load();

            var dataImporter = new DataImporter(databasePath, config.Server, config.User, config.Password, config.Library);

            DateTime fromDate;
            if (requestedFromDate != null)
                fromDate = (DateTime)requestedFromDate;
            else
                fromDate = config.LastImport.AddDays(1);

            DateTime toDate;
            if (requestedToDate != null)
                toDate = (DateTime)requestedToDate;
            else
                toDate = DateTime.Today.AddDays(-1);

            Console.WriteLine("Importing data for from {0:d} to {1:d}", fromDate, toDate);

            var progressIndicator = new Progress<DataImportProgress>(ReportProgress);

            dataImporter.ImportData(fromDate, toDate, progressIndicator);

            config.LastImport = toDate;
            config.Save();
        }

        private static void ReportProgress(DataImportProgress progress)
        {
            Console.WriteLine("Importing data for {0}, imported up to {1:d}", progress.Dataset, progress.LastTimeProcessed.Date);
        }
    }

    class ImportOptions
    {
        [Option("from", HelpText = "Date to import from, if omitted will default to last imported date.")]
        public string FromDateString { get; set; }
        public DateTime? FromDate
        {
            get
            {
                if (FromDateString == null)
                    return null;
                else
                {
                    DateTime date;
                    if (DateTime.TryParse(FromDateString, out date))
                        return date;
                    else
                        return null;
                }
            }
        }

        

        [Option("to", HelpText = "Date to import to, if omitted will default to yesterday.")]
        public string ToDateString { get; set; }
        public DateTime? ToDate
        {
            get
            {
                if (FromDateString == null)
                    return null;
                else
                {
                    DateTime date;
                    if (DateTime.TryParse(ToDateString, out date))
                        return date;
                    else
                        return null;
                }
            }
        }
    }
}
