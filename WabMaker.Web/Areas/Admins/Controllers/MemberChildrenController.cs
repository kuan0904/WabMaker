using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
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
    /// 會員孩子管理
    /// </summary>
    public class MemberChildrenController : AuthBaseController
    {
        private UserService service = new UserService();
        private RoleService roleService = new RoleService();

        public MemberChildrenController()
        {
            if (SessionManager.Client != null)
            {
                service.ClientID = SessionManager.Client.ID;
                service.ClientCode = SessionManager.Client.ClientCode;
                roleService.ClientID = SessionManager.Client.ID;
            }
        }

        [HttpGet]
        public ActionResult Index()
        {
            //ViewBag.RoleSelectList = roleService.GetSelectList(AccountType.Member);

            return View();
        }

        [PartialCheck]
        public ActionResult GetPageList(PageParameter param, UserFilter filter)
        {
            param.SortColumn = SortColumn.CreateTime;
            param.IsDescending = true;

            var result = service.GetChildList(param, filter);
            return PartialView("_PageList", result);
        }

        [PartialCheck]
        public ActionResult Update(Guid id)
        {
            var model = service.GetChild(id);

            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PartialCheck]
        public ActionResult Update(mgt_UserProfile model, string UserMail = "")
        {      
            var result = service.UpdateChild(model, UserMail);
            if (result.IsSuccess)
            {
               
            }

            return Json(result);
        }
        //[PartialCheck]
        //public ActionResult Log(Guid id)
        //{
        //    var model = service.GetView(id, dataLevel: DataLevel.All);

        //    return PartialView("_Log", model);
        //}


        ///// <summary>
        ///// 角色(身分)編輯列表
        ///// </summary>
        ///// <param name="userID">The identifier.</param>
        ///// <returns></returns>
        //[PartialCheck]
        //public ActionResult RoleList(Guid userID)
        //{
        //    var model = service.GetView(userID);

        //    return PartialView("_RoleList", model);
        //}

        //[PartialCheck]
        //public ActionResult RoleGet(Guid id)
        //{
        //    var model = service.GetRoleRelation(id);

        //    return PartialView("_RoleGet", model);
        //}

        //[PartialCheck]
        //public ActionResult RoleEdit(Guid userID, Guid? id)
        //{
        //    var model = new mgt_UserRoleRelation { UserID = userID };

        //    if (id != null)
        //    {
        //        model = service.GetRoleRelation(id.Value);
        //    }

        //    ViewBag.RoleSelectList = roleService.GetSelectList(AccountType.Member, model.RoleID);

        //    return PartialView("_RoleEdit", model);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult RoleEdit(mgt_UserRoleRelation model, bool isCreate)
        //{
        //    var result = isCreate ?
        //        service.CreateUserMemberRoles(model, SessionManager.UserID)
        //        : service.UpdateUserMemberRoles(model, SessionManager.UserID);

        //    var dataResult = new CiResult<string>();
        //    dataResult.IsSuccess = result.IsSuccess;
        //    dataResult.Message = result.Message;
        //    dataResult.Data = $"?id={model.ID}";

        //    return Json(dataResult);
        //}

        //[PartialCheck]
        ////[HttpPost]
        //public ActionResult RoleDelete(Guid id)
        //{
        //    var result = service.DeleteUserMemberRoles(id, SessionManager.UserID);

        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}
    }
}