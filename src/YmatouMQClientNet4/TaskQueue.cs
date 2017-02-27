using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YmatouMessageBusClientNet4
{
    struct ReturnVoid
    {

    }
    class TaskQueue
    {
        class Work
        {
            private readonly TaskCompletionSource<ReturnVoid> _tcs;
            private readonly Action _action;
            private readonly Action<Exception> _errorAction;
            private readonly CancellationToken? _token;
            private readonly Action continueWithAction;

            public TaskCompletionSource<ReturnVoid> tcs { get { return _tcs; } }
            public Action action { get { return _action; } }
            public Action<Exception> errorAction { get { return _errorAction; } }
            public CancellationToken? token { get { return _token; } }
            public Work(TaskCompletionSource<ReturnVoid> tcs, Action action, Action<Exception> errorHandler = null, CancellationToken? token = null, Action continueWithAction=null)
            {
                this._tcs = tcs;
                this._action = action;
                this._errorAction = errorHandler;
                this._token = token;
                this.continueWithAction = continueWithAction;
            }
        }
        private BlockingCollection<Work> work_queue = new BlockingCollection<Work>();
        public void StartTaskQueue(int thread = 1)
        {
            for (var i = 0; i < thread; i++)
                runwork();
        }
       
        public void AddWorkToQueue(Action action, Action<Exception> _errorAction = null, CancellationToken? token = null)
        {
            work_queue.TryAdd(new Work(new TaskCompletionSource<ReturnVoid>(), action, _errorAction, token));
        } 
        private void runwork()
        {
            try
            {
                Task.Factory.StartNew(() =>
                {

                    foreach (var item in work_queue.GetConsumingEnumerable())
                    {
                        if ((item.token != null && item.token.HasValue) && item.token.Value.IsCancellationRequested)
                        {
                            item.token.Value.ThrowIfCancellationRequested();
                        }
                        else
                        {
                            try
                            {
                                item.action();
                                item.tcs.SetResult(new ReturnVoid());
                            }
                            catch (OperationCanceledException ex)
                            {
                                item.tcs.SetCanceled();
                            }
                            catch (Exception ex)
                            {
                                item.tcs.SetException(ex);
                                if (item.errorAction != null)
                                    item.errorAction(ex);
                            }
                        }
                    }
                });
            }
            catch (AggregateException ex)
            {
                Ymatou.CommonService.ApplicationLog.Error("TaskQueue error.0 ", ex);
            }
            catch (Exception ex)
            {
                Ymatou.CommonService.ApplicationLog.Error("TaskQueue error.1", ex);
            }
        }

    }
}
