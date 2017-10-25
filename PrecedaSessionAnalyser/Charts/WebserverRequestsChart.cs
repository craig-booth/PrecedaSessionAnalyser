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
    class WebServerRequests : CartesianChart
    {
        public WebServerRequests(SessionAnalyser sessionAnalyser)
            : base("WebServer Requests", sessionAnalyser)
        {

        }

        public override void DisplayChart(DateTime fromDate, DateTime toDate, PeriodFrequency frequency)
        {
            ChartSeriesCollection.Clear();

            var summary = _SessionAnalyser.GetWebServerSummary(fromDate, toDate, frequency);

            Labels.Clear();
            foreach (var record in summary.Data)
            {
                if (frequency == PeriodFrequency.Hour)
                    Labels.Add(String.Format("{0:t}", record.StartTime));
                else
                    Labels.Add(String.Format("{0:d}", record.StartTime));
            }

            IEnumerable<double> cgi;
            IEnumerable<double> ifs;
            IEnumerable<double> total;

            cgi = summary.Data.Select(x => (double)x.CGIRequests);
            ifs = summary.Data.Select(x => (double)x.IFSRequests);
            total = summary.Data.Select(x => (double)x.TotalRequests);

            ChartSeriesCollection.Add(new LineSeries() { Title = "Total", Values = new ChartValues<double>(total), LineSmoothness = 0, PointGeometry = null, StrokeThickness = 3 });

            ChartSeriesCollection.Add(new LineSeries() { Title = "CGI", Values = new ChartValues<double>(cgi), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });
            ChartSeriesCollection.Add(new LineSeries() { Title = "IFS", Values = new ChartValues<double>(ifs), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });
        }
    }
}
