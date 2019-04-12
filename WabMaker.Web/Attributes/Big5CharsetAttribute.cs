using System;
using System.Web;
using System.Web.Mvc;

namespace WabMaker.Web.Attributes
{
    public class Big5CharsetAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            filterContext.HttpContext.Response.AddHeader("Content-Type", "text/html; charset=big5");
            filterContext.HttpContext.Response.ContentEncoding = System.Text.Encoding.GetEncoding("Big5");
        }
    }
}
