using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQNet4.Core.Publish;
using YmatouMQNet4.Utils;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common;
using YmatouMQ.Log;
using YmatouMQ.Common.Utils;

namespace YmatouMQNet4.Core
{
    internal class PublishBufferActionAsync : PublishMessageBase
    {
        private readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile,
            "YmatouMQNet4.Core.PublishBufferActionAsync");

        private readonly ConcurrentDictionary<string, BufferActionBlockWrapper<PublishMessageContextAsync>> buffer =
            new ConcurrentDictionary<string, BufferActionBlockWrapper<PublishMessageContextAsync>>();

        public override async Task PublishMessageAsync(PublishMessageContextAsync message)
        {
            try
            {
                var bufferAction = buffer.GetOrAdd(message.context.appid,
                    new BufferActionBlockWrapper<PublishMessageContextAsync>(data => Handle(data)));
                await bufferAction.PostAsync(message).ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                ex.Handle(log,
                    "[PublishBufferActionAsync] AggregateException_{0}_{1}".Fomart(message.context.appid,
                        message.context.code));
            }
            catch (Exception ex)
            {
                ex.Handle(log,
                    "[PublishBufferActionAsync] Exception_{0}_{1}".Fomart(message.context.appid, message.context.code));
            }
        }

        private async Task Handle(PublishMessageContextAsync context)
        {
            await context.publishproxy
                .PublishMessageAsync(context)
                .ConfigureAwait(false);
        }

        public void Stop()
        {
            buffer.EachAction(e => e.Value.Complete());
        }

        public override void PublishMessage(PublishMessageContextSync message)
        {
            throw new NotImplementedException();
        }
    }
}
