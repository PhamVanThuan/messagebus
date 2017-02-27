using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace YmatouMQConnectionConsoleApplication
{
    public class TPLDataflowTest
    {
        public static void BatchBlock() 
        {
            var batchStockEvents = new BatchBlock<int>(3, new GroupingDataflowBlockOptions { BoundedCapacity = 1024 });

            var action = new ActionBlock<int[]>(arr => 
            {
                foreach (var item in arr) Console.WriteLine(item);
            });

            batchStockEvents.LinkTo(action);
            for (var i = 0; i < 110; i++)
                batchStockEvents.Post(i);
              
            Console.Read();
        }
    }
}
