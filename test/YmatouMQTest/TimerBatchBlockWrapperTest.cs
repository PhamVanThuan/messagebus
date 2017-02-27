using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YmatouMQTest
{
    [TestClass]
    public class TimerBatchBlockWrapperTest
    {
        [TestMethod]
        public void Batch()
        {
            _TimerBatchBlockWrapper<int> tbw = new _TimerBatchBlockWrapper<int>(TimeSpan.FromSeconds(1), 3, data =>
            {
                foreach (var i in data)
                {
                    Console.WriteLine(i + "__" + DateTime.Now);
                }
            });

            for (var i = 0; i < 100; i++)
            {
                tbw.Enqueue(i);
            }
        }
    }
}
