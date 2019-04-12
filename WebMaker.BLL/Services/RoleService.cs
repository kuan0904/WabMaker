using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.Tools;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebMaker.Entity.Models;
using WebMaker.Entity.ViewModels;

namespace WebMaker.BLL.Services
{
    /// <summary>
    /// 權限角色管理
    /// </summary>
    public class RoleService : BaseService
    {
        #region Get

        private IQueryable<mgt_Role> Query
        {
            get
            {
                return Db.mgt_Role
                   .Where(x => !x.IsDelete && x.ClientID == ClientID);
            }
        }

        public PageModel<mgt_Role> GetList(PageParameter param, AccountType? accountType = null)
        {
            param.IsPaged = false;
            param.SortColumn = SortColumn.Sort;
            param.IsDescending = false;

            var query = Query; 
            if (accountType != null) {
                query = query.Where(x => x.AccountType == (int)accountType);
            }
              
            var pagedModel = PageTool.CreatePage(query, param);
            return pagedModel;
        }

        public mgt_Role Get(Guid id)
        {
            var query = Query.FirstOrDefault(x => x.ID == id);
            return query;
        }

        /// <summary>
        /// 取得角色權限check list
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public List<TreeViewModel> GetMenuCheckList(Guid? id)
        {
            MenuService menuService = new MenuService();
            var rolePermissions = new List<RolePermissionModel>();
            if (id != null)
            {
                rolePermissions = GetRolePermissions(id.Value);
            }

            var tree = new List<TreeViewModel>();
            foreach (MenuType type in Enum.GetValues(typeof(MenuType)))
            {
                var typeTree = new TreeViewModel
                {
                    Name = type.GetDisplayName(),
                    Nodes = menuService.GetTrees(null, type, checkEnabled: true, rolePermissions: rolePermissions)
                };
                tree.Add(typeTree);
            }
            return tree;
        }

        /// <summary>
        /// MenuID 包含權限Actions
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public List<RolePermissionModel> GetRolePermissions(Guid id)
        {
            return Get(id).mgt_RoleMenuRelation
                          .Select(x => new RolePermissionModel { MenuID = x.MenuID, RoleActions = x.RoleActions }).ToList();
        }

        /// <summary>
        /// 取得下拉選單
        /// </summary>
        /// <param name="itemType">角色類型</param>
        /// <param name="selectFirstLevel">是否篩選第一層</param>
        /// <returns></returns>
        public List<SelectListItem> GetSelectList(AccountType accountType, Guid? selectedID = null)
        {
            var result = new List<SelectListItem>();
            var query = Query.AsEnumerable();

            //篩選類型          
            query = query.Where(x => x.AccountType == (int)accountType)
                            .OrderBy(x => x.Sort);

            foreach (var item in query.ToList())
            {
                bool selected = selectedID != null ? item.ID == selectedID.Value : false;

                result.Add(new SelectListItem
                {
                    Value = item.ID.ToString(),
                    Text = item.Name,
                    Selected = selected
                });
            }
            return result;
        }
        #endregion

