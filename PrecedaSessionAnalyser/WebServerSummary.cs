using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrecedaSessionAnalyser
{
    class WebServerSummary : SummaryData<WebServerRecord>
    {
        public WebServerSummary(DateTime startTime, DateTime endTime, PeriodFrequency frequency)
            : base(startTime, endTime, frequency)
        {

        }


        public void IncrementCount(DateTime time, long totalRequests, long cgiRequests, long ifsRequests)
        {
            var summaryRecord = Data.FirstOrDefault(x => (time >= x.StartTime) && (time < x.EndTime));
            if (summaryRecord != null)
            {
                summaryRecord.TotalRequests += totalRequests;
                summaryRecord.CGIRequests += cgiRequests;
                summaryRecord.IFSRequests += ifsRequests;
            }

        }

    }

    class WebServerRecord : SummaryRecord
    {
        public long TotalRequests { get; set; }
        public long CGIRequests { get; set; }
        public long IFSRequests { get; set; }
    }

}
