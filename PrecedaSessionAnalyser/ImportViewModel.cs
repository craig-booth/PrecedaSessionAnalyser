using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

using PrecedaSessionAnalyser.Import;

namespace PrecedaSessionAnalyser
{
    public class ImportViewModel : NotifyClass
    {

        public string Server { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Library { get; set; }
        public DateTime FromDate { get; set; } = DateTime.Today;
        public DateTime ToDate { get; set; } = DateTime.Today;

        private DateTime _LastTimeImported;
        public DateTime LastTimeImported
        {
            get
            {
                return _LastTimeImported;
            }
            set
            {
                if (value != _LastTimeImported)
                {
                    _LastTimeImported = value;
                    OnPropertyChanged();
                }
            }
        }
        private int _RecordsImported;
        public int RecordsImported
        {
            get
            {
                return _RecordsImported;
            }
            set
            {
                if (value != _RecordsImported)
                {
                    _RecordsImported = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _InProgress;
        public bool InProgess
        {
            get
            {
                return _InProgress;
            }
            set
            {
                if (value != _InProgress)
                {
                    _InProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _DataBasePath;

        public ImportViewModel(string dataBasePath)
        {
            _DataBasePath = dataBasePath;
        }

        public Task<int> ImportData()
        {
            var dataImporter = new DataImport(Server, Library, User, Password, _DataBasePath);

            InProgess = true;
            LastTimeImported = FromDate;
            RecordsImported = 0;

            var progressIndicator = new Progress<DataImportProgress>(ReportProgress);

            var importTask = dataImporter.ImportData(FromDate, ToDate, CancellationToken.None, progressIndicator);

            return importTask;
        }

        private void ReportProgress(DataImportProgress progress)
        {
            Application.Current.Dispatcher.Invoke(() => UpdateUI(progress));
        }

        private void UpdateUI(DataImportProgress progress)
        {
            LastTimeImported = progress.LastTimeProcessed;
            RecordsImported = progress.RecordsProcessed;
            InProgess = !progress.Complete;
        }
    }
}
