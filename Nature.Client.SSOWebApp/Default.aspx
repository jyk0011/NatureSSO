<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Nature.Client.Defaultapp" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    
    <script language="javascript" type="text/javascript" src="/Scripts/jquery-1.8.2.min.js"></script>
    <script language="javascript" type="text/javascript" src="/SSOApp/mangoSSO.js"></script>
    
    <script type="text/javascript">
         
        //退出sso
        function logout() {
            Natrue.SSO.getSSOinfo(function() {
                Natrue.SSO.logoutSSO();
            });
        }

    </script>
</head>
<body>
    <form id="form1" runat="server">
    
    <div id="divLogoutSSO" >
        <asp:Label ID="Label1" runat="server" Text="Label"></asp:Label>
        <input id="btnLogout" type="button" value="退出登录" onclick="logout()" />
    </div>
    
   
    <div id="divMsg"></div>
     
    </form>
    </body>
</html>
