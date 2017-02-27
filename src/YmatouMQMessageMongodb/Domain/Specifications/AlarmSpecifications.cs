using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQMessageMongodb.Domain.Module;

namespace YmatouMQMessageMongodb.Domain.Specifications
{
    public class AlarmSpecifications
    {
        public static IMongoQuery MatchCallbackId(string callbackId)
        {
            return Query<Alarm>.EQ(a => a.CallbackId, callbackId);
        }
    }
}
