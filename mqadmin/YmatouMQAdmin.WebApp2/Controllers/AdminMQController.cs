using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using YmatouMQAdmin.Domain.IRepository;
using YmatouMQAdmin.Domain.Specifications;
using YmatouMQAdmin.Repository;
using YmatouMQNet4.Configuration;
using YmatouMQAdmin.WebApp2.Models;
using System.Collections;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using System.Configuration;
using Ymatou.CommonService;
using System.Diagnostics;
using YmatouMessageBusClientNet4;
using YmatouMessageBusClientNet4.Dto;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using YmatouMQAdmin.WebApp2.Authorize;
using System.Reflection;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Extensions.Serialization;
using YmatouMQMessageMongodb.Domain.Specifications;

namespace YmatouMQAdmin.WebApp2.Controllers
{
    [CustomerAuth]
    public class AdminMQController : Controller
    {

        private static void _JsonSettings()
        {
            Newtonsoft.Json.JsonConvert.DefaultSettings = () =>
            {
                return new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.None,
                    TypeNameHandling = TypeNameHandling.None
                };
            };
        }

        private bool IsDisplayDictionaryControl
        {
            get
            {
                var flag = ConfigurationManager.AppSettings["IsDisplayDictionaryControl"];
                if (flag != null)
                {
                    return flag.ToString() == "true" ? true : false;
                }
                else
                {
                    return false;
                }
            }
        }

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

        //
        public string MQAppDomainTable
        {
            get
            {

                var _MQAppDomainTable = ConfigurationManager.AppSettings["MQAppDomainTable"];
                if (_MQAppDomainTable != null)
                {
                    return _MQAppDomainTable.ToString();
                }
                else
                {
                    return "MQ_Appdomain_Cfg";
                }
            }
        }
        //

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
        //
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
        private List<string> ContextTypeArray
        {
            get
            {
                List<string> list = new List<string>();

                var contextType = ConfigurationManager.AppSettings["ContextTypeArray"];
                if (contextType != null)
                {
                    if (!contextType.ToString().Contains("未设置"))
                    {
                        list.Add("未设置");
                    }
                    var array = contextType.ToString().Split(',');
                    foreach (var context in array)
                    {
                        list.Add(context);
                    }
                }
                else
                {
                    list.Add("application/json");
                }
                if (!list.Contains("未设置"))
                {
                    list.Add("未设置");
                }
                return list;
            }
        }
        private List<string> ContentEncodingArray
        {
            get
            {
                List<string> list = new List<string>();

                var ContentEncoding = ConfigurationManager.AppSettings["ContentEncodingArray"];
                if (ContentEncoding != null)
                {
                    if (!ContentEncoding.ToString().Contains("未设置"))
                    {
                        list.Add("未设置");
                    }
                    var array = ContentEncoding.ToString().Split(',');
                    foreach (var Content in array)
                    {
                        list.Add(Content);
                    }
                }
                else
                {
                    list.Add("UTF-8");
                }

                if (!list.Contains("未设置"))
                {
                    list.Add("未设置");
                }
                return list;
            }
        }

        private List<string> httpType
        {
            get
            {
                List<string> list = new List<string>();

                var httpType = ConfigurationManager.AppSettings["httpType"];
                if (httpType != null)
                {
                    if (!httpType.ToString().Contains("未设置"))
                    {
                        list.Add("未设置");
                    }
                    var array = httpType.ToString().Split(',');
                    foreach (var type in array)
                    {
                        list.Add(type);
                    }
                }
                else
                {
                    list.Add("POST");
                    list.Add("GET");
                }
                if (!list.Contains("未设置"))
                {
                    list.Add("未设置");
                }
                return list;
            }
        }
        private Dictionary<string, int> RetryCount
        {
            get
            {
                Dictionary<string, int> dic = new Dictionary<string, int>();
                var _RetryCount = ConfigurationManager.AppSettings["RetryCount"];
                if (_RetryCount != null)
                {
                    var counts = _RetryCount.ToString().Split(',');
                    foreach (var count in counts)
                    {
                        dic.Add(count == "-1" ? "未设置" : count, int.Parse(count));
                    }
                }
                else
                {
                    dic.Add("1", 1);
                    dic.Add("2", 2);
                }
                return dic;
            }
        }

        private List<int> DomainConnectionPool
        {
            get
            {
                List<int> list = new List<int>();
                var domainConnectionPool = ConfigurationManager.AppSettings["DomainConnectionPool"];
                if (domainConnectionPool != null)
                {
                    var pools = domainConnectionPool.ToString().Split(',');
                    foreach (var pool in pools)
                    {
                        list.Add(int.Parse(pool));
                    }
                }
                else
                {
                    list.Add(1);
                    list.Add(2);
                    list.Add(3);
                }
                return list;
            }

        }
        public Dictionary<string, int> RetryMillisecond
        {
            get
            {
                Dictionary<string, int> dic = new Dictionary<string, int>();
                var _RetryMillisecond = ConfigurationManager.AppSettings["RetryMillisecond"];
                if (_RetryMillisecond != null)
                {
                    var counts = _RetryMillisecond.ToString().Split(',');
                    foreach (var count in counts)
                    {
                        dic.Add(count == "-1" ? "未设置" : count, int.Parse(count));
                    }
                }
                else
                {
                    dic.Add("500", 500);
                    dic.Add("1000", 1000);
                }
                return dic;
            }
        }

        public Dictionary<string, int> IsRetry
        {
            get
            {
                Dictionary<string, int> dic = new Dictionary<string, int>();
                var _IsRetry = ConfigurationManager.AppSettings["IsRetry"];
                if (_IsRetry != null)
                {
                    var counts = _IsRetry.ToString().Split(',');
                    foreach (var count in counts)
                    {
                        if (count == "-1")
                        {
                            dic.Add("未设置", -1);
                        }
                        if (count == "0")
                        {
                            dic.Add("不补发", 0);
                        }
                        if (count == "1")
                        {
                            dic.Add("补发", 1);
                        }
                    }
                }
                else
                {
                    dic.Add("未设置", -1);
                    dic.Add("不补发", 0);
                    dic.Add("补发", 1);
                }
                return dic;
            }
        }

        public Dictionary<string, int> buffer
        {
            get
            {

                Dictionary<string, int> dic = new Dictionary<string, int>();
                var _buffer = ConfigurationManager.AppSettings["buffer"];
                if (_buffer != null)
                {
                    var counts = _buffer.ToString().Split(',');
                    foreach (var count in counts)
                    {
                        dic.Add(count == "-1" ? "未设置" : count, int.Parse(count));
                    }
                }
                else
                {
                    dic.Add("0", 0);
                    dic.Add("1", 1);
                }
                return dic;
            }
        }

        public List<AppdomainConfiguration> GetAppdomainConfiguration()
        {
            return CfgRepositoryDeclare.cfgAppdomainRepo.Find(MQAppdomainConfigurationSpecifications.MatchAppdomain(null), MQConfigurationDB, MQAppDomainTable).ToList();
        }

