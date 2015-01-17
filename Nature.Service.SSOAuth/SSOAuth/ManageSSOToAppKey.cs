using System;
using System.Web;

namespace Nature.Service.SSOAuth
{
    #region sso和app交换的票据，存放用户ID等信息
    /// <summary>
    /// sso和app交换的票据，存放用户ID等信息
    /// </summary>
    /// user:jyk
    /// time:2013/1/23 15:54
    public class UserSsoToAppKey
    {
        /// <summary>
        /// 当前访问用户的临时标识。第一次访问的时候生成Guid用于标识当前访问的用户
        /// </summary>
        /// <value>
        /// 第一次访问的时候生成Guid用于标识当前访问的用户
        /// </value>
        /// user:jyk
        /// time:2013/1/23 15:51
        public Guid UserGuid { get; set; }

        /// <summary>
        /// 创建时间，用于判断是否超时失效
        /// </summary>
        /// user:jyk
        /// time:2013/1/23 15:53
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 加密、解密的密钥
        /// </summary>
        /// user:jyk
        /// time:2013/1/23 16:23
        public string Key { get; set; }

        /// <summary>
        /// 票据。保存生成的加密字符串，作为备案
        /// 格式：userSSOID_userAppID_IP_登录时间
        /// </summary>
        /// user:jyk
        /// time:2013/1/23 15:55
        public string Ticket { get; set; }


    }
    #endregion

    #region 管理sso与app之间沟通的票据 
    /// <summary>
    /// 管理sso与app之间沟通的票据，采用application保存
    /// </summary>
    /// user:jyk
    /// time:2013/3/28 11:16
    public static class ManageSSOToAppKey
    {
        /// <summary>
        /// 添加一个票据
        /// </summary>
        /// <param name="key">Guid的key，转成string</param>
        /// <param name="userSsoInfo">票据类，用于验证</param>
        /// user:jyk
        /// time:2013/3/28 11:18
        public static void Add(string key, UserSsoToAppKey userSsoInfo)
        {
            HttpContext.Current.Application.Lock();
            HttpContext.Current.Application[key] = userSsoInfo;
            HttpContext.Current.Application.UnLock();
                
        }

        /// <summary>
        /// 删除（去掉）一个票据
        /// </summary>
        /// <param name="key">Guid的key，转成string</param>
        /// user:jyk
        /// time:2013/3/28 11:18
        public static void Remove(string key)
        {
            HttpContext.Current.Application.Lock();
            HttpContext.Current.Application.Remove(key);
            HttpContext.Current.Application.UnLock();

        }

        /// <summary>
        /// 获取一个票据
        /// </summary>
        /// <param name="key">Guid的key，转成string</param>
        /// user:jyk
        /// time:2013/3/28 11:18
        public static UserSsoToAppKey Get(string key)
        {
            if (HttpContext.Current.Application[key] == null)
                return null;

            //HttpContext.Current.Application.Lock();
            var userSsoInfo = (UserSsoToAppKey)HttpContext.Current.Application[key];
            //HttpContext.Current.Application.UnLock();

            return userSsoInfo;

        }
    }
    #endregion
  
}

