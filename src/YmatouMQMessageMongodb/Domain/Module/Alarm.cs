using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YmatouMQMessageMongodb.Domain.Module
{
    /// <summary>
    /// 总线回调业务预警
    /// </summary>
    public class Alarm
    {       
        public string CallbackId { get; set; }
        public string CallbackUrl { get; set; }
        public string AlarmAppId { get; set; }
        public string Description { get; set; }
        public Alarm(string callbackId,string callbackUrl, string alarmAppId, string desc)
        {           
            this.CallbackId = callbackId;
            this.AlarmAppId = alarmAppId;
            this.Description = desc;
            this.CallbackUrl = callbackUrl;
        }
        protected Alarm()
        {

        }
    }
}
