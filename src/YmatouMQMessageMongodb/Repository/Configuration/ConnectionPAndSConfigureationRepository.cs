using System;
using YmatouMQMessageMongodb.Domain.IRepository;
using YmatouMQMessageMongodb.Repository.Context;
using YmatouMQNet4.Configuration;
using YmtSystem.Repository.Mongodb;
using YmtSystem.Repository.Mongodb.Context;

namespace YmatouMQMessageMongodb.Repository
{
    public class ConnectionPAndSConfigureationRepository : MongodbRepository<ConnectionPAndSConfigureation>, IConnectionPAndSConfigureationRepository
    {
        public ConnectionPAndSConfigureationRepository(MongodbContext context)
            : base(context)
        {
        }

        public ConnectionPAndSConfigureationRepository()
            : this(new ConnectionPAndSConfigureationContext())
        {

        }
    }
}
