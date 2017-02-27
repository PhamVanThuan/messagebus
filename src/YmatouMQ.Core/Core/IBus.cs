using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YmatouMQ.Connection;

namespace YmatouMQNet4.Core
{
    public interface IBus
    {
        void StartBusApplication();
        void StopBusApplication();
    }

    public interface IRabbitMQBus : IBus 
    {
        void Publish(PublishMessageContext messageContext);
        void Publish(IEnumerable<PublishMessageContext> messageContext);
        Task PublishAsync(PublishMessageContext messageContext);
        Task PublishAsync(IEnumerable<PublishMessageContext> messageContex);
        Task PublishBufferAsync(PublishMessageContext messageContext);
        TMessage PullMessage<TMessage>(string appId, string code);
        void PullMessage<TMessage>(string appId, string code, IMessageHandler<TMessage> handle);      
      
        uint MessageCount(string appId, string code);
        IEnumerable<string> GetConnectionPoolKeys { get; }
        bool RemoveCacheExchang(string appid, string code);
        IEnumerable<string> GetChannelsStatus { get; }
        void PublishToDb(PublishMessageContext messageContext);
        void PublishBatchToDb(IEnumerable<PublishMessageContext> messageContext, string appId, string code, string ip);
        BusApplicationStatus BusStatus { get; }  
    }
}
