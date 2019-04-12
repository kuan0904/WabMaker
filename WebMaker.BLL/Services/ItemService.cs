using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.Tools;
using MyTool.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using WebMaker.Entity.Models;
using WebMaker.Entity.ViewModels;

namespace WebMaker.BLL.Services
{
    /// <summary>
    /// 項目管理 (分類、文章...)
    /// </summary>
    /// <seealso cref="WebMaker.BLL.Services.BaseService" />
    public class ItemService : BaseService
    {
        #region Get

        private StructureService structureService = new StructureService();

        private IQueryable<cms_Item> Query
        {
            get
            {
                return Db.cms_Item
                    .Include(x => x.cms_Structure)
                    .Include(x => x.cms_ItemLanguage)
                    .Include(x => x.cms_ItemFile)
                    .Include(x => x.ParentItemRelations)
                    .Where(x => !x.IsDelete && x.ClientID == ClientID);
            }
        }

        public PageModel<ItemViewModel> GetListView(PageParameter param, ItemFilter filter)
        {
            bool selectSearchString = !string.IsNullOrWhiteSpace(filter.SearchString);
            //language (Default in Structure)
            var query = Query.Where(x => x.cms_ItemLanguage.Any(y => y.LanguageType == (int)filter.LangType
                                                                //isEnabled 
                                                                && (!filter.SelectEnabled || y.IsEnabled)

                                                                //searchString
                                                                && (!selectSearchString ||

                                                                   (filter.SearchType == SearchType.tag
                                                                   && y.Keywords.Contains(filter.SearchString)) ||

                                                                filter.SearchType == SearchType.all && (y.Subject.Contains(filter.SearchString)
                                                                || y.Description.Contains(filter.SearchString)
                                                                || y.Content.Contains(filter.SearchString)
                                                                || y.Keywords.Contains(filter.SearchString)
                                                                || y.Author.Contains(filter.SearchString)))
                                                           ));

            //structure
            if (filter.StructureIDs != null)
            {
                query = query.Where(x => filter.StructureIDs.Contains(x.StructureID));
            }
            else
            {
                query = null;
            }

            //部門權限
            if (filter.DepartmentIDs != null)
            {
                query = query.Where(x => x.DepartmentID != null && filter.DepartmentIDs.Contains(x.DepartmentID.Value));
            }

            //category
            if (filter.CategoryIDs != null)
            {
                query = query.Where(x => x.ParentItemRelations.Any(y => filter.CategoryIDs.Contains(y.ParentID)));
            }

            //except
            if (filter.ExceptID != null)
            {
                query = query.Where(x => x.ID != filter.ExceptID);
            }

            //isTop
            if (filter.SelectTop)
            {
                query = query.Where(x => x.IsTop);
            }

            //CoverType
            if (filter.CoverType != null)
            {

                query = query.Where(x => x.cms_ItemFile.Any(y => y.SourceType == (int)SourceType.ItemConver
                                                              && y.FileType == (int)filter.CoverType));
            }

            //paging           
            var pagedModel = PageTool.CreatePage(query.ToList(), param);
            var model = new PageModel<ItemViewModel>
            {
                CurrentPage = pagedModel.CurrentPage,
                PageSize = pagedModel.PageSize,
                TotalCount = pagedModel.TotalCount,
                PageCount = pagedModel.PageCount,
                DataStart = pagedModel.DataStart,
                DataEnd = pagedModel.DataEnd,
                Data = pagedModel.Data.Select(x => ToViewModel(x, filter.LangType, param.DataLevel)).ToList()
            };

            return model;
        }

        public List<ItemViewModel> GetListView(List<Guid> IDs, LanguageType langType)
        {

            var query = Query.Where(x => IDs.Contains(x.ID)
                                      && x.cms_ItemLanguage.Any(y => y.LanguageType == (int)langType)).ToList();

            var model = query.Select(x => ToViewModel(x, langType, DataLevel.Simple)).ToList();
            return model;
        }

        private ItemViewModel ToViewModel(cms_Item item, LanguageType? langType, DataLevel dataLevel)
        {
            var model = new ItemViewModel
            {
                Item = item,
                ItemFiles = item.cms_ItemFile.Where(y => !y.IsDelete).OrderBy(x => x.Sort).ToList(),
                BreadCrumbs = new List<BreadCrumb>(),
                ParentItems = new List<BreadCrumb>(),
                ParentID = item.ParentItemRelations.FirstOrDefault(x => x.IsCrumb)?.ParentID ?? Guid.Empty
            };

            //---一般---
            if (dataLevel >= DataLevel.Simple)
            {
                model.LangNodes = LangNode(item.cms_ItemLanguage.ToList(), item.cms_Structure.LanguageTypes);
            }

            //語系
            if (langType != null)
            {
                model.ItemLanguage = item.cms_ItemLanguage.First(y => y.LanguageType == (int)langType);

                if (dataLevel >= DataLevel.Simple)
                {
                    //parent
                    var parentItemRelations = item.ParentItemRelations.Where(x => !x.ParentItem.IsDelete).OrderByDescending(x => x.IsCrumb).ToList();
                    foreach (var parent in parentItemRelations)
                    {
                        var itemSubject = parent.ParentItem.cms_ItemLanguage.FirstOrDefault(y => y.LanguageType == (int)langType)?.Subject;

                        model.ParentItems.Add(new BreadCrumb
                        {
                            ID = parent.ParentItem.ID,
                            RouteName = parent.ParentItem.RouteName,
                            Subject = itemSubject,
                            ItemTypes = parent.ParentItem.cms_Structure.ItemTypes,
                            StructureID = parent.ParentItem.StructureID
                        });
                    }

                    //麵包屑 loop       
                    while (item.ParentItemRelations.FirstOrDefault(x => x.IsCrumb) != null)
                    {
                        item = item.ParentItemRelations.FirstOrDefault(x => x.IsCrumb)?.ParentItem;
                        var itemSubject = item.cms_ItemLanguage.FirstOrDefault(y => y.LanguageType == (int)langType)?.Subject;

                        if (string.IsNullOrWhiteSpace(itemSubject) || string.IsNullOrWhiteSpace(item.RouteName))
                            break;

                        model.BreadCrumbs.Insert(0, new BreadCrumb
                        {
                            ID = item.ID,
                            RouteName = item.RouteName,
                            Subject = itemSubject,
                            ItemTypes = item.cms_Structure.ItemTypes,
                        });
                    }
                }
            }//emd LangType


            //---完整資料---
            if (dataLevel == DataLevel.Normal)
            {
                model.UserProfile = item.mgt_UserProfile.FirstOrDefault();//for 人才庫列表篩選
            }

            return model;
        }

