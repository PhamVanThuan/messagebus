using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YmatouMQNet4.Configuration;

using YmatouMQ.Common.Extensions.Serialization;
using YmatouMQNet4.Core;
using System.Collections.Generic;
using System.Linq;
using YmatouMQMessageMongodb.AppService.Configuration;
using YmatouMQ.ConfigurationSync;
using YmatouMQ.Common.Extensions;
namespace YmatouMQTest
{
    [TestClass]
    public class ConfigurationTest
    {
        [TestMethod]
        public void Serializatio_MQDefaultCfg_String()
        {
            var str = MQMainConfiguration.DefaultMQCfg.JSONSerializationToString<MQMainConfiguration>();
            Console.WriteLine(str);
        }
        [TestMethod]
        public void Serializatio_MQCfg_String()
        {
            var dic = new Dictionary<string, MQMainConfiguration>();
            dic["seller"] = MQMainConfiguration.DefaultMQCfg;
            dic["app"] = MQMainConfiguration.DefaultMQCfg;
            var str = dic.JSONSerializationToString();
            Console.WriteLine(str);
        }
        [TestMethod]
        public void CreateDomainName()
        {
            Dictionary<string, AppdomainConfiguration> domainList = new Dictionary<string, AppdomainConfiguration> 
            {
                {"a",new AppdomainConfiguration{AppId="c0",Code="c1",Items=new List<DomainItem>{{new DomainItem{AppId="c0",Code="c1"}}}}},
                {"b",new AppdomainConfiguration{AppId="c2",Code="c2",Items=new List<DomainItem>{new DomainItem{AppId="c2",Code="c2"}}}},
                {"c",new AppdomainConfiguration{AppId="c3",Code="c3",Items=new List<DomainItem>{new DomainItem{AppId="c3",Code="c4"}}}}
            };

            var str = domainList.Select(e => e.Value).SelectMany(e => e.Items).Aggregate("ad.", (name, a) => CreateDomainName(name, a.AppId, a.Code), r => r);
            // Console.WriteLine(str);
        }
        private static string CreateDomainName(string name, string appid, string code)
        {
            Console.WriteLine(name);
            return string.Format("ad.{0}.{1}", appid, code);
        }
        //发布消息，全局配置
        [TestMethod]
        public void Find_PublishMessage_Default_Cfg()
        {
            MQAppConfigurationAppService cfgAppService = new MQAppConfigurationAppService();
            var defaultCfg = cfgAppService.FindDefaultAppConfiguration();
            Assert.IsNotNull(defaultCfg);
            Assert.AreEqual("default", defaultCfg.AppId);
            Console.WriteLine(defaultCfg.JSONSerializationToString());
            defaultCfg = cfgAppService.FindDefaultAppConfiguration("secondary");
            Console.WriteLine(defaultCfg.JSONSerializationToString());
        }
        //发布消息，具体配置
        [TestMethod]
        public void Find_PublishMessage_AppCfgInfoDetails()
        {
            MQAppConfigurationAppService cfgAppService = new MQAppConfigurationAppService();
            var all = cfgAppService.FindPublishMessageDomainAppCfgInfoDetails(conntype: "secondary");
            Console.WriteLine(all.JSONSerializationToString());
            Assert.AreEqual(14, all.Count());
            var all2 = cfgAppService.FindPublishMessageDomainAppCfgInfoDetails("100.50");
            Assert.IsTrue(all2.Any());

            Console.WriteLine(all2.JSONSerializationToString());
        }
        //appdomain 配置
        [TestMethod]
        public void Find_AppDomian_Cfg()
        {
            MQAppDomainConfigurationAppService appDomainCfg = new MQAppDomainConfigurationAppService();
            var domain = appDomainCfg.FindAllAppDomainConfiguration();
            Assert.AreEqual(13, domain.Count());
            Console.WriteLine(domain.JSONSerializationToString());
            domain = appDomainCfg.FindAllAppDomainConfiguration();
            Assert.AreEqual(13, domain.Count());
            Console.WriteLine(domain.JSONSerializationToString());
        }
        [TestMethod]
        public void Cfg_Test() 
        {
            MQAppConfigurationAppService cfgRepo = new MQAppConfigurationAppService();
            var cfg1 = cfgRepo.FindPublishMessageDomainAppCfgInfoDetails(conntype: "MessageBusType".GetAppSettings("primary"));

            var cfgList = cfgRepo.FindAllAppCfgInfoDetails("MessageBusType".GetAppSettings("primary"));
            Assert.IsTrue(cfgList.Any());

            var cfg = MQMainConfigurationManager.Builder.GetConfiguration("busperformanctest", "performanctest1");
            Assert.IsTrue(cfg != null);
            Assert.AreEqual(true, cfg.MessagePropertiesCfg.PersistentMessagesMongo);
            Assert.AreEqual(true, cfg.ConsumeCfg.HandleSuccessSendNotice.Value);
        }
        [TestMethod]
        public void Parameters_Test() 
        {
            var r = A("MessageBusType2".GetAppSettings());
            Assert.IsNull(r);
            r = A();
            Assert.AreEqual("test", r);
        }
        private string A(string a = "test") 
        {
            return a;
        }

        [TestMethod]
        public void Test_DbNamesSuffix()
        {
          
            var startTime = Convert.ToDateTime("2016-05-01 00:00:00");
            var dbNames = startTime.ToString("yyyyMM");
            Assert.AreEqual(dbNames, "201605");
        }
    }
}
