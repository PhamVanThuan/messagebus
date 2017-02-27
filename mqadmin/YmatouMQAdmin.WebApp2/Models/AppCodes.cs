using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YmatouMQAdmin.WebApp2.Models
{
    public class AppCodes
    {
        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 业务名称列表
        /// </summary>
        public List<string> Codes { get; set; }
    }
}