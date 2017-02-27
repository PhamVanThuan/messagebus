using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQNet4.Core.Publish;
using YmatouMQNet4.Utils;
using YmatouMQNet4.Extensions;

namespace YmatouMQNet4.Core
{
    internal class PublishBufferActionAsync //: PublishMessageBase
    {
        private readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQNet4.Core.PublishBufferActionAsync");
        private readonly PublishMessageBase publish;
        private readonly ConcurrentDictionary<string, BufferActionBlockWrapper<PublishMessageContextAsync>> buffer = new ConcurrentDictionary<string, BufferActionBlockWrapper<PublishMessageContextAsync>>();
        public PublishBufferActionAsync(PublishMessageBase publish)
        {
            this.publish = publish;
        }
        public async Task PublishMessageAsync(PublishMessageContextAsync message)
        {
            try
            {
                var key = "{0}_{1}".Fomart(message.context.appid, message.context.code);
                var bufferAction = buffer.GetOrAdd(key, new BufferActionBlockWrapper<PublishMessageContextAsync>((async data => await Handle(data))));
                await bufferAction
                                .PostAsync(message)
                                .ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                ex.Handle(log, "PublishBufferActionAsync AggregateException_{0}_{1}".Fomart(message.context.appid, message.context.code));
            }
            catch (Exception ex)
            {
                ex.Handle(log, "PublishBufferActionAsync Exception_{0}_{1}".Fomart(message.context.appid, message.context.code));
            }
        }

        private async Task Handle(PublishMessageContextAsync data)
        {
            await publish
                        .PublishMessageAsync(data)
                       // .ContinueWith(e => e.Exception.Handle(log, "发送消息(Handle)异常 {0} {1}".Fomart(data.context.appid, data.context.code)), TaskContinuationOptions.OnlyOnFaulted)
                        .ConfigureAwait(false);
        }

        public void Stop()
        {
            buffer.EachAction(e => e.Value.Complete());
        }
    }
}
