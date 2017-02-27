using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Xml.Linq;
using YmatouMQAdmin.WebApp2.Models;
using System.Configuration;
using Ymatou.CommonService;
using YmatouMQAdmin.WebApp2.Authorize;
using YmatouMQAdmin.WebApp2.App_Start;
using YmatouMQ.Common.Extensions;

namespace YmatouMQAdmin.WebApp2.Controllers
{
    public class LogOnController : Controller
    {
        private static Dictionary<string, string> roleMapDic = new Dictionary<string, string>();
        private static string filePath = AppDomain.CurrentDomain.BaseDirectory + "Config\\Logging.xml";
        private static Dictionary<string, int> sort_menu = new Dictionary<string, int> 
        {                      
            {"CreateApp",0},
            {"CreateCode",1},
            {"CallBackIndex",2},
            {"AddCallbackUrl",3},
            {"MessageStatusSearch",4},
            {"RetryMessage",5},
            {"AppCodeList",6},                      
            {"AppdomainList",7},          
            {"AppDefault",8},
            {"Index",9},
            {"TestMessage",10},
            {"AppGlobal",11},
            {"EditAppQueueStatusJson",12},
            {"UserList",13},
            {"AlarmManager",14}
        };
        public void InitRoles(bool clear = true)
        {
            roleMapDic.Clear();
            foreach (var v in MQAdminUserManager.RoleMap.Split('|'))
            {
                if (!roleMapDic.ContainsKey(v.Split(':')[0]))
                {
                    roleMapDic.Add(v.Split(':')[0], v.Split(':')[1]);

                }
            }
        }

        public ActionResult logOut()
        {
            Session.Clear();
            Session.Abandon();
            return View("Log");
        }

        public ActionResult Log()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Log(UserInfo user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }
            string _password = Base64Encrypt(user.password, Encoding.UTF8);
            var _user = MQAdminUserManager.userInfoList.Where(u => u.username == user.username && u.password == _password).FirstOrDefault();
            string _role = string.Empty;
            if (_user != null)
            {
                _role = _user.Roles;
            }
            string action = string.Empty;
            string controllerName = string.Empty;
            if (MQAdminUserManager.userInfoList.Exists(u => u.username == user.username && u.password == _password))
            {

                // HttpContext.Request.Cookies.Clear();
                Session["username"] = user.username;
                Session["password"] = _password;
                Session["role"] = _role;

                Dictionary<string, string> controllerAction = GetControllerAction();
                ApplicationLog.Debug("GetControllerAction count " + controllerAction.Count);
                foreach (var kv in controllerAction)
                {
                    if (_role == kv.Key)
                    {
                        action = kv.Key;
                        controllerName = kv.Value;
                        break;
                    }
                    if (_role.Contains("|"))
                    {
                        if (_role.Split('|')[0] == kv.Key)
                        {
                            action = kv.Key;
                            controllerName = kv.Value;
                            break;
                        }
                    }
                }
                ApplicationLog.Debug(string.Concat("Controller:", controllerName, ",ActionName:", action));
                if (!string.IsNullOrEmpty(controllerName) && !string.IsNullOrEmpty(action))
                {
                    return RedirectToAction(action, controllerName);
                }
                else
                {
                    TempData["msg"] = string.Format("用户 {0} 无权限", user.username);
                    return RedirectToAction("Log", "LogOn");
                }
            }
            else
            {
                TempData["msg"] = "用户名或密码错误";
                ApplicationLog.Debug(string.Concat("Controller:", controllerName, ",ActionName:", action));
                return RedirectToAction("Log", "LogOn");
            }
        }
        //[OutputCache(Duration = 60)]
        public ActionResult LeftMenu()
        {
            if (!Session.CheckSessionExists("role") || string.IsNullOrEmpty(Session.To<string>("role")))
                return RedirectToAction("Log", "LogOn");
            InitRoles();
            var list = new List<MenuNode>();
            var _sortMenu = new SortedList<int, string>();
            Session.To<string>("role").Split('|').EachAction(c => _sortMenu[sort_menu.TryGetVal(c, 0)] = c);
            _sortMenu.EachAction(c =>
            {
                var controllerName = c.Value == "UserList" ? "LogOn" : (c.Value == "AppCodeList" || c.Value == "MessageStatusSearch" || c.Value == "RetryMessage" ? "Default" : "AdminMQ");
                var node_i = new MenuNode();
                node_i.Text = roleMapDic.ContainsKey(c.Value) ? roleMapDic[c.Value] : c.Value;
                node_i.NavigateUrl = "/" + controllerName + "/" + c.Value;
                list.Add(node_i);
            });
            TempData["username"] = Session.To<string>("username");
            ViewBag.Menu = new HtmlString(MenuNode.ShowMenu(list, 6, "nav navbar-nav"));
            return PartialView("LeftMenu", null);
        }

