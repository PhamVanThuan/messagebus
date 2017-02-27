using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace YmatouMQNet4.Configuration
{
    /// <summary>
    /// 链接属性配置（primary，secondary）
    /// </summary>
    [DataContract(Name = "connCfg2")]
    public class ConnectionPAndSConfigureation
    {
        public static readonly string connection_primary = "primary";
        public static readonly string connection_secondary = "secondary";
        /// <summary>
        /// 链接ID
        /// </summary>
        [IgnoreDataMember]
        public string ConnId { get; set; }
        /// <summary>
        /// 类型（primary,secondary）
        /// </summary>
        [IgnoreDataMember]
        public string ConnType { get; set; }
        /// <summary>
        /// 链接字符
        /// </summary>
        [DataMember(Name = "connStr2")]
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
