using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using YmatouMQNet4.Utils;

namespace YmatouMQNet4.Extensions
{
    public static class RabbitmqExtensions
    {
        public static Task ExecutedAsync(this IModel channel, Action<IModel> action)
        {
            var tcs = new TaskCompletionSource<ReturnVoid>();
            try
            {
                action(channel);
                tcs.TrySetResult(new ReturnVoid());
            }
            catch (OperationCanceledException ex)
            {
                tcs.SetCanceled();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }
        public static void TryColseChannel(this IModel channel)
        {
            try
            {
                if (!channel.IsClosed)
                    channel.Close();
            }
            catch
            {
            }
        }
        public static Task<T> ExecutedAsync<T>(this IModel channel, Func<IModel, T> action)
        {
            var tcs = new TaskCompletionSource<T>();
            try
            {
                var result = action(channel);
                tcs.TrySetResult(result);
            }
            catch (OperationCanceledException ex)
            {
                tcs.SetCanceled();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }
    }
}
