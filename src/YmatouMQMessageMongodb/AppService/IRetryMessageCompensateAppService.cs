using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQMessageMongodb.Domain.Module;

namespace YmatouMQMessageMongodb.AppService
{
    public interface IRetryMessageCompensateAppService
    {
        /// <summary>
        ///  异步添加重试消息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        Task AddAsync(RetryMessage msg);
    }
}
