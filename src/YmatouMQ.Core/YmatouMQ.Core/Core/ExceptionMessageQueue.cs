using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace YmatouMQNet4.Core
{
    /// <summary>
    /// 异常消息队列
    /// </summary>
    internal class ExceptionMessageQueue : BlockingCollection<ExceptionMessageContext> 
    {
    }   
}
