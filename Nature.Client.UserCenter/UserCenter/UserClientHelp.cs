using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using Nature.Client.SSOApp;
using Nature.Common;
using Nature.Service;
using Nature.Service.UserCenter.Model;
using Nature.SsoConfig;

namespace Nature.Client.UserCenter
{
    /// <summary>
    /// 用户中心客户端(网站应用)的帮助类
    /// </summary>
    /// user:jyk
    /// time:2013/3/2 16:02
    public class UserClientHelp
    {
        #region 提交注册信息
        /// <summary>
        /// 向用户中心发送用户的注册信息，用户ID、用户名、登录账户、网站ID，返回登录密码和二次鉴权密码
        /// </summary>
        /// <param name="userReg">用户注册信息</param>
        /// <param name="userID">当前登录人ID</param>
        /// <returns>操作是否成功。""：成功；其他：不成功原因</returns>
        /// user:jyk
        /// time:2013/3/2 16:04
        public static string SendRegUser(UserRegInfo userReg, string userID)
        {
            //设置form值
            var postVar = new Dictionary<string, string>
                              {
                                  {"userIDapp", userReg.UserAppID.ToString(CultureInfo.InvariantCulture)},
                                  {"userCode", userReg.UserCode},
                                  {"userLoginName", userReg.UserLoginName}
                              };


            string re = MySend("register", userID, postVar,null);
              
            //转换为字典
            Dictionary<string, string> tmpUserInfo = Json.JsonToDictionary(re);

            string key = "";

            key = "msg";
            if (tmpUserInfo.ContainsKey(key))
            {
                if (tmpUserInfo[key].Length > 0)
                {
                    //出现异常
                    return tmpUserInfo[key];
                }
            }

            key = "newID";
            if (tmpUserInfo.ContainsKey(key)) userReg.UserSSOID = Int32.Parse(tmpUserInfo[key]);

            key = "loginPsw";
            if (tmpUserInfo.ContainsKey(key)) userReg.LoginPsw = tmpUserInfo[key];

            key = "secondAuthPsw";
            if (tmpUserInfo.ContainsKey(key)) userReg.SecondAuthPsw = tmpUserInfo[key];

            return "";

        }

        #endregion

        #region 验证登录账户是否重复
        /// <summary>
        /// 验证登录账户是否重复
        /// </summary>
        /// <param name="userLoginName">要检查版本的（本地）用户ID</param>
        /// <param name="userID">当前登录人ID</param>
        /// <returns></returns>
        public static bool CheckLoginName(string userLoginName, string userID)
        {
            //设置form值
            var postVar = new Dictionary<string, string>
                              {
                                  {"userLoginName", userLoginName}
                              };

            string re = MySend("hasloginname", userID, postVar, null);

            //转换为字典
            Dictionary<string, string> tmpUserVer = Json.JsonToDictionary(re);

            string key = "";

            key = "msg";
            if (tmpUserVer.ContainsKey(key))
            {
                return tmpUserVer[key] == "1";
            }

            return false;
             

        }
        #endregion

        #region 获取用户最新版本
        /// <summary>
        /// 获取用户中心里指定的用户的最新版本
        /// </summary>
        /// <param name="userIDapp">要检查版本的（本地）用户ID</param>
        /// <param name="userID">当前登录人ID</param>
        /// <returns></returns>
        public static string GetUserNewVer(string userIDapp, string userID)
        {
            //设置form值
            var postVar = new Dictionary<string, string>
                              {
                                  {"userIDapp", userIDapp}
                              };

            string re = MySend("userNewVer", userID, postVar, null);

            //转换为字典
            Dictionary<string, string> tmpUserVer = Json.JsonToDictionary(re);

            string key = "";

            key = "msg";
            if (tmpUserVer.ContainsKey(key))
            {
                if (tmpUserVer[key].Length > 0)
                {
                    //出现异常
                    return tmpUserVer[key];
                }
            }

            key = "ver";
            if (tmpUserVer.ContainsKey(key))
            {
                return tmpUserVer[key];
            }
            else
            {
                return "";
            }
             

        }
        #endregion

        #region 获取用户最新信息

