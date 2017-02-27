using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YmatouMQAdmin.WebApp2.Models
{
    public class MenuItem
    {
        public string text { get; set; }

        public string controllerName { get; set; }

        public string actionName { get; set; }
    }
}