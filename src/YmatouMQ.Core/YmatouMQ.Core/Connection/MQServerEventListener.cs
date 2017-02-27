using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using YmatouMQNet4.Utils;
using YmatouMQNet4.Extensions;
using YmatouMQNet4.Extensions._Task;

namespace YmatouMQNet4.Connection
{
    /// <summary>
    /// 对MQ服务一系列事件监听
    /// </summary>
    internal class MQServerEventListener
    {
        private static readonly ILog log = LogFactory.GetLogger(LogFactory._LogType, "YmatouMQ.Connection.ConnectionRecovery");
        public readonly IConnRecoveryNotify notify;
        public readonly IConnection conn;
        public readonly string appId;
        private Stopwatch watchBlocked;
        private Stopwatch watchConn;

        public MQServerEventListener(IConnection conn, IConnRecoveryNotify notify, string appId)
        {
            this.conn = conn;
            this.notify = notify;
            this.appId = appId;
            //注册事件监听
            RegisterMQServerEvent();
            log.Info("应用{0}，已注册MQServer事件监听", appId);
        }
        public void UnRegisterMQServerEvent()
        {
            //对conn 作前置条件判断
            YmtSystemAssert.AssertArgumentNotNull(this.conn, "MQ IConnection 取消链接异常监听");
            //注册链接阻塞事件
            this.conn.ConnectionBlocked -= conn_ConnectionBlocked;
            //注册链接解除阻塞事件
            this.conn.ConnectionUnblocked -= conn_ConnectionUnblocked;
            //注册链接中断事件
            this.conn.ConnectionShutdown -= conn_ConnectionShutdown;
            //注册回调异常
            this.conn.CallbackException -= Conn_CallbackException;
        }
        private void RegisterMQServerEvent()
        {
            //对conn 作前置条件判断
            YmtSystemAssert.AssertArgumentNotNull(this.conn, "MQ IConnection 为空不能注册链接监听");
            //注册链接阻塞事件
            this.conn.ConnectionBlocked += conn_ConnectionBlocked;
            //注册链接解除阻塞事件
            this.conn.ConnectionUnblocked += conn_ConnectionUnblocked;
            //注册链接中断事件
            this.conn.ConnectionShutdown += conn_ConnectionShutdown;
            //注册回调异常
            this.conn.CallbackException += Conn_CallbackException;
        }

        void conn_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            //忽略应用程序正常关闭链接
            if (e.Initiator == ShutdownInitiator.Application) return;
            log.Error("应用 {0} 链接 {1} 断开,原因 {2},serverCode:{3}", appId, (sender as IConnection).Endpoint.HostName, e.ReplyText, e.ReplyCode);
            ListenerConnRecovery(conn);
            log.Debug("应用 {0}启动链接恢复尝试", appId);
            watchConn = Stopwatch.StartNew();
        }

        void conn_ConnectionUnblocked(object sender, EventArgs e)
        {
            watchBlocked.Stop();
            log.Error("应用 {0} 链接 {1} 阻塞解除，阻塞{2}秒", appId, (sender as IConnection).Endpoint.HostName, watchBlocked.Elapsed.TotalSeconds);
        }

        void conn_ConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            log.Error("应用 {0} 链接 {1} 阻塞 {2}", appId, (sender as IConnection).Endpoint.HostName, e.Reason);
            watchBlocked = Stopwatch.StartNew();
        }

        void Conn_CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            log.Error("应用 {0} MQNET 回调异常 {1}", appId, e.Exception);
        }

        void Conn_ConnectionShutdown(IConnection connection, ShutdownEventArgs reason)
        {
            //忽略应用程序正常关闭链接
            if (reason.Initiator == ShutdownInitiator.Application) return;
            log.Error("应用 {0} 链接 {1} 断开,原因 {2},{3},{4}", appId, connection.Endpoint.HostName, reason.Initiator, reason.ReplyCode, reason.ReplyText);
            ListenerConnRecovery(conn);
            log.Debug("应用 {0}启动链接恢复尝试", appId);
            watchConn = Stopwatch.StartNew();
        }

        void Conn_ConnectionUnblocked(IConnection sender)
        {
            watchBlocked.Stop();
            log.Error("应用 {0} 链接 {1} 阻塞解除，阻塞{2}秒", appId, sender.Endpoint.HostName, watchBlocked.Elapsed.TotalSeconds);
        }

        void Conn_ConnectionBlocked(IConnection sender, ConnectionBlockedEventArgs args)
        {
            log.Error("应用 {0} 链接 {1} 阻塞 {2}", appId, sender.Endpoint.HostName, args.Reason);
            watchBlocked = Stopwatch.StartNew();
        }

        private void ListenerConnRecovery(IConnection conn)
        {
            if (notify == null)
            {
                log.Warning("应用{0}未注册链接恢复回调", appId);
                return;
            }
            var cancelSource = new CancellationTokenSource();
            var token = cancelSource.Token;

            Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (token.IsCancellationRequested)
                        token.ThrowIfCancellationRequested();
                    if (conn.IsOpen /*&& notify != null*/)
                    {
                        watchConn.Stop();
                        log.Debug("链接已恢复,断开 {0} 秒，执行恢复通知", watchConn.Elapsed.TotalSeconds);

                        notify.Notify(appId, conn.CreateModel()).WithHandleException("{1},{0}", appId, "链接恢复重新发送消息异常");

                        //已执行完成通知，则取消
                        cancelSource.Cancel();
                    }
                    //SpinWait.SpinUntil(() => true, 500);
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current)
            .WithHandleException("链接恢复监听事件异常,应用 {0}", appId);
        }
    }
}
