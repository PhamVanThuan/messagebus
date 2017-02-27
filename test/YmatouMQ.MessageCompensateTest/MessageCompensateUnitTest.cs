using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;
using YmatouMQMessageMongodb.AppService;
using YmatouMQMessageMongodb.Domain.Module;
using YmatouMQ.MessageCompensateService;
using System.Collections.Generic;
using YmatouMQMessageMongodb.Domain.Specifications;
using YmatouMQMessageMongodb.Domain.Domain;
using YmatouMQMessageMongodb.Repository;

namespace YmatouMQ.MessageCompensateTest
{
    [TestClass]
    public class MessageCompensateUnitTest
    {
        [TestMethod]
        public void AlarmAppId() 
        {
            var id = AlarmAppService.FindAlarmAppId("test2_liguo_c3",null);
            Assert.AreEqual("a", id[0]);
            Assert.AreEqual("test", id[1]);
        }
        [TestMethod]
        public void Insert()
        {

            var appService = new RetryMessageCompensateAppService();
            for (var i = 0; i < 10; i++)
            {
                var msg = new RetryMessage("sms", "test", Guid.NewGuid().ToString("N"), "hell_" + i, DateTime.Now.Add(TimeSpan.FromMinutes(10)), new List<string> { "a" });
                appService.InsertCompensateMessage(msg);
            }
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void Update_AppKey()
        {
            Action a1 = () =>
            {
                var appService = new RetryMessageCompensateAppService();
                var result = appService.UpdateAppKey_Test("ad1");
                Console.WriteLine(JsonConvert.SerializeObject(result));
                Assert.IsTrue(true);
            };
            var t1 = Task.Factory.StartNew(a1);
            Action a2 = () =>
            {
                var appService = new RetryMessageCompensateAppService();
                var result = appService.UpdateAppKey_Test("ad2");
                Console.WriteLine(JsonConvert.SerializeObject(result));
                Assert.IsTrue(true);
            };
            var t2 = Task.Factory.StartNew(a2);
            Task.WaitAll(t1, t2);
            //Parallel.Invoke(a1, a2);
        }
        [TestMethod]
        public void Save_timertaskCfg()
        {
            MessageCompensateTaskService.SaveTimerTaskInfoCfg(TimerTaskInfo.Default);
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void Load_timertaskCfg()
        {
            var cfg = MessageCompensateTaskService.BuilderTimerTask().Result;
            Assert.AreEqual(3, cfg.Count());
        }
        [TestMethod]
        public void CheckMessageStatus() 
        {
           IRetryMessageRepository repo = new RetryMessageRepository();
           var query= RetryMessageSpecifications.Match_RetryTimeOut(DateTime.Now.Subtract(TimeSpan.FromHours(-2)));
           Console.WriteLine(query);
           Assert.IsTrue(true);
        }
    }
}
