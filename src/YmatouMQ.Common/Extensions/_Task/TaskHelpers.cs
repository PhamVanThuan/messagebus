using System;
using System.Threading.Tasks;

namespace YmatouMQ.Common.Extensions._Task
{
    /// <summary>
    /// //http://blogs.msdn.com/b/pfxteam/archive/2010/11/21/10094564.aspx
    /// </summary>
    public static class TaskHelpers
    {
        static TaskHelpers()
        {
            var tcs = new TaskCompletionSource<NullStruct>();
            tcs.SetResult(new NullStruct());
            Completed = tcs.Task;
        }

        public static Task Completed { get; private set; }
        /// <summary>
        /// 同步执行操作
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task ExecuteSynchronously(this Action action)
        {
            var tcs = new TaskCompletionSource<NullStruct>();
            try
            {
                action();
                tcs.SetResult(new NullStruct());
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }

            return tcs.Task;
        }
        public static async Task<Task> ExecuteASynchronously(this Action action)
        {
            var tcs = new TaskCompletionSource<NullStruct>();
            try
            {
                await Task.Factory.StartNew(() => action());
                tcs.SetResult(new NullStruct());
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }

            return tcs.Task;
        }
        public static Task<T> ExecuteSynchronously<T>(this Func<T> action)
        {
            var tcs = new TaskCompletionSource<T>();
            try
            {
                var result = action();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }
        /// <summary>
        /// 异步执行方法调用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task<T> ExecuteASynchronously<T>(this Func<T> action)
        {
            var tcs = new TaskCompletionSource<T>();
            try
            {
                Task.Factory.StartNew(() => action())
                                   .ContinueWith(t =>
                                   {
                                       if (t.IsFaulted)
                                           tcs.SetException(t.Exception);
                                       if (t.IsCanceled)
                                           tcs.SetCanceled();
                                       else
                                           tcs.SetResult(t.Result);
                                   });
                return tcs.Task;
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }
        public static Task FromException(Exception ex)
        {
            var tcs = new TaskCompletionSource<NullStruct>();
            tcs.SetException(ex);

            return tcs.Task;
        }

        public static Task<T2> Then<T1, T2>(this Task<T1> first, Func<T2> next)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<T2>();
            first.ContinueWith(x =>
                {
                    if (x.IsFaulted)
                        tcs.TrySetException(x.Exception.InnerExceptions);
                    else if (x.IsCanceled)
                        tcs.TrySetCanceled();
                    else
                    {
                        try
                        {
                            var result = next();
                            tcs.TrySetResult(result);
                        }
                        catch (Exception exc)
                        {
                            tcs.TrySetException(exc);
                        }
                    }
                });
            return tcs.Task;
        }

        public static Task<T2> Then<T2>(this Task first, Func<T2> next)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<T2>();
            first.ContinueWith(x =>
            {
                if (x.IsFaulted)
                    tcs.TrySetException(x.Exception.InnerExceptions);
                else if (x.IsCanceled)
                    tcs.TrySetCanceled();
                else
                {
                    try
                    {
                        var result = next();
                        tcs.TrySetResult(result);
                    }
                    catch (Exception exc)
                    {
                        tcs.TrySetException(exc);
                    }
                }
            });
            return tcs.Task;
        }
        /// <summary>
        /// 任务延续。前置任务 first 执行成功，再执行后续任务next
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="first"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public static Task Then<T1>(this Task<T1> first, Func<T1, Task> next)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<NullStruct>();
            first.ContinueWith(delegate
            {
                if (first.IsFaulted) tcs.TrySetException(first.Exception.InnerExceptions);
                else if (first.IsCanceled) tcs.TrySetCanceled();
                else
                {
                    try
                    {
                        var t = next(first.Result);
                        if (t == null) tcs.TrySetCanceled();
                        else t.ContinueWith(delegate
                        {
                            if (t.IsFaulted) tcs.TrySetException(t.Exception.InnerExceptions);
                            else if (t.IsCanceled) tcs.TrySetCanceled();
                            else tcs.TrySetResult(new NullStruct());
                        });
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            });
            return tcs.Task;
        }

        public static Task<T2> Then<T1, T2>(this Task<T1> first, Func<T1, Task<T2>> next)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<T2>();
            first.ContinueWith(delegate
            {
                if (first.IsFaulted) tcs.TrySetException(first.Exception.InnerExceptions);
                else if (first.IsCanceled) tcs.TrySetCanceled();
                else
                {
                    try
                    {
                        var t = next(first.Result);
                        if (t == null) tcs.TrySetCanceled();
                        else t.ContinueWith(delegate
                        {
                            if (t.IsFaulted) tcs.TrySetException(t.Exception.InnerExceptions);
                            else if (t.IsCanceled) tcs.TrySetCanceled();
                            else tcs.TrySetResult(t.Result);
                        });
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            });
            return tcs.Task;
        }

        public static Task Then(this Task first, Action next)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<NullStruct>();
            first.ContinueWith(x =>
            {
                if (x.IsFaulted)
                    tcs.TrySetException(x.Exception.InnerExceptions);
                else if (x.IsCanceled)
                    tcs.TrySetCanceled();
                else
                {
                    try
                    {
                        next();
                        tcs.TrySetResult(new NullStruct());
                    }
                    catch (Exception exc)
                    {
                        tcs.TrySetException(exc);
                    }
                }
            });
            return tcs.Task;
        }

        public static Task CompleteThen(this Task first, Action next)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (next == null) throw new ArgumentNullException("next");
            var tcs = new TaskCompletionSource<NullStruct>();
            first.ContinueWith(x =>
            {
                try
                {
                    next();
                    tcs.TrySetResult(new NullStruct());
                }
                catch (Exception)
                {
                }
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
            return tcs.Task;
        }

        public static Task<T> FromResult<T>(T value)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(value);
            return tcs.Task;
        }

        public static Task<T> FromException<T>(Exception exception)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetException(exception);
            return tcs.Task;
        }
        public static Task FromReturnVoid()
        {
            var tcs = new TaskCompletionSource<NullStruct>();
            tcs.SetResult(new NullStruct());
            return tcs.Task;

        }
        private struct NullStruct
        {
        }
    }
}