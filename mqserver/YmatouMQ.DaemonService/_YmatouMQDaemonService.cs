using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ymatou.CommonService;
using log4net;
using System.Net;

namespace YmatouMQ.DaemonService
{
    public class _YmatouMQDaemonService
    {
        private static Timer timerTask;
        private static DateTime serverTime = DateTime.Now;
        private static bool sendmessage = false;
        public static void Start()
        {

            log4net.Config.XmlConfigurator.Configure(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "log4net.config")));

            timerTask = new Timer(o =>
            {
                try
                {
                    TryRun();
                }
                catch (Exception ex)
                {
                    ApplicationLog.Error("YmatouMQ.Daemon service error", ex);
                }
                timerTask.Change(Convert.ToInt32(ConfigurationManager.AppSettings["Timer"] ?? "5000"), Timeout.Infinite);
            }, null, Timeout.Infinite, Timeout.Infinite);
            timerTask.Change(Convert.ToInt32(ConfigurationManager.AppSettings["Timer"] ?? "5000"), Timeout.Infinite);
        }
        public static void Stop()
        {
            try
            {
                timerTask.Dispose();
            }
            catch
            {

            }

        }
        public static void TryRun()
        {
            var serviceName = ConfigurationManager.AppSettings["Servicename"];
            var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Script");
            var minutes = Convert.ToInt32(ConfigurationManager.AppSettings["SendMessae_Minutes"] ?? "10");
            var serviceNameArray = serviceName.Split(new char[] { ',' });
            var message = new StringBuilder();
            var status = new StringBuilder();
            foreach (var name in serviceNameArray)
            {
                var serviceExists = CheckServiceIsActivate(name);
                if (serviceExists)
                {
                    status.AppendFormat("{0} is run ok.", name);
                    ApplicationLog.Info("service " + name + " is runing ok");
                }
                else
                {
                    message.AppendFormat("{0} is not runing,", name);
                    var serviceBatFile = string.Format(@"{0}\{1}.start.bat", scriptPath, name);
                    if (CheckStartBatFileExists(serviceBatFile))
                    {

                        var reStartResult = TryReStartService(serviceBatFile);
                        if (reStartResult.Item1)
                        {
                            message.AppendFormat(" {0} restart success.", name);
                        }
                        else
                        {
                            message.AppendFormat("service {0} restart fail,error msg:{1}.", name, reStartResult.Item2);
                        }
                        ApplicationLog.Debug(message.ToString());
                    }
                    else
                    {
                        message.AppendFormat("start script file {0} not find.", serviceBatFile);
                    }
                }
            }
            var subTime = Convert.ToInt32(DateTime.Now.Subtract(serverTime).TotalMinutes);
            if (/*!sendmessage &&*/ subTime > 0 && subTime % minutes == 0)
            {
                serverTime = DateTime.Now;
                status.AppendFormat(" ip {0}", GetLocalHostIp());
                ApplicationLog.Error(status.ToString());
            }
            else
            {
                status.Clear();
            }
            if (message.Length > 0)
            {
                message.AppendFormat(" ip {0}", GetLocalHostIp());
                ApplicationLog.Error(message.ToString());
                message.Clear();
            }
        }
        private static bool CheckServiceIsActivate(string serviceName)
        {
            var p = Process.GetProcessesByName(serviceName);
            if (p == null || !p.Any()) return false;
            var service = p.Single(_ => _.ProcessName == serviceName);
            return service.Responding && !service.HasExited;
        }
        private static Tuple<bool, string> TryReStartService(string serviceStartScriptPath)
        {
            try
            {
                var startInfo = new ProcessStartInfo(serviceStartScriptPath);
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //startInfo.UseShellExecute = true;
                Process.Start(startInfo);
                return Tuple.Create(true, string.Empty);
            }
            catch (Exception ex)
            {
                ApplicationLog.Debug("消息总线守护程序服务异常 " + ex.ToString());
                return Tuple.Create(false, ex.ToString()); ;
            }
        }
        private static bool CheckStartBatFileExists(string batfile)
        {
            if (!File.Exists(batfile)) return false;
            return true;
        }
        public static string GetLocalHostIp()
        {
            return Dns.GetHostAddresses(Dns.GetHostName()).Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).First().ToString();
        }
    }
}
