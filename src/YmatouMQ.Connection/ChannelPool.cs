using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using System.Threading;
using System.Collections.Concurrent;
using RabbitMQ.Client.Framing.Impl;
using YmatouMQ.Common;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Utils;
using YmatouMQ.Log;

namespace YmatouMQ.Connection
{
    /// <summary>
    /// channel struct
    /// </summary>
    public class ChannelStruct
    {
        public IModel channel { get; private set; }
        internal TimeSpan idleTimeOut { get; private set; }
        internal DateTime createTime { get; private set; }
        internal bool busy { get; private set; }

        public ChannelStruct(IModel channel, TimeSpan idelTimeOut, bool isbusy = false)
        {
            this.channel = channel;
            this.idleTimeOut = idelTimeOut;
            this.busy = isbusy;
            this.createTime = DateTime.Now;
        }
        public void SetIdleTimeOut(TimeSpan timeOut)
        {
            this.idleTimeOut = timeOut;
        }
        public void SetBusy()
        {
            this.busy = true;
        }
        public void SetIdle()
        {
            this.busy = false;
        }
    }
    /// <summary>
    /// rabbitmq channel pool
    /// </summary>
    public class ChannelPool : DisposableObject
    {       
        private readonly ObjectPool<ChannelStruct> pool = new ObjectPool<ChannelStruct>();        
        private readonly ILog log = LogFactory.GetLogger(LogFactory._LogType, "YmatouMQ.Connection.ChannelPool");
      
        private readonly int maxChannel;
        private Timer checnkStatustimer;   
        private readonly TimeSpan channelIdleTimeOut;
        public ChannelPool(int maxChannel, TimeSpan channelIdleTimeOut)
        {         
            this.maxChannel = maxChannel;
            this.channelIdleTimeOut = channelIdleTimeOut;
            log.Info("[ChannelPool] ctor call done.");
        }
        public void CreateChannel(AutorecoveringConnection connectionInfo,int minChannel)
        {
            for (var i = 0; i < minChannel; i++)
            {
                NewChannel(connectionInfo, channelIdleTimeOut);
            }
            log.Info("[InitChannelPool] ok,host:{0}",connectionInfo.Endpoint.HostName);
        }
        public ChannelStruct GetChannelStruct(Func<AutorecoveringConnection> connFunc)
        {
            ChannelStruct channel;
            if (pool.TryDequeue(out channel))
            {
                channel.SetBusy();
            }
            else
            {               
                NewChannel(connFunc(), channelIdleTimeOut);
                if (pool.TryDequeue(out channel))
                    log.Info("[ChannelPool] no get channel,new channel success.");
            }
            return channel;
        }

        public int Count {
            get { return pool.Count; }
        }

        public void Free(ChannelStruct channelStruct)
        {
            channelStruct.SetIdle();
            pool.Enqueue(channelStruct);
        }
        public void Maintain()
        {
          //TODO 维护channel
        }
        public void Stop()
        {
           //TODO 释放资源
           
        }
        protected override void InternalDispose()
        {
           //TODO
        }
        private void NewChannel(AutorecoveringConnection connectionInfo, TimeSpan channelIdleTimeOut)
        {
            var channel = TryCreateModel(connectionInfo);
            if (channel != null)
            {
                var channelStruct = new ChannelStruct(channel, channelIdleTimeOut);
                pool.Enqueue(channelStruct);
            }
        }
        private IModel TryCreateModel(IConnection connectionInfo)
        {
            try
            {
                return connectionInfo.CreateModel();
            }
            catch (Exception ex)
            {
                log.Error("[ChannelPool] TryCreateModel exception", ex);
                return null;
            }
        }
    }
}