        public MQMainConfiguration GetDefaultConfiguration()
        {
            return CfgRepositoryDeclare.cfgRepo.Find(MQCfgControllerSpecifications.MmatchDefaultCfg(), MQConfigurationDB, MQDefaultTable).FirstOrDefault();
        }

        public List<MQMainConfiguration> GetAppConfiguration()
        {
            return CfgRepositoryDeclare.cfgRepo.Find(MQCfgControllerSpecifications.MmatchAppCfg(null), MQConfigurationDB, MQAppCfgTable).ToList();
        }

        public MQMainConfiguration GetAppConfigurationByAppId(string appId)
        {
            return CfgRepositoryDeclare.cfgRepo.Find(MQCfgControllerSpecifications.MmatchAppCfg(appId), MQConfigurationDB, MQAppCfgTable).FirstOrDefault();
        }


        public ActionResult DomainIndex()
        {
            List<MQMainConfiguration> main = GetAppConfiguration();
            List<string> appList = main.Select(s => s.AppId).ToList();
            TempData["AppList"] = appList;


            return View();
        }

        [RoleAuth]
        [HttpGet]
        public ActionResult AppDefault(string appId = "default")
        {
            string _appId = appId == "default" ? null : appId;
            ApplicationLog.Debug("AppDefault 开始获取mongodb数据，appid" + appId);
            List<MQMainConfiguration> main = GetAppConfiguration();
            List<string> appList = main.Select(s => s.AppId).ToList();
            TempData["AppList"] = appList;


            return View();
        }

        [RoleAuth]
        [HttpGet]
        public ActionResult AppGlobal(string appId = "default")
        {
            string _appId = appId == "default" ? null : appId;
            List<MQMainConfiguration> main = GetAppConfiguration();
            List<string> appList = main.Select(s => s.AppId).ToList();
            TempData["AppList"] = appList;


            return View();
        }
        [RoleAuth]
        [HttpGet]
        public ActionResult EditAppQueueStatusJson()
        {
            List<MQMainConfiguration> main = GetAppConfiguration();
            List<string> appList = main.Select(s => s.AppId).ToList();
            TempData["QueueStatusJson"] = appList;


            return View();
        }
        //[RoleAuth]
        [HttpPost]
        public ActionResult SaveEditAppQueueStatusJson(string domainId)
        {
            var domainCfg = CfgRepositoryDeclare.cfgAppdomainRepo.FindOne(MQAppdomainConfigurationSpecifications.MatchOneAppdomain("{0}{1}".Fomart("ad_", domainId))
                , MQConfigurationDB, MQAppDomainTable);
            var appString = "not find {0}".Fomart(domainId);
            if (domainCfg != null)
                appString = Newtonsoft.Json.JsonConvert.SerializeObject(domainCfg, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            ViewBag.DomainCfg = appString;

            return View();
        }
        public ActionResult _SaveEditAppQueueStatusJson(string queueJson)
        {
            bool flag = false;
            try
            {
                var domainCfg = queueJson._JSONDeserializeFromString<AppdomainConfiguration>();

                if (domainCfg != null)
                {
                    CfgRepositoryDeclare.cfgAppdomainRepo.Save(domainCfg, MQConfigurationDB, MQAppDomainTable);

                    TempData["message"] = "修改成功";

                    ApplicationLog.Info(string.Concat("修改应用AppId:", domainCfg.DomainName));

                    flag = true;
                }
            }
            catch (Exception ex)
            {
                TempData["message"] = "修改失败" + ex.Message;
                ApplicationLog.Error(ex.Message);

            }

            return Json(new { success = flag });
        }

        [RoleAuth]
        [HttpGet]
        public ActionResult CallBackIndex(string appId = "default")
        {
            string _appId = appId == "default" ? null : appId;
            List<MQMainConfiguration> main = GetAppConfiguration();
            List<string> appList = main.Select(s => s.AppId).ToList();
            TempData["AppList"] = appList;


            return View();
        }

        [HttpPost]
        public JsonResult AppSearch(string appId)
        {
            var main = GetAppConfiguration();
            List<string> appList = main.Select(s => s.AppId).ToList();
            TempData["AppList"] = appList;

            var appCfg = main.SingleOrDefault(c => c.AppId == appId);
            var codes = appCfg.MessageCfgList.VerifyIsEmptyOrNullEnumerable(Enumerable.Empty<MessageConfiguration>()).Select(s => s.Code).ToList();

            ViewBag.appId = appId;
            var items = FillSelectList(codes);
            return Json(new { success = true, codeList = items, count = items.Count() });
        }

        [HttpPost]
        public JsonResult AppDomainSearch(string appId)
        {
            var appDomainModel = CfgRepositoryDeclare.cfgAppdomainRepo.Find(MQAppdomainConfigurationSpecifications.MatchOneAppdomain(string.Concat("ad_", appId)), MQConfigurationDB, MQAppDomainTable).FirstOrDefault();

            List<string> domainList = new List<string>();

            if (appDomainModel != null)
            {
                if (appDomainModel.Items != null)
                {
                    domainList = appDomainModel.Items.Select(s => s.Code).ToList();
                }

            }

            List<MQMainConfiguration> main = GetAppConfiguration();
            List<string> appList = main.Select(s => s.AppId).ToList();
            TempData["AppList"] = appList;

            MQMainConfiguration appCfg = GetAppConfigurationByAppId(appId);
            List<string> codes = appCfg.MessageCfgList.Select(s => s.Code).ToList();

            codes = codes.Except(domainList).ToList();

            codes.Remove("");


            ViewBag.appId = appId;

            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var str in codes)
            {
                items.Add(new SelectListItem() { Value = str, Text = str });
            }

            return Json(new { success = true, codeList = items, count = items.Count() });
        }


        public ActionResult EditApp(string appId, string code)
        {
            MQMainConfiguration main = GetAppConfigurationByAppId(appId);
            MQConfigurationDto model = GetMQConfiguration(main, code);

            ArrayList array = new ArrayList();
            array.Add(true);
            array.Add(false);

            ViewBag.ContextTypeArray = ContextTypeArray;
            ViewBag.ContentEncodingArray = ContentEncodingArray;
            ViewBag.boolArray = array;
            ViewBag.buffers = buffer;
            ViewBag.PublishRetryCount = RetryCount;
            ViewBag.PublishRetryMillisecond = RetryMillisecond;
            ViewBag.httpType = httpType;
            ViewBag.IsDisplayDictionaryControl = IsDisplayDictionaryControl;
            ViewBag.IsRetry = IsRetry;
            List<SelectListItem> item = new List<SelectListItem>();

            Dictionary<int, string> ExchangeTypeDic = new Dictionary<int, string>();

            foreach (var str in Enum.GetNames(typeof(ExchangeType)))
            {
                ExchangeType t = (ExchangeType)Enum.Parse(typeof(ExchangeType), str);
                item.Add(new SelectListItem() { Text = str, Value = ((int)t).ToString() });
                if (str == "direct" || str == "topic")
                {
                    if (!ExchangeTypeDic.ContainsKey((int)t))
                    {
                        ExchangeTypeDic.Add((int)t, str);
                    }
                }
            }
            ViewBag.ExchangeTypeItems = item;
            ViewBag.ExchangeTypeDic = ExchangeTypeDic;
            return View(model);
        }

