using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YmatouMQNet4.Extensions._Task
{
    public static class TaskExtensions
    {
        private static readonly ILog log = LogFactory.GetLogger(LogFactory._LogType, "YmatouMQ.Extensions._Task.TaskExtensions");

        public static TResult GetResult<TResult>(this Task<TResult> task, bool ignoreException, TimeSpan? timeout = null, TResult defReturn = default(TResult), string descript = null)
        {
            try
            {
                if (timeout == null || !timeout.HasValue)
                {
                    Task.WaitAll(task);
                    return task.Result;
                }
                else
                {
                    if (Task.WaitAll(new Task[] { task }, timeout.Value))
                    {
                        return task.Result;
                    }
                    else
                    {
                        return defReturn;
                    }
                }
            }
            catch (AggregateException ex)
            {
                ex.Handle(log, "{0} GetResult(AggregateException)异常".Fomart(descript));
                if (ignoreException) return defReturn;
                else throw;
            }
            catch (Exception ex)
            {
                ex.Handle(log, "{0} GetResult(Exception)异常".Fomart(descript));
                if (ignoreException) return defReturn;
                else throw;
            }
        }
        public static void GetResult(this Task task, bool ignoreException, TimeSpan? timeout = null)
        {
            try
            {
                if (timeout == null || !timeout.HasValue)
                {
                    Task.WaitAll(task);
                    return;
                }
                else
                {
                    Task.WaitAll(new Task[] { task }, timeout.Value);
                    return;
                }
            }
            catch (AggregateException ex)
            {
                ex.Handle(log, "GetResult异常");
                if (ignoreException) return;
                else throw;
            }
            catch (Exception ex)
            {
                ex.Handle(log, "GetResult异常");
                if (ignoreException) return;
                else throw;
            }
        }
        public static Task WithHandleException(this Task task, string format = null, params object[] args)
        {
            task.ContinueWith(t =>
            {
                var msg = string.IsNullOrEmpty(format) ? null : string.Format(format, args);
                foreach (var ex in t.Exception.InnerExceptions)
                    log.Error("task ex desc {0},ex {1}", msg, ex);
                //Console.WriteLine("task ex desc {0},ex {1}", msg, ex);
            }, TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }
        public static Task WithHandleException(this Task task, Action faulted = null, string format = null, params object[] args)
        {
            task.ContinueWith(t =>
            {
                if (faulted != null)
                    faulted();
                var msg = string.IsNullOrEmpty(format) ? null : string.Format(format, args);
                foreach (var ex in t.Exception.InnerExceptions)
                    log.Error("task ex desc {0},ex {1}", msg, ex);
            }, TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }
        private static readonly TimeSpan DoNotRepeat = TimeSpan.FromMilliseconds(-1);

        /// <summary>
        /// Starts a new task that will poll for a result using the specified function, and will be completed when it satisfied the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of value that will be returned when the task completes.</typeparam>
        /// <param name="getResult">Function that will be used for polling.</param>
        /// <param name="isResultValid">Predicate that determines if the result is valid, or if it should continue polling</param>
        /// <param name="pollInterval">Polling interval.</param>
        /// <param name="timeout">The timeout interval.</param>
        /// <returns>The result returned by the specified function, or <see langword="null"/> if the result is not valid and the task times out.</returns>
        public static Task<T> StartNew<T>(Func<T> getResult, Func<T, bool> isResultValid, TimeSpan pollInterval, TimeSpan timeout)
        {
            Timer timer = null;
            TaskCompletionSource<T> taskCompletionSource = null;
            DateTime expirationTime = DateTime.UtcNow.Add(timeout);

            timer =
                new Timer(_ =>
                {
                    try
                    {
                        if (DateTime.UtcNow > expirationTime)
                        {
                            timer.Dispose();
                            taskCompletionSource.SetResult(default(T));
                            return;
                        }

                        var result = getResult();

                        if (isResultValid(result))
                        {
                            timer.Dispose();
                            taskCompletionSource.SetResult(result);
                        }
                        else
                        {
                            // try again
                            timer.Change(pollInterval, DoNotRepeat);
                        }
                    }
                    catch (Exception e)
                    {
                        timer.Dispose();
                        taskCompletionSource.SetException(e);
                    }
                });

            taskCompletionSource = new TaskCompletionSource<T>(timer);

            timer.Change(pollInterval, DoNotRepeat);

            return taskCompletionSource.Task;
        }
    }
}
