using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQNet4.Configuration;
using YmtSystem.Domain.MongodbRepository;

namespace YmatouMQAdmin.Domain.IRepository
{
    public interface IMQAppdomainConfigurationRepository : IMongodbRepository<AppdomainConfiguration>
    {
    }
}
