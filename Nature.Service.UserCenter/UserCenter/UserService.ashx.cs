using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using Nature.Client.SSOApp;
using Nature.Common;
using Nature.Data.Part;
using Nature.DebugWatch;
using Nature.MetaData.Entity;
using Nature.Service.Ashx;
using Nature.User;

namespace Nature.Service.UserCenter
{
    /// <summary>
    /// 用户的相关服务
    /// 1、接收用户注册信息。 Register 
    /// 2、分发新注册的用户信息（基本信息） /开通账户
    /// 3、提供用户信息的最新版本 userNewVersion
    /// 4、提供用户的全部信息（限用户中心里的）
    /// 5、验证是否已经有指定的登录账户
    /// 6、修改登录密码
    /// 7、修改二次鉴权密码
    /// </summary>
    public class UserService : BaseAshxCrud
    {

        public override void ProcessRequest(HttpContext context)
        {
            base.ProcessRequest(context);
               
            var action = context.Request["action"]; //标识

            switch (action.ToLower())
            {
                case "register": //1 新注册用户
                    Register(context);
                    break;
                case "regapp": //2 开通账户
                    RegApp(context);
                    break;
                case "usernewver": //3 提供用户信息的最新版本
                    GetUserNewVersion(context);
                    break;
                case "usernewinfo": // 4 提供用户的全部信息
                    GetUserNewInfo(context);
                    break;
                case "hasloginname": // 5 验证数据库里是否有这个登录账户
                    HasLoginName(context);
                    break;
                case "updatepsw": // 6 修改登录密码
                    Updatepsw(context, "LoginPsw");
                    break;
                case "updateauthpsw": // 7 修改二次鉴权密码
                    Updatepsw(context, "SecondAuthPsw");
                    break;
                case "getuseridsso": // 8 获取用户在中心的ID
                    GetUserIDsso(context);
                    break;

                case "postuserinfo": // 9 接收app端传递过来的用户信息，保存到数据库，保存前后做备份
                    PostUserInfo(context);
                    break;
       
                case "updateuserstate": // 10 修改用户状态
                    UpdateUserState(context);
                    break;
            }

        }

