using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrecedaSessionAnalyser
{

    class SingleSignOnSummary : SummaryData<SessionRecord>
    {
        public SingleSignOnSummary(DateTime startTime, DateTime endTime, PeriodFrequency frequency)
            : base(startTime, endTime, frequency)
        {

        }


        public void IncrementCount(DateTime time, Product product, int count)
        {
            var summaryRecord = Data.FirstOrDefault(x => (time >= x.StartTime) && (time < x.EndTime));
            if (summaryRecord != null)
            {
                if (product == Product.Mobile)
                {
                    summaryRecord.MobileLogons += count;
                }
                else if (product == Product.SelfService)
                {
                    summaryRecord.SelfServiceLogons += count;
                }
                else if (product == Product.Preceda)
                {
                    summaryRecord.PrecedaLogons += count;
                }
                else if (product == Product.IEPreceda)
                {
                    summaryRecord.IELogons += count;
                }
                else if (product == Product.Unknown)
                {
                    summaryRecord.OtherLogons += count;
                }
            }
        }

    }

    class SingleSignOnRecord : SummaryRecord
    {
        public int MobileLogons { get; set; }
        public int SelfServiceLogons { get; set; }
        public int PrecedaLogons { get; set; }
        public int IELogons { get; set; }
        public int OtherLogons { get; set; }
    }

}
