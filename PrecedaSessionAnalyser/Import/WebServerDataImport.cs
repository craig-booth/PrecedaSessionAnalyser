using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


using System.Data.OleDb;
using System.Data.SQLite;

namespace PrecedaSessionAnalyser.Import
{
    class WebServerDataImport : IBMPerformanceDataImport, IDataImport
    {
        private int _MaxRecordCount = 500;
        private int _ArchiveRecordCount = 100;
        private List<WebServerRecord> _WebServerRecords = new List<WebServerRecord>();

        public string DataBasePath { get; }
        private SQLiteConnection _SqliteConnection;

        public WebServerDataImport(string server, string user, string password, string databasePath)
            : base(server, user, password)
        {
            DataBasePath = databasePath;
        }

        public int ImportData(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken, IProgress<DataImportProgress> progress)
        {
            progress.Report(new DataImportProgress(fromDate, 0, false));

            _SqliteConnection = new SQLiteConnection("Data Source=" + DataBasePath + ";Version=3;");
            _SqliteConnection.Open();

            // Setup OLEDB connection string
            var connectionStringBuilder = new OleDbConnectionStringBuilder();
            connectionStringBuilder["Provider"] = "IBMDA400";
            connectionStringBuilder["Data Source"] = Server;
            connectionStringBuilder["User Id"] = User;
            connectionStringBuilder["Password"] = Password;
            connectionStringBuilder["Default Collection"] = "QPFRDATA";
            connectionStringBuilder["Force Translate"] = "0";
            var connectionString = connectionStringBuilder.ConnectionString;

            var connection = new OleDbConnection(connectionString);
            connection.Open();

            int RecordsProcessed = 0;

            var memberList = GetAvailableMembers(connection, "QAPMHTTPD");

            foreach (var member in memberList.Where(x => (x.StartTime.Date >= fromDate) && (x.StartTime.Date <= toDate)))
            {
                SetCurrentMember(connection, "QAPMHTTPD", member.MemberName);


                var sql = String.Format("SELECT INTNUM, HTRTYP, sum(HTRQSR) FROM QTEMP.QAPMHTTPD WHERE HTJNAM = 'PRECEDA' GROUP BY INTNUM, HTRTYP");
                OleDbCommand command = new OleDbCommand(sql, connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var interval = (int)reader.GetDecimal(0);
                        var requestType = reader.GetString(1);
                        var requestCount = reader.GetInt64(2);
                      
                        var startTime = member.StartTime.AddMinutes((interval - 1) * 5);

                        var jobRecord = _WebServerRecords.FirstOrDefault(x => (x.Date == startTime.Date) && (x.Hour == startTime.Hour));
                        if (jobRecord == null)
                            jobRecord = AddWebServerRecord(startTime.Date, startTime.Hour);

                        jobRecord.TotalRequests += requestCount;
                        if (requestType == "CG")
                        {
                            jobRecord.CGIRequests += requestCount;
                        }
                        else
                        {
                            jobRecord.IFSRequests += requestCount;
                        }


                        // Report Progress
                        RecordsProcessed++;
                        if ((RecordsProcessed % 1000) == 0)
                        {
                            progress.Report(new DataImportProgress(startTime, RecordsProcessed, false));
                        }

                        // Check for Cancellation
                        if (cancellationToken.IsCancellationRequested)
                            break;
                    }
                }
            }

            connection.Close();

            WriteJobRecordsToDatabase(true);

            progress.Report(new DataImportProgress(toDate, RecordsProcessed, true));

            return RecordsProcessed;
        }

        private WebServerRecord AddWebServerRecord(DateTime date, int hour)
        {
            if (_WebServerRecords.Count >= _MaxRecordCount)
                WriteJobRecordsToDatabase(false);

            var record = new WebServerRecord()
            {
                Date = date,
                Hour = hour
            };

            _WebServerRecords.Add(record);

            return record;
        }

