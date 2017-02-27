using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using YmatouMQAdmin.Domain.IRepository;
using YmatouMQAdmin.Repository;
using YmatouMQNet4.Configuration;
using MongoDB.Driver.Builders;
using YmatouMQ.Common.Dto;
using YmatouMQ.Common.Extensions.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using YmatouMQ.Common.MessageHandleContract;
using YmatouMQ.ConfigurationSync;
using System.Text.RegularExpressions;

//查看消息
//http://192.168.1.247:999/mq/admin/findmessage/?AppId=test2&Code=siyou_test&message=siyou_test&Status=true
namespace YmatouMQAdminTest
{
    [TestClass]
    public class PagerInfoExTest
    {
        [TestMethod]
        public void Find_Default_Cfg()
        {
            //string rawUrl = "/Default/MessageStatusSearch?status=all";
            //string rawUrl = "/Default/MessageStatusSearch";
            //string rawUrl = "/Default/MessageStatusSearch?status=all&p=&code=ordership";
            string rawUrl = "/Default/MessageStatusSearch?p=&code=ordership";
            //string rawUrl = "/Default/MessageStatusSearch?p=22&status=all";
            //string rawUrl = "/Default/MessageStatusSearch?appId=xlobo&code=ordership&mid=&status=all&startDate=2016-06-01%2000:00:00&endDate=2016-06-30%2017:41:35&clentip=&ps=5&p=22";
            string pageUrl = GetPageUrl(rawUrl, 3);
        }

        public string GetPageUrl(string CurrentUrl, int page)
        {
            string PagePrefix = "p";

            var requestUrl = CurrentUrl;
            string pageLinkText = "";

            var pattern = string.Format(@"[&?]{0}=(\d+)|[&?]{0}=", PagePrefix.ToLower());
            Regex regexPagePattern = new Regex(pattern, RegexOptions.IgnoreCase);

            MatchCollection matchResults = regexPagePattern.Matches(requestUrl);

            if (matchResults.Count > 0)
            {
                pageLinkText = requestUrl.Replace(matchResults[0].ToString().Substring(1), PagePrefix.ToLower() + "=[$page$]");
            }
            else if (requestUrl.IndexOf("?") < 0)
            {
                pageLinkText = requestUrl + "?" + PagePrefix.ToLower() + "=[$page$]";
            }
            else
            {
                pageLinkText = requestUrl + "&" + PagePrefix.ToLower() + "=[$page$]";
            }
            return pageLinkText.Replace("[$page$]", page.ToString());
        }
    }
}
