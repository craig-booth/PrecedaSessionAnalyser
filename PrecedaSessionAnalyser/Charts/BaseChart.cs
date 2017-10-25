using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LiveCharts;
using LiveCharts.Wpf;

using PrecedaSessionAnalyser.Model;

namespace PrecedaSessionAnalyser.Charts
{
    abstract class BaseChart
    {
        public string Description {get; private set;}
        protected SessionAnalyser _SessionAnalyser;

        public BaseChart(string description, SessionAnalyser sessionAnalyser)
        {
            Description = description;
            _SessionAnalyser = sessionAnalyser;
        }

        public abstract void DisplayChart(DateTime fromDate, DateTime toDate, PeriodFrequency frequency);
    }

    abstract class CartesianChart: BaseChart
    {
        public SeriesCollection ChartSeriesCollection { get; set; }
        public List<string> Labels { get; set; }

        public virtual Func<double, string> CountFormatter { get; set; }

        public CartesianChart(string description, SessionAnalyser sessionAnalyser)
            : base(description, sessionAnalyser)
        {
            ChartSeriesCollection = new SeriesCollection();
            Labels = new List<string>();

            CountFormatter = value => value.ToString("n0");
        }
    }
}
