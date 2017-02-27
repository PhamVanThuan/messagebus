using System;
using YmatouMQ.Common;

namespace YmatouMQ.Common.Extensions
{
    public static class ExceptionExtensions
    {
        public static void Handle(this AggregateException ex, ILog log, string description = null)
        {
            if (ex == null || log == null) return;
            foreach (var e in ex.InnerExceptions)
            {
                log.Error("{0},{1}", description, e.ToString());
            }
        }
        public static void Handle(this AggregateException ex, ILog log, string formart, params object[] args)
        {
            if (ex == null || log == null) return;
            foreach (var e in ex.InnerExceptions)
            {
                var msg = !string.IsNullOrEmpty(formart) ? string.Format(formart, args) : null;
                log.Error("{0},{1}", msg, e.ToString());
            }
        }

        public static void Handle(this Exception ex, ILog log, string description = null)
        {
            if (ex == null || log == null) return;

            log.Error("{0},{1}", description, ex.ToString());
        }

        public static void Handle(this Exception ex, ILog log, string formart, params object[] args)
        {
            if (ex == null || log == null) return;
            var msg = string.Format(formart, args);
            log.Error("{0},{1}", msg, ex.ToString());
        }
    }
}