        #region 1 接收注册信息，放到用户中心数据库，然后分发给其他网站
        /// <summary>
        /// 接收注册信息，放到用户中心数据库，然后分发给其他网站
        /// </summary>
        private void Register(HttpContext context)
        {
            #region 定义和接收
            string userIDapp = ""; //用户在应用端的ID
            string userCode = ""; //用户名
            string userLoginName = ""; //登录账户
            string loginPsw = "";      //登录密码，如果没有发送，则生成随机密码，八位数字
            string secondAuthPsw = ""; //鉴权密码、二次操作密码，如果没有发送，则生成随机密码，八位数字

            userIDapp = context.Request["userIDapp"];
            userCode = context.Request["userCode"];
            userLoginName = context.Request["userLoginName"];
            loginPsw = context.Request["loginPsw"];
            secondAuthPsw = context.Request["secondAuthPsw"];
            #endregion

            #region 验证
            if (!Functions.IsInt(userIDapp))
            {
                ResponseWriteError("用户ID必须是数字！");
                return;
            }
            if (string.IsNullOrEmpty(userCode))
            {
                ResponseWriteError( "用户名不能为空！");
                return;
            }
            if (string.IsNullOrEmpty(userLoginName))
            {
                ResponseWriteError("登录账户不能为空！");
                return;
            }
            if (string.IsNullOrEmpty(loginPsw))
            {
                loginPsw = Functions.RndNumNotRepeat(8);
            }
            if (string.IsNullOrEmpty(secondAuthPsw))
            {
                secondAuthPsw = Functions.RndNumNotRepeat(8);
            }
            #endregion

            userCode = userCode.Replace("'", "''");
            userLoginName = userLoginName.Replace("'", "''");

            string sql = "";

            #region 检查是否重名

            sql = "SELECT TOP 1 1 FROM  SSO_UserSSO WHERE (UserLoginName = N'{0}') ";
            if (Dal.DalCustomer.ExecuteExists(string.Format(sql,userLoginName)))
            {
                //有记录，登录账户重复
                ResponseWriteError( "已经有这个登录账户了，请更换！");
                return;
            }

            sql = "SELECT TOP 1 1  FROM  SSO_UserSSO WHERE (UserCode = N'{0}') ";
            if (Dal.DalCustomer.ExecuteExists(string.Format(sql, userCode)))
            {
                //有记录，用户名重复
                ResponseWriteError( "已经有这个用户名了，请更换！");
                return;
            }
            #endregion

            string rndPsw = loginPsw + "," + secondAuthPsw;

            //加密密码
            loginPsw = Functions.ToMD5(loginPsw);
            secondAuthPsw = Functions.ToMD5(secondAuthPsw);

            #region 保存到数据库

            Dal.DalCustomer.ManagerTran.TranBegin();

            sql = "INSERT INTO SSO_UserSSO (UserCode,UserLoginName,LoginPsw,SecondAuthPsw,RandomPsw) VALUES ('{0}','{1}','{2}','{3}','{4}')";
            Dal.DalCustomer.ExecuteNonQuery(string.Format(sql,userCode,userLoginName ,loginPsw ,secondAuthPsw ,rndPsw));
            if (Dal.DalCustomer.ErrorMessage .Length >2)
            {
                //有错误
                ResponseWriteError( "保存数据的时候，发生异常！");
                return;
            }

            //获取新的用户ID
            sql = "SELECT TOP 1 UserSSOID FROM SSO_UserSSO where UserLoginName='{0}' order by UserSSOID desc";
            string newUserSsoID = Dal.DalCustomer.ExecuteString(string.Format(sql,userLoginName));

            //保存到对应关系
            const string sqlUserSSOAppInsert = "INSERT INTO SSO_UserSSOApp (WebAppID,UserSSOID,UserAppID) VALUES ({0},{1},{2})";
            Dal.DalCustomer.ExecuteNonQuery(string.Format(sqlUserSSOAppInsert, WebAppID, newUserSsoID,userIDapp));
            if (Dal.DalCustomer.ErrorMessage.Length > 2)
            {
                //有错误
                ResponseWriteError( "保存数据的时候，发生异常！");
                return;
            }

            Dal.DalCustomer.ManagerTran.TranCommit();

            #endregion

            //写操作日志
            string batch = Guid.NewGuid().ToString().Substring(0,8);
            string content = string.Format("网站应用ID：{0}；网站用户ID：{1}；中心用户ID：{2}；用户名：{3}；登录账户：{4}。", WebAppID, userIDapp, newUserSsoID, userCode, userLoginName);
            UserCenterHelp.WriteLog("注册用户：" + newUserSsoID +"/" +userLoginName, 2, batch, content, MyUser.BaseUser.UserID, Dal.DalCustomer);


            #region 写对应关系
            //获取要开通的网站应用
            string[] appIDarr ;
            string appIDs = "";
            sql = "SELECT TOP  1 WebAppIDs FROM SSO_Webapp WHERE (WebAppID = {0})";
            appIDs = Dal.DalCustomer.ExecuteString(string.Format(sql, WebAppID));

            if (appIDs != null)
            {
                //获取网站地址
                var appUrl = new Dictionary<string, string>();

                #region 获取网站应用信息

                sql = "SELECT   WebAppID, AppURL FROM  SSO_Webapp WHERE  (WebAppID IN ({0}))";
                DataTable dt = Dal.DalCustomer.ExecuteFillDataTable(string.Format(sql, appIDs));
                foreach (DataRow dr in dt.Rows)
                {
                    appUrl.Add(dr[0].ToString(), dr[1].ToString());
                }
                content = string.Format("网站应用列表：{0}。", appIDs);
                UserCenterHelp.WriteLog("获取分发网站应用的列表" , 2, batch, content, MyUser.BaseUser.UserID, Dal.DalCustomer);

                #endregion

                appIDarr = appIDs.Split(',');

                foreach (string tmpWebappID in appIDarr)
                {
                    #region 分发到其他网站应用
                    //记录日志
                    content = string.Format("分发至网站应用ID：{0}；中心用户ID：{1}。", tmpWebappID, newUserSsoID);
                    UserCenterHelp.WriteLog("分发注册用户：" + tmpWebappID + "/" + newUserSsoID, 2, batch, content, MyUser.BaseUser.UserID, Dal.DalCustomer);

                    string re = UserCenterHelp.SendRegUserInfo(tmpWebappID, userCode, userLoginName, newUserSsoID, userIDapp, appUrl[tmpWebappID], batch, Dal.DalCustomer, MyUser.BaseUser.UserID);

                    if (re.Length >0)
                    {
                        string newUserAppID = re;
                        //其他网站注册成功
                        content = string.Format("网站应用ID：{0}；网站用户ID：{1}。", tmpWebappID, newUserAppID);
                        UserCenterHelp.WriteLog("其他网站注册成功：" + tmpWebappID + "/" + newUserAppID, 2, batch, content,
                                                MyUser.BaseUser.UserID, Dal.DalCustomer);

                        //SSO_UserSSOApp 写对应关系
                        Dal.DalCustomer.ExecuteNonQuery(string.Format(sqlUserSSOAppInsert, tmpWebappID, newUserSsoID, newUserAppID));
                        if (Dal.DalCustomer.ErrorMessage.Length > 2)
                        {
                            //有错误，
                            ResponseWriteError( "保存数据的时候，发生异常！");

                            content = string.Format("网站应用ID：{0}；网站用户ID：{1}；中心用户ID：{2}。", tmpWebappID, newUserAppID,
                                                    newUserSsoID);
                            UserCenterHelp.WriteLog("保存对应关系失败：" + tmpWebappID + "/" + newUserAppID, 2, batch, content,
                                                    MyUser.BaseUser.UserID, Dal.DalCustomer);

                        }
                    }
                    else
                    {
                        content = string.Format("分发至网站应用ID：{0}；中心用户ID：{1}；反馈信息：{2}。", tmpWebappID, newUserSsoID,re);
                        UserCenterHelp.WriteLog("其他网站注册失败：" + tmpWebappID + "/" + newUserSsoID, 2, batch, content, MyUser.BaseUser.UserID, Dal.DalCustomer);
                   
                    }

                    #endregion
                }
            }

            #endregion

            string[] tmpPsw = rndPsw.Split(',');
            context.Response.Write(string.Format("{{\"msg\":\"\",\"newID\":{0},\"loginPsw\":\"{1}\",\"secondAuthPsw\":\"{2}\"}}", newUserSsoID, tmpPsw[0], tmpPsw[1]));

        }
        #endregion

