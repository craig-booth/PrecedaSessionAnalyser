using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


using System.Data.OleDb;

namespace PrecedaSessionAnalyser.Import
{
    abstract class IBMPerformanceDataImport  
    {
        public string Server { get; }
        public string User { get; }
        public string Password { get; }

        public IBMPerformanceDataImport(string server, string user, string password)
        {
            Server = server;
            User = user;
            Password = password;
        }

        public IEnumerable<MemberList> GetAvailableMembers(OleDbConnection connection, string fileName)
        {
            var memberList = new List<MemberList>();

            var sql = String.Format("SELECT SYSTEM_TABLE_MEMBER FROM  QSYS2.SYSPARTITIONSTAT WHERE TABLE_SCHEMA = 'QPFRDATA' and TABLE_NAME = '{0}' AND SYSTEM_TABLE_MEMBER like 'Q%'", fileName);
            OleDbCommand command = new OleDbCommand(sql, connection);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    memberList.Add(new MemberList(reader.GetString(0)));
                }
            }

            return memberList;
        }

        public void SetCurrentMember(OleDbConnection connection, string fileName, string memberName)
        {
            var sql = String.Format("CREATE OR REPLACE ALIAS QTEMP.{0} FOR QPFRDATA.{0}({1})", fileName, memberName);
            OleDbCommand command = new OleDbCommand(sql, connection);
            command.ExecuteNonQuery();
        }
    }

    class MemberList
    {
        public string MemberName { get; private set; }
        public DateTime StartTime { get; private set; }

        public MemberList(string memberName)
        {
            MemberName = memberName;

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
