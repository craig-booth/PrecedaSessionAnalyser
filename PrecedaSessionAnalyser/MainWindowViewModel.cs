using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Collections.ObjectModel;

using LiveCharts;
using LiveCharts.Wpf;


using PrecedaSessionAnalyser.Charts;

namespace PrecedaSessionAnalyser
{

    class MainWindowViewModel : NotifyClass
    {
        private DateTime _StartDate;
        public DateTime StartDate
        {
            get
            {
                return _StartDate;
            }
            set
            {
                if (value != _StartDate)
                {
                    _StartDate = value;
                    OnPropertyChanged();

                    UpdateChartData();
                }
            }
        }

        private DateTime _EndDate;
        public DateTime EndDate
        {
            get
            {
                return _EndDate;
            }
            set
            {
                if (value != _EndDate)
                {
                    _EndDate = value;
                    OnPropertyChanged();

                    UpdateChartData();
                }
            }
        }

        private PeriodFrequency _Frequency;
        public PeriodFrequency Frequency
        {
            get
            {
                return _Frequency;
            }
            set
            {
                if (value != _Frequency)
                {
                    _Frequency = value;
                    OnPropertyChanged();

                    UpdateChartData();
                }
            }
        }

        public ObservableCollection<BaseChart> Charts { get; } = new ObservableCollection<BaseChart>();

        private BaseChart _SelectedChart;
        public BaseChart SelectedChart
        {
            get
            {
                return _SelectedChart;
            }
            set
            {
                if (_SelectedChart != value)
                {
                    _SelectedChart = value;

                    OnPropertyChanged();

                    UpdateChartData();
                }
            }
        }


        private SessionAnalyser _Analyser;
       

        public MainWindowViewModel()
        {
            var dataBasePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrecedaSessions.db");
            _Analyser = new SessionAnalyser(dataBasePath);

            Charts.Add(new Charts.LogonsChart(_Analyser));
            Charts.Add(new Charts.ActiveSessionsChart(_Analyser));
            Charts.Add(new Charts.BrowserChart(_Analyser));
            Charts.Add(new Charts.DeviceChart(_Analyser));
            Charts.Add(new Charts.SingleSignOnChart(_Analyser));
            Charts.Add(new Charts.Top5CustomerChart(_Analyser));
            Charts.Add(new Charts.WebServerRequests(_Analyser));
            Charts.Add(new Charts.StartedWebJobsChart(_Analyser));
            Charts.Add(new Charts.ActiveWebJobsChart(_Analyser));
            Charts.Add(new Charts.WebJobCPUChart(_Analyser));
            Charts.Add(new Charts.WebJobIOChart(_Analyser));

            _SelectedChart = Charts[0];

            _StartDate = DateTime.Today;
            _EndDate = DateTime.Today;
            _Frequency = PeriodFrequency.Day;
        }

        public void UpdateChartData()
        {         
            var period = (_EndDate - _StartDate);
            if (period.Days < 0)
                return;
            else if (period.Days == 0)
                _Frequency = PeriodFrequency.Hour;
            else if (period.Days >= 365)
                _Frequency = PeriodFrequency.Week;
            else if (period.Days >= 1)
                _Frequency = PeriodFrequency.Day;

            _SelectedChart.DisplayChart(_StartDate, _EndDate.AddHours(23).AddMinutes(59).AddSeconds(59), _Frequency);        
        }

    }
}