        #region 2 开通用户
        private void RegApp(HttpContext context)
        {
            #region 接收绍与验证
            //中心端的用户ID
            string userIDsso = context.Request["userIDsso"];
            //要开通的网站，可以是多个。1,2,3的形式
            string appIDs = context.Request["appIDs"];

            if (!Functions.IsInt(userIDsso))
            {
                ResponseWriteError( "用户ID必须是数字！");
                return;
            }
            if (!Functions.IsIDString(appIDs))
            {
                ResponseWriteError( "appIDs必须是数字组合（半角逗号分隔）！");
                return;
            }
            #endregion

            //操作人ID
            string optionUserID = MyUser.BaseUser.UserID;
            
            string batch = Functions.RndNumNotRepeat(8);
           
            //记录日志
            string content = string.Format("用户IDsso：{0}；要开通网站ID：{1}；操作人ID：{2}。", userIDsso, appIDs, optionUserID);
            UserCenterHelp.WriteLog("接收开通申请：" + userIDsso + "/" + appIDs, 2, batch, content, optionUserID, Dal.DalCustomer);

            //获取用户ID和网站应用ID
            string userCode = "";           //开通用户的账户
            string userLoginName = "";      //开通用户的登录账户

            string adminTrueName = "";      //管理员的真实姓名

            string sql = "";

            sql = "SELECT TOP 1 UserCode, UserLoginName FROM   SSO_UserSSO WHERE (UserSSOID = {0})";
            string[] tmp = Dal.DalCustomer.ExecuteStringsBySingleRow(string.Format(sql, userIDsso));
            if (tmp == null)
            {
                ResponseWriteError( "没有找到用户信息！");
                return;

            }
            userCode = tmp[0];
            userLoginName = tmp[1];

            sql = "SELECT TOP 1 TrueName FROM   SSO_UserSSO WHERE (UserSSOID = {0})";
            adminTrueName = Dal.DalCustomer.ExecuteString(string.Format(sql, optionUserID));
            if (adminTrueName == null)
            {
                ResponseWriteError( "没有找到操作人员的信息！");
                return;
            }

            sql = "SELECT TOP 1 TrueName, UserLoginName FROM   SSO_UserSSO WHERE (UserSSOID = {0})";
            string[] tmpUserTrueName = Dal.DalCustomer.ExecuteStringsBySingleRow(string.Format(sql, userIDsso));
            if (tmpUserTrueName == null)
            {
                ResponseWriteError( "没有找到用户ID的人员的信息！");
                return;
            }
            string userTrueName = tmpUserTrueName[0].Length == 0 ? tmpUserTrueName[1] : tmpUserTrueName[0];

            //保存对应关系的SQL语句
            const string sqlUserSSOAppInsert = "INSERT INTO SSO_UserSSOApp (WebAppID,UserSSOID,UserAppID) VALUES ({0},{1},{2})";
            
            //遍历要开通的网站，判断是否已经开通，没有开通的发送开通申请，并且记录日志
            int alreayOpenCount = 0;        //以前已经开通的数量
            int openCount = 0;              //本次开通数量
            int error = 0;                  //开通失败数量

            string alreayOpenAppIDs = "";//以前已经开通的appID 集合
            string openAppIDs = "";      //本次开通的appID 集合

            //获取用户在mis里的用户ID
            string misUserID = "";
            sql = "SELECT TOP 1 UserAppID FROM SSO_UserSSOApp WHERE (WebAppID = {0}) AND (UserSSOID = {1})";
            misUserID = Dal.DalCustomer.ExecuteString(string.Format(sql, 4, userIDsso));
            if (misUserID == null)
                misUserID = "-1";

            string[] appIDarr = appIDs.Split(',');
            foreach (string appID in appIDarr )
            {
                //检查是否已经开通
                sql = "SELECT TOP 1 1 FROM SSO_UserSSOApp WHERE (WebAppID = {0}) AND (UserSSOID = {1})";

                if (Dal.DalCustomer.ExecuteExists(string.Format( sql,appID,userIDsso)))
                {
                    //已经开通，记录日志
                    alreayOpenCount++;
                    alreayOpenAppIDs += appID + ",";

                    content = string.Format("网站ID ：{0}。", appID);
                    UserCenterHelp.WriteLog("网站已经开通不发送申请：" + appID, 2, batch, content, optionUserID, Dal.DalCustomer);
                }
                else
                {
                    //发送开通申请
                    content = string.Format("网站ID ：{0}。", appID);
                    UserCenterHelp.WriteLog("网站未开通发送申请：" + appID + "/" + userIDsso, 2, batch, content, MyUser.BaseUser.UserID, Dal.DalCustomer);

                    sql = "SELECT  TOP 1 AppURL, AppName FROM  SSO_Webapp WHERE  (WebAppID = {0})";
                    string[] appUrlandName = Dal.DalCustomer.ExecuteStringsBySingleRow(string.Format(sql, appID));

                    string re = UserCenterHelp.SendRegUserInfo(appID, userCode, userLoginName, userIDsso, misUserID, appUrlandName[0], batch, Dal.DalCustomer, MyUser.BaseUser.UserID);
                    if (re.Length > 0)
                    {
                        #region 其他网站注册成功
                        
                        string newUserAppID = re;
                        openCount++;
                        openAppIDs += appID + ",";

                        content = string.Format("网站应用ID：{0}；用户IDsso：{1}；网站用户ID：{2}。", appID,userIDsso , newUserAppID);
                        UserCenterHelp.WriteLog("其他网站注册成功：" + appID + "/" + newUserAppID, 2, batch, content, optionUserID, Dal.DalCustomer);

                        //SSO_UserSSOApp 写对应关系
                        Dal.DalCustomer.ExecuteNonQuery(string.Format(sqlUserSSOAppInsert, appID, userIDsso, newUserAppID));
                        if (Dal.DalCustomer.ErrorMessage.Length > 2)
                        {
                            //有错误，
                            ResponseWriteError( "保存数据的时候，发生异常！");

                            content = string.Format("网站应用ID：{0}；网站用户ID：{1}；中心用户ID：{2}。", appID, newUserAppID, userIDsso);
                            UserCenterHelp.WriteLog("保存对应关系失败：" + appID + "/" + newUserAppID, 2, batch, content, optionUserID, Dal.DalCustomer);

                        }
                        else
                        {
                            //保存成功，记录操作日志
                            content = string.Format("网站应用ID：{0}；网站用户ID：{1}；中心用户ID：{2}。", appID, newUserAppID, userIDsso);
                            UserCenterHelp.WriteLog("保存对应关系成功：" + appID + "/" + newUserAppID, 2, batch, content, optionUserID, Dal.DalCustomer);
      
                            //记录开通日志
                            string openDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                            if (optionUserID == userIDsso)
                            {
                                //自己开通 appUrlandName[1]
                                content = string.Format("[{0}|{1}]于[{2}]注册开通[{3}]", userTrueName, userIDsso,openDate, appUrlandName[1]);
                            }
                            else
                            {
                                //他人开通
                                content = string.Format("[{4}|{5}]于[{2}]指定开通[{0}|{1}]对[{3}]的访问权限。", userTrueName, userIDsso, openDate, appUrlandName[1], adminTrueName, optionUserID);
                            }
                            UserCenterHelp.WriteLog("开通网站：" + appID + "/" + newUserAppID, 1, batch, content, optionUserID, Dal.DalCustomer);
      
                        }
                        #endregion
                    }
                    else
                    {
                        error++;
                        content = string.Format("分发至网站应用ID：{0}；中心用户ID：{1}；反馈信息：{2}。", appID, userIDsso, re);
                        UserCenterHelp.WriteLog("其他网站注册失败：" + appID + "/" + userIDsso, 2, batch, content, optionUserID, Dal.DalCustomer);

                    }

                }
            }

            openAppIDs = openAppIDs.TrimEnd(',');
            alreayOpenAppIDs = alreayOpenAppIDs.TrimEnd(',');

            context.Response.Write(string.Format("{{\"msg\":\"\",\"alreayOpenCount\":\"{0}\",\"openCount\":\"{1}\",\"error\":\"{2}\",\"openAppIDs\":\"{3}\",\"alreayOpenAppIDs\":\"{4}\"}}", alreayOpenCount, openCount, error, openAppIDs, alreayOpenAppIDs));


        }
        #endregion

