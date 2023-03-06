using System;
// using System.Web.Http.Filters;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class DescriptionNameAttribute : Attribute
{
    /// <summary>
    /// 接口地址
    /// </summary>
    public string Url { get; set; }
    /// <summary>
    /// 请求方式
    /// </summary>
    public string Method { get; set; }
    /// <summary>
    /// 是否禁用
    /// </summary>
    public bool Disabled { get; set; }
    /// <summary>
    /// 验证方式（0匿名1登录2角色）
    /// </summary>
    public int AllowScope { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }
    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime UpdateTime { get; set; }
    /// <summary>
    /// 权限id
    /// </summary>
    public string AuthorityId { get; set; }
    public string ModuleName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">权限id(一个控制器从小到大1,2,3,4)</param>
    /// <param name="url">接口地址</param>
    /// <param name="method">请求方法</param>
    /// <param name="controllerName">控制器名称</param>
    /// <param name="moduleName">模块名称</param>
    /// <param name="message">描述（新增删除编辑等）</param>
    /// <param name="disabled">是否可用</param>
    /// <param name="allowScope">验证方式（0 匿名 1登录 2角色）</param>
    /// /// <param name="servicetName">服务名称</param>
    public DescriptionNameAttribute(string id, string url, string method, bool disabled, int allowScope, string moduleName)
    {
        //从配置文件读取当前服务的名称然后加上一个服务标识
        this.AuthorityId = id;
        this.Url = url;
        this.Method = method;
        this.Disabled = disabled;
        this.AllowScope = allowScope;
        this.ModuleName = moduleName;
    }


    public DescriptionNameAttribute()
    {

    }
}
