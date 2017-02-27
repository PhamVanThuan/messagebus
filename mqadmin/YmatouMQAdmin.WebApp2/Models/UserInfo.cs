using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
namespace YmatouMQAdmin.WebApp2.Models
{
    public class UserInfo
    {
        [Required(ErrorMessage="username can't null")]
        public string username { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "password can't null")]        
        public string password { get; set; }

        public string Roles { get; set; }
    }
}