        #region 3 提供用户信息的最新版本
        private void GetUserNewVersion(HttpContext context)
        {
            string userIDapp = context.Request["userIDapp"];
            #region 验证
            if (!Functions.IsInt(userIDapp))
            {
                ResponseWriteError( "用户ID必须是数字！");
                return;
            }
            #endregion
           
            string userIDsso = UserCenterHelp.GetUserIDssoByUserIDappAppID(context,WebAppID,userIDapp,  Dal.DalCustomer);

            if (string.IsNullOrEmpty(userIDsso))
            {
                //没有这个用户
                return;
            }

            string sql = "";
            sql = "SELECT TOP 1 Version FROM SSO_UserSSO WHERE (UserSSOID = {0})";

            string ver = Dal.DalCustomer.ExecuteString(string.Format(sql, userIDsso));

            context.Response.Write(string.Format("{{\"msg\":\"\",\"ver\":\"{0}\"}}", ver));

        }
        #endregion

        #region 4 提供用户信息的最新版本
        private void GetUserNewInfo(HttpContext context)
        {
            string userIDapp = context.Request["userIDapp"];
            #region 验证
            if (!Functions.IsInt(userIDapp))
            {
                ResponseWriteError( "用户ID必须是数字！");
                return;
            }
            #endregion


            string sql = "";

            sql = "SELECT  TOP 1 UserSSOID FROM SSO_UserSSOApp WHERE (WebAppID = {0}) AND (UserAppID = {1})";
            string userIDsso = Dal.DalCustomer.ExecuteString(string.Format(sql, WebAppID, userIDapp));

            if (string.IsNullOrEmpty(userIDsso))
            {
                //没有这个用户
                context.Response.Write(string.Format("{{\"msg\":\"没有这个用户\"}}"));
                return;
            }

            sql = "SELECT TOP 1 Version FROM SSO_UserSSO WHERE (UserSSOID = {0})";

            string ver = Dal.DalCustomer.ExecuteString(string.Format(sql, userIDsso));

            context.Response.Write(string.Format("{{\"msg\":\"\",\"ver\":\"{0}\"}}", ver));

        }
        #endregion

