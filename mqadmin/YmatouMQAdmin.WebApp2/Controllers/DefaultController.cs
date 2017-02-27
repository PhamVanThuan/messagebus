using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ymatou.CommonService;
using YmatouMQAdmin.Domain.IRepository;
using YmatouMQAdmin.Domain.Specifications;
using YmatouMQAdmin.Repository;
using YmatouMQMessageMongodb.Repository;
using YmatouMQNet4.Configuration;
using YmatouMQAdmin.WebApp2.Models;
using MongoDB.Driver;
using YmatouMQAdmin.Domain.Module;
using YmatouMQMessageMongodb.Domain.IRepository;
using YmatouMQMessageMongodb.Domain.Specifications;
using YmatouMQAdmin.WebApp2.Authorize;
using YmatouMQ.Common.Extensions;
using YmatouMQMessageMongodb.Domain.Module;
using IMQConfigurationRepository = YmatouMQAdmin.Domain.IRepository.IMQConfigurationRepository;
using IMQAppdomainConfigurationRepository = YmatouMQAdmin.Domain.IRepository.IMQAppdomainConfigurationRepository;
using MessageSpec = YmatouMQMessageMongodb.Domain.Specifications.MQMessageSpecifications;
using ReterMessageSpec = YmatouMQMessageMongodb.Domain.Specifications.RetryMessageSpecifications;

namespace YmatouMQAdmin.WebApp2.Controllers
{
    public class DefaultController : Controller
    {
      

        public string MQConfigurationDB
        {
            get
            {

                var _MQConfigurationDB = ConfigurationManager.AppSettings["MQConfigurationDB"];
                if (_MQConfigurationDB != null)
                {
                    return _MQConfigurationDB.ToString();
                }
                else
                {
                    return "MQ_Configuration_201505";
                }
            }
        }

        public string MQAppCfgTable
        {
            get
            {

                var _MQAppCfgTable = ConfigurationManager.AppSettings["MQAppCfgTable"];
                if (_MQAppCfgTable != null)
                {
                    return _MQAppCfgTable.ToString();
                }
                else
                {
                    return "MQ_App_Cfg";
                }
            }
        }

        public string MQDefaultTable
        {
            get
            {

                var _MQDefaultTable = ConfigurationManager.AppSettings["MQDefaultTable"];
                if (_MQDefaultTable != null)
                {
                    return _MQDefaultTable.ToString();
                }
                else
                {
                    return "MQ_Default_Cfg";
                }
            }
        }

        public MQMainConfiguration GetDefaultConfiguration()
        {
            return CfgRepositoryDeclare.cfgRepo.Find(MQCfgControllerSpecifications.MmatchDefaultCfg(), MQConfigurationDB, MQDefaultTable).FirstOrDefault();
        }

        public List<MQMainConfiguration> GetAppConfiguration()
        {
            return CfgRepositoryDeclare.cfgRepo.Find(MQCfgControllerSpecifications.MmatchAppCfg(null), MQConfigurationDB, MQAppCfgTable).ToList();
        }

        public List<MQMainConfiguration> GetAppConfigurationList()
        {
            return CfgRepositoryDeclare.cfgRepo.Find(MQCfgControllerSpecifications.MmatchAppCfg(null), MQConfigurationDB, MQAppCfgTable).Where(c => c.MessageCfgList != null).ToList();
        }

        public MQMainConfiguration GetAppConfigurationByAppId(string appId)
        {
            return CfgRepositoryDeclare.cfgRepo.Find(MQCfgControllerSpecifications.MmatchAppCfg(appId), MQConfigurationDB, MQAppCfgTable).FirstOrDefault();
        }

        [HttpPost]
        public JsonResult AppSearch(string appId)
        {
            ViewBag.appId = appId;
            TempData["AppList"] = GetAppConfiguration().Select(s => s.AppId).ToList();
            var appCfg = GetAppConfigurationByAppId(appId);
            var codes = appCfg.MessageCfgList.Select(s => s.Code).ToList();
            var items = FillSelectList(codes);
            return Json(new { success = true, codeList = items, count = items.Count() });
        }

