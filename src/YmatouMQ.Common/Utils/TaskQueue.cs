using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace YmatouMQ.Common.Utils
{    
    public class TaskQueue
    {
        public class Work
        {
            private readonly TaskCompletionSource<ReturnVoid> tcs;
            private readonly Action _action;
            private readonly Action<object > __action ;
            private readonly CancellationToken? token;
            private readonly Action<object, Exception> errorAction;
            private readonly object state;

            public Work(TaskCompletionSource<ReturnVoid> tcs, Action action, CancellationToken? token, Action<object, Exception> errorAction, object state)
            {
                this.tcs = tcs;
                this._action = action;
                this.token = token;
                this.errorAction = errorAction;
                this.state = state;
            }
            public Work(TaskCompletionSource<ReturnVoid> tcs, Action<object> action, CancellationToken? token, Action<object, Exception> errorAction, object state)
            {
                this.tcs = tcs;
                this.__action = action;
                this.token = token;
                this.errorAction = errorAction;
                this.state = state;
            }
            public TaskCompletionSource<ReturnVoid> Tcs { get { return tcs; } }
            public Action action { get { return _action; } }
            public Action<object> ___action{get{return __action;}}
            public CancellationToken? Token { get { return token; } }
            public Action<object, Exception> ErrorAction { get { return errorAction; } }
            public object State { get { return state; } }
        }
        private readonly BlockingCollection<Work> queue;       
        private readonly CancellationTokenSource tcs;
        private bool isMarkComplete = false;
        public TaskQueue(int workthreadcount = 1)
        {
            if (workthreadcount <= 0 || workthreadcount > Environment.ProcessorCount)
                throw new ArgumentException("workthreadcount error.");
            this.queue = new BlockingCollection<Work>();
            this.tcs = new CancellationTokenSource();
            Start(workthreadcount);
        }

        private void Start(int _workthreadcount) 
        {            
            for (var i = 0; i < _workthreadcount; i++)
            {
                RunWork();
            }
        }
        public int Count { get{return queue.Count;}}
        public bool IsCompleted { get{return queue.Count ==0;}}
        public void EnqueueTask(Action action, Action<object, Exception> errorAction, object state, CancellationToken? token = null)
        {
            if (isMarkComplete)return;
            queue.Add(new Work(new TaskCompletionSource<ReturnVoid>(), action, token, errorAction, state));
        }
        public void EnqueueTask(Action<object> action, Action<object, Exception> errorAction, object state, CancellationToken? token = null)
        {
            if (isMarkComplete) return;
            queue.Add(new Work(new TaskCompletionSource<ReturnVoid>(), action, token, errorAction, state));
        }
        public void Complete()
        {
            isMarkComplete = true;
            queue.CompleteAdding();
            //tcs.Cancel();
        }
        private void RunWork()
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    foreach (var item in queue.GetConsumingEnumerable())
                    {
                        if ((item.Token != null && item.Token.HasValue) && item.Token.Value.IsCancellationRequested)
                        {
                            item.Token.Value.ThrowIfCancellationRequested();
                        }
                        else
                        {
                            try
                            {
                                if(item.State !=null)
                                    item.___action(item.State);
                                else 
                                item.action ();
                                item.Tcs.SetResult(new ReturnVoid());
                            }
                            catch (OperationCanceledException ex)
                            {
                                item.Tcs.SetCanceled();
                            }
                            catch (Exception ex)
                            {
                                item.Tcs.SetException(ex);
                                item.ErrorAction(item.State, ex);
                            }
                        }
                    }
                }, this.tcs.Token);
            }
            catch (OperationCanceledException ex)
            {
                //todo:
            }
        }
    }

    public struct ReturnVoid
    {

    }
}
