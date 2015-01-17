using System;
using System.Collections.Generic;
using System.Data;
using Nature.DebugWatch;
using Nature.SsoConfig;

namespace Nature.Service.SSOAuth
{
    /// <summary>
    /// sso端的各种认证
    /// </summary>
    public class SsoAuth : BaseAshxCrud
    {

        public override void Process()
        {
            base.Process();

            var action = Request["action"]; //标识

            switch (action)
            {
                case "Login":   //用户登录
                    BaseDebug.Title = "sso端用户登录，验证用户名、密码";
                    Login();
                    break;
                case "Logout":   // sso端退出登录
                    BaseDebug.Title = "sso端退出登录";
                    Logout();
                    break;
                case "WhoAmI":   //询问服务器，当前访问人是谁
                    BaseDebug.Title = "sso端当前访问人是谁";
                    WhoAmI( );
                    break;
                case "CanContinueAccess":      //查看登录人的状态，是否可以继续访问
                    BaseDebug.Title = "sso端查看登录人的状态，是否可以继续访问";
                    CanContinueAccess( );
                    break;
                case "IsOnline":   //网站应用访问，查看用户是否已经登录sso，并且生成sso和app的确认标识
                    BaseDebug.Title = "sso端查看用户是否已经登录sso，并且生成sso和app的确认标识";
                    IsOnline( );
                    break;
                case "IsOnlineCallbakUrl":   //网站应用访问，查看用户是否已经登录sso，并且生成sso和app的确认标识
                    BaseDebug.Title = "sso端 IsOnlineCallbakUrl";
                    IsOnlineCallbakUrl( );
                    break;
                case "GetKey":       //网站应用到sso获取密钥
                    BaseDebug.Title = "sso端网站应用到sso获取密钥";
                    GetKey( );
                    break;
                case "LoginService":   //登录服务中心，并且生成sso和app的确认标识，返回cookie
                    BaseDebug.Title = "sso端登录服务中心，并且生成sso和app的确认标识";
                    LoginService( );
                    break;
                case "GetAppURLGuid":   //获取同步登录的网站的URL和票据
                    BaseDebug.Title = "sso端获取同步登录的网站的URL和票据";
                    GetAppURLGuid( );
                    break;
                
                case "tiuser":
                    Response.Write("\"msg\":\"踢人了" + DataID + "\"");
           
                    ManageUserSsoOnlineInfo.Remove(int.Parse(DataID));
                    break;
            }

        }

        #region 登录页面，特殊判断

        protected override void CheckUser(IList<NatureDebugInfo> debugInfoList)
        {
            var debugInfo = new NatureDebugInfo { Title = "[sso端 CheckUser]验证用户是否登录" };

            bool isCheck = true;

            switch (base.Action)
            {
                case "CanContinueAccess": //查看登录人的状态，是否可以继续访问
                case "Login": //用户登录
                case "WhoAmI": //询问服务器，当前访问人是谁
                case "IsOnline": //网站应用访问，查看用户是否已经登录sso，并且生成sso和app的确认标识
                case "IsOnlineCallbakUrl": //网站应用访问，查看用户是否已经登录sso，并且生成sso和app的确认标识
                case "GetKey": //网站应用到sso获取密钥
                case "GetAppURLGuid": //获取同步登录的网站的URL和票据
                    isCheck = false;
                    debugInfo.Remark = "不验证";
                    break;

                case "Logout": // sso端退出登录
                    break;
            }

            //验证是否已经登录
            MyUser = null;
            if (isCheck)
            {
                //如果已经登录了，加载登录人员的信息，
                UserLoginInfo userOneself = ManageUserLoginInfo.GetUserOneselfInfoByCookie(debugInfo.DetailList);

                if (userOneself == null)
                {
                    //没有登录。
                    debugInfo.Remark += " 没有登录";
                }
                else
                {
                    debugInfo.Remark += " 登录了";
                }
            }

            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);
        
        }

        #endregion

        #region 用户登录
        private void Login()
        {
            var debugInfo = new NatureDebugInfo { Title = "[Login]接收用户名和密码，还有验证码" };

            string statusCode = "";     //状态码

            #region 接收数据
            string userCode = Request["userCode"];
            string userPsw = Request["userPsw"];
            string yzm = Request["yzm"];
            
            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);
            #endregion
            
            debugInfo = new NatureDebugInfo { Title = "检查验证码，暂时未采用" };