        #region 5 验证登录账户是否重复
        private void HasLoginName(HttpContext context)
        {
            string userLoginName = ""; //登录账户
            string msg = "";
            
            userLoginName = context.Request["userLoginName"];
            if (string.IsNullOrEmpty(userLoginName))
            {
                msg = "登录账户不能为空！";
            }
            else
            {
                userLoginName = userLoginName.Replace("'", "''");
                string sql = "";

                //检查是否重名
                sql = "SELECT TOP 1 1 FROM  SSO_UserSSO WHERE (UserLoginName = N'{0}') ";
                msg = Dal.DalCustomer.ExecuteExists(string.Format(sql, userLoginName)) ? "1" : "0";
    
            }
            
            context.Response.Write(CallBack.Length == 0
                                       ? string.Format("{{\"msg\":\"{0}\"}}", msg)
                                       : string.Format("{0}({{\"msg\":\"{1}\"}})", CallBack, msg));

        }
        #endregion

        #region 6、7  修改登录密码
        /// <summary>
        ///  修改登录密码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="pswColName">密码字段名</param>
        private void Updatepsw(HttpContext context, string pswColName)
        {
            string msg = UpdatePassWord(context,pswColName);

            //判断是否表单提交，还是ajax提交
            string reUrl = context.Request["url"];

            if (!string.IsNullOrEmpty(reUrl))
            {
                //表单跨域提交，返回提交的页面

                string url = reUrl + "?msg={0}";
                context.Response.Redirect(string.Format(url, msg));
                context.Response.End();
            }

            context.Response.Write(string.Format("{{\"msg\":\"{0}\"}}",msg));

        }

