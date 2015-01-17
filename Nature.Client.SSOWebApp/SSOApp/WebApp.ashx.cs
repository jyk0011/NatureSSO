using System.Collections.Generic;
using Nature.Common;
using Nature.DebugWatch;
using Nature.Service.Ashx;
using Nature.SsoConfig;

namespace Nature.Client.SSOApp
{
    /// <summary>
    /// 单点登录，网站应用端的处理事件
    /// </summary>
    public class WebApp : BaseAshx
    {
        /// user:jyk
        /// time:2013/1/28 14:46
        /// user:jyk
        /// time:2013/4/8 16:44
        public override void Process()
        {
            base.Process();

            switch (Action.ToLower())
            {
                    //金洋增加
                case "whoamiajax": //01 金洋增加，避免和asp的访问冲突。
                    BaseDebug.Title = "whoamiajax";
                    WhoAmIAjax();
                    break;
                    //增加结束

                case "getuserinfo": //00 获取登录人的信息，姓名、账号、id
                    BaseDebug.Title = "获取登录人信息";
                    GetUserInfo();
                    break;
              
                case "login": //02 登录app
                    Login();
                    break;
                case "logout": // 03
                    Logout();
                    break;
                case "getck": //04 获取当前用户的cookies
                    GetCookies();
                    break;
                case "getssourl": //05 返回sso的网址和网站应用的ID
                    GetSSOUrl();
                    break;
            }

        }

        #region 登录页面，特殊判断

        protected override void CheckUser(IList<NatureDebugInfo> debugInfoList)
        {
            var debugInfo = new NatureDebugInfo { Title = "[app端]验证用户是否登录，不验证。" };
           
            debugInfo.Stop();
            BaseDebug.DetailList.Add(debugInfo);

        }

        #endregion

        #region 00 获取当前登录人的信息
        private void GetUserInfo()
        {
            var debugInfo = new NatureDebugInfo { Title = "[Nature.Client.SSOApp.WebApp.GetUserInfo] 获取当前登录人的信息" };
            BaseDebug.DetailList.Add(debugInfo);

            UserWebappInfo userWebapp = AppManage.UserWebappInfoByCookies(debugInfo.DetailList);

            //到数据库获取信息
            string sql = "";
            
            string msg = string.Format("\"msg\":\"\",\"UserSsoID\":{0},\"UserJobNumber\":\"{1}\",\"UserName\":\"{2}\"",
                                       userWebapp.UserSsoID, userWebapp.WebAppID, userWebapp.UserWebappID);
            Response.Write(msg);

            debugInfo.Remark = "发送信息： 。请求结束";
            debugInfo.Stop();
         
        }
        #endregion

        #region 01 当前登录人是谁 //金洋增加，避免和asp的访问冲突
        private void WhoAmIAjax()
        {
            var debugInfo = new NatureDebugInfo { Title = "[Nature.Client.SSOApp.WebApp.WhoAmIAjax] 当前登录人是谁" };
            BaseDebug.DetailList.Add(debugInfo);

            UserWebappInfo userWebapp = AppManage.UserWebappInfoByCookies(debugInfo.DetailList);

            //var arr = new string[6];
            //arr[0] = "没有登录app网站";
            //arr[1] = "可以正常访问";
            //arr[2] = "不可以正常访问";
            //arr[3] = "暂停访问";
            //arr[4] = "被锁定不可以访问";
            //arr[5] = "登录超时";

            string msg = string.Format("\"state\":\"{0}\",\"userSsoID\":\"{1}\",\"userAppID\":\"{2}\"",
                                       (int)userWebapp.State, userWebapp.UserSsoID, userWebapp.UserWebappID);
            Response.Write(msg);

            BaseDebug.UserId = userWebapp.UserSsoID;
            debugInfo.Remark = "发送信息：" + msg.Replace("\"","") + "。请求结束";
            debugInfo.Stop();
            //BaseDebug.DetailList.Add(debugInfo);
         
        }
        #endregion
   
        //增加结束

        #region 02 登录app
        private void Login()
        {
            var debugInfo = new NatureDebugInfo { Title = "[Nature.Client.SSOApp.WebApp.Login] 登录app" };
             
            #region 获取网站中转过来的凭证
            var guid = Request["guid"]; //标识
            var miwen = Request["miwen"]; //票据

            if (string.IsNullOrEmpty(miwen))
            {
                Response.Write("\"msg\":\"没有发现密文！\"");
                return;
            }

            //验证凭证，必须是guid格式
            if (!Functions.IsGuid(guid))
            {
                Response.Write("\"msg\":\"guid格式不正确！\"");
                return;
            }
            #endregion

            //做标记
            UserWebappInfo userWebapp = AppManage.LoginApp(guid, miwen, debugInfo.DetailList );

            if (userWebapp.Error.Length == 0)
            {
                //没有错误
                string re =
                    string.Format("\"msg\":\"0\",\"userSsoID\":\"{0}\",\"userAppID\":\"{1}\",\"ticketAppID\":\"{2}\",\"configAppID\":\"{3}\"",
                        userWebapp.UserSsoID, userWebapp.UserWebappID, userWebapp.WebAppID, SsoInfo.WebAppID);
                Response.Write(re);
            }
            else
            {
                //有点小问题
                Response.Write(string.Format("\"msg\":\"{0}\"",userWebapp.Error));
            }

            debugInfo.Remark = "请求结束。";
            debugInfo.Stop();
            base.BaseDebug.DetailList.Add(debugInfo);
         
        }
        #endregion

        #region 03 登出
        private void Logout()
        {
            var debugInfo = new NatureDebugInfo { Title = "[Nature.Client.SSOApp.WebApp.Logout] 登出" };
            AppCookieManage.ClearAppCookie();

            string re = string.Format("\"msg\":\"0\",\"userAppID\":\"{0}\"", SsoInfo.WebAppID);
            Response.Write(re);

            debugInfo.Remark = "清除本地cookie，请求结束。";
            debugInfo.Stop();
            base.BaseDebug.DetailList.Add(debugInfo);
        }
        #endregion

        #region 04 获取登录人的cookie标识
        private void GetCookies()
        {
            var debugInfo = new NatureDebugInfo { Title = "[Nature.Client.SSOApp.WebApp.GetCookies] 获取登录人的cookie标识 暂时屏蔽" };

            string ck = "pingbi";// AppCookieManage.GetAppCookieValue();
            Response.Write(string.Format("\"ck\":\"{0}\"", ck));

            debugInfo.Stop();
            base.BaseDebug.DetailList.Add(debugInfo);
        }
        #endregion
     
        #region 05 返回sso的网址和网站应用的ID
        private void GetSSOUrl()
        {
            var debugInfo = new NatureDebugInfo { Title = "[Nature.Client.SSOApp.WebApp.GetSSOUrl] 返回sso的网址和网站应用的ID" };

            string msg = string.Format("\"SSOUrl\":\"{0}\",\"WebAppID\":\"{1}\",\"Debug\":\"{2}\"", SsoInfo.SSOUrl,
                                       SsoInfo.WebAppID, SsoInfo.IsWriteAjaxDebug);
            Response.Write(msg);

            debugInfo.Stop();
            base.BaseDebug.DetailList.Add(debugInfo);

        }
        #endregion

      

    }
}