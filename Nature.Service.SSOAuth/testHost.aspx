<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="testHost.aspx.cs" Inherits="Nature.Service.testHost" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>测试host</title>
    <script language="javascript">
        function myopen() {
            window.open("http://natureservice.517.cn/testHost.ashx");
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        <input type="button" ID="Button1" runat="server" value="直接访问 testHost.ashx" onclick="myopen()" /> (打开新窗口显示)
        <br />
        <asp:Button ID="Button2" runat="server" Text="程序访问 testHost.ashx" OnClick="Button2_Click" />
    
    </div>
    </form>
</body>
</html>
