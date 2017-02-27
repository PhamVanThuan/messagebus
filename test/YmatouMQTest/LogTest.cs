using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQNet4.Logs;
using YmatouMQNet4;
using YmatouMQ.Common;
using YmatouMQ.Log;
namespace YmatouMQTest
{
    [TestClass]
    public class LogTest
    {
        private ILog log;
        [TestInitialize]
        public void Setup()
        {
            //log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "log4net.config")));
            log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQTest.LogTest");
        }
        [TestMethod]
        public void Log_Info()
        {
            for (var i = 0; i < 10; i++)
                log.Info("ss" + i);
        }
        [TestMethod]
        public void Log_Error()
        {
            for (var i = 0; i < 10000; i++)
                log.Error("{0},{1}", i, "test2");
        }
        [TestMethod]
        public void Log_Debug()
        {
            for (var i = 0; i < 10; i++)
                log.Debug("ss" + i);
        }
        [TestMethod]
        public void Formart() 
        {
            long l = 1000;
            Console.WriteLine("{0}", l);
        }
    }
}
