using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using YmatouMQ.Common.Extensions.Serialization;
using YmatouMQ.Common.Extensions._Task;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Dto;
using System.IO;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;

namespace YamtouMQ2_0Demo
{
    class Program
    {
        private static readonly string uri = "http://api.mq.ymatou.com:1234/message/publish"; //owin:bus/Message/publish/  //web api:message/publish
        //   private static readonly string uri = "http://api.mq.ymatou.com:2345/message/publish/"; //owin:bus/Message/publish/  //web api:message/publish
        static void Main(string[] args)
        {
            try
            {
                YmatouMessageBusClientNet4.MessageBusAgentBootStart.TryInitBusAgentService();

                PublishMessage();
                Console.Read();

                Console.WriteLine("发送消息个数：");
                var read = Console.ReadLine();
                var count = string.IsNullOrEmpty(read) ? 500000 : Convert.ToInt32(read);
                Console.WriteLine("发送 {0} 个消息", count);
                Console.WriteLine("发送模式，1：顺序发送，2：并行发送，3：异步发送");
                var p = Console.ReadLine();
                Console.WriteLine("输入app,code。逗号分割");
                var appcode = Console.ReadLine().Split(new char[] { ',' });

                var dto = new MessageDto
                {
                    AppId = appcode[0],
                    Code = appcode[1],
                    MsgUniqueId = Guid.NewGuid().ToString("N"),
                    Ip = "127.0.0.1",
                    Body = new { a = 10, b = "aaa", c = DateTime.Now, d = 1000000, e = new List<B> { { new B { _d = 12.0M } } } }
                };

                var _dtoDic = new Dictionary<string, object> 
                {
                    {"appid",dto.AppId},
                    {"code",dto.Code},
                    {"msguniqueid",dto.MsgUniqueId},
                    {"ip",dto.Ip},
                    {"body",dto.Body}
                };

                var by = _dtoDic.JSONSerializationToByte();
                var watch = Stopwatch.StartNew();
                if (p == "2")
                {
                    PostAsync3(by, count);
                }
                else if (p == "1")
                {
                    PostSync(by, count);
                }
                else if (p == "3")
                {
                    PostAsync(by, count, 1);
                }
                else
                {
                    Console.WriteLine("输入错误");
                }
                var total = watch.ElapsedMilliseconds;
                watch.Stop();
                var ops = count * 1000 / (total > 0 ? total : 1);
                var outStr = string.Format("发送完成，耗时 {0} 毫秒,每秒 发送 {1}个消息", total, ops);
                Console.WriteLine(outStr);
                Console.Read();
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            Console.Read();
        }
        private static void PublishMessage()
        {
            Task.Factory.StartNew(() =>
            {
                pub("apisocial", "feed");
            });
            Task.Factory.StartNew(() =>
            {
                pub("test2", "liguo");
            });


        }

        private static void pub(string appid, string code)
        {
            var now = DateTime.Now;

            while (DateTime.Now.Subtract(now).TotalMinutes < 10)
            {
                YmatouMessageBusClientNet4.MessageBusAgent.PublishSync(appid, code, Guid.NewGuid().ToString("N"), new { a = "100" });
                //Thread.Sleep(50);
            }
            Console.WriteLine("end...");
        }
        private static void PostAsync3(byte[] by, int count)
        {
            try
            {
                Parallel.For(0, count, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, async i =>
                {
                    var client = MQClient.httpClient();
                    var content = new System.Net.Http.ByteArrayContent(by);
                    await client.PostAsync(uri, content).ContinueWith(request => request.Result.EnsureSuccessStatusCode());
                });
            }
            catch (Exception ex)
            {
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log"), ex.ToString() + "\r\n", Encoding.GetEncoding("utf-8"));
                Console.WriteLine(ex.ToString());
                Console.Read();
            }
            //Console.WriteLine("创建 {0} 个 httpclient", MQClient.HttpCount);
            #region
            //using (var http = new HttpClient())
            //{
            //    http.BaseAddress = new Uri("http://192.168.1.247:2345/");
            //    http.MaxResponseContentBufferSize = 1000000;
            //    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //    Parallel.For(0, count, async i =>
            //     {
            //         var result = await http.PostAsJsonAsync("bus/Message", by);
            //         var response = await result.Content.ReadAsAsync<ResponseData<ResponseNull>>();
            //     });
            //}
            #endregion
        }
        private static void PostSync(byte[] by, int count)
        {
            try
            {
                var watch = Stopwatch.StartNew();


                for (var i = 0; i < count; i++)
                {
                    var webRequest = WebRequest.Create(uri);
                    // var webRequest = WebRequest.Create("http://sms.queuehandler.alpha.ymatou.com/api/handle/sendmessage/");
                    webRequest.Method = "POST";
                    webRequest.ContentType = "application/json; charset=UTF-8";
                    using (var stream = webRequest.GetRequestStream())
                    {
                        stream.Write(by, 0, by.Length);
                    }
                    using (var _stream = webRequest.GetResponse().GetResponseStream())
                    using (var _streamRead = new StreamReader(_stream))
                    {
                        Console.WriteLine(_streamRead.ReadToEnd());
                    }
                    if (i < 20)
                    {
                        Console.WriteLine("耗时 {0} 毫秒，大小 {1}", watch.ElapsedMilliseconds, by.Length);
                        watch.Restart();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private static void PostAsync(byte[] by, int count, int taskCount = 10)
        {
            Console.WriteLine(uri);
            var list = new List<Task>();
            for (var j = 0; j < taskCount; j++)
            {
                list.Add(Task.Run(async () =>
                {
                    for (var i = 0; i < count; i++)
                    {
                        var client = MQClient.httpClient();
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var content = new System.Net.Http.ByteArrayContent(by);
                        content.Headers.Add("Content-Type", "application/json;charset=utf-8");
                        await client.PostAsync(uri, content).ContinueWith(request => request.Result.EnsureSuccessStatusCode());
                        #region [基于webrequest]
                        // var webRequest = WebRequest.Create(uri + "/message/publish");// 原来的路径:  ...."/bus/Message/"
                        // webRequest.Method = "POST";
                        // webRequest.ContentType = "application/json; charset=UTF-8";

                        // webRequest.GetRequestStreamAsync().ContinueWith(r =>
                        // {
                        //     r.Result.Write(by, 0, by.Length);
                        // }).ContinueWith(_r =>
                        // {
                        //     webRequest.DownloadDataAsync(Encoding.GetEncoding("utf-8")).ContinueWith(r =>
                        //     {
                        //         //Console.WriteLine(r.Result);
                        //     });
                        // })
                        //.Wait();
                        #endregion
                    }

                }));
            }
            Task.WaitAll(list.ToArray());
        }
    }
    class MQClient
    {

        static System.Threading.ThreadLocal<HttpClient> pool = new System.Threading.ThreadLocal<HttpClient>();
        public static HttpClient httpClient()
        {
            try
            {

                Func<HttpClient> fn = () =>
                {
                    var http = new HttpClient();
                    //http.BaseAddress = new Uri(uri);//http://mqserver2.ymatou.com:2345/
                    http.MaxResponseContentBufferSize = 1000000;
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    return http;
                };
                var tmp = pool.Value;
                System.Threading.Interlocked.CompareExchange(ref tmp, fn(), null);
                if (tmp != null)
                    pool.Value = tmp;
            }
            catch (Exception ex)
            {
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log"), ex.ToString() + "\r\n", Encoding.GetEncoding("utf-8"));
                Console.WriteLine("error->" + ex.ToString());
                Console.Read();
            }
            return pool.Value;
        }

        public static int HttpCount { get { return pool.Values.Count; } }
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
