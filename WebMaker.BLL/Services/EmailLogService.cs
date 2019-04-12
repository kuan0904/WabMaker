using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.Tools;
using MyTool.ViewModels;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebMaker.Entity.Models;
using WebMaker.Entity.ViewModels;

namespace WebMaker.BLL.Services
{
    /// <summary>
    /// 郵件服務
    /// </summary>
    public class EmailLogService : BaseService
    {
        #region Get

        private IQueryable<cms_Email> Query
        {
            get
            {
                return Db.cms_Email
                   .Where(x => x.Status != (int)MailStatus.Delete && x.ClientID == ClientID);
            }
        }
        /*
        public PageModel<cms_Email> GetList(PageParameter param)
        {
            //todo filter: SystemMailType、DateTime range、User
            var pagedModel = PageTool.CreatePage(Query, param);
            return pagedModel;
        }

        public cms_Email Get(Guid id)
        {
            var query = Query.FirstOrDefault(x => x.ID == id);
            return query;
        }
        */
        #endregion

        #region Edit            
        /// <summary>
        /// 新增系統信紀錄
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public CiResult CreateLog(EmailViewModel model)
        {
            CiResult result = new CiResult();

            try
            {
                //create
                if (string.IsNullOrEmpty(result.Message))
                {
                    model.Email.ID = Guid.NewGuid();
                    model.Email.ClientID = ClientID;
                    model.Email.UpdateTime = DateTime.Now;

                    if (model.SendUsers != null)
                    {
                        foreach (var sendUser in model.SendUsers)
                        {
                            sendUser.ID = Guid.NewGuid();
                            model.Email.cms_EmailSendUser.Add(sendUser);
                        }
                    }

                    Db.cms_Email.Add(model.Email);
                    Db.SaveChanges();

                    result.Message = SystemMessage.CreateSuccess;
                    result.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.CreateFail;
                _Log.CreateText("Email_CreateLog:" + _Json.ModelToJson(ex));
            }

            return result;
        }
        
        /*
        public CiResult Update(cms_Email model)
        {
            CiResult result = new CiResult();

            try
            {

                //update
                if (string.IsNullOrEmpty(result.Message))
                {
                    model.UpdateTime = DateTime.Now;

                    EntityUpdate(model, new List<string> { "ClientID", "Sort", "IsDelete" });
                   
                    result.Message = SystemMessage.UpdateSuccess;
                    result.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.UpdateFail;
                _Log.CreateText("Email_Update" + _Json.ModelToJson(ex));
            }

            return result;
        }

        public CiResult Delete(Guid id)
        {
            CiResult result = new CiResult();
            var data = Get(id);

            try
            {
                data.Status = (int)EmailStatus.Delete;
                data.UpdateTime = DateTime.Now;
                Db.SaveChanges();

                result.Message = SystemMessage.DeleteSuccess;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.DeleteFail;
                _Log.CreateText("Email_Delete:" + _Json.ModelToJson(ex));
            }

            return result;
        }
        */
        #endregion

        /// <summary>
        /// 延遲寄信
        /// </summary>
        public void SendEmailDelay()
        {
            var settingService = new SystemSettingService();

            //查詢功能開關
            var clients = Db.mgt_Client.Where(x => !x.IsDelete && x.IsEnabled).ToList()
                                           .Where(x => x.ClientSetting.HasValue((int)ClientSetting.SendEmailDelay)).ToList();

            //each client 
            foreach (var client in clients)
            {
                //所有待寄的信
                var userMails = Db.cms_EmailSendUser.Where(x => x.cms_Email.ClientID == client.ID && x.IsSend == false).ToList();

                //get setting       
                settingService.ClientID = client.ID;
                var smtpResult = settingService.Get<SmtpServerViewModel>(SystemSettingType.SmtpServer);
                if (!smtpResult.IsSuccess || !smtpResult.Data.IsEnabled)
                {
                    _Log.CreateText($"SendEmailDelay: no setting");
                    continue;
                }
                _Log.CreateText($"------SendEmail 取得{userMails.Count()}筆------");

                //每封信
                foreach (var model in userMails)
                {
                    var mailTool = new MailTool
                    {
                        email = model.ToEmail,
                        setting = smtpResult.Data,
                        subject = model.cms_Email.Subject,
                        content = model.cms_Email.Content
                    };

                    //寄出信件->狀態完成
                    var result = mailTool.Send();
                    if (result)
                    {
                        _Log.CreateText($"SendEmail: {model.ID}");
                        model.IsSend = true;
                        Db.SaveChanges();
                    }

                    //休息1秒
                    Thread.Sleep(1000);
                }             

            }//end client each

        }
    }
}
