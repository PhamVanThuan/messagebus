using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace YmatouMQ.Common
{
    public static class MQThreadPool
    {
        public static bool SetThreadPoolMaxThreads(int maxWorkThread, int ioThread)
        {
            return ThreadPool.SetMaxThreads(maxWorkThread, ioThread);
        }
        public static bool WaitAvailableThreads(int millisecondsTimeout = 5000)
        {
            int workTh = GetAvailableThreads();
            if (workTh > 0) return true;
            Thread.Sleep(millisecondsTimeout);
            return GetAvailableThreads() > 0;
        }

        private static int GetAvailableThreads()
        {
            int workTh, ioTh;
            ThreadPool.GetAvailableThreads(out workTh, out ioTh);
            return workTh;
        }
        public static Tuple<int, int> GetAvailableWorkThreadsAndIoThread()
        {
            int workTh, ioTh;
            ThreadPool.GetAvailableThreads(out workTh, out ioTh);
            return Tuple.Create(workTh, ioTh);
        }
    }
}
