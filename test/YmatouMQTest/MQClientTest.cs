using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMessageBusClientNet4;
using YmatouMessageBusClientNet4.Dto;
using YmatouMessageBusClientNet4.Persistent;
using System.IO;
using YmatouMessageBusClientNet4.Extensions;
using YmatouMQNet4.Utils;
using YmatouMQ.Common.Extensions;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using YmatouMQ.Common.Utils;
using System.Threading;
namespace YmatouMQTest
{
    [TestClass]
    public class MQClientTest
    {       
        [ClassInitialize]
        public static void Start(TestContext context) 
        {
            MessageBusAgentBootStart.TryInitBusAgentService();
            Assert.AreEqual(MessageBusAgentBootStart.Status, MessageBusAgentStatus.Runing);  
        }
        [ClassCleanup]
        public static void End()
        {         
            MessageBusAgentBootStart.TryStopBusAgentService();
            Assert.AreEqual(MessageBusAgentBootStart.Status, MessageBusAgentStatus.Stoped);
        }       
        [TestMethod]
        public void SendMessageDto()
        {
            MessageBusAgent.Publish(new PulbishMessageDto
            {
                appid = "test2",
                code = "siyou_test",
                messageid = "123",
                body = new { a = "add", c = new { c = 2, f = new { a = "a", c = new List<int> { 1, 2, 3, 4 }, f = new Dictionary<string, string> { { "a", "c" }, { "1", "2" } } } } }
            }, errorHandle: err =>
            {
                Console.WriteLine(err.ToString());
            });

            Assert.IsTrue(true);
        }
        [TestMethod]
        public void SendMessageAsync() 
        {
            MessageBusAgent.PublishAsync("test2", "liguo", Guid.NewGuid().ToString("N"), new { a = 1 });
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void SendMessage()
        {
            var r =new Random();
            for (var i = 0; i < 30000; i++)
            {
                MessageBusAgent.Publish("trading" //appid
                    , "trading_postpay" //code
                    , "mitest_A_"+DateTime.Now.ToString("yyyyMMddHHmm")+"_" + i //messageid
                    , new
                    {
                        //body
                        a = "add",
                        c = new {c = 2, f = new {c = new List<int> {1, 2, 3, 4}}}
                    }
                    );
                Thread.Sleep(r.Next(10, 500));
            }
            Assert.IsTrue(true);
        }
        [TestMethod]
        public async Task SendMessageAsyncTask()
        {
            await MessageBusAgent.PublishAsyncTask("test2", "liguo", "m1", new Guid().ToString());           
        }
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Send_NullMessage()
        {
            MessageBusAgent.Publish(null, errorHandle: err =>
            {
                Console.WriteLine(err.ToString());
            });
        }
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Send_Null_Arg_Message()
        {
            MessageBusAgent.PublishSync(null, null, null, null);
        }
        [TestMethod]
        public void MessageBusClient_PublishAsync()
        {
            MessageBusAgent.PublishAsync("test", "yyyy", Guid.NewGuid().ToString("N"), "a"); ;
        }
        [TestMethod]
        public void List_Except()
        {
            var a = new[] { 1, 2 };
            var b = new[] { 1, 2, 3, 4, 5 };

            var e = a.Except(b);
            var i = 3;
            e.ToList().ForEach(_e => Assert.AreEqual(i++, _e)); ;

        }
        [TestMethod]
        public void Load_Bus_Cfg_file()
        {
            MessageBusClientCfg.Instance.LoadCfg();
            Assert.AreEqual("http://api.mq.ymatou.com:2345", MessageBusClientCfg.Instance.Configruation<string>("e", "a", AppCfgInfo2.bushost_primary));
            Assert.AreEqual(string.Empty, MessageBusClientCfg.Instance.DefaultConfigruation<string>(AppCfgInfo2.storepath));
            Assert.AreEqual(string.Empty, MessageBusClientCfg.Instance.DefaultConfigruation<string>(AppCfgInfo2.storepath, AppCfgInfo2.default_Store_Path));
            Assert.AreEqual(3000, MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.retrytime));
            Assert.AreEqual(30000, MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.queuelimit));
            Assert.AreEqual(1, MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.queuelimitfileSize));
            Assert.AreEqual("/message/publish", MessageBusClientCfg.Instance.DefaultConfigruation<string>(AppCfgInfo2.batchMessageRequestPath));
            Assert.AreEqual("/message/publish", MessageBusClientCfg.Instance.Configruation<string>("a", "b", AppCfgInfo2.requestpath));
            Assert.AreEqual(500, MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.busHttpConnectionLimit));
            Assert.AreEqual(8, MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.batchMessageLimit));
            Assert.AreEqual(TimeSpan.FromMinutes(3), MessageBusClientCfg.Instance.DefaultConfigruation<TimeSpan>(AppCfgInfo2.retrytimeout));
            Assert.AreEqual(5000, MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.publishtimeout));
            Assert.AreEqual(false, MessageBusClientCfg.Instance.DefaultConfigruation<bool>(AppCfgInfo2.publishasync));
            Assert.AreEqual(5000, MessageBusClientCfg.Instance.DefaultConfigruation<int?>(AppCfgInfo2.publishtimeout));
            Assert.AreEqual(TimeSpan.FromMinutes(3), MessageBusClientCfg.Instance.DefaultConfigruation<TimeSpan?>(AppCfgInfo2.retrytimeout));
            Assert.AreEqual(true, MessageBusClientCfg.Instance.DefaultConfigruation<bool>(AppCfgInfo2.sendagentappidtoserver));
            Assert.AreEqual(true, MessageBusClientCfg.Instance.DefaultConfigruation<bool>(AppCfgInfo2.sendagentversiontoserver));
            Assert.AreEqual(true, MessageBusClientCfg.Instance.DefaultConfigruation<bool>(AppCfgInfo2.sendagentappidtoserver));
            Assert.AreEqual(true, MessageBusClientCfg.Instance.DefaultConfigruation<bool>(AppCfgInfo2.sendagentversiontoserver));
        }
        [TestMethod]
        public void Message_Journal()
        {
            Parallel.Invoke(new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            () => JournalFactory.MessageLocalJournalBuilder.Append2("okll1", "ok"),
            () => JournalFactory.MessageSendLogBuilder.Append2("okll2", "ok"),
            () => JournalFactory.MessageSendLogBuilder.Append2("okll3", "fail"),
            () => JournalFactory.MessageSendLogBuilder.Append2("okll4", "ok"),
            () => JournalFactory.MessageLocalJournalBuilder.Append2("okll5", "ok"));          
        }     
        [TestMethod]
        public void Json_test()
        {
            var body = "";//new { a = "add", c = new { c = 2, f = new { a = "a", c = new List<int> { 1, 2, 3, 4 }, f = new Dictionary<string, string> { { "a", "c" }, { "1", "2" } } } } };
            for (var i = 0; i < 5; i++)
            {
                var json = body.ToJson();
                for (var k = 0; k < 3; k++)
                {
                    var strJson = json.ToJson().ToJson();
                    Console.WriteLine(strJson);
                }
            }

            var json2 = "adcb";
            Console.WriteLine(json2.ToJson());
        }
        [TestMethod]
        public void ToLowerInvariant()
        {
            var str = "Ads".ToLowerInvariant();
            Assert.AreEqual("ads", str);
            Assert.AreEqual("ads", "Ads".ToLower());
        }
        [TestMethod]
        public void SafeToDictionary()
        {
            var list = new List<int> { 1, 2, 3, 4, 5, 5 };
            var dic = list.SafeToDictionary(i => i, e => e);

            Assert.AreEqual(5, dic.Count);
            Assert.AreEqual(1, dic[1]);
        }
        [TestMethod]
        public void Queue()
        {
            BlockingCollection<int> q = new BlockingCollection<int>(3);
            Assert.AreEqual(true, q.TryAdd(1));
            Assert.AreEqual(true, q.TryAdd(2));
            Assert.AreEqual(true, q.TryAdd(2));
            Assert.AreEqual(3, q.Count);
            var list = q.ToList();
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(3, q.Count);
            Assert.AreEqual(false, q.TryAdd(4));
        }
        [TestMethod]
        public void SkipIndex()
        {
            var list = new int[] { 1, 2, 3, 4, 5 };
            var val = list.Skip(4).Take(1);
            Assert.AreEqual(1, val.Count());
            Assert.AreEqual(5, val.First());
            val = list.Skip(5).Take(1);
            Assert.AreEqual(0, val.Count());
        }
        [TestMethod]
        public void LocalPath()
        {
            var url = new Uri("http://192.168.1.247:777/OrderHandle/");
            Console.WriteLine(url.LocalPath);
            url = new Uri("http://192.168.1.111:4151/index/feed/?version=v1&appkey=24243&signature=asjfsdsdfjks&timestamp=12334243&signatureversion=v1&signaturenonce=14234&action=post");
            Console.WriteLine(string.Join("_", url.Segments).Replace("/", "").Trim());
            url = new Uri("http://192.168.1.97:4130/index/social/?action=post");
            Console.WriteLine(url.LocalPath);
            url = new Uri("http://im.app.ymatou.com/api/Notice/ShoppingTipNotice");
            Console.WriteLine(url.LocalPath);
        }
        [TestMethod]
        public void CreateDirectory()
        {
            var directory = "d:\\Log.backup\\messagebus.journal\\messagebus.journal";
            var _directory = Path.GetDirectoryName(directory);
            if (!Directory.Exists(_directory))
            {
                var dicInfo = Directory.CreateDirectory(_directory);
            }
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void StringContains()
        {
            var str = "100.50,1.247";
            Assert.AreEqual(false, str.Contains("100.21"));
            Assert.AreEqual(true, str.Contains("1.247"));
        }
        [TestMethod]
        public void Journal_Test()
        {          
            for (var i = 0; i < 10000; i++)
            {
                JournalFactory.MessageLocalJournalBuilder.Append2("ddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd", ":ss");
                JournalFactory.MessageLocalJournalBuilder.Append2("ddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd", ":ss");
                JournalFactory.MessageLocalJournalBuilder.Append2("ddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd", ":ss");
                JournalFactory.MessageLocalJournalBuilder.Append2("ddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd", ":ss");
                JournalFactory.MessageLocalJournalBuilder.Append2("ddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd", ":ss");
            }           
        }

        [TestMethod]
        public void ListExcept()
        {
            var list = new List<string> { "ccfe63e77e9f400ca21cf3e35a95e6d6", "b07819c24d854a66a138faba997fb0ff" };
            var list2 = new List<string> { "8580de44ac534fc6a13f0b1c5b2f5cfe", "048c96e4bd094658ba542bc7397fdebe" };
            var except = list2.Except(list);
            Assert.AreEqual(2, except.Count());
            except.EachAction(m=>Console.WriteLine(m));
            except = list.Except(list2);
            Assert.AreEqual(2, except.Count());
            except.EachAction(m => Console.WriteLine(m));
        }

        [TestMethod]
        public void DnsIp()
        {
           var ips= Dns.GetHostAddresses("rmqnode1");
           Assert.AreEqual("172.16.100.48", ips.First(i => i.AddressFamily== AddressFamily.InterNetwork).ToString());
        }
    }
}
