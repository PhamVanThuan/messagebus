using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YmatouMQ.Common.Extensions;

namespace YmatouMQTest
{
    [TestClass]
    public class OtherTest
    {
        [TestMethod]
        public void Scan_Time()
        {
            var time = "scan_time".GetAppSettings("00:05:00,10,60").Split(new char[] { ',' });
            //补单开始时间，当前时间向前推配置时间
            var startTime = DateTime.Now.Subtract(System.TimeSpan.Parse(time[0]));
            //补单结束时间，当前时间减去延迟更新推送状态时间
            var endTime = DateTime.Now.AddSeconds(-Convert.ToInt32(time[1]));
            //翻页数据大小
            var pageSize = Convert.ToInt32(time[2]);
            //补偿30s的数据
            var totalSeconds = Convert.ToInt32(endTime.Subtract(startTime).TotalSeconds);

            var pageCount = (totalSeconds / pageSize) + (totalSeconds % pageSize > 0 ? 1 : 0);
            Console.WriteLine("starttime {0}~ endtime {1},pagecount {2},now {3}", startTime, endTime, pageCount, DateTime.Now);
            for (var h = 0; h < pageCount; Interlocked.Increment(ref h))
            {
                var etime = startTime.AddSeconds(h * pageSize);
                var stime = etime.AddSeconds(-pageSize);
                if (h == pageCount - 1)
                    etime = endTime;
                Console.WriteLine("{0}~{1},now {2},index {3}", stime, etime, DateTime.Now, h);
            }
        }
        [TestMethod]
        public void Model()
        {
            Assert.AreEqual(0,1/3);
        }
         [TestMethod]
        public void TimeSpan()
        {
            var time = "scan_time".GetAppSettings("1:15:00:00,10,60").Split(new char[] { ',' });
            //补单开始时间，当前时间向前推配置时间
            Console.WriteLine(System.TimeSpan.Parse(time[0]));
            var startTime = DateTime.Now.Subtract(System.TimeSpan.Parse(time[0]));
            Console.WriteLine(startTime);
        }
    }
}
