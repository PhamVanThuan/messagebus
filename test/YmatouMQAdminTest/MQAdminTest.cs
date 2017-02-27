using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using YmatouMQAdmin.Domain.IRepository;
using YmatouMQAdmin.Repository;
using YmatouMQNet4.Configuration;
using MongoDB.Driver.Builders;
using YmatouMQ.Common.Dto;
using YmatouMQ.Common.Extensions.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using YmatouMQ.Common.MessageHandleContract;
using YmatouMQ.ConfigurationSync;

//查看消息
//http://192.168.1.247:999/mq/admin/findmessage/?AppId=test2&Code=siyou_test&message=siyou_test&Status=true
namespace YmatouMQAdminTest
{
    [TestClass]
    public class MQAdminTest
    {
        [TestMethod]
        public void Find_Default_Cfg()
        {
            IMQConfigurationRepository cfg = new MQConfigurationRepository();
            var defCfgInfo = cfg.FindOne(Query.Null, "MQ_Configuration_201505", "MQ_Default_Cfg");

            Assert.AreEqual("default", defCfgInfo.AppId);
            Assert.AreEqual(1, defCfgInfo.MessageCfgList.Count());
        }
        [TestMethod]
        public void Create_NewSystemCfg()
        {
            IMQSystemConfigurationRepository cfg = new MQSystemConfigurationRepository();
            cfg.Add(new MQSystemConfiguration(3000, 3, 5, false, false, 4, true, @"d:\log\mq\"));

            Assert.IsTrue(true);
        }
        /// <summary>
        /// 保存默认应用配置
        /// </summary>
        [TestMethod]
        public void Create_New_Default_MQCfg()
        {
            IMQConfigurationRepository cfg = new MQConfigurationRepository();

            cfg.Save(MQMainConfiguration.DefaultMQCfg);

            Assert.IsTrue(true);
        }
        [TestMethod]
        public void Save_App_MQCfg_Beta()
        {
            IMQConfigurationRepository cfg = new MQConfigurationRepository();
            var cfgInfo2 = new MQMainConfiguration
            {
                AppId = "smsproxy",
                Version = 37,
                ConnCfg = new ConnectionConfigureation
                {
                    ConnectionString = "host=10.10.16.154;port=5672;vHost=/;uNmae=guest;pas=guest;heartbeat=5000;recoveryInterval=12;useBackgroundThreads=true;poolMinSize=5;poolMaxSize=10"
                }
                ,
                MessageCfgList = new List<MessageConfiguration> 
                {
                    {new MessageConfiguration
                        {
                            Code="message",
                            Enable=true,
                            ConsumeCfg=new ConsumeConfiguration {IsAutoAcknowledge=false,MaxThreadCount=3,PrefetchCount=10,HandleSuccessSendNotice=true,RetryTimeOut=1},
                            ExchangeCfg=new ExchangeConfiguration{ExchangeName="smsmessage",Durable=true,_ExchangeType=ExchangeType.direct },
                            QueueCfg=new QueueConfiguration {QueueName="smsmessage",IsDurable=true},
                            CallbackCfgList=new List<CallbackConfiguration>
                            {                           
                                {new CallbackConfiguration{Url="http://sms.mqhandler.ymatou.com/api/handle/sendmessage/",CallbackKey="smsproxy_message_c1",IsRetry=1}}                           
                            }
                        }
                    },
                    {new MessageConfiguration
                        {
                            Code="mqbetatest",
                            Enable=true,
                            ConsumeCfg=new ConsumeConfiguration {IsAutoAcknowledge=false,MaxThreadCount=3,HandleSuccessSendNotice=true,RetryTimeOut=1},
                            ExchangeCfg=new ExchangeConfiguration{ExchangeName="mqbetatest",Durable=true,_ExchangeType=ExchangeType.direct },
                            QueueCfg=new QueueConfiguration {QueueName="mqbetatest",IsDurable=true},
                            CallbackCfgList=new List<CallbackConfiguration>
                            {                           
                                {new CallbackConfiguration{Url="http://mq.test.ymatou.com/OrderHandle/",CallbackKey="smsproxy_mqbetatest_c1",IsRetry=1}}                           
                            }
                        }
                    },                
                }
            };
            cfg.Save(cfgInfo2, "MQ_Configuration_201505", "MQ_App_Cfg");
        }
        [TestMethod]
        public void Save_App_MQCfg_Test()
        {
            IMQConfigurationRepository cfg = new MQConfigurationRepository();

            var cfgInfo = new MQMainConfiguration
            {
                AppId = "test2"
                ,
                Version = 71
                ,
                ConnCfg = new ConnectionConfigureation
                   {
                       ConnectionString = "host=172.16.100.48;port=5672;vHost=/;uNmae=guest;pas=guest;heartbeat=5000;recoveryInterval=5;channelMax=100;useBackgroundThreads=true;poolMinSize=10;poolMaxSize=20"
                   }
                ,
                MessageCfgList = new List<MessageConfiguration> 
                {
                    {   new MessageConfiguration
                        {
                            Code="B",
                            Enable=false,                            
                        }
                    }
                    ,{new MessageConfiguration
                    {
                        Code="gaoxu",
                        Enable=true,
                        ConsumeCfg=new ConsumeConfiguration {MaxThreadCount=3,PrefetchCount=500}
                        ,ExchangeCfg=new ExchangeConfiguration{ExchangeName="gaoxu"},
                        QueueCfg=new QueueConfiguration {QueueName="gaoxu"},
                         CallbackCfgList=new List<CallbackConfiguration>
                        {
                            {new CallbackConfiguration{Url="http://192.168.1.247:777/OrderHandle/",CallbackKey="test2_gaoxu_c1"}},
                            {new CallbackConfiguration{Url="http://192.168.1.247:777/OrderHandle/",CallbackKey="test2_gaoxu_c2",IsRetry=1}},
                            {new CallbackConfiguration{Url="http://192.168.1.247:777/OrderHandle_fail_1/",CallbackKey="test2_gaoxuo_c3"}}
                        }
                    }}
                    ,{new MessageConfiguration
                    {
                        Code="liguo",
                        Enable=true,
                        ConsumeCfg=new ConsumeConfiguration {IsAutoAcknowledge=false,MaxThreadCount=3,PrefetchCount=50,HandleSuccessSendNotice=true},
                        ExchangeCfg=new ExchangeConfiguration{ExchangeName="liguo"},
                        QueueCfg=new QueueConfiguration {QueueName="liguo"},
                        MessagePropertiesCfg=new MessagePropertiesConfiguration {PersistentMessagesMongo=true },
                        CallbackCfgList=new List<CallbackConfiguration>
                        {
                            {new CallbackConfiguration{Url="http://192.168.1.247:777/OrderHandle/",CallbackKey="test2_liguo_c1"}},
                            {new CallbackConfiguration{Url="http://192.168.1.247:777/OrderHandle/",CallbackKey="test2_liguo_c2",IsRetry=1}},
                            {new CallbackConfiguration{Url="http://192.168.1.247:777/OrderHandle_fail_1/",CallbackKey="test2_liguo_c3"}}
                        }
                    }}
                    ,{new MessageConfiguration
                    {
                        Code="siyou",
                        Enable=true,
                        ConsumeCfg=new ConsumeConfiguration {IsAutoAcknowledge=false,MaxThreadCount=10,PrefetchCount=10},
                        ExchangeCfg=new ExchangeConfiguration {ExchangeName="siyou",_ExchangeType=ExchangeType.direct},
                        QueueCfg=new QueueConfiguration {QueueName="siyou"},
                        MessagePropertiesCfg=new MessagePropertiesConfiguration {PersistentMessagesMongo=true },
                         CallbackCfgList=new List<CallbackConfiguration>
                        {
                            {new CallbackConfiguration{Url="http://192.168.1.243:8045/api/handle/sendmessage/",CallbackKey="test2_siyou_c1",IsRetry=1}},                            
                        }
                    }}
                     ,{new MessageConfiguration
                    {
                        Code="siyou_test",
                        Enable=true,
                        ConsumeCfg=new ConsumeConfiguration {IsAutoAcknowledge=false,MaxThreadCount=10,PrefetchCount=10,RetryTimeOut=10},
                        ExchangeCfg=new ExchangeConfiguration {ExchangeName="siyou_test",_ExchangeType=ExchangeType.topic},
                        QueueCfg=new QueueConfiguration {QueueName="siyou_test"},
                        MessagePropertiesCfg=new MessagePropertiesConfiguration {PersistentMessagesMongo=true },
                         CallbackCfgList=new List<CallbackConfiguration>
                        {
                            {new CallbackConfiguration{Url="http://172.16.100.34:7881/api/handle/sendmessage/",CallbackKey="test2_siyou_test_c1",IsRetry=1}},                            
                        }
                    }}
                    ,{new MessageConfiguration
                    {
                           Code="t1",
                        Enable=true,
                        ConsumeCfg=new ConsumeConfiguration {IsAutoAcknowledge=false,MaxThreadCount=2,PrefetchCount=10},
                        ExchangeCfg=new ExchangeConfiguration {ExchangeName="t1"},
                        QueueCfg=new QueueConfiguration {QueueName="t1"}
                    }}
                    ,{new MessageConfiguration
                    {
                           Code="t2",
                        Enable=true,
                        ConsumeCfg=new ConsumeConfiguration {IsAutoAcknowledge=false,MaxThreadCount=2,PrefetchCount=10},
                        ExchangeCfg=new ExchangeConfiguration {ExchangeName="t2"},
                        QueueCfg=new QueueConfiguration {QueueName="t2"}
                    }},                   
                }
            };

            cfg.Save(cfgInfo, "MQ_Configuration_201505", "MQ_App_Cfg");
            var cfgInfo2 = new MQMainConfiguration
            {
                AppId = "test",
                Version = 35,
                ConnCfg = new ConnectionConfigureation
                {
                    ConnectionString = "host=172.16.100.105;port=5672;vHost=/;uNmae=guest;pas=guest;heartbeat=5000;recoveryInterval=12;useBackgroundThreads=true;poolMinSize=5;poolMaxSize=10"
                }
                ,
                MessageCfgList = new List<MessageConfiguration> 
                {
                    {new MessageConfiguration
                    {
                       Code="A",
                        Enable=true,
                        ConsumeCfg=new ConsumeConfiguration {IsAutoAcknowledge=false,MaxThreadCount=3,PrefetchCount=50,UseMultipleThread=true},
                        ExchangeCfg=new ExchangeConfiguration{ExchangeName="test_a"},
                        QueueCfg=new QueueConfiguration {QueueName="test_a"}
                    }}
                    ,
                     {new MessageConfiguration
                    {
                        Code="B",
                        Enable=false
                    }}
                }
            };
            cfg.Save(cfgInfo2, "MQ_Configuration_201505", "MQ_App_Cfg");
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void Update_App_MQCfg()
        {
            IMQConfigurationRepository cfg = new MQConfigurationRepository();

            var cfgInfo = cfg.FindOne(Query<MQMainConfiguration>.EQ(c => c.AppId, "test"), "MQ_Configuration_201505", "MQ_App_Cfg", true);

            Assert.IsNotNull(cfgInfo);
            //增加一个业务类型B
            var q = Query.And(Query.EQ("_id", "test"));
            Console.WriteLine(q);
            var u = Update.Combine(
                                Update.Set("V", 27)
                              , Update.Set("cfgItems.1.code", "B")
                              , Update.Set("cfgItems.1.open", true)
                              , Update.Set("cfgItems.1.C.cAutoAck", false)
                              , Update.Set("cfgItems.1.C.cUrl", "http://mqdemo.ymatou.com/OrderHandle/")
                              , Update.Set("cfgItems.1.E.eName", "ymatou2")
                              , Update.Set("cfgItems.1.Q.qName", "ymatou2")
                              );
            Console.WriteLine(u);
            var result = cfg.Update(
                  q
                , u
                , new MongoUpdateOptions { Flags = UpdateFlags.Upsert }
                , "MQ_Configuration_201505", "MQ_App_Cfg");

            Assert.AreEqual(1, result.DocumentsAffected);

        }
        [TestMethod]
        public async Task Add_Message_to_Mongodb()
        {
            var body = new A
            {
                a = 1000,
                name = "ggg",
                dic = new Dictionary<string, string> { { "f", "c" }, { "d", "d" } }
            };

            var dto = new MessagePersistentDto
            {
                AppId = "test",
                Code = "A",
                Ip = "0.0.0.0",
                MsgUniqueId = Guid.NewGuid().ToString("N"),
                Body = body.JSONSerializationToString()
            };

            await HttpHelp.Post(dto.JSONSerializationToByte(), "mq/admin/m/MessagePersistentDto/");

            Assert.IsTrue(true);
        }
        [TestMethod]
        public void Add_Message_to_Mongodb_sync()
        {
            var body = new A
            {
                a = 1000,
                name = "ggg",
                dic = new Dictionary<string, string> { { "f", "c" }, { "d", "d" } }
            };

            var dto = new MessagePersistentDto
            {
                AppId = "test",
                Code = "A",
                Ip = "0.0.0.0",
                MsgUniqueId = Guid.NewGuid().ToString("N"),
                Body = body.JSONSerializationToString()
            };

            HttpHelp._Post(dto.JSONSerializationToByte(), "mq/admin/m/MessagePersistentDto/");

            Assert.IsTrue(true);
        }
        [TestMethod]
        public void Add_Message_status_to_mongodb_sync()
        {
            var by = new MQMessageStatusDto { AppId = "test", Code = "A", MsgUniqueId = "eb4f39f5842a4cdea288f5c6e88b7775", Status = Status.Normal };

            HttpHelp._Post(by.JSONSerializationToByte(), "mq/admin/m/MQMessageStatus/");
            Assert.IsTrue(true);

            by = new MQMessageStatusDto { AppId = "test", Code = "A", MsgUniqueId = "68c62c8ec6b64ce88b6239c595197735", Status = Status.Normal };

            HttpHelp._Post(by.JSONSerializationToByte(), "mq/admin/m/MQMessageStatus/");
            Assert.IsTrue(true);
        }
        [TestMethod]
        public async Task Send_MessageHandle_Notice()
        {
            //await _PersistentMessage.SendNotice("test", "A", "68c62c8ec6b64ce88b6239c595197735", YmatouMQNet4.Core.Status.HandleSuccess);
            //Assert.IsTrue(true);
            //await _PersistentMessage.SendNotice("test", "A", "eb4f39f5842a4cdea288f5c6e88b7775", YmatouMQNet4.Core.Status.HandleException);
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void Add_AppDomain_Cfg()
        {
            IMQAppdomainConfigurationRepository cfgRepo = new MQAppdomainConfigurationRepository();
            cfgRepo.Save(new AppdomainConfiguration
            {
                DomainName = "ad_test2",
                Version = 2,
                Status = DomainAction.Normal,
                Host = "",
                Items = new DomainItem[] 
                { 
                    new DomainItem { AppId = "test2", Code = "gaoxu", _Status = DomainAction.Normal,ConnectionPoolSize = 3 } 
                    ,new DomainItem { AppId = "test", Code = "A", _Status = DomainAction.Normal,ConnectionPoolSize = 3 } 
                    ,new DomainItem { AppId = "test2", Code = "liguo", _Status = DomainAction.Normal,ConnectionPoolSize = 3 } 
                    ,new DomainItem { AppId = "test2", Code = "t1", _Status = DomainAction.Normal,ConnectionPoolSize = 3 } 
                    ,new DomainItem { AppId = "test2", Code = "t2", _Status = DomainAction.Normal,ConnectionPoolSize = 3 } 
                    ,new DomainItem { AppId = "test2", Code = "siyou", _Status = DomainAction.Normal,ConnectionPoolSize = 3 } 
                    ,new DomainItem { AppId = "test2", Code = "siyou_test", _Status = DomainAction.Normal,ConnectionPoolSize = 3 } 
                }
            });
            cfgRepo.Save(new AppdomainConfiguration { DomainName = "ad_test", Version = 1, Status = DomainAction.Normal, Host = "", Items = new DomainItem[] { new DomainItem { AppId = "test2", Code = "gaoxu", _Status = DomainAction.Normal, ConnectionPoolSize = 3, } } });
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void Add_AppDomain_Cfg_beta()
        {
            IMQAppdomainConfigurationRepository cfgRepo = new MQAppdomainConfigurationRepository();
            cfgRepo.Save(new AppdomainConfiguration
            {
                DomainName = "ad_smsproxy",
                Version = 2,
                Status = DomainAction.Normal,
                Host = "",
                Items = new DomainItem[] 
                { 
                    new DomainItem { AppId = "smsproxy", Code = "message", _Status = DomainAction.Normal,ConnectionPoolSize = 3 } 
                   
                }
            });
            //cfgRepo.Save(new AppdomainConfiguration { DomainName = "ad_test", Version = 1, Status = DomainAction.Normal, Host = "", Items = new DomainItem[] { new DomainItem { AppId = "test2", Code = "gaoxu", _Status = DomainAction.Normal, ConnectionPoolSize = 3, } } });
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void Find_Appdomain_cfg()
        {
            AppdomainConfigurationManager.Builder.LoadAppdomainCfg();
        }
        //生成100个队列
        [TestMethod]
        public void Save_App_MQCfg_performanctest_Test()
        {
            IMQConfigurationRepository cfg = new MQConfigurationRepository();
            cfg.Remove(Query.EQ("_id", "busperformanctest"), "MQ_Configuration_201505"
                , "MQ_App_Cfg");

            var cfgInfo2 = new MQMainConfiguration
            {
                AppId = "busperformanctest",
                Version = 39,
                ConnCfg = new ConnectionConfigureation
                {
                    ConnectionString = "host=172.16.100.48;port=5672;vHost=/;uNmae=guest;pas=guest;heartbeat=5000;recoveryInterval=12;useBackgroundThreads=true;poolMinSize=5;poolMaxSize=10"
                }
            };
            //
            var mlist = new List<MessageConfiguration>();

            for (var i = 0; i < 100; i++)
            {
                mlist.Add(CreateMessageCfg("performanctest" + i
                    , 64
                    , 100
                    , "performanctest"
                    , "performanctest" + i
                    , "performanctest" + i));
            }
            cfgInfo2.MessageCfgList = mlist;
            Assert.AreEqual(100, cfgInfo2.MessageCfgList.Count());
            //
            cfg.Save(cfgInfo2, "MQ_Configuration_201505", "MQ_App_Cfg");
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void InserTMessageToMongo() 
        {
            var repo = new MQMessageRepository();
            repo.Add(new YmatouMQAdmin.Domain.Module.MQMessage("a", "c", "", Guid.NewGuid().ToString("N"), null, "ok"));
            Assert.IsTrue(true);
        }
        private static MessageConfiguration CreateMessageCfg(string code, uint maxThread
            , ushort qos, string exchangName, string queueName, string routeKey)
        {
            return new MessageConfiguration
            {
                Code = code,
                Enable = true,
                ConsumeCfg = new ConsumeConfiguration { MaxThreadCount = maxThread, PrefetchCount = qos, RoutingKey = routeKey },
                ExchangeCfg = new ExchangeConfiguration { ExchangeName = exchangName, Durable = true, _ExchangeType = ExchangeType.direct },
                QueueCfg = new QueueConfiguration { QueueName = queueName, IsDurable = true },
                PublishCfg = new PublishConfiguration { RouteKey = routeKey },
                CallbackCfgList = new List<CallbackConfiguration>
                            {                           
                                {
                                    new CallbackConfiguration
                                    {
                                        Url="http://172.16.100.47:866/api/Values"
                                        ,CallbackKey=code+"_c1"                                       
                                    } 
                                },
                                     new CallbackConfiguration
                                {
                                       Url="http://192.168.1.247:777/OrderHandle/"
                                       ,CallbackKey=code+"_c2"                                      
                               }                           
                            }
            };
        }
    }

    class A
    {
        public int a { get; set; }
        public string name { get; set; }
        public System.Collections.IDictionary dic { get; set; }
    }
}
