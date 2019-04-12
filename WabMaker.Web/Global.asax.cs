using MyTool.Enums;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WabMaker.Web.App_Start;
using WabMaker.Web.Helpers;

namespace WabMaker.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest()
        {
            //設定網站基本參數
            if (ApplicationHelper.ClientID == Guid.Empty)
            {               
                var applicationHelper = new ApplicationHelper();
                applicationHelper.Init();
            }

            // 強迫https
            if (!Context.Request.IsSecureConnection && !ApplicationHelper.IsLocal && ApplicationHelper.IsHttps)
            {
                Response.Redirect(Context.Request.Url.ToString().Replace("http://", "https://"));
            }
        }
    }
}
