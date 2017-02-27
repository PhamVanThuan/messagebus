using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using YmatouMQNet4.Core;
using YmatouMQNet4.Dto;
using YmatouMQNet4.Extensions;

namespace YmatouMQServer.Controllers
{
    public class MessageController : ApiController
    {
        [Route("bus/Message")]
        [HttpGet]
        public ResponseData<ResponseNull> Get(string appid = null, string code = null)
        {
            if (appid.IsEmpty())
                return ResponseData<ResponseNull>.CreateFail(ResponseNull._Null, lastErrorMessage: "AppId is null");
            if (code.IsEmpty())
                return ResponseData<ResponseNull>.CreateFail(ResponseNull._Null, lastErrorMessage: "Code is null");

            return ResponseData<ResponseNull>.CreateSuccess(ResponseNull._Null, "ok");
        }

        [Route("bus/Message/")]
        public async Task<ResponseData<ResponseNull>> Post([FromBody]MessageDto value)
        {
            if (value.IsNull())
                return await ResponseData<ResponseNull>.CreateFailTask(ResponseNull._Null, lastErrorMessage: "MessageDto is null");
            if (value.AppId.IsEmpty())
                return await ResponseData<ResponseNull>.CreateFailTask(ResponseNull._Null, lastErrorMessage: "MessageDto AppId is null");
            if (value.Code.IsEmpty())
                return await ResponseData<ResponseNull>.CreateFailTask(ResponseNull._Null, lastErrorMessage: "MessageDto Code is null");
            if (value.Body.IsNull())
                return await ResponseData<ResponseNull>.CreateFailTask(ResponseNull._Null, lastErrorMessage: "MessageDto Body is null");

            await MessageBus.PublishAsync(value.Body, value.AppId, value.Code, value.MsgUniqueId).ConfigureAwait(false);
            //MessageBus.Publish(value.Body, value.AppId, value.Code, value.MsgUniqueId);

            return await ResponseData<ResponseNull>.CreateSuccessTask(ResponseNull._Null, "ok");
        }
    }
}