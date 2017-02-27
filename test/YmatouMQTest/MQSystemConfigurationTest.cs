using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YmatouMQNet4.Configuration;

namespace YmatouMQTest
{
    [TestClass]
    public class MQSystemConfigurationTest
    {
        [TestMethod]
        public void Save_MQSystem_Cfg()
        {
            MQSystemConfiguration cfg = new MQSystemConfiguration(3000, 30, 5, true, true, 4, true, @"d:\log\mqlog\");
            cfg.SaveCfg();
            Assert.AreEqual(true, true);
        }
        [TestMethod]
        public void Load_MQSystem_Cfg()
        {
            var cfg = MQSystemConfiguration.LoadMQSysConfiguration();
            Assert.AreEqual(30, cfg.FulshLogTimestamp.Value);
            Assert.AreEqual(3000, cfg.FulshMQConfigurationTimestamp.Value);
            Assert.AreEqual(4, cfg.MaxThreadPublishAsync.Value);
            Assert.AreEqual(5, cfg.LogSize.Value);
            Assert.AreEqual(true, cfg.ConnShutdownMessageLocalEnqueue.Value);
            Assert.AreEqual(true, cfg.EnableTrackPubRunTime.Value);
            Assert.AreEqual(true, cfg.EnableTrackSubRunTime.Value);
            Assert.AreEqual(@"d:\log\mqlog\", cfg.LogFilePath);
        }
        [TestMethod]
        public void CheckLogDirectory()
        {
            var path = @"d:\log\mqlog\";
            var _path = Path.GetDirectoryName(path);
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            Assert.AreEqual(true, Directory.Exists(_path));
        }
        [TestMethod]
        public void LogPath() 
        {
            var _path = string.Format("{0}\\{1}{2}.log", @"d:\log\mqlog\", "mq", DateTime.Now.ToString("yyyyMMddHH"));
            Console.WriteLine(_path);
        }
    }
}
