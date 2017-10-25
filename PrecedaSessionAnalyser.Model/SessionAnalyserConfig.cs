using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;

using System.Data;
using System.Data.SQLite;

namespace PrecedaSessionAnalyser.Model
{
    public class SessionAnalyserConfig
    {
        private byte[] EncryptionKey = Encoding.ASCII.GetBytes("enfh8w34fQ4%%2dD");
        private byte[] AuthKey = Encoding.ASCII.GetBytes("$sji32xkcsFCFDF&");

        public string DatabasePath { get; private set; }
        public string Server { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Library { get; set; }
        public DateTime LastImport { get; set; }

        public SessionAnalyserConfig(string databasePath)
        {
            DatabasePath = databasePath;
        }

        public void Load()
        {
            var connection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;");
            connection.Open();

            var sql = String.Format("SELECT Server, User, Password, Library, LastImport FROM Config WHERE Id = 0");
            var command = new SQLiteCommand(sql, connection);
            var reader = command.ExecuteReader();

            if (reader.Read())
            {
                Server = reader.GetString(0);
                User = reader.GetString(1);
                Password = DecryptPassword(reader.GetString(2));
                Library = reader.GetString(3);
                LastImport = reader.GetDateTime(4);
            }
            else
            {
                Server = "";
                User = "";
                Password = "";
                Library = "";
                LastImport = new DateTime(0001, 01, 01);
            }

            reader.Close();

            if (LastImport.Ticks == 0)
            {
                LastImport = CalculateLastImportDate(connection);
            }

            connection.Close();
        }

        public void Save()
        {
            var connection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;");
            connection.Open();

            var sql = String.Format("SELECT Count(*) FROM Config WHERE [Id] = 0");
            var command = new SQLiteCommand(sql, connection);
            var count = command.ExecuteScalar();
            if ((long)count == 0)
            {
                sql = String.Format("INSERT INTO Config(Id, Server, User, Password, Library, LastImport) VALUES (0, @Server, @User, @Password, @Library, @LastImport)");
            }
            else
            {
                sql = String.Format("UPDATE Config SET Server = @Server, User = @User, Password = @Password, Library = @Library, LastImport = @LastImport WHERE [Id] = 0");

            }
            command = new SQLiteCommand(sql, connection);
            command.Parameters.Add("@Server", DbType.String);
            command.Parameters.Add("@User", DbType.String);
            command.Parameters.Add("@Password", DbType.String);
            command.Parameters.Add("@Library", DbType.String);
            command.Parameters.Add("@LastImport", DbType.DateTime);
            command.Prepare();

            command.Parameters["@Server"].Value = Server;
            command.Parameters["@User"].Value = User;
            command.Parameters["@Password"].Value = EncryptPassword(Password);
            command.Parameters["@Library"].Value = Library;
            command.Parameters["@LastImport"].Value = LastImport;

            command.ExecuteNonQuery();
            connection.Close();
        }

        private DateTime CalculateLastImportDate(SQLiteConnection connection)
        {
            var sql = String.Format("SELECT Max(Date) FROM Clients");
            var command2 = new SQLiteCommand(sql, connection);
            var lastDate = command2.ExecuteScalar();
            if (lastDate is System.DBNull)
                return new DateTime(0001, 01, 01);
            else
                return DateTime.ParseExact((string)lastDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private string EncryptPassword(string password)
        {
            if (password == "")
                return "";

            byte[] cipherText;

            using (var aes = new AesManaged
            {
                KeySize = 256,
                BlockSize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            {

                //Use random IV
                aes.GenerateIV();

                using (var encrypter = aes.CreateEncryptor(EncryptionKey, aes.IV))
                using (var cipherStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(cipherStream, encrypter, CryptoStreamMode.Write))
                    using (var binaryWriter = new BinaryWriter(cryptoStream))
                    {
                        binaryWriter.Write(password);
                    }

                    cipherText = cipherStream.ToArray();
                }

                return Convert.ToBase64String(aes.IV.Concat(cipherText).ToArray());

            }
        }


        private string DecryptPassword(string encryptedPassword)
        {
            if (encryptedPassword == "")
                return "";

            var encryptedData = Convert.FromBase64String(encryptedPassword);

            using (var aes = new AesManaged
            {
                KeySize = 256,
                BlockSize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            {
                //Grab IV from message
                var iv = new byte[16];
                Array.Copy(encryptedData, 0, iv, 0, iv.Length);

                using (var decrypter = aes.CreateDecryptor(EncryptionKey, iv))
                using (var plainTextStream = new MemoryStream())
                {
                    using (var decrypterStream = new CryptoStream(plainTextStream, decrypter, CryptoStreamMode.Write))
                    using (var binaryWriter = new BinaryWriter(decrypterStream))
                    {
                        //Decrypt Cipher Text from Message
                        binaryWriter.Write(
                            encryptedData,
                            iv.Length,
                            encryptedData.Length - iv.Length
                        );
                    }
                    //Return Plain Text
                    return Encoding.ASCII.GetString(plainTextStream.ToArray()).Substring(1);
                }
            }
        }
    }
}
