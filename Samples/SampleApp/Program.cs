using NetStash.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            NetStashLog log = new NetStashLog("brspomelkq01.la.imtn.com", 1233, "NSTest", "NSTestLog");
            Dictionary<string, string> vals = new Dictionary<string, string>();

            log.Error("Testing", vals);

            Thread.Sleep(50000);

            log.Stop();
        }
    }
}
