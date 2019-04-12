using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.Tools;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using WebMaker.Entity.Models;

namespace WebMaker.BLL.Services
{
    /// <summary>
    /// 郵件樣板(通知信管理)
    /// </summary>
    public class EmailTemplateService : BaseService
    {
        #region Get

        private IQueryable<cms_EmailTemplate> Query
        {
            get
            {
                return Db.cms_EmailTemplate
                   .Where(x => !x.IsDelete && x.ClientID == ClientID);
            }
        }

        public PageModel<cms_EmailTemplate> GetList(PageParameter param)
        {
            param.SortColumn = SortColumn.Sort;
            param.IsDescending = false;

            var pagedModel = PageTool.CreatePage(Query, param);
            return pagedModel;
        }

        public cms_EmailTemplate Get(Guid id)
        {
            var query = Query.FirstOrDefault(x => x.ID == id);
            return query;
        }

        public cms_EmailTemplate GetByType(SystemMailType type, Guid? structureID = null, bool isEmail = true)
        {
            var query = Query.Where(x => x.SystemMailType == (int)type &&
                                   ((isEmail && x.IsEnabled) || (!isEmail && x.SMSIsEnabled))); //email or sms enabled

            if (structureID != null)
            {
                query = query.Where(x => x.StructureID == structureID.Value);
            }

            if (query.Count() != 1)
                return null;
            else
                return query.FirstOrDefault();
        }

        #endregion

        #region Edit
        public CiResult<Guid> Create(cms_EmailTemplate model)
        {
            CiResult<Guid> result = new CiResult<Guid>();

            try
            {
                //check type
                if (string.IsNullOrEmpty(result.Message))
                {
                    var exist = CheckType((SystemMailType)model.SystemMailType, model.StructureID, null);
                    if (exist)
                        result.Message = SystemMessage.TypeExist;
                }

                //create
                if (string.IsNullOrEmpty(result.Message))
                {
                    model.ID = Guid.NewGuid();
                    model.ClientID = ClientID;
                    model.UpdateTime = DateTime.Now;
                    Db.cms_EmailTemplate.Add(model);

                    Db.SaveChanges();

                    result.Data = model.ID;
                    result.Message = SystemMessage.CreateSuccess;
                    result.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.CreateFail;
                _Log.CreateText("EmailTemplate_Create:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        public CiResult Update(cms_EmailTemplate model)
        {
            CiResult result = new CiResult();

            try
            {
                //check type
                if (string.IsNullOrEmpty(result.Message))
                {
                    var exist = CheckType((SystemMailType)model.SystemMailType, model.StructureID, model.ID);
                    if (exist)
                        result.Message = SystemMessage.TypeExist;
                }

                //update
                if (string.IsNullOrEmpty(result.Message))
                {
                    model.UpdateTime = DateTime.Now;
                    EntityUpdate(model, new List<string> { "ClientID", "IsDelete" });

                    result.Message = SystemMessage.UpdateSuccess;
                    result.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.UpdateFail;
                _Log.CreateText("EmailTemplate_Update" + _Json.ModelToJson(ex));
            }

            return result;
        }

        public CiResult Delete(Guid id, Guid updateUser)
        {
            CiResult result = new CiResult();
            var data = Get(id);

            try
            {
                data.IsDelete = true;
                data.UpdateTime = DateTime.Now;
                data.UpdateUser = updateUser;
                Db.SaveChanges();

                result.Message = SystemMessage.DeleteSuccess;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.DeleteFail;
                _Log.CreateText("EmailTemplate_Delete:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        #endregion

        #region Check
        /// <summary>
        /// Type是否重複
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public bool CheckType(SystemMailType type, Guid? structureID, Guid? id)
        {
            //自訂可多個
            if (type == SystemMailType.None) return true;

            var data = Query;
            // 排除自己
            if (id != null)
            {
                data = data.Where(x => x.ID != id);
            }

            //篩選structureID
            if (structureID != null)
            {
                data = data.Where(x => x.StructureID == structureID);
            }

            bool result = (data.FirstOrDefault(x => x.SystemMailType == (int)type)) != null;

            return result;
        }
        #endregion
    }
}
