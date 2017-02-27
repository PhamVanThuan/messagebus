using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Config;
using YmatouMQ.Common;
using YmatouMQ.Common.Extensions;
using YmatouMQ.ConfigurationSync;
using YmatouMQ.Log;
using YmatouMQMessageMongodb.AppService;
using YmatouMQMessageMongodb.AppService.Configuration;
using YmatouMQMessageMongodb.Domain.IRepository;
using YmatouMQMessageMongodb.Domain.Module;
using YmatouMQMessageMongodb.Domain.Specifications;
using YmatouMQMessageMongodb.Repository;

namespace YmatouMQ.CompensateMessageLog
{
    public class CompensateMessageTask
    {
        private static IMessageRepository mesageRepository = new MQMessageRepository();
        private static readonly MQAppConfigurationAppService cfgAppService = new MQAppConfigurationAppService();

        private static readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile,
            "YmatouMQ.CompensateMessageLog.MessageLog");

        private static readonly IMessageStatusRepository statusRepository = new MessageStatusRepository();

        private static readonly RetryMessageCompensateAppService retryMessageAppService =
            new RetryMessageCompensateAppService();

        public static void Start()
        {
            XmlConfigurator.Configure(
                new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "log4net.config")));
            var watch = Stopwatch.StartNew();
            log.Debug("开始执行补单");
            ExecuteCompensateMessage();
            watch.Stop();
            log.Debug("补单完成，耗时 {0:N0} ms", watch.ElapsedMilliseconds);
        }

        private static void ExecuteCompensateMessage()
        {
            //获取所有库名
            var dbNames = GetMessageLogDbNames();
            if (dbNames.IsEmptyEnumerable()) return;
            //获取需要补单的库（配置）           
            var includeDb = ConfigurationManager.AppSettings["Include_db"];
            if (!includeDb.IsEmpty() && includeDb != "all")
            {
                log.Debug("include db->{0}", includeDb);
                var includeDbArray = includeDb.Split(new char[] {','});
                //过滤出需要补单的库
                dbNames = dbNames.Where(d => includeDbArray.Contains(d));
            }
            if (dbNames.IsEmptyEnumerable())
            {
                log.Debug("dbNames is null.");
                return;
            }
            //遍历所有需要补单的库
            dbNames.EachAction(db =>
            {
                //查找当前库下的所有表
                var allCollections = mesageRepository.FindAllCollections(db)
                    .Where(cName => cName.StartsWith("Message_"));
                log.Debug("db {0} collections->{1}", db, allCollections.Count());
                //filter tables
                var excludeTables = ConfigurationManager.AppSettings["exclude_table"];
                if (!string.IsNullOrEmpty(excludeTables))
                {
                    log.Debug("exclude tables->{0}", excludeTables);
                    var includeTbArray = excludeTables.Split(new char[] {','});
                    allCollections = allCollections.Where(_c => !includeTbArray.Contains(_c));
                }
                //遍历所有需要补单的表
                allCollections.Where(cName => cName.StartsWith("Message_")).EachAction(c =>
                {
                    var time = ConfigurationManager.AppSettings["scan_time"];
                    var startTime = DateTime.Now.AddHours(-1);
                    var endTime = DateTime.Now;
                    if (!string.IsNullOrEmpty(time))
                    {
                        startTime = Convert.ToDateTime(time.Split(new char[] {','})[0]);
                        endTime = Convert.ToDateTime(time.Split(new char[] {','})[1]);
                    }
                    //补偿30分钟的数据
                    var totalMinutes = Convert.ToInt32(endTime.Subtract(startTime).TotalMinutes);
                    var pageCount = (totalMinutes/30) + (totalMinutes%30 > 0 ? 1 : 0);
                    for (var h = 0; h <= pageCount; h++)
                    {
                        var etime = startTime.AddMinutes(h*30);
                        var stime = etime.AddMinutes(-30);

                        //获取不需要补单的ID（防止重复）
                        var excludeId = ConfigurationManager.AppSettings["{0}_{1}".Fomart(db, c)];
                        var excludeIds = string.IsNullOrEmpty(excludeId)
                            ? null
                            : new List<string>(excludeId.Split(new char[] {','}));
                        //查找消息日志表
                        var message = mesageRepository.Find(
                            MQMessageSpecifications.ExcludeMessage(stime, etime, excludeIds), db, c).ToList();
                        log.Debug("db->{0},table->{1},message count->{2},scan time->{3},{4}", db, c, message.Count(),
                            stime,
                            etime);
                        if (message.Any())
                        {
                            //查找状态表中存在的消息
                            var _messageStatusIdArray = statusRepository.Find(
                                MQMessageSpecifications.MatchInMessageStatusId(message.Select(q => q.MsgId))
                                , "MQ_Message_Status_{0}".Fomart(DateTime.Now.ToString("yyyyMM")),
                                "mq_subscribe_ok_{0}".Fomart(message.First().AppId)
                                ).SetFields("_mid");
                            log.Debug("message status info: db->{0},tb->{1},appid->{2},code->{3},status count->{4}",
                                "MQ_Message_Status_{0}".Fomart(DateTime.Now.ToString("yyyyMM")),
                                "mq_subscribe_ok_{0}".Fomart(message.First().AppId),
                                message.First().AppId,
                                message.First().Code,
                                _messageStatusIdArray.Count());
                            //如果状态表存在ID则获取差积 ,否则则全部补偿 
                            var messageIdArray = Enumerable.Empty<string>();
                            if (_messageStatusIdArray.Any())
                            {
                                messageIdArray = _messageStatusIdArray.Select(m => m.MessageId);
                                //获取不存在状态表的消息Id
                                var exceptMessageId = message.Select(_m => _m.MsgId).Except(messageIdArray);
                                message = message.Where(_m => exceptMessageId.Contains(_m.MsgId)).ToList();
                            }
                            //获取需要补偿的消息
                            log.Debug(
                                "db->{0},table->{1},needRetryMessageCount->{2},message count->{3},status count->{4}",
                                db,
                                c,
                                message.Count()
                                , message.Count()
                                , messageIdArray.Count());
                            //如果存在需要补偿的消息则写入补单库
                            if (message.Any())
                            {
                                var listMessage = new List<RetryMessage>();
                                message.EachAction(__m =>
                                {
                                    //获取配置
                                    var cfg = MQMainConfigurationManager.Builder.GetConfiguration(__m.AppId, __m.Code);
                                    var callbackList =
                                        cfg.CallbackCfgList.Where(__ => __.Enable != null && __.Enable.Value == true);
                                    //如果存在需要会调的业务端则写入补单库
                                    if (callbackList.Any())
                                    {
                                        var retryMessageInfo = new RetryMessage(__m.AppId, __m.Code, __m.MsgId, __m.Body,
                                            DateTime.Now.AddMinutes(10),
                                            callbackList.Select(_c => _c.CallbackKey).ToList()
                                            , desc: "budan", uuid: __m._id);
                                        listMessage.Add(retryMessageInfo);
                                    }
                                });
                                //如果存在消息则写入
                                if (listMessage.Any())
                                {
                                    //消息写入补单库
                                    retryMessageAppService.BatchAddMessage(listMessage, message.First().AppId,
                                        message.First().Code);
                                    //消息状态更新为已补单状态
                                    MessageAppService.TryUpdateMultipleMessageStatus(
                                        message.Select(__m => __m._id)
                                        , 2000, message.First().AppId, message.First().Code);
                                    log.Debug(
                                        "db->{0},tables->{1},real need retry message->{2},appid->{3},code->{4} db save ok",
                                        db, c,
                                        listMessage.Count, message.First().AppId, message.First().Code);
                                }
                            }

                        }
                    }
                });
            });
        }

        private static IEnumerable<string> GetMessageLogDbNames()
        {
            var allAppIds = cfgAppService.FindAllAppId();
            log.Debug("get all appid count {0}", allAppIds.Count());
            return allAppIds.Select(a => "MQ_Message_{0}_{1}".Fomart(a, DbNamesSuffix()));
        }

        public static string DbNamesSuffix()
        {
            var timeStr = ConfigurationManager.AppSettings["scan_time"];
            if (string.IsNullOrEmpty(timeStr)) return DateTime.Now.ToString("yyyyMM");
            var startTime = Convert.ToDateTime(timeStr.Split(new char[] {','})[0]);
            var dbNames = startTime.ToString("yyyyMM");
            log.Debug("DbNamesSuffix {0}", dbNames);
            return dbNames;
        }
    }
}
