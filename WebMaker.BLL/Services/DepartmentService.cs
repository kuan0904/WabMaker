using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebMaker.Entity.Models;

namespace WebMaker.BLL.Services
{
    /// <summary>
    /// 部門管理
    /// </summary>
    public class DepartmentService : BaseService
    {
        #region Get
        private IQueryable<mgt_Department> Query
        {
            get
            {
                return Db.mgt_Department
                    .Where(x => !x.IsDelete && x.ClientID == ClientID);
            }
        }

        public List<TreeViewModel> GetTrees(mgt_Department parentData = null, bool getUser = false)
        {
            IEnumerable<mgt_Department> treeData;
            if (parentData == null)
            {
                treeData = Query.Where(x => x.ParentID == null)
                                .OrderBy(x => x.Sort);
            }
            else
            {
                treeData = parentData.ChildDepartments
                                .Where(x => !x.IsDelete)
                                .OrderBy(x => x.Sort);
            }

            var tree = new List<TreeViewModel>();
            foreach (var item in treeData)
            {
                var node = ItemToTreeNode(item);

                #region mgt_Department
                if (getUser)
                {
                    //var users = dept.mgt_Department.Where(x => !x.IsDelete)
                    //                       .OrderBy(x => x.Sort).ToList();

                    //if (users.Any())
                    //{

                    //    foreach (var user in users)
                    //    {
                    //        var nodeNode = new TreeViewModel
                    //        {
                    //            ID = user.ID,
                    //            Name = user.Name,
                    //            Type = TreeType.mgt_Department
                    //        };
                    //        node.Nodes.Add(nodeNode);
                    //    }
                    //}
                }
                #endregion

                #region 遞迴 Child

                var child = item.ChildDepartments.Where(x => !x.IsDelete);
                if (child.Any())
                {
                    node.Nodes.AddRange(GetTrees(item, getUser));
                }
                #endregion

                tree.Add(node);
            }

            return tree;
        }

        public TreeViewModel ItemToTreeNode(mgt_Department item)
        {
            var node = new TreeViewModel
            {
                ID = item.ID,
                Name = item.Name,
                Sort = item.Sort,
                IsEnabled = item.IsEnabled,
                Type = TreeType.Department,
                Nodes = new List<TreeViewModel>()
            };

            return node;
        }

        public mgt_Department Get(Guid id)
        {
            var query = Query.FirstOrDefault(x => x.ID == id);
            return query;
        }

        /// <summary>
        /// 取得下拉選單
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetSelectList(Guid? id = null, Guid? selectedID = null)
        {
            var result = new List<SelectListItem>();
            mgt_Department data = null;

            //DepartmentID 篩選以下
            if (id != null)
            {
                data = Get(id.Value);
                result.Add(new SelectListItem //自己
                {
                    Value = data.ID.ToString(),
                    Text = data.Name,
                    Selected = selectedID != null ? data.ID == selectedID.Value : false
                });
            }

            result.AddRange(GetTreeSelects(data, selectedID));
            return result;
        }

        private List<SelectListItem> GetTreeSelects(mgt_Department parentData = null, Guid? selectedID = null, int level = 0)
        {
            var tree = new List<SelectListItem>();

            IEnumerable<mgt_Department> treeData;
            if (parentData == null)
            {
                treeData = Query.Where(x => x.ParentID == null)
                                .OrderBy(x => x.Sort);
            }
            else
            {
                treeData = parentData.ChildDepartments
                                .Where(x => !x.IsDelete)
                                .OrderBy(x => x.Sort);
            }

            foreach (var item in treeData)
            {
                bool selected = selectedID != null ? item.ID == selectedID.Value : false;
                string preName = new String('-', level);

                var node = new SelectListItem
                {
                    Value = item.ID.ToString(),
                    Text = preName + item.Name,
                    Selected = selected
                };
                tree.Add(node);

                //遞迴 Child
                var child = item.ChildDepartments.Where(x => !x.IsDelete);
                if (child.Any())
                {
                    tree.AddRange(GetTreeSelects(item, selectedID, level + 1));
                }
            }

            return tree;
        }
        #endregion

        #region Edit

        public CiResult<Guid> Create(mgt_Department model)
        {
            CiResult<Guid> result = new CiResult<Guid>();

            try
            {
                //field required
                if (string.IsNullOrEmpty(model.Name))
                {
                    result.Message = string.Format(SystemMessage.FieldNull, "");
                }

                //get SystemNumber
                string systemNumber = "";
                if (string.IsNullOrEmpty(result.Message))
                {
                    var sysResult = CreateSystemNumber(DataTableCode.mgt_Department);
                    if (!sysResult.IsSuccess)
                        result.Message = sysResult.Message;

                    systemNumber = sysResult.Data;
                }

                //create
                if (string.IsNullOrEmpty(result.Message))
                {
                    var data = new mgt_Department
                    {
                        ID = Guid.NewGuid(),
                        ClientID = ClientID,
                        ParentID = model.ParentID,
                        SystemNumber = systemNumber,
                        Name = model.Name.ToTrim(),
                        Address = model.Address,
                        Phone = model.Phone,
                        Sort = model.Sort,
                        IsEnabled = true, //todo Enabled
                        IsDelete = false,
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now
                    };
                    Db.mgt_Department.Add(data);
                    Db.SaveChanges();

                    result.Data = data.ID;
                    result.Message = SystemMessage.CreateSuccess;
                    result.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.CreateFail;
                _Log.CreateText("Department_Create:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        public CiResult Update(mgt_Department model)
        {
            CiResult result = new CiResult();
            mgt_Department data = Get(model.ID);

            try
            {
                data.ParentID = model.ParentID;
                data.Name = model.Name.ToTrim();
                data.Address = model.Address;
                data.Phone = model.Phone;
                data.Sort = model.Sort;
                Db.SaveChanges();

                result.Message = SystemMessage.UpdateSuccess;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.UpdateFail;
                _Log.CreateText("Department_Update" + _Json.ModelToJson(ex));
            }

            return result;
        }

        public CiResult Delete(Guid id)
        {
            CiResult result = new CiResult();
            mgt_Department data = Get(id);

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
                _Log.CreateText("Department_Delete:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        public CiResult Sort(List<Guid> IDs)
        {
            CiResult result = new CiResult();

            try
            {
                var datalist = Query.Where(x => IDs.Contains(x.ID)).ToList();
                var i = 0;//IDs.Count();

                foreach (var id in IDs)
                {
                    var data = datalist.Find(x => x.ID == id);
                    data.Sort = i;
                    i++;
                }

                Db.SaveChanges();

                result.Message = SystemMessage.SortSuccess;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.SortFail;
                _Log.CreateText("Department_Sort:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        #endregion
    }
}
