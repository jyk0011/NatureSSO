using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Web;
using Nature.Common;
using Nature.DebugWatch;
using Nature.Service;
using Nature.SsoConfig;

namespace Nature.Client.SSOApp
{
    /// <summary>
    /// 网站应用端的管理
    /// </summary>
    /// user:jyk
    /// time:2013/1/29 18:32
    public class AppManage
    {
        #region 获取当前访问者的信息
        /// <summary>
        /// 获取当前访问者的信息。
        /// 前提：已经在本站登录。根据本地cookies生成
        /// </summary>
        /// <returns></returns>
        /// user:jyk
        /// time:2013/1/29 18:36
        public static UserWebappInfo GetUserIDyCookies()
        {
            var userWebapp = new UserWebappInfo();

            string ticket = AppCookieManage.GetAppCookieValue();

            //获取访问用户的标识
            if (ticket.Length == 0)
            {
                //没有票据，验证是不是服务器直接访问
                ticket = HttpContext.Current.Request.QueryString["userAppCookie"];
                if (string.IsNullOrEmpty(ticket))
                    ticket = "";
                else
                {
                    //验证访问IP是不是规定的app的IP
                }
            }

            if (ticket.Length == 0)
            {
                //没有登录
                userWebapp.State = UserState.NotLoginApp;
            }
            else
            {
                //登录了，解密
                string yuanwen = DesUrl.Decrypt(ticket, SsoInfo.AppKey);
                string[] user = yuanwen.Split('_');

                userWebapp.WebAppID = user[0];
                userWebapp.UserSsoID = Int32.Parse(user[1]);
                userWebapp.UserWebappID = Int32.Parse(user[2]);
                userWebapp.IP = user[3];
                
                userWebapp.GuidKey = user[5];
                userWebapp.State = UserState.NormalAccess;
            
            }

            return userWebapp;
        }
        #endregion

        #region 获取当前访问者的信息
        /// <summary>
        /// 获取当前访问者的信息，询问sso用户状态
        /// 前提：已经在本站登录。根据本地cookies生成
        /// </summary>
        /// <returns></returns>
        /// user:jyk
        /// time:2013/1/29 18:36
        public static UserWebappInfo UserWebappInfoByCookies(IList<NatureDebugInfo> debugInfoList)
        {
            var debugInfo = new NatureDebugInfo { Title = "[UserWebappInfoByCookies] 获取当前访问者的信息" };

            var userWebapp = GetUserIDyCookies();
            debugInfo.Stop();
            debugInfoList.Add(debugInfo);

            if (userWebapp.State != UserState.NotLoginApp)
            {
                debugInfo.Remark = userWebapp.State.ToString();

                debugInfo = new NatureDebugInfo { Title = "[UserWebappInfoByCookies] 获取当前访问者的信息" };

                //询问sso是否登录，是否可以继续访问
                //string ssoState = Functions.GetHtmlByUrl(string.Format(
                //        "{0}/SSOAuth/SSOAuth.ashx?action=CanContinueAccess&webappID={1}&userSSOID={2}&key={3}", SsoInfo.SSOUrl, SsoInfo.WebAppID, userWebapp.UserSsoID, userWebapp.GuidKey));

                //设置状态
                userWebapp.State = UserState.NormalAccess;//(UserState)(Int32.Parse(ssoState)); //
              
                debugInfo.Stop();
                debugInfoList.Add(debugInfo);

            }

            return userWebapp;
        }
        #endregion

        #region 登录app

