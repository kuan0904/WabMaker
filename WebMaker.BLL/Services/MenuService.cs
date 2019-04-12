using System;
using System.Collections.Generic;
using System.Linq;
using MyTool.Commons;
using MyTool.Enums;
using WebMaker.BLL.Services;
using WebMaker.Entity.Models;
using MyTool.ViewModels;
using MyTool.Services;
using System.Web.Routing;
using System.Web;
using WebMaker.Entity.ViewModels;

namespace WebMaker.BLL.Services
{
    /// <summary>
    /// 選單管理 (所有Client共用)
    /// </summary>
    public class MenuService : BaseService
    {
        /// <summary>
        /// 基本查詢條件
        /// </summary>
        private IQueryable<mgt_Menu> Query
        {
            get
            {
                return Db.mgt_Menu
                    .Where(x => !x.IsDelete);
            }
        }

        #region 後台

        /// <summary>
        /// 尋找所有節點(mgt_Menu編輯側邊樹、mgt_Menu parentId下拉選單、後台選單、Role checkbox)
        /// </summary>
        /// <param name="parentMenu">The parent menu.</param>
        /// <param name="type">後台、前台</param>
        /// <param name="onlyMenu">只顯示menu</param>
        /// <param name="rolePermissions">有權限的選單</param>
        /// <returns></returns>
        public List<TreeViewModel> GetTrees(mgt_Menu parentMenu, MenuType type, bool onlyMenu = false, bool checkEnabled = false
                                            , List<RolePermissionModel> rolePermissions = null, MemberLevel? memberLevel = null)
        {
            IEnumerable<mgt_Menu> treeData;
            bool addNode = true;

            // 第一層
            if (parentMenu == null)
            {
                treeData = Query.Where(x => x.ParentID == null
                                         && x.Type == (int)type)
                                        .OrderBy(x => x.Sort).ToList();
            }
            else
            {
                treeData = parentMenu.ChildMenus.Where(x => x.IsDelete == false)
                                                .OrderBy(x => x.Sort).ToList();
            }

            if (checkEnabled || onlyMenu)
            {
                treeData = treeData.Where(x => x.IsEnabled);
            }
            if (onlyMenu)
            {
                treeData = treeData.Where(x => x.IsMenu);
            }

            #region 取得使用者有權限的mgt_Menu
            if (onlyMenu && !IsSuperManager)
            {
                treeData = treeData.Where(x => rolePermissions.Any(y => y.MenuID == x.ID));
                // super管理員的Menu、設定角色全限時 不用篩選menu
            }
            #endregion

            var tree = new List<TreeViewModel>();
            foreach (var item in treeData)
            {
                //todo menuid可能重複 role不同
                var permission = rolePermissions?.FirstOrDefault(x => x.MenuID == item.ID);

                var node = new TreeViewModel
                {
                    ID = item.ID,
                    Name = item.Name,
                    Type = TreeType.Menu,

                    Controller = item.Controller.ToEnumString<ControllerType>(),
                    Action = item.Action.ToEnumString<ActionType>(),
                    Url = item.Url,

                    // 角色是否包含選單
                    IsChecked = permission != null && permission?.MenuID != Guid.Empty,
                    IsEnabled = item.IsEnabled,
                    MenuActions = item.MenuActions,
                    RoleActions = permission?.RoleActions
                };

                #region 遞迴子層
                var child = item.ChildMenus.Where(x => x.IsDelete == false);
                if (onlyMenu) { child = child.Where(x => x.IsMenu); }

                int childCount = child.Count();
                if (childCount > 0)
                {
                    node.Nodes = GetTrees(item, type, onlyMenu, checkEnabled, rolePermissions);
                }
                #endregion

                #region 遞迴文章 接上Structure=文章的類型 (網站內容 Only Admin)
                if (onlyMenu && type == MenuType.Admin && item.Controller == (int)ControllerType.Article)
                {
                    var structureService = new StructureService { ClientID = ClientID };
                    var nodes = structureService.GetList(ItemType.Article, memberLevel: memberLevel);

                    if (nodes.Data.Any())
                    {
                        node.Nodes = nodes.Data.Where(x => x.IsEnabled).Select(x => new TreeViewModel
                        {
                            Name = x.Name,
                            Controller = node.Controller,
                            Action = node.Action,
                            Url = "?type=" + x.ID.ToString()
                        }).ToList();
                    }
                }
                #endregion

                #region 遞迴訂單 接上Structure=訂單的類型 (會員活動)
                if (onlyMenu && item.Controller == (int)ControllerType.Order)
                {
                    var structureService = new StructureService { ClientID = ClientID };
                    var nodes = structureService.GetList(ItemType.Order, memberLevel: memberLevel);

                    if (nodes.Data.Any())
                    {
                        if (type == MenuType.Admin)
                        {
                            //接在下一層
                            node.Nodes = nodes.Data.Where(x => x.IsEnabled).Select(x => new TreeViewModel
                            {
                                Name = x.OrderName,
                                Controller = node.Controller,
                                Action = node.Action,
                                Url = "?type=" + x.ID.ToString()
                            }).ToList();
                        }
                        else
                        { 
                            //Member: 往上推一層,跳過Order直接structure
                            var struNode = nodes.Data.Where(x => x.IsEnabled).Select(x => new TreeViewModel
                            {
                                Name = x.OrderName,
                                Controller = "Member",
                                Action = node.Controller,
                                Url = "?type=" + x.ID.ToString()
                            }).ToList();

                            tree.AddRange(struNode);
                            addNode = false;
                        }
                    }
                }
                #endregion

                if (addNode)
                {
                    tree.Add(node);
                }
            }

            return tree;
        }

