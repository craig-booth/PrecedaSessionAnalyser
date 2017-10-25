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
    class SingleSignOnChart : CartesianChart
    {
        public SingleSignOnChart(SessionAnalyser sessionAnalyser)
            : base("Single Sign On Logons", sessionAnalyser)
        {

        }

        public override void DisplayChart(DateTime fromDate, DateTime toDate, PeriodFrequency frequency)
        {
            PeriodFrequency frequencyToUse = frequency;

            if (frequency == PeriodFrequency.Hour)
                frequencyToUse = PeriodFrequency.Day;

            ChartSeriesCollection.Clear();

            var summary = _SessionAnalyser.GetSingleSignOnSummary(fromDate, toDate, frequencyToUse);

            Labels.Clear();
            foreach (var record in summary.Data)
            {
                Labels.Add(String.Format("{0:d}", record.StartTime));
            }

            IEnumerable<double> mobile;
            IEnumerable<double> ess;
            IEnumerable<double> preceda;
            IEnumerable<double> ie;
            IEnumerable<double> other;
            IEnumerable<double> total;

            mobile = summary.Data.Select(x => (double)x.MobileLogons);
            ess = summary.Data.Select(x => (double)x.SelfServiceLogons);
            preceda = summary.Data.Select(x => (double)x.PrecedaLogons);
            ie = summary.Data.Select(x => (double)x.IELogons);
            other = summary.Data.Select(x => (double)x.OtherLogons);
            total = summary.Data.Select(x => (double)(x.MobileLogons + x.SelfServiceLogons + x.PrecedaLogons + x.IELogons + x.OtherLogons));


            if (frequency == PeriodFrequency.Hour)
            {
                ChartSeriesCollection.Add(new ColumnSeries() { Title = "Total", Values = new ChartValues<double>(total) });
                ChartSeriesCollection.Add(new ColumnSeries() { Title = "Unknown", Values = new ChartValues<double>(other) });
                ChartSeriesCollection.Add(new ColumnSeries() { Title = "Mobile", Values = new ChartValues<double>(mobile) });
                ChartSeriesCollection.Add(new ColumnSeries() { Title = "ESS", Values = new ChartValues<double>(ess) });
                ChartSeriesCollection.Add(new ColumnSeries() { Title = "Preceda", Values = new ChartValues<double>(preceda) });
                ChartSeriesCollection.Add(new ColumnSeries() { Title = "IE", Values = new ChartValues<double>(ie) });

            }
            else
            {
                ChartSeriesCollection.Add(new LineSeries() { Title = "Total", Values = new ChartValues<double>(total), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null });
                ChartSeriesCollection.Add(new LineSeries() { Title = "Unknown", Values = new ChartValues<double>(other), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });
                ChartSeriesCollection.Add(new LineSeries() { Title = "Mobile", Values = new ChartValues<double>(mobile), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });
                ChartSeriesCollection.Add(new LineSeries() { Title = "ESS", Values = new ChartValues<double>(ess), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });
                ChartSeriesCollection.Add(new LineSeries() { Title = "Preceda", Values = new ChartValues<double>(preceda), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });
                ChartSeriesCollection.Add(new LineSeries() { Title = "IE", Values = new ChartValues<double>(ie), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });
            }
        }
    }
}
