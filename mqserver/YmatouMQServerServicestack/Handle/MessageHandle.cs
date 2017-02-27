using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQNet4.Core;
using YmatouMQNet4.Dto;
using YmatouMQNet4.Extensions;

namespace YmatouMQServerServicestack.Handle
{
    public class MessageHandle : ServiceStack.Service
    {
        public async Task<object> Post(MessageDto request)
        {
            if (request.IsNull())
                return await ResponseData<ResponseNull>.CreateFailTask(ResponseNull._Null, lastErrorMessage: "MessageDto is null");
            if (request.AppId.IsEmpty())
                return await ResponseData<ResponseNull>.CreateFailTask(ResponseNull._Null, lastErrorMessage: "MessageDto AppId is null");
            if (request.Code.IsEmpty())
                return await ResponseData<ResponseNull>.CreateFailTask(ResponseNull._Null, lastErrorMessage: "MessageDto Code is null");
            if (request.Body.IsNull())
                return await ResponseData<ResponseNull>.CreateFailTask(ResponseNull._Null, lastErrorMessage: "MessageDto Body is null");

            await MessageBus.PublishAsync(request.Body, request.AppId, request.Code, request.MsgUniqueId).ConfigureAwait(false);
            // await MessageBus.Publish(request.Body, request.AppId, request.Code, request.MsgUniqueId);

            return await ResponseData<ResponseNull>.CreateSuccessTask(ResponseNull._Null, "ok");
        }
    }
}
