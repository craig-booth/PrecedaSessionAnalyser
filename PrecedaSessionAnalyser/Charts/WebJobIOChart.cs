
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
    class WebJobIOChart : CartesianChart
    {
        public WebJobIOChart(SessionAnalyser sessionAnalyser)
            : base("Web Jobs I/O", sessionAnalyser)
        {

        }

        public override void DisplayChart(DateTime fromDate, DateTime toDate, PeriodFrequency frequency)
        {
            ChartSeriesCollection.Clear();

            var summary = _SessionAnalyser.GetLansaJobsIO(fromDate, toDate, frequency);

            Labels.Clear();
            foreach (var record in summary.Data)
            {
                if (frequency == PeriodFrequency.Hour)
                    Labels.Add(String.Format("{0:t}", record.StartTime));
                else
                    Labels.Add(String.Format("{0:d}", record.StartTime));
            }

            IEnumerable<double> logicalDBRead;
            IEnumerable<double> logicalDBWrite;
            IEnumerable<double> physicalDBRead;
            IEnumerable<double> physicalDBWrite;
            IEnumerable<double> ifsRead;
            IEnumerable<double> ifsWritten;

            logicalDBRead = summary.Data.Select(x => (double)x.LogicalDataBaseReads);
            logicalDBWrite = summary.Data.Select(x => (double)x.LogicalDataBaseWrites);
            physicalDBRead = summary.Data.Select(x => (double)x.PhysicalDataBaseReads);
            physicalDBWrite = summary.Data.Select(x => (double)x.PhysicalDataBaseWrites);
            ifsRead = summary.Data.Select(x => (double)x.IFSBytesRead);
            ifsWritten = summary.Data.Select(x => (double)x.IFSBytesWritten);

            ChartSeriesCollection.Add(new LineSeries() { Title = "Logical Database Reads", Values = new ChartValues<double>(logicalDBRead), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });
            ChartSeriesCollection.Add(new LineSeries() { Title = "Logical Database Write", Values = new ChartValues<double>(logicalDBWrite), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });

            ChartSeriesCollection.Add(new LineSeries() { Title = "Physical Database Reads", Values = new ChartValues<double>(physicalDBRead), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });
            ChartSeriesCollection.Add(new LineSeries() { Title = "Physical Database Write", Values = new ChartValues<double>(physicalDBWrite), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });

            ChartSeriesCollection.Add(new LineSeries() { Title = "IFS Read (KB)", Values = new ChartValues<double>(ifsRead), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });
            ChartSeriesCollection.Add(new LineSeries() { Title = "IFS Written (KB)", Values = new ChartValues<double>(ifsWritten), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });

        }
    }
}
