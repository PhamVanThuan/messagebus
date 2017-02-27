using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQ.Common;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Log;
using YmatouMQMessageMongodb.AppService;

namespace YmatouMQNet4.Core
{
   public class Health
   {
       private static DateTime lastUpdateHealthTime;
       private static readonly ILog _log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile,
           "YmatouMQ.Core.Publish.Health");      
       
       public static bool CheckHealthIsTimeOut(int timeOutSecond)
       {
           if ( lastUpdateHealthTime == DateTime.MinValue ||
                lastUpdateHealthTime.Subtract(DateTime.Now).TotalSeconds >=
                timeOutSecond)
           {             
               lastUpdateHealthTime = DateTime.Now.AddSeconds(timeOutSecond);
               try
               {                   
                   var result= BusPushHealthAppService.CheckBusPushHealthIsTimeOut("HealthAppId".GetAppSettings(),
                       timeOutSecond).Result;
                   _log.Info("[CheckHealthIsTimeOut] is timeOut:{0},next check:{1}", result,
                       lastUpdateHealthTime);
                   return false;
               }
               catch (OperationCanceledException ex)
               {
                   if (ex.InnerException != null)
                       _log.Error("[CheckHealthIsTimeOut] OperationCanceledException {0},{1}", "HealthAppId".GetAppSettings(),
                           ex.InnerException.ToString());
                   return false;
               }
               catch (Exception ex)
               {
                   _log.Error("[CheckHealthIsTimeOut] Exception {0},{1}", "HealthAppId".GetAppSettings(),
                          ex.ToString());
                   return false;
               }

           }
           return false;
       }
    }
}