        private static List<SelectListItem> FillSelectList(List<string> codes)
        {
            var items = new List<SelectListItem>();
            foreach (var str in codes)
            {
                items.Add(new SelectListItem() { Value = str, Text = str });
            }
            return items;
        }

        /// <summary>
        /// 显示全部队列或指定的APPID,Code属性
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="appId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet]
        [CustomerAuth]
        [RoleAuth]
        //
        // GET: /Default/
        public ActionResult AppCodeList([Bind(Prefix = "p")]int? page, [Bind(Prefix = "ps")] int? pageSize, string appId = "default", string code = "_default")
        {
            //变量
            page = page ?? 1;
            pageSize = pageSize ?? 22;
            TempData["AppList"] = GetAppConfiguration().Select(s => s.AppId);
            TempData["appId"] = appId;
            TempData["code"] = code;
            ViewBag.codes = new List<SelectListItem>();
            ViewBag.IsShowList = false;
            //配置项
            var main = GetDefaultConfiguration();
            var codes = new List<AppCodeListModel>();
            if (appId == "default")
                GetAppConfigurationList().EachAction(cfg => BusConfigurationModel.ToAppCodeListModel(main, cfg, codes));
            else
                BusConfigurationModel.ToAppCodeListModel(main, GetAppConfigurationByAppId(appId), codes);
            if (code != "_default")
            {
                codes = codes.Where(c => c.Code == code).ToList();
                var items = new List<SelectListItem>();
                codes.Select(s => s.Code).EachAction(str => items.Add(new SelectListItem() { Value = str, Text = str }));
                ViewBag.codes = items;
            }
            var appList = codes.AsQueryable<AppCodeListModel>().ToPagedList(page.Value, pageSize.Value);
            ViewBag.IsShowList = appList.Count > 0 ? true : false;
            return View(appList);
        }



