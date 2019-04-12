using MyTool.Services;
using MyTool.ViewModels;
using System.Web;
using System.Web.Mvc;
using WabMaker.Web.Helpers;

namespace WabMaker.Web.Controllers
{
    public class HomeController : BaseController
    {
        /// <summary>
        /// 首頁
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(ApplicationHelper.SystemName))
            {
                return ErrorPage();
            }

            if (ApplicationHelper.IsApiHtml)
            {
                return new FilePathResult(HtmlName("Index"), "text/html");
            }
            else
            {
                return View(ViewName("Home", "Index"));
            }
        }

        /// <summary>
        /// 搜尋結果
        /// </summary>
        /// <param name="q">The keyword.</param>
        /// <returns></returns>
        public ActionResult Search(string q, SearchType t = SearchType.all)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return ErrorPage();
            }

            ViewBag.Type = t;

            return View(ViewName("Home", "Search"), model: q);
        }

    }
}