        public static string Base64Encrypt(string input, Encoding encode)
        {
            return Convert.ToBase64String(encode.GetBytes(input));
        }


        public static string Base64Decrypt(string input, Encoding encode)
        {
            return encode.GetString(Convert.FromBase64String(input));
        }

        [CustomerAuth]
        public ActionResult CreateRole()
        {
            // InitRoles();
            //List<UserInfo> userInfoList = WebApiApplication.userRole;
            ViewBag.roleString = MergeRoleMap();
            return View();
        }

        public static Dictionary<string, string> MergeRoleMap()
        {
            Dictionary<string, string> commonRolemap = new Dictionary<string, string>();
            List<string> menuName = new List<string>();
            foreach (var action in GetActionNames())
            {
                foreach (KeyValuePair<string, string> kv in roleMapDic)
                {
                    if (kv.Key == action)
                    {
                        commonRolemap.Add(kv.Key, kv.Value);
                        break;
                    }
                }
                if (!commonRolemap.ContainsKey(action))
                {
                    commonRolemap.Add(action, action);
                }
            }
            return commonRolemap;
        }

        [HttpPost]
        public JsonResult Validate(string username)
        {

            // InitUserInfo();
            //List<UserInfo> userInfoList = WebApiApplication.userRole;
            if (MQAdminUserManager.userInfoList.Find(u => u.username == username.ToLower()) == null)
            {
                return Json(new { success = true, msg = "" });
            }
            else
            {
                return Json(new { success = false, msg = "已经存在此用户，请重新填写!" });
            }
        }

        [HttpPost]
        public JsonResult EditRole(UserInfo user, string roleList)
        {
            try
            {
                if (roleList == "")
                {
                    return Json(new { success = false, msg = "请为此用户至少分配一项权限" });
                }
                if (user.username == "Admin" && !roleList.Contains("UserList"))
                {
                    return Json(new { success = false, msg = "不能去掉系统管理员的权限列表功能" });
                }
                string role = string.Empty;
                string pwd = string.Empty;

                XElement root = XElement.Load(filePath);

                pwd = Base64Encrypt(user.password, Encoding.UTF8);

                roleList.Split(',').ToList().ForEach(r => role += r.ToString() + "|");

                role = role.Substring(0, role.Length - 1);

                XElement node = (from element in root.Descendants("user")
                                 where element.Element("username").Value == user.username
                                 select element).FirstOrDefault();

                if (node != null)
                {
                    node.Element("password").SetValue(pwd);
                    node.Element("role").SetValue(role);
                    root.Save(filePath);
                    //WebApiApplication.userRole = InitUserInfo();
                    MQAdminUserManager.ReInit();
                    return Json(new { success = true, msg = "" });
                }
                else
                {
                    return Json(new { success = false, msg = "修改失败" });
                }



            }
            catch (Exception ex)
            {
                ApplicationLog.Error(ex.Message);
                return Json(new { success = false, msg = "修改失败 " + ex.Message });
            }
        }

        public ActionResult DeleteRole(string username)
        {
            if (username == "Admin")
            {
                TempData["msg"] = "Admin不允许删除";
                return RedirectToAction("UserList");
            }

            try
            {
                XElement root = XElement.Load(filePath);

                XElement node = (from element in root.Descendants("user")
                                 where element.Element("username").Value == username
                                 select element).FirstOrDefault();

                if (node != null)
                {
                    node.Remove();
                    root.Save(filePath);
                    //WebApiApplication.userRole = InitUserInfo();
                    MQAdminUserManager.ReInit();
                    TempData["msg"] = "操作成功";
                }
            }
            catch (Exception ex)
            {
                ApplicationLog.Error(ex.Message);
                TempData["msg"] = "删除失败";
            }


            return RedirectToAction("UserList");
        }

