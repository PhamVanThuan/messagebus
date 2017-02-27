using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YmatouMQ.Common;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Extensions._Task;
using YmatouMQ.Log;
using YmatouMQMessageMongodb.Domain.IRepository;
using YmatouMQMessageMongodb.Domain.Module;
using YmatouMQMessageMongodb.Repository;

namespace YmatouMQMessageMongodb.AppService
{
    public class BusPushHealthAppService
    {
        private static readonly IBusPushHealthRepository HealthRepository = new BusPushHealthRepository();

        private static readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile,
            "YmatouMQMessageMongodb.AppService.BusPushHealthAppService");

        public static async Task UpdateBusPushHealth(string appId, string status)
        {
            if (appId.IsEmpty()) return;
            Action action =
                () => HealthRepository.Save(new BusPushHealth(appId, DateTime.Now, status), "MQ_Alarm", "BusPushHealth");
            await action.ExecuteASynchronously().ConfigureAwait(false);
            log.Info("[UpdateBusPushHealth] done,appid:{0},status:{1}",appId,status);
        }

        public static async Task<bool> CheckBusPushHealthIsTimeOut(string appId, int timeOutSecond = 15)
        {
            if (appId.IsEmpty())
                return false;

            var health =
                await
                    HealthRepository.FindOneAsync(null, "MQ_Alarm", "BusPushHealth", false, 3000).ConfigureAwait(false);
            if (health == null)
            {
                log.Debug("[CheckBusPushHealthIsTimeOut] appId:{0} no enable BusPushHealth", appId);
                return false;
            }
            var isTimeOut = health.CheckHealthIsTimeOut(timeOutSecond);
            if (isTimeOut)
            {
                log.Debug("[CheckBusPushHealthIsTimeOut] appId:{0} consumer Health Is TimeOut!,status:{1}", appId,
                    health.Status);
                return true;
            }
            return false;
        }
    }
}
