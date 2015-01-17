using System;
using System.Collections.Generic;
using System.Web;
using Nature.Client.SSOApp;
using Nature.Common;
using Nature.Data;
using Nature.SsoConfig;

namespace Nature.Service.UserCenter
{
    /// <summary>
    /// 用户中心服务器端的帮助类库
    /// </summary>
    /// user:jyk
    /// time:2013/3/5 13:42
    public static class UserCenterHelp
    {
        #region 向服务发送用户信息，保存用户信息

        /// <summary>
        /// 向服务发送用户信息，保存用户信息
        /// </summary>
        /// <param name="postUserInfo">The post user info.</param>
        /// <param name="param">The param.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="dal">The dal.</param>
        /// <param name="operatorID">操作人ID </param>
        /// <returns></returns>
        /// user:jyk
        /// time:2013/3/8 15:17
        public static string SaveUserSsoInfoByService(Dictionary<string, string> postUserInfo, URLParam param, string batch,DataAccessLibrary dal,string operatorID)
        {
            //访问其他网站的接口，返回该网站的用户ID
            //post参数
            var postVar = postUserInfo;

            string url = SsoInfo.SSOUrl + "/Data/PostData.ashx" + param.ToURLParam(true);

            string errorMsg;
            string re = MyWebClient.Post(url, postVar, out errorMsg);

            //转换为字典
            Dictionary<string, string> tmpUserInfo = Json.JsonToDictionary(re);

            string key = "";

            key = "err";
            if (tmpUserInfo.ContainsKey(key))
            {
                if (tmpUserInfo[key].Length > 0)
                {
                    //有异常
                    WriteLog("向服务发送用户信息时出现异常", 3, batch, tmpUserInfo[key], operatorID, dal);
                    return tmpUserInfo[key];
                }
            }
 
            return "";
        }
        #endregion

        #region 通过userIDapp 和appID获取userIDsso 
        /// <summary>
        /// 通过userIDapp 和appID获取userIDsso 
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="webAppID">网站应用ID</param>
        /// <param name="userIDapp">网站应用端的用户ID</param>
        /// <param name="dal">数据访问实例</param>
        /// <returns></returns>
        /// user:jyk
        /// time:2013/3/8 15:33
        public static string GetUserIDssoByUserIDappAppID(HttpContext context,string webAppID, string userIDapp, DataAccessLibrary dal)
        {
            string userIDsso = "";

            string sql = "";

            sql = "SELECT  TOP 1 UserSSOID FROM SSO_UserSSOApp WHERE (WebAppID = {0}) AND (UserAppID = {1})";
            userIDsso = dal.ExecuteString(string.Format(sql, webAppID, userIDapp));

            if (string.IsNullOrEmpty(userIDsso))
            {
                //没有这个用户
                context.Response.Write(string.Format("{{\"msg\":\"没有这个用户\"}}"));
                return null;
            }

            return userIDsso;
        }
        #endregion

        #region 分发注册信息

        /// <summary>
        /// 分发注册信息
        /// </summary>
        /// <param name="webappID">网站应用ID</param>
        /// <param name="userCode">用户名</param>
        /// <param name="userLoginName">登录账户</param>
        /// <param name="ssoUserID">用户中心端的用户ID </param>
        /// <param name="misUserID">开通网站端的用户ID </param>
        /// <param name="appUrl">网站应用的网址</param>
        /// <param name="batch">批号</param>
        /// <param name="dal">数据库访问实例</param>
        /// <returns></returns>
        /// user:jyk
        /// time:2013/3/5 15:17
        public static string SendRegUserInfo(string webappID, string userCode, string userLoginName,string ssoUserID,string misUserID,string appUrl,string batch,DataAccessLibrary dal,string operatorID)
        {
            //访问其他网站的接口，返回该网站的用户ID
            //post参数
            var postVar = new Dictionary<string, string>
                                      {
                                          {"ssoUserID", ssoUserID},
                                          {"misUserID", misUserID},
                                          {"userCode", userCode},
                                          {"userLoginName", userLoginName}
                                      };

            var param = new URLParam(null)
            {
                WebAppID = webappID,
                ModuleID = "-9"
            };

            string url  = "http://" + appUrl + "/UserCenter/RevRegUser.ashx" + param.ToURLParam(true);
           
            string errorMsg;
            string re = MyWebClient.Post(url, postVar, out errorMsg);

            //转换为字典
            Dictionary<string, string> tmpUserInfo = Json.JsonToDictionary(re);

            string key = "";

            key = "msg";
            if (tmpUserInfo.ContainsKey(key))
            {
                if (tmpUserInfo[key].Length > 0)
                {
                    //有异常
                    WriteLog("分发注册用户出现异常（反馈）", 2, batch, tmpUserInfo[key],operatorID , dal);
                    return "";
                }
            }

            string newUserAppID = "";

            key = "newID";
            if (tmpUserInfo.ContainsKey(key))
            {
                newUserAppID = tmpUserInfo[key];

                if (!Functions.IsInt(newUserAppID))
                {
                    //返回的用户ID不是int的
                    WriteLog("分发注册用户，返回的不是int", 2, batch, newUserAppID, operatorID, dal);
                    return "";
                }
            }
            return newUserAppID;
        }
        #endregion

        #region 记录操作日志

        /// <summary>
        /// 记录操作日志
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="kind">1：开通账户；2：注册用户</param>
        /// <param name="batch">批号</param>
        /// <param name="content">日志内容</param>
        /// <param name="userID">操作人ID</param>
        /// <param name="dal"> </param>
        /// <returns></returns>
        /// user:jyk
        /// time:2013/3/5 13:50
        public static string WriteLog(string title,int kind,string batch,string content,string userID,DataAccessLibrary dal)
        {
            string sql = @"INSERT INTO SSO_Log ([Title],[KindID],[Batch],[Content],[AddUserid]) VALUES
                                                 ('{0}',  {1} ,   '{2}' ,  '{3}',   {4}) ";

            batch = batch.Replace("'", "");
            content = content.Replace("'", "''");

            dal.ExecuteNonQuery(string.Format(sql, title, kind, batch, content, userID));


            return "";
        }
        #endregion
    }
}