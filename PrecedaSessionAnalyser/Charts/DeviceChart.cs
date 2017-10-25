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
    class DeviceChart : CartesianChart
    {
        public DeviceChart(SessionAnalyser sessionAnalyser)
            : base("Device Usage", sessionAnalyser)
        {
            CountFormatter = value => value.ToString("p0");
        }

        public override void DisplayChart(DateTime fromDate, DateTime toDate, PeriodFrequency frequency)
        {
            PeriodFrequency frequencyToUse = frequency;

            if (frequency == PeriodFrequency.Hour)
                frequencyToUse = PeriodFrequency.Day;

            ChartSeriesCollection.Clear();

            var summary = _SessionAnalyser.GetDeviceSummary(fromDate, toDate, frequencyToUse);

            Labels.Clear();
            foreach (var record in summary.Data)
            {
                Labels.Add(String.Format("{0:d}", record.StartTime));
            }

            foreach (var device in summary.DeviceNames.OrderBy(x => x))
            {
                var counts = summary.Data.Select(x => (double)x.DeviceCount[device] / (double)x.TotalCount);

                if (frequency == PeriodFrequency.Hour)
                {
                    ChartSeriesCollection.Add(new ColumnSeries() { Title = device, Values = new ChartValues<double>(counts) });
                }
                else
                {
                    ChartSeriesCollection.Add(new LineSeries() { Title = device, Values = new ChartValues<double>(counts), LineSmoothness = 0, PointGeometry = null, StrokeThickness = 3, Fill = Brushes.Transparent });
                }
            }
        }
    }
}
