using MyTool.Commons;
using MyTool.Enums;
using MyTool.Tools;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WabMaker.Web.Helpers;
using WebMaker.BLL.Helpers;
using WebMaker.BLL.Services;
using WebMaker.Entity.Models;
using WebMaker.Entity.ViewModels;
using static MyTool.Tools.MailTool;
using static WabMaker.Web.Helpers.CookieHelper;

namespace WabMaker.Web.Controllers
{
    public class BaseController : Controller
    {
        /// <summary>
        /// 回傳View
        /// </summary>
        public static string ViewName(string controllerName, string actionName)
        {
            return $"~/Views/{ApplicationHelper.SystemName}/{controllerName}/{actionName}.cshtml";
        }

        public static string HtmlName(string actionName)
        {
            return $"~/html/{ApplicationHelper.SystemName}/{actionName}.html";
        }

        /// <summary>
        /// 回傳錯誤頁
        /// </summary>
        /// <returns></returns>
        protected ActionResult ErrorPage()
        {
            return Redirect("ErrorPage.html");
        }

        /// <summary>
        /// 顯示alert訊息
        /// </summary>
        protected void SetAlertMessage(string message, AlertType type)
        {
            TempData["AlertMessage"] = message;
            TempData["AlertType"] = type.ToString();
        }

        protected void ClearSlertMessage()
        {
            TempData["AlertMessage"] = null;
            TempData["AlertType"] = null;
        }
    }
}