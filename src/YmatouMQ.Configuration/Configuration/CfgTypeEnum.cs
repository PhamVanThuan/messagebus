using System;

namespace YmatouMQNet4.Configuration
{
    /// <summary>
    /// 配置来源类型
    /// </summary>
    public enum CfgTypeEnum : byte
    {
        LocalMemory = 0,
        LocalDisk = 1,
        Server = 2
    }
    public enum CfgUpdateType : byte
    {
        /// <summary>
        /// （原来的链接）链接字符窜修改
        /// </summary>
        ConnStringModify = 0,
        /// <summary>
        /// （原来的链接）增加链接数
        /// </summary>
        ConnNumAddModify = 1,
        /// <summary>
        /// （原来的链接）减少链接
        /// </summary>
        ConnNumDecrement = 2,
        /// <summary>
        /// （原来的链接）重新构建链接池
        /// </summary>
        ConnRinit = 3,
        /// <summary>
        /// 增加新的链接
        /// </summary>
        AddNewConn = 4,
    }
}
