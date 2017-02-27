using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Extensions._Task;

namespace YmatouMQ.Common.Utils
{
    public static class _Utils
    {
        public static string GetLocalHostIp()
        {
            return Dns.GetHostAddresses(Dns.GetHostName()).First(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();
        }

        public static string GetIP4(string hostName)
        {
            try
            {
                var ips = Dns.GetHostAddresses(hostName);
                return ips.First(i => i.AddressFamily == AddressFamily.InterNetwork).ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }          
        }

        public static bool IsOwnerCurrentHost(string host)
        {
            if (string.IsNullOrEmpty(host)) return true;
            var _host = _Utils.GetLocalHostIp().Split(new char[] { '.' });
            var _tmpHost = string.Format("{0}.{1}", _host[2], _host[3]);
            return host.Contains(_tmpHost);
        }

        public static string CreateDomainName(string appid, string code)
        {
            return "ad.{0}.{1}".Fomart(appid, code);
        }

        public static string GetCurrentHostIp4Last2()
        {
            var _host = _Utils.GetLocalHostIp().Split(new char[] { '.' });
            var _tmpHost = string.Format("{0}.{1}", _host[2], _host[3]);
            return _tmpHost;
        }

        public static string LoadLocalConfiguration(string path)
        {
            if (!File.Exists(path))
            {
                return string.Empty;
            }
            using (var fileStream = FileAsync.OpenRead(path))
            using (var streamRead = new StreamReader(fileStream, Encoding.GetEncoding("utf-8")))
            {
                return streamRead.ReadToEnd();
            }
        }
        public static string GetRequestMQConfigurationServer(string url, ILog log, int timeOut = 5000)
        {
            try
            {  //配置服务地址          
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Timeout = timeOut;
                request.ServicePoint.ConnectionLimit = UInt16.MaxValue;
                request.ServicePoint.ReceiveBufferSize = 10000000;
                return request
                    .DownloadDataAsync(Encoding.GetEncoding("utf-8"), ex => log.Error("mqmaincfg DownloadDataAsync error {0}".Fomart(ex.ToString())))
                    .GetResultSync(true, log, defReturn: string.Empty, descript: "请求配置服务 {0}".Fomart(url));
            }
            catch (AggregateException ex)
            {
                log.Error("从配置服务获取app配置异常0", ex);
                return string.Empty;
            }
            catch (Exception ex)
            {
                log.Error("从配置服务获取app配置异常1", ex);
                return string.Empty;
            }
        }
    }
}