        public List<TreeViewModel> GetTrees(cms_Item parentData, ItemTreeFilter filter)
        {
            IEnumerable<cms_Item> treeData;
            List<cms_Item> treeQuery;

            if (parentData == null)
            {
                treeData = Query.Where(x => !x.ParentItemRelations.Any());
                //structure
                if (filter.TopStructures != null)
                {
                    treeData = treeData.Where(x => filter.TopStructures.Contains(x.StructureID));
                }
            }
            else
            {
                treeData = parentData.ChildItemRelations.Select(x => x.ChildItem)
                                .Where(x => !x.IsDelete);
            }

            #region filter
            //ItemType      
            treeData = treeData.Where(x =>
                    // 項目類型
                    x.cms_Structure.ItemTypes.HasValue((int)filter.ItemType)
                    );

            //保留篩選繼續遞迴
            treeQuery = treeData.Where(x =>
                    //MenuPosition
                    (filter.MenuPosition == null || x.MenuPositions.HasValue((int)filter.MenuPosition))

                     //指定Language
                     && x.cms_ItemLanguage.Any(y => y.LanguageType == (int)filter.LangType

                                                 //isEnabled 
                                                 && (!filter.SelectEnabled || y.IsEnabled)

                     )).ToList();

            #endregion

            treeData = treeData.OrderBy(x => x.Sort);
            var tree = new List<TreeViewModel>();

            foreach (var item in treeData.ToList())
            {
                var node = TreeNode(item, filter.LangType, filter.CheckIDs);

                //Item結構的結尾必須接上指定結構
                if (filter.EndWithStructureID != null && !LoopEndStructure(item, filter.EndWithStructureID.Value, filter.ItemType))
                {
                    continue;
                }

                #region 遞迴 Child Client

                var child = item.ChildItemRelations.Select(x => x.ChildItem)
                                .Where(x => !x.IsDelete && x.cms_Structure.ItemTypes.HasValue((int)filter.ItemType));

                if (child.Any())
                {
                    //EmptyContinue(父層不包含也繼續遞迴子層) 子層推前一層
                    if (filter.EmptyContinue)
                    {
                        tree.AddRange(GetTrees(item, filter));
                    }
                    //普通遞迴
                    else
                    {
                        node.Nodes.AddRange(GetTrees(item, filter));
                    }
                }
                //else if (filter.EndWithStructureID != null
                //    && !item.cms_Structure.ChildStructures.Any(x => x.ID == filter.EndWithStructureID.Value && !x.IsDelete))
                //{                   
                //    continue;
                //}
                #endregion

                #region 遞迴Structure
                //Category接上遞迴Structure
                if (filter.JoinStructureTree)
                {
                    structureService.ClientID = ClientID;
                    node.Nodes.AddRange(structureService.GetTrees(item.cms_Structure, filter.ItemType));
                }
                #endregion


                //通過篩選 or EmptyContinue(父層不包含也繼續遞迴子層)
                if (treeQuery.Contains(item))
                {
                    tree.Add(node);
                }
            }

            return tree;
        }

        private bool LoopEndStructure(cms_Item item, Guid endWithStructureID, ItemType itemType)
        {
            bool result = false;
            var child = item.ChildItemRelations.Select(x => x.ChildItem)
                               .Where(x => !x.IsDelete && x.cms_Structure.ItemTypes.HasValue((int)itemType));

            if (child.Any())
            {
                foreach (var c in child)
                {
                    //任何包含endWithStructureID: return true
                    result = LoopEndStructure(c, endWithStructureID, itemType);
                    if (result)
                        break;
                }
            }
            //else
            //{
            if (item.cms_Structure.ChildStructures.Any(x => x.ID == endWithStructureID && !x.IsDelete))
            {
                //Item結構的結尾必須接上指定結構
                return true;
            }
            //}

            return result;
        }

