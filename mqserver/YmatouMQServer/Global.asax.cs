using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using log4net.Config;
using Ymatou.CommonService;
using YmatouMQNet4.Core;
using YmatouMQServer.ContextType;

namespace YmatouMQServer
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            XmlConfigurator.Configure(new System.IO.FileInfo(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config//log4net.config")));

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GlobalConfiguration.Configuration.EnsureInitialized();
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            GlobalConfiguration.Configuration.Formatters.Add(new JsonFormatter());

            MessageBus.StartBusApplication();
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            ApplicationLog.Debug("YmatouMQServer 应用程序启动成功");

        }

        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            ApplicationLog.Error("task error" + e.Exception.ToString());
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            var err = Server.GetLastError();
            if (err != null)
            {
                ApplicationLog.Error("app error", err);
            }
        }
        protected void Application_End(
          object sender,
          EventArgs e
       )
        {
            MessageBus.StopBusApplication();
            ApplicationLog.Debug("YmatouMQServer 应用程序停止完成");
        }
    }
}