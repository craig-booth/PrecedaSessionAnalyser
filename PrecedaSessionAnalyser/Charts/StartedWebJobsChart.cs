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
    class StartedWebJobsChart : CartesianChart
    {
        public StartedWebJobsChart(SessionAnalyser sessionAnalyser)
            : base("Web Jobs Started", sessionAnalyser)
        {

        }

        public override void DisplayChart(DateTime fromDate, DateTime toDate, PeriodFrequency frequency)
        {
            ChartSeriesCollection.Clear();

            var summary = _SessionAnalyser.GetStartedLansaJobs(fromDate, toDate, frequency);

            Labels.Clear();
            foreach (var record in summary.Data)
            {
                if (frequency == PeriodFrequency.Hour)
                    Labels.Add(String.Format("{0:t}", record.StartTime));
                else
                    Labels.Add(String.Format("{0:d}", record.StartTime));
            }

            IEnumerable<double> lansa;
            IEnumerable<double> webserver;
            IEnumerable<double> total;

            lansa = summary.Data.Select(x => (double)x.StartedByLANSA);
            webserver = summary.Data.Select(x => (double)x.StartedByWebServer);
            total = summary.Data.Select(x => (double)x.Started);

            ChartSeriesCollection.Add(new LineSeries() { Title = "Total", Values = new ChartValues<double>(total), LineSmoothness = 0, PointGeometry = null, StrokeThickness = 3 });

            ChartSeriesCollection.Add(new LineSeries() { Title = "Started by LANSA", Values = new ChartValues<double>(lansa), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });
            ChartSeriesCollection.Add(new LineSeries() { Title = "Started by Web Server", Values = new ChartValues<double>(webserver), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });

        }
    }
}