        public TreeViewModel TreeNode(cms_Item item, LanguageType? langType = null, List<Guid> checkIDs = null)
        {
            var itemLangList = item.cms_ItemLanguage.ToList();
            var itemLang = langType != null ?
                           //指定切換語系
                           itemLangList.FirstOrDefault(x => x.LanguageType == (int)langType) :
                           //預設語系
                           itemLangList.FirstOrDefault(x => x.LanguageType == item.cms_Structure.DefaultLanguage) ??
                           //任一語系
                           itemLangList.FirstOrDefault();

            var menuPositions = string.Join(", ", _Enum.ToContainStrList<MenuPosition>(item.MenuPositions));
            var node = new TreeViewModel
            {
                ID = item.ID,
                Name = itemLang.Subject,
                Description = item.cms_Structure.Name + //結構名稱
                             (string.IsNullOrEmpty(menuPositions) ? "" : ", ") + menuPositions,//包含版位
                Sort = item.Sort,
                //IsEnabled = item.IsEnabled,
                IsChecked = checkIDs != null && checkIDs.Contains(item.ID),

                Url = item.RouteName,
                Type = TreeType.Item,
                Nodes = new List<TreeViewModel>(),
                TreeLangs = LangNode(itemLangList, item.cms_Structure.LanguageTypes),
            };

            return node;
        }

        private List<LangNode> LangNode(List<cms_ItemLanguage> itemLangList, string LanguageTypes)
        {
            var langNode = LanguageTypes.ToContainList<LanguageType>()
                            .Select(lang => new LangNode
                            {
                                Lang = lang,
                                Status = itemLangList.Any(x => x.LanguageType == (int)lang) ?
                                         itemLangList.FirstOrDefault(x => x.LanguageType == (int)lang).IsEnabled ?
                                         LanguageStatus.Show : LanguageStatus.Hide : LanguageStatus.None
                            }).ToList();

            return langNode;
        }


        public cms_Item Get(Guid id)
        {
            var query = Query.FirstOrDefault(x => x.ID == id);
            return query;
        }

        public ItemViewModel GetByRoteName(string routeName, LanguageType langType, bool addViewCount = false)
        {
            var query = Query.FirstOrDefault(x => x.RouteName == routeName
                                          && x.cms_ItemLanguage.Any(y => y.LanguageType == (int)langType && y.IsEnabled));

            if (query != null)
            {
                //Update ViewCount (Banner不計viewcount)
                if (addViewCount && (!query.cms_Structure.ItemTypes.HasValue(((int)ItemType.Banner))))
                {
                    try
                    {
                        query.ViewCount += 1;
                        Db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _Log.CreateText("addViewCount:" + _Json.ModelToJson(ex));
                    }
                }

                return ToViewModel(query, langType, DataLevel.Normal);
            }
            return null;
        }

        public ItemViewModel GetView(Guid id, LanguageType? langType = null, DataLevel dataLevel = DataLevel.Normal)
        {
            var query = Get(id);
            return ToViewModel(query, langType, dataLevel);
        }
        
        /// <summary>
        /// 取得訂單文章子項目
        /// </summary>
        /// <returns></returns>
        /// <param name="structureID">訂單文章結構</param>
        /// <param name="langType">訂單文章</param>
        /// <returns></returns>
        public PageModel<ItemViewModel> GetSubOrderList(Guid optionStructureID, Guid itemID, LanguageType langType)
        {
            //var orderItemStructure = Db.cms_Structure.Find(structureID).ChildStructures.FirstOrDefault(x => x.ItemTypes.HasValue((int)ItemType.OrderItem));
            var orderItems = GetListView(
                new PageParameter
                {
                    IsPaged = false,
                    SortColumn = SortColumn.Sort
                },
                new ItemFilter
                {
                    StructureIDs = optionStructureID.ToListObject(),
                    CategoryIDs = itemID.ToListObject(),
                    LangType = langType,
                }
            );

            return orderItems;
        }

        public List<SelectListItem> GetArticleGroup(cms_Structure itemStructure, LanguageType langType, Guid? selectedID = null)
        {
            var result = new List<SelectListItem>();

            var parentStructures = itemStructure.ParentStructure?.Where(x => x.ItemTypes.HasValue((int)ItemType.ArticleGroup)).ToList();
            if (parentStructures.Any())
            {
                var orderItems = GetListView(
                   new PageParameter
                   {
                       IsPaged = false
                   },
                   new ItemFilter
                   {
                       StructureIDs = parentStructures.Select(x => x.ID).ToList(),
                       LangType = langType
                   }
               );

                result = orderItems.Data.Select(x => new SelectListItem
                {
                    Text = x.ItemLanguage.Subject,
                    Value = x.Item.ID.ToString(),
                    Selected = selectedID != null ? x.Item.ID == selectedID : false
                }).ToList();
            }
            return result;
        }

        public List<cms_ItemFile> GetItemFiles(Guid itemID, Guid fileStructureID)
        {
            var query = Query.FirstOrDefault(x => x.ID == itemID);
            var model = query.cms_ItemFile.Where(x => x.StructureID == fileStructureID && !x.IsDelete)
                                          .OrderBy(x => x.Sort).ToList();
            return model;
        }

        public CiResult<List<TagModel>> GetAllTags(TagType type)
        {
            var result = new CiResult<List<TagModel>>();
            try
            {
                var data = Db.Database.SqlQuery<TagModel>("Proc_GetAllTags @ColumnName, @ClientID",
                         new SqlParameter("@ColumnName", type.ToString()),
                         new SqlParameter("@ClientID", ClientID)
                         ).ToList();

                result.Data = data;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.TagError;
                _Log.CreateText("GetTags:" + _Json.ModelToJson(ex));
            }
            return result;
        }

