using System;
using Nature.Service.SSOAuth;

namespace Nature.Service
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack )
            {
                UserLoginInfo userOneself = ManageUserLoginInfo.GetUserOneselfInfoByCookie(null);

                if (userOneself != null)
                {
                    string userID = Convert.ToString(userOneself.UserSsoID);
                    this.DropDownList1.SelectedValue = userID;
                }
                 
            }
        }
       

      
    }
}