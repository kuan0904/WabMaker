using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.Tools;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WabMaker.Web.Helpers;
using WebMaker.BLL.Helpers;
using WebMaker.BLL.Services;
using WebMaker.Entity.Models;
using WebMaker.Entity.ViewModels;
using static MyTool.Tools.MailTool;

namespace WabMaker.Web.MainService
{
    //郵件、簡訊處理
    public class MailService
    {
        private UserService userService = new UserService { };
        private SystemSettingService settingService = new SystemSettingService { };
        private EmailTemplateService templateService = new EmailTemplateService { };
        private EmailLogService emailService = new EmailLogService { };
        private SmsLogService smsService = new SmsLogService { };

        public MailService(Guid ClientID)
        {
            userService.ClientID = ClientID;
            settingService.ClientID = ClientID;
            templateService.ClientID = ClientID;
            emailService.ClientID = ClientID;
            smsService.ClientID = ClientID;
        }

        /// <summary>
        /// 是否存在信件樣板
        /// </summary>
        public bool IsExistTemplate(SystemMailType systemMailType, Guid structureID)
        {
            var data = templateService.GetByType(systemMailType, structureID);
            return data != null;
        }

        /// <summary>
        /// 組合訂單內容
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public string CreateOrderContent(OrderViewModel model)
        {
            string result = "編號：" + model.Order.OrderNumber + "<br>"
                          + "內容：";

            if (model.OrderDetails.Count > 1)
            {
                result += "<br>";
            }

            foreach (var detail in model.OrderDetails)
            {
                result += detail.ItemSubject + "<br>";
            }

            return result;
        }

        /// <summary>
        /// 組合匯款帳號
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public string CreateOrderATMInfo(OrderViewModel model)
        {
            string result = "";
            //滑輪
            if (model.Order.PayType == (int)PayType.ATMVirtual && !string.IsNullOrEmpty(model.Order.VirtualAccount))
            {
                result = $"匯款資訊如下，請於{model.Order.PayDeadline.ToDateString()}前完成匯款。" + "<br>"
                        + "銀行：國泰世華(013) 建國分行" + "<br>"
                       + $"帳號：{model.Order.VirtualAccount}" + "<br>"
                        + "戶名：中華民國滑輪溜冰協會劉福財" + "<br>"
                       + $"金額：{model.Order.TotalPrice.ToPrice()}" + "<br>";
            }

            return result;

        }


        /// <summary>
        /// 寄送郵件
        /// </summary>
        /// <param name="userID">user id</param>
        /// <param name="model">Mail內容取代字串</param>
        /// <param name="systemMailType">郵件類型</param>
        /// <param name="validType">驗證碼類型</param>
        /// <param name="structureID">郵件類型structure</param>
        /// <param name="fromFn">來源</param>
        /// <returns></returns>
        public async Task<CiResult> SendEmail(Guid userID, ReplaceMailContent model,
                                              SystemMailType systemMailType, ValidType? validType = null, Guid? structureID = null,
                                              string fromFn = "")
        {
            var result = new CiResult { IsSuccess = true };
            var mailTool = new MailTool { email = model.UserEmail };

            //ReplaceMailContent
            model.WebsiteUrl = RouteHelper.BaseUrl();
            if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.UserEmail) || !_Check.IsEmail(model.UserEmail))
            {
                result.IsSuccess = false;
            }

            //get setting
            if (result.IsSuccess)
            {
                var smtpResult = settingService.Get<SmtpServerViewModel>(SystemSettingType.SmtpServer);
                mailTool.setting = smtpResult.Data;
                if (!smtpResult.IsSuccess || !smtpResult.Data.IsEnabled)
                {
                    result.IsSuccess = false;
                }
            }

            if (result.IsSuccess)
            {
                var siteResult = settingService.Get<SiteInfoViewModel>(SystemSettingType.SiteInfo);
                if (siteResult.IsSuccess)
                {
                    model.WebsiteName = siteResult.Data.SiteName;
                }
                else
                {
                    result.IsSuccess = false;
                }
            }

            //create validCode           
            if (result.IsSuccess && validType != null)
            {
                var validCodeResult = userService.CreateValidCode(userID, validType.Value);
                if (validCodeResult.IsSuccess)
                {
                    model.ConfirmUrl = RouteHelper.GetConfirmUrl(validType.Value, validCodeResult.Data, model.UserEmail);
                }
                else
                {
                    result.IsSuccess = false;
                }
            }

