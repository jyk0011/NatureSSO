using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Caching;
using Nature.Common;
using Nature.Data;
using Nature.DebugWatch;

namespace Nature.Service.SSOAuth
{

    #region 用户在SSO端的信息
    /// <summary>
    /// 用户在SSO端的信息
    /// </summary>
    /// user:jyk
    /// time:2013/1/23 15:54
    public class UserSsoOnlineInfo
    {
        /// <summary>
        /// 用户在SSO端的用户ID
        /// </summary>
        /// user:jyk
        /// time:2013/1/23 15:51
        public int UserSsoID { get; set; }

        /// <summary>
        /// 用户的状态。1：正常访问；2：暂停访问；3：不能访问
        /// </summary>
        /// user:jyk
        /// time:2013/1/29 14:16
        public int State { set; get; }

        private string _onlineKey = ",";
        /// <summary>
        /// 每个人登录的各自的key的集合，字符串半角逗号分隔
        /// </summary>
        /// user:jyk
        /// time:2013/1/26 9:35
        public string UserOneselfInfoKeys
        {
            get { return _onlineKey; }
            set { _onlineKey = value; }
        }

        /// <summary>
        /// 同一个账户多少人登录，并且还在线的数量
        /// </summary>
        /// user:jyk
        /// time:2013/4/2 14:13
        public int CodeLoginCount
        {
            get { return UserOneselfInfoKeys.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length; }
        }

        /// <summary>
        /// 用户在各个网站应用端的用户ID
        /// key：WebappID
        /// value：用户在网站应用端的ID
        /// </summary>
        /// user:jyk
        /// time:2013/1/26 9:35
        public Dictionary<int, int> UserAppIDs { get; set; }

        /// <summary>
        /// 用户姓名
        /// </summary>
        /// user:jyk
        /// time:2013/5/22 11:55
        public string UserName { get; set; }

        /// <summary>
        /// 用户工号
        /// </summary>
        /// user:jyk
        /// time:2013/5/22 11:55
        public string JobNumber { get; set; }

    }
    #endregion


    /// <summary>
    /// 在线用户的管理
    /// </summary>
    /// user:jyk
    /// time:2013/4/2 9:45
    public static class ManageUserSsoOnlineInfo 
    {
        //cache的名称
        private const string CacheNameSSOUserOnline = "nfwUserSsoOnline";

