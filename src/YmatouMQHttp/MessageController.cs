using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using YmatouMQNet4.Core;
using YmatouMQNet4.Dto;
using YmatouMQNet4.Extensions;

namespace YmatouMQHttp
{   
    public class MessageController : ApiController
    {       
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
            ////MessageBus.Publish(value.Body, value.AppId, value.Code, value.MsgUniqueId);
             
            return await ResponseData<ResponseNull>.CreateSuccessTask(ResponseNull._Null, "ok");
        }
    }
}