            #region 判断验证码是否正确
            //if (context.Session[strIdentify] == null || yzm.ToLower() != context.Session[strIdentify].ToString().ToLower())
            //{
            //    //验证码不一致，不能登录
            //    statusCode = "3";
            //    return;
            //}
          
            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);
            #endregion

            debugInfo = new NatureDebugInfo { Title = "访问数据库，验证是否匹配" };

            #region 访问数据库，验证是否匹配
            string userID = ManageUserSsoOnlineInfo.CheckUserCodePsw(userCode, userPsw, Dal,debugInfo.DetailList);

            if (userID != null)
            {
                //匹配，可以登录
                statusCode = "1";
                debugInfo.Remark = "通过验证，记录cookie。";
          
                int userIDint = Int32.Parse(userID);
                base.BaseDebug.UserId = userIDint;

                //创建访问用户的信息，并且缓存。
                //UserSsoOnlineInfo userSso = SsoManage.CreateUserSsoInfo(WebAppID, userIDint, Dal.DalUser, ssoLog);
                UserSsoOnlineInfo userSso = SsoManage.CreateUserSsoInfo(WebAppID, userIDint, Dal.DalCustomer, debugInfo.DetailList);
               
            }
            else
            {
                //登录账户和登录密码不一致，不能登录！\n请检查登录账户和登录密码是否正确！";
                statusCode = "2";
                debugInfo.Remark = "没通过验证，不记录cookie。";

            }

            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);
            #endregion

            debugInfo = new NatureDebugInfo { Title = "判断是否表单提交，还是ajax异步提交" };

            #region 判断是否表单提交，还是ajax异步提交
            string reUrl = Request["url"];

            if (!string.IsNullOrEmpty(reUrl))
            {
                //表单跨域提交，返回提交的页面
                debugInfo.Remark = "表单跨域提交，返回提交的页面：" + reUrl + "。";

                string url = reUrl + "?statusCode={0}&userCode={1}";
                Response.Redirect(string.Format(url, statusCode, userCode));
                Response.End();
            }

            if (!string.IsNullOrEmpty(CallBack))
            {
                //ajax跨域提交
                debugInfo.Remark = "ajax跨域提交。";

                Response.Write(string.Format("\"statusCode\":\"{0}\"",statusCode));
                return;
            }

            Response.Write(string.Format("\"statusCode\":\"{0}\"", statusCode));

            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);
            #endregion
            

        }
        #endregion

        #region sso端退出登录
        private void Logout()
        {
            var debugInfo = new NatureDebugInfo { Title = "[Logout] sso端退出登录" };
             
            //判断是否跳转
            string reUrl = Request.QueryString["reUrl"];
            if (string.IsNullOrEmpty(reUrl))
            {
                Response.Write("\"msg\":\"0\"");
                debugInfo.Remark = "输出信息，请求结束";
            }
            else
            {
                debugInfo.Remark += "url转向：" + reUrl + "。请求结束";

                Response.Redirect(reUrl);
            }
            
            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);
            
        }
        #endregion

        #region 询问服务器，当前访问人是谁
        private void WhoAmI()
        {
            var debugInfo = new NatureDebugInfo { Title = "[WhoAmI] sso端询问当前访问人是谁" };

            UserLoginInfo userOneself = ManageUserLoginInfo.GetUserOneselfInfoByCookie(debugInfo.DetailList);

            if (userOneself == null)
            {
                debugInfo.Remark = "没有登录";
                Response.Write("\"msg\":\"没有登录\"");
            }
            else
            {
                debugInfo.Remark = "登录了，返回信息";
                base.BaseDebug.UserId = userOneself.UserSsoID;

                //获取用户的更多信息
             
                UserSsoOnlineInfo userSso = SsoManage.CreateUserSsoInfo(WebAppID, userOneself.UserSsoID, Dal.DalCustomer, debugInfo.DetailList);

                Response.Write(string.Format("\"msg\":\"\",\"UserSsoID\":{0},\"UserJobNumber\":\"{1}\",\"UserName\":\"{2}\"", userOneself.UserSsoID, userSso.JobNumber, userSso.UserName));

            }
            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);
          

        }
        #endregion

        #region 查看登录人的状态，是否可以继续访问
        private void CanContinueAccess()
        {
            var debugInfo = new NatureDebugInfo { Title = "[CanContinueAccess] sso端可否继续访问" };
       
            //0：没有登录app网站

            //1：可以正常访问
            //2：不可以正常访问
            //3：暂停访问
            //4：被锁定不可以访问。
            //5：登录超时

            //判断IP是否和webappID匹配，不匹配的不能访问。

            //确实是指定的网站应用服务器发过来的请求，获取用户是否可以继续访问该应用。 
            string msg = SsoManage.CanContinueAccess(UserSsoID, debugInfo.DetailList);

            Response.Write(msg);

            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);
        }

        #endregion

        #region 获取同步登录的网站的URL和票据
        private void GetAppURLGuid()
        {
            var debugInfo = new NatureDebugInfo { Title = "[GetAppURLGuid] sso端获取同步登录的网站的URL和票据" };
        
            string webappUrls = "";
            string webappGuid = "";
            string webappMiwen = "";
            string webAppIDs = "";

            //获取需要同步登录的网址
            string sql = "SELECT WebAppID,AppURL FROM  SSO_webapp WHERE  WebAppID = 3"; // (AppKind IN (1, 2) and  and WebAppID <>" + WebAppID + ) "
            //object[] webappurl = dal.ExecuteStringsByColumns(sql);     //同步登录的网址
            DataTable dtAppInfo = Dal.DalCustomer.ExecuteFillDataTable(sql);

            //string msg = SsoManage.CanContinueAccess(UserSsoID, ssoLog);

            //if (msg != "1")
            //{
            //    context.Response.Write("\"msg\":\"没有登录或者不允许访问\"");
            //}
            //else
            //{
                if (dtAppInfo == null)
                {
                    Response.Write(
                     string.Format("{{\"msg\":\"{0}\",\"webappUrls\":[{1}],\"webappGuid\":[{2}],\"webappMiwen\":[{3}],\"WebAppID\":[{4}]}}", "0", "", "", "", ""));
                }
                else
                {
                    //已经登录，拼接其他网站的信息
                    foreach (DataRow dr in dtAppInfo.Rows)
                    {
                        UserSsoToAppKey verifyInfo = SsoManage.CreateUserSsoWebappVerifyInfo(Int32.Parse(dr[0].ToString()), debugInfo.DetailList);
                        webappGuid += "\"" + verifyInfo.UserGuid + "\",";
                        webappMiwen += "\"" + verifyInfo.Ticket + "\",";
                        webappUrls += "\"" + dr[1] + "\",";
                        webAppIDs += "\"" + dr[0] + "\",";

                    }
                    webAppIDs = webAppIDs.TrimEnd(',');
                    webappUrls = webappUrls.TrimEnd(',');
                    webappGuid = webappGuid.TrimEnd(',');
                    webappMiwen = webappMiwen.TrimEnd(',');


                    Response.Write(
                        string.Format("\"msg\":\"{0}\",\"webappUrls\":[{1}],\"webappGuid\":[{2}],\"webappMiwen\":[{3}],\"WebAppID\":[{4}]", "0", webappUrls, webappGuid, webappMiwen, webAppIDs));
                }
            //}

                debugInfo.Stop();
                BaseDebug.DetailList.Add(debugInfo);
        }
        #endregion

        #region 登录服务中心，并且生成sso和app的确认标识，返回cookie
        private void LoginService()
        {
            var debugInfo = new NatureDebugInfo { Title = "[LoginService] 登录服务中心" };

            string webappUrls = SsoInfo.ResourceURL;
            string webAppIDs = WebAppID;
            string webappGuid = "";
            string webappMiwen = "";

            //获取需要同步登录的网址

            //string msg = SsoManage.CanContinueAccess(UserSsoID, ssoLog);

            //if (msg != "1")
            //{
            //    context.Response.Write("{\"msg\":\"没有登录或者不允许访问\"}");
            //}
            //else
            //{

            //已经登录，拼接其他网站的信息
            UserSsoToAppKey verifyInfo = SsoManage.CreateUserSsoWebappVerifyInfo(int.Parse(webAppIDs), debugInfo.DetailList);
            webappGuid = verifyInfo.UserGuid.ToString();
            webappMiwen = verifyInfo.Ticket;

            Response.Write(
                string.Format("\"msg\":\"{0}\",\"webappUrls\":\"{1}\",\"webappGuid\":\"{2}\",\"webappMiwen\":\"{3}\",\"WebAppID\":\"{4}\"", "0",
                              webappUrls, webappGuid, webappMiwen, webAppIDs));

            //}

            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);
        }

        #endregion

        #region 网站应用到sso获取密钥
        private void GetKey()
        {
            var debugInfo = new NatureDebugInfo { Title = "[GetKey] 网站应用到sso获取密钥" };
            BaseDebug.DetailList.Add(debugInfo);

            //本来想判断的，但是申请端没有登录，所以不能判断
            //查看当前访问人是否已经登录sso
            //UserSsoInfo userSso = SsoManage.GetUserSsoInfoByCookies();
            //ssoLog.Msg += "查看是否登录。";
           
            //if (userSso == null)
            //{
                //没有登录
            //    context.Response.Write("null");
            //    ssoLog.Msg += "没有登录，不能获取密钥。";
            //}
            //else
            //{
            //    ssoLog.UserIDsso = userSso.UserSsoID;

                //登录了才能获取密钥
                string guidstr = Request.QueryString["guid"]; //：当前访问用户的标识

                //判断IP是否和webappID匹配，不匹配的不能访问。

                //获取缓存里的票据
                Guid dicKey = new Guid(guidstr);
                string strKey = dicKey.ToString();
                UserSsoToAppKey userSsoWebappVerify = ManageSSOToAppKey.Get(strKey);

                if (userSsoWebappVerify == null)
                {
                    Response.Write("\"key\":\"null\"");
                    debugInfo.Remark = "没有密钥。";
                }
                else
                {
                    string key = userSsoWebappVerify.Key;
                    //去除密钥，用一次就作废
                    //ManageSSOToAppKey.Remove(strKey);
                    //返回key
                    Response.Write("\"key\":\"" + key + "\"");
                    debugInfo.Remark = "有密钥，返回后销毁。" + key;

                }
            //}

                debugInfo.Stop();
              
        }
        #endregion

        #region 网站应用ashx页面访问，查看用户是否已经登录sso，并且生成sso和app的确认标识
        private void IsOnline()
        {
            var debugInfo = new NatureDebugInfo { Title = "[IsOnline] 网站应用ashx页面访问，查看用户是否已经登录sso，并且生成sso和app的确认标识" };

            UserLoginInfo userOneself = ManageUserLoginInfo.GetUserOneselfInfoByCookie(debugInfo.DetailList);

            //查看当前访问人是否已经登录sso
            //string msg = SsoManage.CanContinueAccess(UserSsoID, ssoLog);

            if (userOneself == null)
            {
                //没有登录
                Response.Write("\"guid\":\"\",\"miwen\":\"\"");
                debugInfo.Remark = "没有登录。";
            }
            else
            {
                //登录了，生成随机密钥，加密用户id，作为确认标识
                UserSsoToAppKey verifyInfo = SsoManage.CreateUserSsoWebappVerifyInfo(int.Parse(WebAppID), debugInfo.DetailList);
                Response.Write(string.Format("\"guid\":\"{0}\",\"miwen\":\"{1}\"",  verifyInfo.UserGuid.ToString(), verifyInfo.Ticket));
                //ssoLog.Msg += "登录了，状态：" + userSso.State + "。";
                
            }

            //清除过期的票据
            SsoManage.ClearSsoTicket();

            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);
        }
        #endregion

        #region 网站应用aspx页面访问，查看用户是否已经登录sso，并且生成sso和app的确认标识
        private void IsOnlineCallbakUrl()
        {
            var debugInfo = new NatureDebugInfo { Title = "[IsOnlineCallbakUrl] 网站应用aspx页面访问，查看用户是否已经登录sso，并且生成sso和app的确认标识" };
           
            string reUrl = Request.QueryString["reurl"];
            string reQuery = Request.QueryString["q"];

            if (string.IsNullOrEmpty(reUrl))
            {
                Response.Write("没有传递回调的URL");
                return;
            }
            if (string.IsNullOrEmpty(reQuery))
            {
                reQuery = "";
            }

            //查看当前访问人是否已经登录sso

            string msg = SsoManage.CanContinueAccess(UserSsoID, debugInfo.DetailList);

            if (msg != "1")
            {
                //没有登录
                string url = string.Format("{0}?guid={1}&miwen={2}&q={3}", reUrl, "-2", "not login sso", reQuery);
                debugInfo.Remark = "没有登录，url转向:" + url + "。请求结束。";
                debugInfo.Stop();
                BaseDebug.DetailList.Add(debugInfo);
                
                Response.Redirect(url,true);
            }
            else
            {
                //登录了，生成随机密钥，加密用户id，作为确认标识
                UserSsoToAppKey verifyInfo = SsoManage.CreateUserSsoWebappVerifyInfo(int.Parse(WebAppID), debugInfo.DetailList);
                string url = string.Format("{0}?guid={1}&miwen={2}&q={3}", reUrl, verifyInfo.UserGuid.ToString(),verifyInfo.Ticket, reQuery);

                debugInfo.Remark = "登录了，url转向:" + url;
                debugInfo.Stop();
                BaseDebug.DetailList.Add(debugInfo);

                Response.Redirect(url, true);
             
            }
        
        }
        #endregion

      
       
    }
}