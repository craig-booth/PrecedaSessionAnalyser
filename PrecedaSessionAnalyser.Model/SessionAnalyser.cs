using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Data;
using System.Data.SQLite;

namespace PrecedaSessionAnalyser.Model
{
    public class SessionAnalyser
    {
        public string DatabasePath { get; private set; }

        public SessionAnalyser(string databasePath)
        {
            DatabasePath = databasePath;
               
            CreateDatabase();      
        }

        private void CreateDatabase()
        {
            var connection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;");
            connection.Open();

            var sql = "CREATE TABLE IF NOT EXISTS Config (Id INT NOT NULL, Server CHAR(50) NOT NULL, User CHAR(10) NOT NULL, Password CHAR(100) NOT NULL, Library CHAR(10) NOT NULL, LastImport CHAR(10) NOT NULL, PRIMARY KEY(Id))";
            var command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();

            sql = "CREATE TABLE IF NOT EXISTS Sessions (Date CHAR(10) NOT NULL, Hour INT NOT NULL, Product INT NOT NULL, LogonCount INT NOT NULL, ActiveCount INT NOT NULL, PRIMARY KEY(Date, Hour, Product))";
            command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();

            sql = "CREATE TABLE IF NOT EXISTS Browsers (Date CHAR(10) NOT NULL, Browser CHAR(20) NOT NULL, Device CHAR(20) NOT NULL, Count INT NOT NULL, PRIMARY KEY(Date, Browser, Device))";
            command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();

            sql = "CREATE TABLE IF NOT EXISTS Clients (Date CHAR(10) NOT NULL, Partition CHAR(3) NOT NULL, FileLibrary CHAR(10) NOT NULL, Product INT NOT NULL, LogonCount INT NOT NULL, SingleSignOnCount INT NOT NULL, PRIMARY KEY(Date, FileLibrary, Product))";
            command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();

            sql = "CREATE TABLE IF NOT EXISTS LansaJobs (Date CHAR(10) NOT NULL, Hour INT NOT NULL, Started INT NOT NULL, Active INT NOT NULL, StartedByWebServer INT NOT NULL, StartedByLANSA INT NOT NULL, TotalCPU INT NOT NULL, LogicalDataBaseReads INT NOT NULL, LogicalDataBaseWrites INT NOT NULL, PhysicalDataBaseReads INT NOT NULL, PhysicalDataBaseWrites INT NOT NULL, TotalIFSBytesRead INT NOT NULL, TotalIFSBytesWritten INT NOT NULL,PRIMARY KEY(Date, Hour))";
            command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();

            sql = "CREATE TABLE IF NOT EXISTS WebServerRequests (Date CHAR(10) NOT NULL, Hour INT NOT NULL, TotalRequests INT NOT NULL, CGIRequests INT NOT NULL, IFSRequests INT NOT NULL,PRIMARY KEY(Date, Hour))";
            command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();

            connection.Close();
        }

        public SessionSummary GetSessionSummary(DateTime startTime, DateTime endTime, PeriodFrequency frequency)
        {
            var result = new SessionSummary(startTime, endTime, frequency);

            var connection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;");
            connection.Open();

            
            var sql = String.Format("SELECT Date, Hour, Product, Sum(LogonCount), Sum(ActiveCount) FROM Sessions WHERE (Date >= \"{0:yyyy-MM-dd}\" AND  Date <= \"{1:yyyy-MM-dd}\") GROUP BY Date, Hour, Product", startTime, endTime);
            var command = new SQLiteCommand(sql, connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var sessionTime = reader.GetDateTime(0).AddHours(reader.GetInt32(1));
                var product = (Product)reader.GetInt32(2);
                var logonCount = reader.GetInt32(3);
                var activeCount = reader.GetInt32(4);

                result.IncrementCount(sessionTime, product, logonCount, activeCount);               
            }

            reader.Close();
            connection.Close(); 

            return result;
        }

