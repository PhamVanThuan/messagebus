using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Extensions.Serialization;
using YmatouMQ.Common.Extensions._Task;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Concurrent;
using System.Diagnostics;
using Ymatou.PerfMonitorClient;
using _MethodMonitor = Ymatou.PerfMonitorClient.MethodMonitor;
using _LocalMethodMonitor = YmatouMQ.Common.Utils.MethodMonitor;
using YmatouMQ.Common.MessageHandleContract;
using YmatouMQ.Common;
using YmatouMQ.Log;
using YmatouMQNet4.Configuration;
using YmatouMQMessageMongodb.AppService;
using YmatouMQMessageMongodb.Domain.Module;
using YmatouMQ.Common.Utils;
using YmatouMQ.ConfigurationSync;

namespace YmatouMQ.MessageScheduler
{   
    public class MessageHandlerScheduler : IMessageHandler<byte[]>, IMessageRetryHandle<byte[]>
    {
        private static readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQMessageHandlerScheduler.MessageHandlerScheduler");
        private readonly ConcurrentDictionary<string, bool> ackcache = new ConcurrentDictionary<string, bool>();      
        private const int buffersize = 1048576;//1 * 1024 * 1024
        private const string request_error_code = "request_exception";
        
        //Mongodb消息调度
        public async Task Handle(MessageHandleContext<byte[]> msgContext
            , Func<IDictionary<string, bool>, Task> callbackEndAction)
        {
            await HandleMessageFromMongodb(msgContext, callbackEndAction);
        }
        //Rabbitmq消息处理调度
        public async Task Handle(MessageHandleContext<byte[]> msgContext
            , Func<string, Task> handleSuccess
            , Func<Task> handleException)
        {
            await HandleMessageFromRabbitMQ(msgContext, handleSuccess, handleException);
        }
        //初始化消息调度器
        public void InitHandle(string appid, string code)
        {
            var cfg = MQMainConfigurationManager.Builder.GetConfiguration(appid, code);
            if (cfg.CallbackCfgList.IsEmptyEnumerable()) return;
            cfg.CallbackCfgList.Where(e => e.Enable.Value).EachAction(
             c =>
             {
                 using (var mm = new _LocalMethodMonitor(log, 500, "httpClient create, appid:{0},code:{1},url:{2}"
                     .Fomart(appid, code, c.Url)))
                 {
                     _HttpClientFactory.Factory(buffersize, c.ContentType, "{0}.{1}".Fomart(appid, code), c.CallbackTimeOut);
                 }
             });
            log.Debug("MessageHandlerScheduler handle init success");
        }
        //关闭调度
        public void CloseHandle(string appid, string code)
        {
            _HttpClientFactory.ClearHttpClient("{0}.{1}".Fomart(appid, code));
            ackcache.Clear();
            log.Debug("ClearHttpClient CloseHandle..ok，{0}.{1}".Fomart(appid, code));
        }
        //HandleMessageFromMongodb
        private async Task HandleMessageFromMongodb(MessageHandleContext<byte[]> msgContext
            , Func<IDictionary<string, bool>, Task> callbackEndAction)
        {
            var callbackCfgArray = MQMainConfigurationManager.Builder.GetConfiguration(msgContext.AppId, msgContext.Code);
            if (!callbackCfgArray.CallbackCfgList.Any())
            {
                log.Debug("[HandleMessageFromMongodb] no callback configuration.appid:{0},code:{1}", msgContext.AppId, msgContext.Code);
                return;
            }
            IEnumerable<CallbackConfiguration> getCallbackCfg;
            if( msgContext.IsCheckEableRetry )
            //回调启用&允许重试&重试key存在
            getCallbackCfg = callbackCfgArray.CallbackCfgList.Where(e => e.Enable.Value == true                                                                            
                                                                            && e.IsRetry.Value > 0
                                                                            && msgContext.RetryCallbackKey.Contains(e.CallbackKey));
            else     
                getCallbackCfg = callbackCfgArray.CallbackCfgList.Where(e => e.Enable.Value == true
                                                                            && msgContext.RetryCallbackKey.Contains(e.CallbackKey));             
            if (!getCallbackCfg.Any())
            {
                log.Debug("[HandleMessageFromMongodb] callback url disable. appid: {0},code:{1}", msgContext.AppId, msgContext.Code);
                return;
            }
            log.Debug("[HandleMessageFromMongodb] begin CallbackAsync,appid:{0},code:{1},body:{2}".Fomart(msgContext.AppId, msgContext.Code, msgContext.Message.GetString()));
            var result = await CallbackAsync(msgContext, getCallbackCfg).ConfigureAwait(false);
            await callbackEndAction(result.SafeToDictionary(c => c.Key, c => c.Value.Item1)).ConfigureAwait(false);
            var success = result.Where(_kv => _kv.Value.Item1 == true).Select(kv => kv.Key);
            log.Debug("[HandleMessageFromMongodb] end CallbackAsync ?，appid:{0},cdoe:{1},result:{2}", msgContext.AppId, msgContext.Code, string.Join(",", success) ?? "fail");
        }
        //HandleMessageFromRabbitMQ
        private async Task HandleMessageFromRabbitMQ(MessageHandleContext<byte[]> msgContext
            , Func<string, Task> handleSuccess
            , Func<Task> handleException)
        {
            //如果消息是RabbitMQ重复推送则直接ack
            //总线在停止过程中，再重新启动后，会出现这种情况
            //RabbititMQ 重复推送消息
            if (msgContext.Redelivered)
            {
                //BUG fix 2016.7.21
                //await handleSuccess(string.Empty).ConfigureAwait(false);
                log.Debug("HandleMessageFromRabbitMQ [warning] message Redelivered, appid:{0},code:{1},mid:{2},uuid:{3}".Fomart(msgContext.AppId, msgContext.Code,
                    msgContext.MessageId,msgContext.Uuid));               
            }
            var cfg = MQMainConfigurationManager.Builder.GetConfiguration(msgContext.AppId, msgContext.Code);
            if (!cfg.CallbackCfgList.Any())
            {
                log.Debug("HandleMessageFromRabbitMQ [warning] HMQ appid:{0},code:{1},no set callback.", msgContext.AppId, msgContext.Code);
                return;
            }
            //如果enable 为false则不回调业务端
            var realCallbackArray = cfg.CallbackCfgList.Where(c => c.Enable.Value == true);
            if (!realCallbackArray.Any())
            {
                log.Debug("HandleMessageFromRabbitMQ [warning] HMQ appid:{0},code:{1},callback disable.", msgContext.AppId, msgContext.Code);
                return;
            }
            log.Debug("HandleMessageFromRabbitMQ appid:{0},code:{1}, callback client count:{2},messageSize:{3} byte, messageInfo:{4}",
                msgContext.AppId, msgContext.Code,
                realCallbackArray.Count(), msgContext.Message.Length,
                msgContext.Message.GetString());
            //异步回调业务端&任何一个业务端返回即ACk消息
            var result = await CallbackAsync(msgContext, realCallbackArray, handleSuccess).ConfigureAwait(false);
            //删除内存中已经ACK的消息
            RemoveMessageId(msgContext.Uuid);
            //如果存在处理失败且需要补发的消息，则发送消息到mongodb，等待补发消息
            var fail = result.Where(_kv => _kv.Value.Item2 == true).Select(kv => kv.Key);           
            if (cfg.ConsumeCfg.RetryTimeOut.Value > 0 && fail.Any())
            {
                var exists = MessageAppService.CheckExistsRetryMessage(msgContext.Uuid, msgContext.AppId,
                    msgContext.Code);
                if (!exists)
                {
                    await
                        RetryMessageSendToMongodb(msgContext, fail.ToList(), cfg.ConsumeCfg.RetryTimeOut.Value)
                            .ConfigureAwait(false);
                    log.Debug("HandleMessageFromRabbitMQ callback client response error,write to mongodb wait retry,appid:{0},code:{1},response fail callbackKey:{2}",
                        msgContext.AppId, msgContext.Code,
                        string.Join(",", fail));
                }
                else
                {
                    log.Debug("HandleMessageFromRabbitMQ [warning] message uuid:{0},mid:{1} retry already.", msgContext.Uuid, msgContext.MessageId);
                }
                #region

                //处理失败允许ACK或者业务端处理成功
                //if ((cfg.ConsumeCfg.HandleFailAcknowledge.Value || success.Any()))
                //    await handleSuccess(string.Empty).ConfigureAwait(false);

                #endregion
            }
            //根据配置保存业务端处理结果
            if (cfg.ConsumeCfg.HandleSuccessSendNotice.Value)
            {
                msgContext.SetCallback(
                    result.Select(e =>
                         HanderResultToString(cfg.CallbackCfgList, e.Key, e.Value.Item1, e.Value.Item3, e.Value.Item4))
                        .ToArray());
                if (!msgContext.RetryCallbackKey.Any())
                {
                    log.Debug("HandleMessageFromRabbitMQ appid:{0},code:{1} CallbackKey is empty.", msgContext.AppId, msgContext.Code);
                }
                await SaveHandleResultToMongodb(msgContext, MessagePublishStatus.PushOk).ConfigureAwait(false);
            }
            //消息放入队列等待更新状态            
            MessagePushStatusAppService.QueueWrapper.SendAsync(new MessagePushStatus2
            {
                AppId = msgContext.AppId,
                Code = msgContext.Code,
                UuId = msgContext.Uuid
            });
        }
        //构建消息处理结果字符串
        private string HanderResultToString(IEnumerable<CallbackConfiguration> callbackCfg, string callbackKey
            , bool resultOk, string message = null, string response = null)
        {
            var strResult = resultOk ? "ok" : "fail";
            var callback = callbackCfg.FirstOrDefault(c => c.CallbackKey == callbackKey);
            var responseString = (!resultOk && response != request_error_code && response != "\"fail\"" && response != "fail")
                ? response.TrySubString("ClientResponseMessageLength".GetAppSettings("100").ToInt32(100)) : null;
            return callback == null ? "" : "{0}，{1}，{2}，{3}".Fomart(strResult, message, responseString, callback.Url);
        }
        //执行业务端回调，返回业务端处理结果，及是否需要补发消息
        [Obsolete]
        private async Task<ConcurrentDictionary<string, Tuple<bool, bool>>> Callback(MessageHandleContext<byte[]> msgContext
            , IEnumerable<CallbackConfiguration> callback)
        {
            //存储每个业务端返回的结果
            var dic = new ConcurrentDictionary<string, Tuple<bool, bool>>();
            #region
            //var allHandleTask = callback.Select(async c =>
            // {
            //     var resultCode = await TryRequestClientHandleMessage(msgContext, c).ConfigureAwait(false);
            //     if (resultCode == request_error_code)
            //         log.Debug("##请求：{0},{1},appid {2},code {3}", c.Url, resultCode, msgContext.AppId, msgContext.Code);
            //     //分析结果
            //     var ok = HandlerResponseCode.IsSuccess(resultCode);
            //     //处理失败且需要补单  
            //     var result = Tuple.Create(ok, !ok && c.IsRetry.Value > 0);
            //     dic.AddOrUpdate(c.CallbackKey, result, (k, b) => result);
            //     return resultCode;
            // });
            //allHandleTask.TryWaitAllTask(log, CancellationToken.None);
            #endregion
            await callback.ForEachAsync(async _callback =>
            {
                return await TryRequestClientHandleMessage(msgContext, _callback).ConfigureAwait(false);
            }, (_callback, _result) =>
            {
                if (_result.Result == request_error_code)
                    log.Debug("##request fail：{0},{1},appid {2},code {3}", _callback.Url, request_error_code, msgContext.AppId, msgContext.Code);
                //分析结果
                var ok = HandlerResponseCode.IsSuccess(_result.Result);
                //处理失败且需要补单  
                var result = Tuple.Create(ok, !ok && _callback.IsRetry.Value > 0);
                dic.AddOrUpdate(_callback.CallbackKey, result, (k, b) => result);
            }, Environment.ProcessorCount);
            return dic;
        }
        //异步回调业务端处理消息
        private async Task<ConcurrentDictionary<string, Tuple<bool, bool, string, string>>> CallbackAsync(MessageHandleContext<byte[]> msgContext
            , IEnumerable<CallbackConfiguration> callback
            , Func<string, Task> callbackAnyComplete = null)
        {
            var dic = new ConcurrentDictionary<string, Tuple<bool, bool, string, string>>();

            await callback.ForEachAsync2(async _callback =>
             {
                 var result = await TryRequestClientHandleMessage(msgContext, _callback).ConfigureAwait(false);
                 return Tuple.Create(_callback, result);
             }, async c =>
             {
                 if (c.Item2.Result == request_error_code)
                     log.Debug("##request fail：{0},{1},appid {2},code {3}", c.Item1.Url
                         , request_error_code, msgContext.AppId, msgContext.Code);
                 //分析结果
                 var ok = HandlerResponseCode.IsSuccess(c.Item2.Result);
                 if (callbackAnyComplete != null)
                     await CallbackAnyCompleteAction(callbackAnyComplete, c.Item2.MessageId, ok, c.Item1.Url).ConfigureAwait(false);
                 //处理失败且需要补单(item1:业务端处理是否成功,item2:是否需要补单,item3:返回的消息含异常)  
                 var result = Tuple.Create(ok, !ok && c.Item1.IsRetry.Value > 0, c.Item2.Message, c.Item2.Result);
                 dic.AddOrUpdate(c.Item1.CallbackKey, result, (k, b) => result);
             });

            return dic;
        }
        //任何第一个回调业务端返回则执行callbackAnyComplete
        private async Task CallbackAnyCompleteAction(Func<string, Task> callbackAnyComplete, string requestId, bool ok, string responseUrl)
        {
            if (!ackcache.ContainsKey(requestId) && callbackAnyComplete != null)
            {
                if (ackcache.TryAdd(requestId, true))
                {
                    log.Debug("mid:{0},ackCacheSize {1},callback client response, url:{2}", requestId, ackcache.Count, responseUrl);
                    await callbackAnyComplete(string.Empty).ConfigureAwait(false);
                }
            }
        }
        //删除缓存中的消息ID
        private void RemoveMessageId(string uuid)
        {
            bool _ack;
            var result = ackcache.TryRemove(uuid, out _ack);
            if (!result)
                log.Debug("ack cache remove {0},ackCacheSize:{1}", result, ackcache.Count);
        }
        //业务端消息处理结果存入mongodb
        private async Task SaveHandleResultToMongodb(MessageHandleContext<byte[]> msgContext, MessagePublishStatus pushStatus)
        {                     
            await
                MessageHandleStatusAppService_Batch.Instance.SaveMessageStatusAsync(
                    new MQMessageStatus(msgContext.MessageId
                        , pushStatus
                        , msgContext.AppId
                        , msgContext.Source
                        , msgContext.RetryCallbackKey
                        , msgContext.Uuid
                        , msgContext.Redelivered)
                    )
                    .WithHandleException(log, null, "消息状态数据发送到mongodb异常 {0},{1},{2}", msgContext.AppId, msgContext.Code,
                        msgContext.Code);
        }

