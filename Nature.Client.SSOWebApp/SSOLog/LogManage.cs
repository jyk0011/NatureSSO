using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using Nature.Client.SSOApp;
using Nature.Common;
using Nature.SsoConfig;

namespace Nature.Client.SSOLog
{
    /// <summary>
    /// 日志的类
    /// </summary>
    /// user:jyk
    /// time:2013/3/28 14:28
    public class SsoLogJson
    {
        /// <summary>
        /// 用户在sso端的用户ID
        /// </summary>
        /// user:jyk
        /// time:2013/3/29 10:07
        public int UserIDsso = -1;
        /// <summary>
        /// 用户在app端的用户ID
        /// </summary>
        public int UserIDapp = -1;
        /// <summary>
        /// 应用的标识
        /// </summary>
        public string AppID = "0";
        /// <summary>
        /// 状态，成功还是失败
        /// </summary>
        public string State = "";
        /// <summary>
        /// 记录一些信息
        /// </summary>
        public string Msg = "";

        private string _key  ;

        /// <summary>
        /// 一个访问周期的标识
        /// </summary>
        public string Key
        {
            get
            {
                if (_key == null)
                {
                    LogManage.Index++;
                    if (LogManage.Index > 9998) LogManage.Index = 1000;
                    _key = LogManage.Index.ToString(CultureInfo.InvariantCulture);
                }
                return _key;
            }
            set { _key = value; }
        }

    }

    /// <summary>
    /// 记录登录日志。
    /// 记录到文件，text 。json格式。
    /// {
    /// userIDsso_时间_序号:{
    ///         "time":"9:32",
    ///         "userIDsso":"1",
    ///         "userIDapp":"1",
    ///         "appID":"4",
    ///         "state":"成功|失败",
    ///         "msg":"记录些描述"
    ///     },
    /// ……
    /// 
    /// }
    /// </summary>
    /// user:jyk
    /// time:2013/3/28 13:41
    public static class LogManage
    {
        //存放日志文件的文件夹的位置
        private static readonly string Path = HttpContext.Current.Server.MapPath("~/log/");
        /// <summary>
        /// 
        /// </summary>
        public static int Index = 1000;

        /// <summary>
        /// 静态类的初始化，判断文件夹是否建立
        /// </summary>
        /// user:jyk
        /// time:2013/3/28 14:10
        static LogManage()
        {
            //判断是否建立了log文件夹
            if (!System.IO.Directory.Exists(Path))
                System.IO.Directory.CreateDirectory(Path);
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// user:jyk
        /// time:2013/3/28 14:11
        public static void WriteOne(SsoLogJson ssoLog )
        {
            //判断是否记录日志
            if (!SsoInfo.IsWriteSsoLog)
            {
                return;
            }

            //记录到错误日志，一个小时一个文件
            string filePath = Path + DateTime.Now.ToString("yyyyMMdd_HH") + ".txt";
            var str = new StringBuilder();

            //写 key
            str.Append(",\"k");
            str.Append(DateTime.Now.ToString("HH:mm:ss"));
            str.Append("_");
            str.Append(ssoLog.Key);
            str.Append("\":{");

            string ip = "";
            string url = "";

            HttpContext context = HttpContext.Current;
            if (context != null)
            {
                try
                {
                    ip = context.Request.UserHostAddress;
                    url = context.Request.Url.ToString();
                }
                catch (Exception e)
                {
                    
                }
            }

            //写入 value
            str.Append("\"time\":\"");
            str.Append(DateTime.Now.ToString("HH:mm:ss")); //访问的时间
            str.Append("\",\"appID\":\"");
            str.Append(ssoLog.AppID);
            str.Append("\",\"userIDsso\":\"");
            str.Append(ssoLog.UserIDsso);
            str.Append("\",\"userIDapp\":\"");
            str.Append(ssoLog.UserIDapp);
            str.Append("\",\"ip\":\"");
            str.Append(ip);
            str.Append("\",\"url\":\"");
            str.Append(url); //访问的网页和URL参数
            str.Append("\",\"state\":\"");
            str.Append(ssoLog.State);
            str.Append("\",\"msg\":");
            Json.StringToJson(ssoLog.Msg,str);
            str.Append("}");
            System.IO.StreamWriter sw = null;
            try
            {
                //判断文件是否存在
                bool isFileExists = System.IO.File.Exists(filePath);
       
                sw = new System.IO.StreamWriter(filePath, true, Encoding.Unicode);

                if (!isFileExists)
                {
                    //没有文件，先写入 {
                    sw.Write("{\"head\":\"cs\"");
                }
                sw.Write(str.ToString());

            }
            catch (Exception ex)
            {
                Functions.MsgBox("没有访问日志文件的权限！或日志文件只读!" + ex.Message, false);
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }
        }
    }
}