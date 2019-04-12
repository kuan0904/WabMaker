using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WabMaker.Web.Helpers;
using WabMaker.Web.WebViewModels;
using WebMaker.BLL.Services;
using WebMaker.Entity.Models;
using WebMaker.Entity.ViewModels;

namespace WabMaker.Web.MainService
{
    /// <summary>
    /// Item基本處理 前端和Api共用
    /// </summary>
    public class ItemMainService
    {
        private ItemService service = new ItemService();
        private StructureService structureService = new StructureService();

        public ItemMainService()
        {
            service.ClientID = ApplicationHelper.ClientID;
            structureService.ClientID = ApplicationHelper.ClientID;
        }

        #region menu tree
        /// <summary>
        /// 取得選單 by cache
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public CiResult<List<TreeWebViewModel>> GetMenuByCache(MenuPosition position)
        {
            var result = new CiResult<List<TreeWebViewModel>>();
            var cacheHelper = new CacheHelper();
            var chacheName = $"{ApplicationHelper.ClientID}_menu_{position.ToString()}";

            //get cache           
            result.Data = cacheHelper.Get<List<TreeWebViewModel>>(chacheName);

            //set cache
            if (result.Data == null)
            {
                result = GetMenu(position);
                if (result.IsSuccess && !string.IsNullOrEmpty(chacheName))
                {
                    cacheHelper.Set(chacheName, result.Data, 72);
                }
            }

            return result;
        }

        /// <summary>
        /// 取得選單
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        private CiResult<List<TreeWebViewModel>> GetMenu(MenuPosition position)
        {
            var result = new CiResult<List<TreeWebViewModel>>();

            try
            {
                var data = service.GetTrees(null, new ItemTreeFilter
                {
                    MenuPosition = position,
                    ItemType = ItemType.Category,
                    LangType = ApplicationHelper.DefaultLanguage,
                    SelectEnabled = true,
                    EmptyContinue = (position == MenuPosition.Bottom)
                });

                result.Data = TransModel(data);
                result.IsSuccess = true;
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }

            return result;
        }

        private List<TreeWebViewModel> TransModel(List<TreeViewModel> model)
        {
            var result = new List<TreeWebViewModel>();
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);

            foreach (var node in model)
            {
                var newItem = new TreeWebViewModel
                {
                    Name = node.Name,
                    RouteName = node.Url,
                    Url = RouteHelper.BaseUrl() + url.Action("Get", "Item", new { routeName = node.Url }),
                    Nodes = new List<TreeWebViewModel>()
                };

                if (node.Nodes.Any())
                {
                    newItem.Nodes = TransModel(node.Nodes);
                }

                result.Add(newItem);
            }

            return result;
        }

        #endregion

        #region detail、list
        /// <summary>
        /// 取得內頁
        /// </summary>
        /// <param name="routeName">Name of the route.</param>       
        /// <returns></returns>
        public CiResult<ItemWebViewModel> GetDetail(string routeName)
        {
            string viewName = "";
            var data = Get(routeName, ref viewName, ItemType.Article);

            var result = new CiResult<ItemWebViewModel>
            {
                IsSuccess = data.IsSuccess,
                Message = data.Message
            };

            if (data != null)
            {
                result.Data = (ItemWebViewModel)data.Data;
            }

            return result;
        }

        /// <summary>
        /// 取得列表頁
        /// </summary>
        /// <param name="routeName">Name of the route.</param>      
        /// <returns></returns>
        public CiResult<ItemListModel> GetList(string routeName)
        {
            string viewName = "";
            var data = Get(routeName, ref viewName);// ItemType.Category

            var result = new CiResult<ItemListModel>
            {
                IsSuccess = data.IsSuccess,
                Message = data.Message
            };

            if (data != null)
            {
                result.Data = (ItemListModel)data.Data;
            }

            return result;
        }

        /// <summary>
        /// 取得(內頁、列表頁)
        /// </summary>
        /// <param name="routeName">Name of the route.</param>
        /// <param name="viewName">Name of the view.</param>
        /// <param name="itemType">Type of the item.</param>
        /// <returns></returns>
        public CiResult<object> Get(string routeName, ref string viewName, ItemType? itemType = null)
        {
            //(Update ViewCount)
            var data = service.GetByRoteName(routeName, ApplicationHelper.DefaultLanguage, addViewCount: true);

            return ShowItem(data, ref viewName, itemType);
        }

