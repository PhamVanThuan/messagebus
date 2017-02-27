using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YmatouMQ.Common;
using YmatouMQ.Log;
using YmatouMQNet4;
using YmatouMQNet4.Configuration;
using YmatouMQ.Connection;
using YmatouMQ.Common.Extensions;
using YmatouMQ.SubscribeAppDomain;
using YmatouMQ.Common.MessageHandleContract;
using YmatouMQ.Subscribe;
using log4net.Config;
using System.IO;
using YmatouMQ.SubscribeAppDomainSingle;
using YmatouMQ.ConfigurationSync;

namespace YmatouMQConsume.AppConsole
{
    class Program
    {
        //private static readonly _YmatouMQAppdomainManager adManager = new _YmatouMQAppdomainManager();
        private static readonly ILog log = LogFactory.GetLogger(LogEngineType.Console, " YmatouMQConsume.AppConsole.Program");
        static void Main(string[] args)
        {
            //ThreadPool.SetMaxThreads(9, 9);
            int workTh, ioTh;
            ThreadPool.GetMaxThreads(out workTh, out ioTh);
            Console.WriteLine("max [work th:{0},io th:{1}]", workTh, ioTh);
            ThreadPool.GetMinThreads(out workTh, out ioTh);
            Console.WriteLine("min [work th:{0},io th:{1}]", workTh, ioTh);
            ThreadPool.GetAvailableThreads(out workTh, out ioTh);
            Console.WriteLine("available [work th:{0},io th:{1}]", workTh, ioTh);
            Console.WriteLine("选择模式 1：单个appdomain；2：多个appdomain");
            var type = Console.ReadLine();

            if (type == "1")
            {
                Console.WriteLine("独立的appdomain 开始启动");
                log.Debug("启动订阅");
                int work, iowork;
                ThreadPool.GetMaxThreads(out work, out iowork);
                Console.WriteLine("启动前：work->{0},iowork->{1}", work, iowork);
                //MessageBusSubscribeSetup.Start();
                _MessageBusSubscribeSetup.Start();
                log.Debug("启动完成,enter C stop");
                var code = Console.ReadKey().Key;
                if (code == ConsoleKey.C)
                {
                    _MessageBusSubscribeSetup.Stop();
                    Console.WriteLine("bus stop");
                }                             
                Console.Read();
            }
            else
            {
                Console.WriteLine("非独立的appdomain 开始启动");
                var assemblyName = System.Configuration.ConfigurationManager.AppSettings["handleAssemblyName"].Split(new char[] { ',' });
                var handler = Activator.CreateInstance(assemblyName[1], assemblyName[0]).Unwrap() as IMessageHandler<byte[]>;
                var cfglist = MQMainConfigurationManager.Builder.GetConfiguration();
                var pool = new MQConnectionPoolManager();
                var list = new List<_Subscribe>();
                cfglist.EachAction(r =>
                  {
                      r.Value.MessageCfgList.EachAction(_m =>
                      {
                          _Subscribe s = new _Subscribe(r.Value.AppId, _m.Code);
                          list.Add(s);
                      });
                  });
                list.ForEach(e => { e.Start(); });

                Console.Read();
            }
            Console.Read();
        }
    }
}
