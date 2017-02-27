using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQ.Common.Utils;

namespace YmatouMQMessageMongodb.Domain.Module
{
    public class BusPushHealth
    {
        public static readonly string ConnStatus_Ok = "ok";
        public static readonly string ConnStatus_shutdown = "shutdown";

        public string HealthId { get; private set; }
        public DateTime LastUpdateTime { get; private set; }
        public string Status { get; private set; }
        public string Ip { get; private set; }

        public BusPushHealth(string id, DateTime lastUpdateTime,string status)
        {
            this.HealthId = id;
            this.LastUpdateTime = lastUpdateTime;
            this.Status = status;
            this.Ip = _Utils.GetLocalHostIp();
        }

        public void SetLastUpdateTime(DateTime dt,string status)
        {
            this.LastUpdateTime = dt;
            this.Status = status;
            this.Ip = _Utils.GetLocalHostIp();
        }

        public bool CheckHealthIsTimeOut(int time)
        {
            if (Status == ConnStatus_shutdown) return true;
            return DateTime.Now.Subtract(this.LastUpdateTime.ToLocalTime()).TotalSeconds <= time;
        }

        protected BusPushHealth()
        {
        }
    }
}