        /// <summary>
        /// 获取用户中心里指定的用户的最新信息
        /// </summary>
        /// <param name="userIDapp">要获取信息的（本地）用户ID</param>
        /// <param name="userID">当前登录人ID</param>
        /// <param name="userSsoInfo">用户中心的用户信息 </param>
        /// <returns></returns>
        public static string GetUserNewInfo(string userIDapp, string userID,ref UserSsoInfo userSsoInfo )
        {
            //设置form值
            var postVar = new Dictionary<string, string>{{"userIDapp", userIDapp}};

            //获取用户在用户中心的ID
            string re = MySend("GetUserIDsso", userID, postVar,null);
              
            //转换为字典
            Dictionary<string, string> tmpUserVer = Json.JsonToDictionary(re);

            string key = "";

            key = "msg";
            if (tmpUserVer.ContainsKey(key))
            {
                if (tmpUserVer[key].Length > 0)
                {
                    //出现异常
                    return tmpUserVer[key];
                }
            }

            string userIDsso = "";
            key = "userIDsso";
            if (tmpUserVer.ContainsKey(key))
            {
                userIDsso = tmpUserVer[key];
            }
            else
            {
                return "没有找到用户在用户中心的ID";
            }

            URLParam param = new URLParam(null)
                                 {
                                     ModuleID = "103",
                                     MasterPageViewID = "10304",
                                     DataBaseID = "2",
                                     Action = "one",
                                     DataID = userIDsso
                                 };

            string jsonUser = MyGetData(userID, null, param);

            //检查返回值
            tmpUserVer = Json.JsonToDictionary(jsonUser);
 
            key = "err";
            if (tmpUserVer.ContainsKey(key))
            {
                if (tmpUserVer[key].Length > 0)
                {
                    //出现异常
                    return tmpUserVer[key];
                }
            }

            MyWebClient.JsonToEntity(userSsoInfo, jsonUser);

            return "";
        }
        #endregion

        #region 向用户中心提交用户信息
        /// <summary>
        /// 向用户中心提交用户信息
        /// </summary>
        /// <param name="userInfo">要修改的用户的信息</param>
        /// <param name="operatorID">登录人ID</param>
        /// <param name="userIDapp">要修改的用户在本地的ID</param>
        /// <returns></returns>
        /// user:jyk
        /// time:2013/3/8 10:18
        public static string SendUserNewInfo(UserInfoSendtoSSO userInfo, string userIDapp, string operatorID)
        {
            //设置form值 PostUserInfo
            Dictionary<string, string> postVar = MyWebClient.EntityToDictionary(userInfo, "c");// new Dictionary<string, string>();


            var param = new URLParam(null)
            {
                ModuleID = "103",
                MasterPageViewID = "10306",
                DataBaseID = "2",
                DataID = userIDapp,
                UserSsoID = operatorID 
            };

            string re = MySend("PostUserInfo", operatorID, postVar, param);

            //转换为字典
            Dictionary<string, string> tmpUserInfo = Json.JsonToDictionary(re);

            string key = "";

            key = "msg";
            if (tmpUserInfo.ContainsKey(key))
            {
                if (tmpUserInfo[key].Length > 0)
                {
                    //出现异常
                    return tmpUserInfo[key];
                }
            }

            

            return "";
        }
        #endregion

        #region 修改密码

        /// <summary>
        /// 修改用户的登录密码
        /// </summary>
        /// <param name="serIDapp">用户在本地的ID </param>
        /// <param name="pswOld">原来的登录密码</param>
        /// <param name="pswNew">新的登录密码</param>
        /// <param name="userID">当前登录人ID</param>
        /// <returns></returns>
        public static string UpdatePsw(string serIDapp,string pswOld,string pswNew, string userID)
        {
            return UpdatePsw(serIDapp, pswOld, pswNew, userID, "updatepsw");

        }

        /// <summary>
        /// 修改用户的入口密码
        /// </summary>
        /// <param name="serIDapp">用户在本地的ID </param>
        /// <param name="pswOld">原来的登录密码</param>
        /// <param name="pswNew">新的登录密码</param>
        /// <param name="userID">当前登录人ID</param>
        /// <returns></returns>
        public static string UpdateAuthPsw(string serIDapp, string pswOld, string pswNew, string userID)
        {
            return UpdatePsw(serIDapp, pswOld, pswNew, userID, "updateauthpsw");

        }

