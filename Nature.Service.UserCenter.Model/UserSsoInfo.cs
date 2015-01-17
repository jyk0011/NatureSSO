using System;
using System.Collections.Generic;
using System.Text;
using Nature.Attributes;

namespace Nature.Service.UserCenter.Model
{
    /// <summary>
    /// 用户的注册信息
    /// </summary>
    /// user:jyk
    /// time:2013/3/2 13:22
    public class UserRegInfo
    {
        #region 属性

        #region 用户ID
        /// <summary>
        /// 用户ID
        /// </summary>
        [ColumnID(10082010)]
        public Int32 UserSSOID { get; set; }
        #endregion

        #region web应用用户ID
        /// <summary>
        /// web应用用户ID
        /// </summary>
        [ColumnID(10084040)]
        public Int32 UserAppID { get; set; }
        #endregion

        #region 用户名
        /// <summary>
        /// 用户名
        /// </summary>
        [ColumnID(10082020)]
        public string UserCode { get; set; }
        #endregion

        #region 登录名
        /// <summary>
        /// 登录名
        /// </summary>
        [ColumnID(10082030)]
        public string UserLoginName { get; set; }
        #endregion

        #region 登录密码
        /// <summary>
        /// 登录密码
        /// </summary>
        [ColumnID(10082040)]
        public string LoginPsw { get; set; }
        #endregion

        #region 关键操作密码
        /// <summary>
        /// 关键操作密码
        /// </summary>
        [ColumnID(10082050)]
        public string SecondAuthPsw { get; set; }
        #endregion
        #endregion
    }

    #region 获取用户在sso端的信息的实体类
    /// <summary>
    /// 用户在Sso的信息
    /// </summary>
    /// user:jyk
    /// time:2013/3/2 13:22
    public class UserSsoInfo
    {
        #region 属性

        #region 用户ID

        /// <summary>
        /// 用户ID
        /// </summary>
        [ColumnID(10082010)]
        public Int32 UserSSOID { get; set; }

        #endregion

        #region 用户名

        /// <summary>
        /// 用户名
        /// </summary>
        [ColumnID(10082020)]
        public string UserCode { get; set; }

        #endregion

        #region 登录名

        /// <summary>
        /// 登录名
        /// </summary>
        [ColumnID(10082030)]
        public string UserLoginName { get; set; }

        #endregion

        #region 状态

        /// <summary>
        /// 状态
        /// </summary>
        [ColumnID(10082070)]
        public Int32 State { get; set; }

        #endregion

        #region 真实姓名

        /// <summary>
        /// 真实姓名
        /// </summary>
        [ColumnID(10082080)]
        public string TrueName { get; set; }

        #endregion

        #region 头像

        /// <summary>
        /// 头像
        /// </summary>
        [ColumnID(10082090)]
        public string Photo { get; set; }

        #endregion

        #region 生日

        /// <summary>
        /// 生日
        /// </summary>
        [ColumnID(10082100)]
        public DateTime Birthday { get; set; }

        #endregion

        #region 性别

        /// <summary>
        /// 性别
        /// </summary>
        [ColumnID(10082110)]
        public string Gender { get; set; }

        #endregion

        #region 身份证号码

        /// <summary>
        /// 身份证号码
        /// </summary>
        [ColumnID(10082120)]
        public string IDCard { get; set; }

        #endregion

        #region 手机号码

        /// <summary>
        /// 手机号码
        /// </summary>
        [ColumnID(10082130)]
        public string Mobile { get; set; }

        #endregion

        #region QQ

        /// <summary>
        /// QQ
        /// </summary>
        [ColumnID(10082140)]
        public string QQ { get; set; }

        #endregion

        #region 邮箱

        /// <summary>
        /// 邮箱
        /// </summary>
        [ColumnID(10082150)]
        public string Email { get; set; }

        #endregion

        #region 银行卡号

        /// <summary>
        /// 银行卡号
        /// </summary>
        [ColumnID(10082160)]
        public string BankCardNumber { get; set; }

        #endregion

        #region 版本号

        /// <summary>
        /// 版本号
        /// </summary>
        [ColumnID(10082170)]
        public string Version { get; set; }

        #endregion

        #endregion
    }
    #endregion

    #region app端提交的用户信息
    /// <summary>
    /// app端提交的用户信息
    /// </summary>
    /// user:jyk
    /// time:2013/3/8 13:52
    public class UserInfoSendtoSSO
    {
        #region 属性

        #region 真实姓名
        /// <summary>
        /// 真实姓名
        /// </summary>
        [ColumnID(10082080)]
        public string TrueName { get; set; }
        #endregion

        #region 头像
        /// <summary>
        /// 头像
        /// </summary>
        [ColumnID(10082090)]
        public string Photo { get; set; }
        #endregion

        #region 生日
        /// <summary>
        /// 生日
        /// </summary>
        [ColumnID(10082100)]
        public DateTime Birthday { get; set; }
        #endregion

        #region 性别
        /// <summary>
        /// 性别
        /// </summary>
        [ColumnID(10082110)]
        public string Gender { get; set; }
        #endregion

        #region 身份证号码
        /// <summary>
        /// 身份证号码
        /// </summary>
        [ColumnID(10082120)]
        public string IDCard { get; set; }
        #endregion

        #region 手机号码
        /// <summary>
        /// 手机号码
        /// </summary>
        [ColumnID(10082130)]
        public string Mobile { get; set; }
        #endregion

        #region QQ
        /// <summary>
        /// QQ
        /// </summary>
        [ColumnID(10082140)]
        public string QQ { get; set; }
        #endregion

        #region 邮箱
        /// <summary>
        /// 邮箱
        /// </summary>
        [ColumnID(10082150)]
        public string Email { get; set; }
        #endregion

        #region 银行卡号
        /// <summary>
        /// 银行卡号
        /// </summary>
        [ColumnID(10082160)]
        public string BankCardNumber { get; set; }
        #endregion
        #endregion
    }
    #endregion

}
