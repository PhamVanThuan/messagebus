using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YmatouMQAdmin.Domain.Module
{
    public class MQCfg<T>
    {
        public DateTime CreateTime { get; set; }
        public DateTime UpDateTime { get; set; }
        public T Value { get; set; }
    }
}