        private static string UpdatePsw(string serIDapp, string pswOld, string pswNew, string userID, string action)
        {
            //设置form值
            var postVar = new Dictionary<string, string>
                              {
                                  {"userIDapp", serIDapp},
                                  {"pswOld", pswOld},
                                  {"pswNew", pswNew}
                              };

            string re = MySend(action, userID, postVar,null);

            //转换为字典
            Dictionary<string, string> tmpUserVer = Json.JsonToDictionary(re);

            string key = "";

            key = "msg";
            if (tmpUserVer.ContainsKey(key))
            {
                if (tmpUserVer[key].Length > 0)
                {
                    //出现异常
                    return tmpUserVer[key];
                }
            }

            return "";
        }
        #endregion


        #region 修改用户状态

        /// <summary>
        /// 修改用户状态
        /// </summary>
        /// <param name="serIDapp">用户在本地的ID </param>
        /// <param name="state">原来的登录密码</param>
        /// <param name="userID">当前登录人ID</param>
        /// <returns></returns>
        public static string UpdateUserState(string serIDapp, string state, string userID)
        {
          
            //设置form值
            var postVar = new Dictionary<string, string>
                              {
                                  {"userIDapp", serIDapp},
                                  {"state", state},
                              };

            string re = MySend("UpdateUserState", userID, postVar, null);

            //转换为字典
            Dictionary<string, string> tmpUserVer = Json.JsonToDictionary(re);

            string key = "";

            key = "msg";
            if (tmpUserVer.ContainsKey(key))
            {
                if (tmpUserVer[key].Length > 0)
                {
                    //出现异常
                    return tmpUserVer[key];
                }
            }

            return "";
        }
        #endregion


        //================================
        #region 内部的发送信息的封装函数

        /// <summary>
        /// 内部的发送信息的封装函数
        /// </summary>
        /// <param name="action">请求的动作</param>
        /// <param name="operatorID">登录人ID</param>
        /// <param name="postVar">提交的form信息</param>
        /// <param name="param">url里的参数 </param>
        /// <returns></returns>
        public static string MySend(string action, string operatorID, Dictionary<string, string> postVar, URLParam param)
        {
            //设置URL参数
            if (param == null)
                param = new URLParam(null)
                {
                    //这里用不到模块ID，但是服务端要验证模块ID，所以填写一个。
                    ModuleID = "-9",
                    //web.config 里面获取网站应用的ID
                    WebAppID = SsoInfo.WebAppID,
                    //登录人ID
                    UserSsoID = operatorID,
                    Action = action  
                };
            else
            {
                param.WebAppID = SsoInfo.WebAppID;
                param.UserSsoID = operatorID;
                param.Action = action;

            }

            //设置网址
            string url = SsoInfo.ResourceURL + "/UserCenter/UserService.ashx" + param.ToURLParam(true);
            string errorMsg;
            //提交给用户中心
            string re = MyWebClient.Post(url, postVar, out errorMsg);
           
            if (re == null)
            {
                return "远程服务器出现异常！";
            }

            if (re.Length == 0)
            {
                return "没有返回信息";
            }

            return re;
        }
        #endregion

        #region 内部的发送信息的封装函数

        /// <summary>
        /// 内部的发送信息的封装函数
        /// </summary>
        /// <param name="userID">登录人ID</param>
        /// <param name="postVar">提交的form信息</param>
        /// <param name="param">url的参数 </param>
        /// <returns></returns>
        public static string MyGetData(string userID, Dictionary<string, string> postVar, URLParam param)
        {
            //设置URL参数
            //"&mdid=103&mpvid=10304&bid=10301&dbid=2&id=15

            //web.config 里面获取网站应用的ID
            param.WebAppID = SsoInfo.WebAppID;
            //登录人ID
            param.UserSsoID = userID;

            //设置网址
            string url = string.Format("{0}/data/GetData.ashx{1}", SsoInfo.ResourceURL, param.ToURLParam(true));
            string errorMsg;
            //提交给用户中心
            string re = MyWebClient.Post(url, postVar, out errorMsg);

            if (re == null)
            {
                return "远程服务器出现异常！";
            }

            if (re.Length == 0)
            {
                return "没有返回信息";
            }

            return re;
        }

        #endregion
    }
}