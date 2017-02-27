using System;
using YmatouMQAdmin.Domain.IRepository;
using YmatouMQAdmin.Repository.Context;
using YmatouMQNet4.Configuration;
using YmtSystem.Repository.Mongodb;
using YmtSystem.Repository.Mongodb.Context;

namespace YmatouMQAdmin.Repository
{
    public class MQConfigurationRepository : MongodbRepository<MQMainConfiguration>, IMQConfigurationRepository
    {
        public MQConfigurationRepository(MongodbContext context)
            : base(context)
        {
        }
        public MQConfigurationRepository() : this(new MQConfigurationContext()) { }
    }
}
