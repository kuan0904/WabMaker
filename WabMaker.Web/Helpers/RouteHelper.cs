using MyTool.Enums;
using MyTool.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WabMaker.Web.Helpers
{
    public class RouteHelper
    {
        /// <summary>
        /// 取得路由參數
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static string Get(RouteName name)
        {
            return HttpContext.Current.Request.RequestContext.RouteData.Values[name.ToString()]?.ToString();
        }

        /// <summary>
        /// 現在網址(相對路徑)
        /// </summary>
        /// <returns></returns>
        public static string Url(bool isAbsolute = false)
        {
            return (isAbsolute ? BaseUrl() : "") + HttpContext.Current.Request.RawUrl;
        }

        public static string HostName()
        {
            return HttpContext.Current.Request.Url.Host;
        }

        public static string BaseUrl()
        {
            return string.Format("{0}://{1}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority);
        }

        /// <summary>
        /// Set FilePath
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="ThumbnailPath">The thumbnail path.</param>
        /// <param name="isAbsolute">if set to <c>true</c> [is absolute].</param>
        /// <returns></returns>
        public static string SetUrlPath(string filePath, string ThumbnailPath = "", bool isAbsolute = false)
        {
            string result = "";
            UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

            //1.縮圖
            if (!string.IsNullOrWhiteSpace(ThumbnailPath))
            {
                result = (isAbsolute ? BaseUrl() : "") + urlHelper.Content(ThumbnailPath);
            }
            //2.原圖
            else if (!string.IsNullOrWhiteSpace(filePath))
            {
                result = (isAbsolute ? BaseUrl() : "") + urlHelper.Content(filePath);
            }
            //3.無
            //Url.Content(null) 會error

            return result;
        }

        public static string SetImagePath(string filePath, string ThumbnailPath, string defaultImg = "")
        {

            return !string.IsNullOrWhiteSpace(ThumbnailPath) ? ThumbnailPath:
                !string.IsNullOrWhiteSpace(filePath) ? filePath: defaultImg;
        }

        /// <summary>
        /// url規則
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="action">The action.</param>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static string CreateUrl(string controller, string action, string url)
        {
            string result = "";
            try
            {
                if (string.IsNullOrEmpty(controller))
                {
                    // 若controller是null，回傳url (外部連結)
                    result = url;
                }
                else
                {
                    // controller + action + url參數
                    UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                    result = urlHelper.Action(action, controller) + url.FieldToString();

                }
            }
            catch (Exception ex)
            {
                _Log.CreateText("CreateUrl:" + _Json.ModelToJson(ex));
            }

            return string.IsNullOrEmpty(result) ? "javascript:" : result;
        }

        /// <summary>
        /// 產生驗證連結
        /// </summary>
        /// <returns></returns>
        public static string GetConfirmUrl(ValidType type, string validCode, string email)
        {
            return BaseUrl() + $"/Member/Confirm?type={type.ToString().ToLower()}&code={validCode}&key={HttpContext.Current.Server.UrlEncode(email)}";
        }

        /// <summary>
        /// 訂單回前一頁
        /// </summary>
        /// <param name="OrderErrorReturnPage">The order error return page.</param>
        /// <returns></returns>
        public static string OrderReturnUrl(int type, string routeName)
        {
            UrlHelper Url = new UrlHelper(HttpContext.Current.Request.RequestContext);

            if (type == (int)OrderErrorReturnPage.ItemDetail)
            {
                return Url.Action("Get", "Item", new { routeName });
            }
            else if (type == (int)OrderErrorReturnPage.MemberIndex)
            {
                return Url.Action("Index", "Member");
            }
            else
            {
                return Url.Action("Index", "Home");
            }
        }
    }

    public enum RouteName
    {
        AdminRoute,
        WebRoute,
        language,
        controller,
        action,
        id
    }
}
