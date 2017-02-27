using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using YmatouMQNet4;
using YmatouMQNet4.Configuration;
using YmatouMQ.Connection;
using YmatouMQNet4.Core;
using YmatouMQ.Common.Dto;
using YmatouMQ.Common.Extensions;
using _MethodMonitor = Ymatou.PerfMonitorClient.MethodMonitor;
using YmatouMQ.Common.Utils;
using YmatouMQ.Common;
using YmatouMQ.Common.Extensions._Task;
using YmatouMQ.ConfigurationSync;

namespace YmatouMQServerConsoleApp.Handler
{
    public class MessageController : ApiController
    {
        private static  readonly WorkStealingTaskScheduler Scheduler=new WorkStealingTaskScheduler();
        [Route("message/publish")]
        [HttpPost]
        public ResponseData<ResponseNull> Publish([FromBody] MessageDto value)
        {
            if (value.IsNull())
                return ResponseData<ResponseNull>.CreateFail(ResponseNull._Null, lastErrorMessage: "MessageDto is null");
            if (value.AppId.IsEmpty())
                return ResponseData<ResponseNull>.CreateFail(ResponseNull._Null,
                    lastErrorMessage: "MessageDto AppId is null");
            if (value.Code.IsEmpty())
                return ResponseData<ResponseNull>.CreateFail(ResponseNull._Null,
                    lastErrorMessage: "MessageDto Code is null");
            if (value.Body.IsNull())
                return ResponseData<ResponseNull>.CreateFail(ResponseNull._Null,
                    lastErrorMessage: "MessageDto Body is null");
            using (_MethodMonitor.New("PubTotal"))
            using (_MethodMonitor.New("{0}_{1}".Fomart(value.AppId, value.Code)))
            {
                //Task.Factory.StartNew(() => MessageBus.Publish(value.Body, value.AppId, value.Code, value.MsgUniqueId, value.Ip), CancellationToken.None, TaskCreationOptions.None, Scheduler);
                MessageBus.Publish(value.Body, value.AppId, value.Code, value.MsgUniqueId, value.Ip);
            }
            return ResponseData<ResponseNull>.CreateSuccess(ResponseNull._Null, "ok");
        }

        [Route("message/publishasync")]
        [HttpPost]
        public async Task<ResponseData<string>> PublishAsync([FromBody] MessageDto value)
        {
            if (value.IsNull())
                return ResponseData<string>.CreateFail(string.Empty, lastErrorMessage: "MessageDto is null");
            if (value.AppId.IsEmpty())
                return ResponseData<string>.CreateFail(string.Empty,
                    lastErrorMessage: "MessageDto AppId is null");
            if (value.Code.IsEmpty())
                return ResponseData<string>.CreateFail(string.Empty,
                    lastErrorMessage: "MessageDto Code is null");
            if (value.Body.IsNull())
                return ResponseData<string>.CreateFail(string.Empty,
                    lastErrorMessage: "MessageDto Body is null");
            using (_MethodMonitor.New("PubTotalAsync"))
            using (_MethodMonitor.New("PubAsync_{0}_{1}".Fomart(value.AppId, value.Code)))
            {
                await
                    MessageBus.PublishAsync(value.Body, value.AppId, value.Code, value.MsgUniqueId, value.Ip)
                        .ConfigureAwait(false);
            }
            return ResponseData<string>.CreateSuccess("ok", "ok");
        }

        [Route("message/publishtodb")]
        [HttpPost]
        public ResponseData<string> PublishToDB([FromBody] MessageDto value)
        {
            if (value.IsNull())
                return ResponseData<string>.CreateFail(string.Empty, lastErrorMessage: "MessageDto is null");
            if (value.AppId.IsEmpty())
                return ResponseData<string>.CreateFail(string.Empty,
                    lastErrorMessage: "MessageDto AppId is null");
            if (value.Code.IsEmpty())
                return ResponseData<string>.CreateFail(string.Empty,
                    lastErrorMessage: "MessageDto Code is null");
            if (value.Body.IsNull())
                return ResponseData<string>.CreateFail(string.Empty,
                    lastErrorMessage: "MessageDto Body is null");
            using (_MethodMonitor.New("PubToDbTotal"))
            using (_MethodMonitor.New("PubToDb_{0}_{1}".Fomart(value.AppId, value.Code)))
            {
                MessageBus.PublishToDb(value.Body, value.AppId, value.Code, value.MsgUniqueId, value.Ip);
            }
            return ResponseData<string>.CreateSuccess("ok", "ok");
        }

