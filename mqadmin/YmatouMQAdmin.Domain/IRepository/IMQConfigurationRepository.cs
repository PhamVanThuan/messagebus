using System;
using YmatouMQNet4.Configuration;
using YmtSystem.Domain.MongodbRepository;

namespace YmatouMQAdmin.Domain.IRepository
{
    public interface IMQConfigurationRepository : IMongodbRepository<MQMainConfiguration>
    {
    }

    public interface IConnectionPAndSConfigureationRepository : IMongodbRepository<ConnectionPAndSConfigureation> 
    {
    }
}
