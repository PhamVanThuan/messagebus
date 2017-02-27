using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using YmatouMQNet4.Configuration;

namespace YmatouMQAdmin.WebApp2.Models
{
    public class MQConfigurationDto
    {
        /// <summary>
        /// 应用ID（唯一）
        /// </summary>
        [DataMember(Name = "appId")]
        public string AppId { get; set; }
        /// <summary>
        /// 配置版本
        /// </summary>
        [DataMember(Name = "version")]
        public int Version { get; set; }
        /// <summary>
        /// 链接属性配置
        /// </summary>
        [DataMember(Name = "host")]
        public string host { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public string port { get; set; }

        public string vhost { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string userName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string password { get; set; }
        /// <summary>
        /// appid归属的host
        /// </summary>
        public string owerhost { get; set; }
        /// <summary>
        ///  channel pool。格式 min-max
        /// </summary>
        public string channelPool { get; set; }
        /// <summary>
        ///  connection pool。格式 min-max
        /// </summary>
        public string connectionPool { get; set; }
        /// <summary>
        /// 消息配置
        /// </summary>
        [DataMember(Name = "msgCfgs")]
        public MessageConfiguration MessageCfgList { get; set; }
    }
}