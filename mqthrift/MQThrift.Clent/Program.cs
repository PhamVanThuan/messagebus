using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQMessageProtocols;

namespace MQThrift.Clent
{
    class Program
    {
        static void Main(string[] args)
        {
            MQBusClient mc = new MQBusClient();
            mc.Start();
            Console.WriteLine("启动成功，开始测试");
            var count = 500000;
            try
            {
                var watch = Stopwatch.StartNew();
                for (var i = 0; i < count; i++)
                {
                    var str = "hell word.." + i;
                    var by = System.Text.Encoding.GetEncoding("utf-8").GetBytes(str);
                    var result = mc._client.publish(new MessageDto { AppId = "test2", Code = "gaoxu", Ip = "0.0.0.0", MsgUniqueId = Guid.NewGuid().ToString("N"), Body = by });
                    if (i == 499999)
                        Console.WriteLine(result.Code);
                }
                var total = watch.ElapsedMilliseconds;
                watch.Stop();
                var ops = count * 1000 / (total > 0 ? total : 1);
                var outStr = string.Format("发送完成，耗时 {0} 毫秒,每秒 发送 {1}个消息", total, ops);
                Console.WriteLine(outStr);
                Console.Read();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ReadLine();
        }
    }
}
