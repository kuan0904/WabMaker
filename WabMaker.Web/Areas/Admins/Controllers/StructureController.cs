using MyTool.ViewModels;
using System;
using System.Web;
using System.Web.Mvc;
using WabMaker.Web.Authorize;
using WebMaker.BLL.Helpers;
using WebMaker.BLL.Services;
using WebMaker.Entity.Models;
using WebMaker.Entity.ViewModels;

namespace WabMaker.Web.Areas.Admins.Controllers
{
    /// <summary>
    /// 結構類型(文章內容組成)
    /// </summary>
    public class StructureController : AuthBaseController
    {
        private StructureService service = new StructureService();
     

        public StructureController()
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
        public ActionResult _Tree()
        {
            var tree = service.GetTrees(null);
            return PartialView(tree);
        }

        [PartialCheck]
        public ActionResult GetNode(Guid? id = null)
        {
            var node = new TreeViewModel();
            if (id != null)
            {
                var item = service.Get(id.Value);
                node = service.ItemToTreeNode(item);
            }

            return PartialView("_TreeNode", node);
        }

        //public ActionResult GetPageList(PageParameter param)
        //{
        //    param.IsPaged = false;
        //    param.SortColumn = "Sort";

        //    var result = service.GetList(param);
        //    return PartialView("_PageList", result);
        //}

        [PartialCheck]
        public ActionResult Create()
        {
            var model = new cms_Structure();
            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(cms_Structure model, Guid? ParentID)
        {
            var result = service.Create(model, ParentID);
            return Json(result);
        }

        [PartialCheck]
        public ActionResult Update(Guid id)
        {
            var model = service.Get(id);

            // Html Decode       
            model.Template = HttpUtility.HtmlDecode(model.Template);

            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(cms_Structure model)
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