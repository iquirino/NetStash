//using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
//using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetStash.Storage.Proxy
{
    public class BaseProxy
    {
        static string dbFilePath = "./NetStash.db";
        static bool initialized = false;
        static object _lock = new object();

        internal static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        public BaseProxy()
        {
            lock (_lock)
            {
                if (initialized) return;

                if (!File.Exists(dbFilePath))
                {
                    if (!IsRunningOnMono())
                    {
                        System.Data.SQLite.SQLiteConnection.CreateFile(dbFilePath);

                        if (!Directory.Exists("x86"))
                        {
                            Directory.CreateDirectory("x86");
                            SaveToDisk("NetStash.x86.SQLite.Interop.dll", "x86\\SQLite.Interop.dll");
                        }

                        if (!Directory.Exists("x64"))
                        {
                            Directory.CreateDirectory("x64");
                            SaveToDisk("NetStash.x64.SQLite.Interop.dll", "x64\\SQLite.Interop.dll");
                        }

                        using (System.Data.SQLite.SQLiteConnection cnn = (System.Data.SQLite.SQLiteConnection)GetConnection())
                        {
                            cnn.Open();
                            System.Data.SQLite.SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand("CREATE TABLE \"Log\" ([IdLog] integer, [Message] nvarchar, PRIMARY KEY(IdLog));", cnn);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        Mono.Data.Sqlite.SqliteConnection.CreateFile(dbFilePath);

                        using (Mono.Data.Sqlite.SqliteConnection cnn = (Mono.Data.Sqlite.SqliteConnection)GetConnection())
                        {
                            cnn.Open();
                            Mono.Data.Sqlite.SqliteCommand cmd = new Mono.Data.Sqlite.SqliteCommand("CREATE TABLE \"Log\" ([IdLog] integer, [Message] nvarchar, PRIMARY KEY(IdLog));", cnn);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                initialized = true;
            }
        }

        internal IDbConnection GetConnection()
        {
            if (!IsRunningOnMono())
                return new System.Data.SQLite.SQLiteConnection(string.Format("Data Source={0};Version=3;", dbFilePath));
            else
                return new Mono.Data.Sqlite.SqliteConnection(string.Format("Data Source={0};Version=3;", dbFilePath));
        }

        private void SaveToDisk(string file, string name)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(file))
            using (FileStream fileStream = new FileStream(name, FileMode.CreateNew))
                for (int i = 0; i < stream.Length; i++)
                    fileStream.WriteByte((byte)stream.ReadByte());
        }
    }
}