        #region Edit
        public CiResult<Guid> Create(RoleViewModel model)
        {
            CiResult<Guid> result = new CiResult<Guid>();

            try
            {
                //field required
                if (string.IsNullOrEmpty(model.Role.Name))
                {
                    result.Message = string.Format(SystemMessage.FieldNull, "");
                }

                //check name
                if (string.IsNullOrEmpty(result.Message))
                {
                    var exist = CheckName(model.Role.Name, null);
                    if (exist)
                        result.Message = string.Format(SystemMessage.FieldExist, "名稱");
                }

                //get SystemNumber
                string systemNumber = "";
                if (string.IsNullOrEmpty(result.Message))
                {
                    var sysResult = CreateSystemNumber(DataTableCode.mgt_Role);
                    if (!sysResult.IsSuccess)
                        result.Message = sysResult.Message;

                    systemNumber = sysResult.Data;
                }

                //create
                if (string.IsNullOrEmpty(result.Message))
                {
                    var data = new mgt_Role
                    {
                        ID = Guid.NewGuid(),
                        ClientID = ClientID,
                        SystemNumber = systemNumber,
                        Name = model.Role.Name.ToTrim(),
                        MemberLevel = model.Role.MemberLevel,
                        AccountType = model.Role.AccountType,
                        UserContentTypes = model.Role.UserContentTypes,
                        UserRequiredTypes = model.Role.UserRequiredTypes,
                        Sort = model.Role.Sort,
                        IsEnabled = model.Role.IsEnabled,
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now
                    };
                    Db.mgt_Role.Add(data);


                    if (model.MenuCheckList != null)
                    {
                        foreach (var item in model.MenuCheckList.Where(x => x.IsChecked).ToList())
                        {
                            var relationData = new mgt_RoleMenuRelation
                            {
                                RoleID = data.ID,
                                MenuID = item.ID,
                                RoleActions = item.RoleActions,
                            };
                            Db.mgt_RoleMenuRelation.Add(relationData);
                        }
                    }

                    Db.SaveChanges();

                    result.Data = data.ID;
                    result.Message = SystemMessage.CreateSuccess;
                    result.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.CreateFail;
                _Log.CreateText("Role_Create:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        public CiResult Update(RoleViewModel model)
        {
            CiResult result = new CiResult();
            mgt_Role data = Get(model.Role.ID);

            try
            {
                //field required
                if (string.IsNullOrEmpty(model.Role.Name))
                {
                    result.Message = string.Format(SystemMessage.FieldNull, "");
                }

                //check name
                if (string.IsNullOrEmpty(result.Message))
                {
                    var exist = CheckName(model.Role.Name, model.Role.ID);
                    if (exist)
                        result.Message = string.Format(SystemMessage.FieldExist, "名稱");
                }

                //update
                if (string.IsNullOrEmpty(result.Message))
                {
                    data.Name = model.Role.Name.ToTrim();
                    data.MemberLevel = model.Role.MemberLevel;
                    data.AccountType = model.Role.AccountType;
                    data.UserContentTypes = model.Role.UserContentTypes;
                    data.UserRequiredTypes = model.Role.UserRequiredTypes;
                    data.Sort = model.Role.Sort;
                    data.IsEnabled = model.Role.IsEnabled;

                    data.mgt_RoleMenuRelation.Clear();
                    if (model.MenuCheckList != null)
                    {
                        foreach (var item in model.MenuCheckList.Where(x => x.IsChecked).ToList())
                        {
                            var relationData = new mgt_RoleMenuRelation
                            {
                                RoleID = data.ID,
                                MenuID = item.ID,
                                RoleActions = item.RoleActions,
                            };
                            Db.mgt_RoleMenuRelation.Add(relationData);
                        }
                    }

                    Db.SaveChanges();

                    result.Message = SystemMessage.UpdateSuccess;
                    result.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.UpdateFail;
                _Log.CreateText("Role_Update" + _Json.ModelToJson(ex));
            }

            return result;
        }

        public CiResult Delete(Guid id)
        {
            CiResult result = new CiResult();
            var data = Get(id);

            try
            {
                data.IsDelete = true;
                data.UpdateTime = DateTime.Now;
                Db.SaveChanges();

                result.Message = SystemMessage.DeleteSuccess;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.DeleteFail;
                _Log.CreateText("Role_Delete:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        #endregion

        #region Check

        /// <summary>
        /// 名稱是否重複
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public bool CheckName(string name, Guid? id)
        {
            var data = Query;
            // 排除自己
            if (id != null)
            {
                data = data.Where(x => x.ID != id);
                // 查無
                //if (Get(id.Value) == null)
                //{
                //    return true;
                //}
            }

            bool result = (data.FirstOrDefault(x => x.Name == name)) != null;

            return result;
        }

        #endregion

    }
}
