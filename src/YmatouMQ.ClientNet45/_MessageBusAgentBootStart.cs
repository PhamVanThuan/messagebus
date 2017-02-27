using System;
using Ymatou.CommonService;

namespace YmatouMQ.ClientNet45
{
    public class _MessageBusAgentBootStart
    {
        private _MessageBusAgentBootStart() { }
        private static MessageBusAgentStatus status;
        public static MessageBusAgentStatus Status { get { return status; } }
        /// <summary>
        /// 初始化BusAgentService
        /// </summary>
        public static void TryInitBusAgentService()
        {
            try
            {
                status = MessageBusAgentStatus.NoInit;
                MessageBusClientCfg.Instance.LoadCfg();
                ApplicationLog.Debug("消息总线加载配置文件完成，成功？ {0}".F(MessageBusClientCfg.Instance.LoadConfigurationOk));
                //WebRequestWrap.SetConnectionLimit(MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.busHttpConnectionLimit));
                _MessageLocalJournal.Instance.Init();
                status = MessageBusAgentStatus.Runing;
                ApplicationLog.Debug("MessageBusAgent start...ok");
            }
            catch (Exception ex)
            {
                status = MessageBusAgentStatus.StartFail;
                ApplicationLog.Error("消息总线启动异常", ex);
            }
        }
        /// <summary>
        /// 停止
        /// </summary>
        public static void TryStopBusAgentService()
        {
            try
            {
                ApplicationLog.Debug("MessageBusAgent stop...ok");
                _MessageLocalJournal.Instance.TryCloseJournal();
                status = MessageBusAgentStatus.Stoped;
            }
            catch (Exception ex)
            {
                ApplicationLog.Error("消息总线停止异常", ex);
            }
        }
    }
    public enum MessageBusAgentStatus
    {
        NoInit = 0,
        StartFail = 1,
        Runing = 2,
        Stoped = 3
    }
}
