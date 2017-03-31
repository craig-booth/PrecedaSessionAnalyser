using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrecedaSessionAnalyser
{
    class BrowserSummary : SummaryData<BrowserRecord>
    {
        public HashSet<string> BrowserNames { get; } = new HashSet<string>();

        public BrowserSummary(DateTime startTime, DateTime endTime, PeriodFrequency frequency)
            : base(startTime, endTime, frequency)
        {

        }

        public void IncrementCount(string browserName, DateTime time, int count)
        {
            if (! BrowserNames.Contains(browserName))
            {
                BrowserNames.Add(browserName);

                foreach(var record in Data)
                {
                    record.BrowserCount.Add(browserName, 0);
                }
            }

            var browserRecord = Data.FirstOrDefault(x => (time >= x.StartTime) && (time < x.EndTime));
            if (browserRecord != null)
            {
                browserRecord.BrowserCount[browserName] += count;
            }
        }
    }

    
    class BrowserRecord : SummaryRecord
    {   
        public Dictionary<string, int> BrowserCount { get; } = new Dictionary<string, int>();

        public int TotalCount
        {
            get
            {
                return BrowserCount.Sum(x => x.Value);
            }
        }
    }
}
