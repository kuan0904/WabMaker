using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.Tools;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WabMaker.Web.Attributes;
using WabMaker.Web.Authorize;
using WabMaker.Web.Helpers;
using WabMaker.Web.MainService;
using WebMaker.BLL.Helpers;
using WebMaker.BLL.Services;
using WebMaker.Entity.Models;
using WebMaker.Entity.ViewModels;
using static MyTool.Tools.MailTool;

namespace WabMaker.Web.Controllers
{
    /// <summary>
    /// 訂單
    /// </summary>  
    [MemberAuthorize]
    public class OrderController : BaseController
    {
        private OrderService service = new OrderService();
        private ItemService itemService = new ItemService();
        private UserService userService = new UserService();

        public OrderController()
        {
            service.ClientID = ApplicationHelper.ClientID;
            itemService.ClientID = ApplicationHelper.ClientID;
            userService.ClientID = ApplicationHelper.ClientID;
        }

        #region 活動報名、申請身分  (訂單文章=商品)
        /// <summary>
        /// 活動報名、申請身分
        /// </summary>
        /// <param name="id">ItemID</param>
        /// <returns></returns>
        public ActionResult SignUp(Guid id)
        {
            // 文章
            var itemModel = itemService.GetView(id, ApplicationHelper.DefaultLanguage);
            if (itemModel.Item == null) return ErrorPage();

            // 訂購商品前檢查      
            var preCheck = service.ItemPreCheck(SessionManager.UserID, itemModel.Item);
            if (!preCheck.IsSuccess)
            {
                SetAlertMessage(preCheck.Message, AlertType.error);

                return Redirect(RouteHelper.OrderReturnUrl(itemModel.Item.cms_Structure.OrderErrorReturnPage, itemModel.Item.RouteName));
            }

            //自動 add Role        
            var addRole = itemModel.Item.cms_ItemOrderRoleRelation.FirstOrDefault(x => x.ItemOrderRoleType == (int)ContentType.OrderCreateRole)?.mgt_Role;

            //文章項目
            var orderItemStructure = itemModel.Item.cms_Structure.ChildStructures.FirstOrDefault(x => x.ItemTypes.HasValue((int)ItemType.OrderItem));
            var orderItems = new List<ItemProductModel>();
            if (orderItemStructure != null)
            {
                //orderItems = itemService.GetSubOrderList(orderItemStructure.ID, id, ApplicationHelper.DefaultLanguage);
                orderItems = service.GetSubOrderList(id);
                if (addRole == null)
                {
                    //選項內的Role (todo滑輪!!!)
                    //addRole = orderItems.Data.SelectMany(x => x.Item.cms_ItemOrderRoleRelation.Where(y => y.ItemOrderRoleType == (int)ItemOrderRoleType.OrderCreateRole)).FirstOrDefault()?.mgt_Role;
                }
            }

            // User個資(必填用任一CreateRole)
            var userModel = userService.GetView(SessionManager.UserID, addRole);
            var model = new OrderViewModel(userModel);
            model.Order.ItemID = id;
            model.RoleSelectList = itemService.GetOrderRoleSelectList(id, ItemOrderRoleType.OrderCreateRole);
            model.ItemViewModel = itemModel;
            model.ItemViewModel.ItemLanguage.Content = HttpUtility.HtmlDecode(model.ItemViewModel.ItemLanguage.Content);
            model.NewRoleRelation = new mgt_UserRoleRelation();
            model.SubItemViewModel = orderItems;


            return View(ViewName("Order", "SignUp"), model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SignUp(OrderViewModel model, HttpPostedFileBase file)
        {
            if (!Request.IsAjaxRequest())
            {
                return ErrorPage(); //404
            }

            //update
            if (model.Order.ID != Guid.Empty)
            {
                return await OrderUpdate(new EditOrderViewModel { OrderViewModel = model, Block = OrderEditBlock.BasicInfo }, file);
            }
            //new
            else
            {
                //var item = itemService.GetView(model.Order.ItemID.Value, ApplicationHelper.DefaultLanguage);
                //model.ItemViewModel = item;

                //活動只有一筆Detail     
                if (model.OrderDetails == null)
                {
                    model.OrderDetails = new List<EditOrderDetail> {
                    new EditOrderDetail {
                        ItemID = model.Order.ItemID.Value,
                        //ItemSubject = item.ItemLanguage.Subject,
                        //SalePrice = item.Item.SalePrice, //價格in service
                        Quantity = 1
                    }
                };
                }
                //選擇Item與Role
                else
                {
                    model.OrderDetails[0].Quantity = 1;

                    //選項內的Role
                    var orderItem = itemService.GetView(model.OrderDetails[0].ItemID, ApplicationHelper.DefaultLanguage);
                    var newRole = orderItem.Item.cms_ItemOrderRoleRelation.FirstOrDefault(x => x.ItemOrderRoleType == (int)ItemOrderRoleType.OrderCreateRole);
                    if (newRole != null)
                    {
                        model.NewRoleRelation.RoleID = newRole.RoleID;
                    }
                }

                return await OrderCreate(model, file);
            }

        }

        /// <summary>
        /// 瀏覽紀錄
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public ActionResult SignUpInfo(Guid id)
        {
            //訂單
            var order = service.GetView(id, ApplicationHelper.DefaultLanguage);
            //check必須是本人
            if (order.Order.CreateUser != SessionManager.UserID)
            {
                return ErrorPage(); //404
            }

            order.ItemViewModel.ItemLanguage.Content = HttpUtility.HtmlDecode(order.ItemViewModel.ItemLanguage.Content);

            return View(ViewName("Order", "SignUp"), order);
        }
        #endregion

        #region 比賽報名 (多個項目)

        /// <summary>
        /// 比賽報名-新增頁
        /// </summary>
        /// <param name="id">ItemID</param>
        /// <returns></returns>
        public ActionResult NewCompetition(Guid id)
        {
            // 文章
            var itemModel = itemService.GetView(id, ApplicationHelper.DefaultLanguage);
            if (itemModel.Item == null) return ErrorPage();

            //parent文章            
            var parentItem = new ItemViewModel();
            if (itemModel.ParentID != Guid.Empty)
            {
                parentItem = itemService.GetView(itemModel.ParentID, ApplicationHelper.DefaultLanguage, DataLevel.VerySimple);
            }

            // 訂購商品前檢查      
            var preCheck = service.ItemPreCheck(SessionManager.UserID, itemModel.Item);
            if (!preCheck.IsSuccess)
            {
                SetAlertMessage(preCheck.Message, AlertType.error);

                //回上層Item
                var routeName = itemModel.Item.RouteName;
                if (itemModel.Item.cms_Structure.OrderErrorReturnPage == (int)OrderErrorReturnPage.ParentItemDetail)
                {
                    routeName = parentItem.Item.RouteName;
                    itemModel.Item.cms_Structure.OrderErrorReturnPage = (int)OrderErrorReturnPage.ItemDetail;
                }

                return Redirect(RouteHelper.OrderReturnUrl(itemModel.Item.cms_Structure.OrderErrorReturnPage, routeName));
            }

            //文章項目
            var orderItemStructure = itemModel.Item.cms_Structure.ChildStructures.FirstOrDefault(x => x.ItemTypes.HasValue((int)ItemType.OrderItem));
            var orderItems = new List<ItemProductModel>();
            if (orderItemStructure != null)
            {
                //orderItems = itemService.GetSubOrderList(orderItemStructure.ID, id, ApplicationHelper.DefaultLanguage);
                orderItems = service.GetSubOrderList(id);
            }

            // User個資(必填用任一CreateRole)
            var userModel = userService.GetView(SessionManager.UserID, itemModel.Item.cms_ItemOrderRoleRelation.FirstOrDefault(x => x.ItemOrderRoleType == (int)ContentType.OrderCreateRole)?.mgt_Role);
            var model = new OrderViewModel(userModel);
            model.Order.ItemID = id;
            model.ItemViewModel = itemModel;
            model.ItemViewModel.ItemLanguage.Content = HttpUtility.HtmlDecode(model.ItemViewModel.ItemLanguage.Content);
            model.SubItemViewModel = orderItems;
            model.ParentItemViewModel = parentItem;

            var editModel = new EditOrderViewModel
            {
                OrderViewModel = model
            };

            return View(ViewName("Order", "Competition"), editModel);
        }

        /// <summary>
        /// 新增報名後,回到編輯頁
        /// </summary>
        /// <returns></returns>
        public ActionResult ToEditCompetition()
        {
            if (SessionManager.TempOrderID == Guid.Empty)
                return HttpNotFound();

            var orderID = SessionManager.TempOrderID;
            SessionManager.TempOrderID = Guid.Empty;

            return RedirectToAction("Competition", new { id = orderID });
        }

        /// <summary>
        /// 比賽報名-編輯頁
        /// </summary>
        /// <param name="id">OrderID</param>
        /// <returns></returns>
        public ActionResult Competition(Guid id)
        {
            /*
              取整頁資料:Competition資料直接分到各Partial
              各區塊編輯時,再自己取GetCompetition
             */
            return GetCompetition(id, OrderEditBlock.All);
        }

        /// <summary>
        /// 比賽報名-編輯頁 by 管理員
        /// </summary>
        /// <param name="id">OrderID</param>
        /// <returns></returns>        
        [AllowAnonymous]
        public ActionResult ManageCompetition(Guid id)
        {
            if (SessionManager.UserID == Guid.Empty || SessionManager.AccountType != AccountType.Admin)
            {
                return ErrorPage(); //404
            }

            //依前台ClientID
            return GetCompetition(id, OrderEditBlock.All);
        }

        /// <summary>
        /// 比賽報名-取得部分區塊
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="block">The block.</param>
        /// <param name="isEditing">if set to <c>true</c> [is editing].</param>
        /// <returns></returns>
        [PartialCheck]
        [AllowAnonymous]
        public ActionResult GetCompetition(Guid id, OrderEditBlock block = OrderEditBlock.BasicInfo, bool isEditing = false, Guid? OrderDetailID = null)
        {
            if (block != OrderEditBlock.All && !Request.IsAjaxRequest())
            {
                return ErrorPage(); //404
            }
            if (SessionManager.UserID == Guid.Empty)
            {
                return new ContentResult { Content = $"LogOutError" };
            }

            //訂單
            var order = service.GetView(id, ApplicationHelper.DefaultLanguage);
            //check必須是本人 (Admin可進入)
            if (SessionManager.AccountType == AccountType.Member && order.Order.CreateUser != SessionManager.UserID)
            {
                return ErrorPage(); //404
            }

            //文章
            var itemModel = itemService.GetView(order.Order.ItemID.Value, ApplicationHelper.DefaultLanguage);
            if (itemModel == null) return ErrorPage();

            //parent文章
            var parentItem = new ItemViewModel();
            if (itemModel.ParentID != Guid.Empty)
            {
                parentItem = itemService.GetView(itemModel.ParentID, ApplicationHelper.DefaultLanguage, DataLevel.VerySimple);
            }

            //篩選部門 for Admin
            if (itemModel.Item.cms_Structure.ContentTypes.HasValue((int)ContentType.Department) && !SessionManager.IsSuperManager)
            {
                if (SessionManager.AccountType == AccountType.Admin && !SessionManager.DepartmentIDs.Contains(itemModel.Item.DepartmentID.Value))
                {
                    return ErrorPage(); //404
                }
            }

            //文章項目
            var orderItemStructure = itemModel.Item.cms_Structure.ChildStructures.FirstOrDefault(x => x.ItemTypes.HasValue((int)ItemType.OrderItem));
            var orderItems = new List<ItemProductModel>();
            if (orderItemStructure != null)
            {
                //orderItems = itemService.GetSubOrderList(orderItemStructure.ID, item.Item.ID, ApplicationHelper.DefaultLanguage);
                orderItems = service.GetSubOrderList(order.Order.ItemID.Value);
            }

            // User個資(必填用任一CreateRole)            
            var userModel = userService.GetView(order.Order.CreateUser, itemModel.Item.cms_ItemOrderRoleRelation.FirstOrDefault(x => x.ItemOrderRoleType == (int)ContentType.OrderCreateRole)?.mgt_Role);
            order.User = userModel.User;
            order.UserProfile = userModel.UserProfile;
            order.ItemViewModel = itemModel;
            order.SubItemViewModel = orderItems;
            order.ParentItemViewModel = parentItem;

            // 訂購商品前檢查-完成後可瀏覽 (Admin不檢查)  
            var preCheck = new CiResult();
            if (SessionManager.AccountType == AccountType.Member)
            {
                preCheck = service.ItemPreCheck(SessionManager.UserID, itemModel.Item, checkStock: false, checkUser: false);
                if (!preCheck.IsSuccess)// && order.Order.OrderStatus == (int)OrderStatus.Editing 
                {
                    SetAlertMessage(preCheck.Message, AlertType.error);
                    isEditing = false;
                }
            }
            else if (SessionManager.AccountType == AccountType.Admin)
            {
                preCheck.IsSuccess = true;
            }

            var editModel = new EditOrderViewModel
            {
                OrderViewModel = order,
                Block = block,
                IsEditing = isEditing,
                IsCheckSuccess = preCheck.IsSuccess,
                IsAdmin = (SessionManager.AccountType == AccountType.Admin),
                OrderDetailID = OrderDetailID == null ? Guid.Empty : OrderDetailID.Value
            };

            //取得可報名選手
            if (block == OrderEditBlock.DetailMember)
            {
                var orderDetail = order.OrderDetails.FirstOrDefault(x => x.ID == OrderDetailID);
                var orderDetailItem = itemService.Get(orderDetail.ItemID);
                editModel.TeamMemberAssigns = userService.GetUserAssignToList(order.Order.CreateUser,//SessionManager.UserID
                    noneItemID: order.Order.ItemID,
                    butOrderDetailID: OrderDetailID,
                    noneParentItemID: order.ParentItemViewModel.Item.ID,
                    startBirthday: orderDetailItem.DateLimit);
            }


            string viewName = block == OrderEditBlock.All ? "Competition" : $"_Competition_{block.ToString()}";
            return View(ViewName("Order", viewName), editModel);
        }

        /// <summary>
        /// 比賽報名-新增一列
        /// </summary>
        /// <param name="block">The block.</param>
        /// <returns></returns>
        [PartialCheck]
        [AllowAnonymous]
        public ActionResult GetCompetitionRow(OrderEditBlock block)
        {
            if (!Request.IsAjaxRequest())
            {
                return ErrorPage(); //404
            }
            if (SessionManager.UserID == Guid.Empty)
            {
                return new ContentResult { Content = $"LogOutError" };
            }

            if (block == OrderEditBlock.UnitList)
            {
                return View(ViewName("Order", "_Competition_UnitList_row"), new erp_OrderUnit());
            }

            if (block == OrderEditBlock.TeamMember)
            {
                return View(ViewName("Order", "_Competition_TeamMember_row"), new mgt_UserProfile());
            }

            if (block == OrderEditBlock.OrderItem)
            {
                return View(ViewName("Order", "_Competition_OrderItem_row"), new EditOrderDetail());
            }

            return ErrorPage(); //404
        }

        /// <summary>
        /// 比賽報名-post (新增/編輯)
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="verifyType">驗證類型</param>
        /// <param name="verifystr">驗證字串/param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<ActionResult> EditCompetition(EditOrderViewModel model, VerifyType verifyType = VerifyType.None, string verifystr = "")
        {
            if (SessionManager.UserID == Guid.Empty)
            {
                return new ContentResult { Content = $"LogOutError" };
            }

            //驗證碼
            //if (captchaRequired && (string.IsNullOrWhiteSpace(captcha) || SessionManager.Captcha != captcha))
            //{
            //    var result = new CiResult { Message = SystemMessage.CaptchaError };
            //    return Json(result);
            //}
            //SessionManager.Captcha = null;

            //驗證電話          
            if (verifyType == VerifyType.PhoneNumber)
            {
                var checkUser = userService.GetbyPhone(verifystr);
                if (checkUser?.ID != SessionManager.UserID)
                {
                    var result = new CiResult { Message = "電話驗證錯誤" };
                    return Json(result);
                }
            }

            if (model.OrderViewModel.Order.ID == Guid.Empty)
            {
                //create
                if (model.OrderViewModel.OrderDetails != null)
                {
                    //項目拆開-每個項目一個團隊
                    var newOrderDetails = new List<EditOrderDetail>();
                    foreach (var detail in model.OrderViewModel.OrderDetails)
                    {
                        for (int i = 0; i < detail.Quantity; i++)
                        {
                            newOrderDetails.Add(new EditOrderDetail
                            {
                                ItemID = detail.ItemID,
                                Quantity = 1
                            });
                        }
                    }
                    model.OrderViewModel.OrderDetails = newOrderDetails;
                }

                if (!model.OrderViewModel.OrderDetails.Any())
                {
                    return Json(new CiResult { Message = "未選擇隊數" });//return error
                }

                return await OrderCreate(model.OrderViewModel);
            }
            else
            {
                //save
                if (model.Block == OrderEditBlock.OrderItem)
                {
                    if (model.OrderViewModel.OrderDetails != null)
                    {
                        foreach (var detail in model.OrderViewModel.OrderDetails)
                        {
                            if (detail.DetailMemberID == null)
                            {
                                return Json(new CiResult { Message = "未選擇選手" });//return error
                            }
                            //數量=選手數量
                            detail.Quantity = detail.DetailMemberID.Count();
                        }
                    }
                }

                return await OrderUpdate(model, sendMail: false);
            }
        }

        /// <summary>
        /// 變更狀態
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="toSuccessView">是否有成功頁</param>
        /// <param name="captchaRequired">是否需要驗證碼</param>
        /// <param name="captcha">The captcha.</param>
        /// <returns></returns>
        public async Task<ActionResult> ChangeStatus(Guid id, OrderStatus status, bool toSuccessView = false, bool captchaRequired = false, string captcha = "")
        {
            var result = new CiResult();

            //驗證碼
            if (captchaRequired && (string.IsNullOrWhiteSpace(captcha) || SessionManager.Captcha != captcha))
            {
                result.Message = SystemMessage.CaptchaError;
            }
            SessionManager.Captcha = null;

            //訂單
            var order = service.Get(id);

            var model = new OrderViewModel
            {
                User = new mgt_User
                {
                    ID = SessionManager.UserID
                },
                Order = new erp_Order
                {
                    ID = id,
                    OrderStatus = (int)status
                }
            };

            // 訂購商品前檢查
            var preCheck = service.ItemPreCheckWithID(SessionManager.UserID, order.ItemID.Value, checkStock: false, checkUser: false);
            if (!preCheck.IsSuccess)
            {
                result.Message = preCheck.Message;
            }

            //save
            if (string.IsNullOrEmpty(result.Message))
            {
                result = service.ChangeStatus(model, AccountType.Member);
            }

            //show message
            if (!result.IsSuccess)
            {
                SetAlertMessage(result.Message, AlertType.error);
            }
            else
            {
                SetAlertMessage(result.Message, AlertType.success);
            }

            //通知信
            if (result.IsSuccess && status != OrderStatus.Delete)
            {
                if (result.IsSuccess)
                {
                    await SendOrderMail(id, order.StructureID, (SystemMailType)status, SessionManager.UserID, fromFn: $"ChangeStatus[{id}]");
                }
            }

            //return         
            if (result.IsSuccess && status == OrderStatus.Delete)
            {
                //列表頁
                return RedirectToAction("Order", "Member", new { type = order.StructureID });
            }
            else if (result.IsSuccess && toSuccessView)
            {
                //成功頁
                ClearSlertMessage();
                return Success(id);
            }
            else
            {
                //編輯/瀏覽頁
                return RedirectToAction("Competition", new { id });
            }
        }

        public async Task<ActionResult> ChangeDetailStatus(Guid detailid, OrderStatus status)
        {
            var result = new CiResult();

            //訂單
            var orderDetail = service.GetDetail(detailid);

            // 訂購商品前檢查
            var preCheck = service.ItemPreCheckWithID(SessionManager.UserID, orderDetail.ItemID, checkStock: false, checkUser: false);
            if (!preCheck.IsSuccess)
            {
                result.Message = preCheck.Message;
            }

            //save
            if (string.IsNullOrEmpty(result.Message))
            {
                result = service.ChangeDetailStatus(detailid, status, AccountType.Member);
            }

            //show message
            if (!result.IsSuccess)
            {
                SetAlertMessage(result.Message, AlertType.error);
            }
            else
            {
                SetAlertMessage(result.Message, AlertType.success);
            }

            //通知信
            if (result.IsSuccess && status != OrderStatus.Delete)
            {
                if (result.IsSuccess)
                {
                    //通知選手參賽
                    var mailType = status == OrderStatus.TeamEditDone ? SystemMailType.MemberEnter : (SystemMailType)status;
                    await SendOrderDetailMail(detailid, orderDetail.erp_Order.StructureID, mailType);
                }
            }

            //return         
            //if (result.IsSuccess && status == OrderStatus.Delete)
            //{
            //    //列表頁
            //    return RedirectToAction("Order", "Member", new { type = order.StructureID });
            //}
            //else if (result.IsSuccess && toSuccessView)
            //{
            //    //成功頁
            //    ClearSlertMessage();
            //    return Success(id);
            //}
            //else
            //{
            //編輯/瀏覽頁
            return RedirectToAction("Competition", new { id = orderDetail.OrderID });
            // }
        }


        /// <summary>
        /// 完成頁
        /// </summary>
        /// <param name="id">OrderID</param>
        /// <returns></returns>
        public ActionResult Success(Guid id)
        {
            //匯款資訊
            var mailService = new MailService(ApplicationHelper.ClientID);
            var dataOrder = service.GetView(id, ApplicationHelper.DefaultLanguage, DataLevel.Simple);
            ViewBag.OrderATMInfo = mailService.CreateOrderATMInfo(dataOrder);

            return View(ViewName("Order", "Success"), id);
        }

        #endregion

        /// <summary>
        /// 編輯超過期限-放棄名額
        /// </summary>
        [AllowAnonymous]
        public ActionResult OverEditTime(string name, bool isTest = false)
        {
            if (name == "xnet")
            {
                var result = service.OverEditDeadline(isTest);
                return Content(_Json.ModelToJson(result));
            }
            else
            {
                return ErrorPage(); //404
            }
        }

        /// <summary>
        /// 釋放名額(已放棄->作廢)
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ReleaseSaleCount(string name, int hh, int mm, bool isTest = false)
        {
            if (name == "xnet" && hh >= 0 && hh <= 23 && mm >= 0 && mm <= 59)
            {
                var result = service.ReleaseSaleCount(hh, mm, isTest);
                return Content(_Json.ModelToJson(result));
            }
            else
            {
                return ErrorPage(); //404
            }
        }

        #region 付款界接

        #region ibon
        /// <summary>
        /// ibon test
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        //[Big5Charset]
        public ActionResult TestIbon()
        {
            return View();
        }

        /// <summary>
        /// 安源ibon資料交換
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [ValidateInput(false)] //allow 具有潛在危險 Request.Form 的值  
        [Big5Charset]
        [HttpPost]
        public ActionResult GetIbonData(string XMLData)
        {
            _Log.CreateText("GetIbonData: " + XMLData);

            var getData = _Xml.XmlToModel<IbonSendData>(XMLData);
            var showData = new IbonShowData();

            try
            {
                showData = new IbonShowData(getData);
                showData.STATUS_CODE = "0000";
                showData.STATUS_DESC = "成功";

                //todo 查詢訂單
                //明細
                if (showData.KEY1 == "8812345671234567"
                 || showData.KEY1 == "88123456123456")
                {
                    showData.BUSINESS = "079002"; //資料交換業者回覆
                    showData.TOTALAMOUNT = 30; //總金額
                    showData.TOTALCOUNT = 1; //總筆數(不包含第一列的 TITLE 資料)

                    //明細:第一列固定為Title
                    showData.LISTDATAs.Add(new IbonListData()
                    {
                        SERIALNO = "00",
                        PRINT = "Y",
                        CP_ORDER = "廠商訂單編號",
                        DATA_1 = "繳費內容",
                        DATA_2 = "用戶號碼",
                        DATA_3 = "用戶名稱",
                        DATA_4 = "小計",
                        DATA_5 = "備註",
                        DATA_6 = "",
                        DATA_7 = "",
                        DATA_8 = ""
                    });

                    showData.LISTDATAs.Add(new IbonListData()
                    {
                        SERIALNO = "01",
                        PRINT = "Y",
                        CP_ORDER = showData.KEY1,//廠商訂單編號
                        DATA_1 = "2019滑輪溜冰報名測試",//繳費內容
                        DATA_2 = "0911222333",//用戶號碼
                        DATA_3 = "陳先生",//用戶名稱
                        DATA_4 = "30",//繳費金額
                        DATA_5 = "包含手續費30元",//說明
                        DATA_6 = "",
                        DATA_7 = "",
                        DATA_8 = ""
                    });
                }
                else if (showData.KEY1 == "8812345679999888")
                {
                    showData.BUSINESS = "079002"; //資料交換業者回覆
                    showData.TOTALAMOUNT = 30000; //總金額
                    showData.TOTALCOUNT = 2; //總筆數(不包含第一列的 TITLE 資料)

                    //明細:第一列固定為Title
                    showData.LISTDATAs.Add(new IbonListData()
                    {
                        SERIALNO = "00",
                        PRINT = "Y",
                        CP_ORDER = "廠商訂單編號",
                        DATA_1 = "繳費內容",
                        DATA_2 = "用戶號碼",
                        DATA_3 = "用戶名稱",
                        DATA_4 = "繳費金額",
                        DATA_5 = "備註",
                        DATA_6 = "資料",
                        DATA_7 = "",
                        DATA_8 = ""
                    });

                    showData.LISTDATAs.Add(new IbonListData()
                    {
                        SERIALNO = "01",
                        PRINT = "Y",
                        CP_ORDER = showData.KEY1,//廠商訂單編號
                        DATA_1 = "2019滑輪溜冰報名測試",//繳費內容
                        DATA_2 = "0933444555",//用戶號碼
                        DATA_3 = "林每每",//用戶名稱
                        DATA_4 = "20000",//繳費金額
                        DATA_5 = "包含手續費30元",//說明
                        DATA_6 = "第一段",
                        DATA_7 = "",
                        DATA_8 = ""
                    });

                    showData.LISTDATAs.Add(new IbonListData()
                    {
                        SERIALNO = "02",
                        PRINT = "Y",
                        CP_ORDER = showData.KEY1,//廠商訂單編號
                        DATA_1 = "2019滑輪溜冰報名測試",//繳費內容
                        DATA_2 = "0933444555",//用戶號碼
                        DATA_3 = "林每每",//用戶名稱
                        DATA_4 = "10000",//繳費金額
                        DATA_5 = "",//說明
                        DATA_6 = "第二段",
                        DATA_7 = "",
                        DATA_8 = ""
                    });

                }
                else if (showData.KEY1 == "8824181227000003")
                {
                    showData.BUSINESS = "079002"; //資料交換業者回覆
                    showData.TOTALAMOUNT = 20010; //總金額
                    showData.TOTALCOUNT = 1; //總筆數(不包含第一列的 TITLE 資料)

                    //明細:第一列固定為Title
                    showData.LISTDATAs.Add(new IbonListData()
                    {
                        SERIALNO = "00",
                        PRINT = "Y",
                        CP_ORDER = "廠商訂單編號",
                        DATA_1 = "繳費內容",
                        DATA_2 = "用戶號碼",
                        DATA_3 = "用戶名稱",
                        DATA_4 = "小計",
                        DATA_5 = "備註",
                        DATA_6 = "",
                        DATA_7 = "",
                        DATA_8 = ""
                    });

                    showData.LISTDATAs.Add(new IbonListData()
                    {
                        SERIALNO = "01",
                        PRINT = "Y",
                        CP_ORDER = showData.KEY1,//廠商訂單編號
                        DATA_1 = "2019滑輪溜冰報名測試",//繳費內容
                        DATA_2 = "0955334444",//用戶號碼
                        DATA_3 = "潘潘",//用戶名稱
                        DATA_4 = "20010",//繳費金額
                        DATA_5 = "包含手續費30元",//說明
                        DATA_6 = "",
                        DATA_7 = "",
                        DATA_8 = ""
                    });
                }
                else
                {
                    showData.STATUS_CODE = "1003";
                    showData.STATUS_DESC = "查無訂單編號";

                    showData.BUSINESS = "079002"; //資料交換業者回覆
                    showData.TOTALAMOUNT = 0; //總金額
                    showData.TOTALCOUNT = 0; //總筆數(不包含第一列的 TITLE 資料)
                }

            }
            catch (Exception)
            {
                showData.STATUS_CODE = "1003";
                showData.STATUS_DESC = "資料格式錯誤";
            }

            return View(showData);
        }

        /// <summary>
        /// 安源ibon資料交換
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [ValidateInput(false)] //allow 具有潛在危險 Request.Form 的值  
        [Big5Charset]
        [HttpPost]
        public ActionResult IbonPay(string XMLData)
        {
            _Log.CreateText("IbonPay: " + XMLData);

            var showData = new IbonPayReturn();

            try
            {
                var getData = _Xml.XmlToModel<IbonPayMoney>(XMLData);

                showData.STATUS_CODE = "0000";
                showData.STATUS_DESC = "成功";
                showData.CONFIRM = "OK";
                showData.SHOPID = getData.SHOPID;
                showData.DETAIL_NUM = getData.DETAIL_NUM;
            }
            catch (Exception)
            {
                showData.STATUS_CODE = "1003";
                showData.STATUS_DESC = "資料格式錯誤";
                showData.CONFIRM = "FAIL";
            }

            return View(showData);
        }

        #endregion

        #region 國泰
        /// <summary>
        /// Tests the cathey.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult TestCathey()
        {
            return View();
        }

        /// <summary>
        /// Cathays the pay from test.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [ValidateInput(false)]
        [HttpPost]
        public async Task<ActionResult> CathayPayFromTest()
        {
            _Log.CreateText("CathayPay From Test");
            return await CathayPay();
        }

        /// <summary>
        /// 國泰虛擬帳號即時入帳通知
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [ValidateInput(false)] //allow 具有潛在危險 Request.Form 的值       
        [HttpPost]
        public async Task<ActionResult> CathayPay()
        {
            var result = new CiResult<List<erp_Order>>();
            try
            {
                foreach (string key in Request.Form.Keys)
                {
                    //key=  <?xml version
                    //Request.Form[key]=   "1.0" encoding="big5"?><MYB2B><HEADER><TXNO>
                    var message = key + "=" + Request.Form[key];
                    _Log.CreateText("CathayPay: " + message);

                    //save message                 
                    result = service.CathyPayMessage(message);

                    //通知信
                    var mailService = new MailService(ApplicationHelper.ClientID);
                    if (result.IsSuccess)
                    {
                        foreach (var order in result.Data)
                        {
                            await SendOrderMail(order.ID, order.StructureID, (SystemMailType)order.OrderStatus, order.CreateUser, fromFn: $"CathayPay");
                        }
                    }

                    if (result.IsSuccess)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _Log.CreateText("CathayPay error" + _Json.ModelToJson(ex));
            }


            if (result.IsSuccess)
            {
                return Content("0000");
            }
            else
            {
                return Content("");
            }
        }

        #endregion

        #endregion

        #region (共用private)

        private async Task<ActionResult> OrderCreate(OrderViewModel model, HttpPostedFileBase file = null)
        {
            var result = new CiResult<OrderStatus>();
            var item = itemService.Get(model.Order.ItemID.Value);

            // 訂購商品前檢查      
            var preCheck = service.ItemPreCheck(SessionManager.UserID, item, checkUser: false);
            if (!preCheck.IsSuccess)
            {
                return ErrorPage(); //404
            }

            // detail check (進service再判斷)
            //var detailItems = model.OrderDetails?.Select(x => x.ItemID).Where(x => x != model.Order.ItemID.Value).Distinct();
            //if (detailItems != null)
            //{
            //    foreach (var itemID in detailItems)
            //    {
            //        int quantity = model.OrderDetails.Where(x => x.ItemID == itemID).Sum(x => x.Quantity);
            //        preCheck = service.ItemPreCheckWithID(SessionManager.UserID, itemID, quantity);
            //        if (!preCheck.IsSuccess)
            //        {
            //            result.Message = preCheck.Message;
            //            break;
            //        }
            //    }
            //}


            if (string.IsNullOrEmpty(result.Message))
            {
                //set Data
                if (model.User == null) model.User = new mgt_User();
                model.User.ID = SessionManager.UserID;
                model.Order.StructureID = item.StructureID;

                //fileUpload (滑輪用)
                //if (file != null)
                //{
                //    var fileFolder = UploadTool.GetFileFolder(ApplicationHelper.SystemName, SourceType.MemberOrder);
                //    var uploadResult = UploadTool.FileUpload(file, FileType.Images, fileFolder);
                //    if (!uploadResult.IsSuccess)
                //    {
                //        result.Message = uploadResult.Message;
                //    }
                //    else
                //    {
                //        model.Order.FilePath = uploadResult.Data.FilePath;
                //    }
                //}
            }

            if (string.IsNullOrEmpty(result.Message))
            {
                result = service.Create(model, ApplicationHelper.DefaultLanguage);
            }

            //通知信
            var mailService = new MailService(ApplicationHelper.ClientID);
            if (result.IsSuccess)
            {
                SessionManager.TempOrderID = result.ID;
                await SendOrderMail(result.ID, model.Order.StructureID, (SystemMailType)result.Data, SessionManager.UserID, fromFn: $"OrderCreate");
            }

            return Json(result);
        }

        private async Task<ActionResult> OrderUpdate(EditOrderViewModel model, HttpPostedFileBase file = null, bool sendMail = true)
        {
            var result = new CiResult<OrderStatus>();
            var item = itemService.Get(model.OrderViewModel.Order.ItemID.Value);

            // 訂購商品前檢查 (Admin不檢查)   
            var preCheck = new CiResult();
            if (SessionManager.AccountType == AccountType.Member)
            {
                preCheck = service.ItemPreCheck(SessionManager.UserID, item, checkStock: false);
                if (!preCheck.IsSuccess)
                {
                    return ErrorPage(); //404
                }
            }
            model.IsAdmin = (SessionManager.AccountType == AccountType.Admin);

            //set Data
            if (model.OrderViewModel.User == null) model.OrderViewModel.User = new mgt_User();
            model.OrderViewModel.User.ID = SessionManager.UserID;
            model.OrderViewModel.Order.StructureID = item.StructureID;

            //fileUpload (image)
            if (file != null)
            {
                var fileFolder = UploadTool.GetFileFolder(ApplicationHelper.SystemName, SourceType.MemberOrder);
                var uploadResult = UploadTool.FileUpload(file, FileType.Images, fileFolder);
                if (!uploadResult.IsSuccess)
                {
                    result.Message = uploadResult.Message;
                }
                else
                {
                    model.OrderViewModel.Order.FilePath = uploadResult.Data.FilePath;
                }
            }
            //fileUpload (audio)
            if (model.OrderViewModel.OrderDetails != null)
            {
                var fileFolder = UploadTool.GetFileFolder(ApplicationHelper.SystemName, SourceType.MemberOrder);
                foreach (var detail in model.OrderViewModel.OrderDetails)
                {
                    if (detail.FileUpload != null)
                    {
                        var uploadResult = UploadTool.FileUpload(detail.FileUpload, FileType.Audio, fileFolder);
                        if (!uploadResult.IsSuccess)
                        {
                            result.Message = uploadResult.Message;
                        }
                        else
                        {
                            detail.FilePath = uploadResult.Data.FilePath;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(result.Message))
            {
                result = service.Update(model, ApplicationHelper.DefaultLanguage);
            }

            //通知信 (no use?)
            if (sendMail)
            {
                var mailService = new MailService(ApplicationHelper.ClientID);
                if (result.IsSuccess)
                {
                    SessionManager.TempOrderID = result.ID;
                    await SendOrderMail(result.ID, model.OrderViewModel.Order.StructureID, (SystemMailType)result.Data, SessionManager.UserID);
                }
            }

            return Json(result);
        }

        private async Task SendOrderMail(Guid orderID, Guid structureID, SystemMailType mailType, Guid userID, string fromFn = "")
        {
            var mailService = new MailService(ApplicationHelper.ClientID);

            //存在emailTemplate才寄信-失敗不提醒
            if (mailService.IsExistTemplate(mailType, structureID))
            {
                try
                {
                    var user = userService.Get(userID);
                    var dataOrder = service.GetView(orderID, ApplicationHelper.DefaultLanguage, DataLevel.Simple);

                    var subject = "";
                    try
                    {
                        subject = dataOrder.ParentItemViewModel?.ItemLanguage.Subject ?? "";
                        if (!string.IsNullOrEmpty(subject))
                        {
                            subject += "-";
                        }
                        subject += dataOrder.ItemViewModel?.ItemLanguage.Subject;
                    }
                    catch (Exception)
                    {
                    }

                    var mailCoutent = new ReplaceMailContent
                    {
                        UserName = user.Name,
                        UserEmail = user.Email,
                        OrderContent = mailService.CreateOrderContent(dataOrder),
                        OrderATMInfo = mailService.CreateOrderATMInfo(dataOrder),
                        OrderAdminNote = dataOrder.Order.PublicNote,
                        CompetitionCame = subject
                    };
                    var mailResult = await mailService.SendEmail(userID, mailCoutent, mailType, structureID: structureID, fromFn: fromFn);
                }
                catch (Exception e)
                {
                    var json = _Json.ModelToJson(e);
                    _Log.CreateText(json);
                }

            }

        }

        private async Task SendOrderDetailMail(Guid orderDetailID, Guid structureID, SystemMailType mailType)
        {
            var mailService = new MailService(ApplicationHelper.ClientID);

            //存在emailTemplate才寄信-失敗不提醒
            if (mailService.IsExistTemplate(mailType, structureID))
            {
                try
                {
                    // var user = userService.Get(userID);
                    var dataOrder = service.GetDetail(orderDetailID);
                    var memberResult = userService.SendMemberEmails(orderDetailID);

                    //盃-區-組
                    var order = service.GetView(dataOrder.OrderID, ApplicationHelper.DefaultLanguage, DataLevel.Simple);
                    var subject = order.ParentItemViewModel?.ItemLanguage.Subject ?? "";
                    if (!string.IsNullOrEmpty(subject))
                    {
                        subject += "-";
                    }
                    subject += order.ItemViewModel?.ItemLanguage.Subject + " " + dataOrder.ItemSubject;

                    if (memberResult.IsSuccess)
                    {
                        //所有家長
                        foreach (var member in memberResult.Data)
                        {
                            var user = member.mgt_UserCreate;

                            if (user.ID == SessionManager.UserID)
                            {
                                //_Log.CreateText($"[Email no send]通知參賽: User={member.ID}, Member={member.NickName}");
                            }
                            else
                            {
                                var mailCoutent = new ReplaceMailContent
                                {
                                    UserName = user.Name,
                                    UserEmail = user.Email,
                                    MemberName = member.NickName,
                                    CompetitionCame = subject
                                };
                                var mailResult = await mailService.SendEmail(user.ID, mailCoutent, mailType, structureID: structureID, fromFn: "SendOrderDetailMail");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    var json = _Json.ModelToJson(e);
                    _Log.CreateText(json);
                }

            }

        }
        #endregion

    }
}