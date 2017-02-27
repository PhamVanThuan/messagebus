using System;
using System.Collections.Concurrent;

namespace YmatouMQ.Connection
{
    /// <summary>
    /// object pool
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ObjectPool<T> : ConcurrentQueue<T>
    {

    }
}
