using System;

namespace YmatouMQNet4.Core
{
    /// <summary>
    /// BUS 应用程序状态
    /// </summary>
    public enum BusApplicationStatus
    {
        /// <summary>
        /// 未启动
        /// </summary>
        NotStart = 0,
        /// <summary>
        /// 启动中
        /// </summary>
        Starting = 1,
        /// <summary>
        /// 运行中
        /// </summary>
        Runing = 2,
        /// <summary>
        /// 停止中
        /// </summary>
        Stoping = 3,
        /// <summary>
        /// 已停止
        /// </summary>
        Stop = 4
    }
}
