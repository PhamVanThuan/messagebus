using System;
using System.Threading.Tasks;
using System.Threading;
using YmatouMQNet4.Utils;
using YmatouMQ.Common;

namespace YmatouMQNet4.Core
{
    /// <summary>
    /// 事件处理
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IMessageHandler<TMessage>
    {
        /// <summary>
        /// 消息处理回调
        /// </summary>
        /// <param name="msgContext"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<ResponseData<ResponseNull>> Handle(MessageHandleContext<TMessage> msgContext);
    }
}
