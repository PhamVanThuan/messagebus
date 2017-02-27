using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Ymatou.CommonService;
using System.Threading;
using System.Net;
using YmatouMQ.Common.Extensions.Serialization;
using _MethodMonitor = Ymatou.PerfMonitorClient.MethodMonitor;
using System.Web.Http;

namespace YmatouMQServerConsoleApp
{
    public class ExceptionHandlerAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            using (_MethodMonitor.New("PubTotalExp"))
            {
                ApplicationLog.Error(string.Format("mq server Exception,current request {0},{1}", actionExecutedContext.ActionContext.ActionArguments.JSONSerializationToString(), actionExecutedContext.Exception.ToString()));

                var code = HttpStatusCode.InternalServerError;
                if (actionExecutedContext.Exception is InvalidOperationException)
                    code = HttpStatusCode.BadRequest;

                //var response = actionExecutedContext.Request.CreateResponse(code, "请求错误");
                //actionExecutedContext.Response = response;

                //base.OnException(actionExecutedContext);
                throw new HttpResponseException(new HttpResponseMessage(code)
                {
                    Content = new StringContent(string.Format("sererver error {0}", actionExecutedContext.Exception.ToString())),
                    ReasonPhrase = "request error..."
                });
            }
        }
    }
}
