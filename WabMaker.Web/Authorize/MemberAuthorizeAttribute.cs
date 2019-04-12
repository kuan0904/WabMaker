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
    public class MemberAuthorizeAttribute : AuthorizeAttribute
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
            else if (SessionManager.UserID != Guid.Empty &&
                     SessionManager.AccountType == AccountType.Member)
            {

            }
            //partial提示已經logout (prevent login display in partial view) 
            else if (ActionPartialViewOnly(filterContext))
            {
                filterContext.Result = new ContentResult { Content = $"LogOutError" };
            }
            //登入頁是跳窗
            else if (ApplicationHelper.LoginStyle == LoginStyle.Popup)
            {              
                filterContext.Result = new RedirectResult("~/?url=" + RouteHelper.Url());
            }
            // 無權限
            else
            {                
                filterContext.Result = new RedirectResult($"~/Member/Login?url=" + RouteHelper.Url());
            }
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