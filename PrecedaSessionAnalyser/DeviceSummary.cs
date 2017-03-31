using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrecedaSessionAnalyser
{
    class DeviceSummary : SummaryData<DeviceRecord>
    {
        public HashSet<string> DeviceNames { get; } = new HashSet<string>();

        public DeviceSummary(DateTime startTime, DateTime endTime, PeriodFrequency frequency)
            : base(startTime, endTime, frequency)
        {

        }

        public void IncrementCount(string deviceName, DateTime time, int count)
        {
            if (!DeviceNames.Contains(deviceName))
            {
                DeviceNames.Add(deviceName);

                foreach (var record in Data)
                {
                    record.DeviceCount.Add(deviceName, 0);
                }
            }

            var browserRecord = Data.FirstOrDefault(x => (time >= x.StartTime) && (time < x.EndTime));
            if (browserRecord != null)
            {
                browserRecord.DeviceCount[deviceName] += count;
            }
        }
    }


    class DeviceRecord : SummaryRecord
    {
        public Dictionary<string, int> DeviceCount { get; } = new Dictionary<string, int>();

        public int TotalCount
        {
            get
            {
                return DeviceCount.Sum(x => x.Value);
            }
        }
    }
}