        private ItemWebViewModel TransModel(ItemViewModel model)
        {
            if (model == null) return null;

            var result = new ItemWebViewModel
            {
                Item = new ItemWebModel
                {
                    ID = model.Item.ID,
                    StructureID = model.Item.StructureID,
                    RouteName = model.Item.RouteName,
                    Date = model.Item.Date,
                    StartTime = model.Item.StartTime,
                    EndTime = model.Item.EndTime,
                    OriginalPrice = model.Item.OriginalPrice,
                    SalePrice = model.Item.SalePrice,
                    StockCount = model.Item.StockCount,
                    SaleCount = model.Item.SaleCount,
                    SaleStartTime = model.Item.SaleStartTime,
                    SaleEndTime = model.Item.SaleEndTime,
                    CreateTime = model.Item.CreateTime,
                    UpdateTime = model.Item.UpdateTime,
                    ViewCount = model.Item.ViewCount
                },
                ItemLanguage = new ItemLanguageWebModel
                {
                    Subject = model.ItemLanguage.Subject,
                    Description = model.ItemLanguage.Description,
                    Content = HttpUtility.HtmlDecode(model.ItemLanguage.Content),
                    TemplateText = HttpUtility.HtmlDecode(model.ItemLanguage.TemplateText),
                    CustomFormatText = model.ItemLanguage.CustomFormatText,
                    LinkUrl = RouteHelper.SetUrlPath(model.ItemLanguage.LinkUrl),
                    IsBlankUrl = model.ItemLanguage.IsBlankUrl,
                    Keywords = model.ItemLanguage.Keywords,
                    Author = model.ItemLanguage.Author,
                    Phone = model.ItemLanguage.Phone,
                    Address = model.ItemLanguage.Address,
                    AddressInfo = model.ItemLanguage.AddressInfo
                },
                ItemFiles = new List<ItemWebFile>(),
                UserProfile = TransModel(model.UserProfile),
                BreadCrumbs = model.BreadCrumbs,
                ParentItems = model.ParentItems,
            };

            if (model.ItemFiles != null)
            {
                foreach (var file in model.ItemFiles)
                {
                    result.ItemFiles.Add(new ItemWebFile
                    {
                        StructureID = file.StructureID,
                        Subject = file.Subject,
                        OriName = file.OriName,
                        FilePath = file.FileType == (int)FileType.YouTube ? file.FilePath : RouteHelper.SetUrlPath(file.FilePath, isAbsolute: true),
                        ThumbnailPath = RouteHelper.SetUrlPath(file.ThumbnailPath, isAbsolute: true),
                        FileType = (FileType)file.FileType,
                        SourceType = (SourceType)file.SourceType
                    });
                }
            }

            return result;
        }

        private UserProfileWebModel TransModel(mgt_UserProfile model)
        {
            if (model == null) return null;

            var result = new UserProfileWebModel
            {
                LastName = model.LastName,
                FirstName = model.FirstName,
                EngName = model.EngName,
                NickName = model.NickName,
                AvatarPath = model.AvatarPath,
                Description = model.Description,
                // IdentityCard 
                Birthday = model.Birthday,
                Gender = model.Gender,
                Marriage = model.Marriage,
                // HomePhone
                // CompanyPhone 
                SecondaryEmail = model.SecondaryEmail,
                // EmergencyContact
                // EmergencyPhone 
                Unit = model.Unit,
                UnitAddress = model.UnitAddress,
                Occupation = model.Occupation,
                Education = model.Education,
                School = model.School,
                Skill = model.Skill,
                Language = model.Language,
                SocialNetwork = model.SocialNetwork,
                // Sports
                Height = model.Height,
                Weight = model.Weight,
                Referrer = model.Referrer,
                CreateTime = model.CreateTime,
                UpdateTime = model.UpdateTime
            };


            return result;
        }

