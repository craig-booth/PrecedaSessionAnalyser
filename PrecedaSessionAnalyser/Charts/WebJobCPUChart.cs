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
    class WebJobCPUChart : CartesianChart
    {
        public WebJobCPUChart(SessionAnalyser sessionAnalyser)
            : base("Web job CPU", sessionAnalyser)
        {

        }

        public override void DisplayChart(DateTime fromDate, DateTime toDate, PeriodFrequency frequency)
        {
            ChartSeriesCollection.Clear();

            var summary = _SessionAnalyser.GetLansaJobsCpu(fromDate, toDate, frequency);

            Labels.Clear();
            foreach (var record in summary.Data)
            {
                if (frequency == PeriodFrequency.Hour)
                    Labels.Add(String.Format("{0:t}", record.StartTime));
                else
                    Labels.Add(String.Format("{0:d}", record.StartTime));
            }

            IEnumerable<double> cpu;

            cpu = summary.Data.Select(x => (double)x.CPUSeconds);

            ChartSeriesCollection.Add(new LineSeries() { Title = "CPU Seconds", Values = new ChartValues<double>(cpu), LineSmoothness = 0, PointGeometry = null, StrokeThickness = 3 });
       }
    }

}
