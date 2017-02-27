#define Test
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;
//using System.Web.Http.SelfHost;
//using LimitsMiddleware;
using Microsoft.Owin.Builder;
using Microsoft.Owin.Hosting;
using Owin;
using YmatouMQNet4.Core;
using YmatouMQServerConsoleApp.owin;
using YmatouMQNet4;
using Microsoft.Owin.BuilderProperties;
using Microsoft.Owin.Diagnostics;
using System.IO;
using YmatouMQ.Common;
using YmatouMQ.Log;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Extensions._Task;
using System.Configuration;

namespace YmatouMQServerConsoleApp
{
    public class MQServerBootStart_Owin
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            ServicePointManager.DefaultConnectionLimit = Int16.MaxValue;
            var configuration = new HttpConfiguration();
            configuration.Routes.MapHttpRoute(
               name: "DefaultApi",
               routeTemplate: "api/{controller}/{id}",
               defaults: new { id = RouteParameter.Optional }
             );
            configuration.MapHttpAttributeRoutes();
            configuration.EnsureInitialized();
            configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            configuration.Filters.Add(new ExceptionHandlerAttribute());
            appBuilder.UseWebApi(configuration);
            #region
            //appBuilder.MaxBandwidthGlobal(0);
            //appBuilder.MaxConcurrentRequests(10000);
            //appBuilder.MaxBandwidthPerRequest(0);
            //appBuilder.Use(typeof(AutoTuneMiddleware), appBuilder.Properties["Microsoft.Owin.Host.HttpListener.OwinHttpListener"]);
            //appBuilder.Use<RequestMonitorMiddleware>();
            #endregion
            appBuilder.Use<MessageRequestHandler>();
            appBuilder.Run((owinContext) =>
            {
                owinContext.Response.ContentType = "text/plain;charset=utf-8";
                return owinContext.Response.WriteAsync("welcome ymatou mq server..");
            });
            //注册当owin 框架资源卸载时执行的操作
            var properties = new AppProperties(appBuilder.Properties);
            var token = properties.OnAppDisposing;
            if (token != CancellationToken.None)
            {
                token.Register(() =>
                {
                    //todo:
                });
            }
            appBuilder.UseErrorPage(new ErrorPageOptions { ShowExceptionDetails = true });

        }
    }
    public class MQHost
    {
        private static readonly ILog logger = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQServerConsoleApp.MQHost");
        private static IDisposable _server = null;
        public static void Start()
        {
            try
            {
                var hostArray = ConfigurationManager.AppSettings["mqapihost"] ?? "http://api.mq.ymatou.com:2345/";
                var hostItem = hostArray.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
                //启动服务
                var startOp = new StartOptions();
                hostItem.EachAction(h => startOp.Urls.Add(h));
                _server = WebApp.Start<MQServerBootStart_Owin>(startOp);
                //邦定log4net 日志服务
                log4net.Config.XmlConfigurator.Configure(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "log4net.config")));
                logger.Debug("bind host {0}", hostArray);
                //异步启动BUS
                var task = Task.Factory.StartNew(() => MessageBus.StartBusApplication()).WithHandleException(logger, null, "{0}", "MessageBusMainError");
                int workth;
                int portth;
                //获取可用的线程
                ThreadPool.GetAvailableThreads(out  workth, out portth);
                logger.Info("work thread {0},async io thread {1}", workth, portth);
                logger.Info("DefaultConnectionLimit {0}", ServicePointManager.DefaultConnectionLimit);
                logger.Info("Press Enter to quit.");
#if ConsoleAPP
                Console.Read();
#endif

            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        public static void Stop()
        {
            MessageBus.StopBusApplication();
            if (_server != null)
            {
                _server.Dispose();
            }
            logger.Info("stop ok..");
        }
    }
}