        public int GetSort(Guid structureID)
        {
            return Query.Count(x => x.StructureID == structureID);
        }

        /// <summary>
        /// 取得Item訂單角色 for Admin
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public List<CheckBoxListItem> GetOrderRoleCheckList(Guid? id, ItemOrderRoleType type)
        {
            List<Guid> ids = new List<Guid>();
            if (id != null)
            {
                // 擁有角色
                ids = Get(id.Value)?.cms_ItemOrderRoleRelation
                                    .Where(x => !x.mgt_Role.IsDelete && x.ItemOrderRoleType == (int)type).Select(x => x.RoleID).ToList();
            }

            // 全部角色
            var roleService = new RoleService { ClientID = ClientID };
            var roleList = roleService.GetList(new PageParameter(), AccountType.Member);

            var checkList = new List<CheckBoxListItem>();

            // 帳號是否包含角色
            foreach (var role in roleList.Data)
            {
                var check = new CheckBoxListItem
                {
                    ID = role.ID,
                    Text = role.Name,
                    IsChecked = (id != null) && ids.Contains(role.ID)
                };
                checkList.Add(check);
            }

            return checkList;
        }

        /// <summary>
        /// 取得Item訂單角色 for Member
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public List<SelectListItem> GetOrderRoleSelectList(Guid id, ItemOrderRoleType type)
        {
            var selectList = Get(id)?.cms_ItemOrderRoleRelation
                        .Where(x => !x.mgt_Role.IsDelete && x.ItemOrderRoleType == (int)type)
                        .OrderBy(x => x.mgt_Role.Sort)
                        .Select(x => new SelectListItem
                        {
                            Value = x.RoleID.ToString(),
                            Text = x.mgt_Role.Name
                        }).ToList();

            return selectList;
        }
        #endregion

        #region Report

        private List<erp_OrderUnit> QueryUnitList(Guid articleID)
        {
            return Db.erp_OrderUnit.Where(x => x.erp_Order.OrderStatus == (int)OrderStatus.Done && x.erp_Order.ItemID == articleID)
                                            .OrderBy(x => x.TempNo).ThenBy(x => x.Unit).ToList();
        }

        private List<mgt_UserProfile> QueryMemberList(Guid articleID)
        {
            return Db.mgt_UserProfile.Where(x => x.erp_Order.OrderStatus == (int)OrderStatus.Done && x.erp_Order.ItemID == articleID)
                                           .OrderBy(x => x.TempNo).ThenBy(x => x.erp_OrderUnit.Unit).ToList();
        }

        /// <summary>
        /// 單位清單
        /// </summary>
        /// <param name="articleID">The article identifier.</param>
        /// <returns></returns>
        public List<CompetitionUnitsModel> GetCompetitionUnits(Guid articleID)
        {
            var data = new List<CompetitionUnitsModel>();

            try
            {
                #region old
                ////所有訂單明細
                //var orderDetailAll = Db.erp_OrderDetail.Where(x => x.erp_Order.ItemID == articleID && x.erp_Order.OrderStatus == (int)OrderStatus.Done).ToList();

                ////所有選手
                //var members = Db.mgt_UserProfile.Where(x => x.erp_Order.OrderStatus == (int)OrderStatus.Done && x.erp_Order.ItemID == articleID)
                //                               .OrderBy(x => x.Unit).ToList();

                ////單位列表
                //data = members
                //        .GroupBy(x => new
                //        {
                //            x.erp_Order.mgt_User.Name,
                //            x.erp_Order.mgt_User.Phone,
                //            x.erp_Order.mgt_User.Email,
                //            x.erp_Order.OrderNumber,
                //            //x.erp_Order.TotalPrice, //單位拆開 金額從報名成員取
                //            x.Unit,
                //            x.UnitShort,
                //            x.HouseholdAddress,
                //            x.erp_Order.Coach,
                //            x.erp_Order.Leader,
                //            x.erp_Order.Manager,
                //        })
                //        .Select(x => new CompetitionUnitsModel
                //        {
                //            //TempNo = x.TempNo,
                //            Creater = x.Key.Name,
                //            Phone = _Crypto.DecryptAES(x.Key.Phone, Setting.UserCryptoKey), //解密Key     
                //            Email = x.Key.Email,
                //            OrderNumber = x.Key.OrderNumber,

                //            Unit = x.Key.Unit,
                //            UnitShort = x.Key.UnitShort,
                //            County = x.Key.HouseholdAddress,
                //            Coach = x.Key.Coach,
                //            Leader = x.Key.Leader,
                //            Manager = x.Key.Manager,
                //            MemberCount = x.Count(),
                //            Members = x.Select(y => new CompetitionItemTeamMember()
                //            {
                //                TempNo = y.TempNo,
                //                MemberName = y.NickName

                //            }).ToList(),
                //        }).ToList();

                #endregion

                //所有單位
                data = QueryUnitList(articleID).Select(x => new CompetitionUnitsModel
                {
                    Creater = x.mgt_User.Name,
                    Phone = _Crypto.DecryptAES(x.mgt_User.Phone, Setting.UserCryptoKey), //解密Key     
                    Email = x.mgt_User.Email,
                    OrderNumber = x.erp_Order.OrderNumber,

                    TempNo = x.TempNo,
                    Unit = x.Unit,
                    UnitShort = x.UnitShort,
                    County = x.County,
                    Coach = x.Coach,
                    Leader = x.Leader,
                    Manager = x.Manager,
                    MemberCount = x.mgt_UserProfile.Count,
                    Members = x.mgt_UserProfile.Select(y => new CompetitionItemTeamMember()
                    {
                        TempNo = y.TempNo,
                        MemberName = y.NickName

                    }).ToList(),

                }).ToList();
            }
            catch (Exception e)
            {
                _Log.CreateText("GetCompetitionUnits:" + _Json.ModelToJson(e));
            }

            return data;
        }