        private string UpdatePassWord(HttpContext context, string pswColName)
        {
            #region 定义和接收
            string userIDapp = ""; //用户在应用端的ID
            string pswOld = "";      //登录密码，如果没有发送，则生成随机密码，八位数字
            string pswNew = "";      //登录密码，如果没有发送，则生成随机密码，八位数字
            
            //string secondAuthPsw = ""; //鉴权密码、二次操作密码，如果没有发送，则生成随机密码，八位数字

            string msg = "";

            userIDapp = context.Request["userIDapp"];
            pswOld = context.Request["pswOld"];
            pswNew = context.Request["pswNew"];
            #endregion

            #region 验证
            if (!Functions.IsInt(userIDapp))
                return "用户ID必须是数字！";
            if (string.IsNullOrEmpty(pswOld))
                return "请输入原来的密码！";
            if (string.IsNullOrEmpty(pswNew))
                return "请输入新的密码！";
          
            #endregion
 
            string sql = "";

            //获取sso端的用户ID
            sql = "SELECT TOP 1 UserSSOID FROM SSO_UserSSOApp WHERE (WebAppID = {0}) AND (UserAppID = {1})";
            string userIDsso = Dal.DalCustomer.ExecuteString(string.Format(sql, WebAppID, userIDapp));

            if (string.IsNullOrEmpty(userIDsso))
                return "没有这个用户";
         
            #region 检查原密码是否正确
            //加密密码
            pswOld = Functions.ToMD5(pswOld);

            sql = "SELECT TOP 1 1 FROM  SSO_UserSSO WHERE UserSSOID = {0} AND {1} ='{2}'";
            if (!Dal.DalCustomer.ExecuteExists(string.Format(sql, userIDsso, pswColName,pswOld)))
                return "原密码不正确！";
             
            #endregion
           
            //加密密码
            pswNew = Functions.ToMD5(pswNew);

            #region 保存到数据库

            sql = "UPDATE SSO_UserSSO SET {0} = '{1}' WHERE UserSSOID={2}";
            Dal.DalCustomer.ExecuteNonQuery(string.Format(sql,pswColName, pswNew, userIDsso));
            if (Dal.DalCustomer.ErrorMessage.Length > 2)
                return "保存数据的时候，发生异常！";
             
            #endregion

            return "";

        }
        #endregion
        
