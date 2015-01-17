using System.Configuration;
using System.Globalization;
using System.Web;

namespace Nature.Client.SSOApp
{
     

    #region 用户的登录状态
    /// <summary>
    /// 用户的登录状态
    /// </summary>
    /// user:jyk
    /// time:2013/1/30 9:05
    public enum UserState
    {
        /// <summary>
        /// 0：没有登录app网站
        /// </summary>
        /// user:jyk
        /// time:2013/1/30 9:09
        NotLoginApp = 0,
        /// <summary>
        /// 1：可以正常访问
        /// </summary>
        NormalAccess = 1,
        /// <summary>
        /// 2：不可以访问
        /// </summary>
        NotAccess = 2,
        /// <summary>
        /// 3：暂停访问
        /// </summary>
        SuspendAccess = 3,
        /// <summary>
        /// 4：被锁定不可以访问。
        /// </summary>
        Locked = 4,
        /// <summary>
        /// 5：登录超时
        /// </summary>
        LoginTimeout = 5

    }
    #endregion

    #region 用户验证信息的类
    /// <summary>
    /// 用户在网站应用端的信息
    /// </summary>
    /// user:jyk
    /// time:2013/1/23 15:54
    public class UserWebappInfo
    {
        /// <summary>
        /// 同一个账户多人登录的区分标记
        /// </summary>
        /// user:jyk
        /// time:2013/4/2 18:58
        public string GuidKey { get; set; }

        /// <summary>
        /// 用户在SSO端的用户ID
        /// </summary>
        /// user:jyk
        /// time:2013/1/23 15:51
        public int UserSsoID { get; set; }

        /// <summary>
        /// 用户在网站应用端的用户ID
        /// </summary>
        /// user:jyk
        /// time:2013/1/23 15:51
        public int UserWebappID { get; set; }

        /// <summary>
        /// 沟通包里IP
        /// </summary>
        /// user:jyk
        /// time:2013/4/3 10:41
        public string IP { get; set; }

        /// <summary>
        /// 包里的网站ID
        /// </summary>
        /// user:jyk
        /// time:2013/4/3 10:43
        public string  WebAppID { get; set; }

        /// <summary>
        /// 用户的访问状态
        ///0：没有登录app网站
        ///1：可以正常访问
        ///2：不可以访问
        ///3：暂停访问
        ///4：被锁定不可以访问。
        ///5：登录超时
        /// </summary>
        /// user:jyk
        /// time:2013/1/30 9:04
        public UserState State { get; set; }

        ///// <summary>
        ///// 创建时间，用于判断是否超时失效
        ///// 用于清理无效信息
        ///// </summary>
        ///// user:jyk
        ///// time:2013/1/23 15:53
        //public DateTime CreateTime { get; set; }

        ///// <summary>
        ///// 最后访问时间
        ///// 用于清理无效信息
        ///// </summary>
        ///// user:jyk
        ///// time:2013/1/23 15:53
        //public DateTime LastTime { get; set; }
         
        /// <summary>
        /// 票据。保存生成的加密字符串，作为备案
        /// </summary>
        /// user:jyk
        /// time:2013/1/23 15:55
        public string Ticket { get; set; }

        /// <summary>
        /// 执行过程中是否出现异常，如果没有异常 返回 string.Empty；如果有异常，返回描述信息
        /// </summary>
        /// user:jyk
        /// time:2013/3/26 14:05
        public string Error { get; set; }
    }
    #endregion
     
}