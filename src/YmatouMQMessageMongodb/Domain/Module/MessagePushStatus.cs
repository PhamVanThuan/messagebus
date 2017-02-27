using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YmatouMQMessageMongodb.Domain.Module
{
   public class MessagePushStatus
    {
       public string PushId { get; set; }
       public int Status { get; set; }
       public DateTime PushTime { get; set; }
    }


   public class MessagePushStatus2
   {
       public string AppId { get; set; }
       public string Code { get; set; }
       public string UuId { get; set; }
   }
}
