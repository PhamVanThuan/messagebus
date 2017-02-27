using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using YmatouMQAdmin.WebApp2.Models;
using System.Xml.Linq;
using System.Web.Caching;
using Ymatou.CommonService;
using YmatouMQAdmin.WebApp2.App_Start;
namespace YmatouMQAdmin.WebApp2.Authorize
{
    public class CustomerAuthAttribute : AuthorizeAttribute
    {

        //private static List<UserInfo> userInfoList = new List<UserInfo>();

        //public static void InitUserInfo()
        //{
        //    XElement xe = XElement.Load(AppDomain.CurrentDomain.BaseDirectory +"Config\\Logging.xml");
        //    userInfoList = (from element in xe.Elements("user")
        //                    select new UserInfo
        //                    {
        //                        username = element.Element("username").Value,
        //                        password = element.Element("password").Value,
        //                        Roles = element.Element("role").Value
        //                    }).ToList();

        //   // HttpContext.Current.Cache.Insert("UserXml", userInfoList);
        //}

        public override void OnAuthorization(AuthorizationContext filterContext)
        {

            // userInfoList = (List<UserInfo>)HttpContext.Current.Cache["UserXml"];

            //InitUserInfo();

            // HttpCookie cookie = filterContext.HttpContext.Request.Cookies["UserInfo"];


            if (filterContext.HttpContext.Session["username"] != null && filterContext.HttpContext.Session["password"] != null)
            {

                string _username = filterContext.HttpContext.Session["username"].ToString();
                string _password = filterContext.HttpContext.Session["password"].ToString();



                if (!MQAdminUserManager.userInfoList.Exists(u => u.username == _username && u.password == _password))
                {
                    ApplicationLog.Debug("Session中没有对应的用户");

                    filterContext.Result = new RedirectResult("/logOn/Log");
                    return;
                }
            }
            else
            {
                ApplicationLog.Debug("没有Session");
                filterContext.Result = new RedirectResult("/logOn/Log");
                return;
            }


        }
    }
}