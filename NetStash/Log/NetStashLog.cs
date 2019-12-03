using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace NetStash.Log
{
    public class NetStashLog
    {
        private string logstashIp = string.Empty;
        private int logstashPort = -1;
        private string logname = string.Empty;
        private string system = string.Empty;

        public NetStashLog(string logstashIp, int logstashPort, SslProtocols protocol, X509CertificateCollection certificates, string serverCertificateName, string system, string logname = "NetStashLogs", RemoteCertificateValidationCallback certificateValidation = null)
        {
            if (string.IsNullOrWhiteSpace(logstashIp))
                throw new ArgumentNullException("logstashIp");

            if (string.IsNullOrWhiteSpace(logname))
                throw new ArgumentNullException("logname");

            if (string.IsNullOrWhiteSpace(system))
                throw new ArgumentNullException("system");

            Worker.TcpWorker.Initialize(logstashIp, logstashPort, protocol, certificates, serverCertificateName, certificateValidation);

            this.logstashIp = logstashIp;
            this.logstashPort = logstashPort;
            this.logname = logname;
            this.system = system;
        }

        public NetStashLog(string logstashIp, int logstashPort, string system, string logname = "NetStashLogs")
        {
            if (string.IsNullOrWhiteSpace(logstashIp))
                throw new ArgumentNullException("logstashIp");

            if (string.IsNullOrWhiteSpace(logname))
                throw new ArgumentNullException("logname");

            if (string.IsNullOrWhiteSpace(system))
                throw new ArgumentNullException("system");

            Worker.TcpWorker.Initialize(logstashIp, logstashPort);

            this.logstashIp = logstashIp;
            this.logstashPort = logstashPort;
            this.logname = logname;
            this.system = system;
        }

        public void Stop()
        {
            Worker.TcpWorker.Stop();
        }

        public void Restart()
        {
            Worker.TcpWorker.Restart();
        }

        public void Verbose(string message, Dictionary<string, string> values = null)
        {
            NetStashEvent netStashEvent = new NetStashEvent();
            netStashEvent.Level = NetStashLogLevel.Verbose.ToString();
            netStashEvent.Message = message;
            netStashEvent.Fields = values;

            this.AddSendToLogstash(netStashEvent);
        }

        public void Debug(string message, Dictionary<string, string> values = null)
        {
            NetStashEvent netStashEvent = new NetStashEvent();
            netStashEvent.Level = NetStashLogLevel.Debug.ToString();
            netStashEvent.Message = message;
            netStashEvent.Fields = values;

            this.AddSendToLogstash(netStashEvent);
        }

        public void Information(string message, Dictionary<string, string> values = null)
        {
            NetStashEvent netStashEvent = new NetStashEvent();
            netStashEvent.Level = NetStashLogLevel.Information.ToString();
            netStashEvent.Message = message;
            netStashEvent.Fields = values;

            this.AddSendToLogstash(netStashEvent);
        }

        public void Warning(string message, Dictionary<string, string> values = null)
        {
            NetStashEvent netStashEvent = new NetStashEvent();
            netStashEvent.Level = NetStashLogLevel.Warning.ToString();
            netStashEvent.Message = message;
            netStashEvent.Fields = values;

            this.AddSendToLogstash(netStashEvent);
        }


        internal void InternalError(string message, Dictionary<string, string> values = null)
        {
            NetStashEvent netStashEvent = new NetStashEvent();
            netStashEvent.Level = NetStashLogLevel.Error.ToString();
            netStashEvent.Message = message;
            netStashEvent.Fields = values;

            this.AddSendToLogstash(netStashEvent, false);
        }

        public void Error(Exception exception, Dictionary<string, string> values)
        {
            NetStashEvent netStashEvent = new NetStashEvent();
            netStashEvent.Level = NetStashLogLevel.Error.ToString();
            netStashEvent.Message = exception.Message;
            netStashEvent.ExceptionDetails = exception.StackTrace;
            netStashEvent.Fields = values;

            this.AddSendToLogstash(netStashEvent);
        }

        public void Error(string message, Dictionary<string, string> values = null)
        {
            NetStashEvent netStashEvent = new NetStashEvent();
            netStashEvent.Level = NetStashLogLevel.Error.ToString();
            netStashEvent.Message = message;
            netStashEvent.Fields = values;

            this.AddSendToLogstash(netStashEvent);
        }

        public void Fatal(string message, Dictionary<string, string> values = null)
        {
            NetStashEvent netStashEvent = new NetStashEvent();
            netStashEvent.Level = NetStashLogLevel.Fatal.ToString();
            netStashEvent.Message = message;
            netStashEvent.Fields = values;

            this.AddSendToLogstash(netStashEvent);
        }

        private void AddSendToLogstash(NetStashEvent e, bool run = true)
        {
            e.Machine = Environment.MachineName;
            e.Source = system;
            e.Index = logname;

            Storage.Proxy.LogProxy proxy = new Storage.Proxy.LogProxy();
            proxy.Add(e);

            if (run)
                Worker.TcpWorker.Run();
        }
    }
}