        public BrowserSummary GetBrowserSummary(DateTime startTime, DateTime endTime, PeriodFrequency frequency)
        {
            var result = new BrowserSummary(startTime, endTime, frequency);

            var connection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;");
            connection.Open();

            var sql = String.Format("SELECT Date, Browser, sum(Count) FROM Browsers WHERE (Date >= \"{0:yyyy-MM-dd}\" AND  Date <= \"{1:yyyy-MM-dd}\") GROUP BY Date, Browser", startTime, endTime);
            var command = new SQLiteCommand(sql, connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var sessionTime = reader.GetDateTime(0);
                var browser = reader.GetString(1);
                var count = reader.GetInt32(2);

                if (browser.StartsWith("CHROME"))
                    browser = "Chrome";
                else if (browser.StartsWith("IE"))
                    browser = "IE";
                else if (browser.StartsWith("GECKO"))
                    browser = "Gecko";
                else if (browser.StartsWith("SAFARI"))
                    browser = "Safari";
                else if (browser.StartsWith("MICROSOFT EDGE"))
                    browser = "Edge";
                else if (browser.StartsWith("OPERA"))
                    browser = "Opera";
                else if (browser.StartsWith("WEBKIT"))
                    browser = "WebKit";
                else if (browser == "OTHER")
                    browser = "Other";
                else if (browser == "")
                    browser = "Unknown";

                result.IncrementCount(browser, sessionTime, count);
            }

            reader.Close();
            connection.Close();

            return result;
        }

        public DeviceSummary GetDeviceSummary(DateTime startTime, DateTime endTime, PeriodFrequency frequency)
        {
            var result = new DeviceSummary(startTime, endTime, frequency);

            var connection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;");
            connection.Open();

            var sql = String.Format("SELECT Date, Device, sum(Count) FROM Browsers WHERE (Date >= \"{0:yyyy-MM-dd}\" AND  Date <= \"{1:yyyy-MM-dd}\") GROUP BY Date, Device", startTime, endTime);
            var command = new SQLiteCommand(sql, connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var sessionTime = reader.GetDateTime(0);
                var device = reader.GetString(1);
                var count = reader.GetInt32(2);

                if (device.StartsWith("WINDOWS"))
                    device = "Windows";
                else if (device.StartsWith("CHROME"))
                    device = "Chrome";
                else if (device.StartsWith("ANDROID"))
                    device = "Andriod";
                else if (device.StartsWith("LINUX"))
                    device = "Linux";
                else if (device == "IOS")
                    device = "iOS";
                else if (device == "OTHER")
                    device = "Other";
                else if (device == "")
                    device = "Unknown";


                result.IncrementCount(device, sessionTime, count);
            }

            reader.Close();
            connection.Close();

            return result;
        }

        public SingleSignOnSummary GetSingleSignOnSummary(DateTime startTime, DateTime endTime, PeriodFrequency frequency)
        {
            var result = new SingleSignOnSummary(startTime, endTime, frequency);

            var connection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;");
            connection.Open();

            var sql = String.Format("SELECT Date, Product, Sum(SingleSignOnCount) FROM Clients WHERE (Date >= \"{0:yyyy-MM-dd}\" AND  Date <= \"{1:yyyy-MM-dd}\") GROUP BY Date, Product", startTime, endTime);
            var command = new SQLiteCommand(sql, connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var sessionTime = reader.GetDateTime(0);
                var product = (Product)reader.GetInt32(1);
                var count = reader.GetInt32(2);

                result.IncrementCount(sessionTime, product, count);
            }

            reader.Close();
            connection.Close();

            return result;
        }

