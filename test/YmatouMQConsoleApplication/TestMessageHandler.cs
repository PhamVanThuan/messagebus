using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQNet4;
using YmatouMQNet4.Core;
using YmatouMQ.Common.Dto;
using YmatouMQ.Common.Utils;
using YmatouMQ.Common;
using YmatouMQ.Log;
namespace YmatouMQConsoleApplication
{
    public class TestMessageHandler : IMessageHandler<string>
    {
        private static readonly ILog log = LogFactory.GetLogger(LogEngineType.Console, "TestMessageHandler");       

        public Task<ResponseData<ResponseNull>> Handle(MessageHandleContext<string> msgContext)
        {
            log.Info("获取到消息 {0}", msgContext.Message);
            return ResponseData<ResponseNull>.CreateSuccessTask(ResponseNull._Null, "ok");
        }
    }
}
