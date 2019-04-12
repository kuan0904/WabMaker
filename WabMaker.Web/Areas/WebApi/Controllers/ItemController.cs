using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;
using WabMaker.Web.MainService;
using WabMaker.Web.WebViewModels;

namespace WabMaker.Web.Areas.WebApi.Controllers
{
    /// <summary>
    /// 網站內容
    /// </summary>
    /// <seealso cref="ApiController" />
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [System.Web.Http.RoutePrefix("api")]
    public class ItemController : BaseApiController
    {
        private ItemMainService mainServie = new ItemMainService();
      
        /// <summary>
        /// 選單
        /// </summary>
        /// <param name="position">版位</param>
        /// <returns></returns>
        [System.Web.Http.Route("Menu")]
        [System.Web.Http.HttpGet]
        public CiResult<List<TreeWebViewModel>> Menu(MenuPosition position)
        {
            var result = mainServie.GetMenuByCache(position);
            return result;
        }

        /// <summary>
        /// 內頁
        /// </summary>
        /// <param name="routeName">路由名稱</param>
        /// <returns></returns>
        [System.Web.Http.Route("Detail")]
        [System.Web.Http.HttpGet]
        public CiResult<ItemWebViewModel> Detail(string routeName)
        {
            var result = mainServie.GetDetail(routeName);
            return result;
        }

        /// <summary>
        /// 列表資訊(本體+分頁參數)
        /// </summary>
        /// <param name="routeName">路由名稱</param>
        /// <returns></returns>
        [System.Web.Http.Route("ListInfo")]
        [System.Web.Http.HttpGet]
        public CiResult<ItemListModel> ListInfo(string routeName)
        {
            var result = mainServie.GetList(routeName);
            return result;
        }

        /// <summary>
        /// 分頁/部分列表
        /// </summary>
        /// <param name="model">PageList</param>
        /// <returns></returns>
        [System.Web.Http.Route("PartialList")]
        [System.Web.Http.HttpPost]
        public CiResult<ItemPageResult> PartialList(ItemPageModel model)
        {
            // 不允許cache
            var result = mainServie.GetPartialList(model);

            return result;
        }
    }
}