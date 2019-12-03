using NetStash.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace NetStash.Worker
{
    public static class TcpWorker
    {
        static string logstashIp = string.Empty;
        static int logstashPort = -1;

        static SslProtocols sslProtocol = SslProtocols.None;
        static X509CertificateCollection certificateCollection = null;
        static RemoteCertificateValidationCallback remoteCertificateValidation = null;
        static string serverCertName = null;

        static object _lock = new object();
        static bool isRunning = false;
        static Task run;

        static bool stopCalled = false;

        public static void Initialize(string logstashAddressIp, int logstashAddressPort, SslProtocols protocol, X509CertificateCollection certificates, string serverCertificateName, RemoteCertificateValidationCallback certificateValidation = null)
        {
            InitializeWorker(logstashAddressIp, logstashAddressPort, protocol, certificates, serverCertificateName, certificateValidation);
        }

        public static void Initialize(string logstashAddressIp, int logstashAddressPort)
        {
            InitializeWorker(logstashAddressIp, logstashAddressPort);
        }

        private static void InitializeWorker(string logstashAddressIp, int logstashAddressPort, SslProtocols protocol = SslProtocols.None, X509CertificateCollection certificates = null, string serverCertificateName = null, RemoteCertificateValidationCallback certificateValidation = null)
        {
            if (string.IsNullOrWhiteSpace(logstashAddressIp))
                throw new ArgumentNullException("logstashAddressIp", "You need to inform a host address");

            if (logstashAddressPort <= 0)
                throw new ArgumentOutOfRangeException("logstashAddressPort", logstashAddressPort, "You need to inform a valid port number");

            logstashIp = logstashAddressIp;
            logstashPort = logstashAddressPort;
            sslProtocol = protocol;

            if (sslProtocol != SslProtocols.None)
            {
                if(certificates == null || certificates.Count <= 0)
                    throw new ArgumentNullException("certificates", "You need to inform a certificate to be used");

                certificateCollection = certificates;
                serverCertName = serverCertificateName;

                if (remoteCertificateValidation != null)
                    remoteCertificateValidation = certificateValidation;
            }

            Run();
        }

        public static void Run()
        {
            if (stopCalled) return;

            if (run == null || run.Status != TaskStatus.Running)
            {
                run = Task.Factory.StartNew(() =>
                {
                    lock (_lock)
                        isRunning = true;

                    while (isRunning && !stopCalled)
                    {
                        try
                        {
                            Runner();
                        }
                        catch (Exception ex)
                        {
                            NetStashLog log = new NetStashLog(logstashIp, logstashPort, "NetStash", "NetStash");
                            log.InternalError("Logstash communication error: " + ex.Message);
                        }
                    }
                });
            }
        }

        internal static void Restart()
        {
            lock (_lock)
                isRunning = true;

            stopCalled = false;

            Run();
        }

        internal static void Stop()
        {
            lock (_lock)
                isRunning = false;

            stopCalled = true;

            if (run != null) run.Wait();
        }

        private static void Runner()
        {
            Storage.Proxy.LogProxy proxy = new Storage.Proxy.LogProxy();
            Dictionary<long, string> evs;

            lock (_lock)
            {
                evs = proxy.GetList();
                if (evs.Count <= 0)
                {
                    isRunning = false;
                    return;
                }
            }

            Send(evs, RemoveEntry);
        }

        private static void RemoveEntry(long id)
        {
            Storage.Proxy.LogProxy proxy = new Storage.Proxy.LogProxy();
            proxy.Delete(id);
        }

        private static void Send(Dictionary<long, string> evs, Action<long> after)
        {
            using (TcpClient client = new TcpClient(logstashIp, logstashPort))
            {
                if (sslProtocol == SslProtocols.None)
                {
                    using (StreamWriter writer = new StreamWriter(client.GetStream()))
                    {
                        foreach (KeyValuePair<long, string> ev in evs)
                        {
                            writer.WriteLine(ev.Value.Replace(Environment.NewLine, "@($NL$)@"));
                            after(ev.Key);
                        }
                    }
                }
                else
                {
                    using (var writer = new SslStream(client.GetStream(), false, remoteCertificateValidation ?? Default_CertificateValidation))
                    {
                        writer.AuthenticateAsClient(serverCertName, certificateCollection, sslProtocol, remoteCertificateValidation == null);
                        foreach (KeyValuePair<long, string> ev in evs)
                        {
                            byte[] data = Encoding.Default.GetBytes(ev.Value.Replace(Environment.NewLine, "@($NL$)@"));
                            writer.Write(data);
                            after(ev.Key);
                        }
                    }
                }
            }
        }

        private static bool Default_CertificateValidation(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None) { return true; }
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors) { return true; }
            return false;
        }
    }
}
