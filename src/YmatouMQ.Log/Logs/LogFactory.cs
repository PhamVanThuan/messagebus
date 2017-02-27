using System;
using System.Collections.Generic;
using YmatouMQ.Common;
using YmatouMQNet4.Logs;

namespace YmatouMQ.Log
{
    public class LogFactory
    {
        private static readonly Dictionary<LogEngineType, Func<string, ILog>> dic = new Dictionary<LogEngineType, Func<string, ILog>>();

        static LogFactory()
        {
            dic.Add(LogEngineType.Console, f => new ConsoleLog(f));
            dic.Add(LogEngineType.RealtimelWriteFile, f =>
            {
                return new GeneralFileLog(f);
            });
            dic.Add(LogEngineType.Null, f => new NullLog());
            dic.Add(LogEngineType.BatchWriteFile, f =>
            {
                return new GeneralFileLog(f);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="logTypeFullName"></param>
        /// <returns></returns>
        public static ILog GetLogger(LogEngineType logType, string logTypeFullName = null)
        {
            return dic[logType](logTypeFullName);
        }
        /// <summary>
        /// 日志引擎类型
        /// </summary>
        public static LogEngineType _LogType
        {
            get
            {
                return LogEngineType.RealtimelWriteFile;
            }
        }
    }
    /// <summary>
    /// 日志类型
    /// </summary>
    [Serializable]
    public enum LogEngineType
    {
        /// <summary>
        /// 控制台
        /// </summary>
        Console = 1,
        /// <summary>
        /// 定时批量写入文件
        /// </summary>
        BatchWriteFile = 2,
        /// <summary>
        /// 实时写入文件
        /// </summary>
        RealtimelWriteFile = 3,
        /// <summary>
        /// 空日志（适合性能测试）
        /// </summary>
        Null = 4
    }
}