        /// <summary>
        /// Item private (from Get、Preview)
        /// </summary>      
        /// <param name="oriData"></param>
        /// <param name="viewName">回傳view</param>
        /// <param name="itemType">指定類型</param>
        /// <returns></returns>
        public CiResult<object> ShowItem(ItemViewModel oriData, ref string viewName, ItemType? itemType = null)
        {
            var result = new CiResult<object>();
            var data = TransModel(oriData);
            if (data == null) // || string.IsNullOrEmpty(structure.ViewName)
            {
                result.Message = "data not found";
                return result; //404
            }

            #region 取得資料      

            //結構
            var structure = structureService.Get(data.Item.StructureID);
            if (structure == null) // || string.IsNullOrEmpty(structure.ViewName)
            {
                result.Message = "structure not found";
                return result; //404
            }

            //子結構(子分類) 文章後推一層
            var subStructures = structureService.GetByParent(structure.ID.ToListObject(), ItemType.CategorySub);
            var subItemList = new PageModel<ItemViewModel>();
            if (subStructures.Any())
            {
                subItemList = service.GetListView(
                    new PageParameter
                    {
                        SortColumn = SortColumn.Sort,
                        IsPaged = false
                    }, new ItemFilter
                    {
                        LangType = ApplicationHelper.DefaultLanguage,
                        StructureIDs = subStructures.Select(x => x.ID).ToList(),
                        CategoryIDs = data.Item.ID.ToListObject()
                    });
            }

            /****若有子分類,文章往後推一層****/

            //子結構(多種文章) 
            List<Guid> articleStructureID = (subStructures.Any()) ? subStructures.Select(x => x.ID).ToList() : structure.ID.ToListObject();
            var articleStructures = structureService.GetByParent(articleStructureID, ItemType.Article);
            //分頁參數
            var pageModel = new ItemPageModel();
            if (articleStructures != null)
            {
                //所有層級Category都可 (父層分類可找到子層分類的文章)
                var CategoryIDs = data.Item.ID.ToListObject();
                if (subStructures.Any())
                {
                    CategoryIDs.AddRange(subItemList.Data.Select(x => x.Item.ID).ToList());
                }
                pageModel.CategoryJson = _Json.ModelToJson(CategoryIDs);
                pageModel.StructureJson = _Json.ModelToJson(articleStructures.Select(x => x.ID));
                //sort
                if (articleStructures.All(x => x.RequiredTypes.HasValue((int)ContentType.Date)))
                {
                    pageModel.SortColumn = SortColumn.Date;
                }
                else if (articleStructures.All(x => x.RequiredTypes.HasValue((int)ContentType.Sort)))
                {
                    pageModel.SortColumn = SortColumn.Sort;
                }
                else
                {
                    pageModel.SortColumn = SortColumn.CreateTime;
                }

                //人才庫
                if (articleStructures.Any(x => x.ItemTypes.HasValue((int)ItemType.Q8People)))
                {
                    pageModel.DataLevel = DataLevel.Normal;
                }
            }

            #endregion

            #region 回傳頁面
            //// ckeditor
            //data.ItemLanguage.Content = HttpUtility.HtmlDecode(data.ItemLanguage.Content);
            //data.ItemLanguage.TemplateText = HttpUtility.HtmlDecode(data.ItemLanguage.TemplateText);

            //Banner
            //if (structure.ItemTypes.HasValue((int)ItemType.Banner) || itemType == ItemType.Banner)
            //{
            //    if (articleStructure == null)
            //        return null; //404

            //    pageModel.ViewName = structure.ViewName;
            //    pageModel.IsPaged = false;

            //    return pageModel;

            //}

            // else

            //內容 Detail
            if (structure.ItemTypes.HasValue((int)ItemType.Article) || itemType == ItemType.Article)
            {
                viewName = structure.ViewName;
                result.Data = data;
                result.IsSuccess = true;
                return result;
            }
            //列表 List
            else if (structure.ItemTypes.HasValue((int)ItemType.Category) || itemType == ItemType.Category)
            {
                //子結構可以是文章或次分類
                if (!articleStructures.Any() && !subStructures.Any())
                    return result; //404

                pageModel.ViewName = structure.ViewName + "_Page";
                var model = new ItemListModel
                {
                    ItemViewModel = data,
                    ItemPageModel = pageModel,
                };
                if (subItemList.Data != null)
                {
                    model.SubItemList = new List<ItemWebViewModel>(
                      subItemList.Data?.Select(x => TransModel(x)).ToList()
                    );
                }

                viewName = structure.ViewName;
                result.Data = model;
                result.IsSuccess = true;
                return result;
            }

            result.Message = "type not found";
            return result; //404   

            #endregion
        }

