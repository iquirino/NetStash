using System.Data;
using System.IO;
using System.Reflection;

namespace NetStash.Storage.Proxy
{
    public class BaseProxy
    {
        private static string dbFilePath = "./NetStash.db";
        private static bool initialized = false;
        private static object _lock = new object();

        public BaseProxy()
        {
            lock (_lock)
            {
                if (initialized) return;

                Directory.CreateDirectory("x86");
                if (!File.Exists("x86\\SQLite.Interop.dll"))
                    this.SaveToDisk("NetStash.x86.SQLite.Interop.dll", "x86\\SQLite.Interop.dll");

                Directory.CreateDirectory("x64");
                if (!File.Exists("x64\\SQLite.Interop.dll"))
                    this.SaveToDisk("NetStash.x64.SQLite.Interop.dll", "x64\\SQLite.Interop.dll");

                if (!File.Exists(dbFilePath))
                {
                    System.Data.SQLite.SQLiteConnection.CreateFile(dbFilePath);

                    using (var cnn = (System.Data.SQLite.SQLiteConnection)this.GetConnection())
                    {
                        cnn.Open();
                        var cmd = new System.Data.SQLite.SQLiteCommand("CREATE TABLE \"Log\" ([IdLog] integer, [Message] nvarchar, PRIMARY KEY(IdLog));", cnn);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (var cnn = (System.Data.SQLite.SQLiteConnection)this.GetConnection())
                    using (System.Data.SQLite.SQLiteCommand command = cnn.CreateCommand())
                    {
                        command.CommandText = "vacuum;";
                        command.ExecuteNonQuery();
                    }
                }

                initialized = true;
            }
        }

        internal IDbConnection GetConnection()
        {
            return new System.Data.SQLite.SQLiteConnection(string.Format("Data Source={0};Version=3;", dbFilePath));
        }

        private void SaveToDisk(string file, string name)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(file))
            using (var fileStream = new FileStream(name, FileMode.CreateNew))
                for (int i = 0; i < stream.Length; i++)
                    fileStream.WriteByte((byte)stream.ReadByte());
        }
    }
}
