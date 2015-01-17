<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="close.aspx.cs" Inherits="Nature.SSO.close" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
     <script type="text/javascript">
//         window.onbeforeunload = onbeforeunload_handler;
//         window.onunload = onunload_handler;
//         function onbeforeunload_handler() {
//             var warning = "确认退出?";
//             return warning;
//         }

//         function onunload_handler() {
//             var warning = "谢谢光临";
//             alert(warning);
//         }

         window.onbeforeunload = function () //author: meizz    
         {
             var n = window.event.screenX - window.screenLeft;
             var b = n > document.documentElement.scrollWidth - 20;
             //if (b && window.event.clientY < 0 || window.event.altKey) {
                 //alert("是关闭而非刷新");
                 if (confirm("您确定要安全退出吗？")) {
                     alert("确定");
                 } else {
                     alert("取消");
                 }
                 window.event.returnValue = ""; //这里可以放置你想做的操作代码    
             //}
         };

         window.onload = function ()
         {
             
         }

     </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    啊啊
    </div>
    </form>
</body>
</html>
