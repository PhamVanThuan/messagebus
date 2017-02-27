using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.IO;
using YmatouMQNet4.Extensions._Task;
using YmatouMQNet4.Extensions;
using YmatouMQNet4.Configuration;

namespace YmatouMQNet4.Logs
{
    /// <summary>
    /// file stream 辅助功能，支持定时刷新
    /// </summary>
    [Obsolete]
    public class LocalLogHelp
    {
        private static readonly object locker = new object();
        //一分钟刷新文件大小写 
        private static readonly int fulshTime = 60 * 1000;
        private static readonly int fileSize = 5;//5M;
        private static StreamWriter sw;
        private static Timer checkFileSizeTimer;
        private static bool fulshfile;
        private static string logfileName;

       
        public static void Write(string str)
        {
            if (string.IsNullOrEmpty(str)) return;
            try
            {
                //如果正在刷新文件则等待2秒
                if (fulshfile)
                {
                    SpinWait.SpinUntil(() => !fulshfile, 2000);
                }
              
                lock (locker)
                {
                    var strArray = (str).ToArray();
                    sw.Write(strArray, 0, strArray.Length);
                }
            }
            catch
            {
                //todo:
            }
        }
        private static void InitStreamWriter()
        {
            if (sw == null)
            {
                lock (locker)
                {
                    if (sw == null)
                    {
                        ReInitStreamWriter();
                    }
                }
            }
        }
        private static void ReInitStreamWriter()
        {
            sw = new StreamWriter(GetLogFileName(), true, Encoding.UTF8, 1024);
            sw.AutoFlush = true;
        }
        private static string GetLogFileName()
        {
            var filePath = MQSystemConfiguration.GetMQSysConfiguration().LogFilePath;
            var _logfileName = "mq_{0}_{1}.log".Fomart(AppDomain.CurrentDomain.FriendlyName.Replace(":", ""), DateTime.Now.ToString("yyyyMMdd_HH_mm"));
            logfileName = Path.Combine(filePath, _logfileName);
            return logfileName;
        }
        public static void Close()
        {
            try
            {
                if (sw != null)
                {
                    sw.Flush();
                    sw.Close();
                }
            }
            catch
            {
            }
        }
        private static void CheckLogDirectory(string path)
        {
            var _path = Path.GetDirectoryName(path);
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
        }
        private static void StartFulshLogFileWork()
        {
            CheckLogDirectory(MQSystemConfiguration.GetMQSysConfiguration().LogFilePath);
            InitStreamWriter();
            checkFileSizeTimer = new Timer(o =>
            {
                if (!string.IsNullOrEmpty(logfileName) && File.Exists(logfileName))
                {
                    var logSize = MQSystemConfiguration.GetMQSysConfiguration().LogSize ?? fileSize;
                    var fs = new FileInfo(logfileName);
                    if (fs.Length >= 10 * 1024 * 1024)
                    {
                        fulshfile = true;
                        Close();
                        ReInitStreamWriter();
                        fulshfile = false;
                    }
                    checkFileSizeTimer.Change(fulshTime, Timeout.Infinite);
                }
            }, null, Timeout.Infinite, Timeout.Infinite);
            checkFileSizeTimer.Change(fulshTime, Timeout.Infinite);

        }
    }
}
