using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using log4net.Config;
using YmatouMQAdmin.WebApp2.App_Start;
using YmatouMQAdmin.WebApp2.ContextType;
using YmatouMQAdmin.WebApp2.Models;
using YmatouMessageBusClientNet4;

namespace YmatouMQAdmin.WebApp2
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        //public static List<UserInfo> userRole = new List<UserInfo>();

        //protected void Session_Start(object sender, EventArgs e)
        //{
        //    MQAdminUserManager log = new MQAdminUserManager();
        //    userRole = log.userInfoList;
        //}

        protected void Application_Start()
        {
            XmlConfigurator.Configure(new System.IO.FileInfo(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config//log4net.config")));

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GlobalConfiguration.Configuration.Filters.Add(new ExceptionHandlerAttribute());
            GlobalConfiguration.Configuration.EnsureInitialized();
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SupportedMediaTypes.Clear();
            GlobalConfiguration.Configuration.Formatters.Add(new ServiceStackTextFormatter());
            //GlobalConfiguration.Configuration.Formatters.JsonFormatter.MediaTypeMappings.Add(new QueryStringMapping("json", "true", "application/json"));  
            //JsonConfig.Register(GlobalConfiguration.Configuration);

            MQAdminUserManager.Init();
            MessageBusAgentBootStart.TryInitBusAgentService();
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            var err = Server.GetLastError();
            if (err != null)
            {
                Ymatou.CommonService.ApplicationLog.Error("Application_Error error ", err);
            }
        }
        protected void Application_End(object sender, EventArgs e) 
        {
            MessageBusAgentBootStart.TryStopBusAgentService();
        }
    }
}