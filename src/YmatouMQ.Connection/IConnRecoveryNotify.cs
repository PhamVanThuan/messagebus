using System;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace YmatouMQ.Connection
{
    /// <summary>
    /// 链接恢复通知
    /// </summary>
    public interface IConnRecoveryNotify
    {
        /// <summary>
        /// 链接恢复后处理事件
        /// </summary>
        /// <param name="appId">应用ID</param>
        /// <param name="channel">MQServer channel</param>
        Task Notify(string appId, IModel channel);
    }
    /// <summary>
    /// 链接断开
    /// </summary>
    public interface IConnShutdownNotify
    {
        Task Notify();
    }
}
