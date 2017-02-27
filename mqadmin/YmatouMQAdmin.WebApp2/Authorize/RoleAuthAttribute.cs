using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Ymatou.CommonService;
using YmatouMQAdmin.WebApp2.Models;
namespace YmatouMQAdmin.WebApp2.Authorize
{
    public class RoleAuthAttribute:AuthorizeAttribute
    {
        //private static List<UserInfo> userInfoList = new List<UserInfo>();

        //public static void InitUserInfo()
        //{
        //    XElement xe = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + "Config\\Logging.xml");
        //    userInfoList = (from element in xe.Elements("user")
        //                    select new UserInfo
        //                    {
        //                        username = element.Element("username").Value,
        //                        password = element.Element("password").Value,
        //                        Roles = element.Element("role").Value
        //                    }).ToList();

        //}

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            //InitUserInfo();

            string actioname = filterContext.ActionDescriptor.ActionName;

           // HttpCookie cookie = filterContext.HttpContext.Request.Cookies["UserInfo"];
            if (filterContext.HttpContext.Session["role"] != null)
            {
               // string name = cookie.Values["username"].ToString();
                string role = filterContext.HttpContext.Session["role"].ToString();
                if (!role.Contains(actioname))
                {
                    ApplicationLog.Debug("Session中无此菜单权限");
                    filterContext.Result = new RedirectResult("/LogOn/Log");
                    return;
                }
            }
            else
            {
                ApplicationLog.Debug("没有Session");
                filterContext.Result = new RedirectResult("/LogOn/Log");
            }
        }
    }
}