        #endregion

        #region pagelist

        /// <summary>
        /// 取得列表、部分內容 by cache
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public CiResult<ItemPageResult> GetPartialListByCache(ItemPageModel model)
        {
            var result = new CiResult<ItemPageResult>();
            var cacheHelper = new CacheHelper();

            //get cache
            if (!string.IsNullOrEmpty(model.CacheName))
            {
                var chacheName = $"{ApplicationHelper.ClientID}_item_{model.CacheName}";
                result.Data = cacheHelper.Get<ItemPageResult>(model.CacheName);
            }

            //set cache
            if (result.Data?.DataResult == null)
            {
                result = GetPartialList(model);
                if (result.IsSuccess && !string.IsNullOrEmpty(model.CacheName))
                {
                    cacheHelper.Set(model.CacheName, result.Data);
                }
            }

            return result;
        }

        /// <summary>
        /// 取得列表、部分內容
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public CiResult<ItemPageResult> GetPartialList(ItemPageModel model)
        {
            var result = new CiResult<ItemPageResult>();

            try
            {
                //search
                if (!string.IsNullOrWhiteSpace(model.SearchString))
                {
                    model.SearchString = HttpUtility.UrlDecode(model.SearchString);
                    model.SearchString = _Check.RelpaceBadCharacters(model.SearchString);

                    //取structure: 包含Article, 排除Banner、Category
                    if (string.IsNullOrEmpty(model.StructureJson) && !string.IsNullOrEmpty(model.SearchString))
                    {
                        var articleStructure = structureService.GetList(ItemType.Article, new List<ItemType> { ItemType.Banner, ItemType.Category });
                        model.StructureJson = _Json.ModelToJson(articleStructure.Data.Select(x => x.ID));
                    }
                }

                //有routeName: 先取分頁參數
                if (!string.IsNullOrEmpty(model.RouteName))
                {
                    var listResult = GetList(model.RouteName);
                    if (listResult.IsSuccess)
                    {
                        model.StructureJson = listResult.Data.ItemPageModel.StructureJson;
                        model.CategoryJson = listResult.Data.ItemPageModel.CategoryJson;
                        model.SortColumn = listResult.Data.ItemPageModel.SortColumn;
                    }
                    else
                    {
                        result.Message = listResult.Message;
                        return result;
                    }
                }

                if (string.IsNullOrEmpty(model.StructureJson))
                {
                    result.Message = "no StructureJson";
                    return result;
                }

                var stuctureIDs = _Json.JsonToModel<List<Guid>>(model.StructureJson);
                var categoryIDs = _Json.JsonToModel<List<Guid>>(model.CategoryJson);

                var listData = service.GetListView(
                  new PageParameter
                  {
                      IsPaged = model.IsPaged,
                      CurrentPage = model.CurrentPage,
                      PageSize = model.PageSize,
                      SortColumn = model.SortColumn,
                      IsDescending = model.IsDescending,
                      DataLevel = model.DataLevel
                  },
                   new ItemFilter
                   {
                       StructureIDs = stuctureIDs,
                       CategoryIDs = categoryIDs,
                       ExceptID = model.ExceptID,
                       CoverType = model.CoverType,
                       SearchString = model.SearchString,
                       SearchType = model.SearchType,
                       LangType = ApplicationHelper.DefaultLanguage,
                       SelectEnabled = true,
                   });


                // Html Decode           
                foreach (var item in listData.Data)
                {
                    item.ItemLanguage.Content = HttpUtility.HtmlDecode(item.ItemLanguage.Content);
                    item.ItemLanguage.TemplateText = HttpUtility.HtmlDecode(item.ItemLanguage.TemplateText);
                }

                var resultModel = new ItemPageResult
                {
                    DataResult = new PageModel<ItemWebViewModel>
                    {
                        CurrentPage = listData.CurrentPage,
                        TotalCount = listData.TotalCount,
                        PageCount = listData.PageCount,
                        DataStart = listData.DataStart,
                        DataEnd = listData.DataEnd,
                        Data = listData.Data.Select(x => TransModel(x)).ToList(),
                    },
                    ItemPageModel = model
                };

                result.Data = resultModel;
                result.IsSuccess = true;
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }

            return result;
        }

        #endregion
    }
}