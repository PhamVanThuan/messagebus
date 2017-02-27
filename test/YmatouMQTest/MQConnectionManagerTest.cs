using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Impl;
using YmatouMQ.Connection;
using YmatouMQNet4.Configuration;
using System.Diagnostics;
//using Xunit;

namespace YmatouMQTest
{
    [TestClass]
    public class MQConnectionManagerTest
    {
        [TestMethod]
        public void Conn_Parse()
        {
            var connVal = "host=x.x.x.x;port=xx;vHost=/;uNmae=guest;pas=guest;recoveryInterval=5;channelMax=100;useBackgroundThreads=true;connTimeOut=3000;pooMinSize=3;pooMaxSize=10";
            var connInfo = ConnectionInfo.Build(connVal);
            Assert.AreEqual("x.x.x.x", connInfo.Host);
            Assert.AreEqual(5672, connInfo.Port);
            Assert.AreEqual("guest", connInfo.UserNmae);
            Assert.AreEqual("guest", connInfo.Password);
            Assert.AreEqual("/", connInfo.VHost);
            Assert.AreEqual(null, connInfo.Heartbeat);
            Assert.AreEqual(null, connInfo.ChannelMax);
            Assert.AreEqual(true, connInfo.UseBackgroundThreads);
            Assert.AreEqual(3000, connInfo.ConnTimeOut);
            Assert.AreEqual(Convert.ToUInt32(10), connInfo.PoolMaxSize.Value);
            Assert.AreEqual(Convert.ToUInt32(3), connInfo.PoolMinSize.Value);
        }
        [TestMethod]
        public void Pool_Index()
        {
            var tmp = new int[] { 1, 2, 3, 4, 5, 5 };
            var index = 6;
            var index2 = 10001;
            var index3 = 700003;

            Console.WriteLine(index % tmp.Length);
            Console.WriteLine(index2 % tmp.Length);
            Console.WriteLine(index3 % tmp.Length);
        }
        [TestMethod]
        public void Pool_init()
        {
            var connVal = "host=172.16.100.48;port=5672;vHost=/;uNmae=guest;pas=guest;recoveryInterval=5;channelMax=3;useBackgroundThreads=true;connTimeOut=3000;poolMinSize=3;pooMaxSize=10";
            var poolManager = new MQConnectionPoolManager();
            poolManager.InitPool("test", connVal, null);

            using (var channel = poolManager.CreateChannel("test"))
            {
                //TODO:
                Assert.AreEqual(true, channel.IsOpen);
            }
        }
        [TestMethod]
        //[ExpectedException(typeof(NullReferenceException), AllowDerivedTypes = true)]        
        public void Pool_conn_channelMax_limit()
        {
            var connVal = "host=172.16.100.48;port=5672;vHost=/;uNmae=guest;pas=guest;heartbeat=5000;recoveryInterval=5;channelmax=100;useBackgroundThreads=true;poolminsize=1;poolmaxsize=10";
            var poolManager = new MQConnectionPoolManager();
            poolManager.InitPool("test", connVal, null);
            for (var i = 0; i < 20; i++)
            {
                var channel = poolManager.CreateChannel("test");
                {
                    //重要：使用 using(var channel = poolManager.CreateChannel("test")) channel max 不会生效
                    //TODO:                                   
                    Assert.AreEqual(true, channel.IsOpen);
                }
            }
            var size = poolManager.GetConnPoolSize("test");
            Assert.AreEqual(Convert.ToUInt32(1), size);
        }
        [TestMethod]
        public void Pool_Channel_Performance()
        {
            var connVal = "host=172.16.100.48;port=5672;vHost=/;uNmae=guest;pas=guest;recoveryInterval=5;useBackgroundThreads=true;connTimeOut=3000;poolMinSize=10;pooMaxSize=20";
            var poolManager = new MQConnectionPoolManager();
            poolManager.InitPool("test", connVal, null);
            //测试1分钟
            var count = 1000;
            var stopWatch = Stopwatch.StartNew();
            for (var i = 0; i < count; i++)
            {
                var channel = poolManager.DirectChannel("test");
            }
            stopWatch.Stop();
            var ops = count * 1000 / stopWatch.ElapsedMilliseconds;
            Console.WriteLine("创建 {3} 个channel总耗时 {0} 毫秒,创建单个channel耗时 {1} 毫秒，平均每秒创建 {2} 个", stopWatch.ElapsedMilliseconds
                , stopWatch.ElapsedMilliseconds / (count * 1.0), ops,count);
        }
        [TestMethod]
        public void Sequence_Item_Compare()
        {
            var a = new int[] { 12, 3, 4, 5 };
            var b = new int[] { 8, 9, 4, 12, 56 };

            var result = b.Any(_b => a.Any(_a => _b > _a));
            Assert.IsTrue(result);
        }
    }
}
