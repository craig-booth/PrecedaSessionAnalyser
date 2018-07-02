using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Data.OleDb;
using System.Data.SQLite;

namespace PrecedaSessionAnalyser.Model.Import
{
    class PrecedaSessionImport : DataImport, IDataImport
    {
        private List<SessionRecord> _SessionRecords = new List<SessionRecord>();
        private List<BrowserRecord> _BrowserRecords = new List<BrowserRecord>();
        private List<ClientRecord> _ClientRecords = new List<ClientRecord>();

        private int _MaxSessionRecordCount = 500;
        private int _MaxBrowserRecordCount = 500;
        private int _MaxClientRecordCount = 500;

        private int _ArchiveSessionRecordCount = 100;
        private int _ArchiveBrowserRecordCount = 100;
        private int _ArchiveClientRecordCount = 100;

        public PrecedaSessionImport(string server, string user, string password, string library)
            : base("Preceda Sessions", server, user, password, library)
        {
        }

        public int ImportData(DateTime fromDate, DateTime toDate, SQLiteConnection sessionDatabase, IProgress<DataImportProgress> progress)
        {
            OnImportStarted(progress, fromDate);

            var connection = ConnectToServer();

            var sql = String.Format("SELECT PP54TYP, PP54STR, PP54END, PP54UTP, PP54DKV, PP54PRT, PP54FLB, PP54SSO, PP54BRW, PP54DEV FROM PPF54 WHERE PP54STR BETWEEN '{0:yyyy-MM-dd-00.00.00.000000}' AND '{1:yyyy-MM-dd-23.59.59.000000}'", fromDate, toDate);
            OleDbCommand command = new OleDbCommand(sql, connection);
            var reader = command.ExecuteReader();

            int RecordsProcessed = 0;

            while (reader.Read())
            {
                var sessionActive = (reader.GetString(0) == "A");
                var sessionStart = DBToDateTime(reader.GetString(1));
                DateTime sessionEnd;
                if (sessionActive)
                    sessionEnd = toDate;
                else
                    sessionEnd = DBToDateTime(reader.GetString(2));
                var userType = reader.GetInt32(3);
                var desktopVersion = reader.GetString(4);


                Product product = Product.Unknown; ;
                if (desktopVersion == "M")
                    product = Product.Mobile;
                else if (userType == 1)
                    product = Product.SelfService;
                else if (desktopVersion == "C")
                    product = Product.Preceda;
                else if (desktopVersion == "I")
                    product = Product.IEPreceda;

                var partition = reader.GetString(5);
                var fileLibrary = reader.GetString(6);
                var singleSignOn = (reader.GetString(7) == "Y");
                var browser = reader.GetString(8);
                var device = reader.GetString(9);


                UpdateSessionRecords(sessionDatabase, sessionStart, sessionEnd, product);
                UpdateBrowserRecord(sessionDatabase, sessionStart.Date, browser, device);
                UpdateClientRecord(sessionDatabase, sessionStart.Date, partition, fileLibrary, product, singleSignOn);

                // Report Progress
                RecordsProcessed++;
                if ((RecordsProcessed % 1000) == 0)
                {
                    OnImportProgress(progress, sessionStart, RecordsProcessed);
                }
            }

            reader.Close();
            connection.Close();

            WriteSessionRecordsToDatabase(sessionDatabase, true);
            WriteBrowserRecordsToDatabase(sessionDatabase, true);
            WriteClientRecordsToDatabase(sessionDatabase, true);

            OnImportComplete(progress, toDate, RecordsProcessed);

            return RecordsProcessed;
        }

        private void UpdateSessionRecords(SQLiteConnection sessionDatabase, DateTime sessionStart, DateTime sessionEnd, Product product)
        {

            // Get the summary records that this session was active for
            var time = sessionStart.Date.AddHours(sessionStart.Hour);

            while (time <= sessionEnd)
            {
                var date = time.Date;
                var hour = time.Hour;

                var sessionRecord = _SessionRecords.FirstOrDefault(x => (x.Date == date) && (x.Hour == hour) && (x.Product == product));
                if (sessionRecord == null)
                    sessionRecord = AddSessionRecord(sessionDatabase, date, hour, product);


                if ((date == sessionStart.Date) && (hour == sessionStart.Hour))
                {
                    sessionRecord.LogonCount++;
                    sessionRecord.ActiveCount++;
                }
                else
                {
                    sessionRecord.ActiveCount++;
                }

                time = time.AddHours(1);
            }
        }


