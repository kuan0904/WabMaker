using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.Tools;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebMaker.Entity.Models;

namespace WebMaker.BLL.Services
{
    /// <summary>
    /// 文章內容組成
    /// </summary>
    public class StructureService : BaseService
    {
        #region Get

        private IQueryable<cms_Structure> Query
        {
            get
            {
                return Db.cms_Structure
                   .Include(x => x.ParentStructure)
                   .Include(x => x.ChildStructures)
                   .Where(x => !x.IsDelete && x.ClientID == ClientID);
            }
        }

        public PageModel<cms_Structure> GetList(ItemType selectType, List<ItemType> exceptTypes = null, MemberLevel? memberLevel = null)
        {
            var param = new PageParameter
            {
                IsPaged = false,
                SortColumn = SortColumn.Sort,
                IsDescending = false
            };

            var query = Query.AsEnumerable();

            //篩選包含類型
            if (selectType == ItemType.Article)
            {
                query = query.Where(x => x.ItemTypes.HasValue((int)ItemType.Article)
                                      || x.ItemTypes.HasValue((int)ItemType.CategoryExpand));
            }
            else
            {
                query = query.Where(x => x.ItemTypes.HasValue((int)selectType));
            }

            if (exceptTypes != null)
            {
                foreach (var xType in exceptTypes)
                {
                    query = query.Where(x => !x.ItemTypes.HasValue((int)xType));
                }
            }

            //篩選權限 (排除限最高權限的)
            if (memberLevel != null && memberLevel.Value != MemberLevel.Highest)
            {
                query = query.Where(x => !x.ToolTypes.HasValue((int)ToolType.OnlyHighestAuthority));
            }

            var pagedModel = PageTool.CreatePage(query, param);
            return pagedModel;
        }

        public List<TreeViewModel> GetTrees(cms_Structure parentData = null, ItemType? itemType = null)
        {
            IEnumerable<cms_Structure> treeData;
            if (parentData == null)
            {
                treeData = Query.Where(x => !x.ParentStructure.Any())
                                .OrderBy(x => x.Sort);
            }
            else
            {
                treeData = parentData.ChildStructures
                                .Where(x => !x.IsDelete)
                                .OrderBy(x => x.Sort);
            }

            #region ItemType

            if (itemType != null)
            {
                treeData = treeData.Where(x => x.ItemTypes.HasValue((int)itemType.Value));
            }
            #endregion

            var tree = new List<TreeViewModel>();
            foreach (var item in treeData)
            {
                var node = ItemToTreeNode(item);

                #region 遞迴 Child Client

                var child = item.ChildStructures.Where(x => !x.IsDelete);
                if (child.Any())
                {
                    node.Nodes.AddRange(GetTrees(item, itemType));
                }
                #endregion

                tree.Add(node);
            }

            return tree;
        }

        public TreeViewModel ItemToTreeNode(cms_Structure item)
        {
            var node = new TreeViewModel
            {
                ID = item.ID,
                Name = item.Name,
                Description = item.SeqNo + ", " + string.Join(",", item.ItemTypes.ToContainStrList<ItemType>()),
                Sort = item.Sort,
                IsEnabled = item.IsEnabled,
                Type = TreeType.Structure,
                Nodes = new List<TreeViewModel>()
            };

            return node;
        }

        /// <summary>
        /// 取得樹狀最上層Parent
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public List<Guid> GetTopParents(Guid id)
        {
            var result = new List<cms_Structure>();
            var query = Query.FirstOrDefault(x => x.ID == id);

            //while (query?.ParentStructure != null)
            //{
            //    query = query.ParentStructure;
            //}

            //多個parent
            return FindMaxParents(query);
        }

        private List<Guid> FindMaxParents(cms_Structure data)
        {
            var result = new List<Guid>();

            if (data.ParentStructure.Any())
            {
                foreach (var stru in data.ParentStructure)
                {
                    result.AddRange(FindMaxParents(stru));
                }
            }
            else
            {
                result.Add(data.ID);
            }

            return result;
        }

        public cms_Structure Get(Guid id)
        {
            var query = Query.FirstOrDefault(x => x.ID == id);
            return query;
        }

        public List<cms_Structure> GetByParent(List<Guid> ParentIDs, ItemType itemType)
        {
            var query = Query.Where(x => x.ParentStructure.Any(y => ParentIDs.Contains(y.ID) && y.IsEnabled && !y.IsDelete)
                                    && x.IsEnabled).AsEnumerable();

            return query.Where(x => x.ItemTypes.HasValue((int)itemType)).ToList();
        }

        /// <summary>
        /// 取得下拉選單
        /// </summary>
        /// <param name="itemType">項目類型</param>
        /// <param name="selectFirstLevel">是否篩選第一層</param>
        /// <returns></returns>
        public List<SelectListItem> GetSelectList(ItemType itemType, Guid? selectedID = null, bool selectFirstLevel = false)
        {
            var result = new List<SelectListItem>();
            var query = Query.AsEnumerable();

            //包含類型          
            query = query.Where(x => x.ItemTypes.HasValue((int)itemType));

            //篩選第一層
            if (selectFirstLevel)
            {
                query = query.Where(x => !x.ParentStructure.Any());
            }

            query = query.OrderBy(x => x.Sort);
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
        public CiResult<Guid> Create(cms_Structure model, Guid? ParentID)
        {
            CiResult<Guid> result = new CiResult<Guid>();

            try
            {
                var parentData = Query.FirstOrDefault(x => x.ID == ParentID);

                if (model.SeqNo != 0)
                {
                    //使用已存在的Structure
                    var data = Query.FirstOrDefault(x => x.SeqNo == model.SeqNo);
                    parentData.ChildStructures.Add(data);
                }
                else
                {
                    //field required   
                    if (string.IsNullOrEmpty(model.Name))
                    {
                        result.Message = string.Format(SystemMessage.FieldNull, "");
                    }

                    //create
                    if (string.IsNullOrEmpty(result.Message))
                    {
                        model.ID = Guid.NewGuid();
                        model.ClientID = ClientID;
                        model.CreateTime = DateTime.Now;
                        model.UpdateTime = DateTime.Now;

                        //relation
                        if (parentData != null)
                        {
                            model.ParentStructure.Add(parentData);
                        }
                        Db.cms_Structure.Add(model);
                    }

                }//--end if SeqNo 

                Db.SaveChanges();

                result.Data = model.ID;
                result.Message = SystemMessage.CreateSuccess;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.CreateFail;
                _Log.CreateText("ItemStructure_Create:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        public CiResult Update(cms_Structure model)
        {
            CiResult result = new CiResult();
            //cms_Structure data = Get(model.ID);

            try
            {
                //field required
                if (string.IsNullOrEmpty(model.Name))
                {
                    result.Message = string.Format(SystemMessage.FieldNull, "");
                }

                //update
                if (string.IsNullOrEmpty(result.Message))
                {
                    model.UpdateTime = DateTime.Now;
                    EntityUpdate(model, new List<string> { "SeqNo", "ClientID", "CreateTime", "IsDelete" });
                    //Db.SaveChanges();

                    result.Message = SystemMessage.UpdateSuccess;
                    result.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.UpdateFail;
                _Log.CreateText("ItemStructure_Update" + _Json.ModelToJson(ex));
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
                _Log.CreateText("ItemStructure_Delete:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        #endregion

    }
}
