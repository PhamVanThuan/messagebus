using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using YmatouMQ.Common.Extensions.Serialization;
using YmatouMQ.Common.Extensions;

namespace YmatouMQ.Common.MessageHandleContract
{
    /// <summary>
    /// 事件处理
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IMessageHandler<TMessage>
    {
        /// <summary>
        /// 消息处理回调
        /// </summary>
        /// <param name="msgContext"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task Handle(MessageHandleContext<TMessage> msgContext, Func<string, Task> handleSuccess, Func<Task> handleException);
        void CloseHandle(string appid, string code);
        void InitHandle(string appid, string code);
    }
    public interface IMessageRetryHandle<TMessage>
    {
        Task Handle(MessageHandleContext<TMessage> msgContext, Func<IDictionary<string, bool>, Task> callbackEndAction);
    }
    public struct HandlerResponseCode
    {
        public const string Success = "ok";
        public const string Fail = "fail";
        public const string _Success = "\"ok\"";
        public const string _Fail = "\"fail\"";
        public static bool IsSuccess(string code)
        {
            if (string.IsNullOrEmpty(code) || code == "request_exception") return false;
            var lowerCode = code.ToLower();
            if (lowerCode == Success || lowerCode == _Success) return true;
            else if (lowerCode == Fail || lowerCode == _Fail) return false;
            else if (lowerCode.StartsWith(Success) || lowerCode.StartsWith(_Success)) return true;
            else if (lowerCode.StartsWith(Fail) || lowerCode.StartsWith(_Fail)) return false;
            else if ("EnableOldClientProtocol".GetAppSettings("0") == "1")
            {
                var result = code._JSONDeserializeFromString<ResponseData<ResponseNull>>();
                if (result != null && result.Code == 200) return true;
                else return false;
            }
            else return false;
        }
    }
}