        [HttpGet]
        [CustomerAuth]
        [RoleAuth]
        public ActionResult MessageStatusSearch([Bind(Prefix = "p")]int? page, [Bind(Prefix = "ps")] int? pageSize
            , string appId, string code, string mid, string status, DateTime? startDate, DateTime? endDate, string clentip)
        {
            //临时变量
            page = page ?? 1;
            pageSize = pageSize ?? 20;
            var _startDate = startDate.HasValue ? startDate.Value : DateTime.Now.Date;
            var _endDate = endDate.HasValue ? endDate.Value : DateTime.Now;
            ViewBag.BeginTime = _startDate.ToString("yyyy-MM-dd HH:mm:ss");
            ViewBag.EndTime = _endDate.ToString("yyyy-MM-dd HH:mm:ss");
            ViewBag.clentip = clentip;
            TempData["appId"] = appId;
            TempData["code"] = code;
            ViewBag.IsShowList = false;
            var mainCfg = GetAppConfiguration();
            TempData["AppList"] = mainCfg.VerifyIsEmptyOrNullEnumerable<MQMainConfiguration>(Enumerable.Empty<MQMainConfiguration>()).Select(s => s.AppId); ;
            if (code == null || code == "default" || mainCfg.IsEmptyEnumerable())
                return View(new PagedList<MessageLogStatus>());
            //消息日志
            var messageList = new List<YmatouMQMessageMongodb.Domain.Module.MQMessage>();
            //消息状态日志
            var messageStatusList = new List<YmatouMQMessageMongodb.Domain.Module.MQMessageStatus>();
            //消息日志&消息状态日志
            IEnumerable<MessageLogStatus> messageLogs = new List<MessageLogStatus>();
            if (!mid.IsEmpty())
            {
                messageList = CfgRepositoryDeclare.NewMsgRepo.Find(MessageSpec.MathchMessageId(mid), GetDbName(appId, _endDate), GetMessageTableName(code)).ToList();
                messageStatusList = CfgRepositoryDeclare.NewStatusRepo.Find(MessageSpec.MatchMessageStatusID(mid), GetMessageStatusDbName(_endDate), GetMessageStatusTableName(appId)).ToList();
                messageLogs = (
                               from m in messageList
                               join s in messageStatusList
                               on m.UuId equals s.MessageUuid into msgs
                               where m.MsgId == mid
                               from msg in msgs.DefaultIfEmpty()
                               select new MessageLogStatus()
                               {
                                   messageId = m.MsgId,
                                   message_aid = m.AppId,
                                   message_code = m.Code,
                                   message_full_body = m.Body.ToString(),
                                   message_body = m.Body.ToString().TrySubString(300),
                                   message_ip = m.Ip,
                                   message_time = m.CreateTime.ToLocalTime(),
                                   status = msg == null ? "NoPush" : msg.Status,
                                   status_time = msg == null ? DateTime.MinValue : msg.CreateTime.ToLocalTime(),
                                   status_cid = msg == null ? null : msg.CallbackId,
                                   status_source = msg == null ? null : msg.HandleSource,
                                   BusReceivedServerIp = m.BusReceivedServerIp,
                                   BusPushServerIp = msg == null ? null : msg.ReceivedMessageIp
                               }).OrderByDescending(o => o.message_time);
            }
            else
            {
                messageList = CfgRepositoryDeclare.NewMsgRepo.Find(MessageSpec.MatchMessageDate(_startDate, _endDate, clentip), GetDbName(appId, _endDate), GetMessageTableName(code)).ToList();
                messageStatusList = CfgRepositoryDeclare.NewStatusRepo.Find(MessageSpec.MatchMessageStatusDate(_startDate, _endDate)
                    , GetMessageStatusDbName(_endDate), GetMessageStatusTableName(appId)).ToList();
                messageLogs = (from m in messageList
                               join s in messageStatusList on m.UuId equals s.MessageUuid into msgs
                               from msg in msgs.DefaultIfEmpty()

                               select new MessageLogStatus()
                               {
                                   messageId = m.MsgId,
                                   message_aid = m.AppId,
                                   message_code = m.Code,
                                   message_full_body = m.Body.ToString(),
                                   message_body = m.Body.ToString().TrySubString(300),
                                   message_ip = m.Ip,
                                   message_time = m.CreateTime.ToLocalTime(),
                                   status = msg == null ? "NoPush" : msg.Status,
                                   status_time = msg == null ? DateTime.MinValue : msg.CreateTime.ToLocalTime(),
                                   status_cid = msg == null ? null : msg.CallbackId,
                                   status_source = msg == null ? null : msg.HandleSource,
                                   BusReceivedServerIp = m.BusReceivedServerIp,
                                   BusPushServerIp = msg == null ? null : msg.ReceivedMessageIp
                               }).OrderByDescending(o => o.message_time);
            }

            var appCfg = GetAppConfigurationByAppId(appId);
            var codes = appCfg.MessageCfgList.Select(s => s.Code).ToList();
            ViewBag.codes = FillSelectList(codes);
            if (status != "all")
                messageLogs = messageLogs.Where(m => m.status_cid.Where(c => c.StartsWith(status)).Any()).ToList();

            var logs = messageLogs.AsQueryable<MessageLogStatus>().ToPagedList(page.Value, pageSize.Value);
            ViewBag.IsShowList = logs.Count > 0 ? true : false;
            return View(logs);
        }

