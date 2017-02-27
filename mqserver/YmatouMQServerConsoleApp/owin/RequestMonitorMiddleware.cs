using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace YmatouMQServerConsoleApp.owin
{

    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RequestMonitorMiddleware
    {
        private int _requestsProcessed = 0;
        private readonly AppFunc _next;
        private Timer _timer;
        public RequestMonitorMiddleware(AppFunc next)
        {
            _next = next;
            _timer = new Timer(TimerFired, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0.1));
        }
        public Task Invoke(IDictionary<string, object> environment)
        {
            Interlocked.Increment(ref _requestsProcessed);
            return _next(environment);
        }
        private void TimerFired(object state)
        {
            int requestsProcessed = Interlocked.Exchange(ref _requestsProcessed, 0);

            //int maxAccepts, maxRequests;
            //_server.GetRequestProcessingLimits(out maxAccepts, out maxRequests);
            //Console.WriteLine("Active/MaxAccepts:"
            //    + maxAccepts + "/" + (int)_currentMaxAccepts
            //    + ", Active/MaxRequests:"
            //    + maxRequests + "/" + _currentMaxRequests
            //    + ", Requests/1sec: " + requestsProcessed);

            //_server.SetRequestProcessingLimits((int)(_currentMaxAccepts += 0.1), _currentMaxRequests);
            Console.WriteLine("Requests/1sec: " + requestsProcessed);
        }
    }
}
