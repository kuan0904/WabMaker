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
    /// 簡訊服務
    /// </summary>
    public class SmsLogService : BaseService
    {
        /// <summary>
        /// 新增系統信紀錄
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public CiResult CreateLog(cms_SmsLog model)
        {
            CiResult result = new CiResult();

            try
            {
                //create                               
                model.ClientID = ClientID;

                Db.cms_SmsLog.Add(model);
                Db.SaveChanges();

                result.Message = SystemMessage.CreateSuccess;
                result.IsSuccess = true;

            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.CreateFail;
                _Log.CreateText("SMS_CreateLog:" + _Json.ModelToJson(ex));
            }

            return result;
        }

    }
}
