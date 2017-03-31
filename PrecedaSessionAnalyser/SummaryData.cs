using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrecedaSessionAnalyser
{
    abstract class SummaryData<R> where R : SummaryRecord, new()
    {
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public PeriodFrequency Frequency { get; private set; }

        public List<R> Data { get; } = new List<R>();

        public SummaryData(DateTime startTime, DateTime endTime, PeriodFrequency frequency)
        {
            StartTime = startTime;
            EndTime = endTime;
            Frequency = frequency;

            InitialiseDataRecords();
        }

        private void InitialiseDataRecords()
        {
            if (Frequency == PeriodFrequency.Hour)
            {
                var start = StartTime;

                while (start < EndTime)
                {
                    var summaryRecord = new R();
                    summaryRecord.StartTime = start;
                    summaryRecord.EndTime = start.AddHours(1);
                    Data.Add(summaryRecord);

                    start = start.AddHours(1);
                }
            }
            else if (Frequency == PeriodFrequency.Day)
            {
                var start = StartTime;

                while (start < EndTime)
                {
                    var summaryRecord = new R();
                    summaryRecord.StartTime = start;
                    summaryRecord.EndTime = start.AddDays(1);
                    Data.Add(summaryRecord);

                    start = start.AddDays(1);
                }
            }
            else if (Frequency == PeriodFrequency.Week)
            {
                var start = StartTime;

                while (start < EndTime)
                {
                    var summaryRecord = new R();
                    summaryRecord.StartTime = start;
                    summaryRecord.EndTime = start.AddDays(7);
                    Data.Add(summaryRecord);

                    start = start.AddDays(7);
                }
            }
            else if (Frequency == PeriodFrequency.Month)
            {
                var start = StartTime;

                while (start < EndTime)
                {
                    var summaryRecord = new R();
                    summaryRecord.StartTime = start;
                    summaryRecord.EndTime = start.AddMonths(1);
                    Data.Add(summaryRecord);

                    start = start.AddMonths(1);
                }
            }
        }

    }

    abstract class SummaryRecord
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
