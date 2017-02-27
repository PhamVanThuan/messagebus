using System;

namespace YmatouMQNet4.Core
{
    public enum Status
    {
        Normal = 0,
        Exception = 1,
        MemoryQueueGtLimit = 2,
        HandleException = 3,
        HandleSuccess = 4
    }
}
