using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.Tools;
using MyTool.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using WebMaker.Entity.Models;
using WebMaker.Entity.ViewModels;
using System.Transactions;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Web.Mvc;
using System.Xml;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.IO;
using System.Data;
using System.Data.SqlClient;

namespace WebMaker.BLL.Services
{
    /// <summary>
    /// 訂單
    /// </summary>
    public class OrderService : BaseService
    {
        #region Get

        private IQueryable<erp_Order> Query
        {
            get
            {
                return Db.erp_Order
                   .Include(x => x.erp_OrderDetail)
                   .Include(x => x.erp_OrderLog)
                   .Include(x => x.erp_GetPayMessage)
                   .Include(x => x.mgt_UserRoleRelation)
                   .Include(x => x.erp_OrderUnit)
                   .Include(x => x.cms_Item)
                   .Where(x => x.ClientID == ClientID);
            }
        }

        public PageModel<OrderViewModel> GetList(PageParameter param, OrderFilter filter)
        {
            param.SortColumn = SortColumn.Custom;
            var dataLevel = DataLevel.Simple;
            var query = Query.Where(x => x.StructureID == filter.StructureID);

            //搜尋關鍵字
            if (!string.IsNullOrEmpty(filter.SearchString))
            {
                query = query.Where(x => x.OrderNumber.Contains(filter.SearchString)
                                    || x.mgt_User.Name.Contains(filter.SearchString)
                                    || x.VirtualAccount == filter.SearchString
                                    || x.erp_OrderDetail.Any(y => y.ItemSubject == filter.SearchString));
            }

            //訂單文章
            if (filter.ArticleID != null)
            {
                //param.IsPaged = false; //不分頁
                query = query.Where(x => x.ItemID == filter.ArticleID);
            }

            if (filter.GroupArticleID != null)
            {
                query = query.Where(x => x.cms_Item.ParentItemRelations.Any(y => y.ParentID == filter.GroupArticleID));
            }

            //部門權限
            if (filter.DepartmentIDs != null)
            {
                query = query.Where(x => x.cms_Item.DepartmentID != null && filter.DepartmentIDs.Contains(x.cms_Item.DepartmentID.Value));
            }

            //顯示個資
            if (filter.DisplayProfile)
            {
                dataLevel = DataLevel.Normal; // todo 個資解密
            }

            //建立者
            if (filter.CreateUser != null)
            {
                query = query.Where(x => x.CreateUser == filter.CreateUser);
            }

            //前端or後端
            if (!filter.IsAdmin)
            {
                query = query.Where(x => x.OrderStatus != (int)OrderStatus.OverSoldFail);
            }

            //篩選狀態
            if (filter.SelectOrderStatus != null && filter.SelectOrderStatus.Any())
            {
                query = query.Where(x => filter.SelectOrderStatus.Any(y => x.OrderStatus == (int)y));
            }
            //篩選狀態
            if (filter.SelectOrderDetailStatus != null && filter.SelectOrderDetailStatus.Any())
            {
                query = query.Where(x => x.erp_OrderDetail.Any(z => filter.SelectOrderDetailStatus.Any(y => z.OrderStatus == (int)y)));
            }


            //篩選自動入帳
            if (filter.SelectAutoPay)
            {
                query = query.Where(x => x.erp_GetPayMessage.Any());
            }

            //篩選日期
            if (filter.StartTime != null)
            {
                query = query.Where(x => x.CreateTime > filter.StartTime.Value);
            }
            if (filter.EndTime != null) //必須ToList (LINQ to Entities 無法辨識方法 'System.DateTime AddDays(Double)' 方法
            {
                var endTime = filter.EndTime.Value.AddDays(1);
                query = query.Where(x => x.CreateTime < endTime);
            }

            //sort
            //query = query.OrderByDescending(x => x.OrderNumber);
            query = query.OrderByDescending(x => x.CreateTime);

            var pagedModel = PageTool.CreatePage(query.ToList(), param);
            var model = new PageModel<OrderViewModel>
            {
                CurrentPage = pagedModel.CurrentPage,
                PageSize = pagedModel.PageSize,
                TotalCount = pagedModel.TotalCount,
                PageCount = pagedModel.PageCount,
                DataStart = pagedModel.DataStart,
                DataEnd = pagedModel.DataEnd,
                PriceSum = query.Any() ? query.Sum(x => x.TotalPrice) : 0,
                Data = pagedModel.Data.Select(x => ToViewModel(x, filter.LangType, dataLevel)).ToList()
            };

            return model;
        }

        public OrderViewModel GetView(Guid id, LanguageType? langType = null, DataLevel dataLevel = DataLevel.All)
        {
            var query = Query.FirstOrDefault(x => x.ID == id);
            return ToViewModel(query, langType, dataLevel);
        }

        public erp_Order Get(Guid id)
        {
            var query = Query.FirstOrDefault(x => x.ID == id);
            return query;
        }

        public erp_OrderDetail GetDetail(Guid id)
        {
            var query = Db.erp_OrderDetail
                   .Include(x => x.erp_Order)
                   .Include(x => x.TeamMembers)
                   .FirstOrDefault(x => x.erp_Order.ClientID == ClientID && x.ID == id);
            return query;
        }

        private OrderViewModel ToViewModel(erp_Order order, LanguageType? langType = null, DataLevel dataLevel = DataLevel.Simple, bool IsDecrypt = true)
        {
            /* 
             * 儲存前不可解密IsDecrypt, 會造成解密的資料直接被儲存
             */

            var details = order.erp_OrderDetail.ToList(); //Item文章 排序
            if (order.erp_OrderDetail.Any(x => x.cms_Item.cms_Structure.ContentTypes.HasValue((int)ContentType.Sort)))
            {
                details = details.OrderByDescending(x => x.cms_Item.Sort).ToList();
            }

            var model = new OrderViewModel
            {
                Order = order,
                OrderDetails = details.Select(x => new EditOrderDetail(x)
                {
                    DepartmentName = x.cms_Item?.mgt_Department?.Name,
                    DetailMembers = x.TeamMembers.ToList()
                }).ToList(),
                OrderDiscounts = order.erp_OrderDiscount.ToList()
            };

            //完整資料
            if (dataLevel >= DataLevel.Normal)
            {
                //解密
                if (IsDecrypt)
                {
                    model.Order.PayInfo = _Crypto.DecryptAES(model.Order.PayInfo, Setting.OrderCryptoKey);
                    model.Order.ReceiverPhone = _Crypto.DecryptAES(model.Order.ReceiverPhone, Setting.OrderCryptoKey);
                    model.Order.ReceiverAddres = _Crypto.DecryptAES(model.Order.ReceiverAddres, Setting.OrderCryptoKey);
                }

                //比賽團隊成員            
                model.TeamMembers = model.Order.TeamMembers.OrderBy(x => x.Sort).ToList();
                model.Units = model.Order.erp_OrderUnit.OrderBy(x => x.Sort).ToList();

                if (dataLevel == DataLevel.All)
                {
                    #region 單位下拉選單
                    var selecUnitItems = new List<SelectOptionModel>();
                    if (model.Units.Any())
                    {
                        foreach (var data in model.Units)
                        {
                            selecUnitItems.Add(new SelectOptionModel
                            {
                                Value = data.ID.ToString(),
                                Text = data.Unit
                            });
                        }
                    }

                    model.UnitsSelectJson = _Json.ModelToJson(selecUnitItems);
                    #endregion

                    #region 成員下拉選單
                    var selecItems = new List<SelectOptionModel>();
                    if (model.TeamMembers.Any())
                    {
                        foreach (var member in model.TeamMembers)
                        {
                            if (IsDecrypt)
                            {
                                member.IdentityCard = _Crypto.DecryptAES(member.IdentityCard, Setting.OrderCryptoKey);
                            }

                            selecItems.Add(new SelectOptionModel
                            {
                                Value = member.ID.ToString(),
                                Text = member.NickName
                            });
                        }
                    }

                    model.TeamMemberSelectJson = _Json.ModelToJson(selecItems);
                    #endregion

                    //申請身分
                    model.NewRoleRelation = order.mgt_UserRoleRelation.FirstOrDefault();

                    //可併入的訂單: 同建立者、處理中、stucture相同、排除自己               
                    model.CombinOrderSelectList = Query.Where(x => x.CreateUser == model.Order.CreateUser
                                                                && x.OrderStatus == (int)OrderStatus.Processing
                                                                && x.StructureID == model.Order.StructureID
                                                                && x.ID != model.Order.ID)
                        .ToList().Select(x => new SelectListItem
                        {
                            Text = x.OrderNumber,
                            Value = x.ID.ToString()
                        }).ToList();
                }
            }


            //訂單文章
            if (order.ItemID != null && langType != null)
            {
                if (dataLevel == DataLevel.All)//複雜:下拉選單
                {
                    ItemService itemService = new ItemService { ClientID = ClientID };
                    model.ItemViewModel = itemService.GetView(order.ItemID.Value, langType);

                    var orderItemStructure = order.cms_Structure.ChildStructures.FirstOrDefault(x => x.ItemTypes.HasValue((int)ItemType.OrderItem));
                    if (orderItemStructure != null)
                    {
                        var orderItems = itemService.GetSubOrderList(orderItemStructure.ID, order.ItemID.Value, langType.Value);
                        if (orderItems.Data != null)
                        {
                            #region 比賽項目、組別 下拉選單 (兩層)

                            var selecItems = new List<SelectOptionModel>();
                            if (orderItems.Data.Any())
                            {
                            }
                            foreach (var itemModel in orderItems.Data)
                            {
                                var attributes = new List<KeyValuePair<string, string>>();
                                attributes.Add(new KeyValuePair<string, string>("PeopleMin", itemModel.Item.PeopleMin.ToString()));
                                attributes.Add(new KeyValuePair<string, string>("PeopleMax", itemModel.Item.PeopleMax.ToString()));
                                attributes.Add(new KeyValuePair<string, string>("SalePrice", itemModel.Item.SalePrice.ToString("0")));

                                selecItems.Add(new SelectOptionModel
                                {
                                    Value = itemModel.Item.ID.ToString(),
                                    Text = itemModel.ItemLanguage.Subject,
                                    SubSelect = itemModel.Item.Options?.Split(',').Select(x =>
                                    new SelectOptionModel
                                    {
                                        Value = x,
                                        Text = x
                                    }).ToList(),
                                    Attributes = attributes
                                });
                            }//end foreach

                            model.SubItemSelectJson = _Json.ModelToJson(selecItems);

                            #endregion
                        }
                    }
                    itemService.Dispose();
                }
                else
                {
                    model.ItemViewModel = new ItemViewModel
                    {
                        Item = model.Order.cms_Item,
                        ItemLanguage = model.Order.cms_Item?.cms_ItemLanguage.FirstOrDefault(x => x.LanguageType == (int)langType)
                    };

                    model.ParentItemViewModel = new ItemViewModel
                    {
                        Item = model.Order.cms_Item.ParentItemRelations.FirstOrDefault(x => x.IsCrumb)?.ParentItem
                    };
                    if (model.ParentItemViewModel.Item != null)
                    {
                        model.ParentItemViewModel.ItemLanguage = model.ParentItemViewModel.Item?.cms_ItemLanguage.FirstOrDefault(x => x.LanguageType == (int)langType);
                    }
                }
            }

            return model;
        }

