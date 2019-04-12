using MyTool.Enums;
using MyTool.Services;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WabMaker.Web.Helpers;
using WabMaker.Web.MainService;
using WabMaker.Web.WebViewModels;
using WebMaker.BLL.Helpers;
using WebMaker.BLL.Services;
using WebMaker.Entity.ViewModels;

namespace WabMaker.Web.Controllers
{
    public class ItemController : BaseController
    {
        //private ItemService service = new ItemService();
        //private StructureService structureService = new StructureService();
        private ItemMainService mainServie = new ItemMainService();

        //public ItemController()
        //{
        //    service.ClientID = ApplicationHelper.ClientID;
        //    structureService.ClientID = ApplicationHelper.ClientID;
        //}

        /// <summary>
        /// 選單
        /// </summary>
        /// <param name="position">版位</param>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult Menu(MenuPosition position, string style = "")
        {
            var result = mainServie.GetMenuByCache(position);
            ViewBag.style = style;

            return PartialView(ViewName("Item", "Menu_" + position.ToString()), result.Data);
        }


        /// <summary>
        /// Item (Banner、內容、列表)
        /// </summary>
        /// <param name="routeName"></param>
        /// <param name="specViewName">指定特殊ViewName(default:structure)</param>
        /// <returns></returns>
        public ActionResult Get(string routeName, string specViewName = "")
        {
            if (ApplicationHelper.IsApiHtml)
            {
                return new FilePathResult(HtmlName("detail"), "text/html");
            }

            string viewName = "";
            var result = mainServie.Get(routeName, ref viewName);

            if (!string.IsNullOrEmpty(specViewName)) viewName = specViewName;

            if (!result.IsSuccess || string.IsNullOrEmpty(viewName))
                return ErrorPage();//404
            else
                return View(ViewName("Item", viewName), result.Data);
        }

        /// <summary>
        /// html用列表
        /// </summary>
        /// <param name="routeName">Name of the route.</param>
        /// <returns></returns>
        public ActionResult List(string routeName)
        {
            if (ApplicationHelper.IsApiHtml)
            {
                return new FilePathResult(HtmlName("list"), "text/html");
            }
            return ErrorPage();//404
        }

        /// <summary>
        /// 預覽Item
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>      
        [HttpPost]
        public ActionResult Preview(ItemViewModel data)
        {
            if (SessionManager.UserID != Guid.Empty)
            {
                //service.ClientID = SessionManager.Client.ID;
                //structureService.ClientID = SessionManager.Client.ID;

                string viewName = "";
                var result = mainServie.ShowItem(data, ref viewName);

                if (!result.IsSuccess || string.IsNullOrEmpty(viewName))
                    return ErrorPage();//404
                else
                    return View(ViewName("Item", viewName), result.Data);
            }
            return ErrorPage();
        }

        /// <summary>
        /// 分頁 for ajax
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>      
        public ActionResult GetPageList(ItemPageModel model)
        {
            if (!Request.IsAjaxRequest() && !ApplicationHelper.IsLocal)
            {
                return ErrorPage(); //404
            }

            return Partial(model);
        }

        /// <summary>
        /// 列表Partial View (最新、熱門、Banner)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult Partial(ItemPageModel model)
        {
            //search必須有關鍵字
            if (model.ViewName == "Search_Page" && string.IsNullOrWhiteSpace(model.SearchString))
            {
                return ErrorPage(); //404
            }

            var result = mainServie.GetPartialListByCache(model);

            return PartialView(ViewName("Item", model.ViewName), result.Data);
        }
    }
}