        /// <summary>
        /// 項目清單
        /// </summary>
        /// <param name="articleID">The article identifier.</param>
        /// <returns></returns>
        public List<CompetitionItemsModel> GetCompetitionItems(Guid articleID)
        {
            var data = new List<CompetitionItemsModel>();

            try
            {
                //所有訂單明細
                var orderDetailAll = Db.erp_OrderDetail.Where(x => x.erp_Order.ItemID == articleID && x.erp_Order.OrderStatus == (int)OrderStatus.Done).ToList();

                //所有組別、項目
                var items = Query.Where(x => !x.IsDelete && x.ParentItemRelations.Any(y => y.ParentID == articleID)).OrderByDescending(x => x.Sort).ToList();
                foreach (var item in items)
                {
                    if (!string.IsNullOrWhiteSpace(item.Options))
                    {
                        var options = item.Options.Split(',');
                        foreach (var option in options)
                        {
                            var orderDetail = orderDetailAll.Where(x => x.ItemID == item.ID && x.Option == option).ToList();
                            data.Add(new CompetitionItemsModel
                            {
                                ID = item.ID,
                                Subject = item.cms_ItemLanguage.FirstOrDefault().Subject,
                                Option = option,
                                TeamCount = orderDetail.Count(),
                                Teams = orderDetail.Select(x => new CompetitionItemTeam
                                {
                                    DetailTeamName = x.DetailTeamName,
                                    FilePath = x.FilePath,
                                    DetailPrice = x.SalePrice * (decimal)x.Quantity,  //計算價格 = 單價*數量
                                    DiscountPrice = x.erp_OrderDiscount.Sum(y => y.DiscountPrice), //所有優惠
                                    Members = x.TeamMembers.Select(y => new CompetitionItemTeamMember
                                    {
                                        TempNo = y.TempNo,
                                        MemberName = y.NickName,
                                        Unit = y.erp_OrderUnit?.Unit
                                    }).ToList()
                                }).ToList()
                            });
                        }//end foreach
                    }
                }
            }
            catch (Exception e)
            {
                _Log.CreateText("GetCompetitionItems:" + _Json.ModelToJson(e));
            }

            return data;
        }

        /// <summary>
        /// 選手清單
        /// </summary>
        /// <param name="articleID">The article identifier.</param>
        /// <returns></returns>
        public List<CompetitionMembersModel> GetCompetitionMembers(Guid articleID)
        {
            var data = new List<CompetitionMembersModel>();

            try
            {
                //所有訂單明細
                var orderDetailAll = Db.erp_OrderDetail.Where(x => x.erp_Order.ItemID == articleID && x.erp_Order.OrderStatus == (int)OrderStatus.Done).ToList();

                //所有選手             
                var members = QueryMemberList(articleID);
                foreach (var member in members)
                {
                    var orderDetail = orderDetailAll.Where(x => x.TeamMembers.Contains(member)).ToList();
                    data.Add(new CompetitionMembersModel
                    {
                        TempNo = member.TempNo,
                        MemberName = member.NickName,
                        IdentityCard = _Crypto.DecryptAES(member.IdentityCard, Setting.OrderCryptoKey),
                        Birthday = member.Birthday,
                        Gender = member.Gender,
                        County = member.erp_OrderUnit?.County,
                        Unit = member.erp_OrderUnit?.Unit,
                        Coach = member.erp_OrderUnit?.Coach,
                        ItemCount = orderDetail.Count(),
                        Items = orderDetail.Select(x => new CompetitionMemberItem
                        {
                            Subject = x.ItemSubject,
                            Option = x.Option,
                            DetailPrice = x.SalePrice,//單價
                            DiscountPrice = x.erp_OrderDiscount.Sum(y => y.DiscountPrice) / x.TeamMembers.Count() // 所有優惠/人數
                        }).ToList()
                    });

                }
            }
            catch (Exception e)
            {
                _Log.CreateText("GetCompetitionMembers:" + _Json.ModelToJson(e));
            }

            return data;
        }

