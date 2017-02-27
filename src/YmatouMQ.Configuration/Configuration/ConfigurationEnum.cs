using System;

namespace YmatouMQNet4.Configuration
{
    /// <summary>
    /// 交换机类型
    /// </summary>
    public enum ExchangeType
    {
        /// <summary>
        /// 直接
        /// </summary>
        direct = 2,
        /// <summary>
        /// 广播
        /// </summary>
        fanout = 1,

        /// <summary>
        /// 根据规则组合
        /// </summary>
        topic = 3,
        /// <summary>
        /// 
        /// </summary>
        headers = 4
    }

    //public class ExchangeTypeNameConst
    //{
    //    public const string Direct = "direct";
    //    public const string Fanout = "fanout";
    //    public const string Headers = "headers";
    //    public const string Topic = "topic";
    //}
}