        #region 创建一个在线用户
        /// <summary>
        /// 创建一个在线用户
        /// </summary>
        /// <param name="userIDsso"></param>
        /// <param name="debugInfoList"></param>
        /// <param name="dal"></param>
        /// <returns></returns>
        public static UserSsoOnlineInfo Create(int userIDsso, IList<NatureDebugInfo> debugInfoList, DataAccessLibrary dal)
        {
            var debugInfo = new NatureDebugInfo { Title = "[ManageUserSsoOnlineInfo.Create] 生成UserSsoOnlineInfo的实例  " };
          
            var userSsoOnline = new UserSsoOnlineInfo {UserSsoID = userIDsso};
            debugInfo.Stop();
            debugInfoList.Add(debugInfo);
      
            debugInfo = new NatureDebugInfo { Title = "从数据库里获取用户状态 userIDsso：" + userIDsso };

            #region 从数据库里获取状态
            string sql = "SELECT top 1 State FROM SSO_UserSSO WHERE (UserSSOID = {0})";
            userSsoOnline.State = dal.ExecuteScalar<int>(string.Format(sql, userIDsso));

            if (dal.ErrorMessage.Length > 2)
            {
                debugInfo.Remark = "获取用户状态时出现异常！";
                debugInfo.Stop();
                debugInfoList.Add(debugInfo);
                return null;
            }

            debugInfo.Remark = "到数据库获取用户状态：" + userSsoOnline.State + "。";
            debugInfo.Stop();
            debugInfoList.Add(debugInfo);
      
            #endregion

            debugInfo = new NatureDebugInfo { Title = "从数据库里获取网站用户的对应关系"};

            #region 从数据库里获取网站用户的对应关系
            sql = "SELECT WebAppID, UserAppID FROM SSO_UserSSOApp WHERE (UserSSOID = {0})";
            DataTable userMapper = dal.ExecuteFillDataTable(string.Format(sql, userIDsso));

            if (dal.ErrorMessage.Length > 2)
            {
                debugInfo.Remark = "从数据库里获取网站用户的对应关系时出现异常！";
                debugInfo.Stop();
                debugInfoList.Add(debugInfo);
                return null;
            }

            debugInfo.Remark = " 从数据库里获取网站用户的对应关系，对应数量：";
            debugInfo.Stop();
            debugInfoList.Add(debugInfo);

            #endregion

            #region 网站用户的对应关系放到Dictionary
            if (userMapper != null)
            {
                debugInfo.Remark += userMapper.Rows.Count + "。";

                debugInfo = new NatureDebugInfo { Title = "网站用户的对应关系放到Dictionary" };

                var tmpUserAppID = new Dictionary<int, int>();

                foreach (DataRow row in userMapper.Rows)
                {
                    //                              网站应用ID                      网站里的用户ID
                    tmpUserAppID.Add(Int32.Parse(row[0].ToString()), Int32.Parse(row[1].ToString()));
                }
                userSsoOnline.UserAppIDs = tmpUserAppID;

                debugInfo.Stop();
                debugInfoList.Add(debugInfo);

            }
            #endregion

            debugInfo = new NatureDebugInfo { Title = "从数据库加载工号和姓名" };

            #region 加载工号和姓名
            sql = "SELECT top 1 TrueName,UserCode FROM SSO_UserSSO WHERE (UserSSOID = {0})";
            string[] str = dal.ExecuteStringsBySingleRow(string.Format(sql, userIDsso));
            if (str != null)
            {
                userSsoOnline.UserName = str[0];
                userSsoOnline.JobNumber = str[1];
            }
            debugInfo.Stop();
            debugInfoList.Add(debugInfo);
            #endregion

            return userSsoOnline;
        }

        #endregion

        #region 添加一个在线用户。如果已经有了，则修改其内容
        /// <summary>
        /// 添加一个在线用户。如果已经有了，则修改其内容
        /// </summary>
        /// user:jyk
        /// time:2013/4/2 10:41
        public static UserSsoOnlineInfo Add(UserLoginInfo userOneself, IList<NatureDebugInfo> debugInfoList, DataAccessLibrary dal)
        {
            var debugInfo = new NatureDebugInfo { Title = "[ManageUserSsoOnlineInfo.Add] 添加一个在线用户。如果已经有了，则修改其内容" };
            //锁定，避免冲突，加锁
            //Locker.AcquireWriterLock(-1);
            //解锁
            //Locker.ReleaseWriterLock();

            int userIDsso = userOneself.UserSsoID;

            //查看cache里是否有userID对应的记录
            debugInfo.Remark = "查看cache里是否有userID对应的记录；";
            Dictionary<int, UserSsoOnlineInfo> dicUserOnlineDictionary;
            if (HttpContext.Current.Cache[CacheNameSSOUserOnline] == null)
            {
                debugInfo.Remark += "缓存里没有记录；";
                dicUserOnlineDictionary = new Dictionary<int, UserSsoOnlineInfo>();
            }
            else
            {
                debugInfo.Remark += "缓存里有记录；";
                dicUserOnlineDictionary = (Dictionary<int, UserSsoOnlineInfo>)HttpContext.Current.Cache[CacheNameSSOUserOnline];
            }
            //找到实例
            UserSsoOnlineInfo userSsoOnline ;

            bool hasUser = true;
            if (dicUserOnlineDictionary.ContainsKey(userIDsso))
            {
                //包含，已经登录过了
                debugInfo.Remark += "字典里包含，已经登录过了；";
                userSsoOnline = dicUserOnlineDictionary[userIDsso];
            }
            else
            {
                //没有，创建一个。
                debugInfo.Remark += "字典里没有，创建一个；";
                hasUser = false;
                userSsoOnline = Create(userIDsso, debugInfo.DetailList, dal);

            }

            //修改在线用户含有的登录key
            debugInfo.Remark += "修改在线用户含有的登录key；";
            userSsoOnline.UserOneselfInfoKeys += userOneself.GuidKey + ",";

            //修改字典
            if (hasUser)
            {
                debugInfo.Remark += "修改字典；";
                dicUserOnlineDictionary[userIDsso] = userSsoOnline;
            }
            else
            {
                debugInfo.Remark += "追加字典；";
                dicUserOnlineDictionary.Add(userIDsso, userSsoOnline);
            }
            //修改缓存
            debugInfo.Remark += "修改缓存信息";
            HttpContext.Current.Cache.Insert(CacheNameSSOUserOnline, dicUserOnlineDictionary, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration);

            debugInfo.Stop();
            debugInfoList.Add(debugInfo);
      
            return userSsoOnline;
        }
        #endregion

       
        #region 获取指定的用户的在线信息
        /// <summary>
        /// 获取指定的用户的在线信息
        /// </summary>
        /// user:jyk
        /// time:2013/4/2 10:41
        public static UserSsoOnlineInfo Get(int userIDsso )
        {
            UserSsoOnlineInfo userSso;

            Dictionary<int, UserSsoOnlineInfo> userOnlineDictionary;
            if (HttpContext.Current.Cache[CacheNameSSOUserOnline] == null)
                return null;
            else
                userOnlineDictionary = (Dictionary<int, UserSsoOnlineInfo>)HttpContext.Current.Cache[CacheNameSSOUserOnline];

            if (userOnlineDictionary.ContainsKey(userIDsso))
            {
                //包含
                userSso = userOnlineDictionary[userIDsso] ;
            }
            else
            {
                //不包含，说明该账户目前只有一个人登录
                return null; 
            }

            return userSso;

        }
        #endregion

