using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using PrecedaSessionAnalyser.Model;
using PrecedaSessionAnalyser.Model.Import;


namespace PrecedaSessionAnalyserConfig
{
    class MainWindowViewModel : NotifyClass
    {
        private readonly string _DatabasePath;
        private SessionAnalyserConfig _Config;

        public string Server { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Library { get; set; }
        private DateTime _LastImportDate;
        public DateTime LastImportDate
        {
            get
            {
                return _LastImportDate;
            }
            set
            {
                if (_LastImportDate != value)
                {
                    _LastImportDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand ImportCommand { get; private set; }

        private bool _ImportInProgress;
        public bool ImportInProgress
        {
            get
            {
                return _ImportInProgress;
            }
            private set
            {
                if (_ImportInProgress != value)
                {
                    _ImportInProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainWindowViewModel()
        {
            _DatabasePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrecedaSessions.db");
            _Config = new SessionAnalyserConfig(_DatabasePath);
            _Config.Load();

            Server = _Config.Server;
            User = _Config.User;
            Password = _Config.Password;
            Library = _Config.Library;
            LastImportDate = _Config.LastImport;

            ImportInProgress = false;

            SaveCommand = new RelayCommand(Save);
            ImportCommand = new RelayCommand(Import, () => !ImportInProgress);
        }

        public void Save()
        {
            _Config.Server = Server;
            _Config.User = User;
            _Config.Password = Password;
            _Config.Library = Library;

            _Config.Save();
        }

        public void Import()
        {
            var dataImporter = new DataImporter(_DatabasePath, Server, User, Password, Library);

            var fromDate = LastImportDate.AddDays(1);
            var toDate = DateTime.Today.AddDays(-1);

            var progressIndicator = new Progress<DataImportProgress>(ReportProgress);

            ImportInProgress = true;
            dataImporter.ImportData(fromDate, toDate, progressIndicator);
            ImportInProgress = false;

            _Config.LastImport = toDate;
            _Config.Save();
        }

        private void ReportProgress(DataImportProgress progress)
        {
            LastImportDate = progress.LastTimeProcessed.Date;
        }

    }
}
