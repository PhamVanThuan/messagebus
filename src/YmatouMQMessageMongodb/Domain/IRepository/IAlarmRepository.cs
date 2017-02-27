using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQMessageMongodb.Domain.Module;
using YmtSystem.Domain.MongodbRepository;

namespace YmatouMQMessageMongodb.Domain.IRepository
{
    public interface IAlarmRepository : IMongodbRepository<Alarm>
    {

    }
}