        /// <summary>
        /// 取得單筆資料
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public mgt_Menu Get(Guid id)
        {
            var query = Query.FirstOrDefault(x => x.ID == id);
            return query;
        }

        /// <summary>
        /// 新增資料
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public CiResult<Guid> Create(mgt_Menu model)
        {
            CiResult<Guid> result = new CiResult<Guid>();

            //field required
            if (string.IsNullOrEmpty(model.Name))
            {
                result.Message = string.Format(SystemMessage.FieldNull, "");
            }
            else
            {
                try
                {
                    model.ID = Guid.NewGuid();
                    model.IsDelete = false;

                    Db.mgt_Menu.Add(model);
                    Db.SaveChanges();

                    result.Data = model.ID;
                    result.Message = SystemMessage.CreateSuccess;
                    result.IsSuccess = true;
                }
                catch (Exception ex)
                {
                    result.Message = SystemMessage.CreateFail;
                    _Log.CreateText("Menu_Create:" + _Json.ModelToJson(ex));
                }
            }

            return result;
        }

        /// <summary>
        /// 修改資料
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public CiResult Update(mgt_Menu model)
        {
            CiResult result = new CiResult();

            try
            {
                var data = Get(model.ID);

                data.Name = model.Name.ToTrim();
                data.Controller = model.Controller;
                data.Action = model.Action;
                data.MenuActions = model.MenuActions;
                data.Url = model.Url.ToTrim();
                data.ParentID = model.ParentID;
                data.IsMenu = model.IsMenu;
                data.Sort = model.Sort;
                data.IsEnabled = model.IsEnabled;
                Db.SaveChanges();

                result.Message = SystemMessage.UpdateSuccess;
                result.IsSuccess = true;

            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.UpdateFail;
                _Log.CreateText("Menu_Update" + _Json.ModelToJson(ex));
            }

            return result;
        }

        /// <summary>
        /// 刪除資料
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>是否執行成功</returns>
        public CiResult Delete(Guid id)
        {
            CiResult result = new CiResult();

            try
            {
                var data = Get(id);

                data.IsDelete = true;
                Db.SaveChanges();

                result.Message = "刪除成功。";
                result.IsSuccess = true;

            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.DeleteFail;
                _Log.CreateText("Menu_Delete:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        #endregion

        #region 前台


        #endregion

    }
}
