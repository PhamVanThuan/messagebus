using System;

namespace YmatouMessageBusClientNet4.Dto
{
    /// <summary>
    /// MQ 发布消息结构
    /// </summary>
    public class PulbishMessageDto
    {
        /// <summary>
        /// 应用id 
        /// </summary>
        public string appid { get; set; }
        /// <summary>
        /// 业务id 
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 消息id 
        /// </summary>
        public string messageid { get; set; }
        /// <summary>
        /// 消息体
        /// </summary>
        public object body { get; set; }
        /// <summary>
        /// ip （可选）
        /// </summary>
        public string ip { get; set; }
        /// <summary>
        /// 请求路径（可选，默认 /message/publish）
        /// </summary>
        public string requestpath { get; set; }
    }
}