        private void UpdateBrowserRecord(SQLiteConnection sessionDatabase, DateTime date, string browser, string device)
        {

            var browserRecord = _BrowserRecords.FirstOrDefault(x => (x.Date == date) && (x.Browser == browser) && (x.Device == device));
            if (browserRecord == null)
                browserRecord = AddBrowserRecord(sessionDatabase, date, browser, device);

            browserRecord.Count++;
        }

        private void UpdateClientRecord(SQLiteConnection sessionDatabase, DateTime date, string partition, string fileLibrary, Product product, bool singleSignOn)
        {

            var clientRecord = _ClientRecords.FirstOrDefault(x => (x.Date == date) && (x.Partition == partition) && (x.FileLibrary == fileLibrary) && (x.Product == product));
            if (clientRecord == null)
                clientRecord = AddClientRecord(sessionDatabase, date, partition, fileLibrary, product);

            clientRecord.LogonCount++;
            if (singleSignOn)
                clientRecord.SingleSignOnCount++;
        }


        private SessionRecord AddSessionRecord(SQLiteConnection sessionDatabase, DateTime date, int hour, Product product)
        {
            if (_SessionRecords.Count >= _MaxSessionRecordCount)
                WriteSessionRecordsToDatabase(sessionDatabase, false);

            var record = new SessionRecord()
            {
                Date = date,
                Hour = hour,
                Product = product
            };

            _SessionRecords.Add(record);

            return record;
        }

        private BrowserRecord AddBrowserRecord(SQLiteConnection sessionDatabase, DateTime date, string browser, string device)
        {
            if (_BrowserRecords.Count >= _MaxBrowserRecordCount)
                WriteBrowserRecordsToDatabase(sessionDatabase, false);

            var record = new BrowserRecord()
            {
                Date = date,
                Browser = browser,
                Device = device
            };

            _BrowserRecords.Add(record);

            return record;
        }

        private ClientRecord AddClientRecord(SQLiteConnection sessionDatabase, DateTime date, string partition, string fileLibrary, Product product)
        {
            if (_ClientRecords.Count >= _MaxClientRecordCount)
                WriteClientRecordsToDatabase(sessionDatabase, false);

            var record = new ClientRecord()
            {
                Date = date,
                Partition = partition,
                FileLibrary = fileLibrary,
                Product = product
            };

            _ClientRecords.Add(record);

            return record;
        }


        private SQLiteCommand _AddSessionsSQL;
        private SQLiteCommand _UpdateSessionsSQL;
        private SQLiteCommand _ReadSessionsSQL;
        private void WriteSessionRecordsToDatabase(SQLiteConnection sessionDatabase, bool writeAll)
        {
            IEnumerable<SessionRecord> recordsToWrite;
            if (writeAll)
                recordsToWrite = _SessionRecords.ToList();
            else
                recordsToWrite = _SessionRecords.OrderBy(x => x.Date).ThenBy(x => x.Hour).Take(_ArchiveSessionRecordCount);

            if (_AddSessionsSQL == null)
            {
                _AddSessionsSQL = new SQLiteCommand("INSERT INTO Sessions(Date, Hour, Product, LogonCount, ActiveCount) VALUES(@Date, @Hour, @Product, @LogonCount, @ActiveCount)", sessionDatabase);
                _AddSessionsSQL.Prepare();

                _UpdateSessionsSQL = new SQLiteCommand("UPDATE Sessions SET LogonCount = @LogonCount, ActiveCount =  @ActiveCount WHERE Date = @Date AND Hour = @Hour AND Product = @Product", sessionDatabase);
                _UpdateSessionsSQL.Prepare();

                _ReadSessionsSQL = new SQLiteCommand("SELECT LogonCount, ActiveCount From Sessions WHERE Date = @Date AND Hour = @Hour AND Product = @Product", sessionDatabase);
                _ReadSessionsSQL.Prepare();
            }

            foreach (var record in recordsToWrite)
            {
                _ReadSessionsSQL.Parameters.AddWithValue("@Date", record.Date.ToString("yyy-MM-dd"));
                _ReadSessionsSQL.Parameters.AddWithValue("@Hour", record.Hour);
                _ReadSessionsSQL.Parameters.AddWithValue("@Product", record.Product);

                var reader = _ReadSessionsSQL.ExecuteReader();
                if (reader.Read())
                {
                    record.LogonCount += reader.GetInt32(0);
                    record.ActiveCount += reader.GetInt32(1);

                    _UpdateSessionsSQL.Parameters.AddWithValue("@Date", record.Date.ToString("yyy-MM-dd"));
                    _UpdateSessionsSQL.Parameters.AddWithValue("@Hour", record.Hour);
                    _UpdateSessionsSQL.Parameters.AddWithValue("@Product", record.Product);
                    _UpdateSessionsSQL.Parameters.AddWithValue("@LogonCount", record.LogonCount);
                    _UpdateSessionsSQL.Parameters.AddWithValue("@ActiveCount", record.ActiveCount);

                    _UpdateSessionsSQL.ExecuteNonQuery();
                }
                else
                {

                    _AddSessionsSQL.Parameters.AddWithValue("@Date", record.Date.ToString("yyy-MM-dd"));
                    _AddSessionsSQL.Parameters.AddWithValue("@Hour", record.Hour);
                    _AddSessionsSQL.Parameters.AddWithValue("@Product", record.Product);
                    _AddSessionsSQL.Parameters.AddWithValue("@LogonCount", record.LogonCount);
                    _AddSessionsSQL.Parameters.AddWithValue("@ActiveCount", record.ActiveCount);

                    _AddSessionsSQL.ExecuteNonQuery();
                }
                reader.Close();


                _SessionRecords.Remove(record);
            }
        }