        /// <summary>
        /// 在应用端做标记
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="miwen"></param>
        /// <param name="debugInfoList"></param>
        /// <returns></returns>
        public static UserWebappInfo LoginApp(string guid, string miwen, IList<NatureDebugInfo> debugInfoList)
        {
            var debugInfo = new NatureDebugInfo { Title = "[LoginApp] 在应用端做标记" };

            var userWebappInfo = new UserWebappInfo();

            userWebappInfo.Error = string.Empty;

            //到sso端获取解密密钥
            string url = string.Format("{0}/SSOAuth/SSOAuth.ashx?action=GetKey&webappID={1}&guid={2}", SsoInfo.SSOUrl, SsoInfo.WebAppID, guid);
            //HttpContext.Current.Response.Write(url);
            debugInfo.Remark = "向sso申请密钥：url:" + url;
            //string ssokey = Functions.GetHtmlByUrl(url);

            string errorMsg = "";
            string ssokey = MyWebClient.Post(url,null,out errorMsg);
            
            if (errorMsg.Length > 1)
            {
                userWebappInfo.Error = "获取密钥的时候发生意外！url:" + url + "errorMsg:" + errorMsg;
                debugInfo.Remark += "。获取密钥的时候发生意外：key:" + url + "errorMsg:" + errorMsg;
                debugInfo.Stop();
                debugInfoList.Add(debugInfo);
                return userWebappInfo;
            }

            var dicRe = Json.JsonToDictionary(ssokey);
            ssokey = dicRe["key"];
            if (ssokey.Length > 100)
            {
                userWebappInfo.Error = "密钥长度不正确！url————:" + url + "__" + ssokey + "||";
                debugInfo.Remark += "。密钥长度不正确：key:" + url + "___" + ssokey + "||";
                debugInfo.Stop();
                debugInfoList.Add(debugInfo);
                return userWebappInfo;
            }

            if (ssokey == "null")
            {
                userWebappInfo.Error = "没有获得密钥！key:" + ssokey + "，url:" + url + "。";
                debugInfo.Remark += "。没有获得密钥：key:" + ssokey + "，url:" + url + "。";
                debugInfo.Stop();
                debugInfoList.Add(debugInfo);
                return userWebappInfo;
            }

            debugInfo.Remark += "。申请密钥成功：key:" + ssokey + "。";

            //解密
            string yuanwen = DesUrl.Decrypt(miwen, ssokey);
            //string[] user = yuanwen.Split('_');

            //userWebappInfo.UserSsoID = int.Parse(user[2]);
            //userWebappInfo.UserWebappID = int.Parse(user[3]);
            //userWebappInfo.State = UserState.NormalAccess;
            //userWebappInfo.IP = user[4];
            //userWebappInfo.WebAppID = user[1];
            //userWebappInfo.GuidKey = user[6];

            var dicYuanwen = Json.JsonToDictionary(yuanwen);
            userWebappInfo.UserSsoID = int.Parse(dicYuanwen["userIDsso"]);
            userWebappInfo.UserWebappID = int.Parse(dicYuanwen["userIDapp"]);
            userWebappInfo.State = UserState.NormalAccess;
            userWebappInfo.IP = dicYuanwen["userIP"];
            userWebappInfo.WebAppID = dicYuanwen["webAppID"];
            userWebappInfo.GuidKey = dicYuanwen["GuidKey"];


            //验证包里的webappID和配置信息是否一致
            //if (userWebappInfo.WebAppID != AppConfig.WebAppID)
            //{
            //    //string msg = string.Format("\"msg\":\"票据的WebAppID不符合。票据：{0}；配置：{1}\"", userWebappInfo.WebAppID, AppConfig.WebAppID);
            //    string msg = string.Format("票据的WebAppID不符合。票据：{0}；配置：{1}", userWebappInfo.WebAppID, AppConfig.WebAppID);
            //    userWebappInfo.Error = msg;
            //    //HttpContext.Current.Response.Write(msg);
            //    ssoLog.Msg += "验证未通过：" + msg + "。";
            //    return userWebappInfo;
            //}

            string userIP = HttpContext.Current.Request.UserHostAddress;

            //验证包里的【IP】和【访问IP】是否一致
            if (userWebappInfo.IP != userIP)
            {
                #region 张晓丰编辑 判断白名单黑名单
                //是否在白名单中
                bool IsInWhite = false;
                if (userIP == null)
                {
                    userWebappInfo.Error = "当前无法取得用户IP";
                    return userWebappInfo;
                }
                //取得黑白名单
                string listwhite = ConfigurationManager.AppSettings["ssolistwhite"];
                string listblack = ConfigurationManager.AppSettings["ssolistblack"];
                if (!string.IsNullOrEmpty(listblack))
                {
                    string[] list_Arr = listblack.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (string str in list_Arr)
                    {
                        if (!userIP.Contains(str)) continue;
                        userWebappInfo.Error = "当前无法取得用户IP";
                        return userWebappInfo;
                    }
                }
                if (!string.IsNullOrEmpty(listwhite))
                {
                    string[] list_Arr = listwhite.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (string str in list_Arr)
                    {
                        if (!userIP.Contains(str)) continue;
                        IsInWhite = true;
                        break;
                    }
                }
                #endregion
                if (!IsInWhite && userIP != "127.0.0.1" && userIP != "::1")
                {
                    userWebappInfo.Error = string.Format("\"msg\":\"票据的IP不符合。票据：{0}；本次：{1}\"", userWebappInfo.IP, userIP);
                    debugInfo.Stop();
                    debugInfoList.Add(debugInfo);
                    return userWebappInfo;
                }
            }

            //创建cookie
            AppCookieManage.CreateAppCookie(userWebappInfo, userIP);
            debugInfo.Stop();
            debugInfoList.Add(debugInfo);

            return userWebappInfo;
        }
        #endregion

        #region 登出sso和app
        /// <summary>
        /// 退出登录，清除本地cookie。跳页的方式退出sso
        /// </summary>
        /// <param name="reUrl">必须http://开头</param>
        /// user:jyk
        /// time:2013/3/26 18:25
        public static void Logout(string reUrl)
        {
            AppCookieManage.ClearAppCookie();

            //跳页面的方式退出sso
            string url = SsoInfo.SSOUrl + "/ssoauth/ssoauth.ashx?action=Logout&reUrl=" + reUrl;
            HttpContext.Current.Response.Redirect(url);

        }
        #endregion

