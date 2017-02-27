using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using YmatouMQMessageMongodb.Domain.IRepository;
using YmatouMQMessageMongodb.Repository;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Utils;

namespace YmatouMQMessageMongodb.AppService
{
    public class AlarmAppService
    {
        private static readonly Lazy<LocalMemoryCache<string, string[]>> cache = new Lazy<LocalMemoryCache<string, string[]>>(() => new LocalMemoryCache<string, string[]>());
        private static readonly Lazy<IAlarmRepository> alarmRepo = new Lazy<IAlarmRepository>(() => new AlarmRepository());
        public static string[] FindAlarmAppId(string callbackId,string url)
        {
            var cacheAlarmAppId = cache.Value.GetCacheItem(callbackId);
            if (!cacheAlarmAppId.IsEmptyEnumerable()) return cacheAlarmAppId;
            var alarm = alarmRepo.Value.FindById(callbackId);
            if (alarm != null)
            {
                var _alarmAppId = new string[] { alarm.AlarmAppId, "AppId".GetAppSettings() };
                cache.Value.AddItem(callbackId, _alarmAppId, TimeSpan.FromMinutes(3));
                return _alarmAppId;
            }
            else
            {

                var defaultAlarmAppId = FindAlarmAppIdByUri(url);
                cache.Value.AddItem(callbackId, defaultAlarmAppId, TimeSpan.FromMinutes(3));
                return defaultAlarmAppId;
            }
        }

        public static string[] FindAlarmAppIdByUri(string url)
        {
            if (url.IsEmpty() || !url.StartsWith("http://")) return new string[] { "AppId".GetAppSettings() };
            var uri=new Uri(url);
            return new string[] { uri.Host, "AppId".GetAppSettings() };
        }
    }
}
