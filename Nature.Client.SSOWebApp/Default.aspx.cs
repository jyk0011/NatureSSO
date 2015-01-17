using System;
using Nature.Client.SSOApp;

/*
 * 应用网站，webform网站的模拟访问
 * 
 */

namespace Nature.Client
{
    /// <summary>
    /// 应用网站，webform网站的模拟访问
    /// </summary>
    /// user:jyk
    /// time:2013/1/19 17:45
    public partial class Defaultapp : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //判断是否登录网站应用
            AppManage.IsLoginApp();
            //当前用户是否可以继续访问
            AppManage.CanContinueAccess();
            //获取当前用户信息
            var userAppInfo =  AppManage.UserWebappInfoByCookies();

            if (AppManage.IsLoginApp()  )
            {
                //登录了app端
                //根据需求，是否需要询问sso端，当前登录用户是否可以继续访问
                if(AppManage.CanContinueAccess())
                {
                    //可以继续访问
                }
                else
                {
                    //不可以继续访问
                }

                Label1.Text = "欢迎："+userAppInfo.UserWebappID;
            }
            else
            {
                //没有登录app端，跳转到登录页面
                Response.Redirect("loginSSO.htm");
            }

             
        }
           
        
    }
}