<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Nature.Service._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>模仿用户登录</title>
</head>
<body>
    <form id="form1" runat="server">
     <div>
    
        <asp:Button ID="Button2" runat="server" Text="读取本站cookies"   />
        <asp:TextBox ID="TextBox2" runat="server" Width="834px"></asp:TextBox>
    
         <br />
    
    </div>
     <div>
         当前登录用户：<asp:DropDownList ID="DropDownList1" runat="server">
             <asp:ListItem Value="0">没有登录</asp:ListItem>
             <asp:ListItem Value="1">小风</asp:ListItem>
             <asp:ListItem Value="2">小新</asp:ListItem>
             <asp:ListItem Value="3">金子</asp:ListItem>
             <asp:ListItem Value="4">海洋</asp:ListItem>
         </asp:DropDownList>
       
         </div>
    </form>
</body>
</html>
