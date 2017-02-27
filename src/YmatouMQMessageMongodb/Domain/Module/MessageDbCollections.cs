using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YmatouMQMessageMongodb.Domain.Module
{
    public class MessageDbCollections
    {
        private readonly string appId;
        private readonly string code;
        public MessageDbCollections(string appid, string code)
        {
            this.appId = appid;
            this.code = code;
        }

        public string GenerateDbName()
        {
            return string.Format("MQ_Message_{0}_{1}", this.appId, DateTime.Now.ToString("yyyyMM"));
        }

        public string GenerateCollectionsName()
        {
            return string.Format("Message_{0}", this.code);
        }

        public static string GenerateDbName(string appId)
        {
            return string.Format("MQ_Message_{0}_{1}", appId, DateTime.Now.ToString("yyyyMM"));
        }
        public static string GenerateCollectionsName(string code)
        {
            return string.Format("Message_{0}", code);
        }
    }
}