        [Route("message/publishbatchtodb")]
        [HttpPost]
        public ResponseData<string> PublishBatchToDB([FromBody] MessageBatchDto value)
        {
            if (value.IsNull())
                return ResponseData<string>.CreateFail(string.Empty, lastErrorMessage: "MessageDto is null");
            if (value.AppId.IsEmpty())
                return ResponseData<string>.CreateFail(string.Empty,
                    lastErrorMessage: "MessageDto AppId is null");
            if (value.Code.IsEmpty())
                return ResponseData<string>.CreateFail(string.Empty,
                    lastErrorMessage: "MessageDto Code is null");
            if (value.Items.IsNull())
                return ResponseData<string>.CreateFail(string.Empty,
                    lastErrorMessage: "MessageDto Body is null");
            using (_MethodMonitor.New("PubBatchToDbTotal"))
            using (_MethodMonitor.New("PubBatchToDb_{0}_{1}".Fomart(value.AppId, value.Code)))
            {
                MessageBus.PublishBatchToDb(value.Items, value.AppId, value.Code, value.Ip);
            }
            return ResponseData<string>.CreateSuccess("ok", "ok");
        }

        [Route("message/publishbatch")]
        [HttpPost]
        public ResponseData<string> PulishBatch([FromBody] MessageBatchDto value)
        {
            if (value.IsNull())
                return ResponseData<string>.CreateFail(string.Empty, lastErrorMessage: "MessageDto is null");
            if (value.AppId.IsEmpty())
                return ResponseData<string>.CreateFail(string.Empty,
                    lastErrorMessage: "MessageDto AppId is null");
            if (value.Code.IsEmpty())
                return ResponseData<string>.CreateFail(string.Empty,
                    lastErrorMessage: "MessageDto Code is null");
            if (value.Items.IsEmptyEnumerable())
                return ResponseData<string>.CreateFail(string.Empty, lastErrorMessage: "MessageDto is null");
            using (_MethodMonitor.New("PulishBatchTotal"))
            using (_MethodMonitor.New("PulishBatch_{0}_{1}".Fomart(value.AppId, value.Code)))
            {
                MessageBus.PublishBatch(value.Items, value.AppId, value.Code, value.Ip);
            }
            return ResponseData<string>.CreateSuccess("ok", "ok");
        }       
        [Route("message/publishbatchasync")]
        [HttpPost]
        public async Task<ResponseData<string>> PulishBatchAsync([FromBody] MessageBatchDto value)
        {
            if (value.IsNull())
                return ResponseData<string>.CreateFail(string.Empty, lastErrorMessage: "MessageDto is null");
            if (value.AppId.IsEmpty())
                return ResponseData<string>.CreateFail(string.Empty,
                    lastErrorMessage: "MessageDto AppId is null");
            if (value.Code.IsEmpty())
                return ResponseData<string>.CreateFail(string.Empty,
                    lastErrorMessage: "MessageDto Code is null");
            if (value.Items.IsEmptyEnumerable())
                return ResponseData<string>.CreateFail(string.Empty, lastErrorMessage: "MessageDto is null");
            using (_MethodMonitor.New("PulishBatchAsyncTotal"))
            using (_MethodMonitor.New("PulishBatchAsync_{0}_{1}".Fomart(value.AppId, value.Code)))
            {
                await MessageBus.PublishBatchAsync(value.Items, value.AppId, value.Code, value.Ip).ConfigureAwait(false);
            }
            return ResponseData<string>.CreateSuccess("ok", "ok");
        }
        [Route("message/viewcfg")]
        [HttpGet]
        public Dictionary<string, MQMainConfiguration> ViewCfg(string value = null)
        {
            var cfg = MQMainConfigurationManager.Builder.GetConfiguration();
            cfg["serverip"] = new MQMainConfiguration {AppId = _Utils.GetLocalHostIp()};
            return cfg;
        }
    }
}
