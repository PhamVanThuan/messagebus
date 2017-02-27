using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YmatouMQNet4.Configuration
{
    public class AppdomainConfiguration
    {
        /// <summary>
        /// domain  成员
        /// </summary>
       public IEnumerable<DomainItem> Items { get; set; }
        /// <summary>
        /// appdomain 友好名称
        /// </summary>
        public string DomainName { get; set; }
        /// <summary>
        /// 应用Id 
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// 业务ＩＤ
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// domain 操作
        /// </summary>
        public DomainAction Status { get; set; }
        /// <summary>
        /// 配置版本
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        ///  归属主机
        /// </summary>
        public string Host { get; set; }
      
       
        public static readonly AppdomainConfiguration DefaultAppdomainCfg = new AppdomainConfiguration
        {
            DomainName = "ad_test2_liguo",
            Items = new DomainItem[] { new DomainItem { AppId = "test2", Code = "gaoxu", _Status = DomainAction.Normal }, new DomainItem { AppId = "test2", Code = "gaoxu2", _Status = DomainAction.Normal, ConnectionPoolSize = 3 } },         
            Status = DomainAction.Normal,
            Version = 0,
            Host = "localhost",
           
        };

    }
    public class DomainItem
    {
        public string AppId { get; set; }
        public string Code { get; set; }
        public DomainAction _Status { get; set; }
        public uint ConnectionPoolSize { get; set; }

    }
    public enum DomainAction : byte
    {
        Normal = 0,
        //Create = 1,
        Remove = 2,
    }
}
