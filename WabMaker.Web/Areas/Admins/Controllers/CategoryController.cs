using MyTool.Enums;
using MyTool.ViewModels;
using System;
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
    /// Item-分類管理
    /// </summary>
    public class CategoryController : AuthBaseController
    {
        private ItemService service = new ItemService();
        private StructureService structureService = new StructureService();

        public CategoryController()
        {
            if (SessionManager.Client != null)
            {
                service.ClientID = SessionManager.Client.ID;
                service.ClientCode = SessionManager.Client.ClientCode;
                structureService.ClientID = SessionManager.Client.ID;
                structureService.ClientCode = SessionManager.Client.ClientCode;
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
            var tree = service.GetTrees(null, new ItemTreeFilter
            {
                ItemType = ItemType.Category,
                LangType = (LanguageType)SessionManager.Client.DefaultLanguage,
                JoinStructureTree = true
            });
            return PartialView(tree);
        }

        [PartialCheck]
        public ActionResult GetNode(Guid? id = null)
        {
            var node = new TreeViewModel();

            if (id != null)
            {
                var item = service.Get(id.Value);
                node = service.TreeNode(item);
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
        public ActionResult Create(Guid? structureID = null)
        {
            var model = new EditItemViewModel
            {
                Item = new cms_Item(),
                ItemLanguage = new cms_ItemLanguage(),//不指定語系
                SelectList = structureService.GetSelectList(ItemType.Category, selectFirstLevel: true)
            };

            //指定Structure(第二層之後)
            if (structureID != null)
            {
                var structure = structureService.Get(structureID.Value);
                model.Item.cms_Structure = structure;
                model.Item.StructureID = structure.ID;
                model.ItemLanguage.LanguageType = structure.DefaultLanguage;
            }

            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PartialCheck]
        public ActionResult Create(EditItemViewModel model)
        {
            model.Item.UpdateUser = SessionManager.UserID;

            //新增時,語系為Structure預設語系
            var structure = structureService.Get(model.Item.StructureID);
            model.ItemLanguage.LanguageType = structure.DefaultLanguage;

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
        public ActionResult Update(Guid id, LanguageType langType)
        {
            var dataModel = service.GetView(id, langType);       
            var model = new EditItemViewModel(dataModel);
            model.SelectList = structureService.GetSelectList(ItemType.Category, model.Item.StructureID, true);

            //語系可null
            if (model.ItemLanguage == null)
            {
                model.ItemLanguage = new cms_ItemLanguage { LanguageType = (int)langType };
            }

            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PartialCheck]
        public ActionResult Update(EditItemViewModel model)
        {
            model.Item.UpdateUser = SessionManager.UserID;

            var result = service.Update(model, onlySubject:true);
            if (result.IsSuccess)
            {
                //remove all cache
                var cacheHelper = new CacheHelper();
                cacheHelper.RemoveAll();
            }

            return Json(result);
        }

        [PartialCheck]
        [HttpPost]
        public ActionResult Delete(Guid id)
        {
            var result = service.Delete(id, SessionManager.UserID);
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