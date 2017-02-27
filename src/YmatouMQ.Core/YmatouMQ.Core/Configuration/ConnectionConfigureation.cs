using System;
using System.Runtime.Serialization;

namespace YmatouMQNet4.Configuration
{
    /// <summary>
    /// 链接属性配置
    /// </summary>
    [DataContract(Name = "connCfg")]
    public class ConnectionConfigureation
    {
        /// <summary>
        /// 链接字符串。
        /// 格式：host=x.x.x.x;port:xx;vHost=/;uNmae=guest;pas=guest;heartbeat=xxs;recoveryInterval=5s;channelMax=100;useBackgroundThreads=true;connTimeOut=3000
        /// </summary>
        [DataMember(Name = "connStr")]
        public string ConnectionString { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj as ConnectionConfigureation == null) return false;
            //只比较链接字符窜
            return ConnectionString == (obj as ConnectionConfigureation).ConnectionString;
        }
     
        public override int GetHashCode()
        {
            return ConnectionString.GetHashCode();
        }
        public override string ToString()
        {
            return ConnectionString;
        }
    }
}
