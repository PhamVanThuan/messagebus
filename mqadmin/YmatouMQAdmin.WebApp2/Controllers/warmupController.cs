using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ymatou.CommonService;
using YmatouMQMessageMongodb.Domain.Module;
using YmatouMQNet4.Configuration;

namespace YmatouMQAdmin.WebApp2.Controllers
{
    /// <summary>
    /// 点火
    /// </summary>
    public class warmupController : Controller
    {
        public ActionResult Log()
        {
            return Index();
        }

        public ActionResult Index()
        {
            try
            {
                CfgRepositoryDeclare.MsgRepo.FindOne(Query<YmatouMQAdmin.Domain.Module.MQMessage>.Where(e => !string.IsNullOrEmpty(e.MsgId)));//mongotest                
                CfgRepositoryDeclare.NewRetryMsgRepo.FindOne(Query<RetryMessage>.GTE(e => e.CreateTime, DateTime.Now.AddYears(-1)));//RetryMessageMongoUrl
                CfgRepositoryDeclare.cfgRepo.FindOne(Query<MQMainConfiguration>.Where(e => string.Equals(e.AppId, "default")));//MQConfigurationMongoUrl
                CfgRepositoryDeclare.AlarmRepoInstance.FindOne(Query<Alarm>.EQ(a => a.CallbackId, ""));//AlarmMongoUrl


                ApplicationLog.Debug("点火 成功");

                return Content("ok");
            }
            catch (Exception ex)
            {
                ApplicationLog.Error(string.Format("点火 异常：{0}", ex.Message), ex);
                return Content(string.Format("string:{0}", ex.Message));
            }
        }
    }
}
