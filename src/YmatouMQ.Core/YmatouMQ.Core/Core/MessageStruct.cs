using System;
using RabbitMQ.Client;
using System.Threading.Tasks;

namespace YmatouMQNet4.Core
{
    public struct PublishMessageContext
    {
        public string appid { get; set; }
        public string code { get; set; }
        public string messageid { get; set; }
        public string ip { get; set; }
        public object body { get; set; }
    }
    public struct PublishMessageContextSync
    {
        public PublishMessageContext context { get; set; }
        public IModel channel { get; set; }
    }
    public struct PublishMessageContextAsync
    {
        public PublishMessageContext context { get; set; }
        public Func<Task<IModel>> channel { get; set; }
        //public Func<IModel> channel { get; set; }
    }
}
