using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using YmatouMQServerConsoleApp.owin;

namespace YmatouMQServerConsoleApp
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    #region
    //using SendFileFunc =
    //   Func<string, // File Name and path
    //       long, // Initial file offset
    //       long?, // Byte count, null for remainder of file
    //       CancellationToken,
    //       Task>; // Complete
    //using WebSocketAccept =
    //    Action<IDictionary<string, object>, // WebSocket Accept parameters
    //        Func<IDictionary<string, object>, // WebSocket environment
    //            Task>>; // Complete
    //using WebSocketReceiveAsync =
    //    Func<ArraySegment<byte> /* data */,
    //        CancellationToken /* cancel */,
    //        Task<Tuple<int /* messageType */,
    //            bool /* endOfMessage */,
    //            int>>>; /* count */
    //using WebSocketSendAsync =
    //    Func<ArraySegment<byte> /* data */,
    //        int /* messageType */,
    //        bool /* endOfMessage */,
    //        CancellationToken /* cancel */,
    //        Task>;
    #endregion
    public class MQOwinApiBase
    {
        private readonly AppFunc _next;
        private readonly Dictionary<string, Tuple<AppFunc, string>> _paths;
        public MQOwinApiBase(AppFunc next)
        {
            _next = next;

            _paths = new Dictionary<string, Tuple<AppFunc, string>>();
            _paths["/"] = new Tuple<AppFunc, string>(Index, null);

            var items = this.GetType().GetMethods()
                .Select(methodInfo => new
                {
                    MethodInfo = methodInfo,
                    Attribute = methodInfo.GetCustomAttributes(true).OfType<CanonicalRequestAttribute>().SingleOrDefault()
                })
                .Where(item => item.Attribute != null)
                .Select(item => new
                {
                    App = (AppFunc)Delegate.CreateDelegate(typeof(AppFunc), this, item.MethodInfo),
                    item.Attribute.Description,
                    item.Attribute.Path,
                });

            foreach (var item in items)
            {
                _paths.Add(item.Path, Tuple.Create(item.App, item.Description));
            }
        }
        public Task Invoke(IDictionary<string, object> env)
        {
            Tuple<AppFunc, string> handler;
            return _paths.TryGetValue(Util.RequestPath(env), out handler)
                ? handler.Item1(env)
                : _next(env);
        }
        public Task Index(IDictionary<string, object> env)
        {
            Util.ResponseHeaders(env)["Content-Type"] = new[] { "text/html" };
            Stream output = Util.ResponseBody(env);
            using (var writer = new StreamWriter(output))
            {
                writer.Write("<ul>");
                foreach (var kv in _paths.Where(item => item.Value.Item2 != null))
                {
                    writer.Write("<li><a href='");
                    writer.Write(kv.Key);
                    writer.Write("'>");
                    writer.Write(kv.Key);
                    writer.Write("</a> ");
                    writer.Write(kv.Value.Item2);
                    writer.Write("</li>");
                }

                writer.Write("<li><a href='/testpage'>/testpage</a> Test Page</li>");
                writer.Write("<li><a href='/Welcome'>/Welcome</a> Welcome Page</li>");

                writer.Write("</ul>");
            }
            return Task.FromResult<object>(null);
        }
    }
}
