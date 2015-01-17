using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Web;
using Nature.Common;

namespace Nature.Service.Permissions
{
    /// <summary>
    /// 权限过滤。可以显示哪些元素，比如功能节点、按钮、列、控件等。
    /// </summary>
    /// user:jyk
    /// time:2013/2/5 14:02
    public class PermissionsFilter : BaseAshxCrud
    {

        public override void ProcessRequest(HttpContext context)
        {
            base.ProcessRequest(context );

           
            string active = context.Request["active"];//：网站应用ID

            if (!Functions.IsInt(active))
            {
                return;
            }

            ActionKind actionKind = (ActionKind)int.Parse(active);

            switch (actionKind)
            {
                case ActionKind.GetModleIDs: //1 树
                    TreeMeta(context);
                    break;

                case ActionKind.GetButtonIDs : //2 按钮
                    ButtonMeta(context);
                    break;
                case ActionKind.GetGridColumnIDs : //3 列
                    GridMeta(context);
                    break;
                case ActionKind.GetFindColumnIDs : //4 查询
                    FindMeta(context);
                    break;
                case ActionKind.GetFormColumnIDs : //5 表单
                    FormMeta(context);
                    break;
                case ActionKind.GetQuery : //6 查询条件
                    Query(context);
                    break;
            }

             
           
        }

        #region 1 获取树的元数据
        private void TreeMeta(HttpContext context)
        {
            //判断权限
            string roleModuleID = MyUser.UserPermission.ModuleIDs;

            string json = string.Format("{{\"ModuleID\":\"{0}\"}}", roleModuleID);

            context.Response.Write(json);
        }
        #endregion

        #region 2 获取按钮的元数据
        private void ButtonMeta(HttpContext context)
        {
            //返回模块ID的json
           
            var sb = new StringBuilder(  300);

            if (MyUser.BaseUser.UserID == "1")
            {
                //超级管理员，可以访问全部的按钮
                sb.Append("{\"buttonRole\":\"admin\"}");
            }
            else
            {
                //获取可以操作的按钮
                string sql = "SELECT TOP 1 ButtonIDs FROM Role_RoleButtonPV WHERE RoleID in ({0}) AND ModuleID = {1}";
                //当前用户可以访问的按钮ID集合
                string buttonIDs = Dal.DalMetadata.ExecuteString(string.Format(sql, MyUser.UserPermission.RoleIDs, ModuleID));

                sb.Append("{\"buttonRole\":\"");
                sb.Append(buttonIDs);
                sb.Append("\" }");

            }

            context.Response.Write(sb.ToString());

        }
        #endregion

        #region 3 获取列表的元数据
        private void GridMeta(HttpContext context)
        {
            var sb = new StringBuilder( 300);
            //设置可以访问的字段
            SetCanUseCol(sb, MasterPageViewID.ToString(CultureInfo.InvariantCulture));
            context.Response.Write(sb.ToString());
        }
        #endregion
     
        #region 4 获取查询的元数据
        private void FindMeta(HttpContext context)
        {
            var sb = new StringBuilder(300);
            //设置可以访问的字段
            SetCanUseCol(sb, FindPageViewID.ToString(CultureInfo.InvariantCulture));
            context.Response.Write(sb.ToString());
        }
        #endregion

        #region 5 获取表单的元数据
        private void FormMeta(HttpContext context)
        {
            var sb = new StringBuilder( 300);
            //设置可以访问的字段
            SetCanUseCol(sb, MasterPageViewID.ToString(CultureInfo.InvariantCulture));
            context.Response.Write(sb.ToString());
        }
        #endregion

        #region 0 获取可以访问的字段
        private void SetCanUseCol(StringBuilder sb, string pageViewID)
        {
            if (MyUser.BaseUser.UserID == "1")
            {
                //超级管理员，可以访问全部的列
                sb.Append("{\"colRole\":\"admin\"}");
            }
            else
            {
                //获取可以操作的列
                string sql = @"SELECT TOP 1 ColumnIDs FROM Role_RoleColumn WHERE RoleID in ({0}) AND ModuleID = {1}  AND PVID = {2}";

                //当前用户可以访问的按钮ID集合
                string colIDs = Dal.DalRole.ExecuteString(string.Format(sql, MyUser.UserPermission.RoleIDs, ModuleID, pageViewID));

                sb.Append("{\"colRole\":\"");
                sb.Append(colIDs);
                sb.Append("\" }");

            }
        }
        #endregion


        #region 6 获取查询条件
        private void Query(HttpContext context)
        {
            var sb = new StringBuilder(300);

            if (MyUser.BaseUser.UserID == "1")
            {
                //超级管理员，可以访问全部的按钮
                sb.Append("\"query\":\"admin\"}");
            }
            else
            {
                //获取 过滤方案，就是查询语句
                string sql = "SELECT TOP 1 SQLFilter FROM V_Frmae_List_RoleFilterPV WHERE RoleID in ({0}) AND ModuleID = {1} AND PVID = {2}";
                //当前用户可以访问的按钮ID集合
                string filter = Dal.DalMetadata.ExecuteString(string.Format(sql, MyUser.UserPermission.RoleIDs, ModuleID, MasterPageViewID));

                sb.Append("\"query\":\"");
                sb.Append(filter);
                sb.Append("\" }");

            }

            context.Response.Write(sb.ToString());
        }
        #endregion

    }
}