        /// <summary>
        /// 分組名單
        /// </summary>
        /// <param name="param"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public PageModel<erp_OrderDetail> GetTeamList(PageParameter param, OrderFilter filter)
        {
            var dataLevel = DataLevel.Simple;
            var query = Query.Where(x => x.ItemID == filter.ArticleID).ToList();

            //篩選狀態
            //if (filter.SelectOrderStatus != null && filter.SelectOrderStatus.Any())
            //{
            //    query = query.Where(x => filter.SelectOrderStatus.Any(y => x.OrderStatus == (int)y)).ToList();
            //}

            //detail展開
            var data = query.SelectMany(x => x.erp_OrderDetail).ToList();

            //狀態篩選detail
            if (filter.SelectOrderStatus != null && filter.SelectOrderStatus.Any())
            {
                data = data.Where(x => filter.SelectOrderStatus.Any(y => x.OrderStatus == (int)y)).ToList();
            }

            //排序
            param.SortColumn = SortColumn.Custom;
            if (filter.OrderTeamSort == OrderTeamSort.Member)
            {
                data = data.OrderBy(x => x.TeamMembers.FirstOrDefault()?.NickName).ToList();
            }
            else if (filter.OrderTeamSort == OrderTeamSort.CreateTime)
            {
                data = data.OrderBy(x => x.erp_Order.CreateTime).ToList();
            }
            else
            {
                data = data.OrderByDescending(x => x.cms_Item.Sort).ThenBy(x => x.Option).ToList();
            }

            var pagedModel = PageTool.CreatePage(data, param);
            return pagedModel;
        }


        /// <summary>
        /// 狀態統計
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public List<StatusModel> GetStatusList(OrderFilter filter)
        {
            var result = new List<StatusModel>();

            var query = Query.Where(x => x.ItemID == filter.ArticleID)
                //detail展開
                .SelectMany(x => x.erp_OrderDetail).ToList();

            //包含狀態
            var orderStatus = Db.cms_Structure.Find(filter.StructureID).OrderStatuses.ToContainList<OrderStatus>();

            //統計
            foreach (var status in orderStatus)
            {
                result.Add(new StatusModel
                {
                    OrderStatus = status,
                    TotalCount = query.Count(x => x.OrderStatus == (int)status)
                });
            }

            return result;
        }


        /// <summary>
        /// 對帳功能
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public PageModel<erp_GetPayMessage> GetPayMessage(PageParameter param, OrderFilter filter)
        {
            var dataLevel = DataLevel.Simple;
            var query = Db.erp_GetPayMessage.Include(x => x.erp_Order)
                          .Where(x => x.ClientID == ClientID).ToList();

            //為排程查詢後入帳
            if (filter.IsByQuery)
            {
                query = query.Where(x => x.IsByQuery).ToList();
            }

            //篩選異常
            if (filter.IsWrong)
            {
                query = query.Where(x => x.IsEnabled == false).ToList();
            }

            //篩選日期
            if (filter.StartTime != null)
            {
                query = query.Where(x => x.PayTime != null && x.PayTime > filter.StartTime.Value).ToList();
            }
            if (filter.EndTime != null)
            {
                query = query.Where(x => x.PayTime != null && x.PayTime < filter.EndTime.Value.AddDays(1)).ToList();
            }

            //排序
            param.SortColumn = SortColumn.Custom;
            query = query.OrderByDescending(x => x.CreateTime).ToList();

            var pagedModel = PageTool.CreatePage(query, param);
            pagedModel.PriceSum = query.Sum(x => x.PayPrice != null ? x.PayPrice.Value : 0);

            return pagedModel;
        }

        #endregion

        #region Edit

        /// <summary>
        /// 會員建立訂單
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="langType">Type of the language.</param>
        /// <returns></returns>
        public CiResult<OrderStatus> Create(OrderViewModel model, LanguageType langType)
        {
            CiResult<OrderStatus> result = new CiResult<OrderStatus>();

            //using (TransactionScope trans = new TransactionScope())
            //{
            try
            {
                var structure = Db.cms_Structure.Find(model.Order.StructureID);
                //包含類型
                var orderContentTypes = structure.OrderContentTypes.ToContainList<OrderContentType>();

                //--更新User個資: 依據NewRoleRelation, 新增Order後再add Role
                var addRoles = new List<mgt_Role>();
                var addRoleRelations = new List<mgt_UserRoleRelation>();

                //活動報名、申請資格 (滑輪用)
                #region 更新User身分
                //if (model.Order.ItemID != null)
                //{
                //    var item = Db.cms_Item.Find(model.Order.ItemID.Value);

                //    //不自動加入Role,但更新個資(管理員手動加入身分)                        
                //    //var addTargetRole = !item.Item.cms_Structure.ContentTypes.HasValue((int)ContentType.OrderAutoRole_Not);

                //    //手動 (下拉選單)
                //    if (model.NewRoleRelation != null && model.NewRoleRelation.RoleID != Guid.Empty)
                //    {
                //        addRoles = Db.mgt_Role.Find(model.NewRoleRelation.RoleID)?.ToListObject();
                //        addRoleRelations = model.NewRoleRelation.ToListObject();
                //    }
                //    //自動
                //    else
                //    {
                //        addRoles = item.cms_ItemOrderRoleRelation.Where(x => x.ItemOrderRoleType == (int)ContentType.OrderCreateRole).Select(x => x.mgt_Role).ToList();
                //        if (addRoles.Any())
                //        {
                //            addRoleRelations = addRoles.Select(x => new mgt_UserRoleRelation
                //            {
                //                RoleID = x.ID
                //            }).ToList();
                //        }
                //    }

                //    if (addRoles.Any())
                //    {
                //        UserService userService = new UserService { ClientID = ClientID };
                //        var updateUser = userService.UpdateMember(model, false, targetRoles: addRoles);
                //        if (!updateUser.IsSuccess)
                //        {
                //            result.Message = updateUser.Message;
                //        }
                //        userService.Dispose();
                //    }
                //}
                #endregion

                //get OrderNumber
                //string orderNumber = "";
                //if (string.IsNullOrEmpty(result.Message))
                //{
                //    var sysResult = CreateOrderNumber();
                //    if (!sysResult.IsSuccess)
                //        result.Message = sysResult.Message;

                //    orderNumber = sysResult.Data;
                //}

                //get Status
                var firstStatus = structure.OrderStatuses.ToContainList<OrderStatus>().Min();

                //create (加密:付款資訊、地址、電話)
                if (string.IsNullOrEmpty(result.Message))
                {
                    //---Order---
                    #region data
                    var data = new erp_Order
                    {
                        ID = Guid.NewGuid(),
                        ClientID = ClientID,
                        StructureID = model.Order.StructureID,
                        ItemID = model.Order.ItemID,
                        //OrderNumber = GetNewOrderNumber(),

                        PayType = model.Order.PayType,
                        PayInfo = _Crypto.EncryptAES(model.Order.PayInfo, Setting.OrderCryptoKey),
                        DeliveryType = model.Order.DeliveryType,
                        ReceiverName = model.Order.ReceiverName,
                        ReceiverPhone = _Crypto.EncryptAES(model.Order.ReceiverPhone, Setting.OrderCryptoKey),
                        ReceiverAddres = _Crypto.EncryptAES(model.Order.ReceiverAddres, Setting.OrderCryptoKey),
                        ReceiverEmail = model.Order.ReceiverEmail,

                        TeamName = model.Order.TeamName,
                        Coach = model.Order.Coach,
                        Leader = model.Order.Leader,
                        Manager = model.Order.Manager,

                        OrderNote = model.Order.OrderNote,
                        FilePath = model.Order.FilePath,
                        //RoleNumber = model.Order.RoleNumber,
                        //RoleID = model.Order.RoleID,
                        PublicNote = model.Order.PublicNote,
                        PrivateNote = model.Order.PrivateNote,
                        ShippingFee = model.Order.ShippingFee,

                        OrderStatus = (int)firstStatus,
                        CreateTime = DateTime.Now,
                        CreateUser = model.User.ID,
                    };
                    #endregion

                    //編輯期限24小時
                    if (orderContentTypes.Contains(OrderContentType.EditDeadline))
                    {
                        data.EditDeadline = data.CreateTime.AddDays(1);
                    }

                    //---OrderDetail---                       
                    if (model.OrderDetails != null)
                    {
                        UpdateOrderDetails(data, langType, model.OrderDetails);
                    }

                    //---add Role (滑輪用)
                    //if (addRoleRelations != null)
                    //{
                    //    foreach (var roleRelate in addRoleRelations)
                    //    {
                    //        var role = new mgt_UserRoleRelation
                    //        {
                    //            ID = Guid.NewGuid(),
                    //            UserID = model.User.ID,
                    //            RoleID = roleRelate.RoleID,
                    //            RoleNumber = roleRelate.RoleNumber,
                    //            CreateTime = DateTime.Now,
                    //            IsEnabled = firstStatus == OrderStatus.Done//完成自動啟用身分
                    //        };

                    //        data.mgt_UserRoleRelation.Add(role);
                    //    }
                    //}

                    //check new create
                    if (firstStatus != OrderStatus.Editing)
                    {
                        var checkResult = NewOrderCheck(data);
                        if (!checkResult.IsSuccess)
                        {
                            result.Message = checkResult.Message;
                        }
                    }

                    //save
                    if (string.IsNullOrEmpty(result.Message))
                    {
                        Db.erp_Order.Add(data);

                        //更新訂單價格總和
                        UpdateOrderPrice(data);

                        //***總價0,付款方式改為無***
                        if (data.TotalPrice == 0 && firstStatus != OrderStatus.Editing)
                        {
                            data.PayType = (int)PayType.None;
                        }

                        //OrderLog
                        AddLog(data.ID, firstStatus, model.User.ID);

                        //model.OrderDetails (已展開每個組別數量皆1
                        //1.銷售量 pre_check 失敗不儲存                
                        var itemList = model.OrderDetails.Select(x => new KeyValueModel { Key = x.ItemID, Value = x.Quantity }).ToList();
                        var saleResult = CheckSaleCount(itemList, model.Order.ItemID.Value, "pre");
                        //fail
                        if (!saleResult.IsSuccess)
                        {
                            result.Message = saleResult.Message;
                        }
                        //success
                        else
                        {
                            //訂單編號proc
                            data.OrderNumber = GetNewOrderNumber();

                            Db.SaveChanges();
                        }

                        //2.銷售量 after_check 失敗回寫超賣
                        if (saleResult.IsSuccess)
                        {
                            var saleResult2 = CheckSaleCount(itemList, model.Order.ItemID.Value, "after", true);
                            //fail
                            if (!saleResult2.IsSuccess)
                            {
                                result.Message = saleResult2.Message;
                                data.OrderStatus = (int)OrderStatus.OverSoldFail;
                                firstStatus = OrderStatus.OverSoldFail;
                                if (data.erp_OrderDetail != null)
                                {
                                    foreach (var item in data.erp_OrderDetail)
                                    {
                                        item.OrderStatus = (int)OrderStatus.OverSoldFail;
                                    }
                                }
                                Db.SaveChanges();
                            }
                        }

                        // 更新銷售量
                        //if (model.OrderDetails != null)
                        //{
                        //    var countResult = UpdateSaleCount(model.OrderDetails.Select(x => x.ItemID).ToList());
                        //    if (!countResult)
                        //    {
                        //        result.Message = SystemMessage.StockNotEnough;
                        //    }
                        //}
                    }

                    if (string.IsNullOrEmpty(result.Message))
                    {
                        //trans.Complete();
                        result.Data = firstStatus;
                        result.ID = data.ID;
                        result.Message = SystemMessage.CreateSuccess;
                        result.IsSuccess = true;
                    }

                }//end create
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.CreateFail;
                _Log.CreateText("Order_Create:" + _Json.ModelToJson(ex));
            }//end try
            //}//end using trans

            return result;
        }

