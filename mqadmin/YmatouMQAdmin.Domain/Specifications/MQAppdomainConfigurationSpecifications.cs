using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using YmatouMQNet4.Configuration;

namespace YmatouMQAdmin.Domain.Specifications
{
    public class MQAppdomainConfigurationSpecifications
    {
        public static IMongoQuery MatchAppdomain(string domainName)
        {
            if (string.IsNullOrEmpty(domainName)) return Query.Null;
            var arr = domainName.Split(new char[] { ',' });
            return Query.And(Query<AppdomainConfiguration>.In(e => e.DomainName, arr));
        }
        public static IMongoQuery MatchOneAppdomain(string domainName)
        {
            if (string.IsNullOrEmpty(domainName)) return Query.Null;
            domainName = domainName.ToLower();
            return Query.And(Query<AppdomainConfiguration>.EQ(e => e.DomainName, domainName));
        }
        public static IMongoQuery _MatchOneOrAllAppdomain(string domainName)
        {
            if (string.IsNullOrEmpty(domainName) || domainName == "_all") return Query.Null;
            domainName = domainName.ToLower();
            return Query.And(Query<AppdomainConfiguration>.EQ(e => e.DomainName, domainName));
        }
    }
}
