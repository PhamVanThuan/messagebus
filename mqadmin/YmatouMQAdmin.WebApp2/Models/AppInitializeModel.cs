using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace YmatouMQAdmin.WebApp2.Models
{
    public class AppInitializeModel
    {
        /// <summary>
        /// 应用ID（唯一）
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// 配置版本
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// 链接属性配置
        /// </summary>
        public string host { get; set; }

        /// <summary>
        /// 归属Host
        /// </summary>
        public string ownerHost { get; set; }

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
        /// 业务名称
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 回调URL
        /// </summary>
        public List<string> UrlList { get; set; }

        /// <summary>
        /// 是否使用默认设置
        /// </summary>
        public bool IsUseDefault { get; set; }
    }
}