using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace YmatouMQNet4.Utils
{
    public struct MethodMonitor : IDisposable
    {
        private Stopwatch watch;
        private string des;
        private TimeSpan ts;
        private ILog log;
        private double gtLimitMilliseconds;
        public MethodMonitor(ILog _log, double _gtLimitMilliseconds = 3000, string descript = null)
        {
            watch = Stopwatch.StartNew();
            des = descript;
            ts = TimeSpan.MinValue;
            log = _log;
            gtLimitMilliseconds = _gtLimitMilliseconds;
        }
        public void Run(Action action, Action<TimeSpan> time)
        {
            Stopwatch watch = Stopwatch.StartNew();
            action();
            watch.Stop();
            time(watch.Elapsed);
        }
        public T Run<T>(Func<T> action, Action<TimeSpan> time)
        {
            Stopwatch watch = Stopwatch.StartNew();
            var r = action();
            watch.Stop();
            time(watch.Elapsed);
            return r;
        }

        public void Dispose()
        {
            //TODO:计量时间 
            ts = watch.Elapsed;
            watch.Stop();
            if (ts.TotalMilliseconds > gtLimitMilliseconds)
                log.Debug("方法 {0} 执行耗时 {1} 毫秒", des, ts.TotalMilliseconds);
        }
    }
}
