using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQ.Common.Dto;
using YmatouMQ.Common.Extensions.Serialization;
using Newtonsoft.Json;
using System.Diagnostics;

namespace YmatouMQTest
{
    [TestClass]
    public class MessageSerializationTest
    {
        [TestMethod]
        public void Json_Serialization_string()
        {
            new S { appid = Guid.NewGuid().ToString("N"), code = "sss" }.JSONSerializationToString();
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void Json_Serialization_string_Parallel()
        {
            Parallel.For(0, 10000, i =>
            {
                new S { appid = Guid.NewGuid().ToString("N"), code = "sss" + i }.JSONSerializationToString();
                Assert.IsTrue(true);
            });
        }
        [TestMethod]
        public void Json_Serialization_string_async_Parallel()
        {
            Parallel.For(0, 10000, async i =>
            {
                await new S { appid = Guid.NewGuid().ToString("N"), code = "sss" + i }.JSONSerializationToStringAsync().ContinueWith(r =>
                  {
                      if (r.IsFaulted)
                      {
                          Console.WriteLine(r.Exception.ToString());
                      }
                      Assert.IsFalse(r.IsFaulted);
                  });

                Assert.IsTrue(true);
            });
        }
        [TestMethod]
        public void Json_Serialization_byte_Parallel()
        {
            Parallel.For(0, 10000, async i =>
            {
                await new S { appid = Guid.NewGuid().ToString("N"), code = "sss" + i }.JSONSerializationToByteAsync().ContinueWith(r =>
                {
                    if (r.IsFaulted)
                    {
                        Console.WriteLine(r.Exception.ToString());
                    }
                    Assert.IsFalse(r.IsFaulted);
                });

                Assert.IsTrue(true);
            });
        }
        [TestMethod]
        public void Serialization_ToString()
        {
            var dto = new MessageDto
            {
                AppId = "aa",
                Code = "bb",
                MsgUniqueId = Guid.NewGuid().ToString("N"),
                Ip = "127.0.0.1",
                Body = new A { a = 10, b = "aaa", c = DateTime.Now, d = new List<B> { { new B { _d = 12.0M } } } }
            };
            var stopwatch = Stopwatch.StartNew();
            var str = dto.JSONSerializationToString();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.WriteLine(str);
        }
        [TestMethod]
        public void JSONNET_Serialization_ToString()
        {
            var stopwatch = Stopwatch.StartNew();
            JsonConvert.DefaultSettings = () =>
           {
               return new JsonSerializerSettings
               {
                   DefaultValueHandling = DefaultValueHandling.Ignore,
                   NullValueHandling = NullValueHandling.Ignore,
               };
           };
            var json = JsonConvert.SerializeObject(new A { a = 10, b = "aaa", c = DateTime.Now, d = new List<B> { { new B { _d = 12.0M } } } });
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.WriteLine("jsonnet:" + json);
            Console.WriteLine("ServiceStack" + ServiceStack.Text.JsonSerializer.SerializeToString(new A { a = 10, b = "aaa", c = DateTime.Now, d = new List<B> { { new B { _d = 12.0M } } } }));


        }
        [TestMethod]
        public void JSONNET_Serialization_Stream()
        {
            var str = "'ssssssssssssssss'";
            var by = Encoding.GetEncoding("utf-8").GetBytes(str);
            using (var stream = new System.IO.MemoryStream(by))
            {
                var json = stream._JSONDeserializeFromStream<string>();
                Assert.AreEqual("ssssssssssssssss", json);
            }
        }
        [TestMethod]
        public void JSONNET_Serialization_ServiceStack_Stream()
        {
            var str = "ssssssssssssssss";
            var by = Encoding.GetEncoding("utf-8").GetBytes(str);
            using (var stream = new System.IO.MemoryStream(by))
            {
                var json = stream.JSONDeserializeFromStream<string>();
                Assert.AreEqual("ssssssssssssssss", json);
            }
        }
    }

    class S
    {
        public string appid { get; set; }
        public string code { get; set; }
    }

    class A
    {
        public int a { get; set; }
        public string b { get; set; }
        public DateTime c { get; set; }

        public List<B> d { get; set; }
    }
    class B
    {
        public decimal _d { get; set; }
    }
}
