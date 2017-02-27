//using System;
//using System.Web.Http;
//using System.Web.Http.SelfHost;

//namespace YmatouMQServerWinService
//{
//    public class MQServerBootStart
//    {
//        private readonly  HttpSelfHostServer _server;
//        private readonly HttpSelfHostConfiguration _config;
//        public const string ServiceAddress = "http://localhost:2345";

//        public MQServerBootStart() 
//        {
//            _config = new HttpSelfHostConfiguration(ServiceAddress);
//            _config.Routes.MapHttpRoute("DefaultApi",
//                "api/{controller}/{id}",
//                new { id = RouteParameter.Optional });

//            _server = new HttpSelfHostServer(_config);
//        }

//        public  void Start()
//        {         
//            _server.OpenAsync().Wait();
//        }

//        public  void Stop()
//        {
//            _server.CloseAsync().Wait();
//        }
//    }
//}
