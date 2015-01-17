using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using Nature.Common;
using Nature.Data;
using Nature.DebugWatch;

namespace Nature.Service.SSOAuth
{
    /// <summary>
    /// SSO的一个管理类，缓存票据，创建票据，获取票据等
    /// </summary>
    /// user:jyk
    /// time:2013/1/21 14:31
    public class SsoManage
    {
        //static readonly ReaderWriterLock Locker = new ReaderWriterLock();

        #region 获取用户的超时时间
        /// <summary>
        /// 获取用户的超时时间
        /// </summary>
        /// user:jyk
        /// time:2013/1/23 16:44
        public static int UserTimeOut
        {
            get
            {
                string tmp = ConfigurationManager.AppSettings["SSOUserTimeOut"];
                return tmp == null ? 30 : int.Parse(tmp);
            }
            
        }
        #endregion
         

        #region 创建用户验证信息

        /// <summary>
        /// 创建网站应用与sso交换用的认证信息
        /// </summary>
        /// <returns></returns>
        /// <param name="webAppID">网站应用的ID</param>
        /// <param name="debugInfoList">写日志 </param>
        /// <returns></returns>
        /// user:jyk
        /// time:2013/1/21 14:33
        public static UserSsoToAppKey CreateUserSsoWebappVerifyInfo(int webAppID, IList<NatureDebugInfo> debugInfoList)
        {
            var debugInfo = new NatureDebugInfo { Title = "[CreateUserSsoWebappVerifyInfo] 创建网站应用与sso交换用的认证信息" };
            
            #region 生成数据
            UserLoginInfo userOneself = ManageUserLoginInfo.GetUserOneselfInfoByCookie(debugInfo.DetailList);

            if (userOneself == null)
            {
                return null;
            }

            UserSsoOnlineInfo userSsoInfo = ManageUserSsoOnlineInfo.Get(userOneself.UserSsoID);
            
            Guid dicKey = Guid.NewGuid();           //生成guid用于标识当前访问用户
            string key =  Guid.NewGuid().ToString();//生成加密密钥
            int userAppID = 0;
            string msg = "1";
            if (userSsoInfo.UserAppIDs.ContainsKey(webAppID))
            {
                userAppID = userSsoInfo.UserAppIDs[webAppID];
            }
            else
            {
                //该用户不能访问指定的网站
                msg = "没有开通这个网站";
                debugInfo.Remark = "没有开通这个网站:" + webAppID;
            }

            //沟通标识  
            string source = string.Format("msg:\"{0}\",webAppID:\"{1}\",userIDsso:\"{2}\",userIDapp:\"{3}\",userIP:\"{4}\",dateTime:\"{5}\",GuidKey:\"{6}\""
                                            , msg, webAppID, userSsoInfo.UserSsoID, userAppID, userOneself.UserIP, DateTime.Now, userOneself.GuidKey);

            //string miwen = DesBase64.Encrypt(source, key);
            string miwen = DesUrl.Encrypt(source, key);

            var userSsoVerify = new UserSsoToAppKey
                                    {
                                        UserGuid = dicKey,
                                        CreateTime = DateTime.Now,
                                        Key = key,
                                        Ticket = miwen
                                    };
            #endregion
             
            //从缓存里获取验证信息的集合列表
            ManageSSOToAppKey.Add(dicKey.ToString(), userSsoVerify);
            debugInfo.Stop();
            debugInfoList.Add(debugInfo);
      
            return userSsoVerify;
        }
        #endregion

        #region 创建用户票据和凭证

        /// <summary>
        /// 创建SSO端用户凭证，在线用户
        /// </summary>
        /// <returns></returns>
        /// <param name="webAppId">网站应用的ID</param>
        /// <param name="userId">当前访问用户的ID</param>
        /// <param name="dal">访问数据库的实例</param>
        /// <param name="debugInfoList">写日志的实例 </param>
        /// <returns></returns>
        /// user:jyk
        /// time:2013/1/21 14:33
        public static UserSsoOnlineInfo CreateUserSsoInfo(string webAppId, int userId, DataAccessLibrary dal, IList<NatureDebugInfo> debugInfoList)
        {
            var debugInfo = new NatureDebugInfo { Title = "[CreateUserSsoInfo] 创建SSO端用户凭证，在线用户" };

            //创建个人标识，并且加到缓存
            UserLoginInfo userOneself = ManageUserLoginInfo.Create(webAppId, userId, debugInfo.DetailList);
             
            //添加在线用户的缓存
            UserSsoOnlineInfo userSsoInfo = ManageUserSsoOnlineInfo.Add(userOneself, debugInfo.DetailList, dal);

            debugInfo.Remark = "记录列表：UserSSOKeyDictionary、UserOnlineDictionary。";
            debugInfo.Stop();
            debugInfoList.Add(debugInfo);
      
            return userSsoInfo;
        }

