using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YmatouMQ.Common;

namespace YmatouMQ.Common.Extensions._Task
{
    public static class TaskExtensions
    {
        /// <summary>
        /// 同步等待结果
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task"></param>
        /// <param name="ignoreException"></param>
        /// <param name="log"></param>
        /// <param name="timeout"></param>
        /// <param name="defReturn"></param>
        /// <param name="descript"></param>
        /// <returns></returns>
        public static TResult GetResultSync<TResult>(this Task<TResult> task, bool ignoreException, ILog log, TimeSpan? timeout = null
            , TResult defReturn = default(TResult)
            , string descript = null)
        {

            try
            {
                if (task.IsCompleted) return task.Result;
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
                ex.Handle(log, "{0} GetResult(AggregateException)".Fomart(descript));
                if (ignoreException) return defReturn;
                else throw;
            }
            catch (Exception ex)
            {
                ex.Handle(log, "{0} GetResult(Exception)".Fomart(descript));
                if (ignoreException) return defReturn;
                else throw;
            }
        }
        public static void GetResult(this Task task, bool ignoreException, ILog log, TimeSpan? timeout = null)
        {
            try
            {
                if (task.IsCompleted) return;
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
                ex.Handle(log, "GetResult");
                if (ignoreException) return;
                else throw;
            }
            catch (Exception ex)
            {
                ex.Handle(log, "GetResult");
                if (ignoreException) return;
                else throw;
            }
        }

        public static void WithIgnoreException(this Task task)
        {
           
        }
        public static void WithIgnoreException<TResult>(this Task<TResult> task)
        {

        }
        public static Task IgnoreExceptions(this Task task)
        {
            task.ContinueWith(t => { var ignored = t.Exception; },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);
            return task;
        }
        public static Task<T> IgnoreExceptions<T>(this Task<T> task)
        {
            return (Task<T>)((Task)task).IgnoreExceptions();
        }       

        public static Task WithHandleException(this Task task, ILog log, string desc)
        {
            //if (task.Status == TaskStatus.RanToCompletion) return task;
            task.ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    t.Exception.Handle(e => true);
                    t.Exception.Handle(log,desc);
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }

        public static Task WithHandleException(this Task task, ILog log, string format = null, params object[] args)
        {
            //if (task.Status == TaskStatus.RanToCompletion) return task;
            task.ContinueWith(t =>
            {               
                if (t.Exception != null)
                {
                    t.Exception.Handle(e=>true);
                    t.Exception.Handle(log, format, args);
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }
        public static Task<TResult> WithHandleException<TResult>(this Task<TResult> task, ILog log, string format = null, params object[] args)
        {
            //if (task.Status == TaskStatus.RanToCompletion) return task;
            task.ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    t.Exception.Handle(e => true);
                    t.Exception.Handle(log, format, args);
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }
        public static Task<TResult> WithHandleException<TResult>(this Task<TResult> task, Action<Exception> faulted = null)
        {
            //if (task.Status == TaskStatus.RanToCompletion) return task;
            task.ContinueWith(t =>
            {
                if (faulted != null)
                    faulted(t.Exception);               
            }, TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }
        public static void TryWaitAllTask(this IEnumerable<Task> task, ILog log, CancellationToken token)
        {
            try
            {
                Task.WaitAll(task.ToArray(), token);
            }
            catch (AggregateException ex)
            {
                ex.Handle(log, "WaitAllTask(AggregateException)");
            }
            catch (Exception ex)
            {
                ex.Handle(log, "WaitAllTask(Exception)");
            }
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
    public static class LazyExtensions
    {
        /// <summary>Forces value creation of a Lazy instance.</summary>
        /// <typeparam name="T">Specifies the type of the value being lazily initialized.</typeparam>
        /// <param name="lazy">The Lazy instance.</param>
        /// <returns>The initialized Lazy instance.</returns>
        public static Lazy<T> Force<T>(this Lazy<T> lazy)
        {
            var ignored = lazy.Value;
            return lazy;
        }

        /// <summary>Retrieves the value of a Lazy asynchronously.</summary>
        /// <typeparam name="T">Specifies the type of the value being lazily initialized.</typeparam>
        /// <param name="lazy">The Lazy instance.</param>
        /// <returns>A Task representing the Lazy's value.</returns>
        public static Task<T> GetValueAsync<T>(this Lazy<T> lazy)
        {
            return Task.Factory.StartNew(() => lazy.Value);
        }

        /// <summary>Creates a Lazy that's already been initialized to a specified value.</summary>
        /// <typeparam name="T">The type of the data to be initialized.</typeparam>
        /// <param name="value">The value with which to initialize the Lazy instance.</param>
        /// <returns>The initialized Lazy.</returns>
        public static Lazy<T> Create<T>(T value)
        {
            return new Lazy<T>(() => value, false).Force();
        }
    }
}
