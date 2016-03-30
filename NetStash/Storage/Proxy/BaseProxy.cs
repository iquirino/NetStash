using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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

        public BaseProxy()
        {
            lock (_lock)
            {
                if (initialized) return;

                if (!File.Exists(dbFilePath))
                {
                    SQLiteConnection.CreateFile(dbFilePath);

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

                    using (SQLiteConnection cnn = (SQLiteConnection)GetConnection())
                    {
                        cnn.Open();
                        SQLiteCommand cmd = new SQLiteCommand("CREATE TABLE \"Log\" ([IdLog] integer, [Message] nvarchar, PRIMARY KEY(IdLog));", cnn);
                        cmd.ExecuteNonQuery();
                    }
                }

                initialized = true;
            }
        }

        internal IDbConnection GetConnection()
        {
            return new SQLiteConnection(string.Format("Data Source={0};Version=3;", dbFilePath));
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
