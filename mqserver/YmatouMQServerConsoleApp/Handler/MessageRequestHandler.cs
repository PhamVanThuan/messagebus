using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Host.HttpListener;
using YmatouMQ.Common.Extensions.Serialization;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Dto;
using YmatouMQNet4.Core;
using YmatouMQNet4;

namespace YmatouMQServerConsoleApp.owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using System.Diagnostics;
    using YmatouMQNet4.Configuration;
    using YmatouMQNet4.Utils;
    using System.Text;
    using log4net.Appender;
    using YmatouMQ.Common;
    using YmatouMQ.Log;
    using YmatouMQ.Common.Utils;
    using YmatouMQ.ConfigurationSync;
    using YmatouMQMessageMongodb.AppService;

    #region [...]
    //    using SendFileFunc =
    //        Func<string, // File Name and path
    //            long, // Initial file offset
    //            long?, // Byte count, null for remainder of file
    //            CancellationToken,
    //            Task>; // Complete
    //    using WebSocketAccept =
    //        Action<IDictionary<string, object>, // WebSocket Accept parameters
    //            Func<IDictionary<string, object>, // WebSocket environment
    //                Task>>; // Complete
    //    using WebSocketReceiveAsync =
    //        Func<ArraySegment<byte> /* data */,
    //            CancellationToken /* cancel */,
    //            Task<Tuple<int /* messageType */,
    //                bool /* endOfMessage */,
    //                int>>>; /* count */
    //    using WebSocketSendAsync =
    //        Func<ArraySegment<byte> /* data */,
    //            int /* messageType */,
    //            bool /* endOfMessage */,
    //            CancellationToken /* cancel */,
    //            Task>;
    #endregion
    public class MessageRequestHandler : MQOwinApiBase
    {
        private static readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQServerConsoleApp.owin.MessageRequestHandler");
        #region [...]
        //private readonly AppFunc _next;
        //private readonly Dictionary<string, Tuple<AppFunc, string>> _paths;
        //public MessageRequestHandler(AppFunc next)
        //{
        //    _next = next;

        //    _paths = new Dictionary<string, Tuple<AppFunc, string>>();
        //    _paths["/"] = new Tuple<AppFunc, string>(Index, null);

        //    var items = GetType().GetMethods()
        //        .Select(methodInfo => new
        //        {
        //            MethodInfo = methodInfo,
        //            Attribute = methodInfo.GetCustomAttributes(true).OfType<CanonicalRequestAttribute>().SingleOrDefault()
        //        })
        //        .Where(item => item.Attribute != null)
        //        .Select(item => new
        //        {
        //            App = (AppFunc)Delegate.CreateDelegate(typeof(AppFunc), this, item.MethodInfo),
        //            item.Attribute.Description,
        //            item.Attribute.Path,
        //        });

        //    foreach (var item in items)
        //    {
        //        _paths.Add(item.Path, Tuple.Create(item.App, item.Description));
        //    }
        //}
        //public Task Invoke(IDictionary<string, object> env)
        //{
        //    Tuple<AppFunc, string> handler;
        //    return _paths.TryGetValue(Util.RequestPath(env), out handler)
        //        ? handler.Item1(env)
        //        : _next(env);
        //}
        //public Task Index(IDictionary<string, object> env)
        //{
        //    Util.ResponseHeaders(env)["Content-Type"] = new[] { "text/html" };
        //    Stream output = Util.ResponseBody(env);
        //    using (var writer = new StreamWriter(output))
        //    {
        //        writer.Write("<ul>");
        //        foreach (var kv in _paths.Where(item => item.Value.Item2 != null))
        //        {
        //            writer.Write("<li><a href='");
        //            writer.Write(kv.Key);
        //            writer.Write("'>");
        //            writer.Write(kv.Key);
        //            writer.Write("</a> ");
        //            writer.Write(kv.Value.Item2);
        //            writer.Write("</li>");
        //        }

        //        writer.Write("<li><a href='/testpage'>/testpage</a> Test Page</li>");
        //        writer.Write("<li><a href='/Welcome'>/Welcome</a> Welcome Page</li>");

        //        writer.Write("</ul>");
        //    }
        //    return Task.FromResult<object>(null);
        //}
        #endregion
        public MessageRequestHandler(AppFunc next) : base(next) { }
        [CanonicalRequest(Path = "/bus/Message/pull/", Description = "pull message from rabbitmq")]
        public async Task PullMessage(IDictionary<string, object> env)
        {
            var context = new OwinContext(env);
            var appid = context.Request.Query["appid"];
            var code = context.Request.Query["code"];
            if (string.IsNullOrEmpty(appid) || string.IsNullOrEmpty(code))
            {
                env.OutPut(await ResponseData<ResponseNull>.CreateFailTask(ResponseNull._Null, lastErrorMessage: "appid且code不能为空"));
                return;
            }
            var message = MessageBus.PullMessage<string>(appid, code);
            env.OutPut(await ResponseData<string>.CreateTask(message, !message.IsEmpty(), message.IsEmpty() ? "no message" : ""));
            await Task.FromResult<object>(null);
        }
        [CanonicalRequest(Path = "/bus/owin/Message/publish", Description = "publish message to rabbitmq")]
        public async Task PublishMessage(IDictionary<string, object> env)
        {
            var watch = Stopwatch.StartNew();
            MessageDto dto = Util.Get<Stream>(env, "owin.RequestBody").ReadAsString()._JSONDeserializeFromString<MessageDto>();
            //Microsoft.Owin.Form#collection                                 
            if (dto == null)
            {
                env.OutPut(ResponseData<ResponseNull>.CreateFail(ResponseNull._Null, lastErrorMessage: "请求数据不能为空"));
                return;
            }
            #region
            await MessageBus.PublishBufferAsync(dto.Body, dto.AppId, dto.Code, dto.MsgUniqueId, dto.Ip).ConfigureAwait(false);
            //await MessageBus.PublishAsync(dto.Body, dto.AppId, dto.Code, dto.MsgUniqueId).ConfigureAwait(false);           
            #endregion
            env.OutPut(ResponseData<ResponseNull>.CreateSuccess(ResponseNull._Null, "ok"));
            watch.Stop();
            log.Debug("发布消息耗时 {0}", watch.ElapsedMilliseconds);
        }
        [CanonicalRequest(Path = "/bus/cfg/view", Description = "view message cache configuration")]
        public async Task BusCfgView(IDictionary<string, object> env)
        {
            var cfg = MQMainConfigurationManager.Builder.GetConfiguration();
            cfg["serverip"] = new MQMainConfiguration { AppId = _Utils.GetLocalHostIp() };
            await env.OutPutAsync(cfg);
        }
        [CanonicalRequest(Path = "/bus/busmemoryqueue/count/", Description = "show busmemoryqueue item count")]
        public async Task BusMemoryQueue(IDictionary<string, object> env)
        {
            var count = MessageAppService_TimerBatch.Instance.Count;
            var response = new { serverip = _Utils.GetLocalHostIp(), messagecount = count };
            await env.OutPutAsync(response);
        }
        [CanonicalRequest(Path = "/bus/conn/showkeys/", Description = "show conn keys")]
        public async Task GetConnectionPoolKeys(IDictionary<string, object> env)
        {
            var response = new { serverIP = _Utils.GetLocalHostIp(), poolKeys = MessageBus.GetConnectionPoolKeys };
            await env.OutPutAsync(response);
        }
         [CanonicalRequest(Path = "/bus/channel/status/", Description = "show channel status")]
        public async Task GetAllChannelStatus(IDictionary<string, object> env)
        {
            var response = new { serverIP = _Utils.GetLocalHostIp(), channelStatus = MessageBus.GetAllChannelStatus };
            await env.OutPutAsync(response);
        }
         [CanonicalRequest(Path = "/bus/status/", Description = "show bus app status")]
         public async Task GetBusStatus(IDictionary<string, object> env)
         {
             var response = new { serverIP = _Utils.GetLocalHostIp(), status = MessageBus.BusApplicationStatus };
             await env.OutPutAsync(response);
         }
        [CanonicalRequest(Path = "/bus/cfg/flush/", Description = "bus publish cache flush.(e.g:?appid=xx&username=xx&password=xx appid Optional；username,password Required)")]
        public async Task CfgCacheFlush(IDictionary<string, object> env)
        {
            var context = new OwinContext(env);
            var appid = context.Request.Query["appid"];
            var username = context.Request.Query["username"];
            var password = context.Request.Query["password"];
            var flushSync = context.Request.Query["sync"];
            if (username.IsEmpty() || password.IsEmpty())
            {
                await env.OutPutAsync("enter username password").ConfigureAwait(false);
                //env.OutPut("enter username passwor");
                return;
            }
            if (username != "busadmin" && password != "busadmin")
            {
                await env.OutPutAsync("username password error").ConfigureAwait(false);
                //env.OutPut("username password error");
                return;
            }
            try
            {
                var cfg = MQMainConfigurationManager.Builder.FlushCache(appid, flushSync == "1");
                cfg["serverip"] = new MQMainConfiguration { AppId = _Utils.GetLocalHostIp() };
                await env.OutPutAsync(cfg).ConfigureAwait(false);
                //env.OutPut(cfg);
                log.Debug("username {0},password {1} flush, configuration appid {2}", username, password, appid);
            }
            catch (AggregateException ex)
            {
                log.Error("flush cache exception.0", ex.ToString());
                env.OutPut(ex.ToString());
            }
            catch (Exception ex)
            {
                log.Error("flush cache exception.1", ex.ToString());
                env.OutPut(ex.ToString());
            }
        }
        [CanonicalRequest(Path = "/bus/removecacheexchang/", Description = "Remove Cache Exchang ?appid=xx&code=xx")]
        public async Task RemoveCacheExchang(IDictionary<string, object> env)
        {
            var context = new OwinContext(env);
            var appid = context.Request.Query["appid"];
            var code = context.Request.Query["code"];
            var remove = MessageBus.RemoveCacheExchang(appid, code);
            var response = new { serverIp = _Utils.GetLocalHostIp(), result = remove };
            await env.OutPutAsync(response);
        }
        [CanonicalRequest(Path = "/bus/publish/log/", Description = "display message publish log.（e.g:?type=error,exception|debug|info&top=100）")]
        public async Task PublishLog(IDictionary<string, object> env)
        {
            var context = new OwinContext(env);
            var logType = context.Request.Query["type"] ?? "debug";
            var top = context.Request.Query["top"] ?? "500";
            Util.ResponseHeaders(env)["Content-Type"] = new[] { "text/html;charset=utf-8" };
            Stream responseBody = Util.ResponseBody(env);

            var log4NetFile = ((log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository());
            var filePath = string.Empty;
            foreach (var item in log4NetFile.Root.Appenders)
            {
                if (item is FileAppender)
                {
                    var fileName = Path.GetFileName(((FileAppender)item).Name.ToLower());
                    if (fileName.Contains(logType))
                    {
                        filePath = ((FileAppender)item).File;

                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(filePath))
            {
                var by = Encoding.GetEncoding("utf-8").GetBytes("未找到日志文件路径 ");
                await responseBody.WriteAsync(by, 0, by.Length);
                return;
            }
            var serverInfo = Encoding.GetEncoding("utf-8").GetBytes(string.Format("server ip:{0}{1}", _Utils.GetLocalHostIp(), "</br>"));
            await responseBody.WriteAsync(serverInfo, 0, serverInfo.Length);

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Write, 4096, true))
            using (var streamReader = new StreamReader(fileStream, Encoding.GetEncoding("gb2312")))
            {

                fileStream.Seek(-(fileStream.Length - 4096), SeekOrigin.End);
                var i = 0;
                while (!streamReader.EndOfStream && i < Convert.ToInt32(top))
                {
                    var by = Encoding.GetEncoding("utf-8").GetBytes(string.Format("{0} {1}{2}", i, await streamReader.ReadLineAsync(), "</br>"));
                    await responseBody.WriteAsync(by, 0, by.Length);
                    i++;
                }
            }
        }

        public async Task LogAsync(IDictionary<string, object> env)
        {

        }

    }
}