        /// <summary>
        /// 建立選手、單位編號
        /// </summary>
        /// <param name="articleID">The article identifier.</param>
        /// <returns></returns>
        public CiResult SetCompetitionMembers(Guid articleID)
        {
            var result = new CiResult();

            try
            {
                var item = Get(articleID);

                //檢查-活動已結束
                //if (item.SaleEndTime != null && item.SaleEndTime.Value.AddDays(1) >= DateTime.Now)
                //{
                //    result.Message = "活動未結束";
                //}


                if (string.IsNullOrEmpty(result.Message))
                {

                    //檢查-付款完成 (只提醒)
                    if (string.IsNullOrEmpty(result.Message))
                    {
                        var orders = item.erp_Order.Where(x => x.OrderStatus == (int)OrderStatus.NonPayment);

                        if (orders.Any())
                        {
                            result.Message = string.Format("提醒: 尚有{0}筆待付款。<br>", orders.Count());
                        }
                    }


                    //---選手---
                    var members = QueryMemberList(articleID);
                    if (members.Any())
                    {
                        //全部歸零
                        members.ForEach(x => { x.TempNo = 0; });

                        int i = 1;
                        foreach (var data in members)
                        {
                            //身分證重複 使用同編號
                            var sameData = members.FirstOrDefault(x => x.TempNo != 0 && x.IdentityCard == data.IdentityCard);
                            if (sameData == null)
                            {
                                data.TempNo = i;
                                i++;
                            }
                            else
                            {
                                data.TempNo = sameData.TempNo;
                            }
                        }

                        Db.SaveChanges();
                        result.Message += string.Format("選手編號建立完成，共{0}筆。<br>", i);
                        result.IsSuccess = true;

                    }
                    else
                    {
                        result.Message += "無選手資料。<br>";
                    }


                    //---單位---
                    var units = QueryUnitList(articleID);
                    if (units.Any())
                    {
                        //全部歸零
                        units.ForEach(x => { x.TempNo = 0; });

                        int i = 1;
                        foreach (var data in units)
                        {
                            //縣市、單位名稱重複 使用同編號
                            var sameData = units.FirstOrDefault(x => x.TempNo != 0 && x.County == data.County && x.Unit == data.Unit);
                            if (sameData == null)
                            {
                                data.TempNo = i;
                                i++;
                            }
                            else
                            {
                                data.TempNo = sameData.TempNo;
                            }
                        }

                        Db.SaveChanges();
                        result.Message += string.Format("單位編號建立完成，共{0}筆。<br>", i);
                        result.IsSuccess = true;

                    }
                    else
                    {
                        result.Message += "無單位資料。<br>";
                    }


                }

            }
            catch (Exception e)
            {
                result.Message = SystemMessage.CreateFail;
                _Log.CreateText("SetCompetitionMembers:" + _Json.ModelToJson(e));
            }

            return result;
        }

        /// <summary>
        /// 比賽銷售和狀態統計
        /// </summary>
        /// <param name="ParentItemID">The parent item identifier.</param>
        /// <returns></returns>
        public List<CompetitionCountModel> GetCompetitionCount(Guid ParentItemID)
        {
            var data = new List<CompetitionCountModel>();
            try
            {
                data = Db.Database.SqlQuery<CompetitionCountModel>("Proc_GetCompetitionCount @ClientID, @ParentItemID",
                         new SqlParameter("@ParentItemID", ParentItemID),
                         new SqlParameter("@ClientID", ClientID)
                         ).ToList();

            }
            catch (Exception ex)
            {
                _Log.CreateText("GetCompetitionCount:" + _Json.ModelToJson(ex));
            }
            return data;
        }
        #endregion

