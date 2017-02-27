using System;
using YmatouMQNet4.Configuration;
using YmtSystem.Domain.MongodbRepository;

namespace YmatouMQMessageMongodb.Domain.IRepository
{
    public interface IMQAppdomainConfigurationRepository : IMongodbRepository<AppdomainConfiguration>
    {
    }
}
