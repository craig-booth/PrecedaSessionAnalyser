﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using LiveCharts;
using LiveCharts.Wpf;

namespace PrecedaSessionAnalyser.Charts
{
    class LogonsChart : CartesianChart
    {
        public LogonsChart(SessionAnalyser sessionAnalyser)
            : base("Logons", sessionAnalyser)
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
            IEnumerable<double> other;
            IEnumerable<double> total;

            mobile = summary.Data.Select(x => (double)x.MobileLogons);
            ess = summary.Data.Select(x => (double)x.SelfServiceLogons);
            preceda = summary.Data.Select(x => (double)x.PrecedaLogons);
            ie = summary.Data.Select(x => (double)x.IELogons);
            other = summary.Data.Select(x => (double)x.OtherLogons);
            total = summary.Data.Select(x => (double)(x.MobileLogons + x.SelfServiceLogons + x.PrecedaLogons + x.IELogons + x.OtherLogons));

            ChartSeriesCollection.Add(new LineSeries() { Title = "Total", Values = new ChartValues<double>(total), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null });

            ChartSeriesCollection.Add(new LineSeries() { Title = "Unknown", Values = new ChartValues<double>(other), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });

            ChartSeriesCollection.Add(new LineSeries() { Title = "Mobile", Values = new ChartValues<double>(mobile), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });
            ChartSeriesCollection.Add(new LineSeries() { Title = "ESS", Values = new ChartValues<double>(ess), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });
            ChartSeriesCollection.Add(new LineSeries() { Title = "Preceda", Values = new ChartValues<double>(preceda), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });
            ChartSeriesCollection.Add(new LineSeries() { Title = "IE", Values = new ChartValues<double>(ie), LineSmoothness = 0, StrokeThickness = 3, PointGeometry = null, Fill = Brushes.Transparent });

            
        }
    }
}
