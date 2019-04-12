using MyTool.Commons;
using MyTool.Enums;
using MyTool.Tools;
using MyTool.ViewModels;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using WabMaker.Web.Helpers;
using WebMaker.BLL.Helpers;
using WebMaker.BLL.Services;

namespace WabMaker.Web.Areas.Admins.Controllers
{
    /// <summary>
    /// 系統設定
    /// </summary>
    public class SystemSettingController : AuthBaseController
    {
        private SystemSettingService service = new SystemSettingService();

        public SystemSettingController()
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

        /// <summary>
        /// 客戶(公司)基本資料設定
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ClientInfo()
        {
            var result = service.Get<ClientInfoViewModel>(SystemSettingType.ClientInfo);

            if (!result.IsSuccess)
            {
                SetAlertMessage(result.Message, AlertType.warning);
            }
            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClientInfo(ClientInfoViewModel model, List<UploadViewModel> ImageModel)
        {
            var result = new CiResult();

            var uploadResult = FileUpload(ImageModel);
            if (!uploadResult.IsSuccess)
            {
                result.Message = uploadResult.Message;
            }
            else
            {
                model.Logo = uploadResult.Data;
            }


            //save
            if (string.IsNullOrEmpty(result.Message))
            {
                result = service.Save(SystemSettingType.ClientInfo, model);
            }

            return Json(result);
        }

        /// <summary>
        /// 網站基本資料設定
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SiteInfo()
        {
            var result = service.Get<SiteInfoViewModel>(SystemSettingType.SiteInfo);
            if (!result.IsSuccess)
            {
                SetAlertMessage(result.Message, AlertType.warning);
            }
            else
            {
                // Html Decode
                result.Data.Footer = HttpUtility.HtmlDecode(result.Data.Footer);
            }

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SiteInfo(SiteInfoViewModel model, List<UploadViewModel> ImageModel)
        {
            var result = new CiResult();

            var uploadResult = FileUpload(ImageModel);
            if (!uploadResult.IsSuccess)
            {
                result.Message = uploadResult.Message;
            }
            else
            {
                model.MetaTagOgImage = uploadResult.Data;
            }

            //save
            if (string.IsNullOrEmpty(result.Message))
            {
                result = service.Save(SystemSettingType.SiteInfo, model);
            }

            //web reset
            if (result.IsSuccess)
            {
                var applicationHelper = new ApplicationHelper();
                applicationHelper.RemoveAll();
                applicationHelper.Init();
            }

            return Json(result);
        }

        /// <summary>
        /// API key設定
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ApiKey()
        {
            var result = service.Get<ApiKeyViewModel>(SystemSettingType.ApiKey);
            if (!result.IsSuccess)
            {
                SetAlertMessage(result.Message, AlertType.warning);
            }
            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApiKey(ApiKeyViewModel model)
        {
            var result = service.Save(SystemSettingType.ApiKey, model);

            //web reset
            if (result.IsSuccess)
            {
                var applicationHelper = new ApplicationHelper();
                applicationHelper.RemoveAll();
                applicationHelper.Init();
            }

            return Json(result);
        }

        /// <summary>
        /// 寄件服務設定
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SmtpServer()
        {
            var result = service.Get<SmtpServerViewModel>(SystemSettingType.SmtpServer);
            if (!result.IsSuccess)
            {
                SetAlertMessage(result.Message, AlertType.warning);
            }
            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SmtpServer(SmtpServerViewModel model)
        {
            //password no update
            if (string.IsNullOrEmpty(model.Password) && model.MailServer != "localhost")
            {
                var data = service.Get<SmtpServerViewModel>(SystemSettingType.SmtpServer);
                model.Password = data.Data.Password;
            }

            var result = service.Save(SystemSettingType.SmtpServer, model);
            return Json(result);
        }


        /// <summary>
        /// 簡訊服務設定
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SmsService()
        {
            var result = service.Get<SmsServiceViewModel>(SystemSettingType.SmsService);
            if (!result.IsSuccess)
            {
                SetAlertMessage(result.Message, AlertType.warning);
            }
            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SmsService(SmsServiceViewModel model)
        {
            //password no update
            if (string.IsNullOrEmpty(model.Password))
            {
                var data = service.Get<SmsServiceViewModel>(SystemSettingType.SmsService);
                model.Password = data.Data.Password;
            }

            var result = service.Save(SystemSettingType.SmsService, model);
            return Json(result);
        }


        /// <summary>
        /// 上傳圖片共用
        /// </summary>
        /// <param name="ImageModel">The image model.</param>
        /// <returns>fileUploadPath</returns>
        private CiResult<string> FileUpload(List<UploadViewModel> ImageModel)
        {
            var result = new CiResult<string> { IsSuccess = true };

            if (ImageModel != null)
            {
                //old
                if (ImageModel[0].FileStatus != FileStatus.Delete)
                {
                    result.Data = ImageModel[0].FilePath;
                }

                //fileUpload
                if (ImageModel[0].FileUpload != null)
                {
                    var fileFolder = UploadTool.GetFileFolder(SessionManager.Client.SystemName, SourceType.System);
                    var uploadResult = UploadTool.FileUpload(ImageModel[0].FileUpload, ImageModel[0].FileType, fileFolder);
                    if (!uploadResult.IsSuccess)
                    {
                        result.Message = uploadResult.Message;
                        result.IsSuccess = false;
                    }
                    else
                    {
                        result.Data = uploadResult.Data.FilePath;
                    }
                }
            }

            return result;
        }
    }
}