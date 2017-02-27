using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Ymatou.CommonService;
using YmatouMQAdmin.Domain.IRepository;
using YmatouMQAdmin.Domain.Module;
using YmatouMQAdmin.Repository;
using YmatouMQ.Common.Dto;
using YmatouMQAdmin.WebApp2.Controllers;

namespace YmatouMQAdmin.WebApp2
{
    public class MQMessageStatusController : ApiController
    {
        //
        // GET: /MQMessageStatus/
        //private static readonly IMessageStatusRepository repo = new MessageStatusRepository();
        [Route("mq/admin/m/MQMessageStatus")]
        public async Task Post([FromBody]MQMessageStatusDto value)
        {
            if (value == null) return;
            var info = new MQMessageStatus(value.MsgUniqueId, value.Status.ToString(), value.AppId);
            await CfgRepositoryDeclare.statusRepo.AddAsync(info, info.AssignCollectionName(), TimeSpan.FromMilliseconds(2000));
            ApplicationLog.Debug(string.Format("message {0} add to mongodb", value.MsgUniqueId));
        }
    }
}
