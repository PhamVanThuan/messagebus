using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using YmatouMQMessageMongodb.Domain.IRepository;
using YmatouMQMessageMongodb.Domain.Specifications;
using YmatouMQMessageMongodb.Repository.Context;
using YmatouMQNet4.Configuration;
using YmtSystem.Repository.Mongodb;
using YmtSystem.Repository.Mongodb.Context;

namespace YmatouMQMessageMongodb.Repository
{
    public class MQConfigurationRepository : MongodbRepository<MQMainConfiguration>, IMQConfigurationRepository
    {
        public MQConfigurationRepository(MongodbContext context)
            : base(context)
        {
        }

        public MQConfigurationRepository() : this(new MQConfigurationContext())
        {
        }

        public IEnumerable<string> FindAllAppId()
        {
            return this.Context.GetCollection<MQMainConfiguration>(MQConfigurationSpecifications.ConfigurationDb,
                MQConfigurationSpecifications.ConfigurationAppDetailsTb)
                .Find(MQConfigurationSpecifications.MatchAllAppId())
                .SetFields(Fields.Include("_id"))
                .Select(c => c.AppId);
        }
    }
}
