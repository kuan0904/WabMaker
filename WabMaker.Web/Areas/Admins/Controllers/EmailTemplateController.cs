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
    /// 郵件樣板(通知信管理)
    /// </summary>
    public class EmailTemplateController : AuthBaseController
    {
        private EmailTemplateService service = new EmailTemplateService();
     

        public EmailTemplateController()
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
            var model = new cms_EmailTemplate();
            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(cms_EmailTemplate model)
        {
            model.UpdateUser = SessionManager.UserID;
            var result = service.Create(model);
            return Json(result);
        }

        [PartialCheck]
        public ActionResult Update(Guid id)
        {
            var model = service.Get(id);

            // Html Decode       
            model.Content = HttpUtility.HtmlDecode(model.Content);

            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(cms_EmailTemplate model)
        {
            model.UpdateUser = SessionManager.UserID;
            var result = service.Update(model);
            return Json(result);
        }

        [HttpPost]
        public ActionResult Delete(Guid id)
        {
            var result = service.Delete(id, SessionManager.UserID);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}