        //发送需要重试的消息发送到mongodb
        private async Task RetryMessageSendToMongodb(MessageHandleContext<byte[]> msgContext, List<string> callbackKey, int retryTimeout)
        {
            await RetryMessageAppService_Batch.Instance.AddRetryMessageAsync(new RetryMessage(msgContext.AppId
                                                                            , msgContext.Code
                                                                            , msgContext.MessageId
                                                                            , Encoding.GetEncoding("utf-8").GetString(msgContext.Message)
                                                                            , DateTime.Now.AddMinutes(retryTimeout)
                                                                            , callbackKey
                                                                            , uuid:msgContext.Uuid
                                                                            , messageSource: _MessageSource.MessageSource_Retry))
                                                      .WithHandleException(log, null, "数据发送到mongodb异常 {0},{1},{2}", msgContext.AppId
                                                                            , msgContext.Code, msgContext.Code);

        }
        //发送消息到业务端处理。item1：业务端返回结果，item2：错误消息
        public async Task<CallbackResponse> TryRequestClientHandleMessage(MessageHandleContext<byte[]> msgContext, CallbackConfiguration callback)
        {
            using (var total = _MethodMonitor.New("Total"))//total monitor
            using (var mm = _MethodMonitor.New("{0}_{1}".Fomart(msgContext.AppId, msgContext.Code))) //appid,code monitor
            using (var api_mm = _MethodMonitor.New("{0}_{1}_{2}".Fomart(msgContext.AppId, msgContext.Code, new Uri(callback.Url).LocalPath))) //url monitor
            using (var _mm = new _LocalMethodMonitor(log, descript: "callback {0}_{1}_{2},".Fomart(msgContext.AppId, msgContext.Code, new Uri(callback.Url).LocalPath)))  //local monitor run time
            {
                try
                {
                    log.Debug("[begin callback client], url:{0},timeout:{1},callbackKey:{2},msgid:{3}", callback.Url,
                        callback.CallbackTimeOut
                        , callback.CallbackKey, msgContext.MessageId);
                    var uri = new Uri(callback.Url);
                    var httpClient = _HttpClientFactory.Factory(buffersize, callback.ContentType, msgContext.Code
                        /*callback.CallbackKey*/, callback.CallbackTimeOut);
                    var context = new ByteArrayContent(msgContext.Message);
                    context.Headers.Add("Content-Type", "{0};charset=utf-8".Fomart(callback.ContentType));
                    var clientApiResult = await httpClient.PostAsync(uri
                        , context
                        ,
                        callback.CallbackTimeOut > 0
                            ? new CancellationTokenSource(callback.CallbackTimeOut.Value).Token
                            : CancellationToken.None)
                        .ConfigureAwait(false);

                    //确保http 状态码 200                   
                    clientApiResult.EnsureSuccessStatusCode();
                    var responseString = await clientApiResult.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var availableThread = MQThreadPool.GetAvailableWorkThreadsAndIoThread();
                    log.Debug(
                        "[end callback client],url:{0},result:{1}, AvailableWorkThread:{2},AvailableIoThread:{3},requestTimeOut:{4},client run time:{5} ms,msgid:{6},uuid:{7}"
                        , uri.ToString(), responseString, availableThread.Item1, availableThread.Item2,
                        callback.CallbackTimeOut
                        , _mm.GetRunTime.TotalMilliseconds, msgContext.MessageId, msgContext.Uuid);
                    return new CallbackResponse(callback.CallbackKey, responseString, "{0} ms"
                        .Fomart(_mm.GetRunTime.TotalMilliseconds), msgContext.Uuid);
                }
                catch (AggregateException ex)
                {
                    log.Error2(AlarmAppService.FindAlarmAppId(callback.CallbackKey,msgContext.Url)
                        ,
                        "messagebus callback AggregateException,appid:{0},code:{1},url:{2},run:{3:N0} ms,request timeOut:{4},msgid:{5},uuid:{6}"
                            .Fomart(msgContext.AppId, msgContext.Code
                                , callback.Url, _mm.GetRunTime2, callback.CallbackTimeOut,
                                msgContext.MessageId, msgContext.Uuid));
                    return new CallbackResponse(callback.CallbackKey, request_error_code, "{0}，{1} ms"
                        .Fomart(_InnerExceptionMessage(ex), _mm.GetRunTime.TotalMilliseconds),
                        msgContext.Uuid ?? msgContext.MessageId);
                }
                catch (OperationCanceledException ex)
                {
                    log.Error2(AlarmAppService.FindAlarmAppId(callback.CallbackKey, msgContext.Url),
                        "messagebus callback TimeOutExceptiont,appid:{0},code:{1},url:{2},run:{3:N0} ms,request timeOut:{4},msgId:{5},uuid:{6},{7}"
                        , msgContext.AppId, msgContext.Code, callback.Url, _mm.GetRunTime2
                        , callback.CallbackTimeOut, msgContext.MessageId, msgContext.Uuid, ex.ToString());
                    return new CallbackResponse(callback.CallbackKey, request_error_code, "Timeout，{0} ms"
                        .Fomart(_mm.GetRunTime.TotalMilliseconds), msgContext.Uuid ?? msgContext.MessageId);
                }
                catch (WebException ex)
                {
                    log.Error2(AlarmAppService.FindAlarmAppId(callback.CallbackKey, msgContext.Url)
                        , "messagebus callback WebException,appid:{0},code:{1},url:{2},run:{3:N0} ms,request timeOut:{4},msgId:{5},{6}",
                        msgContext.AppId, msgContext.Code
                        , callback.Url, _mm.GetRunTime2, callback.CallbackTimeOut, msgContext.MessageId,
                        ex.ToString());
                    return new CallbackResponse(callback.CallbackKey, request_error_code, "{0}，{1} ms"
                        .Fomart(_InnerExceptionMessage(ex), _mm.GetRunTime.TotalMilliseconds),
                        msgContext.Uuid ?? msgContext.MessageId);
                }
                catch (Exception ex)
                {
                    log.Error2(AlarmAppService.FindAlarmAppId(callback.CallbackKey, msgContext.Url)
                        , "messagebus callback Exception, appid:{0},code:{1},url:{2},run {3:N0} ms,request timeOut:{4},msgId:{5},{6}",
                        msgContext.AppId, msgContext.Code
                        , callback.Url, _mm.GetRunTime2, callback.CallbackTimeOut, msgContext.MessageId,
                        ex.ToString());
                    return new CallbackResponse(callback.CallbackKey, request_error_code, "{0}，{1} ms"
                        .Fomart(_InnerExceptionMessage(ex), _mm.GetRunTime.TotalMilliseconds),
                        msgContext.Uuid ?? msgContext.MessageId);
                }
            }
        }

        private static string _InnerExceptionMessage(Exception ex)
        {
            return ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        private string InnserExceptionMessage(AggregateException ex)
        {
            return ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
    }
}
