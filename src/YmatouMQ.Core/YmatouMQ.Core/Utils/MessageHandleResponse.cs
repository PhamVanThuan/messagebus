using System;

namespace YmatouMQNet4.Utils
{
    /// <summary>
    /// 消息处理结果
    /// </summary>
    public class MessageHandleResponse
    {
        /// <summary>
        /// 业务处理结果代码。200，表示处理成功。
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 业务处理结果返回的消息
        /// </summary>
        public string Message { get; set; }

        public MessageHandleResponse(int code, string message)
        {
            this.Code = code;
            this.Message = message;
        }
        /// <summary>
        /// 成功
        /// </summary>
        /// <returns></returns>
        public static MessageHandleResponse Success()
        {
            return new MessageHandleResponse(200, "ok");
        }
        /// <summary>
        /// 失败
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static MessageHandleResponse Fail(int code, string message = null)
        {
            return new MessageHandleResponse(code, message);
        }
    }
}
