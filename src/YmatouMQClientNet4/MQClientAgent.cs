using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ymatou.CommonService;
using YmatouMessageBusClientNet4.Dto;
using YmatouMessageBusClientNet4.Extensions;
using YmatouMessageBusClientNet4.Persistent;

namespace YmatouMessageBusClientNet4
{
    public class MessageBusAgent
    {
        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="dto">声明发布消息dto</param>
        /// <param name="errorHandle">异常时回调</param>
        /// <param name="timeOut">发送消息超时时间（默认3秒）</param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="WebException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
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
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="WebException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static void Publish(string appid, string code, string messageid, object body, int timeOut = 3000)
        {
            //
            _Assert.AssertArgumentNotEmpty(appid, "appid 不能为空");
            _Assert.AssertArgumentNotEmpty(code, "业务类型不能为空");
            _Assert.AssertArgumentNotEmpty(messageid, "消息id不能为空");
            _Assert.AssertArgumentNotNull(body, "消息体不能为空");
            //
            if (!ChecnkMessageBusAgentStatus(appid, code, body, messageid))
            {
                AppendToLocalJournal(appid, code, messageid, body);
                return;
            }
            //
            TryPublish(appid
                        , code
                        , messageid
                        , body
                        , MessageBusClientCfg.Instance.Configruation<bool>(appid, code, AppCfgInfo2.publishasync));
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
        /// <exception cref="InvalidOperationException"></exception>
        public static string PublishSync(string appid, string code, string messageid, object body)
        {
            //
            _Assert.AssertArgumentNotEmpty(appid, "appid 不能为空");
            _Assert.AssertArgumentNotEmpty(code, "业务类型不能为空");
            _Assert.AssertArgumentNotEmpty(messageid, "消息id不能为空");
            _Assert.AssertArgumentNotNull(body, "消息体不能为空");
            //
            if (!ChecnkMessageBusAgentStatus(appid, code, body, messageid))
            {
                AppendToLocalJournal(appid, code, messageid, body);
                return "config not init.";
            }
            //
            return TryPublish(appid, code, messageid, body, false);
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
            //
            _Assert.AssertArgumentNotEmpty(appid, "appid 不能为空");
            _Assert.AssertArgumentNotEmpty(code, "业务类型不能为空");
            _Assert.AssertArgumentNotEmpty(messageid, "消息id不能为空");
            _Assert.AssertArgumentNotNull(body, "消息体不能为空");
            //
            if (!ChecnkMessageBusAgentStatus(appid, code, body, messageid))
            {
                AppendToLocalJournal(appid, code, messageid, body);
                return;
            }
            //
            TryPublish(appid, code, messageid, body, true);
        }
        /// <summary>
        /// 发送消息（异步）
        /// </summary>
        /// <param name="appid">应用Id（必填）</param>
        /// <param name="code">业务类型（必填）</param>
        /// <param name="messageid">消息Id（必填）</param>
        /// <param name="body">消息正文（必填）</param>
        public static Task PublishAsyncTask(string appid, string code, string messageid, object body)
        {
            _Assert.AssertArgumentNotEmpty(appid, "appid 不能为空");
            _Assert.AssertArgumentNotEmpty(code, "业务类型不能为空");
            _Assert.AssertArgumentNotEmpty(messageid, "消息id不能为空");
            _Assert.AssertArgumentNotNull(body, "消息体不能为空");

            if (!ChecnkMessageBusAgentStatus(appid, code, body, messageid))
            {
                AppendToLocalJournal(appid, code, messageid, body);
                var task = new TaskCompletionSource<object>();
                task.SetResult(null);
                return task.Task;
            }

            return _PublishToPrimaryMessageBusAsyncTask(appid, code, messageid, body);
        }
        private static Task _PublishToPrimaryMessageBusAsyncTask(string appid, string code, string messageid, object body)
        {
            var _lowerAppid = appid.ToLower();
            var _lowerCode = code.ToLower();

            var requestData = Request.Builder.Add(AppCfgInfo2._appid, _lowerAppid)
                                             .Add(AppCfgInfo2._code, _lowerCode)
                                             .Add(AppCfgInfo2._messageid, messageid)
                                             .Add(AppCfgInfo2._body, body)
                                             .Add(AppCfgInfo2._ip, GetMachineIp())
                                             .Add(() => IsSendVersionToServer(), AppCfgInfo2._version, AppCfgInfo2.agent_version)
                                             .Add(() => IsSendAgentClientAppId(), AppCfgInfo2._clientappid, BusAgentClientAppId())
                                             .ToRequestDto()
                                             .ToJson()
                                             ;

            //if (MessageBusClientCfg.Instance.Configruation<bool>(appid, code, AppCfgInfo2.journalenable))
            //    JournalFactory.MessageLocalJournalBuilder.AppendAsync2(requestData, null);

            var timeOut = SetRequestTimeOut(appid, code);
            var requestDataByte = requestData.ToByte();
            var httpConnectionLimits = MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.busHttpConnectionLimit, 1000);

            return WebRequestWrap.Builder.PostAsync(
                RequestPrimaryMessageBus(appid, code)
                , WebRequestWrap.Content_Json
                , requestDataByte
                , timeOut
                , result =>
                 {
                     if (result.Exception != null)
                     {
                         ApplicationLog.Debug("异步请求消息总线主通道错误，使用从通道 {0} {1}，{2}".F(appid, code, messageid));
                         _PublishToSecondaryASync(appid, code, requestDataByte, timeOut, messageid, httpConnectionLimits);
                     }
                     else
                     {
                         using (var streamReader = new StreamReader(result.ResponseStream))
                         {
                             if (streamReader.ReadToEnd().FromJsonTo<ResponseData<ResponseNull>>().Code != 200) throw new Exception();
                         }
                     }

                 }, httpConnectionLimit: httpConnectionLimits).WithHandler(ex =>
                 {
                     ApplicationLog.Debug("异步请求消息总线主通道错误，使用从通道 {0} {1}，{2}".F(appid, code, messageid));
                     _PublishToSecondaryASync(appid, code, requestDataByte, timeOut, messageid, httpConnectionLimits);
                 }
                 , () => ApplicationLog.Debug("异步请求消息总线主通道发送消息成功 {0},{1},{2}".F(appid, code, messageid)));
        }
        private static void _PublishToPrimaryMessageBusAsync(string appid, string code, string messageid, object body)
        {
            _PublishToPrimaryMessageBusAsyncTask(appid, code, messageid, body);
        }
        private static string _PublishToPrimaryMessageBusSync(string appid, string code, string messageid, object body)
        {
            var _lowerAppid = appid.ToLower();
            var _lowerCode = code.ToLower();

            var requestData = Request.Builder.Add(AppCfgInfo2._appid, _lowerAppid)
                                             .Add(AppCfgInfo2._code, _lowerCode)
                                             .Add(AppCfgInfo2._messageid, messageid)
                                             .Add(AppCfgInfo2._body, body)
                                             .Add(AppCfgInfo2._ip, GetMachineIp())
                                             .Add(() => IsSendVersionToServer(), AppCfgInfo2._version, AppCfgInfo2.agent_version)
                                             .Add(() => IsSendAgentClientAppId(), AppCfgInfo2._clientappid, BusAgentClientAppId())
                                             .ToRequestDto()
                                             .ToJson()
                                             ;

            //if (MessageBusClientCfg.Instance.Configruation<bool>(appid, code, AppCfgInfo2.journalenable))
            //    JournalFactory.MessageLocalJournalBuilder.AppendAsync2(requestData, null);

            var requestDataByte = requestData.ToByte();
            var timeOut = SetRequestTimeOut(appid, code);
            var result = "ok";
            var userPrimary = true;
            var httpConnectionLimits = MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.busHttpConnectionLimit, 1000);

