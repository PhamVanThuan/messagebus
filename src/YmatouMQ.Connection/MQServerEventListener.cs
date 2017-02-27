using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using YmatouMQNet4.Utils;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Extensions._Task;
using YmatouMQ.Common;
using YmatouMQ.Log;
using RabbitMQ.Client.Framing.Impl;

namespace YmatouMQ.Connection
{
    /// <summary>
    /// 对MQ服务一系列事件监听
    /// </summary>
    internal class MQServerEventListener
    {
        private readonly ILog log = LogFactory.GetLogger(LogFactory._LogType, "YmatouMQ.Connection.ConnectionRecovery");
        public readonly IConnRecoveryNotify notify;
        public readonly IConnShutdownNotify shutdownNotify;
        public readonly AutorecoveringConnection conn;
        public readonly string appId;
        private Stopwatch watchBlocked;
        private Stopwatch watchConn;

        public MQServerEventListener(AutorecoveringConnection conn, IConnRecoveryNotify notify, string appId,IConnShutdownNotify shutdownNotify=null)
        {
            this.conn = conn;
            this.notify = notify;
            this.shutdownNotify = shutdownNotify;
            this.appId = appId;
            //注册事件监听
            RegisterMQServerEvent();
            log.Info("appId{0}，已注册MQServer事件监听", appId);
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
            this.conn.Recovery -= conn_Recovery;
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
            this.conn.Recovery += conn_Recovery;
        }

        void conn_Recovery(object sender, EventArgs e)
        {
            watchConn.Stop();
            notify.Notify(appId,(sender  as AutorecoveringConnection).CreateModel());
            log.Debug("appid:{0},sender:{1} 连接已恢复,断开 {2} ms",appId, sender.ToString(),watchConn.ElapsedMilliseconds);
        }

        void conn_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            //忽略appId程序正常关闭链接
            if (e.Initiator == ShutdownInitiator.Application) return;
            log.Error("appId {0} 链接 {1} 断开,原因 {2},{3},{4}", appId, (sender as IConnection).Endpoint.HostName, e.Initiator, e.ReplyCode, e.ReplyText);
//            ListenerConnRecovery(conn);
//            log.Debug("appId {0}启动链接恢复尝试", appId);
            watchConn = Stopwatch.StartNew();
            if (this.shutdownNotify != null) this.shutdownNotify.Notify();
        }

        void conn_ConnectionUnblocked(object sender, EventArgs e)
        {
            watchBlocked.Stop();
            log.Error("appId {0} 链接 {1} 阻塞解除，阻塞{2}秒", appId, (sender as IConnection).Endpoint.HostName, watchBlocked.Elapsed.TotalSeconds);
        }

        void conn_ConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            log.Error("appId {0} 链接 {1} 阻塞 {2}", appId, (sender as IConnection).Endpoint.HostName, e.Reason);
            watchBlocked = Stopwatch.StartNew();            
        }

        void Conn_CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            log.Error("appId {0} MQNET 回调异常 {1}", appId, e.Exception);
        }
         
        private void ListenerConnRecovery(AutorecoveringConnection conn)
        {
            if (notify == null)
            {
                log.Debug("appId{0}未注册链接恢复回调", appId);
                return;
            }

            Task.Factory.StartNew(() =>
            {
                var cancelSource = new CancellationTokenSource();
                var token = cancelSource.Token;
                while (!token.IsCancellationRequested)
                {
                    if (token.IsCancellationRequested)
                        //token.ThrowIfCancellationRequested();
                        break;
                    if (conn.IsOpen)
                    {
                        watchConn.Stop();
                        log.Debug("appid {0},链接已恢复,断开 {1}，执行恢复通知", appId, watchConn.Elapsed);

                        notify.Notify(appId, conn.CreateModel());

                        //已执行完成通知，则取消
                        cancelSource.Cancel();
                    }
                    else
                    {
                        log.Info("appid {0},链接未恢复,断开 {1}", appId, watchConn.Elapsed);
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
            })
            .WithHandleException(log,"链接恢复监听事件异常,appId {0}", appId);
        }
    }
}