        [HttpGet]
        [CustomerAuth]
        [RoleAuth]
        public ActionResult RetryMessage([Bind(Prefix = "p")]int? page, [Bind(Prefix = "ps")] int? pageSize
            , string appId, string code, string mid, DateTime? startDate, DateTime? endDate, string status)
        {
            page = page ?? 1;
            pageSize = pageSize ?? 20;
            var _startDate = startDate.HasValue ? startDate.Value : DateTime.Now.Date;
            var _endDate = endDate.HasValue ? endDate.Value : DateTime.Now;
            ViewBag.BeginTime = _startDate.ToString("yyyy-MM-dd HH:mm:ss");
            ViewBag.EndTime = _endDate.ToString("yyyy-MM-dd HH:mm:ss");
            TempData["appId"] = appId;
            TempData["code"] = code;
            ViewBag.IsShowList = false;
            var main = GetAppConfiguration();
            var appList = main.VerifyIsEmptyOrNullEnumerable<MQMainConfiguration>(Enumerable.Empty<MQMainConfiguration>()).Select(s => s.AppId).ToList();
            TempData["AppList"] = appList;

            if (appId == null || appId == "default" || code == null || code == "default" || main.IsEmptyEnumerable())
                return View(new PagedList<RetryMessageLogStatus>());

            var retryMessageList = CfgRepositoryDeclare.NewRetryMsgRepo.Find(ReterMessageSpec.MatchRetryMessageDate(_startDate, _endDate), "MQ_Message_Compensate", GetRetryMessageTableName(appId, code)).ToList();
            var retryMessageLogs = new List<RetryMessageLogStatus>();
            if (!mid.IsNullOrEmpty())
            {
                retryMessageLogs = (from r in retryMessageList
                                    where r.MessageId == mid
                                    select new RetryMessageLogStatus()
                                    {
                                        messageId = r.MessageId,
                                        status = r.Status.ToString(),
                                        createTime = r.CreateTime.ToLocalTime(),
                                        expiredTime = r.RetryExpiredTime.ToLocalTime(),
                                        body = r.Body.ToString(),
                                        appKey = r.AppKey,
                                        retryCount = r.RetryCount,
                                        CallbackKey = ToCallbackInfoModels(r.CallbackKey, status),
                                        RetryTime = r.RetryTime.HasValue ? r.RetryTime.Value.ToLocalTime().ToString() : ""
                                    }).OrderByDescending(o => o.createTime).ToList();
            }
            else
            {
                retryMessageLogs = (from r in retryMessageList
                                    select new RetryMessageLogStatus()
                                    {
                                        messageId = r.MessageId,
                                        status = r.Status.ToString(),
                                        createTime = r.CreateTime.ToLocalTime(),
                                        expiredTime = r.RetryExpiredTime.ToLocalTime(),
                                        body = r.Body.ToString(),
                                        appKey = r.AppKey,
                                        retryCount = r.RetryCount,
                                        CallbackKey = ToCallbackInfoModels(r.CallbackKey, status),
                                        RetryTime = r.RetryTime.HasValue ? r.RetryTime.Value.ToLocalTime().ToString() : ""

                                    }).OrderByDescending(o => o.createTime).ToList();
            }
            var codes = main.SingleOrDefault(c => c.AppId == appId).MessageCfgList.Select(s => s.Code).ToList();
            ViewBag.codes = FillSelectList(codes);
            var callback = main.Where(m => m.AppId == appId).SelectMany(_m => _m.MessageCfgList).Where(c => c.Code == code).SelectMany(_c => _c.CallbackCfgList);
            var result = retryMessageLogs.Where(c => c.CallbackKey.Count > 0);
            result.EachAction(r =>
            {
                r.CallbackKey.EachAction(_r =>
                {
                    var _callbackUrl = callback.FirstOrDefault(c => c.CallbackKey == _r.CallbackUrl).Url;
                    _r.FullCallbackUrl = _callbackUrl;
                    _r.CallbackUrl = _callbackUrl.TrySubString(40);
                });
            });
            var logs = result.AsQueryable<RetryMessageLogStatus>().ToPagedList(page.Value, pageSize.Value);
            ViewBag.IsShowList = logs.Count > 0 ? true : false;
            return View(logs);
        }
        private IList<CallbackInfoModels> ToCallbackInfoModels(IEnumerable<CallbackInfo> callbackList, string status)
        {
            var list = new List<CallbackInfoModels>(callbackList.Count());
            callbackList.Where(c => status == "all" ? true : status == c.Status.ToString()).EachAction(c =>
            list.Add(new CallbackInfoModels
            {
                CallbackUrl = c.CallbackKey,
                RetryCount = c.RetryCount,
                Status = c.Status.ToString(),
                FullCallbackUrl = c.CallbackKey
            }));
            return list;
        }
        public static string GetDbName(string appId, DateTime data)
        {
            return string.Format("MQ_Message_{0}_{1}", appId, data.ToString("yyyyMM"));
        }

        public static string GetMessageTableName(string code)
        {
            return string.Format("Message_{0}", code);
        }

        public static string GetMessageStatusDbName(DateTime data)
        {
            return string.Format("MQ_Message_Status_{0}", data.ToString("yyyyMM"));
        }

        public static string GetMessageStatusTableName(string appId)
        {
            return string.Format("mq_subscribe_ok_{0}", appId);
        }

        public static string GetRetryMessageTableName(string appId, string code)
        {
            return string.Format("Mq_{0}_{1}", appId, code);
        }
    }
}