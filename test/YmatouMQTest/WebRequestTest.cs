using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using YmatouMQ.Common.Extensions._Task;
using YmatouMQ.Common.Extensions.Serialization;

namespace YmatouMQTest
{
    [TestClass]
    public class WebRequestTest
    {
        [TestMethod]
        public void Request()
        {
            var request = WebRequest.Create("http://www.sina.com.cn");
            request.Method = "GET";
           
            var by=request.DownloadDataAsync().Result;

            var result = by.JSONDeserializeFromByteArray<string>();

            Console.WriteLine(result);
        }
    }
}
