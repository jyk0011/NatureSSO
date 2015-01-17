using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using Nature.Common;
using Nature.Service.Ashx;

namespace Nature.Client.UserCenter
{
    #region 用户的注册信息
    /// <summary>
    /// 用户的注册信息
    /// </summary>
    /// user:jyk
    /// time:2013/3/9 9:26
    public class UserReg
    {
        /// <summary>
        /// 用户ID（网站应用端）
        /// </summary>
        /// user:jyk
        /// time:2013/3/9 9:27
        public string UserIDapp;
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserCode;
        /// <summary>
        /// 登录账户
        /// </summary>
        public string UserLoginName;

    }
    #endregion

    /// <summary>
    /// 接收用户中心发过来的用户信息，注册，并且返回新用户的ID，必须是int类型。
    /// </summary>
    public class RevRegUserV : BaseAshx
    {
        public override  void  ProcessRequest(HttpContext context)
        {
            base.ProcessRequest(context);

            context.Response.ContentType = "text/plain";

            //接收信息
            string userCode = context.Request["userCode"];
            string userLoginName = context.Request["userLoginName"];

            if (string.IsNullOrEmpty(userCode))
            {
                ResponseWriteError( "用户名不能为空！");
                return;
            }
            if (string.IsNullOrEmpty(userLoginName))
            {
                ResponseWriteError( "登录账户不能为空！");
                return;
            }

            var userReg = new UserReg {UserCode = userCode, UserLoginName = userLoginName};

            //保存到本地，
            string msg = SaveUser(userReg);

            context.Response.Write(string.Format("{{\"msg\":\"{0}\",\"newID\":\"{1}\"}}",msg, userReg.UserIDapp));

        }

        /// <summary>
        /// 保存用户注册信息，返回异常信息
        /// 如果成功，返回string.Empty ，否则返回错误描述信息
        /// </summary>
        /// <returns></returns>
        /// user:jyk
        /// time:2013/3/9 9:20
        protected virtual string SaveUser(UserReg userReg)
        {
            //模拟一个用户ID。
            userReg.UserIDapp = Functions.RndInt(1, 10000).ToString(CultureInfo.InvariantCulture);

            return "";
        }

        
    }
}