        //========
        /// <summary>
        /// 判断sso是否登录，如果没登录，跳转
        /// </summary>
        /// <returns></returns>
        /// user:jyk
        /// time:2013/3/26 14:07
        public static UserWebappInfo WebformIslogin(IList<NatureDebugInfo> debugInfoList)
        {
            var debugInfo = new NatureDebugInfo { Title = "[WebformIslogin] 判断sso是否登录，如果没登录，跳转" };

            //webform判断是否登录

            UserWebappInfo userWebappInfo = UserWebappInfoByCookies(debugInfo.DetailList);
            userWebappInfo.Error = string.Empty;

            string guid = HttpContext.Current.Request.QueryString["guid"];
            #region 张晓丰修正.修正已经登陆的人切换登陆用户时本站点Cookie清空
            if (userWebappInfo.State != UserState.NormalAccess)
            {
                AppCookieManage.ClearAppCookie();

                userWebappInfo.State = UserState.NotLoginApp;
            }
            #endregion

            switch (userWebappInfo.State )
            {
                case UserState.NotLoginApp:
                    //0 没有登录app，判断有没有guid ，没有参数询问sso；有参数实现登录
                    //可以直接跳转（不需要保存post数据），也可以打开新窗口（post）
                    //目前只实现直接跳转

                    string miwen = HttpContext.Current.Request.QueryString["miwen"];

                    miwen = HttpContext.Current.Server.UrlDecode(miwen);
                    if (string.IsNullOrEmpty(guid))
                    {
                        //没有参数，跳转到sso，询问sso是否登录
                        string reUrl = HttpContext.Current.Request.Url.ToString();
                        string reQuery = "";

                        //处理参数
                        string[] tmpUrl = reUrl.Split('?');
                        reUrl = tmpUrl[0];
                        if (tmpUrl.Length >1)
                        {
                            reQuery = DesUrl.Encrypt(tmpUrl[1],"12345678");
                        }
                        string url = string.Format("{0}/SSOAuth/SSOAuth.ashx?action=IsOnlineCallbakUrl&webappID={1}&reurl={2}&q={3}", SsoInfo.SSOUrl, SsoInfo.WebAppID, reUrl, reQuery);
                        HttpContext.Current.Response.Redirect(url);
                    }
                    else
                    {
                        //有参数，判断
                        if (!Functions.IsGuid(guid))
                        {
                            //没登录
                            userWebappInfo.State = UserState.LoginTimeout;
                            return userWebappInfo;
                        }

                        //实现登录app
                        if (string.IsNullOrEmpty(miwen))
                        {
                            //没有传递密文
                            userWebappInfo.Error = "没有传递密文";
                        }
                        else
                        {
                            userWebappInfo = LoginApp(guid, miwen, debugInfo.DetailList);

                            if (userWebappInfo.Error.Length ==0)
                            {
                                var urls = HttpContext.Current.Request.Url;
                                string url = "http://";
                                if (urls.Port == 80)
                                    url += urls.Host + urls.LocalPath;
                                else
                                    url += urls.Host + ":" + urls.Port + urls.LocalPath;

                                //判断原有的参数
                                string query = HttpContext.Current.Request.QueryString["q"];
                                if (!string.IsNullOrEmpty(query))
                                {
                                    url += "?" + DesUrl.Decrypt(query, "12345678");
                                }
                                HttpContext.Current.Response.Redirect(url);
                            }
                        }
                    }
                    break;

                case UserState.NormalAccess:
                    //1 可以正常访问，看有没有guid参数，有的话跳转去掉参数
                    if (!string.IsNullOrEmpty(guid))
                    {
                        var urls = HttpContext.Current.Request.Url;
                        string url = "http://";
                        if (urls.Port == 80)
                            url += urls.Host + urls.LocalPath;
                        else
                            url += urls.Host + ":"+ urls.Port  + urls.LocalPath;
                        
                        //判断原有的参数
                        string query = HttpContext.Current.Request.QueryString["q"];
                        if (!string.IsNullOrEmpty(query))
                        {
                            url += "?" + DesUrl.Decrypt(query, "12345678");
                        }
                        HttpContext.Current.Response.Redirect(url);
                   
                    }
                    break;

                case UserState.SuspendAccess:
                    //3 暂停访问。终止
                    break;

                case UserState.Locked:
                    //4 被锁定不能访问
                    break;

                case UserState.LoginTimeout:
                    //5 sso超时
                    break;
         
            }
            debugInfo.Stop();
            debugInfoList.Add(debugInfo);
      
            return userWebappInfo;
        }

    }
}