        public ActionResult EditCurrentApp(string appId)
        {
            MQMainConfiguration main = GetAppConfigurationByAppId(appId);

            string appString = Newtonsoft.Json.JsonConvert.SerializeObject(main, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            ViewBag.GlobalApp = appString;

            return View();
        }

        [HttpPost]
        public JsonResult EditGlobalApp(string appContent)
        {
            var flag = false;
            var _msg = string.Empty;
            try
            {
                var mqModel = Newtonsoft.Json.JsonConvert.DeserializeObject<MQMainConfiguration>(appContent, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                if (mqModel != null)
                {
                    CfgRepositoryDeclare.cfgRepo.Save(mqModel, MQConfigurationDB, MQAppCfgTable);
                    ApplicationLog.Info(string.Concat("修改应用AppId:", mqModel.AppId));
                    _msg = "修改成功";
                    flag = true;
                }
                else
                {
                    _msg = "反序列化后Model为空";
                }
            }
            catch (Exception ex)
            {
                TempData["message"] = "修改失败" + ex.Message;
                ApplicationLog.Error(ex.Message);
                return Json(new { success = flag, msg = "修改失败" + ex.Message });

            }

            return Json(new { success = flag, msg = _msg });
        }

        public ActionResult EditCallbackConfiguration(string appId, string code)
        {

            MQMainConfiguration main = GetAppConfigurationByAppId(appId);

            MQConfigurationDto model = GetMQConfiguration(main, code);

            ViewBag.ContextTypeArray = ContextTypeArray;

            ViewBag.buffers = buffer;

            ViewBag.httpType = httpType;

            ViewBag.IsRetry = IsRetry;

            ViewBag.appId = appId;
            ViewBag.code = code;

            return View(model.MessageCfgList.CallbackCfgList);
        }



        public MQConfigurationDto GetMQConfiguration(MQMainConfiguration main, string code)
        {
            var connectionDic = BusConfigurationModel.ConnectionStringToDictionary(main.ConnCfg.ConnectionString);
            MQConfigurationDto model = new MQConfigurationDto()
            {
                AppId = main.AppId,
                Version = main.Version,
                MessageCfgList = main.MessageCfgList.Where(m => m.Code == code).FirstOrDefault(),
                host = connectionDic.TryGetVal("host"),
                port = connectionDic.TryGetVal("port"),
                vhost = connectionDic.TryGetVal("vhost"),
                userName = connectionDic.TryGetVal("unmae"),
                password = connectionDic.TryGetVal("pas"),
                owerhost = main.OwnerHost,
                connectionPool = "{0}-{1}".Fomart(connectionDic.TryGetVal("poominsize", "3"), connectionDic.TryGetVal("poomaxsize", "10")),
                channelPool = "{0}-{1}".Fomart(connectionDic.TryGetVal("channelpoolminsize", "10"), connectionDic.TryGetVal("channelpoolmaxsize", "30")),
            };
            if (model.MessageCfgList != null)
            {
                JavaScriptSerializer jserializer = new JavaScriptSerializer();
                //MessageCfgList.QueueCfg
                if (model.MessageCfgList.ConsumeCfg != null)
                {
                    if (model.MessageCfgList.ConsumeCfg.Args != null)
                    {
                        ViewBag.MessageCfgList_ConsumeCfg_args = jserializer.Serialize(model.MessageCfgList.ConsumeCfg.Args);
                    }
                }
                if (model.MessageCfgList.ExchangeCfg != null)
                {
                    if (model.MessageCfgList.ExchangeCfg.Arguments != null)
                    {
                        ViewBag.MessageCfgList_ExchangeCfg_Arguments = jserializer.Serialize(model.MessageCfgList.ExchangeCfg.Arguments);
                    }
                }
                if (model.MessageCfgList.QueueCfg != null)
                {
                    if (model.MessageCfgList.QueueCfg.Args != null)
                    {
                        ViewBag.MessageCfgList_QueueCfg_args = jserializer.Serialize(model.MessageCfgList.QueueCfg.Args);
                    }
                    if (model.MessageCfgList.QueueCfg.HeadArgs != null)
                    {
                        ViewBag.MessageCfgList_QueueCfg_headArgs = jserializer.Serialize(model.MessageCfgList.QueueCfg.HeadArgs);
                    }
                }
            }
            else
            {
                model.MessageCfgList = new MessageConfiguration();
                model.MessageCfgList.ConsumeCfg = new ConsumeConfiguration();

                model.MessageCfgList.ExchangeCfg = new ExchangeConfiguration();
                model.MessageCfgList.MessagePropertiesCfg = new MessagePropertiesConfiguration();
                model.MessageCfgList.PublishCfg = new PublishConfiguration();
                model.MessageCfgList.QueueCfg = new QueueConfiguration();

            }
            if (model.MessageCfgList.ConsumeCfg == null)
            {
                model.MessageCfgList.ConsumeCfg = new ConsumeConfiguration();
            }
            if (model.MessageCfgList.ExchangeCfg == null)
            {
                model.MessageCfgList.ExchangeCfg = new ExchangeConfiguration();
            }
            if (model.MessageCfgList.MessagePropertiesCfg == null)
            {
                model.MessageCfgList.MessagePropertiesCfg = new MessagePropertiesConfiguration();
            }
            if (model.MessageCfgList.PublishCfg == null)
            {
                model.MessageCfgList.PublishCfg = new PublishConfiguration();
            }
            if (model.MessageCfgList.QueueCfg == null)
            {
                model.MessageCfgList.QueueCfg = new QueueConfiguration();
            }

            if (model.MessageCfgList.CallbackCfgList == null)
            {
                CallbackConfiguration callback = new CallbackConfiguration()
                {
                    AcceptMessageTimeRange = "",
                    CallbackKey = "",
                    CallbackTimeOut = null,
                    ContentType = "",
                    Enable = false,
                    HttpMethod = "",
                    IsRetry = null,
                    Priority = null,
                    Url = "",

                };
                List<CallbackConfiguration> callList = new List<CallbackConfiguration>();
                callList.Add(callback);

                model.MessageCfgList.CallbackCfgList = callList;
            }
            else
            {


                List<CallbackConfiguration> callList = new List<CallbackConfiguration>();
                model.MessageCfgList.CallbackCfgList.ToList().ForEach(c => callList.Add(new CallbackConfiguration()
                    {
                        IsApproveRetry = (c.IsRetry.HasValue ? (c.IsRetry.Value == 1 ? true : false) : false),
                        ApproveEnable = c.Enable.HasValue ? c.Enable.Value : false,
                        Url = c.Url,
                        Priority = c.Priority,
                        AcceptMessageTimeRange = c.AcceptMessageTimeRange,
                        CallbackKey = c.CallbackKey,
                        CallbackTimeOut = c.CallbackTimeOut,
                        ContentType = c.ContentType,
                        Enable = c.Enable,
                        HttpMethod = c.HttpMethod,
                        IsRetry = c.IsRetry
                    }));
                model.MessageCfgList.CallbackCfgList = callList;
            }

            return model;
        }

        [RoleAuth]
        [HttpGet]
        public ActionResult Index(string code = "default")
        {
            MQMainConfiguration main = GetDefaultConfiguration();
            ViewBag.ContextTypeArray = ContextTypeArray;
            ViewBag.ContentEncodingArray = ContentEncodingArray;
            ArrayList array = new ArrayList();
            array.Add(true);
            array.Add(false);
            ViewBag.boolArray = array;
            ViewBag.buffers = buffer;
            ViewBag.PublishRetryCount = RetryCount;
            ViewBag.PublishRetryMillisecond = RetryMillisecond;
            ViewBag.httpType = httpType;
            ViewBag.IsDisplayDictionaryControl = IsDisplayDictionaryControl;
            ViewBag.IsRetry = IsRetry;
            List<SelectListItem> item = new List<SelectListItem>();
            Dictionary<int, string> ExchangeTypeDic = new Dictionary<int, string>();
            foreach (var str in Enum.GetNames(typeof(ExchangeType)))
            {
                ExchangeType t = (ExchangeType)Enum.Parse(typeof(ExchangeType), str);
                item.Add(new SelectListItem() { Text = str, Value = ((int)t).ToString() });
                if (str == "direct" || str == "headers")
                {
                    if (!ExchangeTypeDic.ContainsKey((int)t))
                    {
                        ExchangeTypeDic.Add((int)t, str);
                    }
                }

            }
            ViewBag.ExchangeTypeItems = item;
            ViewBag.ExchangeTypeDic = ExchangeTypeDic;
            MQConfigurationDto model = GetMQConfiguration(main, code);
            return View(model);
        }

        //更新默认全局配置
        [HttpPost]
        public ActionResult UpdateDefaultConfiguration(MQConfigurationDto model, List<CallbackConfiguration> callback)
        {
            try
            {
                var defaultCfg = GetDefaultConfiguration();
                BusConfigurationModel.SetVersion(defaultCfg);
                BusConfigurationModel.SetConnCfg(defaultCfg, model);
                BusConfigurationModel.SetMessagePropertiesCfg(defaultCfg.MessageCfgList.First().MessagePropertiesCfg, model);
                BusConfigurationModel.SetPublishCfg(defaultCfg.MessageCfgList.First().PublishCfg, model);
                BusConfigurationModel.SetConsumeCfg(defaultCfg.MessageCfgList.First().ConsumeCfg, model);
                BusConfigurationModel.SetQueryCfg(defaultCfg.MessageCfgList.First().QueueCfg, model);
                BusConfigurationModel.SetExchangCfg(defaultCfg.MessageCfgList.First().ExchangeCfg, model);
                BusConfigurationModel.SetCallbackCfg("default", defaultCfg, callback);
                CfgRepositoryDeclare.cfgRepo.Save(defaultCfg, MQConfigurationDB, MQDefaultTable);
                return this.JavaScriptResultResponse("busalert('修改成功');");
            }
            catch (Exception ex)
            {
                return this.JavaScriptResultResponse("busalert('修改失败'" + ex.Message + ");");
            }
        }
        /// <summary>
        /// 保存具体配置。liguo 2015-12-11
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateCodeDetailsCfg(MQConfigurationDto model)
        {
            try
            {
                var main = GetAppConfigurationByAppId(model.AppId);
                BusConfigurationModel.SetVersion(main);
                BusConfigurationModel.SetConnCfg(main, model);
                var code_Cfg = main.MessageCfgList.SingleOrDefault(c => c.Code == model.MessageCfgList.Code);
                BusConfigurationModel.SetCodeEnable(code_Cfg, model);
                code_Cfg.MessagePropertiesCfg = BusConfigurationModel.SetMessagePropertiesCfg(code_Cfg.MessagePropertiesCfg, model);
                code_Cfg.PublishCfg = BusConfigurationModel.SetPublishCfg(code_Cfg.PublishCfg, model);
                code_Cfg.ConsumeCfg = BusConfigurationModel.SetConsumeCfg(code_Cfg.ConsumeCfg, model);
                CfgRepositoryDeclare.cfgRepo.Save(main, MQConfigurationDB, MQAppCfgTable);
                return this.JavaScriptResultResponse("busalert('appid {0},code {1} 修改成功');".Fomart(model.AppId, model.MessageCfgList.Code));
            }
            catch (Exception ex)
            {
                ApplicationLog.Error(ex.Message);
                return this.JavaScriptResultResponse("busalert('修改失败'" + ex.Message + ");");
            }
        }
        //修改回调配置 lg 2015-12-11
        [HttpPost]
        public ActionResult SaveCallbackConfiguration(string appId, string code, List<CallbackConfiguration> callback)
        {
            try
            {
                var mainCfg = GetAppConfigurationByAppId(appId);
                BusConfigurationModel.SetVersion(mainCfg);
                BusConfigurationModel.SetCallbackCfg(code, mainCfg, callback);
                CfgRepositoryDeclare.cfgRepo.Save(mainCfg, MQConfigurationDB, MQAppCfgTable);
                ApplicationLog.Info(string.Concat("修改应用AppId:", appId));
                return this.JavaScriptResultResponse("busalert('appid :{0},code:{1} 修改成功');".Fomart(appId, code));
            }
            catch (Exception ex)
            {
                ApplicationLog.Error(ex.Message);
                return this.JavaScriptResultResponse("busalert('修改失败 " + ex.Message + "');");
            }
        }

        public MessageConfiguration GetMessageConfiguration(MQConfigurationDto model, List<CallbackConfiguration> callback)
        {
            _JsonSettings();
            string ConsumeCfg_arg = string.Empty;
            string ExchangeArguments = string.Empty;
            string QueueArgs = string.Empty;
            string queueHeadArgs = string.Empty;
            if (IsDisplayDictionaryControl)
            {
                if (Request.Form.Get("MessageCfgList.ConsumeCfg.Args") != null)
                {
                    if (Request.Form.Get("MessageCfgList.ConsumeCfg.Args") != "{value:null}")
                    {
                        ConsumeCfg_arg = Request.Form.Get("MessageCfgList.ConsumeCfg.Args").ToString();
                    }
                }
                if (Request.Form.Get("MessageCfgList.ExchangeCfg.Arguments") != null)
                {
                    if (Request.Form.Get("MessageCfgList.ExchangeCfg.Arguments") != "{value:null}")
                    {
                        ExchangeArguments = Request.Form.Get("MessageCfgList.ExchangeCfg.Arguments").ToString();
                    }
                }
                if (Request.Form.Get("MessageCfgList.QueueCfg.Args") != null)
                {
                    if (Request.Form.Get("MessageCfgList.QueueCfg.Args") != "{value:null}")
                    {
                        QueueArgs = Request.Form.Get("MessageCfgList.QueueCfg.Args").ToString();
                    }
                }
                if (Request.Form.Get("MessageCfgList.QueueCfg.HeadArgs") != null)
                {
                    if (Request.Form.Get("MessageCfgList.QueueCfg.HeadArgs") != "{value:null}")
                    {
                        queueHeadArgs = Request.Form.Get("MessageCfgList.QueueCfg.HeadArgs").ToString();
                    }
                }

            }

            ConsumeConfiguration _ConsumeCfg = new ConsumeConfiguration();
            MessagePropertiesConfiguration _MessagePropertiesCfg = new MessagePropertiesConfiguration();
            PublishConfiguration _PublishCfg = new PublishConfiguration();

            Type consume_type = model.MessageCfgList.ConsumeCfg.GetType();

            PropertyInfo[] consumeProperties = consume_type.GetProperties();

            bool consume_flag = true;
            bool messagecfg_flag = true;

            foreach (var prop in consumeProperties)
            {
                object obj = prop.GetValue(model.MessageCfgList.ConsumeCfg);
                if (obj != null)
                {
                    _ConsumeCfg = new ConsumeConfiguration()
                    {

                        Args = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(ConsumeCfg_arg),
                        RoutingKey = model.MessageCfgList.ConsumeCfg.RoutingKey,
                        IsAutoAcknowledge = model.MessageCfgList.ConsumeCfg.IsAutoAcknowledge,
                        HandleFailAcknowledge = model.MessageCfgList.ConsumeCfg.HandleFailAcknowledge,
                        PrefetchCount = model.MessageCfgList.ConsumeCfg.PrefetchCount,
                        MaxThreadCount = model.MessageCfgList.ConsumeCfg.MaxThreadCount,
                        HandleSuccessSendNotice = model.MessageCfgList.ConsumeCfg.HandleSuccessSendNotice,
                        RetryTimeOut = model.MessageCfgList.ConsumeCfg.RetryTimeOut
                    };
                    consume_flag = false;
                    break;
                }
            }
            if (consume_flag)
            {
                _ConsumeCfg = null;
            }


            Type messageCfg_type = model.MessageCfgList.MessagePropertiesCfg.GetType();
            PropertyInfo[] messageProperties = messageCfg_type.GetProperties();
            foreach (var prop in messageProperties)
            {
                object obj = prop.GetValue(model.MessageCfgList.MessagePropertiesCfg);
                //string other = obj != null ? (obj.ToString() == "-1" ? int.Parse(obj.ToString()) : int.Parse(obj.ToString())) : null;
                if (obj != null)
                {
                    if (obj.ToString() != "未设置")
                    {
                        if (obj is int)
                        {
                            int o = (int)obj;
                            if (o != -1)
                            {
                                _MessagePropertiesCfg = new MessagePropertiesConfiguration()
                                {
                                    ContentEncoding = model.MessageCfgList.MessagePropertiesCfg.ContentEncoding == "未设置" ? null : model.MessageCfgList.MessagePropertiesCfg.ContentEncoding,
                                    ContextType = model.MessageCfgList.MessagePropertiesCfg.ContextType == "未设置" ? null : model.MessageCfgList.MessagePropertiesCfg.ContextType,
                                    Expiration = model.MessageCfgList.MessagePropertiesCfg.Expiration,
                                    PersistentMessages = model.MessageCfgList.MessagePropertiesCfg.PersistentMessages,
                                    PersistentMessagesLocal = model.MessageCfgList.MessagePropertiesCfg.PersistentMessagesLocal,
                                    PersistentMessagesMongo = model.MessageCfgList.MessagePropertiesCfg.PersistentMessagesMongo,
                                    Priority = model.MessageCfgList.MessagePropertiesCfg.Priority == -1 ? null : model.MessageCfgList.MessagePropertiesCfg.Priority
                                };

                                messagecfg_flag = false;
                                break;
                            }
                        }
                        else
                        {
                            _MessagePropertiesCfg = new MessagePropertiesConfiguration()
                            {
                                ContentEncoding = model.MessageCfgList.MessagePropertiesCfg.ContentEncoding == "未设置" ? null : model.MessageCfgList.MessagePropertiesCfg.ContentEncoding,
                                ContextType = model.MessageCfgList.MessagePropertiesCfg.ContextType == "未设置" ? null : model.MessageCfgList.MessagePropertiesCfg.ContextType,
                                Expiration = model.MessageCfgList.MessagePropertiesCfg.Expiration,
                                PersistentMessages = model.MessageCfgList.MessagePropertiesCfg.PersistentMessages,
                                PersistentMessagesLocal = model.MessageCfgList.MessagePropertiesCfg.PersistentMessagesLocal,
                                PersistentMessagesMongo = model.MessageCfgList.MessagePropertiesCfg.PersistentMessagesMongo,
                                Priority = model.MessageCfgList.MessagePropertiesCfg.Priority == -1 ? null : model.MessageCfgList.MessagePropertiesCfg.Priority
                            };
                            messagecfg_flag = false;
                            break;
                        }
                    }

                }
            }
            if (messagecfg_flag)
            {
                _MessagePropertiesCfg = null;
            }

            bool publish_flag = true;
            Type publish_type = model.MessageCfgList.PublishCfg.GetType();
            PropertyInfo[] publishProperties = publish_type.GetProperties();
            foreach (var prop in publishProperties)
            {
                object obj = prop.GetValue(model.MessageCfgList.PublishCfg);
                if (obj != null)
                {
                    _PublishCfg = new PublishConfiguration()
                    {
                        MemoryQueueLimit = model.MessageCfgList.PublishCfg.MemoryQueueLimit,
                        PublisherConfirms = model.MessageCfgList.PublishCfg.PublisherConfirms,
                        RetryCount = model.MessageCfgList.PublishCfg.RetryCount.HasValue ? (int.Parse(model.MessageCfgList.PublishCfg.RetryCount.Value.ToString()) == -1 ? null : model.MessageCfgList.PublishCfg.RetryCount) : null,
                        RetryMillisecond = model.MessageCfgList.PublishCfg.RetryMillisecond.HasValue ? (int.Parse(model.MessageCfgList.PublishCfg.RetryMillisecond.Value.ToString()) == -1 ? null : model.MessageCfgList.PublishCfg.RetryMillisecond) : null,
                        RouteKey = model.MessageCfgList.PublishCfg.RouteKey
                    };
                    publish_flag = false;
                    break;
                }
            }

            if (publish_flag)
            {
                _PublishCfg = null;
            }


            MessageConfiguration messageModel = new MessageConfiguration()
            {
                Code = model.MessageCfgList.Code,
                Enable = model.MessageCfgList.Enable,
                ConsumeCfg = _ConsumeCfg,
                ExchangeCfg = new ExchangeConfiguration()
                {
                    _ExchangeType = model.MessageCfgList.ExchangeCfg._ExchangeType != null ? ((int)model.MessageCfgList.ExchangeCfg._ExchangeType == 0 ? null : model.MessageCfgList.ExchangeCfg._ExchangeType) : null,
                    Arguments = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(ExchangeArguments),
                    Durable = model.MessageCfgList.ExchangeCfg.Durable,
                    ExchangeName = model.MessageCfgList.ExchangeCfg.ExchangeName,
                    IsExchangeAutoDelete = model.MessageCfgList.ExchangeCfg.IsExchangeAutoDelete
                },
                MessagePropertiesCfg = _MessagePropertiesCfg,
                PublishCfg = _PublishCfg,
                QueueCfg = new QueueConfiguration()
                {
                    Args = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(QueueArgs),
                    HeadArgs = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(queueHeadArgs),
                    IsAutoDelete = model.MessageCfgList.QueueCfg.IsAutoDelete,
                    IsDurable = model.MessageCfgList.QueueCfg.IsDurable,
                    IsQueueExclusive = model.MessageCfgList.QueueCfg.IsQueueExclusive,
                    QueueName = model.MessageCfgList.QueueCfg.QueueName
                }

            };

            List<CallbackConfiguration> callbacks = new List<CallbackConfiguration>();

            callback.ForEach(f => callbacks.Add(new CallbackConfiguration()
            {
                Enable = f.Enable,
                CallbackKey = f.CallbackKey,
                AcceptMessageTimeRange = f.AcceptMessageTimeRange,
                CallbackTimeOut = f.CallbackTimeOut,
                ContentType = f.ContentType == "未设置" ? null : f.ContentType,
                HttpMethod = f.HttpMethod == "未设置" ? null : f.HttpMethod,
                IsRetry = f.IsRetry.HasValue ? (f.IsRetry.Value == -1 ? null : (int?)f.IsRetry.Value) : null,
                Priority = f.Priority,
                Url = f.Url
            }));


            messageModel.CallbackCfgList = callbacks;

            return messageModel;
        }

        [HttpGet]
        public ActionResult CreateApp()
        {
            AppInitializeModel model = new AppInitializeModel();


            ArrayList array = new ArrayList();
            array.Add(true);
            array.Add(false);

            ViewBag.ContextTypeArray = ContextTypeArray;
            ViewBag.ContentEncodingArray = ContentEncodingArray;
            ViewBag.boolArray = array;
            ViewBag.buffers = buffer;
            ViewBag.PublishRetryCount = RetryCount;
            ViewBag.PublishRetryMillisecond = RetryMillisecond;
            ViewBag.httpType = httpType;
            ViewBag.IsRetry = IsRetry;
            List<SelectListItem> item = new List<SelectListItem>();
            Dictionary<int, string> ExchangeTypeDic = new Dictionary<int, string>();

            foreach (var str in Enum.GetNames(typeof(ExchangeType)))
            {
                ExchangeType t = (ExchangeType)Enum.Parse(typeof(ExchangeType), str);
                item.Add(new SelectListItem() { Text = str, Value = ((int)t).ToString() });
                if (str == "direct" || str == "headers")
                {
                    if (!ExchangeTypeDic.ContainsKey((int)t))
                    {
                        ExchangeTypeDic.Add((int)t, str);
                    }
                }

            }
            ViewBag.ExchangeTypeItems = item;
            ViewBag.ExchangeTypeDic = ExchangeTypeDic;
            return View(model);
        }

        [HttpPost]
        public JsonResult ValidateApp(string appId)
        {
            MQMainConfiguration model = GetAppConfigurationByAppId(appId);
            if (model != null)
            {
                return Json(new { success = false, message = string.Concat("此应用", appId, "已存在") });
            }
            else
            {
                return Json(new { success = true, message = "" });
            }
        }

        [HttpPost]
        public JsonResult ValidateAppCode(string appId, string code)
        {
            MQMainConfiguration model = GetAppConfigurationByAppId(appId);

            if (model.MessageCfgList != null)
            {
                if (model.MessageCfgList.ToList().Exists(m => m.Code == code.ToLower()))
                {
                    return Json(new { success = false, message = string.Concat("此应用", appId, "已存在此", code) });
                }
                else
                {
                    return Json(new { success = true, message = "" });
                }
            }
            else
            {
                return Json(new { success = true, message = "" });
            }
        }

        [HttpPost]
        public JsonResult SaveApp(string AppId, string host, string port, string vhost, string userName, string password, bool IsUseDefault, string ownerHost)
        {
            if (AppId.ToLower() == "default")
            {
                return Json(new { success = false, msg = "{0}，不能使用deault关键字".Fomart(AppId) });
            }
            ConnectionConfigureation connectionstring = null;
            try
            {

                if (IsUseDefault)
                {
                    connectionstring = GetDefaultConfiguration().ConnCfg;
                }
                else
                {
                    connectionstring = new ConnectionConfigureation()
                    {
                        ConnectionString = GetConnectionString(host, port, vhost, userName, password)
                    };
                }


                MQMainConfiguration main = new MQMainConfiguration()
                {
                    AppId = AppId.ToLower(),
                    Version = 1,
                    OwnerHost = ownerHost == "" ? null : ownerHost,
                    ConnCfg = connectionstring
                };


                CfgRepositoryDeclare.cfgRepo.Add(main, MQConfigurationDB, MQAppCfgTable);

                ApplicationLog.Info(string.Concat("增加应用配置AppId", AppId, "connectionstring:", connectionstring.ConnectionString));

                return Json(new { success = true, msg = "{0}，保存成功".Fomart(AppId) });

            }
            catch (Exception ex)
            {
                ApplicationLog.Error(ex.Message);
                TempData["message"] = "修改失败" + ex.Message;
                return Json(new { success = false, msg = ex.ToString() });
            }

        }

        [RoleAuth]
        [HttpGet]
        public ActionResult AppdomainList()
        {
            List<AppdomainConfiguration> domains = GetAppdomainConfiguration().Where(p => p.DomainName != null).ToList();
            return View(domains);
        }

        public void DeleteDomain(string domainName)
        {
            AppdomainConfiguration domain = CfgRepositoryDeclare.cfgAppdomainRepo.FindOne(MQAppdomainConfigurationSpecifications.MatchOneAppdomain(domainName), MQConfigurationDB, MQAppDomainTable);

            // appDomianConfigurationRepository.Remove(domain, MQConfigurationDB, MQAppDomainTable);
        }

        public ActionResult EditDomain(string domainName)
        {
            AppdomainConfiguration domain = CfgRepositoryDeclare.cfgAppdomainRepo.FindOne(MQAppdomainConfigurationSpecifications.MatchOneAppdomain(domainName), MQConfigurationDB, MQAppDomainTable);

            if (domain.Items == null)
            {
                List<DomainItem> items = new List<DomainItem>(){
                 new DomainItem()
                 {
                  AppId = domain.AppId,
                   Code = domain.Code
                 }
                };
                domain.Items = items;
            }

            List<SelectListItem> listItems = new List<SelectListItem>();
            foreach (var _item in Enum.GetNames(typeof(DomainAction)))
            {
                DomainAction _domian = (DomainAction)Enum.Parse(typeof(DomainAction), _item);
                listItems.Add(new SelectListItem() { Text = _item, Value = ((int)_domian).ToString() });
            }
            ViewBag.DomainActionItems = listItems;
            ViewBag.DomainConnectionPool = DomainConnectionPool;
            return View(domain);
        }

        [HttpPost]
        public ActionResult EditSaveDomainCfg(AppdomainConfiguration domain, List<DomainItem> items)
        {
            try
            {
                AppdomainConfiguration model = CfgRepositoryDeclare.cfgAppdomainRepo.FindOne(MQAppdomainConfigurationSpecifications.MatchOneAppdomain(domain.DomainName), MQConfigurationDB, MQAppDomainTable);
                model.Status = domain.Status;
                model.Host = domain.Host;
                model.Items = items;
                model.Version = model.Version + 1;
                CfgRepositoryDeclare.cfgAppdomainRepo.Save(model, MQConfigurationDB, MQAppDomainTable);
                ApplicationLog.Info(string.Concat("修改domain,AppId:", model.AppId, "Code：", model.Code));
                return this.JavaScriptResultResponse("busalert('修改成功');");
            }
            catch (Exception ex)
            {
                ApplicationLog.Error(ex.Message);
                return this.JavaScriptResultResponse("busalert('修改成功 " + ex.Message + "');");
            }
            //return RedirectToAction("AppdomainList");
            //return View("EditDomain");
        }

        [HttpPost]
        public ActionResult SaveNewDomainAppCfg(string appId, string owerHost, List<string> checkboxList)
        {
            var appDomainModel = CfgRepositoryDeclare.cfgAppdomainRepo.FindOne(MQAppdomainConfigurationSpecifications.MatchOneAppdomain(string.Concat("ad_", appId))
                , MQConfigurationDB, MQAppDomainTable);

            List<DomainItem> itemsList = new List<DomainItem>();

            if (appDomainModel != null)
            {
                if (appDomainModel.Items != null)
                {
                    itemsList = appDomainModel.Items.ToList();
                }
            }
            string codes = string.Empty;
            // var domainCfg = appDomianConfigurationRepository.FindOne(MQAppdomainConfigurationSpecifications.MatchOneAppdomain(string.Concat("ad_", appId)), MQConfigurationDB, MQAppDomainTable);
            try
            {
                //  
                foreach (var v in checkboxList)
                {
                    if (v.Contains("true"))
                    {
                        if (itemsList.Exists(i => i.Code == v.Split(':')[0]))
                        {
                            TempData["message"] = string.Concat("已经存在此Domain 已经存在此业务:", v.Split(':')[0]);

                            return RedirectToAction("Index", "Home");
                        }
                        itemsList.Add(new DomainItem()
                        {
                            _Status = DomainAction.Normal,
                            AppId = appId,
                            Code = v.Split(':')[0],
                            ConnectionPoolSize = 1
                        });

                        codes += v.Split(':')[0] + ",";
                    }
                }
                if (appDomainModel == null)
                {
                    appDomainModel = new AppdomainConfiguration()
                    {
                        AppId = string.Concat("ad_", appId),
                        Host = owerHost == "" ? null : owerHost,
                        Status = DomainAction.Normal,
                        Version = 1,
                        DomainName = string.Concat("ad_", appId),
                        Items = itemsList
                    };
                }
                else
                {
                    owerHost.NullObjectReplace(v => appDomainModel.Host = v);
                    appDomainModel.Version += 1;
                    appDomainModel.Items = itemsList;
                }

                CfgRepositoryDeclare.cfgAppdomainRepo.Save(appDomainModel, MQConfigurationDB, MQAppDomainTable);


                TempData["message"] = "修改成功";
            }
            catch (Exception ex)
            {
                ApplicationLog.Error(ex.Message);
                TempData["message"] = string.Concat("修改失败,原因:", ex.Message);
            }

            ApplicationLog.Info(string.Concat("关联Domain,Appid:", appId, codes));
            return RedirectToAction("AppdomainList");
        }


        //增加Code（队列） liguo 2015.12.11
        [HttpPost]
        public ActionResult SaveNewCode(string appId, string code, string UrlList, bool isDefaultExchange, string exchangeName, string exchangeType)
        {
            var _code = code.ToLower();
            var cfg = GetAppConfigurationByAppId(appId);
            if (cfg == null)
                return Json(new { success = false, error = "{0}不存在，无法创建队列".Fomart(appId) });
            cfg.Version += 1;
            var exists_code = cfg.MessageCfgList.VerifyIsEmptyOrNullEnumerable(Enumerable.Empty<MessageConfiguration>()).FirstOrDefault(c => c.Code == _code);
            if (exists_code != null)
            {
                return Json(new { suucess = false, msg = "Code {0}，已存在".Fomart(code) });
            }
            var _exchange = exchangeType != "0" ? (ExchangeType)Enum.Parse(typeof(ExchangeType), exchangeType) : (ExchangeType?)null;
            var _exchangName = isDefaultExchange ? "ex_{0}_{1}".Fomart(appId, code) : exchangeName;
            var msg_cfg = new MessageConfiguration();
            msg_cfg.Enable = true;
            msg_cfg.Code = _code;
            msg_cfg.QueueCfg = new QueueConfiguration { QueueName = "{0}_{1}".Fomart(appId, code) };
            msg_cfg.ConsumeCfg = new ConsumeConfiguration { RoutingKey = code };
            msg_cfg.PublishCfg = new PublishConfiguration { RouteKey = code };
            msg_cfg.ExchangeCfg = new ExchangeConfiguration { ExchangeName = _exchangName, _ExchangeType = _exchange };
            var _callback_list = new List<CallbackConfiguration>();
            var _callback_index = 0;
            UrlList.Split(',').EachAction(url =>
            {
                var c_info = new CallbackConfiguration();
                c_info.CallbackKey = "{0}_{1}_c{2}".Fomart(appId, code, _callback_index);
                c_info.Url = url;
                _callback_list.Add(c_info);
                _callback_index++;
            });
            msg_cfg.CallbackCfgList = _callback_list;
            cfg.MessageCfgList = cfg.MessageCfgList.AddItem(msg_cfg);
            CfgRepositoryDeclare.cfgRepo.Save(cfg, MQConfigurationDB, MQAppCfgTable);
            return Json(new { suucess = true, msg = "Code {0}，保存成功".Fomart(code) });
        }

        [HttpGet]
        [RoleAuth]
        public ActionResult CreateCode()
        {
            //MQMainConfiguration main = GetAppConfigurationByAppId(appId);
            List<MQMainConfiguration> main = GetAppConfiguration();

            List<string> appList = main.Select(s => s.AppId).ToList();
            TempData["AppList"] = appList;
            List<SelectListItem> item = new List<SelectListItem>();



            foreach (var str in Enum.GetNames(typeof(ExchangeType)))
            {
                ExchangeType t = (ExchangeType)Enum.Parse(typeof(ExchangeType), str);
                if (str == "direct" || str == "topic")
                {
                    item.Add(new SelectListItem() { Text = str, Value = ((int)t).ToString() });
                }

            }
            ViewBag.ExchangeTypeItems = item;
            return View();
        }

        public void Save(MQMainConfiguration configuration, string dbName, string tableName)
        {

        }
        /// <summary>
        /// 增加业务端回调
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [RoleAuth]
        public ActionResult AddCallbackUrl()
        {
            TempData["AppList"] = GetAppConfiguration().Select(e => e.AppId);
            return View();
        }
        [HttpPost]
        public ActionResult AddCallbackUrl(string appId, string code, string UrlList)
        {
            if (appId.IsEmpty() || appId == "default") return Json(new { success = false, msg = "请选择AppID" });
            if (code.IsEmpty() || code == "default") return Json(new { success = false, msg = "请选择Code" });
            if (UrlList.IsEmpty()) return Json(new { success = false, msg = "请选择Code" });
            var _code = code.ToLower();
            var cfg = GetAppConfigurationByAppId(appId);
            if (cfg == null)
                return Json(new { success = false, error = "{0}不存在，无法增加回调".Fomart(appId) });
            cfg.Version += 1;
            var exists_code = cfg.MessageCfgList.VerifyIsEmptyOrNullEnumerable(Enumerable.Empty<MessageConfiguration>()).FirstOrDefault(c => c.Code == _code);
            if (exists_code == null)
            {
                return Json(new { suucess = false, msg = "Code {0}，已不存在，先创建Code再添加回调URL".Fomart(code) });
            }
            var _callback_index = exists_code.CallbackCfgList.Count();
            UrlList.Split(',').EachAction(url =>
            {
                _callback_index++;
                var c_info = new CallbackConfiguration();
                c_info.CallbackKey = "{0}_{1}_c{2}".Fomart(appId, code, _callback_index);
                c_info.Url = url;
                exists_code.CallbackCfgList.AddItem(c_info);
            });
            CfgRepositoryDeclare.cfgRepo.Save(cfg, MQConfigurationDB, MQAppCfgTable);
            return Json(new { suucess = true, msg = "Code {0}，保存成功".Fomart(code) });
        }
        //liguo 2015.11.10
        [HttpGet]
        public ActionResult TestMessage()
        {
            var applist = GetAppConfiguration().Select(e => e.AppId);
            TempData["AppList"] = applist;
            return View();
        }
        //liguo 2015.12.10
        [HttpGet]
        public ActionResult GetCallbackUrlList(string appId, string code)
        {
            var cfg = GetAppConfigurationByAppId(appId);
            if (cfg == null)
                return Json(new { success = false, msg = "{0} not find.".Fomart(appId) }, JsonRequestBehavior.AllowGet);
            var code_cfg = cfg.MessageCfgList.VerifyIsEmptyOrNullEnumerable<MessageConfiguration>(Enumerable.Empty<MessageConfiguration>()).SingleOrDefault(c => c.Code == code);
            if (code_cfg == null)
                return Json(new { success = false, msg = "{0} not find.".Fomart(code) }, JsonRequestBehavior.AllowGet);
            var urlList = code_cfg.CallbackCfgList.Select(c => new { c.Url, c.CallbackKey });
            return Json(new { success = true, msg = "ok", urls = urlList }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetCallbackAlarmCfgInfo(string callbackId)
        {
            var alarmCfg = CfgRepositoryDeclare.AlarmRepoInstance.FindOne(AlarmSpecifications.MatchCallbackId(callbackId), false);
            if (alarmCfg == null) return Json(new { success = false, msg = "{0} 不存在".Fomart(callbackId) }, JsonRequestBehavior.AllowGet);
            return Json(new { success = true, _data = new { alarmId = alarmCfg.AlarmAppId, desc = alarmCfg.Description } }
                , JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult CallbackAlarmCfgSave(string alarmAppid, string alarmDesc, string callbackId, string callbackUrl)
        {
            var alarm = new YmatouMQMessageMongodb.Domain.Module.Alarm(callbackId, callbackUrl, alarmAppid, alarmDesc.TrySubString(200));
            CfgRepositoryDeclare.AlarmRepoInstance.Save(alarm);
            return Json(new { success = true, msg = "预警设置成功" });
        }
        //liguo 2015.12.10
        [HttpGet]
        public ActionResult AlarmManager()
        {
            TempData["AppList"] = GetAppConfiguration().Select(e => e.AppId);
            return View();
        }
        [HttpPost]
        public string PublishMessageTest(string AppId, string Code, string Msgcontent, int Repeatcount)
        {
            var watch = Stopwatch.StartNew();
            ApplicationLog.Debug("发送消息  个" + Repeatcount);
            for (var i = 0; i < Repeatcount; i++)
            {
                MessageBusAgent.Publish(new PulbishMessageDto
                {
                    appid = AppId,//必填
                    code = Code, //必填
                    messageid = Guid.NewGuid().ToString("N"), //消息Id建议填写
                    body = Newtonsoft.Json.JsonConvert.DeserializeObject(Msgcontent.Replace("\n", "")),//消息正文
                    requestpath = System.Configuration.ConfigurationManager.AppSettings["path"] ?? "bus/Message/publish/"//web api //请求路径（可选）
                }, errorHandle: err =>
                {
                    err.ToString();
                });
            }
            var total = watch.ElapsedMilliseconds;
            watch.Stop();
            var ops = Repeatcount * 1000 / (total > 0 ? total : 1);
            return string.Format("发送完成，耗时 {0} 毫秒,每秒 发送 {1}个消息", total, ops);
            //ViewBag.PubResultMessage = string.Format("发送完成，耗时 {0} 毫秒,每秒 发送 {1}个消息", total, ops);
            //return View("TestMessage");
        }
        public string GetConnectionString(string host, string port, string vhost, string userName, string password)
        {
            return string.Format("host={0};port={1};vHost={2};uNmae={3};pas={4};heartbeat=5000;recoveryInterval=5;channelMax=100;useBackgroundThreads=true;pooMinSize=3;pooMaxSize=10", host, port, vhost, userName, password);
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
    }
}
