using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YmatouMQNet4.Connection;

namespace YmatouMQNet4.Core
{
    public interface IBus : IConnRecoveryNotify
    {
        void Publish(PublishMessageContext messageContext);
        void Publish(IEnumerable<PublishMessageContext> messageContext);
        Task PublishAsync(PublishMessageContext messageContext);
        IEnumerable<Task> PublishAsync(IEnumerable<PublishMessageContext> messageContext);
        TMessage PullMessage<TMessage>(string appId, string code);
        void PullMessage<TMessage>(string appId, string code, IMessageHandler<TMessage> handle);        
        void StartBusApplication();
        void StopBusApplication();
        uint MessageCount(string appId, string code);
    }
}
