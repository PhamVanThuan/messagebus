using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using YmatouMQ.Common.Extensions._Task;
namespace YmatouMQ.MessageCompensateService
{
    public class TimerTaskInfo
    {
        /// <summary>
        /// timer task
        /// </summary>
        public Timer timer { get; private set; }
        /// <summary>
        /// 执行周期
        /// </summary>
        public TimeSpan cycletime { get; private set; }
        /// <summary>
        /// 拉取数据大小
        /// </summary>
        public int size { get; private set; }
        /// <summary>
        /// timer task ID
        /// </summary>
        public string id { get; private set; }
        /// <summary>
        /// 拉取数据时间范围 （秒）
        /// </summary>
        public TimeSpan scan { get; private set; }

        public static readonly IEnumerable<TimerTaskInfo> Default = new List<TimerTaskInfo>
        {
                {new TimerTaskInfo(TimeSpan.FromSeconds(5),20,"t5s",TimeSpan.FromSeconds (180))},
                 {new TimerTaskInfo(TimeSpan.FromSeconds(10),40,"t10s",TimeSpan.FromMinutes (5))},
                   {new TimerTaskInfo(TimeSpan.FromSeconds(30),100,"t30s",TimeSpan.FromMinutes (20))},
                    {new TimerTaskInfo(TimeSpan.FromMinutes(1),100,"t1m",TimeSpan.FromMinutes (30))},
                     {new TimerTaskInfo(TimeSpan.FromMinutes(5),200,"t5m",TimeSpan.FromHours (1))},
                      {new TimerTaskInfo(TimeSpan.FromMinutes(10),300,"t10m",TimeSpan.FromHours (2))},            
                        {new TimerTaskInfo(TimeSpan.FromMinutes(30),500,"t30m",TimeSpan.FromHours (3))},
                         {new TimerTaskInfo(TimeSpan.FromHours(1),1000,"t1h",TimeSpan.FromHours (12))},
                          {new TimerTaskInfo(TimeSpan.FromHours(2),2000,"t2h",TimeSpan.FromHours (24))},
                          {new TimerTaskInfo(TimeSpan.FromMinutes(3),2000,"t_check",TimeSpan.FromSeconds (180))},
        };
        public TimerTaskInfo(TimeSpan cycletime, int size, string id, TimeSpan scan)
        {
            this.cycletime = cycletime;
            this.size = size;
            this.id = id;
            this.scan = scan;
        }
        public string CreateTimerKey(string id)
        {
            var idKey = ConfigurationManager.AppSettings["Compenssate_Key"];
            return string.Format("{0}_{1}", id, idKey);
        }
        public Task SetTimer(Timer timer)
        {
           Action a=()=> this.timer = timer;
           return a.ExecuteSynchronously();
        }
    }
}
