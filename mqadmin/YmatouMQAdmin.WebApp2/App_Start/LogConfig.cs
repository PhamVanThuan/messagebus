using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Ymatou.CommonService;
using YmatouMQAdmin.WebApp2.Models;

namespace YmatouMQAdmin.WebApp2.App_Start
{
    public class MQAdminUserManager
    {
        private static List<UserInfo> _userInfoList = new List<UserInfo>();
        private static readonly string filePath = AppDomain.CurrentDomain.BaseDirectory + "Config\\Logging.xml";

        public MQAdminUserManager()
        {
          
        }

        public static void Init()
        {
            XElement xe = XElement.Load(filePath);
           
            _userInfoList = (from element in xe.Elements("user")
                             select new UserInfo
                             {
                                 username = element.Element("username").Value,
                                 password = element.Element("password").Value,
                                 Roles = element.Element("role").Value
                             }).ToList();

            ApplicationLog.Debug("load userinf file ok");
        }
        public static void ReInit() 
        {
            Init();
        }
        public static List<UserInfo> userInfoList
        {
            get { return _userInfoList; }
        }

        public static string RoleMap
        {
            get
            {
                var _role = ConfigurationManager.AppSettings["RoleMap"];
                if (_role != null)
                {
                    return _role;
                }
                else
                {
                    return "AppCodeList:应用业务列表|Index:编辑默认配置|AppDefault:编辑具体配置|CallBackIndex:回调业务配置|CreateApp:增加应用配置|AppdomainList:队列状态管理|CreateCode:增加业务配置|MessageStatusSearch:发送处理消息日志|UserList:管理员权限列表|TestMessage:测试收发消息|RetryMessage:补单日志查询|AppGlobal:编辑消息JSON";
                }
            }
        }
    }
}