            result = WebRequestWrap.Builder.Post(RequestPrimaryMessageBus(appid, code)
                                                    , WebRequestWrap.Content_Json
                                                    , requestDataByte
                                                    , timeOut
                                                    , requestError: _requestError =>
                                                    {
                                                        ApplicationLog.Error("同步请求消息总线主通道错误，使用从通道。appid:{0},code:{1},msgid:{2}".F(appid, code, messageid), _requestError);
                                                        result = _PublishToSecondarySync(appid, code, requestDataByte, timeOut, messageid, httpConnectionLimits);
                                                        userPrimary = false;
                                                        ApplicationLog.Debug("同步请求消息总线从通道发送消息{3}。 appid:{0},code:{1},msgid:{2}".F(appid, code, messageid, result));
                                                    }
                                                    , responseAction: r =>
                                                    {
                                                        using (var streamReader = new StreamReader(r))
                                                        {
                                                            if (streamReader.ReadToEnd().FromJsonTo<ResponseData<ResponseNull>>().Code != 200)
                                                                throw new Exception();
                                                        }
                                                    }
                                                    , httpConnectionLimit: httpConnectionLimits);
            if (userPrimary)
                ApplicationLog.Debug("同步请求消息总线主通道发送消息{3}。 appid:{0},code:{1},msgid:{2}".F(appid, code, messageid, result));
            return result;
        }
        private static bool ChecnkMessageBusAgentStatus(string appid, string code, object body, string messageid)
        {
            if (MessageBusAgentBootStart.Status != MessageBusAgentStatus.Runing)
            {
                MessageBusAgentBootStart.TryInitBusAgentService();
            }
            ApplicationLog.Debug("延迟启动总线客户端成功 {0}".F(MessageBusAgentBootStart.Status));
            return true;
        }
        private static string _PublishToSecondarySync(string appid, string code, byte[] requestData, int timeOut, string messageId, int httpConnectionLimits)
        {
            return WebRequestWrap.Builder.Post(RequestSecondaryMessageBus(appid, code)
                                         , WebRequestWrap.Content_Json
                                         , requestData
                                         , timeOut
                                         , requestError: err =>
                                         {
                                             //若备份通道发送错误，则记录日志
                                             JournalFactory.MessageLocalJournalBuilder.AppendAsync2(requestData.ToJson(), null);
                                             ApplicationLog.Error("请求消息总线从通道错误, appid:{0},code:{1},messageid:{2}".F(appid, code, messageId), err);
                                         }
                                         , httpConnectionLimit: httpConnectionLimits);
        }
        private static void _PublishToSecondaryASync(string appid, string code, byte[] requestData, int timeOut, string messageid, int httpConnectionLimits)
        {
            WebRequestWrap.Builder.PostAsync(RequestSecondaryMessageBus(appid, code)
                                                    , WebRequestWrap.Content_Json
                                                    , requestData
                                                    , timeOut
                                                    , response =>
                                                    {
                                                        if (response.Exception != null)
                                                        {
                                                            JournalFactory.MessageLocalJournalBuilder.AppendAsync2(requestData.ToJson(), null);
                                                            ApplicationLog.Error("异步请求消息总线从通道发送消息服务端响应异常 {0},{1},{2}".F(appid, code, messageid), response.Exception);
                                                        }
                                                        else
                                                        {
                                                            using (var streamReader = new StreamReader(response.ResponseStream))
                                                            {
                                                                if (streamReader.ReadToEnd().FromJsonTo<ResponseData<ResponseNull>>().Code == 200)
                                                                    ApplicationLog.Debug("异步请求消息总线从通道发送消息成功{0},{1},{2}".F(appid, code, messageid));
                                                            }
                                                        }
                                                    }
                                                    , httpConnectionLimit: httpConnectionLimits).WithHandler(
                                                    _ex =>
                                                    {
                                                        JournalFactory.MessageLocalJournalBuilder.AppendAsync2(requestData.ToJson(), null);
                                                        ApplicationLog.Error("异步请求消息总线从通道发送消息异常 {0},{1},{2}".F(appid, code, messageid), _ex);
                                                    }
                                                    , () => ApplicationLog.Debug("异步请求消息总线从通道发送消息成功 {0},{1},{2}".F(appid, code, messageid)));
        }
        private static string TryPublish(string appid, string code, string messageid, object body, bool async)
        {
            try
            {
                if (async)
                    _PublishToPrimaryMessageBusAsync(appid, code, messageid, body);
                else
                    _PublishToPrimaryMessageBusSync(appid, code, messageid, body);

                return "ok";
            }
            catch (Exception ex)
            {
                ApplicationLog.Error("发布消息异常 {0},{1},{2}".F(appid, code, body.ToJson()), ex);
                return ex.ToString();
            }
        }
        private static void AppendToLocalJournal(string appid, string code, string messageid, object body)
        {
            var requestData = Request.Builder.Add(AppCfgInfo2._appid, appid.ToLower())
                                             .Add(AppCfgInfo2._code, code.ToLower())
                                             .Add(AppCfgInfo2._messageid, messageid)
                                             .Add(AppCfgInfo2._body, body)
                                             .Add(AppCfgInfo2._ip, GetMachineIp())
                                             .Add(() => IsSendVersionToServer(), AppCfgInfo2._version, AppCfgInfo2.agent_version)
                                             .Add(() => IsSendAgentClientAppId(), AppCfgInfo2._clientappid, BusAgentClientAppId())
                                             .ToRequestDto()
                                             .ToJson();
            JournalFactory.MessageLocalJournalBuilder.AppendAsync2(requestData, null);
        }
        private static int SetRequestTimeOut(string appid, string code)
        {
            return MessageBusClientCfg.Instance.Configruation<int>(appid.ToLower(), code.ToLower(), AppCfgInfo2.publishtimeout);
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
        private static bool IsSendVersionToServer()
        {
            return MessageBusClientCfg.Instance.DefaultConfigruation<bool>(AppCfgInfo2.sendagentversiontoserver);
        }
        private static bool IsSendAgentClientAppId()
        {
            return MessageBusClientCfg.Instance.DefaultConfigruation<bool>(AppCfgInfo2.sendagentappidtoserver);
        }
        private static string BusAgentClientAppId()
        {
            return MessageBusClientCfg.Instance.DefaultConfigruation<string>(AppCfgInfo2.agentappid, null)
                ?? ConfigurationManager.AppSettings["AppId"];
        }
    }
}
