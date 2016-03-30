using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace NetStash.Storage.Proxy
{
    public class LogProxy : BaseProxy
    {
        public LogProxy() : base()
        {

        }

        public void Add(NetStashEvent log)
        {
            Entities.Log addLog = new Entities.Log();
            addLog.Message = Newtonsoft.Json.JsonConvert.SerializeObject(log);

            using (IDbConnection db = base.GetConnection())
                db.Execute("INSERT INTO Log (Message) VALUES (@Message)", new { Message = addLog.Message });
        }

        public NetStashEvent Get(out long id)
        {
            Entities.Log getLog;
            using (IDbConnection db = base.GetConnection())
                getLog = db.QueryFirstOrDefault<Entities.Log>("SELECT IdLog, Message from Log order by IdLog asc LIMIT 1");

            if (getLog == null)
            {
                id = -1;
                return null;
            }

            id = getLog.IdLog;

            return Newtonsoft.Json.JsonConvert.DeserializeObject<NetStashEvent>(getLog.Message);
        }

        public Dictionary<long, string> GetList(int count = 100)
        {
            Dictionary<long, string> ret = new Dictionary<long, string>();

            IEnumerable<Entities.Log> evs;

            using (IDbConnection db = base.GetConnection())
                evs = db.Query<Entities.Log>("SELECT IdLog, Message from Log order by IdLog asc LIMIT " + count);

            foreach (Entities.Log item in evs)
                ret.Add(item.IdLog, item.Message);

            return ret;
        }

        public void Delete(long id)
        {
            if (id < 0) return;

            using (IDbConnection db = base.GetConnection())
                db.Execute("DELETE FROM Log WHERE IdLog = @IdLog", new { IdLog = id });
        }
    }
}
