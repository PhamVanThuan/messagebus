using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YmatouMQNet4.Core;
using YmatouMQMessageMongodb.Domain.Module;
using YmatouMQ.Common.Extensions;
using System.Collections.Concurrent;
namespace YmatouMQTest
{
    [TestClass]
    public class MessageBusTest
    {
        [TestMethod]
        public void Publish_Msg_1()
        {
            MessageBus.Publish<string>("hell1", "test", "test", "a");
            MessageBus.Publish<string>("hell2", "test", "test", "b");
            MessageBus.Publish<string>("hell3", "test", "test", "c");
        }
        [TestMethod]
        public void Publish_BatchMessage()
        {
            MessageBus.Publish(new { a="as", b="b", c="c" }, "test", "test", "a");
        }
        [TestMethod]
        public void MessageGroup()
        {
            var list = new List<MQMessage>();
            list.Add(new MQMessage("A", "a1", "0.0.0.0", Guid.NewGuid().ToString(), null, ""));
            list.Add(new MQMessage("A", "a1", "0.0.0.0", Guid.NewGuid().ToString(), null, ""));
            list.Add(new MQMessage("A", "a3", "0.0.0.0", Guid.NewGuid().ToString(), null, ""));
            list.Add(new MQMessage("B", "b1", "0.0.0.0", Guid.NewGuid().ToString(), null, ""));
            list.Add(new MQMessage("C", "c1", "0.0.0.0", Guid.NewGuid().ToString(), null, ""));
            list.Add(new MQMessage("C", "c1", "0.0.0.0", Guid.NewGuid().ToString(), null, ""));
            list
               .GroupBy(e => e.AppId)
               .EachAction(e =>
               {
                   e.GroupBy(c => c.Code).EachAction(_c =>
                   {
                       Console.WriteLine(e.Key + "__" + _c.Key + "___" + _c.Select(__ => __).Count());
                   });
               });
        }
        [TestMethod]
        public void RetryMessageGroup()
        {
            var list = new List<RetryMessage>();

            list.Add(new RetryMessage("A1", "a1.0", Guid.NewGuid().ToString(), null, DateTime.Now, new List<string> { }));
            list.Add(new RetryMessage("A1", "a1.1", Guid.NewGuid().ToString(), null, DateTime.Now, new List<string> { }));
            list.Add(new RetryMessage("A2", "a2.0", Guid.NewGuid().ToString(), null, DateTime.Now, new List<string> { }));
            list.Add(new RetryMessage("A3", "a3.0", Guid.NewGuid().ToString(), null, DateTime.Now, new List<string> { }));
            list.Add(new RetryMessage("A1", "a1.0", Guid.NewGuid().ToString(), null, DateTime.Now, new List<string> { }));
            list.Add(new RetryMessage("A2", "a2.0", Guid.NewGuid().ToString(), null, DateTime.Now, new List<string> { }));
            list.Add(new RetryMessage("A3", "a3.0", Guid.NewGuid().ToString(), null, DateTime.Now, new List<string> { }));
            list.Add(new RetryMessage("A1", "a1.2", Guid.NewGuid().ToString(), null, DateTime.Now, new List<string> { }));

            var result = Adpter2(list);

            Assert.AreEqual(2, result.Where(c => c.Code == "a1.0").SelectMany(c => c.Message).Count());
            Assert.AreEqual(2, result.Where(c => c.Code == "a2.0").SelectMany(c => c.Message).Count());
            Assert.AreEqual(1, result.Where(c => c.Code == "a1.1").SelectMany(c => c.Message).Count());
            Assert.AreEqual(2, result.Where(c => c.Code == "a3.0").SelectMany(c => c.Message).Count());
            Assert.AreEqual(1, result.Where(c => c.Code == "a1.2").SelectMany(c => c.Message).Count());
        }
        [TestMethod]
        public void RetryMessageGroup2()
        {
            var list = new List<RetryMessage>();
            list.Add(new RetryMessage("A1", "a1.0", Guid.NewGuid().ToString(), null, DateTime.Now, new List<string> { }));
            list.Add(new RetryMessage("A1", "a1.1", Guid.NewGuid().ToString(), null, DateTime.Now, new List<string> { }));
            list.Add(new RetryMessage("A2", "a2.0", Guid.NewGuid().ToString(), null, DateTime.Now, new List<string> { }));
            list.Add(new RetryMessage("A3", "a3.0", Guid.NewGuid().ToString(), null, DateTime.Now, new List<string> { }));
            list.Add(new RetryMessage("A1", "a1.0", Guid.NewGuid().ToString(), null, DateTime.Now, new List<string> { }));
            list.Add(new RetryMessage("A2", "a2.0", Guid.NewGuid().ToString(), null, DateTime.Now, new List<string> { }));
            list.Add(new RetryMessage("A3", "a3.0", Guid.NewGuid().ToString(), null, DateTime.Now, new List<string> { }));
            list.Add(new RetryMessage("A1", "a1.2", Guid.NewGuid().ToString(), null, DateTime.Now, new List<string> { }));

            var result = Adpter(list);

            Assert.AreEqual(2, result.Where(c => c.Code == "a1.0").SelectMany(c => c.Message).Count());
            Assert.AreEqual(2, result.Where(c => c.Code == "a2.0").SelectMany(c => c.Message).Count());
            Assert.AreEqual(1, result.Where(c => c.Code == "a1.1").SelectMany(c => c.Message).Count());
            Assert.AreEqual(2, result.Where(c => c.Code == "a3.0").SelectMany(c => c.Message).Count());
            Assert.AreEqual(1, result.Where(c => c.Code == "a1.2").SelectMany(c => c.Message).Count());
        }

        [TestMethod]
        public void ConvertToByte()
        {
            Convert.ToByte(-1);
        }

        public IEnumerable<_RetryMessage> Adpter2(List<RetryMessage> messgaeList)
        {
            var list = new ConcurrentBag<_RetryMessage>();
            var codes = messgaeList.Select(e => e.Code).Distinct();
            //var codes = messgaeList.Select(e => e.Code).DistinctBy();
            codes.EachAction(c =>
            {
                var messge = messgaeList.AsParallel().Where(_c => _c.Code == c);
                var m = new _RetryMessage
                {
                    AppId = messge.First().AppId,
                    Code = c,
                    Message = messge.ToList()
                };
                list.Add(m);
            });
            return list;
        }

        public List<_RetryMessage> Adpter(List<RetryMessage> messgaeList)
        {
            var list = new List<_RetryMessage>();
            messgaeList.AsParallel().GroupBy(e => e.AppId)
               .EachAction(c =>
               {
                   var codeGroup = c.Select(_c => _c).GroupBy(_ => _.Code);
                   var _message = codeGroup.SelectMany(m => m);
                   var _codes = codeGroup.Select(_code => _code.Key);
                   _codes.EachAction(__ =>
                       {
                           var _list = new List<RetryMessage>();
                           _list.AddRange(_message.Where(__c => __c.Code == __));
                           list.Add(new _RetryMessage
                           {
                               AppId = c.Key,
                               Code = __,
                               Message = _list
                           });
                       });
               });
            return list;
        }
        public class _RetryMessage
        {
            public string AppId { get; set; }
            public string Code { get; set; }
            public List<RetryMessage> Message { get; set; }
        }
    }
}