        #region 移除在线用户信息
        /// <summary>
        /// 移除在线用户信息
        /// </summary>
        /// user:jyk
        /// time:2013/4/2 10:41
        public static void Remove(int userIDsso)
        {
            Dictionary<int, UserSsoOnlineInfo> userOnlineDictionary;
            if (HttpContext.Current.Cache[CacheNameSSOUserOnline] == null)
                return;
            else
                userOnlineDictionary = (Dictionary<int, UserSsoOnlineInfo>)HttpContext.Current.Cache[CacheNameSSOUserOnline];

            if (userOnlineDictionary.ContainsKey(userIDsso))
            {
                //包含，查看有多少人登录，有哪些人登录
                userOnlineDictionary.Remove(userIDsso);
                HttpContext.Current.Cache[CacheNameSSOUserOnline] = userOnlineDictionary;
            }
            else
            {
                //不包含，说明该账户目前只有一个人登录
                return;
            }

            if (1==0) ;

        }
        #endregion


        #region 验证用户名密码
        /// <summary>
        /// 验证用户名密码是否匹配
        /// </summary>
        /// <param name="userCode">登录账户</param>
        /// <param name="userPsw">密码</param>
        /// <param name="dal">数据访问函数库的实例集合</param>
        /// <param name="debugInfoList">写操作日志的实例</param>
        /// user:jyk
        /// time:2013/4/2 13:51
        public static string CheckUserCodePsw(string userCode, string userPsw, DalCollection dal, IList<NatureDebugInfo> debugInfoList)
        {
            if (debugInfoList == null)
                debugInfoList = new List<NatureDebugInfo>();

            var debugInfo = new NatureDebugInfo { Title = "[CheckUserCodePsw] 验证用户名密码是否匹配" };
          
            userCode = userCode.Replace("'", "''");
            userPsw = Functions.ToMD5(userPsw);

            const string sql = "SELECT TOP 1 UserSSOID from SSO_UserSSO where UserLoginName='{0}' and LoginPsw ='{1}'";

            string userID = dal.DalUser.ExecuteString(string.Format(sql, userCode, userPsw));
            if (dal.DalUser.ErrorMessage.Length > 2)
            {
                debugInfo.Remark = "到数据库验证登录账户和密码，出现异常！";
            }
            debugInfo.Stop();
            debugInfoList.Add(debugInfo);
      
            return userID;

        }
        #endregion
    }
}