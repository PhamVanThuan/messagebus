using System;
using System.Collections.Generic;
using YmatouMQNet4.Configuration;
using YmtSystem.Domain.MongodbRepository;

namespace YmatouMQMessageMongodb.Domain.IRepository
{
    public interface IMQConfigurationRepository : IMongodbRepository<MQMainConfiguration>
    {
        IEnumerable<string> FindAllAppId();
    }

    public interface IConnectionPAndSConfigureationRepository : IMongodbRepository<ConnectionPAndSConfigureation> 
    {
    }
}
