using System;
using YmatouMQNet4.Core.Publish;

namespace YmatouMQNet4.Core
{
    internal class PublishMessageFactory
    {
        public static PublishMessageBase Context(PublishMessageType type)
        {
            if (type == PublishMessageType.Sync) return new _PublishMessageSync();
            return new _PublishMessageAsync();
        }
    }

    internal enum PublishMessageType
    {
        Sync = 0,
        Async = 1,
        BufferAsync = 2
    }
}
