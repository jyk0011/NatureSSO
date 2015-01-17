

namespace Nature.Service.Permissions
{
    /// <summary>
    /// 权限的请求类型枚举
    /// </summary>
    public enum ActionKind
    {
        /// <summary>
        /// 1 获取可以操作的模块ID集合。1,2,3的字符串
        /// </summary>
        /// user:jyk
        /// time:2013/2/2 13:05
        GetModleIDs = 1,

        /// <summary>
        /// 2 获取指定模块里可以使用的按钮ID集合。1,2,3的字符串
        /// </summary>
        GetButtonIDs = 2,
        /// <summary>
        /// 3 获取指定模块ID和页面视图ID里，可以使用的【数据列表】里的列的ID集合。1,2,3的字符串
        /// </summary>
        GetGridColumnIDs = 3,
        /// <summary>
        /// 4 获取指定模块ID和页面视图ID里，可以使用的【查询】里的字段的ID集合。1,2,3的字符串
        /// </summary>
        GetFindColumnIDs = 4,
        /// <summary>
        /// 5 获取指定模块ID和页面视图ID里，可以使用的【表单】里的字段的ID集合。1,2,3的字符串
        /// </summary>
        GetFormColumnIDs = 5,
        /// <summary>
        /// 6 获取指定模块ID的读取数据的SQL应该加的查询条件，作为权限过滤条件
        /// </summary>
        GetQuery = 6

    }
}