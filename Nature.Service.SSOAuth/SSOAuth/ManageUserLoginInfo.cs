using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using Nature.Common;
using Nature.DebugWatch;

namespace Nature.Service.SSOAuth
{
    #region 一个账号多个用户登录的不同的信息
    /// <summary>
    /// 登录信息，即一个账号多人登录的区分标识
    /// </summary>
    /// user:jyk
    /// time:2013/4/2 11:00
    public class UserLoginInfo
    {
        /// <summary>
        /// cookie里的标志。
        /// </summary>
        /// user:jyk
        /// time:2013/4/2 13:31
        public Guid GuidKey { get; set; }

        /// <summary>
        /// 用户在SSO端的用户ID
        /// </summary>
        /// user:jyk
        /// time:2013/1/23 15:51
        public int UserSsoID { get; set; }

        /// <summary>
        /// 创建时间，用于判断是否超时失效
        /// 用于清理无效信息
        /// </summary>
        /// user:jyk
        /// time:2013/1/23 15:53
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后访问时间
        /// 用于清理无效信息
        /// </summary>
        /// user:jyk
        /// time:2013/1/23 15:53
        public DateTime LastTime { get; set; }

        /// <summary>
        /// 客户端IP
        /// </summary>
        /// user:jyk
        /// time:2013/1/23 15:54
        public string UserIP { get; set; }

        /// <summary>
        /// 加密、解密的密钥
        /// </summary>
        /// user:jyk
        /// time:2013/1/23 16:23
        public string PasswordKey { get; set; }

        /// <summary>
        /// 票据。保存生成的加密字符串，作为备案
        /// 格式：userSSOID_IP_登录时间
        /// </summary>
        /// user:jyk
        /// time:2013/1/23 15:55
        public string Ticket { get; set; }



    }
    #endregion

    /// <summary>
    /// 一个账户多人登录，每个人的自己的标识的管理
    /// 创建，加入缓存，移除缓存，获取标识
    /// </summary>
    /// user:jyk
    /// time:2013/4/2 10:04
    public static class ManageUserLoginInfo
    {
        //cache的名称
        private const string SSOUserCacheName = "nfwUserSsoCookie";
        //cookie的名称
        private const string SSOUserCookieName = "ssoNtfwUserssoID";

