using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrecedaSessionAnalyser.Model
{
    public class Top5CustomerSummary
    {
        public List<CustomerRecord> TotalLogons = new List<CustomerRecord>();
        public List<CustomerRecord> SingleSignOnLogons = new List<CustomerRecord>();
        public List<CustomerRecord> MobileLogons = new List<CustomerRecord>();
        public List<CustomerRecord> SelfServiceLogons = new List<CustomerRecord>();
        public List<CustomerRecord> PrecedaLogons = new List<CustomerRecord>();
        public List<CustomerRecord> IELogons = new List<CustomerRecord>();
    }

    public class CustomerRecord
    {
        public string Customer { get; set; }
        public int Count { get; set; }
        public int Average { get; set; }
        public double Percentage { get; set; }

        public CustomerRecord(string customer, int count)
        {
            Customer = customer;
            Count = count;
        }
    }
}
