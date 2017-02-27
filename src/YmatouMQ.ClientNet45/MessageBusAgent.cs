using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Ymatou.CommonService;
using YmatouMessageBusClientNet4;
using YmatouMessageBusClientNet4.Dto;

namespace YmatouMQ.ClientNet45
{
    /// <summary>
    ///  MessageBusAgent net 4.5
    /// </summary>
    public class MessageBusAgent
    {
        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="dto">声明发布消息dto</param>
        /// <param name="errorHandle">异常时回调</param>
        /// <param name="timeOut">发送消息超时时间（默认3秒）</param>
        [Obsolete("推荐使用void Publish(string appid, string code, string messageid, object body, int timeOut = 3000)")]
        public static void Publish(PulbishMessageDto dto, Action<Exception> errorHandle, int timeOut = 3000)
        {
            _Assert.AssertArgumentNotNull(dto, "dto 不能为空");

            Publish(dto.appid, dto.code, dto.messageid, dto.body);
        }
        /// <summary>
        /// 发布消息。
        /// <remarks>此方法根据配置 publishasync 是否启动异步发送消息。</remarks>
        /// </summary>
        /// <param name="appid">应用Id（必填）</param>
        /// <param name="code">业务类型（必填）</param>
        /// <param name="messageid">消息Id（必填）</param>
        /// <param name="body">消息正文（必填）</param>      
        public static void Publish(string appid, string code, string messageid, object body, int timeOut = 3000)
        {
            if (!ChecnkMessageBusAgentStatus(appid, code, body, messageid)) return;

            _Assert.AssertArgumentNotEmpty(appid, "appid 不能为空");
            _Assert.AssertArgumentNotEmpty(code, "业务类型不能为空");
            _Assert.AssertArgumentNotEmpty(messageid, "消息id不能为空");

            try
            {
                var useAsync = MessageBusClientCfg.Instance.Configruation<bool>(appid, code, AppCfgInfo2.publishasync);
                if (useAsync)
                    _PublishToPrimaryMessageBusAsync(appid, code, messageid, body);
                else
                    _PublishToPrimaryMessageBusSync(appid, code, messageid, body);
            }
            catch (Exception ex)
            {
                var logmessage = "发布消息异常 {0},{1},{2}".F(appid, code, body.ToJson());
                ApplicationLog.Error(logmessage, ex);
            }
        }
        /// <summary>
        /// 发送消息（同步）。
        /// <remarks>ok 发送消息成功，fail 发送消息失败</remarks>
        /// </summary>
        /// <param name="appid">应用Id（必填）</param>
        /// <param name="code">业务类型（必填）</param>
        /// <param name="messageid">消息Id（必填）</param>
        /// <param name="body">消息正文（必填）</param>
        /// <returns>ok 发送消息成功，fail 发送消息失败</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="WebException"></exception>
        [Obsolete("使用void PublishAsync(string appid, string code, string messageid, object body)")]
        public static string PublishSync(string appid, string code, string messageid, object body)
        {
            if (!ChecnkMessageBusAgentStatus(appid, code, body, messageid)) return "fail";

            _Assert.AssertArgumentNotEmpty(appid, "appid 不能为空");
            _Assert.AssertArgumentNotEmpty(code, "业务类型不能为空");
            _Assert.AssertArgumentNotEmpty(messageid, "消息id不能为空");
            try
            {
                _PublishToPrimaryMessageBusAsync(appid, code, messageid, body);
                return "ok";
            }
            catch (Exception ex)
            {
                var logmessage = "message bus PublishSync {0},{1},{2}".F(appid, code, body.ToJson());
                ApplicationLog.Error(logmessage, ex);
                return "fail";
            }
        }
        /// <summary>
        /// 发送消息（异步）
        /// </summary>
        /// <param name="appid">应用Id（必填）</param>
        /// <param name="code">业务类型（必填）</param>
        /// <param name="messageid">消息Id（必填）</param>
        /// <param name="body">消息正文（必填）</param>
        public static void PublishAsync(string appid, string code, string messageid, object body)
        {
            if (!ChecnkMessageBusAgentStatus(appid, code, body, messageid)) return;

            _Assert.AssertArgumentNotEmpty(appid, "appid 不能为空");
            _Assert.AssertArgumentNotEmpty(code, "业务类型不能为空");
            _Assert.AssertArgumentNotEmpty(messageid, "消息id不能为空");

            _PublishToPrimaryMessageBusAsync(appid, code, messageid, body);
        }

        internal static void _PublishToPrimaryMessageBusAsync(string appid, string code, string messageid, object body)
        {
            _Assert.AssertArgumentNotEmpty(appid, "appid 不能为空");
            _Assert.AssertArgumentNotEmpty(code, "业务类型不能为空");
            _Assert.AssertArgumentNotEmpty(messageid, "消息id不能为空");

            var _lowerAppid = appid.ToLowerInvariant();
            var _lowerCode = code.ToLowerInvariant();

            var requestData = Request.Builder.Add(AppCfgInfo2._appid, _lowerAppid)
                                             .Add(AppCfgInfo2._code, _lowerCode)
                                             .Add(AppCfgInfo2._messageid, messageid)
                                             .Add(AppCfgInfo2._body, body)
                                             .Add(AppCfgInfo2._ip, GetMachineIp())
                                             .ToRequestDto()
                                             .ToJson()
                                             ;

            if (MessageBusClientCfg.Instance.Configruation<bool>(appid, code, AppCfgInfo2.journalenable))
                ApplicationLog.Debug("bus#{0}".F(requestData));

            var timeOut = SetRequestTimeOut(appid, code);
            var requestDataByte = requestData.ToByte();
            var httpConnectionLimits = MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.busHttpConnectionLimit, 500);