        //todo check 必填        
        /// <summary>
        /// 會員編輯訂單(必賽報名)
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="langType">Type of the language.</param>
        /// <returns></returns>
        public CiResult<OrderStatus> Update(EditOrderViewModel model, LanguageType langType)
        {
            var result = new CiResult<OrderStatus>();
            var dataView = GetView(model.OrderViewModel.Order.ID, langType, DataLevel.Simple);
            var data = dataView.Order;

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    //只有建立者自己能編輯 or IsAdmin
                    if (model.OrderViewModel.User.AccountType == (int)AccountType.Member &&
                        data.CreateUser != model.OrderViewModel.User.ID)
                    {
                        result.Message = SystemMessage.Error;
                    }

                    if (string.IsNullOrEmpty(result.Message))
                    {
                        if (model.Block == OrderEditBlock.BasicInfo)
                        {
                            #region 基本資料 (備註)
                            data.PayType = model.OrderViewModel.Order.PayType;
                            data.PayInfo = _Crypto.EncryptAES(model.OrderViewModel.Order.PayInfo, Setting.OrderCryptoKey);
                            //DeliveryType = model.Order.DeliveryType;
                            //ReceiverName = model.Order.ReceiverName;
                            //ReceiverPhone = _Crypto.EncryptAES(model.Order.ReceiverPhone, Setting.OrderCryptoKey);
                            //ReceiverAddres = _Crypto.EncryptAES(model.Order.ReceiverAddres, Setting.OrderCryptoKey);
                            //ReceiverEmail = model.Order.ReceiverEmail;

                            //--教練、領隊、管理改放單位內--//
                            data.TeamName = model.OrderViewModel.Order.TeamName;
                            data.Coach = model.OrderViewModel.Order.Coach;
                            data.Leader = model.OrderViewModel.Order.Leader;
                            data.Manager = model.OrderViewModel.Order.Manager;

                            data.OrderNote = model.OrderViewModel.Order.OrderNote;
                            //檔案
                            if (!string.IsNullOrEmpty(model.OrderViewModel.Order.FilePath))
                            {
                                data.FilePath = model.OrderViewModel.Order.FilePath;
                            }

                            //証號
                            var roleRelate = data.mgt_UserRoleRelation.FirstOrDefault();
                            if (roleRelate != null)
                            {
                                roleRelate.RoleNumber = model.OrderViewModel.NewRoleRelation.RoleNumber;
                            }

                            #endregion
                        }
                        else if (model.Block == OrderEditBlock.UnitList)
                        {
                            #region 單位 (縣市、單位、簡寫、領隊、教練、管理)

                            //不存在更新的,就是刪除
                            var ids = model.OrderViewModel.Units.Select(y => y.ID);
                            var delUnit = data.erp_OrderUnit.Where(x => !ids.Contains(x.ID)).ToList();

                            if (delUnit.Any())
                            {
                                //單位有選手 不可刪除
                                var forceDelete = delUnit.Where(x => x.mgt_UserProfile.Any());
                                if (forceDelete.Any())
                                {
                                    result.Message = string.Format(SystemMessage.OrderDeleteError, string.Join(",", forceDelete.Select(x => x.Unit)), "選手");
                                }
                                else
                                {
                                    Db.erp_OrderUnit.RemoveRange(delUnit);
                                }
                            }

                            //save
                            if (string.IsNullOrEmpty(result.Message)
                                && model.OrderViewModel.Units != null)
                            {
                                int sort = 1;

                                foreach (var unit in model.OrderViewModel.Units)
                                {
                                    if (unit.ID == Guid.Empty)
                                    {
                                        //create
                                        unit.ID = Guid.NewGuid();
                                        unit.Unit = unit.Unit.ToTrim();
                                        unit.UnitShort = unit.UnitShort.ToTrim();
                                        unit.County = unit.County.ToTrim();
                                        unit.Coach = unit.Coach.ToTrim();
                                        unit.Leader = unit.Leader.ToTrim();
                                        unit.Manager = unit.Manager.ToTrim();

                                        unit.CreateTime = DateTime.Now;
                                        unit.UpdateTime = DateTime.Now;
                                        unit.CreateUser = data.CreateUser;
                                        unit.Sort = sort;
                                        data.erp_OrderUnit.Add(unit);
                                    }
                                    else
                                    {
                                        //update
                                        var unitData = data.erp_OrderUnit.FirstOrDefault(x => x.ID == unit.ID);
                                        unitData.Unit = unit.Unit.ToTrim();
                                        unitData.UnitShort = unit.UnitShort.ToTrim();
                                        unitData.County = unit.County.ToTrim();
                                        unitData.Coach = unit.Coach.ToTrim();
                                        unitData.Leader = unit.Leader.ToTrim();
                                        unitData.Manager = unit.Manager.ToTrim();
                                        unitData.UpdateTime = DateTime.Now;
                                        unitData.Sort = sort;
                                    }
                                    sort++;
                                }
                            }
                            #endregion
                        }
                        else if (model.Block == OrderEditBlock.DetailUnit)
                        {
                            #region 明細的單位

                            //只有一個單位                            
                            var unit = model.OrderViewModel.Units.FirstOrDefault();
                            var orderDetail = data.erp_OrderDetail.FirstOrDefault(x => x.ID == unit.OrderDetailID);
                            if (unit == null || orderDetail == null)//必須存在OrderDetailID
                            {
                                result.Message = string.Format(SystemMessage.UpdateFail);
                            }

                            //save                          
                            if (string.IsNullOrEmpty(result.Message))
                            {
                                if (unit.ID == Guid.Empty)
                                {
                                    //create
                                    unit.ID = Guid.NewGuid();
                                    unit.OrderID = data.ID;
                                    unit.Unit = unit.Unit.ToTrim();
                                    unit.UnitShort = unit.UnitShort.ToTrim();
                                    unit.County = unit.County.ToTrim();
                                    unit.Coach = unit.Coach.ToTrim();
                                    unit.Leader = unit.Leader.ToTrim();
                                    unit.Manager = unit.Manager.ToTrim();

                                    unit.CreateTime = DateTime.Now;
                                    unit.UpdateTime = DateTime.Now;
                                    unit.CreateUser = data.CreateUser;
                                    //data.erp_OrderUnit.Add(unit);
                                    orderDetail.erp_OrderUnit = unit.ToListObject();
                                }
                                else
                                {
                                    //update
                                    var unitData = data.erp_OrderUnit.FirstOrDefault(x => x.ID == unit.ID);
                                    unitData.Unit = unit.Unit.ToTrim();
                                    unitData.UnitShort = unit.UnitShort.ToTrim();
                                    unitData.County = unit.County.ToTrim();
                                    unitData.Coach = unit.Coach.ToTrim();
                                    unitData.Leader = unit.Leader.ToTrim();
                                    unitData.Manager = unit.Manager.ToTrim();
                                    unitData.UpdateTime = DateTime.Now;
                                }
                            }

                            #endregion

                            //變更明細狀態
                            UpdateEditDetailStatus(orderDetail);
                        }
                        else if (model.Block == OrderEditBlock.TeamMember)
                        {
                            #region 團隊成員 (姓名、生日、性別、身分證、選單位)
                            var userService = new UserService { ClientID = ClientID };

                            //不存在更新的,就是刪除
                            var delMember = data.TeamMembers.Where(x => !model.OrderViewModel.TeamMembers.Select(y => y.ID).Contains(x.ID)).ToList();
                            if (delMember.Any())
                            {
                                //選手有項目 不可刪除
                                var forceDelete = delMember.Where(x => x.erp_OrderDetail.Any());
                                if (forceDelete.Any())
                                {
                                    result.Message = string.Format(SystemMessage.OrderDeleteError, string.Join(",", forceDelete.Select(x => x.NickName)), "參賽項目");
                                }
                                else
                                {
                                    Db.mgt_UserProfile.RemoveRange(delMember);
                                }
                            }

                            //save
                            if (string.IsNullOrEmpty(result.Message)
                                && model.OrderViewModel.TeamMembers != null)
                            {
                                int sort = 1;

                                foreach (var member in model.OrderViewModel.TeamMembers)
                                {
                                    //get
                                    //var userProfile = data.TeamMembers.FirstOrDefault(x => x.ID == member.ID);
                                    member.CreateUser = data.CreateUser;
                                    member.OrderID = data.ID;
                                    member.Sort = sort;

                                    //save
                                    var profileResut = userService.UpdateUserProfile(member.ID, member, Setting.OrderCryptoKey);
                                    if (!profileResut.IsSuccess)
                                    {
                                        result.Message = profileResut.Message;
                                        break;
                                    }
                                    sort++;
                                }
                            }
                            userService.Dispose();
                            #endregion
                        }
                        else if (model.Block == OrderEditBlock.DetailMember)
                        {
                            #region 明細的成員
                            //只有一個單明細                           
                            var modelDetail = model.OrderViewModel.OrderDetails.FirstOrDefault();
                            var temp = data.erp_OrderDetail.FirstOrDefault(x => x.ID == modelDetail.ID);
                            if (temp == null || temp == null)
                            {
                                result.Message = string.Format(SystemMessage.UpdateFail);
                            }

                            var orderDetail = Db.erp_OrderDetail.Include(x => x.TeamMembers)
                                                      .FirstOrDefault(x => x.ID == modelDetail.ID);

                            if (string.IsNullOrEmpty(result.Message))
                            {
                                //不存在更新的,就是刪除 (DetailMemberID是source)
                                var delMember = orderDetail.TeamMembers;
                                if (modelDetail.DetailMemberID != null)
                                {
                                    delMember = delMember.Where(x => !modelDetail.DetailMemberID.Contains(x.FromSourceID.Value)).ToList();
                                }
                                if (delMember.Any())
                                {
                                    Db.mgt_UserProfile.RemoveRange(delMember);
                                }

                                if (modelDetail.DetailMemberID != null)
                                {
                                    UserService userService = new UserService { ClientID = ClientID };

                                    //來源member
                                    var sourceMembers = userService.GetUserAssignToList(data.CreateUser,// model.OrderViewModel.User.ID,
                                        noneItemID: orderDetail.ItemID,
                                        butOrderDetailID: modelDetail.ID,
                                        noneParentItemID: dataView.ParentItemViewModel.Item.ID);

                                    //選中的member
                                    sourceMembers = sourceMembers.Where(x => modelDetail.DetailMemberID.Contains(x.ID)).ToList();

                                    //排除已存在
                                    if (orderDetail.TeamMembers.Any())
                                    {
                                        sourceMembers = sourceMembers.Where(x => !orderDetail.TeamMembers.Any(y => y.FromSourceID == x.ID)).ToList();
                                    }

                                    var resultIDs = new List<Guid>();

                                    //複製
                                    foreach (var member in sourceMembers)
                                    {
                                        var newMember = new mgt_UserProfile
                                        {
                                            CreateUser = model.OrderViewModel.User.ID,
                                            NickName = member.NickName,
                                            Birthday = member.Birthday,
                                            OrderID = data.ID,
                                            FromSourceID = member.ID
                                        };

                                        //save
                                        var profileResut = userService.UpdateUserProfile(null, newMember, Setting.OrderCryptoKey);
                                        if (!profileResut.IsSuccess)
                                        {
                                            result.Message = profileResut.Message;
                                            break;
                                        }

                                        resultIDs.Add(profileResut.ID);
                                        orderDetail.TeamMembers.Add(Db.mgt_UserProfile.Find(profileResut.ID));
                                    }

                                    // orderDetail.TeamMembers = Db.mgt_UserProfile.Where(x => resultIDs.Contains(x.ID)).ToList();

                                }
                            }
                            #endregion

                            //變更明細狀態
                            UpdateEditDetailStatus(orderDetail);
                        }
                        else if (model.Block == OrderEditBlock.OrderItem)
                        {
                            #region 比賽項目與組別 (選項目、選成員)

                            //優惠清空
                            if (data.erp_OrderDiscount != null)
                            {
                                Db.erp_OrderDiscount.RemoveRange(data.erp_OrderDiscount);
                            }

                            //不存在更新的,就是刪除
                            var delOrderItem = data.erp_OrderDetail.ToList();
                            if (model.OrderViewModel.OrderDetails != null)
                            {
                                delOrderItem = delOrderItem.Where(x => !model.OrderViewModel.OrderDetails.Select(y => y.ID).Contains(x.ID)).ToList();
                            }

                            if (delOrderItem != null)
                            {
                                foreach (var detail in delOrderItem)
                                {
                                    detail.TeamMembers.Clear();
                                }
                                Db.erp_OrderDetail.RemoveRange(delOrderItem);
                            }

                            //save
                            if (model.OrderViewModel.OrderDetails != null)
                            {
                                UpdateOrderDetails(data, langType, model.OrderViewModel.OrderDetails);
                            }

                            #endregion

                            //更新訂單價格總和
                            UpdateOrderPrice(data);
                        }
                    }

                    if (string.IsNullOrEmpty(result.Message))
                    {
                        //admin
                        if (model.IsAdmin)
                        {
                            AddLog(data.ID, OrderStatus.AdminEdit, model.OrderViewModel.User.ID);
                        }
                        //next status (no use?)    
                        //else if (data.OrderStatus != (int)OrderStatus.Editing)
                        //{
                        //    var firstStatus = data.cms_Structure.OrderStatuses.ToContainList<OrderStatus>().Min();
                        //    data.OrderStatus = (int)firstStatus;

                        //    //OrderLog
                        //    AddLog(data.ID, (OrderStatus)data.OrderStatus, model.OrderViewModel.User.ID);
                        //}
                        Db.SaveChanges();
                        trans.Complete();

                        result.Message = SystemMessage.UpdateSuccess;
                        result.IsSuccess = true;
                        result.Data = (OrderStatus)data.OrderStatus;
                        result.ID = data.ID;
                    }
                }
                catch (Exception ex)
                {
                    result.Message = SystemMessage.UpdateFail;
                    _Log.CreateText("Order_Update:" + _Json.ModelToJson(ex));
                }
            }//end using trans

