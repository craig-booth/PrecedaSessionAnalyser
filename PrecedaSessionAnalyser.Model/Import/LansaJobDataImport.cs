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
    class LansaJobDataImport : IBMPerformanceDataImport, IDataImport
    {
        private int _MaxRecordCount = 500;
        private int _ArchiveRecordCount = 100;
        private List<LansaJobRecord> _JobRecords = new List<LansaJobRecord>();

        public LansaJobDataImport(string server, string user, string password)
            : base("LANSA Jobs", server, user, password)
        {
        }

        public int ImportData(DateTime fromDate, DateTime toDate, SQLiteConnection sessionDatabase, IProgress<DataImportProgress> progress)
        {
            OnImportStarted(progress, fromDate);

            var connection = ConnectToServer();

            int RecordsProcessed = 0;

            var memberList = GetAvailableMembers(connection);

            foreach (var member in memberList.Where(x => (x.StartTime.Date >= fromDate) && (x.StartTime.Date <= toDate)))
            {
                SetCurrentMember(connection, "QAPMJOBL", member.MemberName);


                var sql = String.Format("SELECT INTNUM, JBSJNM, JBSTSF, count(*), sum(JBCPU), sum(JBLRD), sum(JBLWT), sum(JBDBR) + sum(JBADBR), sum(JBDBW) + sum(JBADBW), sum(JBXRFR), sum(JBXRFW) FROM QTEMP.QAPMJOBL WHERE JBNAME = 'LWEB_JOB' GROUP BY INTNUM, JBSJNM, JBSTSF ORDER BY INTNUM");
                OleDbCommand command = new OleDbCommand(sql, connection);

                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var interval = (int)reader.GetDecimal(0);
                            var submittedBy = reader.GetString(1);
                            var jobStatus = (IBMJobStatusFlag)reader.GetDecimal(2);
                            var jobCount = reader.GetInt32(3);
                            var totalCPU = (int)(reader.GetDecimal(4) / 1000);
                            var logicalDBReads = (int)reader.GetDecimal(5);
                            var logicalDBWrites = (int)reader.GetDecimal(6);
                            var physicalDBReads = (int)reader.GetDecimal(7);
                            var physicalDBWrites = (int)reader.GetDecimal(8);
                            var totalIFSBytesRead = (int)reader.GetDecimal(9);
                            var totalIFSBytesWritten = (int)reader.GetDecimal(10);

                            var startTime = member.StartTime.AddMinutes((interval - 1) * member.Interval);

                            var jobRecord = _JobRecords.FirstOrDefault(x => (x.Date == startTime.Date) && (x.Hour == startTime.Hour));
                            if (jobRecord == null)
                                jobRecord = AddJobRecord(sessionDatabase, startTime.Date, startTime.Hour);

                            if ((jobStatus == IBMJobStatusFlag.Started) || (jobStatus == IBMJobStatusFlag.StartedAndEnded))
                            {
                                jobRecord.Started += jobCount;

                                if (submittedBy == "LWEB_MON")
                                    jobRecord.StartedByLANSA += jobCount;
                                else
                                    jobRecord.StartedByWebServer += jobCount;
                            }

                            jobRecord.Active += jobCount;
                            jobRecord.TotalCPU += totalCPU;
                            jobRecord.LogicalDataBaseReads = logicalDBReads;
                            jobRecord.LogicalDataBaseWrites = logicalDBWrites;
                            jobRecord.PhysicalDataBaseReads += physicalDBReads;
                            jobRecord.PhysicalDataBaseWrites += physicalDBWrites;
                            jobRecord.TotalIFSBytesRead += totalIFSBytesRead;
                            jobRecord.TotalIFSBytesWritten += totalIFSBytesWritten;


                            // Report Progress
                            RecordsProcessed++;
                            if ((RecordsProcessed % 1000) == 0)
                            {
                                OnImportProgress(progress, startTime, RecordsProcessed);
                            }
                        }
                    }
                }
                catch
                {

                }
            }

            connection.Close();

            WriteJobRecordsToDatabase(sessionDatabase, true);

            OnImportComplete(progress, toDate, RecordsProcessed);

            return RecordsProcessed;
        }

        private LansaJobRecord AddJobRecord(SQLiteConnection sessionDatabase, DateTime date, int hour)
        {
            if (_JobRecords.Count >= _MaxRecordCount)
                WriteJobRecordsToDatabase(sessionDatabase, false);

            var record = new LansaJobRecord()
            {
                Date = date,
                Hour = hour
            };

            _JobRecords.Add(record);

            return record;
        }

        private SQLiteCommand _AddLANSAJobsSQL;
        private SQLiteCommand _UpdateLANSAJobsSQL;
        private SQLiteCommand _ReadLANSAJobsSQL;
        private void WriteJobRecordsToDatabase(SQLiteConnection sessionDatabase, bool writeAll)
        {
            IEnumerable<LansaJobRecord> recordsToWrite;
            if (writeAll)
                recordsToWrite = _JobRecords.ToList();
            else
                recordsToWrite = _JobRecords.OrderBy(x => x.Date).ThenBy(x => x.Hour).Take(_ArchiveRecordCount);


            if (_AddLANSAJobsSQL == null)
            {
                _AddLANSAJobsSQL = new SQLiteCommand("INSERT INTO LansaJobs(Date, Hour, Started, Active, StartedByWebServer, StartedByLANSA, TotalCPU, LogicalDataBaseReads, LogicalDataBaseWrites, PhysicalDataBaseReads, PhysicalDataBaseWrites, TotalIFSBytesRead, TotalIFSBytesWritten) VALUES(@Date, @Hour, @Started, @Active, @StartedByWebServer, @StartedByLANSA, @TotalCPU, @LogicalDataBaseReads, @LogicalDataBaseWrites, @PhysicalDataBaseReads, @PhysicalDataBaseWrites, @TotalIFSBytesRead, @TotalIFSBytesWritten)", sessionDatabase);
                _AddLANSAJobsSQL.Prepare();

                _UpdateLANSAJobsSQL = new SQLiteCommand("UPDATE LansaJobs SET Started = @Started, Active = @Active, StartedByWebServer = @StartedByWebServer, StartedByLANSA = @StartedByLANSA, TotalCPU = @TotalCPU, LogicalDataBaseReads = @LogicalDataBaseReads, LogicalDataBaseWrites = @LogicalDataBaseWrites, PhysicalDataBaseReads = @PhysicalDataBaseReads, PhysicalDataBaseWrites = @PhysicalDataBaseWrites, TotalIFSBytesRead = @TotalIFSBytesRead, TotalIFSBytesWritten = @TotalIFSBytesWritten WHERE Date = @Date AND Hour = @Hour", sessionDatabase);
                _UpdateLANSAJobsSQL.Prepare();

                _ReadLANSAJobsSQL = new SQLiteCommand("SELECT Started, Active, StartedByWebServer, StartedByLANSA, TotalCPU, LogicalDataBaseReads, LogicalDataBaseWrites, PhysicalDataBaseReads, PhysicalDataBaseWrites, TotalIFSBytesRead, TotalIFSBytesWritten From LansaJobs WHERE Date = @Date AND Hour = @Hour", sessionDatabase);
                _ReadLANSAJobsSQL.Prepare();
            }


            foreach (var record in recordsToWrite)
            {
                _ReadLANSAJobsSQL.Parameters.AddWithValue("@Date", record.Date.ToString("yyy-MM-dd"));
                _ReadLANSAJobsSQL.Parameters.AddWithValue("@Hour", record.Hour);

                var reader = _ReadLANSAJobsSQL.ExecuteReader();
                if (reader.Read())
                {
                    record.Started += reader.GetInt32(0);
                    record.Active += reader.GetInt32(1);
                    record.StartedByWebServer += reader.GetInt32(2);
                    record.StartedByLANSA += reader.GetInt32(3);
                    record.TotalCPU += reader.GetInt32(4);
                    record.LogicalDataBaseReads += reader.GetInt32(5);
                    record.LogicalDataBaseWrites += reader.GetInt32(6);
                    record.PhysicalDataBaseReads += reader.GetInt32(7);
                    record.PhysicalDataBaseWrites += reader.GetInt32(8);
                    record.TotalIFSBytesRead += reader.GetInt32(9);
                    record.TotalIFSBytesWritten += reader.GetInt32(10);

                    _UpdateLANSAJobsSQL.Parameters.AddWithValue("@Date", record.Date.ToString("yyy-MM-dd"));
                    _UpdateLANSAJobsSQL.Parameters.AddWithValue("@Hour", record.Hour);
                    _UpdateLANSAJobsSQL.Parameters.AddWithValue("@Started", record.Started);
                    _UpdateLANSAJobsSQL.Parameters.AddWithValue("@Active", record.Active);
                    _UpdateLANSAJobsSQL.Parameters.AddWithValue("@StartedByWebServer", record.StartedByWebServer);
                    _UpdateLANSAJobsSQL.Parameters.AddWithValue("@StartedByLANSA", record.StartedByLANSA);
                    _UpdateLANSAJobsSQL.Parameters.AddWithValue("@TotalCPU", record.TotalCPU);
                    _UpdateLANSAJobsSQL.Parameters.AddWithValue("@LogicalDataBaseReads", record.LogicalDataBaseReads);
                    _UpdateLANSAJobsSQL.Parameters.AddWithValue("@LogicalDataBaseWrites", record.LogicalDataBaseWrites);
                    _UpdateLANSAJobsSQL.Parameters.AddWithValue("@PhysicalDataBaseReads", record.PhysicalDataBaseReads);
                    _UpdateLANSAJobsSQL.Parameters.AddWithValue("@PhysicalDataBaseWrites", record.PhysicalDataBaseWrites);
                    _UpdateLANSAJobsSQL.Parameters.AddWithValue("@TotalIFSBytesRead", record.TotalIFSBytesRead);
                    _UpdateLANSAJobsSQL.Parameters.AddWithValue("@TotalIFSBytesWritten", record.TotalIFSBytesWritten);

                    _UpdateLANSAJobsSQL.ExecuteNonQuery();
                }
                else
                {

                    _AddLANSAJobsSQL.Parameters.AddWithValue("@Date", record.Date.ToString("yyy-MM-dd"));
                    _AddLANSAJobsSQL.Parameters.AddWithValue("@Hour", record.Hour);
                    _AddLANSAJobsSQL.Parameters.AddWithValue("@Started", record.Started);
                    _AddLANSAJobsSQL.Parameters.AddWithValue("@Active", record.Active);
                    _AddLANSAJobsSQL.Parameters.AddWithValue("@StartedByWebServer", record.StartedByWebServer);
                    _AddLANSAJobsSQL.Parameters.AddWithValue("@StartedByLANSA", record.StartedByLANSA);
                    _AddLANSAJobsSQL.Parameters.AddWithValue("@TotalCPU", record.TotalCPU);
                    _AddLANSAJobsSQL.Parameters.AddWithValue("@LogicalDataBaseReads", record.LogicalDataBaseReads);
                    _AddLANSAJobsSQL.Parameters.AddWithValue("@LogicalDataBaseWrites", record.LogicalDataBaseWrites);
                    _AddLANSAJobsSQL.Parameters.AddWithValue("@PhysicalDataBaseReads", record.PhysicalDataBaseReads);
                    _AddLANSAJobsSQL.Parameters.AddWithValue("@PhysicalDataBaseWrites", record.PhysicalDataBaseWrites);
                    _AddLANSAJobsSQL.Parameters.AddWithValue("@TotalIFSBytesRead", record.TotalIFSBytesRead);
                    _AddLANSAJobsSQL.Parameters.AddWithValue("@TotalIFSBytesWritten", record.TotalIFSBytesWritten);

                    _AddLANSAJobsSQL.ExecuteNonQuery();
                }
                reader.Close();


                _JobRecords.Remove(record);
            }

        }

    }

    enum IBMJobStatusFlag { Normal, Started, Ended, StartedAndEnded }

    class LansaJobRecord
    {
        public DateTime Date { get; set; }
        public int Hour { get; set; }

        public int Started { get; set; }
        public int Active { get; set; }
        public int StartedByWebServer { get; set; }
        public int StartedByLANSA { get; set; }
        public int TotalCPU { get; set; }
        public int LogicalDataBaseReads { get; set; }
        public int LogicalDataBaseWrites { get; set; }
        public int PhysicalDataBaseReads { get; set; }
        public int PhysicalDataBaseWrites { get; set; }
        public int TotalIFSBytesRead { get; set; }
        public int TotalIFSBytesWritten { get; set; }
    }
}
