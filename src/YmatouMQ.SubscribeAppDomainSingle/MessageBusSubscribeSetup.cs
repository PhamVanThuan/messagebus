using System;

namespace YmatouMQ.SubscribeAppDomainSingle
{
    public class _MessageBusSubscribeSetup
    {
        public static void Start()
        {
            MessageBusSubscribeManager.Init();
        }
        public static void Stop()
        {
            MessageBusSubscribeManager.Close();
        }
    }
}
