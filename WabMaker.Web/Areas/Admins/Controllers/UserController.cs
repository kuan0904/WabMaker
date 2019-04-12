using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// 登入登出、管理員帳號管理
    /// </summary>
    public class UserController : AuthBaseController
    {
        private UserService service = new UserService();
        private ClientService clientService = new ClientService();
        private DepartmentService departmentService = new DepartmentService();

        public UserController()
        {
            if (SessionManager.Client != null)
            {
                service.ClientID = SessionManager.Client.ID;
                service.ClientCode = SessionManager.Client.ClientCode;

                departmentService.ClientID = SessionManager.Client.ID;
                departmentService.IsSuperManager = SessionManager.IsSuperManager;
            }
        }

        #region 登入登出

        [AllowAnonymous]
        public ActionResult Login()
        {
            //已經登入導向首頁
            if (SessionManager.UserID != Guid.Empty &&
                SessionManager.AccountType == AccountType.Admin &&
                //(不分大小寫)
                RouteHelper.Get(RouteName.AdminRoute).ToLower() == SessionManager.Client.AdminRoute.ToLower())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string account, string password, string url = "")
        {
            SessionManager.RemoveAll();

            //設定Client
            if (SetClient())
            {
                //比對登入帳密
                service.ClientID = SessionManager.Client.ID;

                var result = service.CheckLogin(new LoginViewModel
                {
                    Account = account,
                    Password = password,
                    IP = _Web.GetIp(),
                    AccountType = AccountType.Admin,
                    LoginType = LoginType.Account
                });

                if (result.IsSuccess)
                {
                    SessionManager.UserID = result.Data.ID;
                    SessionManager.UserName = result.Data.Name;
                    SessionManager.AccountType = AccountType.Admin;                 
                    SessionManager.DepartmentID = result.Data.DepartmentID;

                    departmentService.ClientID = SessionManager.Client.ID;
                    SessionManager.DepartmentIDs = departmentService.GetSelectList(SessionManager.DepartmentID)
                                                        .Select(x => new Guid(x.Value)).ToList();
                    SessionManager.IsSuperManager = (result.Data.ID == Setting.SuperManagerID);
                   
                    //menu
                    var menuService = new MenuService()
                    {
                        ClientID = SessionManager.Client.ID,
                        IsSuperManager = SessionManager.IsSuperManager
                    };
                    var rolePermissions = SessionManager.IsSuperManager ? new List<RolePermissionModel>()
                                                                       : service.GetRolePermissions(result.Data.ID, AccountType.Admin);
                    SessionManager.MemberLevel = SessionManager.IsSuperManager ? MemberLevel.Highest : service.GetMemberLevel(result.Data.ID, AccountType.Admin);
                    SessionManager.RolePermissions = rolePermissions;
                    SessionManager.MenuTree = menuService.GetTrees(null, MenuType.Admin, onlyMenu: true, rolePermissions: rolePermissions, memberLevel: SessionManager.MemberLevel);

                    if (!string.IsNullOrEmpty(url))
                    {
                        return Redirect(url);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }

                SessionManager.RemoveAll();
                SetAlertMessage(result.Message, AlertType.error);
            }//end SetClient

            return View();
        }

        private bool SetClient()
        {
            var clientRoute = RouteHelper.Get(RouteName.AdminRoute);
            var result = clientService.GetByAdminRoute(clientRoute);

            if (result.IsSuccess)
            {
                SessionManager.Client = result.Data;
            }
            else
            {
                SetAlertMessage(result.Message, AlertType.error);
            }

            return result.IsSuccess;
        }

        [AllowAnonymous]
        public ActionResult LogOut()
        {
            SessionManager.RemoveAll();
            SetAlertMessage(SystemMessage.LogoutSuccess, AlertType.success);

            return RedirectToAction("Login");
        }
        #endregion

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [PartialCheck]
        public ActionResult GetPageList(PageParameter param, UserFilter filter)
        {
            param.SortColumn = SortColumn.CreateTime;
            param.IsDescending = true;

            var result = service.GetList(param, filter, new List<int> { (int)AccountType.Admin });
            return PartialView("_PageList", result);
        }

        [PartialCheck]
        public ActionResult Create()
        {
            var model = new UserViewModel
            {
                User = new mgt_User(),
                RoleCheckList = service.GetRoleCheckList(null),
                DepartmentSelectList = departmentService.GetSelectList()
            };

            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PartialCheck]
        public ActionResult Create(UserViewModel model)
        {
            //帳號類型&狀態
            model.User.AccountType = (int)AccountType.Admin;
            model.User.UserStatus = (int)UserStatus.Enabled;
            model.LoginType = LoginType.Account;

            model.RoleIDs = _Model.ToCheckedGuid(model.RoleCheckList); 
            var result = service.Create(model, SessionManager.UserID);
            return Json(result);
        }

        [PartialCheck]
        public ActionResult Update(Guid id)
        {
            var model = service.GetView(id);
            model.RoleCheckList = service.GetRoleCheckList(id);
            model.DepartmentSelectList = departmentService.GetSelectList(selectedID: model.User.ID);

            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PartialCheck]
        public ActionResult Update(UserViewModel model)
        {
            model.RoleIDs = _Model.ToCheckedGuid(model.RoleCheckList);
            model.LoginType = LoginType.Account;

            var result = service.UpdateAdmin(model, SessionManager.UserID);
            return Json(result);
        }

        //todo editMy 修改自己的密碼

        [PartialCheck]
        public ActionResult Delete(Guid id)
        {
            CiResult result = service.Delete(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}