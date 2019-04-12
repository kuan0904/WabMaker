using MyTool.Commons;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WabMaker.Web.Areas.Admins
{
    public class AdminsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admins";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {

            context.MapRoute(
               name: "Admins_default",
               url: "Admins/{AdminRoute}/{controller}/{action}/{id}",
               defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
               namespaces: new[] { "WabMaker.Web.Areas.Admins.Controllers" },
               constraints: new { AdminRoute = new EqualConstraint(Setting.AdminRoutes) }
            );

            //context.MapRoute(
            //    "Admins_default",
            //    "Admins/{controller}/{action}/{id}",
            //    new { action = "Index", id = UrlParameter.Optional }
            //);
        }

        /// <summary>
        /// 限制符合
        /// </summary>
        /// <seealso cref="System.Web.Routing.IRouteConstraint" />
        public class EqualConstraint : IRouteConstraint
        {
            private string[] _constraintName = new string[] { };

            public EqualConstraint(string[] constraintName)
            {
                _constraintName = constraintName;
            }

            public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
            {
                var stringValue = values[parameterName] as string;
                //不考慮大小寫的比對方式
                return Array.Exists(_constraintName,
                                    val => val.Equals(stringValue, StringComparison.InvariantCultureIgnoreCase));
            }
        }

    }
}