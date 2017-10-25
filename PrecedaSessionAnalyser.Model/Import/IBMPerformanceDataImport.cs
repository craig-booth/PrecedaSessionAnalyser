using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


using System.Data.OleDb;

namespace PrecedaSessionAnalyser.Model.Import
{
    abstract class IBMPerformanceDataImport : DataImport
    {

        public IBMPerformanceDataImport(string dataset, string server, string user, string password)
            : base(dataset, server, user, password, "QPFRDATA")
        {

        }

        public IEnumerable<MemberDetails> GetAvailableMembers(OleDbConnection connection)
        {
            var memberList = new List<MemberDetails>();

            var sql = String.Format("SELECT SYSTEM_TABLE_MEMBER FROM QSYS2.SYSPARTITIONSTAT WHERE TABLE_SCHEMA = 'QPFRDATA' and TABLE_NAME = 'QAPMCONF' AND SYSTEM_TABLE_MEMBER like 'Q%'");
            OleDbCommand command = new OleDbCommand(sql, connection);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    memberList.Add(GetMemberDetails(connection, reader.GetString(0)));
                }
            }

            return memberList;
        }

        public MemberDetails GetMemberDetails(OleDbConnection connection, string memberName)
        {
            int interval = 5;
            byte[] data = new byte[10];

            SetCurrentMember(connection, "QAPMCONF", memberName);

            var sql = String.Format("SELECT GKEY, GDES FROM QPFRDATA.QAPMCONF WHERE GKEY = ' I'");
            OleDbCommand command = new OleDbCommand(sql, connection);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var key = reader.GetString(0);
                    reader.GetBytes(1, 0, data, 0, 10);
                    if (key == " I")
                    {
                        interval = (int)PackedToDecimal(data, 3);
                    }
                }
            }


            return new MemberDetails(memberName, interval);
        }

        public void SetCurrentMember(OleDbConnection connection, string fileName, string memberName)
        {
            var sql = String.Format("CREATE OR REPLACE ALIAS QTEMP.{0} FOR QPFRDATA.{0}({1})", fileName, memberName);
            OleDbCommand command = new OleDbCommand(sql, connection);
            var result = command.ExecuteNonQuery();
        }

        private decimal PackedToDecimal(byte[] packed, int length)
        {
            decimal result = 0.00m;

            var byteCount = (length / 2) + 1;

            for (var i = 0; i < byteCount; i++)
            {
                var d1 = packed[i] >> 4;
                var d2 = packed[i] & '\x0F';

                result = (result * 10) + d1;
                if (d2 != '\x0F')
                    result = (result * 10) + d2;
            }

            return result;
        }

    }

    class MemberDetails
    {
        public string MemberName { get; private set; }
        public DateTime StartTime { get; private set; }
        public int Interval { get; private set; }

        public MemberDetails(string memberName, int interval)
        {
            MemberName = memberName;
            Interval = interval;

            var dayOfYear = int.Parse(memberName.Substring(1, 3));
            
            var year = DateTime.Now.Year;
            if (dayOfYear > DateTime.Now.DayOfYear)
                year--;
            var startOfYear = new DateTime(year, 1, 1, 0, 0, 0);

            var hour = int.Parse(memberName.Substring(4, 2));
            var minute = int.Parse(memberName.Substring(6, 2));
            var second = int.Parse(memberName.Substring(8, 2));

            StartTime = startOfYear.AddDays(dayOfYear - 1).AddHours(hour).AddMinutes(minute).AddSeconds(second);
        }
    }

}