        public Top5CustomerSummary GetTop5CustomerSummary(DateTime startTime, DateTime endTime)
        {
            var result = new Top5CustomerSummary();

            var days = (endTime - startTime).Days + 1;

            var connection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;");
            connection.Open();

            var sql = String.Format("SELECT FileLibrary, Sum(LogonCount) FROM Clients WHERE (Date >= \"{0:yyyy-MM-dd}\" AND  Date <= \"{1:yyyy-MM-dd}\") GROUP BY FileLibrary", startTime, endTime);
            result.TotalLogons.AddRange(GetTop5CustomerData(connection, sql, days));

            sql = String.Format("SELECT FileLibrary, Sum(SingleSignOnCount) FROM Clients WHERE (Date >= \"{0:yyyy-MM-dd}\" AND  Date <= \"{1:yyyy-MM-dd}\") GROUP BY FileLibrary", startTime, endTime);
            result.SingleSignOnLogons.AddRange(GetTop5CustomerData(connection, sql, days));

            sql = String.Format("SELECT FileLibrary, Sum(LogonCount) FROM Clients WHERE (Date >= \"{0:yyyy-MM-dd}\" AND  Date <= \"{1:yyyy-MM-dd}\") AND Product = {2} GROUP BY FileLibrary", startTime, endTime, (int)Product.Mobile);
            result.MobileLogons.AddRange(GetTop5CustomerData(connection, sql, days));

            sql = String.Format("SELECT FileLibrary, Sum(LogonCount) FROM Clients WHERE (Date >= \"{0:yyyy-MM-dd}\" AND  Date <= \"{1:yyyy-MM-dd}\") AND Product = {2} GROUP BY FileLibrary", startTime, endTime, (int)Product.SelfService);
            result.SelfServiceLogons.AddRange(GetTop5CustomerData(connection, sql, days));

            sql = String.Format("SELECT FileLibrary, Sum(LogonCount) FROM Clients WHERE (Date >= \"{0:yyyy-MM-dd}\" AND  Date <= \"{1:yyyy-MM-dd}\") AND Product = {2} GROUP BY FileLibrary", startTime, endTime, (int)Product.Preceda);
            result.PrecedaLogons.AddRange(GetTop5CustomerData(connection, sql, days));

            sql = String.Format("SELECT FileLibrary, Sum(LogonCount) FROM Clients WHERE (Date >= \"{0:yyyy-MM-dd}\" AND  Date <= \"{1:yyyy-MM-dd}\") AND Product = {2} GROUP BY FileLibrary", startTime, endTime, (int)Product.IEPreceda);
            result.IELogons.AddRange(GetTop5CustomerData(connection, sql, days));
            
            connection.Close();

            return result;
        }

        private IEnumerable<CustomerRecord> GetTop5CustomerData(SQLiteConnection connection, string sql, int days)
        {
            var command = new SQLiteCommand(sql, connection);
            var reader = command.ExecuteReader();

            var customerRecords = new List<CustomerRecord>();
            while (reader.Read())
            {
                var customer = reader.GetString(0);
                var count = reader.GetInt32(1);

                customerRecords.Add(new CustomerRecord(customer, count));
            }
            reader.Close();

            var total = (double)customerRecords.Sum(x => x.Count);

            var top5 = customerRecords.OrderByDescending(x => x.Count).Take(5);
            foreach (var record in top5)
            {
                record.Average = record.Count / days;
                record.Percentage = (double)record.Count / total;
            }

            return top5;
        }

        public WebServerSummary GetWebServerSummary(DateTime startTime, DateTime endTime, PeriodFrequency frequency)
        {
            var result = new WebServerSummary(startTime, endTime, frequency);

            var connection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;");
            connection.Open();

            var sql = String.Format("SELECT Date, Hour, TotalRequests, CGIREquests, IFSRequests FROM WebServerRequests WHERE (Date >= \"{0:yyyy-MM-dd}\" AND  Date <= \"{1:yyyy-MM-dd}\")", startTime, endTime);
            var command = new SQLiteCommand(sql, connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var sessionTime = reader.GetDateTime(0).AddHours(reader.GetInt32(1));
                var totalRequests = reader.GetInt64(2);
                var cgiRequests = reader.GetInt64(3);
                var ifsRequests = reader.GetInt64(4);

                result.IncrementCount(sessionTime, totalRequests, cgiRequests, ifsRequests);
            }

            reader.Close();
            connection.Close();

            return result;
        }

