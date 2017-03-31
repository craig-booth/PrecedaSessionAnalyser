using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace PrecedaSessionAnalyser.Charts
{
    class Top5CustomerChart : BaseChart
    {

        public ObservableCollection<CustomerRecord> TotalLogons { get; } = new ObservableCollection<CustomerRecord>();
        public ObservableCollection<CustomerRecord> SingleSignOnLogons { get; } = new ObservableCollection<CustomerRecord>();
        public ObservableCollection<CustomerRecord> MobileLogons { get; } = new ObservableCollection<CustomerRecord>();
        public ObservableCollection<CustomerRecord> SelfServiceLogons { get; } = new ObservableCollection<CustomerRecord>();
        public ObservableCollection<CustomerRecord> PrecedaLogons { get; } = new ObservableCollection<CustomerRecord>();
        public ObservableCollection<CustomerRecord> IELogons { get; } = new ObservableCollection<CustomerRecord>();

        public Top5CustomerChart(SessionAnalyser sessionAnalyser)
            : base("Top 5 Customers", sessionAnalyser)
        {

        }

        public override void DisplayChart(DateTime fromDate, DateTime toDate, PeriodFrequency frequency)
        {
            var summary = _SessionAnalyser.GetTop5CustomerSummary(fromDate, toDate);

            TotalLogons.Clear();
            foreach (var record in summary.TotalLogons)
                TotalLogons.Add(record);

            SingleSignOnLogons.Clear();
            foreach (var record in summary.SingleSignOnLogons)
                SingleSignOnLogons.Add(record);

            MobileLogons.Clear();
            foreach (var record in summary.MobileLogons)
                MobileLogons.Add(record);

            SelfServiceLogons.Clear();
            foreach (var record in summary.SelfServiceLogons)
                SelfServiceLogons.Add(record);

            PrecedaLogons.Clear();
            foreach (var record in summary.PrecedaLogons)
                PrecedaLogons.Add(record);

            IELogons.Clear();
            foreach (var record in summary.IELogons)
                IELogons.Add(record);
        }
    }
}
