using MyTool.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WabMaker.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
            /*
            * member index
            */
            name: "index",
            url: "{controller}",
            defaults: new { controller = "Home", action = "Index" },
            constraints: new
            {
                controller = new EqualConstraint(new string[] { "Member" }),
                action = new EqualConstraint(new string[] { "Index" }),
            },
            namespaces: new[] { "WabMaker.Web.Controllers" }
            );

            routes.MapRoute(
            /*
            * search
            */
            name: "search",
            url: "Search",
            defaults: new { controller = "Home", action = "Search" },
            constraints: new
            {
                controller = new EqualConstraint(new string[] { "Home" }),
                action = new EqualConstraint(new string[] { "Search" })
            },
            namespaces: new[] { "WabMaker.Web.Controllers" }
            );
            
            routes.MapRoute(
            /*
            * item list
            */
            name: "List",
            url: "List/{routeName}",
            defaults: new { controller = "Item", action = "List" },
             constraints: new
             {
                 controller = new EqualConstraint(new string[] { "Item" }),
                 action = new EqualConstraint(new string[] { "List" }),
                 routeName = @".+"
             },
            namespaces: new[] { "WabMaker.Web.Controllers" }
            );
            
            routes.MapRoute(
            /*
            * item get
            */
            name: "item",
            url: "{routeName}",
            defaults: new { controller = "Item", action = "Get" },
            constraints: new
            {
                controller = new EqualConstraint(new string[] { "Item" }),
                action = new EqualConstraint(new string[] { "Get" }),
                routeName = @".+"
            },
             namespaces: new[] { "WabMaker.Web.Controllers" }
            );

            routes.MapRoute(
            /*
             * Default
             */
            name: "Default",
            url: "{controller}/{action}/{id}",
            defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
            namespaces: new[] { "WabMaker.Web.Controllers" }
            );
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

        /// <summary>
        /// 限制不符合
        /// </summary>
        /// <seealso cref="System.Web.Routing.IRouteConstraint" />
        public class NotEqual : IRouteConstraint
        {
            private string[] _constraintName = new string[] { };

            public NotEqual(string[] constraintName)
            {
                _constraintName = constraintName;
            }

            public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
            {
                var stringValue = values[parameterName] as string;
                //不考慮大小寫的比對方式
                return !Array.Exists(_constraintName,
                                    val => val.Equals(stringValue, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        /// <summary>
        /// 限制guid格式
        /// </summary>
        public class GuidConstraint : IRouteConstraint
        {
            public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values,
                                RouteDirection routeDirection)
            {
                if (values.ContainsKey(parameterName))
                {
                    var guid = values[parameterName] as Guid?;
                    if (guid.HasValue == false)
                    {
                        var stringValue = values[parameterName] as string;
                        if (string.IsNullOrWhiteSpace(stringValue) == false)
                        {
                            Guid parsedGuid;
                            // .NET 4 新增的 Guid.TryParse
                            Guid.TryParse(stringValue, out parsedGuid);
                            guid = parsedGuid;
                        }
                    }
                    return (guid.HasValue && guid.Value != Guid.Empty);
                }
                return false;
            }
        }

    }
}
