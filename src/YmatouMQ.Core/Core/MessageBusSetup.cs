using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YmatouMQNet4.Core
{
    public static class MessageBusSetup
    {
        /// <summary>
        /// 停止BUS应用。
        /// </summary>
        public static void StopBusApplication()
        {
            Bus.Builder.StopBusApplication();
        }
        /// <summary>
        /// 启动BUS应用
        /// </summary>
        public static void StartBusApplication()
        {
            Bus.Builder.StartBusApplication();
        }
    }
}