        private SQLiteCommand _AddBrowserSQL;
        private SQLiteCommand _UpdateBrowserSQL;
        private SQLiteCommand _ReadBrowserSQL;
        private void WriteBrowserRecordsToDatabase(SQLiteConnection sessionDatabase, bool writeAll)
        {
            IEnumerable<BrowserRecord> recordsToWrite;
            if (writeAll)
                recordsToWrite = _BrowserRecords.ToList();
            else
                recordsToWrite = _BrowserRecords.OrderBy(x => x.Date).Take(_ArchiveBrowserRecordCount);

            if (_AddBrowserSQL == null)
            {
                _AddBrowserSQL = new SQLiteCommand("INSERT INTO Browsers(Date, Browser, Device, Count) VALUES(@Date, @Browser, @Device, @Count)", sessionDatabase);
                _AddBrowserSQL.Prepare();

                _UpdateBrowserSQL = new SQLiteCommand("UPDATE Browsers SET Count = @Count WHERE Date = @Date AND Browser = @Browser AND Device = @Device", sessionDatabase);
                _UpdateBrowserSQL.Prepare();

                _ReadBrowserSQL = new SQLiteCommand("SELECT  Count From Browsers WHERE Date = @Date AND Browser = @Browser AND Device = @Device", sessionDatabase);
                _ReadBrowserSQL.Prepare();
            }

            foreach (var record in recordsToWrite)
            {
                _ReadBrowserSQL.Parameters.AddWithValue("@Date", record.Date.ToString("yyy-MM-dd"));
                _ReadBrowserSQL.Parameters.AddWithValue("@Browser", record.Browser);
                _ReadBrowserSQL.Parameters.AddWithValue("@Device", record.Device);

                var reader = _ReadBrowserSQL.ExecuteReader();
                if (reader.Read())
                {
                    record.Count += reader.GetInt32(0);

                    _UpdateBrowserSQL.Parameters.AddWithValue("@Date", record.Date.ToString("yyy-MM-dd"));
                    _UpdateBrowserSQL.Parameters.AddWithValue("@Browser", record.Browser);
                    _UpdateBrowserSQL.Parameters.AddWithValue("@Device", record.Device);
                    _UpdateBrowserSQL.Parameters.AddWithValue("@Count", record.Count);


                    _UpdateBrowserSQL.ExecuteNonQuery();
                }
                else
                {
                    _AddBrowserSQL.Parameters.AddWithValue("@Date", record.Date.ToString("yyy-MM-dd"));
                    _AddBrowserSQL.Parameters.AddWithValue("@Browser", record.Browser);
                    _AddBrowserSQL.Parameters.AddWithValue("@Device", record.Device);
                    _AddBrowserSQL.Parameters.AddWithValue("@Count", record.Count);

                    _AddBrowserSQL.ExecuteNonQuery();
                }
                reader.Close();

                _BrowserRecords.Remove(record);
            }
        }