        #endregion
         
    
        #region 询问是否可以继续访问
        /// <summary>
        /// 询问是否可以继续访问
        /// </summary>
        /// user:jyk
        /// time:2013/2/16 9:06
        public static string CanContinueAccess(string userIDsso, IList<NatureDebugInfo> debugInfoList)
        {
            var debugInfo = new NatureDebugInfo { Title = "[CanContinueAccess] 询问是否可以继续访问" };
            string msg = "";

            if(!Functions.IsInt(userIDsso) )
            {
                //超时了//去掉标记
                msg = "5";
                return msg;
            }

            int userIDint = int.Parse(userIDsso);

            string guidKey = HttpContext.Current.Request.QueryString["key"];
            if (!Functions.IsGuid(guidKey))
            {
                //没有传递 标记
                debugInfo.Remark = "key参数不正确：" + guidKey;
                return "6";
            }

            UserSsoOnlineInfo userSsoOnline = ManageUserSsoOnlineInfo.Get(userIDint);

            var key = new Guid(guidKey);

            UserLoginInfo userOneself = ManageUserLoginInfo.GetUserOneselfInfoByKey(key, debugInfo.DetailList);

            if (userOneself != null)
            {
                //有，登录了
                
                debugInfo.Remark = "【" + userSsoOnline.UserSsoID + "】已经登录。";
                
                //查看最后访问时间，判断是否超时
                if (userOneself.LastTime.AddMinutes(+UserTimeOut) < DateTime.Now)
                {
                    debugInfo.Remark = "【" + userOneself.UserSsoID + "】超时，最后访问：" + userOneself.LastTime.ToString("yyyy-MM-dd HH:mm:ss") + "。";
                    
                    //超时了//去掉标记
                    msg = "5";
                    ManageUserLoginInfo.Remove(debugInfo.DetailList);
                    debugInfo.Remark = "【" + userOneself.UserSsoID + "】移除UserSSOKeyDictionary，UserOnlineDictionary，清掉cookie。";
                
                }
                else
                {
                    debugInfo.Remark = "【" + userOneself.UserSsoID + "】没超时，判断状态。";

                    msg = "1";

                    //记录最后访问时间
                    userOneself.LastTime = DateTime.Now;
                }

            }
            else
            {
                debugInfo.Remark = "超时，在线列表中没有。";
                //没有，超时了。
                msg = "5";
              
            }
            debugInfo.Stop();
            debugInfoList.Add(debugInfo);
      
            return msg;
        }
        #endregion

        #region 清除过期的sso端的票据
        /// <summary>
        /// 清除过期的sso端的票据
        /// </summary>
        /// user:jyk
        /// time:2013/2/16 9:06
        public static void ClearSsoTicket()
        {
            //锁定，避免冲突，加锁
            //Locker.AcquireWriterLock(-1);

            //Dictionary<Guid, UserSsoInfo> userSsoInfos = UserSSOKeyDictionary;
            //List<Guid> tmpKey = new List<Guid>();

            //foreach (KeyValuePair<Guid, UserSsoInfo> dict in userSsoInfos)
            //{
            //    UserSsoInfo use = dict.Value;
            //    if (use.LastTime < DateTime.Now.AddDays(-1))
            //    {
            //        tmpKey.Add(dict.Key);
            //    }
            //}

            //去掉已经过期的票据，默认保存一天
            //foreach (Guid g in tmpKey)
            //{
            //    userSsoInfos.Remove(g);
            //}

            //UserSSOKeyDictionary = userSsoInfos;

            //解锁
            //Locker.ReleaseWriterLock();
        }
        #endregion

    }
}