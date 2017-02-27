using System;
using Ymatou.CommonService;
using YmatouMessageBusClientNet4.Persistent;
using YmatouMessageBusClientNet4.Extensions;

namespace YmatouMessageBusClientNet4
{
    public class MessageBusAgentBootStart
    {
        private MessageBusAgentBootStart() { }
        private static MessageBusAgentStatus status;
        public static MessageBusAgentStatus Status { get { return status; } }
        /// <summary>
        /// 初始化BusAgentService
        /// </summary>
        public static void TryInitBusAgentService()
        {
            if (status == MessageBusAgentStatus.Runing) return;
            try
            {
                status = MessageBusAgentStatus.NoInit;
                MessageBusClientCfg.Instance.LoadCfg();
                ApplicationLog.Debug("消息总线加载配置文件完成，成功？ {0}".F(MessageBusClientCfg.Instance.LoadConfigurationOk));
                WebRequestWrap.SetConnectionLimit(MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.busHttpConnectionLimit));
                //JournalFactory.MessageSendLogBuilder.Init();
                JournalFactory.MessageLocalJournalBuilder.Init();
                status = MessageBusAgentStatus.Runing;
                ApplicationLog.Debug("MessageBusAgent start...");
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
            if (status != MessageBusAgentStatus.Runing) return;
            try
            {
                ApplicationLog.Debug("MessageBusAgent stop...");
                JournalFactory.MessageLocalJournalBuilder.TryCloseJournal();
                //JournalFactory.MessageSendLogBuilder.TryCloseJournal();
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