        private SQLiteCommand _AddClientSQL;
        private SQLiteCommand _UpdateClientSQL;
        private SQLiteCommand _ReadClientSQL;
        private void WriteClientRecordsToDatabase(SQLiteConnection sessionDatabase, bool writeAll)
        {
            IEnumerable<ClientRecord> recordsToWrite;
            if (writeAll)
                recordsToWrite = _ClientRecords.ToList();
            else
                recordsToWrite = _ClientRecords.OrderBy(x => x.Date).Take(_ArchiveClientRecordCount);


            if (_AddClientSQL == null)
            {
                _AddClientSQL = new SQLiteCommand("INSERT INTO Clients(Date, Partition, FileLibrary, Product, LogonCount, SingleSignOnCount) VALUES(@Date, @Partition, @FileLibrary, @Product, @LogonCount, @SingleSignOnCount)", sessionDatabase);
                _AddClientSQL.Prepare();

                _UpdateClientSQL = new SQLiteCommand("UPDATE Clients SET LogonCount = @LogonCount, SingleSignOnCount = @SingleSignOnCount WHERE Date = @Date AND FileLibrary = @FileLibrary AND Product = @Product", sessionDatabase);
                _UpdateClientSQL.Prepare();

                _ReadClientSQL = new SQLiteCommand("SELECT LogonCount, SingleSignOnCount From Clients WHERE Date = @Date AND FileLibrary = @FileLibrary AND Product = @Product", sessionDatabase);
                _ReadClientSQL.Prepare();
            }


            foreach (var record in recordsToWrite)
            {
                _ReadClientSQL.Parameters.AddWithValue("@Date", record.Date.ToString("yyy-MM-dd"));
                _ReadClientSQL.Parameters.AddWithValue("@FileLibrary", record.FileLibrary);
                _ReadClientSQL.Parameters.AddWithValue("@Product", record.Product);

                var reader = _ReadClientSQL.ExecuteReader();
                if (reader.Read())
                {
                    record.LogonCount += reader.GetInt32(0);
                    record.SingleSignOnCount += reader.GetInt32(1);

                    _UpdateClientSQL.Parameters.AddWithValue("@Date", record.Date.ToString("yyy-MM-dd"));
                    _UpdateClientSQL.Parameters.AddWithValue("@FileLibrary", record.FileLibrary);
                    _UpdateClientSQL.Parameters.AddWithValue("@Product", record.Product);
                    _UpdateClientSQL.Parameters.AddWithValue("@LogonCount", record.LogonCount);
                    _UpdateClientSQL.Parameters.AddWithValue("@SingleSignOnCount", record.SingleSignOnCount);

                    _UpdateClientSQL.ExecuteNonQuery();
                }
                else
                {
                    _AddClientSQL.Parameters.AddWithValue("@Date", record.Date.ToString("yyy-MM-dd"));
                    _AddClientSQL.Parameters.AddWithValue("@Partition", record.Partition);
                    _AddClientSQL.Parameters.AddWithValue("@FileLibrary", record.FileLibrary);
                    _AddClientSQL.Parameters.AddWithValue("@Product", record.Product);
                    _AddClientSQL.Parameters.AddWithValue("@LogonCount", record.LogonCount);
                    _AddClientSQL.Parameters.AddWithValue("@SingleSignOnCount", record.SingleSignOnCount);

                    _AddClientSQL.ExecuteNonQuery();
                }
                reader.Close();

                _ClientRecords.Remove(record);
            }
        }

        private DateTime DBToDateTime(string dbDate)
        {
            var date = dbDate.Substring(0, 10) + "T" + dbDate.Substring(11, 2) + ":" + dbDate.Substring(14, 2) + ":" + dbDate.Substring(17, 9);
            return (DateTime.Parse(date));
        }
    }

    class SessionRecord
    {
        public DateTime Date { get; set; }
        public int Hour { get; set; }
        public Product Product { get; set; }

        public int LogonCount { get; set; }
        public int ActiveCount { get; set; }
    }

    class BrowserRecord
    {
        public DateTime Date { get; set; }
        public string Browser { get; set; }
        public string Device { get; set; }

        public int Count { get; set; }
    }

    class ClientRecord
    {
        public DateTime Date { get; set; }
        public string Partition { get; set; }
        public string FileLibrary { get; set; }
        public Product Product { get; set; }

        public int LogonCount { get; set; }
        public int SingleSignOnCount { get; set; }
    }
}
