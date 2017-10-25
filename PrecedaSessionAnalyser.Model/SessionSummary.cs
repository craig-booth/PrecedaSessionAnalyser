using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrecedaSessionAnalyser.Model
{


    public class SessionSummary : SummaryData<SessionRecord>
    {
        public SessionSummary(DateTime startTime, DateTime endTime, PeriodFrequency frequency)
            : base(startTime, endTime, frequency)
        {

        }


        public void IncrementCount(DateTime time, Product product, int logonCount, int activeCount)
        {
            var summaryRecord = Data.FirstOrDefault(x => (time >= x.StartTime) && (time < x.EndTime));
            if (summaryRecord != null)
            {
                if (product == Product.Mobile)
                {
                    summaryRecord.MobileLogons += logonCount;
                    summaryRecord.ActiveMobileSessions += activeCount;
                }
                else if (product == Product.SelfService)
                {
                    summaryRecord.SelfServiceLogons += logonCount;
                    summaryRecord.ActiveSelfServiceSessions += activeCount;
                }
                else if (product == Product.Preceda)
                {
                    summaryRecord.PrecedaLogons += logonCount;
                    summaryRecord.ActivePrecedaSessions += activeCount;
                }
                else if (product == Product.IEPreceda)
                {
                    summaryRecord.IELogons += logonCount;
                    summaryRecord.ActiveIESessions += activeCount;
                }
                else if (product == Product.Unknown)
                {
                    summaryRecord.OtherLogons += logonCount;
                }
            }
        }

    }

    public class SessionRecord : SummaryRecord
    {
        public int ActiveMobileSessions { get; set; }
        public int MobileLogons { get; set; }

        public int ActiveSelfServiceSessions { get; set; }
        public int SelfServiceLogons { get; set; }

        public int ActivePrecedaSessions { get; set; }
        public int PrecedaLogons { get; set; }

        public int ActiveIESessions { get; set; }
        public int IELogons { get; set; }

        public int OtherLogons { get; set; }

    }


}
