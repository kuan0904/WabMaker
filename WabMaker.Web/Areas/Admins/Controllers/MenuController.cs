using MyTool.Enums;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WabMaker.Web.Authorize;
using WabMaker.Web.Helpers;
using WebMaker.BLL.Helpers;
using WebMaker.BLL.Services;
using WebMaker.Entity.Models;
using WebMaker.Entity.ViewModels;

namespace WabMaker.Web.Areas.Admins.Controllers
{
    /// <summary>
    /// 選單管理 (所有Client共用)
    /// </summary>
    public class MenuController : AuthBaseController
    {
        private MenuService service = new MenuService();

        public MenuController()
        {
            //if (SessionManager.Client != null)
            //{
            //    service.ClientID = SessionManager.Client.ID;
            //    service.ClientCode = SessionManager.Client.ClientCode;
            //}
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [PartialCheck]
        public ActionResult _Tree(MenuType type)
        {
            var tree = service.GetTrees(null, type);
            return PartialView(tree);
        }

        [PartialCheck]
        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _TreeMenu()
        {
            var tree = SessionManager.MenuTree;//.service.GetTrees(null, type, onlyMenu: true);
            return PartialView(tree);
        }

        [PartialCheck]
        public ActionResult GetNode(Guid? id = null)
        {
            var node = new TreeViewModel();
            if (id != null)
            {
                var model = service.Get(id.Value);
                node.ID = model.ID;
                node.Name = model.Name;
            }

            return PartialView("_TreeNode", node);
        }

        [PartialCheck]
        public ActionResult Create()
        {
            var model = new mgt_Menu();
            return PartialView("_Edit", model);
        }
                
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(mgt_Menu model)
        {         
            var result = service.Create(model);
            if (result.IsSuccess)
            {
                //remove all cache
                var cacheHelper = new CacheHelper();
                cacheHelper.RemoveAll();
            }

            return Json(result);
        }

        [PartialCheck]
        public ActionResult Update(Guid id)
        {
            var model = service.Get(id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(mgt_Menu model)
        {        
            var result = service.Update(model);
            if (result.IsSuccess)
            {
                //remove all cache
                var cacheHelper = new CacheHelper();
                cacheHelper.RemoveAll();
            }

            return Json(result);
        }

        [HttpPost]
        public ActionResult Delete(Guid id)
        {
            var result = service.Delete(id);
            if (result.IsSuccess)
            {
                //remove all cache
                var cacheHelper = new CacheHelper();
                cacheHelper.RemoveAll();
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}