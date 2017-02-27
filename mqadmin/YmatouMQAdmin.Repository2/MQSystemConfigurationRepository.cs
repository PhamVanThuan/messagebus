using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQAdmin.Domain.IRepository;
using YmatouMQAdmin.Repository.Context;
using YmatouMQNet4.Configuration;
using YmtSystem.Repository.Mongodb;
using YmtSystem.Repository.Mongodb.Context;

namespace YmatouMQAdmin.Repository
{
    public class MQSystemConfigurationRepository : MongodbRepository<MQSystemConfiguration>, IMQSystemConfigurationRepository
    {
        public MQSystemConfigurationRepository(MongodbContext context)
            : base(context)
        {
        }

        public MQSystemConfigurationRepository() : this(new MQSystemConfigurationContext()) 
        {

        }
    }
}
