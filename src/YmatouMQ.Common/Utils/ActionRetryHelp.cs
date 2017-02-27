using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YmatouMQ.Common.Utils
{
    /// <summary>
    /// 操作重试
    /// </summary>
    public class ActionRetryHelp
    {
        /// <summary>
        /// 方法重试。说明：方法至少执行一次，如果行异常时，则进入重试逻辑。
        /// </summary>
        /// <param name="action">action</param>
        /// <param name="retrycount">重试次数</param>
        /// <param name="retryTime">重试间隔时间</param>
        /// <param name="exceptionAction">发生异常时操作</param>
        /// <param name="errorHandle">异常消息处理</param>
        /// <param name="gtRetrycountAction">超过重试次数后且任然失败的操作</param>
        public static void Retry(Action action, uint retrycount, TimeSpan retryTime, Action exceptionAction = null, Action<Exception> errorHandle = null, Action gtRetrycountAction = null)
        {
            bool isException = false;
            int count = 0;
            //SpinWait wait = new SpinWait();
            do
            {
                try
                {
                    isException = false;
                    action();
                }
                catch (Exception ex)
                {
                    isException = true;
                    if (exceptionAction != null)
                    {
                        try
                        {
                            exceptionAction();
                            isException = false;
                            return;
                        }
                        catch (Exception _ex)
                        {
                            if (errorHandle != null)
                                errorHandle(_ex);
                        }
                    }
                    if (errorHandle != null)
                        errorHandle(ex);
                    Thread.Sleep(retryTime);
                }
            } while (isException && Interlocked.Increment(ref count) < retrycount);
            if (isException && count >= retrycount && gtRetrycountAction != null)
                gtRetrycountAction();
        }
        /// <summary>
        /// 方法重试。说明：方法至少执行一次，如果行异常时，则进入重试逻辑。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">action</param>
        /// <param name="retrycount">重试次数</param>
        /// <param name="retryTime">重试间隔时间</param>
        /// <param name="exceptionAction">发生异常时操作</param>
        /// <param name="errorHandle">异常消息处理</param>
        /// <param name="defReturn">默认返回值</param>
        /// <returns></returns>
        public static T Retry<T>(Func<T> action, uint retrycount, TimeSpan retryTime, Action exceptionAction = null, Action<Exception> errorHandle = null, T defReturn = default (T))
        {
            bool isException = false;
            int count = 0;           
            do
            {
                try
                {
                    isException = false;
                    return action();
                }
                catch (Exception ex)
                {
                    isException = true;
                    if (exceptionAction != null)
                        exceptionAction();
                    if (errorHandle != null)
                        errorHandle(ex);
                    Thread.Sleep(retryTime);
                }
            } while (isException && Interlocked.Increment(ref count) < retrycount);
            return defReturn;
        }
    }
}