        private SQLiteCommand _AddWebServerRequestsSQL;
        private SQLiteCommand _UpdateWebServerRequestsSQL;
        private SQLiteCommand _ReadWebServerRequestsSQL;
        private void WriteJobRecordsToDatabase(bool writeAll)
        {
            IEnumerable<WebServerRecord> recordsToWrite;
            if (writeAll)
                recordsToWrite = _WebServerRecords.ToList();
            else
                recordsToWrite = _WebServerRecords.OrderBy(x => x.Date).ThenBy(x => x.Hour).Take(_ArchiveRecordCount);


            if (_AddWebServerRequestsSQL == null)
            {
                _AddWebServerRequestsSQL = new SQLiteCommand("INSERT INTO WebServerRequests (Date, Hour, TotalRequests, CGIRequests, IFSRequests) VALUES(@Date, @Hour, @TotalRequests, @CGIRequests, @IFSRequests)", _SqliteConnection);
                _AddWebServerRequestsSQL.Prepare();

                _UpdateWebServerRequestsSQL = new SQLiteCommand("UPDATE WebServerRequests SET TotalRequests = @TotalRequests, CGIRequests = @CGIRequests, IFSRequests = @IFSRequests", _SqliteConnection);
                _UpdateWebServerRequestsSQL.Prepare();

                _ReadWebServerRequestsSQL = new SQLiteCommand("SELECT TotalRequests, CGIRequests, IFSRequests From WebServerRequests WHERE Date = @Date AND Hour = @Hour", _SqliteConnection);
                _ReadWebServerRequestsSQL.Prepare(); 
            }


            var transaction = _SqliteConnection.BeginTransaction();
            foreach (var record in recordsToWrite)
            {
                _ReadWebServerRequestsSQL.Parameters.AddWithValue("@Date", record.Date.ToString("yyy-MM-dd"));
                _ReadWebServerRequestsSQL.Parameters.AddWithValue("@Hour", record.Hour);

                var reader = _ReadWebServerRequestsSQL.ExecuteReader();
                if (reader.Read())
                {
                    record.TotalRequests += reader.GetInt32(0);
                    record.CGIRequests += reader.GetInt32(1);
                    record.IFSRequests += reader.GetInt32(2);

                    _UpdateWebServerRequestsSQL.Parameters.AddWithValue("@Date", record.Date.ToString("yyy-MM-dd"));
                    _UpdateWebServerRequestsSQL.Parameters.AddWithValue("@Hour", record.Hour);
                    _UpdateWebServerRequestsSQL.Parameters.AddWithValue("@TotalRequests", record.TotalRequests);
                    _UpdateWebServerRequestsSQL.Parameters.AddWithValue("@CGIRequests", record.CGIRequests);
                    _UpdateWebServerRequestsSQL.Parameters.AddWithValue("@IFSRequests", record.IFSRequests);

                    _UpdateWebServerRequestsSQL.ExecuteNonQuery();
                }
                else
                {

                    _AddWebServerRequestsSQL.Parameters.AddWithValue("@Date", record.Date.ToString("yyy-MM-dd"));
                    _AddWebServerRequestsSQL.Parameters.AddWithValue("@Hour", record.Hour);
                    _AddWebServerRequestsSQL.Parameters.AddWithValue("@TotalRequests", record.TotalRequests);
                    _AddWebServerRequestsSQL.Parameters.AddWithValue("@CGIRequests", record.CGIRequests);
                    _AddWebServerRequestsSQL.Parameters.AddWithValue("@IFSRequests", record.IFSRequests);

                    _AddWebServerRequestsSQL.ExecuteNonQuery(); 
                }
                reader.Close();


                _WebServerRecords.Remove(record);
            }

            transaction.Commit();

        }

    }

    class WebServerRecord
    {
        public DateTime Date { get; set; }
        public int Hour { get; set; }

        public long TotalRequests { get; set; }
        public long CGIRequests { get; set; }
        public long IFSRequests { get; set; }
    }
}