            return result;
        }

        /// <summary>
        /// 變更編輯狀態(手動)
        /// </summary>
        /// <returns></returns>
        public CiResult UpdateEditDetailStatus(erp_OrderDetail orderDetail)
        {
            var result = new CiResult();
            var itemService = new ItemService { ClientID = ClientID };
            var item = itemService.Get(orderDetail.ItemID);
            var order = orderDetail.erp_Order;

            //截止
            //bool IsTimeUp = order.cms_Item.SaleEndTime.Value.AddDays(1) < DateTime.Now;

            //有單位、成員人數在範圍內 -> 編輯待確認(完成)
            int memberCount = orderDetail.TeamMembers.Count();
            if (orderDetail.erp_OrderUnit.Any() && item.PeopleMin <= memberCount && item.PeopleMax >= memberCount)
            {
                orderDetail.OrderStatus = (int)OrderStatus.TeamEditConfirm;
            }
            //未完成
            else
            {
                orderDetail.OrderStatus = (int)OrderStatus.TeamEdit;
            }

            Db.SaveChanges();
            return result;
        }

        /// <summary>
        /// 修改狀態
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public CiResult ChangeStatus(OrderViewModel model, AccountType accountType, bool updateNote = false)
        {
            UserService userService = new UserService { ClientID = ClientID };
            CiResult result = new CiResult();
            var data = Get(model.Order.ID);

            //包含流程
            var orderStatus = data.cms_Structure.OrderStatuses.ToContainList<OrderStatus>();
            //允許流程
            var allowStep = OrderTool.AllowSteps((OrderStatus)data.OrderStatus, accountType, orderStatus);

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    var oriOrderStatus = (OrderStatus)data.OrderStatus;
                    string virtualAccount = null;
                    #region 國泰虛擬帳號

                    if (string.IsNullOrEmpty(result.Message)
                        && model.Order.OrderStatus == (int)OrderStatus.NonPayment
                        && data.PayType == (int)PayType.ATMVirtual)
                    {
                        //繳費期限 7天 晚上11:59:59
                        var payDeadline = DateTime.Today.AddDays(7).AddSeconds(-1);
                        //取得虛擬帳號
                        var virtualResult = CreateVirtualAccount(payDeadline, (int)data.TotalPrice);
                        if (virtualResult.IsSuccess)
                        {
                            virtualAccount = virtualResult.Data;
                            data.VirtualAccount = virtualResult.Data;
                            data.VirtualCreateTime = DateTime.Now;
                            data.PayDeadline = payDeadline;
                        }
                        else
                        {
                            result.Message = virtualResult.Message;
                        }
                    }
                    #endregion

                    #region Status change

                    if (string.IsNullOrEmpty(result.Message)
                        && model.Order.OrderStatus != 0 && model.Order.OrderStatus != data.OrderStatus)
                    {
                        if (allowStep.Contains((OrderStatus)model.Order.OrderStatus))
                        {
                            //Order、OrderDetail status相同
                            data.OrderStatus = model.Order.OrderStatus;
                            if (data.erp_OrderDetail != null || data.erp_OrderDetail.Any())
                            {
                                foreach (var detail in data.erp_OrderDetail)
                                {
                                    detail.OrderStatus = model.Order.OrderStatus;
                                }
                            }

                            //OrderLog (編輯中刪除不紀錄)
                            if (model.Order.OrderStatus != (int)OrderStatus.Delete)
                            {
                                AddLog(data.ID, (OrderStatus)model.Order.OrderStatus, model.User.ID, virtualAccount);
                            }
                        }
                        else
                        {
                            result.Message = SystemMessage.UpdateFail;
                        }
                    }
                    #endregion


                    #region 申請身分
                    //(一般訂單只對應一個身分)
                    if (string.IsNullOrEmpty(result.Message)
                        && data.mgt_UserRoleRelation != null)
                    {
                        foreach (var role in data.mgt_UserRoleRelation)
                        {
                            if (model.NewRoleRelation != null && model.NewRoleRelation.RoleID != Guid.Empty)
                            {
                                //update                               
                                role.RoleNumber = model.NewRoleRelation.RoleNumber;
                                role.IsTimeLimited = model.NewRoleRelation.IsTimeLimited;
                                role.StartTime = model.NewRoleRelation.StartTime;
                                role.EndTime = model.NewRoleRelation.EndTime;
                                role.CreateTime = DateTime.Now;
                            }

                            //done:狀態完成自動新增身分 (建立訂單時已加入RoleRelation,直接啟用)
                            if (model.Order.OrderStatus == (int)OrderStatus.Done)
                            {
                                role.IsEnabled = true;
                                role.CreateTime = DateTime.Now;
                                //UserLog
                                userService.AddLog(role.UserID, UserLogType.CreateRole, "mgt_UserRoleRelation", role, role.UserID);


                                //---合併的訂單也新增身分---
                                if (data.FromCombineOrder != null)
                                {
                                    foreach (var fromOrder in data.FromCombineOrder)
                                    {
                                        foreach (var fRole in fromOrder.mgt_UserRoleRelation)
                                        {
                                            fRole.IsEnabled = true;
                                            //UserLog
                                            userService.AddLog(fRole.UserID, UserLogType.CreateRole, "mgt_UserRoleRelation", fRole, fRole.UserID);
                                        }
                                    }
                                }//end: fromCombine
                            }
                        }
                    }

                    //申請身分 create (舊結構可能有沒選身分下拉選單的人)
                    if (string.IsNullOrEmpty(result.Message)
                        && model.NewRoleRelation != null
                        && model.NewRoleRelation.RoleID != Guid.Empty && model.NewRoleRelation.ID == Guid.Empty)
                    {
                        model.NewRoleRelation.UserID = model.Order.CreateUser;
                        model.NewRoleRelation.OrderID = model.Order.ID;
                        var isEnabled = model.Order.OrderStatus == (int)OrderStatus.Done;

                        var roleResult = userService.CreateUserMemberRoles(model.NewRoleRelation, model.User.ID, isEnabled);
                        if (!roleResult.IsSuccess)
                        {
                            result.Message = roleResult.Message;
                        }
                    }
                    #endregion

                    #region 合併 Combine
                    if (string.IsNullOrEmpty(result.Message)
                      && model.Order.OrderStatus == (int)OrderStatus.Combine)
                    {
                        //check 同建立者、stucture相同、狀態-處理中
                        var toOrder = Query.FirstOrDefault(x => x.ID == model.Order.CombineOrderID
                        && x.CreateUser == data.CreateUser
                        && x.StructureID == data.StructureID
                        && x.OrderStatus == (int)OrderStatus.Processing);


                        if (toOrder == null)
                        {
                            result.Message = SystemMessage.Error;
                        }
                        else if (data.FromCombineOrder.Any())
                        {
                            //不可合併多次: a->b->c 會無法復原
                            result.Message = SystemMessage.NoCombineMulti;
                        }
                        else
                        {
                            //明細全轉過去
                            foreach (var detail in data.erp_OrderDetail)
                            {
                                detail.OrderID = toOrder.ID;
                                detail.OrderStatus = toOrder.OrderStatus;
                                detail.CombineOriOrderID = data.ID;//原訂單ID
                            }
                            //紀錄併入訂單
                            data.CombineOrderID = model.Order.CombineOrderID;
                            //原依附在舊訂單的,也指向新訂單 (-->不可合併多次)
                            //if (data.FromCombineOrder != null)
                            //{
                            //    foreach (var fromOrder in data.FromCombineOrder)
                            //    {
                            //        fromOrder.CombineOrderID = model.Order.CombineOrderID;
                            //    }
                            //}
                            Db.SaveChanges();

                            //更新訂單價格總和
                            UpdateOrderPrice(data);
                            UpdateOrderPrice(toOrder);
                        }
                    }
                    #endregion

                    #region check new create
                    if (string.IsNullOrEmpty(result.Message)
                       && model.Order.OrderStatus != (int)OrderStatus.Editing
                       && model.Order.OrderStatus != (int)OrderStatus.Delete
                       && model.Order.OrderStatus != (int)OrderStatus.Combine)
                    {
                        var checkResult = NewOrderCheck(data);
                        if (!checkResult.IsSuccess)
                        {
                            result.Message = checkResult.Message;
                        }
                    }
                    #endregion

                    #region Delete
                    if (string.IsNullOrEmpty(result.Message)
                       && model.Order.OrderStatus == (int)OrderStatus.Delete)
                    {
                        //只有建立者自己能刪除 & 在編輯中 & 只有一筆log
                        if (data.CreateUser == model.User.ID && oriOrderStatus == OrderStatus.Editing && data.erp_OrderLog.Count <= 1)
                        {
                            Db.erp_OrderLog.RemoveRange(data.erp_OrderLog.Where(x => x.OrderStatus == (int)OrderStatus.Editing));
                            Db.erp_OrderDiscount.RemoveRange(data.erp_OrderDetail.SelectMany(x => x.erp_OrderDiscount).ToList());
                            Db.mgt_UserProfile.RemoveRange(data.erp_OrderDetail.SelectMany(x => x.TeamMembers).ToList());
                            Db.erp_OrderDetail.RemoveRange(data.erp_OrderDetail);
                            Db.erp_OrderUnit.RemoveRange(data.erp_OrderUnit);
                            Db.mgt_UserProfile.RemoveRange(data.TeamMembers);
                            Db.erp_Order.Remove(data);
                        }
                        else
                        {
                            result.Message = SystemMessage.Error;
                        }
                    }
                    #endregion

                    //update
                    if (string.IsNullOrEmpty(result.Message))
                    {
                        if (updateNote)
                        {
                            data.PublicNote = model.Order.PublicNote;
                            data.PrivateNote = model.Order.PrivateNote;
                        }

                        Db.SaveChanges();

                        // 更新銷售量
                        if (data.erp_OrderDetail != null)
                        {
                            UpdateSaleCount(data.erp_OrderDetail.Select(x => x.ItemID).ToList());
                        }

                        Db.SaveChanges();

                        trans.Complete();
                        if (model.Order.OrderStatus == (int)OrderStatus.Delete || model.Order.OrderStatus == (int)OrderStatus.Invalid)
                        {
                            result.Message = SystemMessage.DeleteSuccess;
                        }
                        else
                        {
                            result.Message = SystemMessage.UpdateSuccess;
                        }
                        result.IsSuccess = true;
                    }
                }
                catch (Exception ex)
                {
                    result.Message = SystemMessage.UpdateFail;
                    _Log.CreateText("Order_ChangeStatus" + _Json.ModelToJson(ex));
                }
            }//end using trans

            userService.Dispose();
            return result;
        }

        /// <summary>
        /// 修改明細狀態 多個
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="accountType">Type of the account.</param>
        /// <returns></returns>
        public CiResult ChangeDetailStatusAll(OrderViewModel model, AccountType accountType)
        {
            CiResult result = new CiResult();
            var data = Get(model.Order.ID);

            //包含流程
            var orderStatus = data.cms_Structure.OrderStatuses.ToContainList<OrderStatus>();

            try
            {

                foreach (var orderDetail in model.OrderDetails)
                {
                    var dataDetail = data.erp_OrderDetail.First(x => x.ID == orderDetail.ID);
                    var changeStatus = (OrderStatus)orderDetail.OrderStatus;

                    //允許流程
                    var allowStep = OrderTool.AllowSteps((OrderStatus)dataDetail.OrderStatus, accountType, orderStatus);

                    // Status change
                    if ((int)changeStatus != dataDetail.OrderStatus && changeStatus != 0)
                    {
                        if (allowStep.Contains(changeStatus))
                        {
                            dataDetail.OrderStatus = (int)changeStatus;
                        }
                        else
                        {
                            result.Message = SystemMessage.UpdateFail;
                        }
                    }
                    else
                    {
                        continue;
                    }


                    //放棄名額 > 移除選手名單 (才能再報同盃賽)
                    if (string.IsNullOrEmpty(result.Message)
                        && dataDetail.OrderStatus == (int)OrderStatus.Abandon)
                    {
                        var delMember = dataDetail.TeamMembers;
                        if (delMember.Any())
                        {
                            Db.mgt_UserProfile.RemoveRange(delMember);
                        }
                    }
                }

                //update
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
                _Log.CreateText("Order_ChangeDetailStatusAll" + _Json.ModelToJson(ex));
            }

            return result;
        }

        /// <summary>
        /// 修改明細狀態 單個
        /// </summary>
        /// <param name="detailID">The detail identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="accountType">Type of the account.</param>
        /// <returns></returns>
        public CiResult ChangeDetailStatus(Guid detailID, OrderStatus changeStatus, AccountType accountType)
        {
            CiResult result = new CiResult();
            var dataDetail = GetDetail(detailID);

            //包含流程
            var orderStatus = dataDetail.erp_Order.cms_Structure.OrderStatuses.ToContainList<OrderStatus>();
            //允許流程
            var allowStep = OrderTool.AllowSteps((OrderStatus)dataDetail.OrderStatus, accountType, orderStatus);

            try
            {
                // Status change
                if (string.IsNullOrEmpty(result.Message)
                    && (int)changeStatus != dataDetail.OrderStatus)
                {
                    if (allowStep.Contains(changeStatus))
                    {
                        dataDetail.OrderStatus = (int)changeStatus;
                    }
                    else
                    {
                        result.Message = SystemMessage.UpdateFail;
                    }
                }

                //放棄名額 > 移除選手名單 (才能再報同盃賽)
                if (string.IsNullOrEmpty(result.Message)
                    && dataDetail.OrderStatus == (int)OrderStatus.Abandon)
                {
                    var delMember = dataDetail.TeamMembers;
                    if (delMember.Any())
                    {
                        Db.mgt_UserProfile.RemoveRange(delMember);
                    }
                }

                //update
                if (string.IsNullOrEmpty(result.Message))
                {
                    Db.SaveChanges();

                    result.Message = SystemMessage.Success;
                    result.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.UpdateFail;
                _Log.CreateText("Order_ChangeDetailStatus" + _Json.ModelToJson(ex));
            }

            return result;
        }



        ///// <summary>
        ///// 選手退賽
        ///// </summary>
        ///// <param name="memberID">The member identifier.</param>
        ///// <returns></returns>
        //public CiResult MemberOurOrderTeam(Guid memberID, Guid creater)
        //{
        //}

        /// <summary>
        /// 復原合併
        /// </summary>
        /// <param name="detailID">The detail identifier.</param>
        /// <param name="userID">The user identifier.</param>
        /// <returns></returns>
        public CiResult Revert(Guid detailID, Guid userID)
        {
            CiResult result = new CiResult();


            try
            {
                var detail = Db.erp_OrderDetail.Find(detailID);
                var data = detail.erp_Order;

                //check 狀態-已合併
                var toOrder = detail.erp_OrderCombineOri;
                if (toOrder.OrderStatus != (int)OrderStatus.Combine)
                {
                    result.Message = SystemMessage.Error;
                }
                else
                {
                    //明細復原                   
                    detail.OrderID = toOrder.ID;
                    detail.OrderStatus = toOrder.OrderStatus;
                    detail.CombineOriOrderID = null;

                    //復原訂單狀態改為-處理中
                    toOrder.OrderStatus = (int)OrderStatus.Processing;
                    toOrder.CombineOrderID = null;
                    AddLog(toOrder.ID, (OrderStatus)toOrder.OrderStatus, userID);

                    //更新訂單價格總和
                    UpdateOrderPrice(data);
                    UpdateOrderPrice(toOrder);

                    Db.SaveChanges();

                    result.Message = SystemMessage.UpdateSuccess;
                    result.IsSuccess = true;
                }

            }
            catch (Exception ex)
            {
                result.Message = SystemMessage.UpdateFail;
                _Log.CreateText("Order_Revert" + _Json.ModelToJson(ex));
            }

            return result;
        }

        /// <summary>
        /// 更新訂單價格總和
        /// </summary>
        /// <param name="entity">The entity.</param>
        private void UpdateOrderPrice(erp_Order entity)
        {
            //Detail總和
            entity.DetailPrice = entity.erp_OrderDetail.Sum(x => x.SalePrice * (decimal)x.Quantity);

            //訂單總和 = 明細+運費+(負)折扣
            entity.TotalPrice = entity.DetailPrice + entity.ShippingFee + entity.erp_OrderDiscount.Sum(x => x.DiscountPrice);
        }

        /// <summary>
        /// 更新訂單明細&訂單優惠
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="langType">Type of the language.</param>
        /// <param name="models">The models.</param>
        private void UpdateOrderDetails(erp_Order entity, LanguageType langType, List<EditOrderDetail> models)
        {
            ItemService itemService = new ItemService { ClientID = ClientID };
            var orderDetails = new List<erp_OrderDetail>();

            //原項目(取標題、金額) in detail.ItemID
            var itemList = itemService.GetListView(models.Select(x => x.ItemID).ToList(), langType);

            #region OrderDtail

            //展開訂單明細 (不同明細可選同一個Item)
            foreach (var detail in models)
            {
                var itemModel = itemList.First(x => x.Item.ID == detail.ItemID);
                var orderDetail = new erp_OrderDetail();
                if (detail.ID == Guid.Empty)
                {
                    //create
                    orderDetail.ID = Guid.NewGuid();
                    entity.erp_OrderDetail.Add(orderDetail);
                }
                else
                {
                    //update
                    orderDetail = entity.erp_OrderDetail.FirstOrDefault(x => x.ID == detail.ID);
                }

                //計價類型 todo check
                if (itemModel.Item.PriceType != 0)
                {
                    if (itemModel.Item.PriceType == (int)PriceType.Person)
                    {
                        detail.Quantity = detail.DetailMemberID.Count();
                    }
                    else if (itemModel.Item.PriceType == (int)PriceType.Group)
                    {
                        detail.Quantity = 1;
                    }
                }

                //從原文章取-價格、標題
                orderDetail.ItemID = detail.ItemID;
                orderDetail.OrderStatus = entity.OrderStatus;
                orderDetail.SalePrice = itemModel.Item.SalePrice;
                orderDetail.ItemSubject = itemModel.ItemLanguage.Subject;
                orderDetail.DetailTeamName = detail.DetailTeamName;
                orderDetail.FilePath = detail.FilePath;
                orderDetail.Option = detail.Option;
                orderDetail.Quantity = detail.Quantity; //(SignUp=1, Competition=MemberCount)

                orderDetails.Add(orderDetail);

                //TeamMembers
                orderDetail.TeamMembers.Clear();
                if (detail.DetailMemberID != null)
                {
                    orderDetail.TeamMembers = Db.mgt_UserProfile.Where(x => detail.DetailMemberID.Contains(x.ID)).ToList();
                }
            }
            #endregion

            #region OrederDiscount
            //暫無 OrderDetailID = null 的 OrderDiscount

            //Item文章 排序
            if (itemList.Any(x => x.Item.cms_Structure.ContentTypes.HasValue((int)ContentType.Sort)))
            {
                orderDetails = orderDetails.OrderByDescending(x => itemList.First(y => y.Item.ID == x.ItemID).Item.Sort).ToList();
            }

            //項目包含的所有優惠
            var discIDs = itemList.SelectMany(x => x.ParentItems).Where(x => x.ItemTypes.HasValue((int)ItemType.OrderDiscount)).Select(x => x.ID).Distinct().ToList();
            var discList = itemService.GetListView(discIDs, langType);


            //-------[團體價限制]-------
            //優惠each 
            foreach (var discItem in discList.Where(x => x.Item.DiscountType == (int)DiscountType.GroupPrice))
            {
                //包含這種優惠的訂單明細
                var items = itemList.Where(x => x.ParentItems.Any(y => y.ID == discItem.Item.ID)).Select(x => x.Item.ID).ToList();
                var details = orderDetails.Where(x => items.Contains(x.ItemID)).ToList();

                //依優惠種類加入折扣           
                foreach (var detail in details)
                {
                    detail.Quantity = discItem.Item.StockCount;
                }
            }

            //-------[選手多件優惠]-------
            //所有選手each
            foreach (var member in entity.TeamMembers)
            {
                //以使用優惠的detail (優惠使用條件數量大的優先)
                var existDetailID = new List<Guid>();
                //優惠each 條件大到小
                foreach (var discItem in discList.Where(x => x.Item.DiscountType == (int)DiscountType.MemberMultiple)
                                           .OrderByDescending(x => x.Item.StockCount))
                {
                    //包含這種優惠的訂單明細
                    var items = itemList.Where(x => x.ParentItems.Any(y => y.ID == discItem.Item.ID)).Select(x => x.Item.ID).ToList();
                    var details = orderDetails.Where(x => items.Contains(x.ItemID)).ToList();
                    //訂單跳過達成數量(同訂單不會有重複的人)
                    var addDetail = details.Where(x => x.TeamMembers.Any(y => y.ID == member.ID)).ToList();
                    if (discItem.Item.StockCount > 1)
                    {
                        addDetail = addDetail.Skip(discItem.Item.StockCount - 1).ToList();
                    }
                    foreach (var detail in addDetail)
                    {
                        if (existDetailID.Contains(detail.ID))
                        {
                            continue; //已套用過優惠的跳過
                        }
                        //加入訂單優惠
                        var discount = new erp_OrderDiscount
                        {
                            ID = Guid.NewGuid(),
                            OrderID = entity.ID,
                            OrderDetailID = detail.ID,
                            DiscountID = discItem.Item.ID,
                            DiscountPrice = (detail.SalePrice - discItem.Item.SalePrice) * -1 //計算金額=訂單價-折扣價
                        };
                        Db.erp_OrderDiscount.Add(discount);
                        existDetailID.Add(detail.ID);
                    }
                }
            }

            #endregion

            itemService.Dispose();
        }

        /// <summary>
        /// 更新銷售量
        /// </summary>
        /// <returns></returns>
        private bool UpdateSaleCount(List<Guid> itemIDs)
        {
            var items = Db.cms_Item.Where(x => itemIDs.Contains(x.ID)).ToList();

            //每個item in details都更新
            foreach (var id in itemIDs)
            {
                //跨Order取明細, 狀態在運行中
                var count = Db.erp_OrderDetail.Count(x => x.ItemID == id
                                                     && OrderTool.EnabledStatus.Contains((OrderStatus)x.OrderStatus));

                var item = items.Find(x => x.ID == id);
                //包含欄位          
                List<ContentType> contentTypes = item.cms_Structure.ContentTypes.ToContainList<ContentType>();

                item.SaleCount = count;
                //check 
                if (contentTypes.Contains(ContentType.StockCount)
                    && item.SaleCount > item.StockCount)
                {
                    return false; //--超出銷售量
                }
            }

            Db.SaveChanges();

            return true;
        }

        /// <summary>
        /// proc 取得SubItem與銷售量
        /// </summary>
        /// <param name="articleID">The item identifier.</param>
        /// <returns></returns>
        public List<ItemProductModel> GetSubOrderList(Guid articleID)
        {
            var data = new List<ItemProductModel>();
            try
            {
                data = Db.Database.SqlQuery<ItemProductModel>("Proc_GetSubOrderList @ArticleID",
                       new SqlParameter("@ArticleID", articleID)
                       ).ToList();
            }
            catch (Exception ex)
            {
                _Log.CreateText("GetSubOrderList:" + _Json.ModelToJson(ex));
            }
            return data;
        }

        public CiResult CheckSaleCount(List<KeyValueModel> itemIDs, Guid articleID, string addMessage = "", bool noCount = false)
        {
            var result = new CiResult();
            result.IsSuccess = true;

            var model = GetSubOrderList(articleID).Where(x => itemIDs.Any(y => y.Key == x.ID) && x.IsCheckSaleCount).ToList();

            //每個item in details都更新
            foreach (var item in model)
            {
                var quantity = noCount ? 0 : itemIDs.Where(x => x.Key == item.ID).Sum(x => x.Value);

                //check 
                if (item.SaleCount + quantity > item.StockCount)
                {
                    result.IsSuccess = false;//--超出銷售量
                    result.Message = (quantity != 0 ? $"{item.Subject} " : "") + SystemMessage.StockNotEnough;
                    _Log.CreateText($"Order OverSoldFail {addMessage}: {item.Subject} [Sale]{item.SaleCount} + [Quantity]{quantity} > [Stock]{item.StockCount}");
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// 加入訂單變更紀錄
        /// </summary>
        /// <param name="orderID">The order identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="userID">The user identifier.</param>
        private void AddLog(Guid orderID, OrderStatus status, Guid userID, string dataContent = "")
        {
            var data = new erp_OrderLog
            {
                OrderID = orderID,
                OrderStatus = (int)status,
                DataContent = dataContent, //產生虛擬帳號
                CreateTime = DateTime.Now,
                CreateUser = userID
            };
            Db.erp_OrderLog.Add(data);
        }

        #endregion

        #region OrderNumber

        /// <summary>
        /// 取得訂單編號 (不分站台唯一)
        /// </summary>
        /// <returns></returns>
        private CiResult<string> CreateOrderNumber()
        {
            //訂單編號=日期(6碼)流水號(6碼)
            var result = new CiResult<string>();

            try
            {
                var date = DateTime.Now;

                //create
                var count = GetOrderCodeCount(date.Date);
                var number = (count + 1).ToString().PadLeft(6, '0');
                var dateCode = date.ToString("yyMMdd");
                var orderNumber = $"{dateCode}{number}";

                //repeat (try 3 times)
                int i = 0;
                while (i < 1)
                {
                    if (!CheckOrderCode(orderNumber))
                    {
                        long maxNumberInt = 0;
                        var maxNumber = GetOrderCodeMax(date);
                        if (long.TryParse(maxNumber, out maxNumberInt))
                        {
                            orderNumber = (maxNumberInt + 1).ToString();
                        }
                    }
                    else
                    {
                        break;
                    }
                    i++;
                }

                //check
                if (orderNumber.ToTrim().Length == 12)
                {
                    result.Data = orderNumber;
                    result.IsSuccess = true;
                }
                else
                {
                    _Log.CreateText("CreateOrderNumber: error " + orderNumber);
                }
            }
            catch (Exception ex)
            {
                _Log.CreateText("CreateOrderNumber:" + _Json.ModelToJson(ex));
            }

            if (!result.IsSuccess)
            {
                result.Message = SystemMessage.SystemNumberError;
            }

            return result;
        }

        /// <summary>
        /// OrderNumber-取得資料數量 不分Client、同日期 
        /// </summary>
        /// <returns></returns>
        private int GetOrderCodeCount(DateTime date)
        {
            var sqlStr = $"select count(*) from erp_Order "
                       + $"where convert(varchar(10),CreateTime,111)='{date.ToString("yyyy/MM/dd")}'";

            return Db.Database.SqlQuery<int>(sqlStr).First();
        }

        private string GetOrderCodeMax(DateTime date)
        {
            var sqlStr = $"select Max(OrderNumber) from erp_Order "
                       + $"where convert(varchar(10),CreateTime,111)='{date.ToString("yyyy/MM/dd")}'";

            return Db.Database.SqlQuery<string>(sqlStr).First();
        }

        /// <summary>
        /// OrderNumber-確認無重複
        /// </summary>
        /// <param name="orderNumber">The order number.</param>
        /// <returns></returns>
        private bool CheckOrderCode(string orderNumber)
        {
            var sqlStr = $"select count(*) from erp_Order "
                       + $"where OrderNumber = '{orderNumber} '";

            var data = Db.Database.SqlQuery<int>(sqlStr).First();
            return data == 0;
        }

        private string GetNewOrderNumber()
        {
            var data = Db.Database.SqlQuery<string>("Proc_NewOrderNumber").First();
            return data;
        }
        #endregion

        /// <summary>
        /// 編輯超過期限-放棄名額
        /// </summary>
        public CiResult OverEditDeadline(bool isTest = false)
        {
            var addStr = isTest ? "[Test] " : "";
            var result = new CiResult();
            try
            {
                var endTime = DateTime.Now.Date;

                //明細狀態=未完成
                var orderDetails = Db.erp_OrderDetail.Where(x => x.OrderStatus == (int)OrderStatus.TeamEdit
                    //完全無選手名單
                    && !x.TeamMembers.Any()
                    //超過編輯時間
                    && x.erp_Order.EditDeadline != null && x.erp_Order.EditDeadline < endTime && x.erp_Order.ClientID == ClientID
                    //文章存在
                    && !x.cms_Item.IsDelete && !x.erp_Order.cms_Item.IsDelete
                ).ToList();

                _Log.CreateText($"------{addStr} OverEditDeadline Start ------");
                var i = 0;
                foreach (var item in orderDetails)
                {
                    item.OrderStatus = (int)OrderStatus.Abandon;
                    _Log.CreateText($"OverEditDeadline: {item.erp_Order.OrderNumber}, {item.erp_Order.EditDeadline.ToDateString("yyyy/MM/dd HH:mm")}, {item.ItemSubject}");
                    i++;
                }

                //save
                if (!isTest)
                {
                    Db.SaveChanges();
                }

                _Log.CreateText($"------{addStr} OverEditDeadline Done ------");

                result.IsSuccess = true;
                result.Message = $"編輯超過期限-放棄名額: 共{i}筆";
            }
            catch (Exception ex)
            {
                _Log.CreateText("Order_OverEditDeadline error" + _Json.ModelToJson(ex));
            }

            return result;
        }

        /// <summary>
        /// 釋放名額(已放棄->作廢)
        /// </summary>
        /// <returns></returns>
        public CiResult ReleaseSaleCount(int hour, int minute, bool isTest = false)
        {
            var addStr = isTest ? "[Test] " : "";
            var result = new CiResult();
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    var nowTime = DateTime.Now; //new DateTime(2019, 4, 7, 9, 59, 50);
                    var endTime = nowTime >= nowTime.Date.AddHours(hour).AddMinutes(minute) ?
                                  nowTime.Date : nowTime.Date.AddDays(-1);

                    //明細狀態=已放棄
                    var orderDetails = Db.erp_OrderDetail.Where(x => x.OrderStatus == (int)OrderStatus.Abandon
                        //超過編輯時間 (隔日早上10:00)
                        && x.erp_Order.EditDeadline != null && x.erp_Order.EditDeadline < endTime && x.erp_Order.ClientID == ClientID
                        //文章存在
                        && !x.cms_Item.IsDelete && !x.erp_Order.cms_Item.IsDelete
                    ).ToList();

                    _Log.CreateText($"------{addStr} ReleaseSaleCount Start ------");
                    var i = 0;
                    foreach (var item in orderDetails)
                    {
                        item.OrderStatus = (int)OrderStatus.Invalid;
                        _Log.CreateText($"OverEditDeadline: {item.erp_Order.OrderNumber}, {item.erp_Order.EditDeadline.ToDateString("yyyy/MM/dd HH:mm")}, {item.ItemSubject}");
                        i++;
                    }

                    //save
                    if (!isTest)
                    {
                        Db.SaveChanges();

                        //更新數量
                        var itemIDs = orderDetails.Select(x => x.ItemID).Distinct().ToList();
                        var countResult = UpdateSaleCount(itemIDs);
                        if (countResult)
                        {
                            trans.Complete();

                            result.IsSuccess = true;
                        }
                    }

                    result.Message = $"釋放名額: 共{i}筆";
                    _Log.CreateText($"------{addStr} ReleaseSaleCount Done ------");
                }
                catch (Exception ex)
                {
                    _Log.CreateText("Order_ReleaseSaleCount error" + _Json.ModelToJson(ex));
                }//end try
            }//end using trans

            return result;
        }

        #region check 

        public CiResult ItemPreCheckWithID(Guid userID, Guid itemID, int quantity = 1, bool checkStock = true, bool checkUser = true)
        {
            var item = Db.cms_Item.Find(itemID);
            return ItemPreCheck(userID, item, quantity, checkStock);
        }

        /// <summary>
        /// 訂購商品前檢查
        /// </summary>
        /// <param name="userID">The user identifier.</param>
        /// <param name="itemID">The item identifier.</param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="checkStock">if set to <c>true</c> [check stock].</param>
        /// <returns></returns>
        public CiResult ItemPreCheck(Guid userID, cms_Item item, int quantity = 1, bool checkStock = true, bool checkUser = true)
        {
            var result = new CiResult();

            //包含欄位          
            List<ContentType> ContentTypes = item.cms_Structure.ContentTypes.ToContainList<ContentType>();

            if (checkUser)
            {
                var user = Db.mgt_User.Find(userID);

                //允許身分
                if (ContentTypes.Contains(ContentType.OrderAllowRole))
                {
                    var allowRoles = item.cms_ItemOrderRoleRelation.Where(x => x.ItemOrderRoleType == (int)ItemOrderRoleType.OrderAllowRole).Select(x => x.RoleID).ToList();
                    if (!user.mgt_UserRoleRelation.Any(x => allowRoles.Contains(x.RoleID) && x.IsEnabled && !x.IsDelete
                    && (!x.IsTimeLimited || (x.IsTimeLimited && (x.StartTime == null || x.StartTime <= DateTime.Now) && (x.EndTime == null || x.EndTime.Value.AddDays(1) > DateTime.Now)))
                    ))
                    {
                        result.Message = SystemMessage.NoAuthorize;
                    }
                }

                //user完成Email驗證
                if (string.IsNullOrEmpty(result.Message)
                    && !user.EmailIsVerify)
                {
                    result.Message = SystemMessage.EmailNotEnabled;
                }
            }

            //日期+時間
            if (ContentTypes.Contains(ContentType.SaleDateHourRange))
            {
                if (string.IsNullOrEmpty(result.Message)
               && item.SaleStartTime != null && item.SaleStartTime > DateTime.Now)
                {
                    result.Message = SystemMessage.StartTimeError;
                }
                if (string.IsNullOrEmpty(result.Message)
                   && item.SaleEndTime != null && item.SaleEndTime < DateTime.Now)
                {
                    result.Message = SystemMessage.EndTimeError;
                }
            }

            //判斷販售時間-日期
            else if (ContentTypes.Contains(ContentType.SaleDateRange))
            {
                if (string.IsNullOrEmpty(result.Message)
               && item.SaleStartTime != null && item.SaleStartTime > DateTime.Now)
                {
                    result.Message = SystemMessage.StartTimeError;
                }
                if (string.IsNullOrEmpty(result.Message)
                   && item.SaleEndTime != null && item.SaleEndTime.Value.AddDays(1) < DateTime.Now)
                {
                    result.Message = SystemMessage.EndTimeError;
                }
            }

            //detail進service判斷
            //if (checkStock)
            //{
            //    //判斷販售數量
            //    if (string.IsNullOrEmpty(result.Message)
            //        && ContentTypes.Contains(ContentType.StockCount)
            //        && item.SaleCount + quantity > item.StockCount)
            //    {
            //        //剩餘xx組
            //        if (item.SaleCount + 1 <= item.StockCount)
            //        {
            //            result.Message = string.Format(SystemMessage.StockRemain, item.cms_ItemLanguage.FirstOrDefault()?.Subject, item.StockCount - item.SaleCount);
            //        }
            //        //售完
            //        else
            //        {
            //            result.Message = item.cms_ItemLanguage.FirstOrDefault()?.Subject + ": " + SystemMessage.NoStock;
            //        }
            //    }
            //}

            //不可重複報名 (拙八用)
            //if (string.IsNullOrEmpty(result.Message)
            //    && ContentTypes.Contains(ContentType.OrderNoRepeatMember)
            //    && user.erp_Order.Any(x => x.ItemID == item.ID && OrderTool.EnabledStatus.Contains((OrderStatus)x.OrderStatus)))
            //{
            //    result.Message = SystemMessage.NoRepeatMember;
            //}

            if (string.IsNullOrEmpty(result.Message))
            {
                result.IsSuccess = true;
            }

            return result;
        }

        /// <summary>
        /// 新訂單檢查
        /// </summary>
        /// <returns></returns>
        private CiResult NewOrderCheck(erp_Order data)
        {
            var result = new CiResult();

            if (data.erp_OrderDetail == null || !data.erp_OrderDetail.Any())
            {
                // 必須要有detail
                result.Message = SystemMessage.OrderEmpty;
            }
            else
            {
                // 商品數量不可為0
                foreach (var detail in data.erp_OrderDetail.ToList())
                {
                    if (detail.Quantity == 0)
                    {
                        result.Message = SystemMessage.Error;
                    }
                }
            }

            return result;
        }

        #endregion

        #region Pay
        /// <summary>
        /// 建立國泰虛擬帳號
        /// </summary>
        /// <param name="payDeadline">繳款期限</param>
        /// <returns></returns>
        private CiResult<string> CreateVirtualAccount(DateTime payDeadline, int price)
        {
            var result = new CiResult<string>();

            try
            {
                string virtualAccount = "";

                //滑輪規則
                if (ClientID == new Guid("524dde74-fdef-481a-b9ed-49bab41f7964"))
                {
                    //第1~4碼:代碼
                    string code = "4033";

                    //第5~10碼:為限繳日期(YYMMDD)                 
                    var dateCode = payDeadline.ToString("yyMMdd");

                    //第11~13碼:流水號
                    long maxNumberInt = 0;
                    var maxNumber = Query.Where(x => x.PayDeadline == payDeadline).Max(x => x.VirtualAccount);

                    if (maxNumber != null
                        && long.TryParse(maxNumber.Substring(10, 3), out maxNumberInt))
                    {
                        maxNumberInt += 1;
                    }
                    maxNumber = maxNumberInt.ToString().PadLeft(3, '0');

                    virtualAccount = $"{code}{dateCode}{maxNumber}";


                    //第14碼:檢查碼                   
                    //帳號檢核
                    var accCheck = VitualCheckCode(virtualAccount, "4567891234567");
                    //金額檢核
                    var monCheck = VitualCheckCode(price.ToString().PadLeft(8, '0'), "87654321");
                    //帳號檢核碼+金額檢核碼 取個位數
                    int checkCode = (accCheck + monCheck) % 10;

                    virtualAccount += checkCode.ToString();
                }

                //check
                if (virtualAccount.ToTrim().Length == 14 && !Query.Any(x => x.VirtualAccount == virtualAccount))
                {
                    result.Data = virtualAccount;
                    result.IsSuccess = true;
                }
                else
                {
                    _Log.CreateText("CreateVirtualAccount: error " + virtualAccount);
                }
            }
            catch (Exception ex)
            {
                _Log.CreateText("CreateVirtualAccount:" + _Json.ModelToJson(ex));
            }

            if (!result.IsSuccess)
            {
                result.Message = SystemMessage.SystemNumberError;
            }

            return result;
        }

        /// <summary>
        /// 產生檢核碼
        /// </summary>
        /// <param name="virtualAccount">The virtual account.</param>
        /// <returns></returns>
        private int VitualCheckCode(string virtualAccount, string numstr)
        {
            //1.帳號與權數,上下相乘後取個位數
            int sum = 0;
            for (int i = 0; i < numstr.Length; i++)
            {
                int acc = int.Parse(virtualAccount.Substring(i, 1));
                int num = int.Parse(numstr.Substring(i, 1));
                sum += (acc * num) % 10;
            }

            //2.相加再取個位數
            sum = sum % 10;

            //3.10-x
            sum = (10 - sum) % 10;

            return sum;
        }


        /// <summary>
        /// 國泰入帳通知解密
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="isByQuery">自動通知or排程再查詢</param>
        /// <returns></returns>
        public CiResult<List<erp_Order>> CathyPayMessage(string messages, bool isByQuery = false)
        {
            var result = new CiResult<List<erp_Order>>()
            {
                Data = new List<erp_Order>(),
                IsSuccess = true
            };
            string passkey = "";

            //即時主動回傳 xml
            if (!isByQuery)
            {
                #region 解密

                //滑輪解密key
                if (ClientID == new Guid("524dde74-fdef-481a-b9ed-49bab41f7964"))
                {
                    passkey = "rollersports7211";
                }

                //找到xml的data節點,解密資料
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(messages);
                byte[] bdata;
                foreach (XmlNode xNode in xmlDoc.SelectNodes("//MYB2B/BODY"))
                {
                    string data = xNode.SelectSingleNode("DATA").InnerText;
                    bdata = _Crypto.HexToByte(data);
                    data = System.Text.Encoding.ASCII.GetString(bdata);
                    messages = "";
                    foreach (char c in data)
                    {
                        int unicode = c;
                        if (unicode != 0) messages += Convert.ToChar(c);
                    }

                    messages = _Crypto.AES_Decrypt(messages, passkey);
                }

                #endregion
            }

            //多筆訊息以換行符號相隔
            string[] messageArr = messages.SplitByNewLine();
            foreach (var msg in messageArr)
            {
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    //儲存入帳通知
                    var resultMsg = AddPayMessage(msg, isByQuery);
                    result.IsSuccess &= resultMsg.IsSuccess;

                    if (resultMsg.IsSuccess && resultMsg.Data != null)
                    {
                        if (isByQuery)
                        {
                            _Log.CreateText($"QueryPayMessage Success: {resultMsg.Data.VirtualAccount}");
                        }

                        result.Data.Add(resultMsg.Data);
                    }
                }
            }//end foreach

            return result;
        }

        /// <summary>
        /// 國泰儲存入帳通知
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="payType">Type of the pay.</param>
        /// <param name="isByQuery">自動通知or排程再查詢</param>
        /// <returns></returns>
        private CiResult<erp_Order> AddPayMessage(string msg, bool isByQuery = false)
        {
            var result = new CiResult<erp_Order>();
            string addStr = isByQuery ? " [Console]" : "";
            try
            {
                var model = new erp_GetPayMessage
                {
                    ClientID = ClientID,
                    OriData = msg,
                    //DecryptData
                    //OrderID
                    PayType = (int)PayType.ATMVirtual,
                    //PayPrice
                    //PayTime
                    CreateTime = DateTime.Now,
                    IsByQuery = isByQuery,
                    IsEnabled = true
                };

                //國泰虛擬帳號               
                string virtualAccount = "";

                #region 解字串

                model.DecryptData = msg;
                model.PayPrice = int.Parse(model.DecryptData.Substring(38, 14));

                /*
                223035004687      591861717          2+0000000001000+00000000200300040331902180272      0999 跨行轉入812
                0028881000601099    16593020190212

                39 39 SIGN X(01)    交易金額正負號 +或"-"
                40 52 AMOUNT 9(13)  交易金額 靠右,左補 0(無角分) 
                69 88 MEMO1 X(20)   備註一 靠左,右補空白  => 虛擬帳號 4033xxxxxxxxxx    
                189 194 TX_TIME 9(06) 交易時間 24 小時制 HHMMSS
                195 202 TX_DATE 9(08) 交易日期 YYYYMMDD                                 
                其他:交易序號、帳戶餘額、備註、對方帳號
                */

                try
                {
                    //莫名位移
                    var str = model.DecryptData.Trim();
                    var len = str.Length;
                    var yy = model.DecryptData.Substring(len - 8, 4);
                    var MM = model.DecryptData.Substring(len - 4, 2);
                    var dd = model.DecryptData.Substring(len - 2, 2);
                    var HH = model.DecryptData.Substring(len - 14, 2);
                    var mm = model.DecryptData.Substring(len - 12, 2);
                    var ss = model.DecryptData.Substring(len - 10, 2);

                    model.PayTime = new DateTime(int.Parse(yy), int.Parse(MM), int.Parse(dd), int.Parse(HH), int.Parse(mm), int.Parse(ss));
                }
                catch (Exception e)
                {

                }
                virtualAccount = model.DecryptData.Substring(68, 20).ToTrim();
                model.VirtualAccount = virtualAccount;
                #endregion

                using (TransactionScope trans = new TransactionScope())
                {
                    var statusResult = new CiResult();

                    //訂單待付款
                    var order = Query.FirstOrDefault(x => x.VirtualAccount == virtualAccount);

                    //1.查無虛擬帳號 也回傳成功
                    if (order == null)
                    {
                        _Log.CreateText($"AddPayMessage{addStr} error VirtualAccount:" + virtualAccount);

                        var query = Db.erp_GetPayMessage.FirstOrDefault(x => x.VirtualAccount == virtualAccount);
                        //無紀錄,再新增
                        if (query == null)
                        {
                            //在紀錄中找虛擬帳號
                            var orderLog = Db.erp_OrderLog.FirstOrDefault(x => x.DataContent == virtualAccount);
                            if (orderLog != null)
                            {
                                model.OrderID = orderLog.OrderID;
                            }
                            model.IsEnabled = false;
                            statusResult.IsSuccess = true;
                        }
                        //直接回傳成功
                        else
                        {
                            result.IsSuccess = true;
                        }
                    }
                    //2.非待付款 也回傳成功,避免銀行一直重傳
                    else if (order.OrderStatus != (int)OrderStatus.NonPayment)
                    {
                        _Log.CreateText($"AddPayMessage{addStr} 狀態不需付款:" + (OrderStatus)order.OrderStatus + " " + order.OrderNumber);


                        //無入帳資訊 再新增
                        if (order.erp_GetPayMessage == null)
                        {
                            model.OrderID = order.ID;
                            model.IsEnabled = false;
                            statusResult.IsSuccess = true;
                        }
                        //直接回傳成功
                        else
                        {
                            result.IsSuccess = true;
                        }
                    }
                    //3.變更狀態 (進入 ChangeStatus 在先 GetOrder再 set Status 才能保有OriStatus                         
                    else
                    {
                        model.OrderID = order.ID;
                        var oderView = new OrderViewModel
                        {
                            Order = new erp_Order { ID = order.ID, OrderStatus = (int)OrderStatus.Done },
                            User = new mgt_User { ID = order.CreateUser }
                        };
                        statusResult = ChangeStatus(oderView, AccountType.Admin);
                        result.Data = order; //紀錄order寄出通知信
                    }

                    if (statusResult.IsSuccess)
                    {
                        Db.erp_GetPayMessage.Add(model);
                        Db.SaveChanges();

                        result.IsSuccess = true;
                        trans.Complete();
                    }
                }

            }
            catch (Exception ex)
            {
                _Log.CreateText($"AddPayMessage{addStr}:" + _Json.ModelToJson(ex));
            }

            return result;
        }

        /// <summary>
        /// 自動查帳(Console)
        /// </summary>
        public void QueryPayMessage()
        {
            try
            {
                //查詢功能開關
                var clients = Db.mgt_Client.Where(x => !x.IsDelete && x.IsEnabled).ToList()
                                           .Where(x => x.ClientSetting.HasValue((int)ClientSetting.CathayPayAuto)).ToList();

                //each client
                foreach (var client in clients)
                {
                    string cust_id = "", cust_nickname = "", cust_pwd = "", acno = ""
                           , from_date = "", to_date = "";

                    #region 查詢帳密設定
                    //[滑輪]
                    if (client.ID == new Guid("524dde74-fdef-481a-b9ed-49bab41f7964"))
                    {
                        ClientID = client.ID;
                        cust_id = "00977211";
                        cust_nickname = "xnet2322";
                        cust_pwd = "npc7qq4m7TsV";
                        acno = "223035004687";
                    }
                    #endregion

                    //所有待付款的訂單
                    var orders = Db.erp_Order.Where(x => x.ClientID == ClientID
                                              && x.OrderStatus == (int)OrderStatus.NonPayment && x.PayType == (int)PayType.ATMVirtual && !string.IsNullOrEmpty(x.VirtualAccount)).ToList();
                    //查詢日期範圍
                    if (orders.Any())
                    {
                        from_date = orders.Min(x => x.VirtualCreateTime).ToDateString("yyyyMMdd");//最早的 虛擬帳號建立日期
                        to_date = DateTime.Now.ToDateString("yyyyMMdd");//現在
                        _Log.CreateText($"------QueryPayMessage Range: {from_date}~{to_date}------");
                    }
                    else
                    {
                        //---無訂單需要查詢---
                        continue;
                    }

                    //[測試查詢開關]
                    bool isTest = false;

                    //入帳訊息 (多個以換行符號相隔
                    string messages = "";

                    #region 查詢資料 (間隔查詢時間3分鐘)
                    if (isTest)
                    {
                        messages = "223035004687      392751743          2+0000000000800+00000000147820740331903110398      0999 跨行轉入004                                                                0048004000016330296 13302920190305\n"
                                 + "223035004687      416931743          2+0000000001000+00000000147920740331903060060      0999 跨行轉入808                                                                8080001311979081199 13304220190305\n"
                                 + "223035004687      556231743          2+0000000001600+00000000148160740331903110410      0999 跨行轉入004                                                                0048004000016330296 13315920190305\n";
                    }
                    else
                    {
                        string url = "https://www.globalmyb2b.com/securities/tx10d0_txt.aspx";

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                        request.Method = "POST";
                        request.ContentType = "application/x-www-form-urlencoded";

                        string txdate8 = "Y";
                        //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
                        NameValueCollection postParams = System.Web.HttpUtility.ParseQueryString(string.Empty);
                        postParams.Add("cust_id", cust_id);
                        postParams.Add("cust_nickname", cust_nickname);
                        postParams.Add("acno", acno);
                        postParams.Add("cust_pwd", cust_pwd);
                        postParams.Add("from_date", from_date);
                        postParams.Add("to_date", to_date);
                        postParams.Add("txdate8", txdate8);//交易日期是否西元年 
                        //Console.WriteLine(postParams.ToString());// 將取得"version=1.0&action=preserveCodeCheck&pCode=pCode&TxID=guid&appId=appId", key和value會自動UrlEncode
                        //要發送的字串轉為byte[] 
                        byte[] byteArray = Encoding.Default.GetBytes(postParams.ToString());
                        using (Stream reqStream = request.GetRequestStream())
                        {
                            reqStream.Write(byteArray, 0, byteArray.Length);
                        }//end using

                        //API回傳的字串
                        string responseStr = "";
                        //發出Request
                        using (WebResponse rsp = request.GetResponse())
                        {
                            using (StreamReader sr = new StreamReader(rsp.GetResponseStream(), Encoding.Default))
                            {
                                messages = sr.ReadToEnd();
                                _Log.CreateText("QueryPayMessage\n" + messages);
                            }
                        }
                    }

                    #endregion

                    if (string.IsNullOrEmpty(messages))
                        continue;

                    //狀態改為完成           
                    CathyPayMessage(messages, isByQuery: true);

                    //未付款過期
                    if (!isTest)
                    {
                        foreach (var order in orders)
                        {
                            if (order.PayDeadline.Value.AddDays(1) < DateTime.Now.AddHours(-6))
                            {
                                var oderView = new OrderViewModel
                                {
                                    Order = new erp_Order { ID = order.ID, OrderStatus = (int)OrderStatus.OverduePayment },
                                    User = new mgt_User { ID = order.CreateUser }
                                };
                                var statusResult = ChangeStatus(oderView, AccountType.Admin);
                                if (statusResult.IsSuccess)
                                {
                                    _Log.CreateText($"QueryPayMessage Overdue: {order.VirtualAccount}");
                                }
                            }
                        }
                    }
                } //end foreach

            }
            catch (Exception ex)
            {
                _Log.CreateText("QueryPayMessage:" + _Json.ModelToJson(ex));
            }
        }


        #endregion

        #region test
        /// <summary>
        /// roller 移轉資料
        /// </summary>
        /// <returns></returns>
        public string TransOrder()
        {
            string result = "";

            Guid oldStructureID = new Guid("044f4287-0df2-4f36-97f1-9313e137b8dc");
            Guid newStructureID = new Guid("cad7be08-2c1c-4cad-8546-da5bfb1d0f7b");
            Guid newSubOption = new Guid("792ed2e2-0b57-4216-b9eb-1084755b0d7a");


            //舊order list
            var oldOrders = Query.Where(x => x.StructureID == oldStructureID).ToList();
            //new article
            var articles = Db.cms_Item.Where(x => x.StructureID == newStructureID).ToList();
            //new option
            var subItems = Db.cms_Item
                .Include(x => x.cms_ItemOrderRoleRelation)
                .Where(x => x.StructureID == newSubOption).ToList();

            //轉成新結構
            int count = 1;
            int nullCOunt = 0;
            _Log.CreateText("-------------------------------");
            foreach (var order in oldOrders)
            {
                _Log.CreateText("count:" + count);
                count++;
                var roleRelate = order.mgt_UserRoleRelation.FirstOrDefault();
                var article = articles.FirstOrDefault(x => x.cms_ItemLanguage.FirstOrDefault().Subject == order.cms_Item.cms_ItemLanguage.FirstOrDefault().Subject);
                var detail = order.erp_OrderDetail.FirstOrDefault();
                var itemOption = new cms_Item();

                if (roleRelate != null)
                {
                    itemOption = subItems.FirstOrDefault(x => x.cms_ItemOrderRoleRelation.FirstOrDefault().RoleID == roleRelate.RoleID);
                }

                var price = itemOption?.SalePrice ?? 0;

                var newOrder = new erp_Order
                {
                    ID = Guid.NewGuid(),//new
                    ClientID = order.ClientID,
                    StructureID = newStructureID, //new
                    ItemID = article.ID, //new
                    OrderNumber = order.OrderNumber.Substring(0, 6) + "099" + order.OrderNumber.Substring(9),
                    PayType = order.PayType,
                    DeliveryType = order.DeliveryType,
                    OrderNote = order.OrderNote,
                    FilePath = order.FilePath,
                    PublicNote = order.PublicNote,
                    DetailPrice = price,//new
                    TotalPrice = price,//new
                    OrderStatus = order.OrderStatus,
                    CreateTime = order.CreateTime,
                    CreateUser = order.CreateUser
                };
                Db.erp_Order.Add(newOrder);

                if (article == null)
                {
                    _Log.CreateText("article null:" + order.OrderNumber);
                }

                if (itemOption == null || itemOption.ID == Guid.Empty)
                {
                    _Log.CreateText("itemOption null:" + order.OrderNumber);
                    nullCOunt++;
                }
                else
                {
                    _Log.CreateText("subject:" + itemOption.cms_ItemLanguage.FirstOrDefault()?.Subject);
                    var newDetail = new erp_OrderDetail
                    {
                        ID = Guid.NewGuid(),
                        ItemID = itemOption.ID,
                        ItemSubject = itemOption.cms_ItemLanguage.FirstOrDefault().Subject,
                        SalePrice = price,
                        Quantity = detail.Quantity,
                        OrderStatus = detail.OrderStatus
                    };
                    newOrder.erp_OrderDetail.Add(newDetail);
                }

                if (roleRelate != null)
                {
                    var newRole = new mgt_UserRoleRelation
                    {
                        ID = Guid.NewGuid(),
                        UserID = roleRelate.UserID,
                        RoleID = roleRelate.RoleID,
                        RoleNumber = roleRelate.RoleNumber,
                        IsTimeLimited = roleRelate.IsTimeLimited,
                        StartTime = roleRelate.StartTime,
                        EndTime = roleRelate.EndTime,
                        CreateTime = roleRelate.CreateTime,
                        OrderID = newOrder.ID,//new
                        IsEnabled = roleRelate.IsEnabled,
                        IsDelete = false
                    };
                    newOrder.mgt_UserRoleRelation.Add(newRole);
                }

            }

            _Log.CreateText("----------null: " + nullCOunt + "-----------");

            try
            {
                //  Db.SaveChanges();
            }
            catch (Exception e)
            {
                throw;
            }


            return result;
        }

        #endregion
    }
}
