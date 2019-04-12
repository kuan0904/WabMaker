using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using WabMaker.Web.Authorize;
using WabMaker.Web.Helpers;
using WabMaker.Web.MainService;
using WebMaker.BLL.Helpers;
using WebMaker.BLL.Services;
using WebMaker.Entity.Models;
using WebMaker.Entity.ViewModels;
using static MyTool.Tools.MailTool;

namespace WabMaker.Web.Areas.Admins.Controllers
{
    /// <summary>
    /// 會員管理
    /// </summary>
    public class MemberController : AuthBaseController
    {
        private UserService service = new UserService();
        private RoleService roleService = new RoleService();

        public MemberController()
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
            ViewBag.RoleSelectList = roleService.GetSelectList(AccountType.Member);

            return View();
        }

        [PartialCheck]
        public ActionResult GetPageList(PageParameter param, UserFilter filter)
        {
            param.SortColumn = SortColumn.CreateTime;
            param.IsDescending = true;

            var result = service.GetList(param, filter, new List<int> { (int)AccountType.None, (int)AccountType.Member });
            return PartialView("_PageList", result);
        }

        [PartialCheck]
        public ActionResult View(Guid id)
        {
            var model = service.GetView(id);

            return PartialView("_View", model);
        }

        [PartialCheck]
        public ActionResult Log(Guid id)
        {
            var model = service.GetView(id, dataLevel: DataLevel.All);

            return PartialView("_Log", model);
        }


        /// <summary>
        /// 重寄驗證信
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task<ActionResult> SendConfirmMail(Guid id)
        {
            var result = new CiResult<mgt_User>();
            var user = service.Get(id);

            //check mail
            var model = new SendEmailViewModel
            {
                Email = user.Email,
                SystemMailType = SystemMailType.ConfirmEmail
            };
            result = service.SendValidCodeCheck(model);

            //send
            if (result.IsSuccess)
            {
                var mailService = new MailService(SessionManager.Client.ID);
                var mailContent = new ReplaceMailContent
                {
                    UserName = result.Data.Name,
                    UserEmail = model.Email
                };
                var mailResult = await mailService.SendEmail(result.Data.ID, mailContent, model.SystemMailType, model.ValidType, fromFn: "Admin_SendConfirmMail");
                return Json(mailResult, JsonRequestBehavior.AllowGet);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region Role

        /// <summary>
        /// 角色(身分)編輯列表
        /// </summary>
        /// <param name="userID">The identifier.</param>
        /// <returns></returns>
        [PartialCheck]
        public ActionResult RoleList(Guid userID)
        {
            var model = service.GetView(userID);

            return PartialView("_RoleList", model);
        }

        [PartialCheck]
        public ActionResult RoleGet(Guid id)
        {
            var model = service.GetRoleRelation(id);

            return PartialView("_RoleGet", model);
        }

        [PartialCheck]
        public ActionResult RoleEdit(Guid userID, Guid? id)
        {
            var model = new mgt_UserRoleRelation { UserID = userID };

            if (id != null)
            {
                model = service.GetRoleRelation(id.Value);
            }
          
            ViewBag.RoleSelectList = roleService.GetSelectList(AccountType.Member, model.RoleID);

            return PartialView("_RoleEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoleEdit(mgt_UserRoleRelation model, bool isCreate)
        {
            var result = isCreate ?
                service.CreateUserMemberRoles(model, SessionManager.UserID)
                : service.UpdateUserMemberRoles(model, SessionManager.UserID);

            var dataResult = new CiResult<string>();
            dataResult.IsSuccess = result.IsSuccess;
            dataResult.Message = result.Message;
            dataResult.Data = $"?id={model.ID}";

            return Json(dataResult);
        }

        [PartialCheck]
        //[HttpPost]
        public ActionResult RoleDelete(Guid id)
        {
            var result = service.DeleteUserMemberRoles(id, SessionManager.UserID);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

    }
}