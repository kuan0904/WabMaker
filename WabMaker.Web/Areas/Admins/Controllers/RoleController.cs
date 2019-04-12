using MyTool.ViewModels;
using System;
using System.Web.Mvc;
using WabMaker.Web.Authorize;
using WebMaker.BLL.Helpers;
using WebMaker.BLL.Services;
using WebMaker.Entity.Models;
using WebMaker.Entity.ViewModels;

namespace WabMaker.Web.Areas.Admins.Controllers
{
    /// <summary>
    /// 權限角色管理
    /// </summary>
    public class RoleController : AuthBaseController
    {
        private RoleService service = new RoleService();
     
        public RoleController()
        {
            if (SessionManager.Client != null)
            {
                service.ClientID = SessionManager.Client.ID;
                service.ClientCode = SessionManager.Client.ClientCode;
            }
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [PartialCheck]
        public ActionResult GetPageList(PageParameter param)
        {     
            var result = service.GetList(param);
            return PartialView("_PageList", result);
        }

        [PartialCheck]
        public ActionResult Create()
        {
            var model = new RoleViewModel
            {
                Role = new mgt_Role(),
                MenuCheckList = service.GetMenuCheckList(null)
            };

            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RoleViewModel model)
        {
            var result = service.Create(model);
            return Json(result);
        }

        [PartialCheck]
        public ActionResult Update(Guid id)
        {
            var model = new RoleViewModel
            {
                Role = service.Get(id),
                MenuCheckList = service.GetMenuCheckList(id),
            };

            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(RoleViewModel model)
        {
            var result = service.Update(model);
            return Json(result);
        }

        [HttpPost]
        public ActionResult Delete(Guid id)
        {
            var result = service.Delete(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}