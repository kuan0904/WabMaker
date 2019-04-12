using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.Tools;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WabMaker.Web.Authorize;
using WabMaker.Web.Helpers;
using WabMaker.Web.MainService;
using WebMaker.BLL.Helpers;
using WebMaker.BLL.Services;
using WebMaker.Entity.Models;
using WebMaker.Entity.ViewModels;
using static MyTool.Tools.MailTool;
using static WebMaker.Entity.ViewModels.OrderViewModel;

namespace WabMaker.Web.Controllers
{
    /// <summary>
    /// 會員專區
    /// </summary>
    [MemberAuthorize]
    public class MemberController : BaseController
    {
        private UserService service = new UserService();
        private OrderService orderService = new OrderService();
        private StructureService structureService = new StructureService();

        public MemberController()
        {
            service.ClientID = ApplicationHelper.ClientID;
            service.ClientCode = ApplicationHelper.ClientCode;
            orderService.ClientID = ApplicationHelper.ClientID;
            structureService.ClientID = ApplicationHelper.ClientID;
        }

        #region 登入登出

        ///<summary>
        /// 產生驗證碼
        /// </summary>    
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult CaptchaImage()
        {
            var verifyCode = new VerifyCodeTool();

            var code = verifyCode.RandomPassword();
            SessionManager.Captcha = code;

            return File(verifyCode.Create(code), "image/Jpeg"); ;
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            //已經登入導向首頁
            if (SessionManager.UserID != Guid.Empty &&
                SessionManager.AccountType == AccountType.Member)
            {
                return RedirectToAction("Index", "Member");
            }

            if (ApplicationHelper.LoginStyle == LoginStyle.Popup)
            {
                return RedirectToAction("Index", "Home", new { url = "#" });
            }
            else
            {
                return View(ViewName("Member", "0_Login"));
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string url = "")
        {
            var errorMessage = "";

            //驗證碼
            if (string.IsNullOrWhiteSpace(model.Captcha) || SessionManager.Captcha != model.Captcha)
            {
                errorMessage = SystemMessage.CaptchaError;
            }

            //比對登入帳密         
            model.LoginType = LoginType.Email;

            return LoginHelper(model, url, errorMessage, "");
        }

        /// <summary>
        /// 一般登入/外部登入共用
        /// </summary>
        /// <param name="model">The model.</param>
        private ActionResult LoginHelper(LoginViewModel model, string url, string errorMessage, string successMessage)
        {
            var result = new CiResult<mgt_User>();

            SessionManager.RemoveAll();
            model.IP = _Web.GetIp();
            model.AccountType = AccountType.Member;

            if (!string.IsNullOrEmpty(errorMessage))
            {
                result.Message = errorMessage;
            }
            else
            {
                result = service.CheckLogin(model);
                if (result.IsSuccess)
                {
                    SessionManager.UserID = result.Data.ID;
                    SessionManager.UserName = result.Data.Name;
                    SessionManager.AccountType = AccountType.Member;

                    //menu
                    var menuService = new MenuService() { ClientID = ApplicationHelper.ClientID };
                    var rolePermission = service.GetRolePermissions(result.Data.ID, AccountType.Member);
                    SessionManager.MenuTree = menuService.GetTrees(null, MenuType.Mmeber, onlyMenu: true, rolePermissions: rolePermission);
                }
            }


            //success
            if (result.IsSuccess)
            {
                if (!string.IsNullOrEmpty(successMessage))
                {
                    SetAlertMessage(successMessage, AlertType.success);
                }

                if (!string.IsNullOrEmpty(url))
                {
                    return Redirect(url);
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }

            //fail          
            SetAlertMessage(result.Message, AlertType.error);
            SessionManager.RemoveAll();
            if (model.LoginType == LoginType.Facebook)
            {
                model.Account = "";
            }

            if (ApplicationHelper.LoginStyle == LoginStyle.Popup)
            {
                return RedirectToAction("Index", "Home", new { url = "#" });
            }
            else
            {
                return View(ViewName("Member", "0_Login"), model);
            }
        }

        [AllowAnonymous]
        public ActionResult LogOut()
        {
            SessionManager.RemoveAll();
            SetAlertMessage(SystemMessage.LogoutSuccess, AlertType.success);

            if (ApplicationHelper.LoginStyle == LoginStyle.Popup)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        #endregion

        #region 註冊、驗證、忘記密碼

        [AllowAnonymous]
        public ActionResult SignUp(ExternalLoginViewModel model)
        {
            var resultModel = new SignUpViewModel();

            //from外部登入
            if (!string.IsNullOrEmpty(model.ExternalKey))
            {
                resultModel.Email = model.Email;
                resultModel.Name = model.UserName;
                resultModel.ExternalType = model.ExternalType;
                if (SessionManager.ExternalLogin == null)
                {
                    return RedirectToAction("Login");
                }
            }

            return View(ViewName("Member", "1_SignUp"), resultModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<ActionResult> SignUp(SignUpViewModel model)
        {
            if (!Request.IsAjaxRequest())
            {
                return ErrorPage(); //404
            }
            var result = new CiResult<Guid>();

            //驗證碼 (外部登入不用)
            if (model.ExternalType != null)
            {
                if (SessionManager.ExternalLogin == null)
                {
                    return ErrorPage(); //404
                }
            }
            else if (string.IsNullOrWhiteSpace(model.Captcha) || SessionManager.Captcha != model.Captcha)
            {
                result.Message = SystemMessage.CaptchaError;
            }

            //註冊Email會員
            if (string.IsNullOrEmpty(result.Message))
            {
                var newData = new UserViewModel
                {
                    User = new mgt_User
                    {
                        Email = model.Email,
                        Password = model.Password,
                        Name = model.Name,
                        Phone = model.Phone,
                        Address = model.Address,
                        //帳號類型&狀態
                        AccountType = (int)AccountType.Member,
                        UserStatus = (int)UserStatus.OnVerify,
                    },
                    LoginType = model.ExternalType != null ? (LoginType)model.ExternalType : LoginType.Email
                };

                if (SessionManager.ExternalLogin != null)
                {
                    newData.UserExternalLogin = new mgt_UserExternalLogin
                    {
                        ExternalType = (int)SessionManager.ExternalLogin.ExternalType,
                        ExternalKey = SessionManager.ExternalLogin.ExternalKey
                    };
                }

                result = service.Create(newData, isExternalBinding: model.Email == SessionManager.ExternalLogin?.Email);
            }

            //寄Email驗證信  
            if (result.IsSuccess)
            {
                //clear
                SessionManager.FBstate = "";
                SessionManager.Captcha = "";
                SessionManager.ExternalLogin = null;

                //綁定成功, 不用重送驗證信
                if (result.Message != SystemMessage.BindingSuccess)
                {
                    var mailService = new MailService(ApplicationHelper.ClientID);
                    var mailContent = new ReplaceMailContent
                    {
                        UserName = model.Name,
                        UserEmail = model.Email
                    };
                    var mailResult = await mailService.SendEmail(result.Data, mailContent, SystemMailType.ConfirmEmail, ValidType.ConfirmEmail, fromFn: "SignUp");
                    return Json(mailResult);
                }
            }

            return Json(result);
        }

        [AllowAnonymous]
        public ActionResult ReSend()
        {
            var model = new SendEmailViewModel
            {
                SystemMailType = SystemMailType.ConfirmEmail,
                ValidType = ValidType.ConfirmEmail
            };
            ViewBag.Title = "重寄驗證信";
            return View(ViewName("Member", "2_SendValidCode"), model);
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            var model = new SendEmailViewModel
            {
                SystemMailType = SystemMailType.ForgotPassword,
                ValidType = ValidType.ForgotPassword
            };
            ViewBag.Title = "忘記密碼";
            return View(ViewName("Member", "2_SendValidCode"), model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult ResetPassword(string Password)
        {
            if (SessionManager.UserID == Guid.Empty)
                return ErrorPage(); //404

            var result = service.UpdatePassword(SessionManager.UserID, Password);
            //重新登入
            SessionManager.RemoveAll();

            return Json(result);
        }

        /// <summary>
        /// post重寄驗證信、忘記密碼
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<ActionResult> SendValidCode(SendEmailViewModel model)
        {
            var result = new CiResult<mgt_User>();

            //驗證碼
            if (string.IsNullOrWhiteSpace(model.Captcha) || SessionManager.Captcha != model.Captcha)
            {
                result.Message = SystemMessage.CaptchaError;
            }

            if (string.IsNullOrEmpty(result.Message))
            {
                result = service.SendValidCodeCheck(model);
                if (result.IsSuccess)
                {
                    var mailService = new MailService(ApplicationHelper.ClientID);
                    var mailContent = new ReplaceMailContent
                    {
                        UserName = result.Data.Name,
                        UserEmail = model.Email
                    };
                    var mailResult = await mailService.SendEmail(result.Data.ID, mailContent, model.SystemMailType, model.ValidType, fromFn: "SendValidCode");
                    return Json(mailResult);
                }
            }

            return Json(result);
        }

        /// <summary>
        /// 重寄驗證信(給自己)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> SendValidCodeSelf()
        {
            var user = service.Get(SessionManager.UserID);
            var model = new SendEmailViewModel
            {
                Email = user.Email,
                SystemMailType = SystemMailType.ConfirmEmail,
                ValidType = ValidType.ConfirmEmail
            };

            var result = service.SendValidCodeCheck(model);
            if (result.IsSuccess)
            {
                var mailService = new MailService(ApplicationHelper.ClientID);
                var mailContent = new ReplaceMailContent
                {
                    UserName = result.Data.Name,
                    UserEmail = model.Email
                };
                var mailResult = await mailService.SendEmail(result.Data.ID, mailContent, model.SystemMailType, model.ValidType, fromFn: "SendValidCodeSelf");
                return Json(mailResult);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 驗證連結(mail link)
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Confirm(string type, string code, string key)
        {
            SessionManager.RemoveAll();

            var result = service.CheckValidCode(
                new ValidCodeViewModel
                {
                    Type = type,
                    Code = code,
                    Key = Server.UrlDecode(key)
                });

            //UserID 用 session傳遞
            if (result.IsSuccess && result.Data.ValidType == ValidType.ForgotPassword)
            {
                SessionManager.UserID = result.Data.UserID;
            }

            return View(ViewName("Member", "3_Confirm"), result);
        }

        /// <summary>
        /// 訂閱電子報
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult SubEpaper(SendEmailViewModel model)
        {
            var result = service.SubEpaper(model.Email);

            return Json(result);
        }

        #endregion

        #region 外部登入   

        /// <summary>
        /// Facebook導向登入網址
        /// </summary>
        /// <param name="url">The url.</param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult FacebookLogin(string url = null)
        {
            //APIKeyEmpty return error
            if (string.IsNullOrEmpty(ApplicationHelper.ApiKey?.FacebookAppId) || string.IsNullOrEmpty(ApplicationHelper.ApiKey?.FacebookAppSecret))
            {
                SetAlertMessage(SystemMessage.FacebookError, AlertType.error);
                return RedirectToAction("Login", "Member");
            }

            SessionManager.FBstate = Guid.NewGuid().ToString();
            SessionManager.ReturnUrl = url;

            // redirect_uri必須和APP裡的一致
            string targetUri = "https://www.facebook.com/v3.1/dialog/oauth?"
             + "client_id=" + ApplicationHelper.ApiKey.FacebookAppId
             + "&redirect_uri=" + RouteHelper.BaseUrl() + Url.Action("FacebookLoginCallback", "Member")
             + "&scope=email"
             + "&state=" + SessionManager.FBstate;

            return Redirect(targetUri);
        }

        /// <summary>
        /// Facebook回傳token 確認身分
        /// </summary>
        /// <param name="state">自訂代碼(用來防止跨網站偽造)</param>
        /// <param name="code">FB回傳代碼</param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult FacebookLoginCallback(string state, string code)
        {
            if (string.IsNullOrEmpty(state) || state != SessionManager.FBstate)
                return ErrorPage();

            string returnUrl = SessionManager.ReturnUrl;
            List<string> exceptRoute = new List<string> { "FacebookLoginCallback", "Confirm" };
            if (exceptRoute.Any(x => returnUrl.Contains(x)))
            {
                returnUrl = "";
            }

            SessionManager.ReturnUrl = "";
            SessionManager.FBstate = "";
            SessionManager.ExternalLogin = null;

            string errorMessage = "", successMessage = "";
            var tokenModel = new FacebookTokenModel();
            var userModel = new FacebookUserModel();
            var loginModel = new LoginViewModel();

            //1.取得access_token
            string url = "https://graph.facebook.com/v3.1/oauth/access_token?"
             + "client_id=" + ApplicationHelper.ApiKey.FacebookAppId
             + "&client_secret=" + ApplicationHelper.ApiKey.FacebookAppSecret
             + "&redirect_uri=" + RouteHelper.BaseUrl() + Url.Action("FacebookLoginCallback", "Member")
             + "&code=" + code;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                var response = (HttpWebResponse)request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream());

                string jsonStr = reader.ReadToEnd().ToString();
                tokenModel = _Json.JsonToModel<FacebookTokenModel>(jsonStr);

                reader.Close();
                response.Close();
            }
            catch (Exception)
            {
                errorMessage = "無法取得Facebook存取權";
            }

            //2.取得使用者資訊
            if (string.IsNullOrEmpty(errorMessage))
            {
                try
                {
                    url = "https://graph.facebook.com/me?fields=id,name,email&access_token=" + tokenModel.access_token;

                    var request = (HttpWebRequest)WebRequest.Create(url);
                    var response = (HttpWebResponse)request.GetResponse();
                    var reader = new StreamReader(response.GetResponseStream());

                    var jsonStr = reader.ReadToEnd().ToString();
                    userModel = _Json.JsonToModel<FacebookUserModel>(jsonStr);

                    reader.Close();
                    response.Close();
                }
                catch (Exception)
                {
                    errorMessage = "取得Facebook使用者錯誤";
                }
            }

            //3.Login/SignUp
            if (string.IsNullOrEmpty(errorMessage))
            {
                var model = new ExternalLoginViewModel
                {
                    ExternalType = ExternalType.Facebook,
                    ExternalKey = userModel.id,
                    UserName = userModel.name,
                    Email = userModel.email
                };

                //FB新註冊, 一律先到確認頁 > 送出驗證Email
                if (!service.ExistExternalLogin(model))
                {
                    SessionManager.ExternalLogin = model;
                    return RedirectToAction("SignUp", model);
                }

                //可外部登入
                loginModel = new LoginViewModel
                {
                    Account = userModel.id,
                    LoginType = LoginType.Facebook
                };
            }

            return LoginHelper(loginModel, returnUrl, errorMessage, successMessage);
        }

        #region Owin (no use)
        /*
        /// <summary>
        /// 外部登入
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <param name="externalType">Type of the external.</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(ExternalType externalType, string returnUrl)
        {
            ControllerContext.HttpContext.Session.RemoveAll();
            SessionManager.RemoveAll();
            // 要求重新導向至外部登入提供者
            return new ChallengeResult(externalType.ToString(), Url.Action("ExternalLoginCallback", "Member", new { ReturnUrl = returnUrl }));
        }

        /// <summary>
        /// 傳送其他驗證的登入訊息
        /// </summary>
        /// <param name="externalType">登入提供者</param>
        /// <param name="url">The return URL.</param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
                return RedirectToAction("Login");

            // type not exist
            ExternalType externalType;
            if (!Enum.TryParse(loginInfo.Login.LoginProvider, true, out externalType))
                return ErrorPage(); //404

            // SignUp
            var model = new ExternalLoginViewModel
            {
                ExternalType = externalType,
                ExternalKey = loginInfo.Login.ProviderKey,
                UserName = loginInfo.DefaultUserName,
                Email = loginInfo.Email
            };

            var signupResult = service.SignUpExternal(model);
            var errorMessage = signupResult.IsSuccess ? "" : signupResult.Message;

            //比對登入
            var loginModel = new LoginViewModel
            {
                Account = model.ExternalKey,
                LoginType = (LoginType)externalType
            };

            return LoginHelper(loginModel, returnUrl, errorMessage);
        }
        */
        #region Helper
        // 新增外部登入時用來當做 XSRF 保護
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
        #endregion

        #endregion

        #region 會員專區(編輯)

        /// <summary>
        /// 會員中心
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            //導向指定首頁
            if (!string.IsNullOrEmpty(ApplicationHelper.MemberIndex) && ApplicationHelper.MemberIndex != "Index")
            {
                return RedirectToAction(ApplicationHelper.MemberIndex);
            }

            return PartialView(ViewName("Member", "Index"));
        }

        /// <summary>
        /// 基本資料
        /// </summary>
        /// <returns></returns>
        public ActionResult Profile()
        {
            var model = service.GetView(SessionManager.UserID);

            return PartialView(ViewName("Member", "Profile"), model);
        }

        /// <summary>
        /// 修改基本資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Profile(UserViewModel model)
        {
            model.User.ID = SessionManager.UserID;
            var result = service.UpdateMember(model, ApplicationHelper.ProfileWithEmail, ApplicationHelper.ProfileWithPhone);

            //重寄驗證信
            if (result.IsSuccess && result.Data && ApplicationHelper.ProfileWithEmail)
            {
                var mailService = new MailService(ApplicationHelper.ClientID);
                var mailContent = new ReplaceMailContent
                {
                    UserName = model.User.Name,
                    UserEmail = model.User.Email
                };
                var mailResult = await mailService.SendEmail(SessionManager.UserID, mailContent, SystemMailType.ConfirmEmail, ValidType.ConfirmEmail);
                return Json(mailResult);
            }

            return Json(result);
        }


        /// <summary>
        /// 修改Email帳號
        /// </summary>
        /// <returns></returns>       
        public ActionResult UpdateEmail()
        {
            var model = service.GetView(SessionManager.UserID);
            return View(ViewName("Member", "UpdateEmail"), model: model.User.Email);
        }

        /// <summary>
        /// 修改Email帳號, 重寄驗證信
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateEmail(string Email)
        {
            var result = service.UpdateEmail(SessionManager.UserID, Email);

            //check and addLog
            if (result.IsSuccess)
            {
                var model = new SendEmailViewModel
                {
                    Email = Email,
                    SystemMailType = SystemMailType.ConfirmEmail,
                    ValidType = ValidType.ConfirmEmail
                };
                result = service.SendValidCodeCheck(model);
            }

            //重寄驗證信
            if (result.IsSuccess)
            {
                var mailService = new MailService(ApplicationHelper.ClientID);
                var mailContent = new ReplaceMailContent
                {
                    UserName = SessionManager.UserName,
                    UserEmail = Email
                };
                var mailResult = await mailService.SendEmail(SessionManager.UserID, mailContent, SystemMailType.ConfirmEmail, ValidType.ConfirmEmail);


                //重新登入
                //SessionManager.RemoveAll();

                return Json(mailResult);
            }

            return Json(result);
        }

        /// <summary>
        /// 修改電話
        /// </summary>
        /// <returns></returns>       
        public ActionResult UpdatePhone()
        {
            var model = service.GetView(SessionManager.UserID);
            return View(ViewName("Member", "UpdatePhone"), model: model.User.Phone);
        }

        /// <summary>
        /// 修改電話, 重寄驗證碼
        /// </summary>
        /// <returns></returns>       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdatePhone(string Phone)
        {
            if (!Request.IsAjaxRequest())
            {
                return ErrorPage(); //404
            }


            var result = service.UpdatePhone(SessionManager.UserID, Phone);

            //check and addLog
            if (result.IsSuccess)
            {
                result = service.SendSmsValidCodeCheck(Phone);
            }

            //送出簡訊驗證碼
            if (result.IsSuccess)
            {
                var mailService = new MailService(ApplicationHelper.ClientID);
                var mailContent = new ReplaceMailContent
                {
                    PhoneNumber = Phone
                };
                var mailResult = await mailService.SendSms(SessionManager.UserID, mailContent, SystemMailType.ConfirmEmail, ValidType.ConfirmPhone, fromFn: "UpdatePhone");

                return Json(mailResult);
            }

            return Json(result);
        }

        /// <summary>
        /// 重寄驗證信SMS(給自己)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> SendSmsValidCodeSelf()
        {
            var phoneNumber = service.GetMyPhoneNumber(SessionManager.UserID);

            var result = service.SendSmsValidCodeCheck(phoneNumber);
            //送出簡訊驗證碼
            if (result.IsSuccess)
            {
                var mailService = new MailService(ApplicationHelper.ClientID);
                var mailContent = new ReplaceMailContent
                {
                    PhoneNumber = phoneNumber
                };
                var mailResult = await mailService.SendSms(SessionManager.UserID, mailContent, SystemMailType.ConfirmEmail, ValidType.ConfirmPhone, fromFn: "UpdatePhone");

                return Json(mailResult);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改電話-回傳驗證碼
        /// </summary>
        /// <returns></returns>       
        public ActionResult UpdatePhoneCorfirm()
        {
            var model = service.GetView(SessionManager.UserID);
            return View(ViewName("Member", "UpdatePhoneCorfirm"), model: model.User.Phone);
        }

        /// <summary>
        /// 修改電話-回傳驗證碼
        /// </summary>
        /// <returns></returns>       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePhoneCorfirm(string code, string key)
        {
            if (!Request.IsAjaxRequest())
            {
                return ErrorPage(); //404
            }

            var result = service.CheckValidCode(
              new ValidCodeViewModel
              {
                  Type = ValidType.ConfirmPhone.ToString(),
                  Code = code,
                  Key = key
              });

            //UserID 用 session傳遞
            if (result.IsSuccess && result.Data.ValidType == ValidType.ForgotPassword)
            {
                SessionManager.UserID = result.Data.UserID;
            }

            return Json(result);
        }

        /// <summary>
        /// 修改密碼
        /// </summary>
        /// <returns></returns>
        public ActionResult Password()
        {
            var model = new UpdatePasswordViewModel
            {
                ExistPassword = service.ExistPassword(SessionManager.UserID)
            };
            return PartialView(ViewName("Member", "Password"), model);
        }

        /// <summary>
        /// 修改密碼
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Password(UpdatePasswordViewModel model)
        {
            var result = service.UpdatePassword(SessionManager.UserID, model);

            //重新登入
            if (result.IsSuccess)
            {
                SessionManager.RemoveAll();
            }

            return Json(result);
        }


        #region 綁定登入 (no use)
        ///// <summary>
        ///// 綁定外部登入
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult BindingExternalLogin()
        //{
        //    return ErrorPage();
        //}

        ///// <summary>
        ///// 新增外部登入
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult CreateExternalLogin()
        //{
        //    return ErrorPage();
        //}

        ///// <summary>
        ///// 移除外部登入
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult RemoveExternalLogin()
        //{
        //    return ErrorPage();
        //}
        #endregion

        #endregion

        #region 家長編輯

        #region 編輯
        /// <summary>
        /// 我的孩子們
        /// </summary>
        /// <returns></returns>
        public ActionResult MyChildren()
        {
            // 新增選手前檢查    
            var preCheck = service.UserChildPreCheck(SessionManager.UserID);
            if (!preCheck.IsSuccess)
            {
                SetAlertMessage(preCheck.Message, AlertType.error);

                return Redirect("Profile");
            }

            var model = service.GetMyChildList(SessionManager.UserID);

            return PartialView(ViewName("Member", "MyChildren"), model);
        }

        /// <summary>
        /// 取得單個
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="IsEdit">if set to <c>true</c> [is edit].</param>
        /// <returns></returns>
        public ActionResult GetMyChild(Guid id, bool IsEdit)
        {
            var data = service.GetMyChild(SessionManager.UserID, id, true);

            var view = IsEdit ? "MyChildren_Edit" : "MyChildren_View";
            return PartialView(ViewName("Member", view), data);
        }

        public ActionResult GetMyChildEmpty()
        {
            return PartialView(ViewName("Member", "MyChildren_Edit"), new mgt_UserProfile());
        }

        /// <summary>
        /// 新增/修改孩子
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateMyChild(mgt_UserProfile model, bool IsCreate)
        {
            var result = new CiResult();

            model.CreateUser = SessionManager.UserID;
            if (IsCreate)
            {
                result = service.CreateMyChild(model);
            }
            else
            {
                result = service.UpdateMyChild(model);
            }

            return Json(result);
        }

        /// <summary>
        /// 刪除
        /// </summary>
        /// <returns></returns>
        public ActionResult DeleteMyChild(Guid childID, string captcha)
        {
            var result = new CiResult();

            //驗證碼
            if (string.IsNullOrWhiteSpace(captcha) || SessionManager.Captcha != captcha)
            {
                result.Message = SystemMessage.CaptchaError;
            }

            //save
            if (string.IsNullOrEmpty(result.Message))
            {
                result = service.DeleteMyChild(SessionManager.UserID, childID);
            }

            //show message
            if (!result.IsSuccess)
            {
                SetAlertMessage(result.Message, AlertType.error);
            }
            else
            {
                SetAlertMessage(result.Message, AlertType.success);
            }

            return RedirectToAction("MyChildren");
        }
        #endregion

        #region 指派與授權
        /// <summary>
        /// 授權出去的名單(教練列表)
        /// </summary>
        /// <returns></returns>
        public ActionResult MyChildren_Assign(Guid childID)
        {
            if (!Request.IsAjaxRequest())
            {
                return ErrorPage(); //404
            }
            
            var model = service.GetUserAssignFromList(SessionManager.UserID, childID);

            ViewBag.Member = service.GetMyChild(SessionManager.UserID, childID);

            return PartialView(ViewName("Member", "MyChildren_Assign"), model);
        }

        /// <summary>
        /// 新增指派
        /// </summary>
        /// <param name="childID">The child identifier.</param>
        /// <param name="AssignPhoneNumber">The phone number.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAssign(Guid childID, string AssignPhoneNumber)
        {
            var result = new CiResult();
            var toUser = service.GetbyPhone(AssignPhoneNumber.ToTrim());
            if (toUser == null)
            {
                result.Message = "查無電話號碼";
            }

            if (string.IsNullOrEmpty(result.Message))
            {
                var model = new mgt_UserAssign
                {
                    UserProfileID = childID,
                    FromUser = SessionManager.UserID,
                    ToUser = toUser.ID
                };
                result = service.CreateAssign(model);
            }

            //通知:選手指派新增 (失敗不提醒)
            if (result.IsSuccess)
            {
                try
                {
                    var data = service.GetUserAssign(result.ID);

                    if (data.ToUser.ID == data.FromUser.ID)
                    {
                        //_Log.CreateText($"[Email no send]指派給自己: User={data.ToUser.ID}, Member={data.Member.NickName}");
                    }
                    else
                    {
                        var mailService = new MailService(ApplicationHelper.ClientID);
                        var mailContent = new ReplaceMailContent
                        {
                            UserName = data.ToUser.Name,
                            UserEmail = data.ToUser.Email,
                            MemberName = data.Member.NickName
                        };
                        var mailResult = await mailService.SendEmail(data.ToUser.ID, mailContent, SystemMailType.AssignCreate, fromFn: "CreateAssign");
                        if (mailResult.IsSuccess)
                        {
                            result.Message += ", " + mailResult.Message;
                        }
                    }
                }
                catch (Exception e)
                {
                    var json = _Json.ModelToJson(e);
                    _Log.CreateText(json);
                }
            }

            return Json(result);
        }

        /// <summary>
        /// 刪除指派
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> DeleteAssign(Guid id)
        {
            var result = service.DeleteAssign(SessionManager.UserID, id);

            //通知:選手指派取消 (失敗不提醒)
            if (result.IsSuccess)
            {
                try
                {
                    var data = service.GetUserAssign(id);

                    var mailService = new MailService(ApplicationHelper.ClientID);
                    var mailContent = new ReplaceMailContent
                    {
                        UserName = data.ToUser.Name,
                        UserEmail = data.ToUser.Email,
                        MemberName = data.Member.NickName
                    };
                    var mailResult = await mailService.SendEmail(data.ToUser.ID, mailContent, SystemMailType.AssignCancel, fromFn: "DeleteAssign");
                    if (mailResult.IsSuccess)
                    {
                        result.Message += ", " + mailResult.Message;
                    }
                }
                catch (Exception e)
                {
                    var json = _Json.ModelToJson(e);
                    _Log.CreateText(json);
                }
            }

            return Json(result);
        }

        /// <summary>
        /// 授權給我的列表(我的選手)
        /// </summary>
        /// <returns></returns>
        public ActionResult MyTeamMember()
        {
            return PartialView(ViewName("Member", "MyTeamMember"));
        }
        #endregion

        #region 我的選手
        /// <summary>
        /// 我的選手分頁 ajax
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <returns></returns>
        public ActionResult GetMyTeamMemberPageList(PageParameter param)
        {
            if (!Request.IsAjaxRequest())
            {
                return ErrorPage(); //404
            }

            return MyTeamMemberPartial(param);
        }

        /// <summary>
        /// 我的選手分頁
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult MyTeamMemberPartial(PageParameter param)
        {
            var result = service.GetUserAssignToList(param, SessionManager.UserID);

            var resultModel = new PageResult<UserAssignViewModel>
            {
                DataResult = result,
                PageModel = param
            };

            return PartialView(ViewName("Member", "MyTeamMember_page"), resultModel);
        }

        #endregion

        #region 參賽紀錄、退賽
        /// <summary>
        /// 我的孩子們紀錄
        /// </summary>
        /// <returns></returns>
        public ActionResult MyChildrenRecord()
        {
            return PartialView(ViewName("Member", "MyChildrenRecord"));
        }

        /// <summary>
        /// 分頁 for ajax
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <returns></returns>
        public ActionResult GetMyChildrenRecordList(PageParameter param)
        {
            if (!Request.IsAjaxRequest())
            {
                return ErrorPage(); //404
            }

            return MyChildrenRecordPartial(param);
        }

        [ChildActionOnly]
        public ActionResult MyChildrenRecordPartial(PageParameter param)
        {
            var result = service.GetMyChildOrderList(param, SessionManager.UserID);

            var resultModel = new PageResult<MyChildrenRecordModel>
            {
                DataResult = result,
                PageModel = param
            };

            return PartialView(ViewName("Member", "MyChildrenRecord_Page"), resultModel);
        }

        ///// <summary>
        ///// 退出比賽
        ///// </summary>
        ///// <param name="id">The identifier.</param>
        ///// <param name="captcha">The captcha.</param>
        ///// <returns></returns>
        //public async Task<ActionResult> ChildOutOrder(Guid id, string captcha = "")
        //{
        //    //check在編輯期間

        //}
        #endregion

        #endregion

        #region 訂單紀錄

        public ActionResult Order(Guid type)
        {
            var model = new OrderListModel
            {
                Structure = structureService.Get(type),
                OrderPageModel = new OrderPageModel
                {
                    StructureID = type,
                }
            };

            return View(ViewName("Member", "Order"), model);
        }

        /// <summary>
        /// 分頁 for ajax
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>  
        public ActionResult GetOrderPageList(OrderPageModel model)
        {
            if (!Request.IsAjaxRequest())
            {
                return ErrorPage(); //404
            }

            return OrderPartial(model);
        }

        [ChildActionOnly]
        public ActionResult OrderPartial(OrderPageModel model)
        {
            var result = orderService.GetList((PageParameter)model,
                new OrderFilter
                {
                    StructureID = model.StructureID,
                    CreateUser = SessionManager.UserID,
                    LangType = ApplicationHelper.DefaultLanguage,
                    IsAdmin = false,
                });

            var resultModel = new OrderPageResult
            {
                DataResult = result,
                OrderPageModel = model
            };

            var structure = structureService.Get(model.StructureID);
            ViewBag.Structure = structure;

            return PartialView(ViewName("Member", "Order_Page"), resultModel);
        }
        #endregion

    }
}