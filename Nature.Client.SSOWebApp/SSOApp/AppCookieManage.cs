using System;
using System.Web;
using Nature.Common;
using Nature.SsoConfig;

namespace Nature.Client.SSOApp
{
    /// <summary>
    /// 管理客户端的cookie
    /// </summary>
    /// user:jyk
    /// time:2013/4/8 16:35
    public static  class AppCookieManage
    {
        private static string _appCookieName = "webappNtfwUserssoID";

        #region 创建app cookie webappNtfwUserssoID
        /// <summary>
        /// 创建应用端的用户cookie
        /// </summary>
        /// <returns></returns>
        /// user:jyk
        /// time:2013/4/8 17:30
        public static string CreateAppCookie(UserWebappInfo userWebappInfo, string userIP)
        {
            //在本地做一个标识
            string source = string.Format("{0}_{1}_{2}_{3}_{4}_{5}", SsoInfo.WebAppID, userWebappInfo.UserSsoID, userWebappInfo.UserWebappID, userIP,
                                          DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), userWebappInfo.GuidKey);

            string miwen = DesUrl.Encrypt(source, SsoInfo.AppKey);

            //票据保存到cookies，作为访问用户的标识
            HttpContext.Current.Response.Cookies[_appCookieName].Value = miwen;

            return miwen;

            //string re = "";
            //var httpCookie = HttpContext.Current.Request.Cookies[_appCookieName];
            //if (httpCookie != null)
            //{
            //    re = httpCookie.Value;
            //}

            //return re;
        }
        #endregion

        #region 获取应用端的cookie的值 webappNtfwUserssoID
        /// <summary>
        /// 获取应用端的cookie的值 webappNtfwUserssoID
        /// </summary>
        /// <returns></returns>
        /// user:jyk
        /// time:2013/4/8 16:39
        public static string GetAppCookieValue()
        {
            string re = "";
            var httpCookie = HttpContext.Current.Request.Cookies[_appCookieName];
            if (httpCookie != null)
            {
                re = httpCookie.Value;
            }

            return re;
        }
        #endregion

        #region 清除应用端的cookie webappNtfwUserssoID
        /// <summary>
        /// 清除应用端的cookie webappNtfwUserssoID
        /// </summary>
        /// user:jyk
        /// time:2013/4/8 16:36
        public static void ClearAppCookie()
        {
            var httpCookie = HttpContext.Current.Request.Cookies[_appCookieName];
            if (httpCookie != null)
            {
                httpCookie.Value = "";
                httpCookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.AppendCookie(httpCookie);
            }
        }
        #endregion

    
    }
}