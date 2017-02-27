using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using YmatouMQ.Common.Utils;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Extensions._Task;

namespace YmatouMQ.Connection
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
        public static void TryCloseChannel(this IModel channel)
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

        public static T GetMQHeaderValue<T>(this IBasicProperties properties, string key = "msgid", T defVal = default (T))
        {
            object _mid;
            if (properties.Headers != null && properties.Headers.TryGetValue(key, out _mid))
            {
                return (T)((object)((byte[])_mid).GetString());
            }
            return defVal;
        }
        public static string GetMQMessageId(this IBasicProperties properties, string key = "msgid", string defVal = null)
        {
            object _mid;
            if (properties.Headers != null && properties.Headers.TryGetValue(key, out _mid))
            {
                return ((byte[])_mid).GetString();
            }
            return defVal;
        }
    }
}