        #region Edit
        public CiResult<Guid> Create(EditItemViewModel model)
        {
            CiResult<Guid> result = new CiResult<Guid>();

            try
            {
                #region RouteName
                //路由不開放編輯時, 在Create預設Guid random

                //從標題自動產生
                if (model.IsAutoRouteName && !string.IsNullOrWhiteSpace(model.ItemLanguage.Subject))
                {
                    model.Item.RouteName = _Check.RelpaceBadCharacters(model.ItemLanguage.Subject, "-") + "-" + DateTime.Now.ToString("yyMMddHHmmf");
                }

                //field required 
                model.Item.RouteName = model.Item.RouteName.ToTrim();
                if (string.IsNullOrWhiteSpace(model.Item.RouteName))
                {
                    result.Message = string.Format(SystemMessage.FieldNull, "路由名稱");
                }

                //check routename
                if (string.IsNullOrEmpty(result.Message))
                {
                    var exist = CheckRouteName(model.Item.RouteName, null);
                    if (exist)
                        result.Message = string.Format(SystemMessage.FieldExist, "路由名稱");
                }

                //bad characters
                if (!(_Check.IsRouteWords(model.Item.RouteName) && _Check.NobadCharacters(model.Item.RouteName)))
                {
                    result.Message = "路由名稱" + SystemMessage.BadCharacters;
                }

                #endregion

                //get SystemNumber
                string systemNumber = "";
                if (string.IsNullOrEmpty(result.Message))
                {
                    var sysResult = CreateSystemNumber(DataTableCode.cms_Item);
                    if (!sysResult.IsSuccess)
                        result.Message = sysResult.Message;

                    systemNumber = sysResult.Data;
                }

                //check enum              
                if (!Enum.IsDefined(typeof(LanguageType), model.ItemLanguage.LanguageType))
                {
                    result.Message = SystemMessage.LanguageError;
                }

                //create
                if (string.IsNullOrEmpty(result.Message))
                {
                    //main
                    model.Item.ID = Guid.NewGuid();
                    model.Item.ClientID = ClientID;
                    model.Item.SystemNumber = systemNumber;
                    model.Item.IsDelete = false;
                    model.Item.CreateTime = DateTime.Now;
                    model.Item.UpdateTime = DateTime.Now;
                    model.Item.CreateUser = model.Item.UpdateUser;

                    model.Item.cms_ItemLanguage.Add(model.ItemLanguage);

                    //relation
                    UpdateRelation(model.Item.ID, model.ParentID, model.CategoryTree);

                    //itemFile  
                    if (model.ItemFiles != null)
                    {
                        UpdateFile(model.Item.ID, model.ItemFiles);
                    }

                    //role check                       
                    UpdateItemAllowRoles(model.Item, model.AllowRoleIDs, ItemOrderRoleType.OrderAllowRole);
                    UpdateItemAllowRoles(model.Item, model.CreateRoleIDs, ItemOrderRoleType.OrderCreateRole);

                    Db.cms_Item.Add(model.Item);
                    Db.SaveChanges();


                    result.Data = model.Item.ID;
                    result.Message = SystemMessage.CreateSuccess;
                    result.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.CreateFail;
                _Log.CreateText("Item_Create:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        public CiResult Update(EditItemViewModel model, bool onlySubject = false)
        {
            CiResult result = new CiResult();
            //cms_Item data = Get(model.ID);

            try
            {
                //field required
                model.Item.RouteName = model.Item.RouteName.ToTrim();
                if (string.IsNullOrEmpty(model.Item.RouteName))
                {
                    result.Message = string.Format(SystemMessage.FieldNull, "路由名稱");
                }

                //check name
                if (string.IsNullOrEmpty(result.Message))
                {
                    var exist = CheckRouteName(model.Item.RouteName, model.Item.ID);
                    if (exist)
                        result.Message = string.Format(SystemMessage.FieldExist, "路由名稱");
                }

                //bad characters
                if (!(_Check.IsRouteWords(model.Item.RouteName) && _Check.NobadCharacters(model.Item.RouteName)))
                {
                    result.Message = "路由名稱" + SystemMessage.BadCharacters;
                }

                //update
                if (string.IsNullOrEmpty(result.Message))
                {
                    //item
                    var data = GetView(model.Item.ID, (LanguageType)model.ItemLanguage.LanguageType);
                    data.Item.DepartmentID = model.Item.DepartmentID;
                    data.Item.RouteName = model.Item.RouteName;
                    data.Item.IsTop = model.Item.IsTop;
                    data.Item.MenuPositions = model.Item.MenuPositions;
                    data.Item.Date = model.Item.Date;
                    data.Item.StartTime = model.Item.StartTime;
                    data.Item.EndTime = model.Item.EndTime;

                    data.Item.OriginalPrice = model.Item.OriginalPrice;
                    data.Item.SalePrice = model.Item.SalePrice;
                    data.Item.StockCount = model.Item.StockCount;
                    //data.Item.SaleCount = model.Item.SaleCount; //change in order
                    data.Item.SaleStartTime = model.Item.SaleStartTime;
                    data.Item.SaleEndTime = model.Item.SaleEndTime;
                    data.Item.PriceType = model.Item.PriceType;
                    data.Item.DiscountType = model.Item.DiscountType;
                    data.Item.PeopleMin = model.Item.PeopleMin;
                    data.Item.PeopleMax = model.Item.PeopleMax;
                    data.Item.SaleLimit = model.Item.SaleLimit;
                    data.Item.DateLimit = model.Item.DateLimit;

                    //data.Item.OrderAutoRole = model.Item.OrderAutoRole;
                    data.Item.Options = model.Item.Options;
                    //data.Item.RoleTimeLimitYear = model.Item.RoleTimeLimitYear; //remove

                    data.Item.Sort = model.Item.Sort;
                    data.Item.UpdateTime = DateTime.Now;

                    //itemLang
                    if (data.ItemLanguage == null)
                    {
                        //create
                        data.Item.cms_ItemLanguage.Add(model.ItemLanguage);
                    }
                    else
                    {
                        //from Category 只更新標題, 否則內容會被清空
                        if (onlySubject)
                        {
                            data.ItemLanguage.Subject = model.ItemLanguage.Subject.ToTrim();
                            data.ItemLanguage.IsEnabled = model.ItemLanguage.IsEnabled;
                        }
                        //update                     
                        else
                        {
                            UpdateLang(data.ItemLanguage, model.ItemLanguage);
                        }
                    }

                    //relation
                    data.Item.ParentItemRelations.Clear();
                    UpdateRelation(model.Item.ID, model.ParentID, model.CategoryTree);

                    //itemFile   
                    if (model.ItemFiles != null)
                    {
                        UpdateFile(model.Item.ID, model.ItemFiles, data.ItemFiles);
                    }

                    //role check                   
                    UpdateItemAllowRoles(data.Item, model.AllowRoleIDs, ItemOrderRoleType.OrderAllowRole);
                    UpdateItemAllowRoles(data.Item, model.CreateRoleIDs, ItemOrderRoleType.OrderCreateRole);

                    Db.SaveChanges();
                    result.Message = SystemMessage.UpdateSuccess;
                    result.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.UpdateFail;
                _Log.CreateText("Item_Update:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        private void UpdateLang(cms_ItemLanguage entity, cms_ItemLanguage model)
        {
            entity.Subject = model.Subject.ToTrim();
            entity.Description = model.Description;
            entity.Content = model.Content;
            entity.TemplateText = model.TemplateText;
            entity.CustomFormatText = model.CustomFormatText;
            entity.LinkUrl = model.LinkUrl;
            entity.IsBlankUrl = model.IsBlankUrl;
            entity.Keywords = model.Keywords;
            entity.Author = model.Author;
            entity.Phone = model.Phone;
            entity.Address = model.Address;
            entity.AddressInfo = model.AddressInfo;
            entity.DiscountText = model.DiscountText;
            entity.IsEnabled = model.IsEnabled;
        }
        private void UpdateRelation(Guid itemID, Guid parentID, List<TreeViewModel> categoryTree)
        {
            //data.Item.ParentItemRelations.Clear();

            //主關聯
            if (parentID != Guid.Empty)
            {
                var relation = new cms_ItemRelation
                {
                    ParentID = parentID,
                    ChildID = itemID,
                    IsCrumb = true
                };
                Db.cms_ItemRelation.Add(relation);
            }

            //其他關聯
            if (categoryTree != null)
            {
                foreach (var node in categoryTree.Where(x => x.IsChecked && x.ID != parentID).ToList())
                {
                    var relation = new cms_ItemRelation
                    {
                        ParentID = node.ID,
                        ChildID = itemID,
                        IsCrumb = false
                    };
                    Db.cms_ItemRelation.Add(relation);
                }
            }

        }
        private void UpdateFile(Guid itemID, List<cms_ItemFile> models, List<cms_ItemFile> entity = null)
        {
            foreach (var model in models)
            {
                if (model.ID != Guid.Empty)
                {
                    //update
                    var data = entity.FirstOrDefault(x => x.ID == model.ID);
                    data.Sort = model.Sort;
                    data.Subject = model.Subject.ToTrim();
                    data.IsDelete = model.IsDelete;
                    data.UpdateTime = DateTime.Now;
                    data.UpdateUser = model.UpdateUser;
                }
                else
                {
                    //create
                    model.ID = Guid.NewGuid();
                    model.ItemID = itemID;
                    model.ClientID = ClientID;
                    model.CreateTime = DateTime.Now;
                    model.UpdateTime = DateTime.Now;
                    Db.cms_ItemFile.Add(model);
                }
            }
        }

        private void UpdateItemAllowRoles(cms_Item item, List<Guid> RoleIDs, ItemOrderRoleType type)
        {
            var deletes = item.cms_ItemOrderRoleRelation.Where(x => x.ItemOrderRoleType == (int)type).ToList();
            if (deletes != null)
            {
                Db.cms_ItemOrderRoleRelation.RemoveRange(deletes);
            }

            if (RoleIDs != null)
            {
                foreach (var id in RoleIDs)
                {
                    //var role = Db.mgt_Role.Find(id);
                    //item.mgt_RoleAllows.Add(role);
                    Db.cms_ItemOrderRoleRelation.Add(new cms_ItemOrderRoleRelation
                    {
                        ItemID = item.ID,
                        RoleID = id,
                        ItemOrderRoleType = (int)type
                    });
                }
            }
        }

        public CiResult Delete(Guid id, Guid updateUser)
        {
            CiResult result = new CiResult();

            try
            {
                var data = Get(id);

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
                _Log.CreateText("Item_Delete:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        public CiResult UpdateItemFile(Guid itemID, List<cms_ItemFile> model)
        {
            CiResult result = new CiResult();

            try
            {
                //item
                var data = GetView(itemID);

                //iteFile                 
                UpdateFile(itemID, model, data.ItemFiles);

                Db.SaveChanges();

                result.Message = SystemMessage.UpdateSuccess;
                result.IsSuccess = true;

            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.UpdateFail;
                _Log.CreateText("ItemFile_Update:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        public CiResult Sort(List<Guid> IDs)
        {
            CiResult result = new CiResult();

            try
            {
                var datalist = Query.Where(x => IDs.Contains(x.ID)).ToList();
                var i = IDs.Count();

                foreach (var id in IDs)
                {
                    var data = datalist.Find(x => x.ID == id);
                    data.Sort = i;
                    i--;
                }

                Db.SaveChanges();

                result.Message = SystemMessage.SortSuccess;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.SortFail;
                _Log.CreateText("Item_Sort:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        #endregion

        #region Check

        /// <summary>
        /// 名稱是否重複
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public bool CheckRouteName(string name, Guid? id)
        {
            var data = Query;
            // 排除自己
            if (id != null)
            {
                data = data.Where(x => x.ID != id);
                // 查無 todo (無用??
                //if (Get(id.Value) == null)
                //{
                //    return true;
                //}
            }

            //路由名稱不分大小寫
            bool result = (data.FirstOrDefault(x => x.RouteName.ToLower() == name.ToLower())) != null;

            return result;
        }

        #endregion

        #region UserProfile

        public mgt_UserProfile GetUserProfileByItem(Guid itemID)
        {
            var query = Db.mgt_UserProfile.FirstOrDefault(x => x.ItemID == itemID);
            return query;
        }

        /// <summary>
        /// 修改文章人物資料
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public CiResult UpdateItemUserProfile(mgt_UserProfile model)
        {
            CiResult result = new CiResult();

            try
            {
                //itemID 必填
                if (model.ItemID.Value == null)
                {
                    result.Message = SystemMessage.Error;
                }

                if (string.IsNullOrEmpty(result.Message))
                {
                    var userService = new UserService { ClientID = ClientID };

                    //get
                    var data = GetUserProfileByItem(model.ItemID.Value);

                    //save
                    result = userService.UpdateUserProfile(data?.ID, model, Setting.UserCryptoKey);
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.UpdateFail;
                _Log.CreateText("Item_UpdateItemUserProfile:" + _Json.ModelToJson(ex));
            }

            return result;
        }
        #endregion
    }
}
