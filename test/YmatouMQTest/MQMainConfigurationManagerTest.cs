using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YmatouMQNet4.Configuration;
using YmatouMQ.Common.Extensions._Task;
using YmatouMQ.ConfigurationSync;
namespace YmatouMQTest
{
    [TestClass]
    public class MQMainConfigurationManagerTest
    {
        [TestMethod]
        public void Start_Cfg_Maintain()
        {
            //MQMainConfigurationManager.TestConfigurationMaintain();
        }
        [TestMethod]
        public void Save_defaultCfg()
        {
            MQMainConfigurationManager.Builder.DumpMQDefaultConfigurationFile(MQMainConfiguration.DefaultMQCfg);
            Assert.IsTrue(true);
        }
        [TestMethod]
        [Xunit.Fact]
        public async Task Request_CfgServer_MQAppCfg()
        {
            var url = "http://mq.admin.ymatou.com/api/MQAppCfg/";
            var request = WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 3000;
            var s = await request.DownloadDataAsync(Encoding.GetEncoding("utf-8"));
            Console.WriteLine(s);
        }
        [TestMethod]
        [Xunit.Fact]
        public async Task Request_CfgServer_MQSysCfg()
        {
            var url = "http://mqadmin.ymatou.com/api/MQSysCfg/";
            var request = WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 3000;
            var s = await request.DownloadDataAsync(Encoding.GetEncoding("utf-8"));
            Console.WriteLine(s);

        }
        [TestMethod]
        [Xunit.Fact]
        public async Task Request_CfgServer_MQDefaultCfg()
        {
            var url = "http://mq.admin.ymatou.com/api/MQDefaultCfg/";
            var request = WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 3000;
            var s = await request.DownloadDataAsync(Encoding.GetEncoding("utf-8"));
            Console.WriteLine(s);
        }
    }
}