        [HttpPost]
        public JsonResult SaveRole(UserInfo user, string roleList)
        {
            try
            {
                if (roleList == "")
                {
                    return Json(new { success = false, msg = "请为此用户至少分配一项权限" });
                }
                if (user.username == null)
                {
                    return Json(new { success = false, msg = "用户名不能为空" });
                }
                if (user.password == null)
                {
                    return Json(new { success = false, msg = "密码不能为空" });
                }
                XElement root = XElement.Load(filePath);
                string password = Base64Encrypt(user.password, Encoding.UTF8);
                string role = string.Empty;
                roleList.Split(',').ToList().ForEach(r => role += r.ToString() + "|");

                role = role.Substring(0, role.Length - 1);

                XElement elementNode = new XElement("user",
                                                    new XElement("username", user.username),
                                                    new XElement("password", password),
                                                    new XElement("role", role));

                root.Add(elementNode);
                root.Save(filePath);
                //WebApiApplication.userRole = InitUserInfo();
                MQAdminUserManager.ReInit();
                return Json(new { success = true, msg = "" });
            }
            catch (Exception ex)
            {
                ApplicationLog.Error(ex.Message);

                return Json(new { success = false, msg = "保存失败" });
            }
        }
        [CustomerAuth]
        public ActionResult EditRole(string username)
        {
            //  InitUserInfo();
            //List<UserInfo> userInfoList = InitUserInfo();

            var _user = MQAdminUserManager.userInfoList.Where(u => u.username == username).FirstOrDefault();

            UserInfo user = new UserInfo()
            {
                password = _user.password,
                Roles = _user.Roles,
                username = _user.username
            };


            user.password = Base64Decrypt(user.password, Encoding.UTF8);

            List<string> roles = MQAdminUserManager.userInfoList.Where(u => u.username == username).FirstOrDefault().Roles.Split('|').ToList();
            // List<string> actions = GetActionNames();
            List<SelectListItem> items = new List<SelectListItem>();

            GetActionNames().ForEach(a => items.Add(new SelectListItem() { Text = a.ToString(), Value = a.ToString(), Selected = roles.Contains(a) }));

            items.ForEach(i =>
                {
                    if (roleMapDic.ContainsKey(i.Text))
                    {
                        i.Text = roleMapDic[i.Text].ToString();
                    }
                });
            ViewBag.roleString = items;
            return View(user);
        }


        [CustomerAuth]
        [RoleAuth]
        [HttpGet]
        public ActionResult UserList()
        {
            roleMapDic.Clear();

            List<UserInfo> _userinfo = new List<UserInfo>();
            MQAdminUserManager.userInfoList.ForEach(e => _userinfo.Add(new UserInfo
            {
                Roles = e.Roles,
                password = e.password,
                username = e.username
            }));
            InitRoles();
            _userinfo.ForEach(u =>
            {
                List<string> roles = u.Roles.Split('|').ToList();
                for (int i = 0; i < roles.Count; i++)
                {
                    if (roleMapDic.ContainsKey(roles[i]))
                    {
                        roles[i] = roleMapDic[roles[i]].ToString();
                    }
                }
                u.Roles = "";
                roles.ForEach(r =>
                {
                    u.Roles += r + " | ";
                });
                u.Roles = u.Roles.Substring(0, u.Roles.Length - 2);
            });

            return View(_userinfo);
        }

        private static List<Type> GetSubClasses<T>()
        {
            return Assembly.GetCallingAssembly().GetTypes().Where(
                type => type.IsSubclassOf(typeof(T))).ToList();
        }

        private static List<MethodInfo> GetSubMethods(Type t)
        {
            return t.GetMethods().Where(m => m.ReturnType == typeof(ActionResult) && m.IsPublic == true
                && m.CustomAttributes.ToList().Exists(a => a.AttributeType == typeof(HttpGetAttribute))).ToList();
        }

        public static List<string> GetActionNames()
        {

            List<string> Actions = new List<string>();
            foreach (var t in GetSubClasses<Controller>())
            {
                // controllerNames.Add(t.Name);
                List<MethodInfo> mfCollection = GetSubMethods(t);
                mfCollection.ForEach(method => Actions.Add(method.Name));

            }
            return Actions;
        }

        public static Dictionary<string, string> GetControllerAction()
        {
            Dictionary<string, string> controlerActionDic = new Dictionary<string, string>();
            foreach (var t in GetSubClasses<Controller>())
            {
                List<MethodInfo> mfCollection = GetSubMethods(t);
                mfCollection.ForEach(method =>
                {
                    if (!controlerActionDic.ContainsKey(method.Name))
                    {
                        controlerActionDic.Add(method.Name, t.Name.Replace("Controller", ""));
                    }
                });

            }
            return controlerActionDic;
        }
    }
}