        #region 8 获取用户在中心的ID
        private void GetUserIDsso(HttpContext context)
        {
            string userIDapp = context.Request["userIDapp"];
            #region 验证
            if (!Functions.IsInt(userIDapp))
            {
                ResponseWriteError( "用户ID必须是数字！");
                return;
            }
            #endregion

            string sql = "";

            sql = "SELECT  TOP 1 UserSSOID FROM SSO_UserSSOApp WHERE (WebAppID = {0}) AND (UserAppID = {1})";
            string userIDsso = Dal.DalCustomer.ExecuteString(string.Format(sql, WebAppID, userIDapp));

            if (string.IsNullOrEmpty(userIDsso))
            {
                //没有这个用户
                context.Response.Write(string.Format("{{\"msg\":\"没有这个用户\"}}"));
                return;
            }

            context.Response.Write(string.Format("{{\"msg\":\"\",\"userIDsso\":\"{0}\"}}", userIDsso));

        }
        #endregion

        #region 9 接收app端传递过来的用户信息，保存到数据库，保存前后做备份
        private void PostUserInfo(HttpContext context)
        {
            //转换成字典，
            //接收数据，装入字典
            Dictionary<string, string> dicForm = MyWebClient.RequestForm();

            //要修改的人员信息的ID，转换为sso端的ID
            string userIDapp = DataID;
            string webAppID = WebAppID;

            string userIDsso = UserCenterHelp.GetUserIDssoByUserIDappAppID(context, WebAppID, userIDapp, Dal.DalCustomer);

            if (string.IsNullOrEmpty(userIDsso))
            {
                //没有这个用户
                context.Response.Write(string.Format("{{\"msg\":\"没有这个用户\"}}"));
                return;
            }

            //保存原有记录
            ManagerParameter mgrParam = Dal.DalCustomer.ManagerParameter;
            mgrParam.ClearParameter();
            mgrParam.AddNewInParameter("UserSSOID", int.Parse(userIDsso));
            mgrParam.AddNewInParameter("WebAppID", int.Parse(webAppID));
            mgrParam.AddNewInParameter("OperatorID",MyUser.BaseUser.UserID );
            Dal.DalCustomer.ExecuteNonQuery("Pro_AddSsoUserLog");

            //更新数据库
            URLParam param = new URLParam(context);

            param.DataID = userIDsso;
            param.Action = "savedata";
            param.ButtonID = "10307";

            string batch = Guid.NewGuid().ToString().Substring(0, 7);
            string re = UserCenterHelp.SaveUserSsoInfoByService(dicForm, param, batch, Dal.DalCustomer,"2");//MyUser.BaseUser.UserID


            //保存新纪录
            Dal.DalCustomer.ExecuteNonQuery("Pro_AddSsoUserLog");

            context.Response.Write(string.Format("{{\"msg\":\"{0}\"}}",re));

        }
        #endregion

