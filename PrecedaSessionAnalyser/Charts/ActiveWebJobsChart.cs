using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using LiveCharts;
using LiveCharts.Wpf;

namespace PrecedaSessionAnalyser.Charts
{
    class ActiveWebJobsChart : CartesianChart
    {
        public ActiveWebJobsChart(SessionAnalyser sessionAnalyser)
            : base("Active Web Jobs", sessionAnalyser)
        {

        }

        public override void DisplayChart(DateTime fromDate, DateTime toDate, PeriodFrequency frequency)
        {
            ChartSeriesCollection.Clear();

            var summary = _SessionAnalyser.GetActiveLansaJobs(fromDate, toDate, frequency);

            Labels.Clear();
            foreach (var record in summary.Data)
            {
                if (frequency == PeriodFrequency.Hour)
                    Labels.Add(String.Format("{0:t}", record.StartTime));
                else
                    Labels.Add(String.Format("{0:d}", record.StartTime));
            }

            IEnumerable<double> count;

            count = summary.Data.Select(x => (double)x.Count);

            ChartSeriesCollection.Add(new LineSeries() { Title = "Count", Values = new ChartValues<double>(count), LineSmoothness = 0, PointGeometry = null, StrokeThickness = 3 });
        }
    }
}
