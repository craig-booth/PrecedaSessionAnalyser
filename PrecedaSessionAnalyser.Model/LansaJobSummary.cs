using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrecedaSessionAnalyser.Model
{

    public class StartedLansaJobSummary : SummaryData<StartedLansaJobRecord>
    {
        public StartedLansaJobSummary(DateTime startTime, DateTime endTime, PeriodFrequency frequency)
            : base(startTime, endTime, frequency)
        {

        }

        public void IncrementCount(DateTime time, long started, long startedByLANSA, long startedByWebServer)
        {
            var summaryRecord = Data.FirstOrDefault(x => (time >= x.StartTime) && (time < x.EndTime));
            if (summaryRecord != null)
            {
                summaryRecord.Started += started;
                summaryRecord.StartedByLANSA += startedByLANSA;
                summaryRecord.StartedByWebServer += startedByWebServer;
            }
        }

    }

    public class ActiveLansaJobSummary : SummaryData<ActiveLansaJobRecord>
    {
        public ActiveLansaJobSummary(DateTime startTime, DateTime endTime, PeriodFrequency frequency)
            : base(startTime, endTime, frequency)
        {

        }

        public void IncrementCount(DateTime time, long count)
        {
            var summaryRecord = Data.FirstOrDefault(x => (time >= x.StartTime) && (time < x.EndTime));
            if (summaryRecord != null)
            {
                summaryRecord.Count += count;
            }
        }

    }


    public class LansaJobCPUSummary : SummaryData<LansaJobCPURecord>
    {
        public LansaJobCPUSummary(DateTime startTime, DateTime endTime, PeriodFrequency frequency)
            : base(startTime, endTime, frequency)
        {

        }

        public void IncrementCount(DateTime time, long cpuSeconds)
        {
            var summaryRecord = Data.FirstOrDefault(x => (time >= x.StartTime) && (time < x.EndTime));
            if (summaryRecord != null)
            {
                summaryRecord.CPUSeconds += cpuSeconds;
            }
        }

    }

    public class LansaJobIOSummary : SummaryData<LansaJobIORecord>
    {
        public LansaJobIOSummary(DateTime startTime, DateTime endTime, PeriodFrequency frequency)
            : base(startTime, endTime, frequency)
        {

        }

        public void IncrementCount(DateTime time, long logicalDBReads, long logicalDBWrites, long physicalDBReads, long physicalDBWrites, long ifsBytesRead, long ifsBytesWritten)
        {
            var summaryRecord = Data.FirstOrDefault(x => (time >= x.StartTime) && (time < x.EndTime));
            if (summaryRecord != null)
            {
                summaryRecord.LogicalDataBaseReads += logicalDBReads;
                summaryRecord.LogicalDataBaseWrites += logicalDBWrites;
                summaryRecord.PhysicalDataBaseReads += physicalDBReads;
                summaryRecord.PhysicalDataBaseWrites += physicalDBWrites;
                summaryRecord.IFSBytesRead += ifsBytesRead;
                summaryRecord.IFSBytesWritten += ifsBytesWritten;
            }
        }

    }

    public class StartedLansaJobRecord : SummaryRecord
    {
        public long Started { get; set; }
        public long StartedByLANSA { get; set; }
        public long StartedByWebServer { get; set; }
    }

    public class ActiveLansaJobRecord : SummaryRecord
    {
        public long Count { get; set; }
    }

    public class LansaJobCPURecord : SummaryRecord
    {
        public long CPUSeconds { get; set; }
    }

    public class LansaJobIORecord : SummaryRecord
    {
        public long LogicalDataBaseReads { get; set; }
        public long LogicalDataBaseWrites { get; set; }
        public long PhysicalDataBaseReads { get; set; }
        public long PhysicalDataBaseWrites { get; set; }
        public double IFSBytesRead { get; set; }
        public double IFSBytesWritten { get; set; }
    }
}