        #region  10 修改用户状态
        /// <summary>
        ///  修改用户状态——1：可以登录；2：不可以登录；3：暂停；4：锁定
        /// </summary>
        /// <param name="context"></param>
        private void UpdateUserState(HttpContext context )
        {
            #region 定义和接收
            string userIDapp = ""; //用户在应用端的ID
            string state = "";      //用户状态——1：可以登录；2：不可以登录；3：暂停；4：锁定
           
            //string secondAuthPsw = ""; //鉴权密码、二次操作密码，如果没有发送，则生成随机密码，八位数字

            userIDapp = context.Request["userIDapp"];
            state = context.Request["state"];
            #endregion

            #region 验证
            if (!Functions.IsInt(userIDapp))
            {
                ResponseWriteError( "用户ID必须是数字！");
                return;
            }
            if (!Functions.IsInt(state))
            {
                ResponseWriteError( "用户状态必须是数字！1：可以登录；2：不可以登录；3：暂停；4：锁定");
                return;
            }

            #endregion

            string sql = "";

            //获取sso端的用户ID
            sql = "SELECT  TOP 1 UserSSOID FROM SSO_UserSSOApp WHERE (WebAppID = {0}) AND (UserAppID = {1})";
            string userIDsso = Dal.DalCustomer.ExecuteString(string.Format(sql, WebAppID, userIDapp));

            if (string.IsNullOrEmpty(userIDsso))
            {
                //没有这个用户
                context.Response.Write(string.Format("{{\"msg\":\"没有这个用户\"}}"));
                return;
            }
             
           
            #region 保存到数据库

            sql = "UPDATE SSO_UserSSO SET State = {0} WHERE UserSSOID={1}";
            Dal.DalCustomer.ExecuteNonQuery(string.Format(sql, state, userIDsso));
            if (Dal.DalCustomer.ErrorMessage.Length > 2)
            {
                //有错误
                ResponseWriteError( "保存数据的时候，发生异常！");
                return;
            }

            #endregion

            context.Response.Write(string.Format("{{\"msg\":\"\"}}"));

        }
        #endregion


        #region 覆盖基类，验证用户是否登录

        /// <summary>
        /// 坚持当前访问者是否登录。
        /// 两种情况：
        /// 1、登录页面：这个不能检查，所以做个钩子，登录页面重新函数搞定
        /// 2、其他页面：已登录页面，需要检查了，放在基类里，子类省事了。
        /// </summary>
        /// <param name="debugInfoList"></param>
        protected override void CheckUser(IList<NatureDebugInfo> debugInfoList)
        {
            //验证是否已经登录
            //如果已经登录了，加载登录人员的信息，
            var manageUser = new ManageUser {Dal = Dal};

            UserWebappInfo userWebappInfo = AppManage.UserWebappInfoByCookies(null);

            if (userWebappInfo.State != UserState.NormalAccess)
            {
                //没有登录。
                //判断是否表单提交，还是ajax提交
                string reUrl = Request["url"];

                if (!string.IsNullOrEmpty(reUrl))
                {
                    //表单跨域提交，返回提交的页面

                    string url = reUrl + "?msg={0}";
                    Response.Redirect(string.Format(url, "没有登录，或者登录超时！"));
                    Response.End();
                }

                if (string.IsNullOrEmpty(CallBack))
                    Response.Write("{\"msg\":\"-1\",\"test\":\"test-1\"}");
                else
                    Response.Write(CallBack + "({\"msg\":\"-1\",\"test\":\"test-1\"})");

                MyUser = null;
                Response.End();

            }
            else
            {
                MyUser = manageUser.CreateUser(Convert.ToString(userWebappInfo.UserSsoID),null);
            }
        }

        #endregion

    }
}