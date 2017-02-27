using System;
using YmatouMQMessageMongodb.Domain.IRepository;
using YmatouMQMessageMongodb.Repository.Context;
using YmatouMQNet4.Configuration;
using YmtSystem.Repository.Mongodb;
using YmtSystem.Repository.Mongodb.Context;

namespace YmatouMQMessageMongodb.Repository
{
    public class MQAppdomainConfigurationRepository : MongodbRepository<AppdomainConfiguration>, IMQAppdomainConfigurationRepository
    {
        public MQAppdomainConfigurationRepository(MongodbContext context)
            : base(context)
        {
        }

        public MQAppdomainConfigurationRepository()
            : this(new MQAppdomainConfigurationContext())
        {

        }
    }
}