        #region 创建一个标识
        /// <summary>
        /// 有人登录了，创建一个标识，并且加到字典和缓存
        /// </summary>
        /// <param name="webAppId">The web app id.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="debugInfoList">The sso log.</param>
        /// <returns></returns>
        /// user:jyk
        /// time:2013/4/2 14:36
        public static UserLoginInfo Create(string webAppId, int userId, IList<NatureDebugInfo> debugInfoList)
        {
            var debugInfo = new NatureDebugInfo { Title = "[GetGuidKeyByCookie]sso端，根据cookie获取用户自己的标识" };

            //判断当前cookie是否有已经登录的，如果有先去掉对应的标识。
            //Remove(ssoLog);

            Guid dicKey = Guid.NewGuid(); //字典的key，同时放在cookies里面，作为标识
            string key = Guid.NewGuid().ToString(); //生成加密密钥
            string userIP = HttpContext.Current.Request.UserHostAddress;
            //sso端的标识包
            string source = string.Format("{0}_{1}_{2}", userId, userIP, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            //加密
            string miwen = DesUrl.Encrypt(source, key);

            var userOneself = new UserLoginInfo
                                  {
                                      GuidKey = dicKey,
                                      UserSsoID = userId,
                                      CreateTime = DateTime.Now,
                                      LastTime = DateTime.Now,
                                      UserIP = userIP,
                                      PasswordKey = key,
                                      //格式字符串只能是“D”、“d”、“N”、“n”、“P”、“p”、“B”或“b”。
                                      Ticket = dicKey.ToString("N") + miwen
                                  };

            //加到字典和缓存
            Add(userOneself);

            //票据保存到cookies，作为访问用户的标识
            HttpContext.Current.Response.Cookies[SSOUserCookieName].Value = userOneself.Ticket;
            debugInfo.Stop();
            debugInfoList.Add(debugInfo);
      
            return userOneself;

        }

        #endregion

        #region 添加一个标识到缓存
        /// <summary>
        /// 添加一个标识到缓存
        /// </summary>
        /// <param name="userOneself">自己的标记</param>
        /// user:jyk
        /// time:2013/4/2 10:08
        public static void Add( UserLoginInfo userOneself )
        {
            Dictionary<Guid, UserLoginInfo> userOneselfInfo;

            if (HttpContext.Current.Cache[SSOUserCacheName] == null)
                userOneselfInfo = new Dictionary<Guid, UserLoginInfo>();
            else
                userOneselfInfo = (Dictionary<Guid, UserLoginInfo>)HttpContext.Current.Cache[SSOUserCacheName];

            userOneselfInfo.Add(userOneself.GuidKey, userOneself);

            HttpContext.Current.Cache.Insert(SSOUserCacheName, userOneselfInfo, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration);

        }
        #endregion

        #region 删除对应的索引
        /// <summary>
        /// 根据cookie，删除对应的记录
        /// </summary>
        /// <param name="debugInfoList">写日志的标识</param>
        /// user:jyk
        /// time:2013/4/2 10:08
        public static void Remove(IList<NatureDebugInfo> debugInfoList)
        {
            var debugInfo = new NatureDebugInfo { Title = "[GetGuidKeyByCookie]sso端，根据cookie获取用户自己的标识" };

            Guid? guidKey = GetGuidKeyByCookie(debugInfo.DetailList);

            Guid key;
            if (guidKey == null)
            {
                return ;
            }
            else
            {
                key = (Guid)guidKey;
            }

            //根据cookie，去掉对应的标记
            Dictionary<Guid, UserLoginInfo> ssoUserCookie;

            if (HttpContext.Current.Cache[SSOUserCacheName] == null)
                return ;
            else
                ssoUserCookie = (Dictionary<Guid, UserLoginInfo>)HttpContext.Current.Cache[SSOUserCacheName];

            //修改在线用户列表

            //修改cookie标记缓存
            ssoUserCookie.Remove(key);

            HttpContext.Current.Cache.Insert(SSOUserCacheName, ssoUserCookie, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration);

            //去掉本地cookies
            var httpCookie = HttpContext.Current.Request.Cookies[SSOUserCookieName];
            if (httpCookie != null)
            {
                httpCookie.Value = "";
                httpCookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.AppendCookie(httpCookie);
            }

            debugInfo.Remark = "清除cookie。";
            debugInfo.Stop();
            debugInfoList.Add(debugInfo);
      
        }
        #endregion

        #region 根据cookie获取用户自己的标识

        /// <summary>
        /// 获取sso的cookie里的用户自己的标识
        /// </summary>
        /// <param name="debugInfoList">写日志的标识</param>
        /// <returns></returns>
        /// user:jyk
        /// time:2013/4/2 10:20
        public static Guid? GetGuidKeyByCookie(IList<NatureDebugInfo> debugInfoList)
        {
            var debugInfo = new NatureDebugInfo { Title = "[GetGuidKeyByCookie]sso端，根据cookie获取用户自己的标识" };
 
            string ckUserKey = "";

            //检查cookie里是否有需要的记录
            var httpCookie = HttpContext.Current.Request.Cookies[SSOUserCookieName];
            if (httpCookie != null)
            {
                ckUserKey = httpCookie.Value;
                debugInfo.Remark = "sso的cookie里有标识。";
            }
            else
            {
                debugInfo.Remark = "sso的cookie里没有标识。";
                debugInfo.Stop();
                debugInfoList.Add(debugInfo); return null;
            }

            string key = ckUserKey.Substring(0, 32);
            //判断是否是guid
            if (!Functions.IsGuid(key + "1234"))
            {
                debugInfo.Remark = "sso的cookie格式不正确：" + key + "。";
                debugInfo.Stop();
                debugInfoList.Add(debugInfo); return null;
            }

            Guid? guidKey = new Guid(key);
      
            debugInfo.Stop();
            debugInfoList.Add(debugInfo);
      
            return guidKey;

        }
        #endregion
    
        #region 根据cookie获取用户自己的标识

        /// <summary>
        /// 根据cookie获取用户自己的标识
        /// </summary>
        /// <param name="debugInfoList">写日志的标识</param>
        /// <returns></returns>
        /// user:jyk
        /// time:2013/4/2 10:20
        public static UserLoginInfo GetUserOneselfInfoByCookie(IList<NatureDebugInfo> debugInfoList)
        {
            var debugInfo = new NatureDebugInfo { Title = "[GetUserOneselfInfoByCookie]sso端，根据cookie获取用户自己的标识" };
 
            //获取sso的cookie里的标识
            Guid? guidKey = GetGuidKeyByCookie(debugInfo.DetailList);

            Guid key;
            if (guidKey == null)
            {
                return null;
            }
            else
            {
                key = (Guid)guidKey;
            }

            Dictionary<Guid, UserLoginInfo> dicUserOneselfInfo;

            if (HttpContext.Current.Cache[SSOUserCacheName] == null)
            {
                debugInfo.Remark = "没有UserOneselfInfo缓存列表。";
                debugInfo.Stop();
                debugInfoList.Add(debugInfo);
      
                return null;
            }
            else
            {
                dicUserOneselfInfo = (Dictionary<Guid, UserLoginInfo>)HttpContext.Current.Cache[SSOUserCacheName];
            }

            if (dicUserOneselfInfo.ContainsKey(key))
            {
                debugInfo.Remark = "UserOneselfInfo的缓存里有对应记录。";
                debugInfo.Stop();
                debugInfoList.Add(debugInfo);
      
                return dicUserOneselfInfo[key];
            }
            else
            {
                debugInfo.Remark = "UserOneselfInfo的缓存里没有对应记录。";
                debugInfo.Stop();
                debugInfoList.Add(debugInfo);
      
                return null;
            }

        }
        #endregion

        #region 根据cookie获取用户自己的标识

        /// <summary>
        /// 根据 key 获取用户自己的标识
        /// </summary>
        /// <param name="key">字典的key </param>
        /// <param name="debugInfoList">写日志的标识</param>
        /// <returns></returns>
        /// user:jyk
        /// time:2013/4/2 10:20
        public static UserLoginInfo GetUserOneselfInfoByKey(Guid key, IList<NatureDebugInfo> debugInfoList)
        {
            var debugInfo = new NatureDebugInfo
                {
                    Title = "[GetUserOneselfInfoByKey]sso端，根据 key 获取用户自己的标识",
                    Remark = key.ToString()
                };

            Dictionary<Guid, UserLoginInfo> dicUserOneselfInfo;

            if (HttpContext.Current.Cache[SSOUserCacheName] == null)
            {
                debugInfo.Remark = "没有UserOneselfInfo缓存列表。";
                debugInfo.Stop();
                debugInfoList.Add(debugInfo);
      
                return null;
            }
            else
            {
                dicUserOneselfInfo = (Dictionary<Guid, UserLoginInfo>)HttpContext.Current.Cache[SSOUserCacheName];
            }

            if (dicUserOneselfInfo.ContainsKey(key))
            {
                debugInfo.Remark = "UserOneselfInfo的缓存里有对应记录。";
                debugInfo.Stop();
                debugInfoList.Add(debugInfo);
                return dicUserOneselfInfo[key];
            }
            else
            {
                debugInfo.Remark = "UserOneselfInfo的缓存里没有对应记录。";
                debugInfo.Stop();
                debugInfoList.Add(debugInfo);
                return null;
            }

        }
        #endregion

        #region 判断cookie里的包格式是否正确
        public static bool CheckCookieIsRight()
        {
            //判断包是否正确
            //if (ckValue != userSso.Ticket)
            //    return null;
            //if (userSso.UserIP != userIP)
            //    return null;

            //#endregion

            ////解密
            ////string yuanwen = DesBase64.Decrypt(miwen, userSso.Key);
            //string yuanwen = DesUrl.Decrypt(miwen, userSso.PasswordKey);

            //string[] tmpYuanwem = yuanwen.Split('_');
            ////判断IP、时间

            //if (Int32.Parse(tmpYuanwem[0]) != userSso.UserSsoID)
            //    return null;

            //if ((tmpYuanwem[1]) != userIP)
            //    return null;

            return true;
        }
        #endregion
    }
}