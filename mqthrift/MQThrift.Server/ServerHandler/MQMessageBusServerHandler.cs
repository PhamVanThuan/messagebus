using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQMessageProtocols;

namespace MQThrift.Server.ServerHandler
{
    public class MQMessageBusServerHandler : _MQMessageProtocols.Iface
    {
        public string testString(string val)
        {
            return string.Format("welcome mq bus application v1.0 ,now" + DateTime.Now.ToString());
        }

        public IAsyncResult Begin_testString(AsyncCallback callback, object state, string val)
        {
            throw new NotImplementedException();
        }

        public string End_testString(IAsyncResult asyncResult)
        {

            throw new NotImplementedException();
        }

        public async Task<string> testStringAsync(string val)
        {
            return await Task.Factory.StartNew(() => string.Format("welcome mq bus application v1.0 ,now" + DateTime.Now.ToString()));
        }

        public Response publish(MessageDto dto)
        {
            var s = YmatouMQNet4.Core.MessageBus.PublishAsync(dto.Body, dto.AppId, dto.Code, dto.MsgUniqueId);
            return new Response { Code = 200, Message = "ok" };
        }

        public IAsyncResult Begin_publish(AsyncCallback callback, object state, MessageDto dto)
        {
            throw new NotImplementedException();
        }

        public Response End_publish(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public async Task<Response> publishAsync(MessageDto dto)
        {
            await YmatouMQNet4.Core.MessageBus.PublishAsync(dto.Body, dto.AppId, dto.Code, dto.MsgUniqueId);
            return await Task.Factory.StartNew(() => new Response { Code = 200, Message = "ok" });
        }
    }
}
