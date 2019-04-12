using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Claims;
using System.Web.Mvc;
using WabMaker.Web.Helpers;
using WebMaker.BLL.Helpers;
using MyTool.Enums;

namespace WabMaker.Web.Authorize
{
    /// <summary>
    /// 角色權限判斷，取得Request上的Action與Controller
    /// Super管理員有全部權限
    /// </summary>
    public class AutoRolesAuthorizeAttribute : AuthorizeAttribute
    {
        public string ActionName { get; set; }
        protected string ControllerName { get; set; }

        /// <summary>
        /// 權限驗證
        /// </summary>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var adminRoute = RouteHelper.Get(RouteName.AdminRoute);

            // 如果沒有filterContext 則報錯
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            // 允許匿名訪問
            if (ActionAllowAnonymousAccess(filterContext))
            {

            }
            // 擁有權限
            else if (AuthorizeCore(filterContext.HttpContext))
            {
               
            }
            // 有登入
            else if (SessionManager.UserID != Guid.Empty
                && SessionManager.AccountType == AccountType.Admin
                //(不分大小寫)
                && adminRoute.ToLower() == SessionManager.Client.AdminRoute.ToLower()) {

                filterContext.Result = new HttpStatusCodeResult(403);
            }
            //partial提示已經logout (prevent login display in partial view) 
            else if (ActionPartialViewOnly(filterContext))
            {
                filterContext.Result = new ContentResult { Content = $"LogOutError" };
            }
            // 無權限
            else
            {
                filterContext.Result = new RedirectResult($"~/Admins/{adminRoute}/User/Login?url=" + RouteHelper.Url());
            }
        }

        /// <summary>
        /// 回傳授權結果
        /// </summary>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");

            //if (!httpContext.User.Identity.IsAuthenticated)
            //    return false;

            var adminRoute = RouteHelper.Get(RouteName.AdminRoute);
            string actionName = RouteHelper.Get(RouteName.action);         
            string controllerName = RouteHelper.Get(RouteName.controller);         
            //string rolename = controllerName + actionName;
        
            //會員已登入 & 有Client權限
            if (SessionManager.UserID != Guid.Empty 
                && SessionManager.AccountType == AccountType.Admin
                //(不分大小寫)
                && adminRoute.ToLower() == SessionManager.Client.AdminRoute.ToLower())
            {
               
                // Super管理員
                if (SessionManager.IsSuperManager)
                {
                    return true;
                }

                //首頁: 有登入都可進入
                if (controllerName == "Home" && (string.IsNullOrEmpty(actionName) || actionName == "Index")) {
                    return true;
                }

                //權限檔controller : 只能進入Menu範圍內的程式           
                ControllerType controllerType;                   
                if (Enum.TryParse(controllerName, out controllerType))
                {
                    var contain = SessionManager.RolePermissions.Any(x => x.ControllerType == (int)controllerType);
                    return contain;
                }
            }

            return false;
        }

        /// <summary>
        /// 驗證的方法是否有掛AllowAnonymousAttribute
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        private bool ActionAllowAnonymousAccess(AuthorizationContext filterContext)
        {
            return
                (filterContext.ActionDescriptor.GetCustomAttributes(typeof(AllowAnonymousAttribute), true)
                     .FirstOrDefault() as AllowAnonymousAttribute) != null;
        }

        private bool ActionPartialViewOnly(AuthorizationContext filterContext)
        {
            return
                (filterContext.ActionDescriptor.GetCustomAttributes(typeof(PartialCheck), true)
                     .FirstOrDefault() as PartialCheck) != null;
        }
        
    }
}