        public StartedLansaJobSummary GetStartedLansaJobs(DateTime startTime, DateTime endTime, PeriodFrequency frequency)
        {
            var result = new StartedLansaJobSummary(startTime, endTime, frequency);

            var connection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;");
            connection.Open();

            var sql = String.Format("SELECT Date, Hour, Started, StartedByLansa, StartedByWebServer FROM LansaJobs WHERE (Date >= \"{0:yyyy-MM-dd}\" AND  Date <= \"{1:yyyy-MM-dd}\")", startTime, endTime);
            var command = new SQLiteCommand(sql, connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var sessionTime = reader.GetDateTime(0).AddHours(reader.GetInt32(1));
                var started = reader.GetInt64(2);
                var lansa = reader.GetInt64(3);
                var webserver = reader.GetInt64(4);

                result.IncrementCount(sessionTime, started, lansa, webserver);
            }

            reader.Close();
            connection.Close();

            return result;
        }

        public ActiveLansaJobSummary GetActiveLansaJobs(DateTime startTime, DateTime endTime, PeriodFrequency frequency)
        {
            var result = new ActiveLansaJobSummary(startTime, endTime, frequency);

            var connection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;");
            connection.Open();

            var sql = String.Format("SELECT Date, Hour, Active FROM LansaJobs WHERE (Date >= \"{0:yyyy-MM-dd}\" AND  Date <= \"{1:yyyy-MM-dd}\")", startTime, endTime);
            var command = new SQLiteCommand(sql, connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var sessionTime = reader.GetDateTime(0).AddHours(reader.GetInt32(1));
                var active = reader.GetInt64(2);

                result.IncrementCount(sessionTime, active);
            }

            reader.Close();
            connection.Close();

            return result;
        }

        public LansaJobCPUSummary GetLansaJobsCpu(DateTime startTime, DateTime endTime, PeriodFrequency frequency)
        {
            var result = new LansaJobCPUSummary(startTime, endTime, frequency);

            var connection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;");
            connection.Open();

            var sql = String.Format("SELECT Date, Hour, TotalCPU FROM LansaJobs WHERE (Date >= \"{0:yyyy-MM-dd}\" AND  Date <= \"{1:yyyy-MM-dd}\")", startTime, endTime);
            var command = new SQLiteCommand(sql, connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var sessionTime = reader.GetDateTime(0).AddHours(reader.GetInt32(1));
                var cpu = reader.GetInt64(2);

                result.IncrementCount(sessionTime, cpu);
            }

            reader.Close();
            connection.Close();

            return result;
        }

        public LansaJobIOSummary GetLansaJobsIO(DateTime startTime, DateTime endTime, PeriodFrequency frequency)
        {
            var result = new LansaJobIOSummary(startTime, endTime, frequency);

            var connection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;");
            connection.Open();

            var sql = String.Format("SELECT Date, Hour, LogicalDatabaseReads, LogicalDatabaseWrites, PhysicalDatabaseReads, PhysicalDatabaseWrites, TotalIFSBytesRead, TotalIFSBytesWritten FROM LansaJobs WHERE (Date >= \"{0:yyyy-MM-dd}\" AND  Date <= \"{1:yyyy-MM-dd}\")", startTime, endTime);
            var command = new SQLiteCommand(sql, connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var sessionTime = reader.GetDateTime(0).AddHours(reader.GetInt32(1));
                var logicalDBRead = reader.GetInt64(2);
                var logicalDBWrite = reader.GetInt64(3);
                var physicalDBRead = reader.GetInt64(4);
                var physicalDBWrite = reader.GetInt64(5);
                var ifsRead = reader.GetInt64(6) / 1024;
                var ifsWritten = reader.GetInt64(7) / 1024;

                result.IncrementCount(sessionTime, logicalDBRead, logicalDBWrite, physicalDBRead, physicalDBWrite, ifsRead, ifsWritten);
            }

            reader.Close();
            connection.Close();

            return result;
        }

        private DateTime DBToDateTime(string dbDate)
        {
            var date = dbDate.Substring(0, 10) + "T" + dbDate.Substring(11, 2) + ":" + dbDate.Substring(14, 2) + ":" + dbDate.Substring(17, 9);
            return (DateTime.Parse(date));
        }
    }




}
