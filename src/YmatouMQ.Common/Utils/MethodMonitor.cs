using System;
using System.Collections.Generic;
using System.Diagnostics;
using YmatouMQ.Common;

namespace YmatouMQ.Common.Utils
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
            watch.Stop();
            var total = watch.ElapsedMilliseconds;
            if (watch.Elapsed.TotalMilliseconds > gtLimitMilliseconds && log != null)
                log.Info("{0} run {1:N0} ms", des, total);
        }

        public TimeSpan GetRunTime
        {
            get
            {
                watch.Stop();
                return watch.Elapsed;
            }
        }

        public long GetRunTime2
        {
            get
            {
                watch.Stop();
                return watch.ElapsedMilliseconds;
            }
        }
    }
}