            _HttpClientFactory.PostAsync(appid, RequestPrimaryMessageBus(appid, code), requestDataByte, timeOut, result =>
            {
                if (result.Exception != null)
                {
                    ApplicationLog.Error("异步请求消息总线主通道错误，使用从通道 {0} {1}，{2}".F(appid, code, messageid), result.Exception);
                    _PublishToSecondaryASync(appid, code, requestDataByte, timeOut, messageid, httpConnectionLimits);
                }
                else
                {
                    using (var streamReader = new StreamReader(result.ResponseStream))
                    {
                        if (streamReader.ReadToEnd().FromJsonTo<ResponseData<ResponseNull>>().Code != 200) throw new Exception();
                    }
                }

            }).WithHandler(ex =>
            {
                ApplicationLog.Error("异步请求消息总线主通道错误，使用从通道 {0} {1}，{2}".F(appid, code, messageid), ex);
                _PublishToSecondaryASync(appid, code, requestDataByte, timeOut, messageid, httpConnectionLimits);
            }
            , () => ApplicationLog.Debug("异步请求消息总线主通道发送消息成功 {0},{1},{2}".F(appid, code, messageid))
            );
        }

        internal static void _PublishToPrimaryMessageBusSync(string appid, string code, string messageid, object body)
        {
            _PublishToPrimaryMessageBusAsync(appid, code, messageid, body);
        }
        private static bool ChecnkMessageBusAgentStatus(string appid, string code, object body, string messageid)
        {
            if (_MessageBusAgentBootStart.Status != MessageBusAgentStatus.Runing)
            {
                _MessageBusAgentBootStart.TryInitBusAgentService();
                ApplicationLog.Debug("延迟启动总线客户端成功");
                return true;
            }
            if (_MessageBusAgentBootStart.Status == MessageBusAgentStatus.NoInit
                || _MessageBusAgentBootStart.Status == MessageBusAgentStatus.StartFail
                || _MessageBusAgentBootStart.Status == MessageBusAgentStatus.Stoped)
            {
                var message = "消息总线客户端程序未启动，使用默认配置发送消息 appid:{0},code:{1},msgid:{2},body:{3}".F(appid, code, messageid, body.ToJson());
                ApplicationLog.Debug(message);
                return true;
            }
            return true;
        }

        private static void _PublishToSecondaryASync(string appid, string code, byte[] requestData, int timeOut, string messageid, int httpConnectionLimits)
        {
            _HttpClientFactory.PostAsync(appid, RequestSecondaryMessageBus(appid, code)
                                                    , requestData
                                                    , timeOut
                                                    , response =>
                                                    {
                                                        if (response.Exception != null)
                                                            ApplicationLog.Error("异步请求消息总线从通道发送消息服务端响应异常 {0},{1}".F(appid, code, messageid), response.Exception);
                                                        else
                                                        {
                                                            using (var streamReader = new StreamReader(response.ResponseStream))
                                                            {
                                                                if (streamReader.ReadToEnd().FromJsonTo<ResponseData<ResponseNull>>().Code == 200)
                                                                    ApplicationLog.Debug("异步请求消息总线从通道发送消息成功{0},{1},{2}".F(appid, code, messageid));
                                                            }
                                                        }
                                                    })
                                                    .WithHandler(
                                                    _ex => ApplicationLog.Error("异步请求消息总线从通道发送消息异常 {0},{1}".F(appid, code, messageid), _ex)
                                                    , () => ApplicationLog.Debug("异步请求消息总线从通道发送消息成功 {0},{1},{2}".F(appid, code, messageid))); ;
        }
        private static int SetRequestTimeOut(string appid, string code)
        {
            return MessageBusClientCfg.Instance.Configruation<int>(appid.ToLower(), code.ToLower(), AppCfgInfo2.publishtimeout);
        }
        private static DateTime SetRetryTimeOut(string appid, string code)
        {
            return DateTime.Now.Add(MessageBusClientCfg.Instance.Configruation<TimeSpan>(appid.ToLower(), code.ToLower(), AppCfgInfo2.retrytimeout));
        }
        private static string GetMachineIp()
        {
            return Dns.GetHostAddresses(Dns.GetHostName()).Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).First().ToString();
        }
        private static string ApiHost()
        {
            return ConfigurationManager.AppSettings["MQBusServerHost"] ?? "http://api.mq.ymatou.com:2345";
        }
        private static string RequestPrimaryMessageBus(string appid, string code)
        {
            return "{0}{1}".F(MessageBusClientCfg.Instance.Configruation<string>(appid, code, AppCfgInfo2.bushost_primary) ?? ApiHost()
                            , MessageBusClientCfg.Instance.Configruation<string>(appid, code, AppCfgInfo2.requestpath));
        }
        private static string RequestSecondaryMessageBus(string appid, string code)
        {
            return "{0}{1}".F(MessageBusClientCfg.Instance.Configruation<string>(appid, code, AppCfgInfo2.bushost_secondary) ?? ApiHost()
                            , MessageBusClientCfg.Instance.Configruation<string>(appid, code, AppCfgInfo2.requestpath));
        }
    }
}
