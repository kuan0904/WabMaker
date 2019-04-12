using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.ViewModels;
using System;
using System.Linq;
using WebMaker.Entity.Models;

namespace WebMaker.BLL.Services
{
    /// <summary>
    /// 系統設定
    /// </summary>
    public class SystemSettingService : BaseService
    {
        private string CryptoKey = "gp52S4tg1npwk0";

        private IQueryable<mgt_SystemSetting> Query
        {
            get
            {
                return Db.mgt_SystemSetting.Where(x => x.ClientID == ClientID);
            }
        }

        /// <summary>
        /// 取得系統設定檔
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public CiResult<T> Get<T>(SystemSettingType type) where T : BaseSetting
        {
            CiResult<T> result = new CiResult<T>();
            var data = Query.FirstOrDefault(x => x.Type == type.ToString());

            try
            {
                if (data != null)
                {
                    //解密
                    var settingText = _Crypto.DecryptAES(data.SettingText, CryptoKey);

                    result.Data = _Json.JsonToModel<T>(settingText);
                    result.IsSuccess = true;
                }
                else
                {
                    result.Message = SystemMessage.SettingNone;
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.ErrorAndHelp;
                _Log.CreateText("mgt_SystemSetting_Get:" + _Json.ModelToJson(ex));
            }           

            return result;
        }

        /// <summary>
        /// 儲存系統設定檔(Create/Update)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public CiResult Save<T>(SystemSettingType type, T model) where T : BaseSetting
        {
            CiResult result = new CiResult();
            var data = Query.FirstOrDefault(x => x.Type == type.ToString());

            try
            {
                var jsonString = _Json.ModelToJson(model);
                //加密
                jsonString = _Crypto.EncryptAES(jsonString, CryptoKey);

                if (data == null)
                {
                    var sysResult = CreateSystemNumber(DataTableCode.mgt_SystemSetting);
                    if (!sysResult.IsSuccess)
                    {
                        result.Message = sysResult.Message;
                    }
                    else
                    {
                        //create
                        var newData = new mgt_SystemSetting
                        {
                            ClientID = ClientID,
                            SystemNumber = sysResult.Data,
                            Type = type.ToString(),
                            SettingText = jsonString,
                            CreateTime = DateTime.Now,
                            UpdateTime = DateTime.Now
                        };

                        Db.mgt_SystemSetting.Add(newData);
                    }
                }
                else
                {
                    //update
                    data.SettingText = jsonString;
                    data.UpdateTime = DateTime.Now;
                }

                //success result
                if (string.IsNullOrEmpty(result.Message))
                {
                    Db.SaveChanges();
                    result.Message = SystemMessage.UpdateSuccess;
                    result.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.UpdateFail;
                _Log.CreateText("mgt_SystemSetting_Save:" + _Json.ModelToJson(ex));
            }

            return result;
        }

    }
}