using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using LiveCharts;
using LiveCharts.Wpf;

using PrecedaSessionAnalyser.Model;

namespace PrecedaSessionAnalyser.Charts
{
    class ActiveSessionsChart : CartesianChart
    {
        public ActiveSessionsChart(SessionAnalyser sessionAnalyser)
            : base("Active Sessions", sessionAnalyser)
        {

        }

        public override void DisplayChart(DateTime fromDate, DateTime toDate, PeriodFrequency frequency)
        {
            ChartSeriesCollection.Clear();

            var summary = _SessionAnalyser.GetSessionSummary(fromDate, toDate, frequency);

            Labels.Clear();
            foreach (var record in summary.Data)
            {
                if (frequency == PeriodFrequency.Hour)
                    Labels.Add(String.Format("{0:t}", record.StartTime));
                else
                    Labels.Add(String.Format("{0:d}", record.StartTime));
            }

            IEnumerable<double> mobile;
            IEnumerable<double> ess;
            IEnumerable<double> preceda;
            IEnumerable<double> ie;
            IEnumerable<double> total;

            mobile = summary.Data.Select(x => (double)x.ActiveMobileSessions);
            ess = summary.Data.Select(x => (double)x.ActiveSelfServiceSessions);
            preceda = summary.Data.Select(x => (double)x.ActivePrecedaSessions);
            ie = summary.Data.Select(x => (double)x.ActiveIESessions);
            total = summary.Data.Select(x => (double)(x.ActiveMobileSessions + x.ActiveSelfServiceSessions + x.ActivePrecedaSessions + x.IELogons + x.ActiveIESessions));

            ChartSeriesCollection.Add(new LineSeries() { Title = "Total", Values = new ChartValues<double>(total), LineSmoothness = 0, PointGeometry = null, StrokeThickness = 3 });

            ChartSeriesCollection.Add(new LineSeries() { Title = "Mobile", Values = new ChartValues<double>(mobile), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });
            ChartSeriesCollection.Add(new LineSeries() { Title = "ESS", Values = new ChartValues<double>(ess), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });
            ChartSeriesCollection.Add(new LineSeries() { Title = "Preceda", Values = new ChartValues<double>(preceda), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });
            ChartSeriesCollection.Add(new LineSeries() { Title = "IE", Values = new ChartValues<double>(ie), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });
        }
    }
}
