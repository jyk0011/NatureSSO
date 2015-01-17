using System;
using System.Collections.Generic;
using System.Web;

namespace Nature.Service
{
    /// <summary>
    /// testHost1 的摘要说明
    /// </summary>
    public class testHost1 : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("Hello World");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}