            //get template
            var template = new cms_EmailTemplate();
            if (result.IsSuccess)
            {
                template = templateService.GetByType(systemMailType, structureID);
                if (template != null)
                {
                    // ckeditor
                    template.Content = HttpUtility.HtmlDecode(template.Content);

                    // set template
                    mailTool.subject = model.ReplaceContent(template.Subject);
                    mailTool.content = model.ReplaceContent(template.Content);
                    if (!string.IsNullOrEmpty(template.TemplateBcc))
                    {
                        mailTool.bccReceiver = template.TemplateBcc.Split(',');
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    _Log.CreateText($"SendEmail no template: {systemMailType.ToString()}");
                }
            }

            //send mail (測試中:只能寄給開發者)
            if (result.IsSuccess)
            {
                if (!ApplicationHelper.IsLocal || (ApplicationHelper.IsLocal && mailTool.IsTestingMail(model.UserEmail)))
                {
                    result.IsSuccess = await mailTool.SendAsync(fromFn);
                }
                else
                {
                    _Log.CreateText($"Local not send: {mailTool.email}");
                }

                //add log
                emailService.CreateLog(new EmailViewModel
                {
                    Email = new cms_Email
                    {
                        Subject = mailTool.subject,
                        Content = mailTool.content,
                        SystemMailType = (int)systemMailType,
                        Status = (int)MailStatus.Done,
                        SendTime = DateTime.Now
                    },
                    SendUsers = new List<cms_EmailSendUser> {
                        new cms_EmailSendUser {
                            ToEmail = model.UserEmail ,
                            ToUser = userID,
                            IsSend = result.IsSuccess,//是否已發送
                            SendTime = DateTime.Now
                        }
                    }
                });

            }

            if (result.IsSuccess)
            {
                if (systemMailType == SystemMailType.ConfirmEmail)
                {
                    result.Message = string.Format(SystemMessage.EmailConfirm, model.UserEmail);
                }
                else if (systemMailType == SystemMailType.ForgotPassword)
                {
                    result.Message = string.Format(SystemMessage.PasswordReset, model.UserEmail);
                }
                else
                {
                    result.Message = SystemMessage.SendSuccess;
                }
            }
            else if (string.IsNullOrEmpty(result.Message))
            {
                result.Message = SystemMessage.MailServerError;
            }

            SessionManager.Captcha = "";

            return result;
        }


        /// <summary>
        /// 寄送簡訊
        /// </summary>
        /// <param name="userID">user id</param>
        /// <param name="model">Mail內容取代字串</param>
        /// <param name="systemMailType">郵件類型</param>
        /// <param name="validType">驗證碼類型</param>
        /// <param name="structureID">郵件類型structure</param>
        /// <param name="fromFn">來源</param>
        /// <returns></returns>
        public async Task<CiResult> SendSms(Guid userID, ReplaceMailContent model,
                                              SystemMailType systemMailType, ValidType? validType = null, Guid? structureID = null,
                                              string fromFn = "")
        {
            var result = new CiResult<SMSViewModel> { IsSuccess = true };
            var smsTool = new SmsTool { };

            //ReplaceMailContent
            model.WebsiteUrl = RouteHelper.BaseUrl();
            if (string.IsNullOrEmpty(model.PhoneNumber) || !_Check.IsPhone(model.PhoneNumber))
            {
                result.IsSuccess = false;
            }

            //system setting
            if (!ApplicationHelper.ClientSettings.Contains(ClientSetting.SMS))
            {
                result.IsSuccess = false;
                _Log.CreateText($"SendSms no setting");
            }

            //get setting
            if (result.IsSuccess)
            {
                var smsResult = settingService.Get<SmsServiceViewModel>(SystemSettingType.SmsService);
                smsTool.setting = smsResult.Data;
                if (!smsResult.IsSuccess || !smsResult.Data.IsEnabled)
                {
                    result.IsSuccess = false;
                }
            }

            if (result.IsSuccess)
            {
                var siteResult = settingService.Get<SiteInfoViewModel>(SystemSettingType.SiteInfo);
                if (siteResult.IsSuccess)
                {
                    model.WebsiteName = siteResult.Data.SiteName;
                }
                else
                {
                    result.IsSuccess = false;
                }
            }

            //create validCode           
            if (result.IsSuccess && validType != null)
            {
                var validCodeResult = userService.CreateValidCode(userID, validType.Value, length: 5);
                if (validCodeResult.IsSuccess)
                {
                    model.ValidCode = validCodeResult.Data;
                }
                else
                {
                    result.IsSuccess = false;
                }
            }

            //get template
            var template = new cms_EmailTemplate();
            if (result.IsSuccess)
            {
                template = templateService.GetByType(systemMailType, structureID);
                if (template != null)
                {
                    // set template                  
                    smsTool.message = model.ReplaceContent(template.SMSContent);
                }
                else
                {
                    result.IsSuccess = false;
                    _Log.CreateText($"SendSms no template: {systemMailType.ToString()}");

                }
            }

            //send mail (測試中不寄簡訊)
            if (result.IsSuccess)
            {
                if (ApplicationHelper.IsLocal)
                {
                    _Log.CreateText($"Local not send SMS: {model.PhoneNumber} >> {smsTool.message}");
                }
                else
                {
                    result = await smsTool.Send(userID, model.PhoneNumber);
                }

                //add log
                var log = new cms_SmsLog
                {
                    PhoneNumber = model.PhoneNumber,
                    SMSContent = smsTool.message,
                    ToUser = userID,
                    IsSend = result.IsSuccess,//是否已發送
                    SendTime = DateTime.Now,
                    CreateTime = DateTime.Now,
                };
                if (result.IsSuccess && result.Data != null)
                {
                    log.Msgid = result.Data.MsgId;
                    log.SmsResultType = (int)result.Data.ResultType;
                    log.UpdateResultTime = DateTime.Now;
                }
                smsService.CreateLog(log);
            }

            if (result.IsSuccess)
            {
                if (systemMailType == SystemMailType.ConfirmEmail)
                {
                    result.Message = string.Format(SystemMessage.PhoneConfirm, model.PhoneNumber);
                }
                else
                {
                    result.Message = SystemMessage.SendSmsSuccess;
                }
            }
            else if (string.IsNullOrEmpty(result.Message))
            {
                result.Message = SystemMessage.SmsServiceError;
            }

            SessionManager.Captcha = "";

            return result;
        }
    }
}