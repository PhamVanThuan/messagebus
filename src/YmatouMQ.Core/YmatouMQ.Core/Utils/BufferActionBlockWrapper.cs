using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace YmatouMQNet4.Utils
{
    public class BufferActionBlockWrapper<T>
    {
        private readonly BufferBlock<T> buffer;
        private readonly ActionBlock<T> action;

        public BufferActionBlockWrapper()
        {

        }
        public BufferActionBlockWrapper(Action<T> _action)
        {
            buffer = new BufferBlock<T>();
            action = new ActionBlock<T>(_action, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = Environment.ProcessorCount });
            buffer.LinkTo(action);
        }
        public BufferActionBlockWrapper(Func<T, Task> _action)
        {
            buffer = new BufferBlock<T>();
            action = new ActionBlock<T>(_action, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = Environment.ProcessorCount });
            buffer.LinkTo(action);
        }
        public void Post(T data)
        {
            buffer.Post(data);
        }
        public async Task PostAsync(T data)
        {            
            var result = await buffer.SendAsync(data).ConfigureAwait(false);           
        }
        public void Complete()
        {
            try
            {
                buffer.Complete();
                action.Complete();
            }
            catch
            {

            }
        }
    }
}
