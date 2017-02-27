using System;
using System.Runtime.Serialization;

namespace YmatouMQNet4.Configuration
{
    [DataContract(Name = "callbackCfg")]
    public class CallbackConfiguration
    {
        /// <summary>
        /// 回调业务端标识
        /// </summary>
        [DataMember(Name = "_key")]
        public string CallbackKey { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        [DataMember(Name = "Enable")]
        public bool? Enable { get; set; }
        /// <summary>
        /// 业务端URL
        /// </summary>
        [DataMember(Name = "url")]
        public string Url { get; set; }
        /// <summary>
        /// 回调业务端超时时间（0：不超时，大于0 则表示超时），默认不超时
        /// </summary>
        [DataMember(Name = "timeout")]
        public int? CallbackTimeOut { get; set; }
        /// <summary>
        /// 业务端接受的http类型，默认post
        /// </summary>
        [DataMember(Name = "method")]
        public string HttpMethod { get; set; }
        /// <summary>
        /// 业务端接受的媒体类型，默认json
        /// </summary>
        [DataMember(Name = "contenttype")]
        public string ContentType { get; set; }
        /// <summary>
        /// 优先级（1，2...），0表示正常级别，默认空
        /// </summary>
        [DataMember(Name = "priority")]
        public int? Priority { get; set; }
        /// <summary>
        /// 业务端接受消息时间范围（暂未实现此功能）
        /// </summary>
        [DataMember(Name = "accepttime")]
        public string AcceptMessageTimeRange { get; set; }
        /// <summary>
        /// 补发消息超时时间（0，表示不补发）
        /// </summary>
        [DataMember(Name = "isRetry")]
        public int? IsRetry { get; set; }

        /// <summary>
        /// 是否可以补发
        /// </summary>
        public bool? IsApproveRetry
        {
            get;
            set;
        }

        public bool ApproveEnable
        {
            get;
            set;
        }
    }
}
