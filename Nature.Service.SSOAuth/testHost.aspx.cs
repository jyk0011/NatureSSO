using System;
using System.Collections.Generic;

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nature.Service
{
    public partial class testHost : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            string url = "http://natureservice.517.cn/testHost.ashx";
            string errorMsg = "";
            string ssokey = MyWebClient.Post(url, null, out errorMsg);

            if (errorMsg.Length > 1)
            {
                Response.Write("访问出现异常！<br>");
                Response.Write(errorMsg);
            }

            Response.Write("<br><br>程序访问：" + url);
            Response.Write("<br><br>访问结果：");

            Response.Write(ssokey);
            
        }
    }
}