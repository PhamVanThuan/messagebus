using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft;
using System.IO;

namespace YmatouMQConsumeDemo.WebApi.Controllers
{
    public class ValuesController : ApiController
    {
        // POST api/values
        public string Post([FromBody]Order value)
        {
            //TODO:实现业务处理
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mq.log"), DateTime.Now + " " + Newtonsoft.Json.JsonConvert.SerializeObject(value) + "\r\n");
            return "ok";
        }

    }
    public class Order
    {
        public string Id { get; set; }
        public DateTime CreateTime { get; set; }
        public List<OrderItem> Items { get; set; }
    }
    public class OrderItem
    {
        public int A { get; set; }
    }
}