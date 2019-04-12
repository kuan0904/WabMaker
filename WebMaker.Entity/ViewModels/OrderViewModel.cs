using MyTool.Enums;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebMaker.Entity.Models;

namespace WebMaker.Entity.ViewModels
{
    /*
     * 訂單與訂單明細: Member下單時產生
     * 可有對應文章 Order.ItemID
     * 訂單結構Order.StructureID = 訂單文章結構Item.StructureID     *
     * 活動報名只有一筆Detail (=主文章Item)
     */
    public class OrderViewModel : UserViewModel //更新User個資
    {
        public erp_Order Order { get; set; }
        public List<EditOrderDetail> OrderDetails { get; set; }
        public List<erp_OrderDiscount> OrderDiscounts { get; set; } //if OrderDetailID = null: 全訂單折扣
        public List<erp_OrderLog> OrderLogs { get; set; }
        //public Guid UserID { get; set; } //使用 model.User.ID 

        //訂單文章
        public ItemViewModel ItemViewModel { get; set; }
        public ItemViewModel ParentItemViewModel { get; set; }

        //單位-比賽報名用
        public List<erp_OrderUnit> Units { get; set; }
        //下拉選單 List<SelectOptionModel>
        public string UnitsSelectJson { get; set; }

        //團隊所有成員-比賽報名用
        public List<mgt_UserProfile> TeamMembers { get; set; }
        //下拉選單 List<SelectOptionModel> (放在最外面View的script, 因為new row無預設資料)
        public string TeamMemberSelectJson { get; set; }

        ////比賽項目
        //public List<ItemViewModel> SubItemViewModels { get; set; }
        //比賽項目、組別 下拉選單 List<SelectOptionModel>
        public string SubItemSelectJson { get; set; }
        public List<ItemProductModel> SubItemViewModel { get; set; }

        //新增角色 Order:RoleID
        public List<SelectListItem> RoleSelectList { get; set; }
        //public bool IsCreateRole { get; set; }
        public mgt_UserRoleRelation NewRoleRelation { get; set; }

        //可併入的訂單
        public List<SelectListItem> CombinOrderSelectList { get; set; }

        public OrderViewModel() { }
        public OrderViewModel(UserViewModel baseData)
        {
            this.Order = new erp_Order();
            this.User = baseData.User;
            this.UserProfile = baseData.UserProfile;
            this.RoleRelations = baseData.RoleRelations;
            this.UserContentTypes = baseData.UserContentTypes;
            this.UserRequiredTypes = baseData.UserRequiredTypes;
            this.RoleCheckList = baseData.RoleCheckList;
            this.RoleIDs = baseData.RoleIDs;
            this.UserExternalLogin = baseData.UserExternalLogin;
            this.LoginType = baseData.LoginType;
        }
    }

    /// <summary>
    /// 訂單明細
    /// </summary>
    public class EditOrderDetail : erp_OrderDetail
    {
        public EditOrderDetail() { }
        public EditOrderDetail(erp_OrderDetail baseData)
        {
            this.ID = baseData.ID;
            this.OrderID = baseData.OrderID;
            this.ItemID = baseData.ItemID;
            this.ItemSubject = baseData.ItemSubject;
            this.Option = baseData.Option;
            this.DetailTeamName = baseData.DetailTeamName;
            this.FilePath = baseData.FilePath;
            this.SalePrice = baseData.SalePrice;
            this.Quantity = baseData.Quantity;
            this.OrderStatus = baseData.OrderStatus;
            this.CombineOriOrderID = baseData.CombineOriOrderID;

            this.erp_OrderCombineOri = baseData.erp_OrderCombineOri;
        }
        //訂單完成加入身分
        // public Guid? OrderAutoRole { get; set; }

        //主辦縣市(顯示用)
        public string DepartmentName { get; set; }

        //團隊出賽成員-比賽報名用
        public List<mgt_UserProfile> DetailMembers { get; set; }
        public List<Guid> DetailMemberID { get; set; }

        //上傳檔案
        public HttpPostedFileBase FileUpload { get; set; }
    }

    /// <summary>
    /// 訂單編輯 全
    /// </summary>
    public class EditOrderViewModel
    {
        public OrderViewModel OrderViewModel { get; set; }

        //區塊
        public OrderEditBlock Block { get; set; }

        //瀏覽或編輯
        public bool IsEditing { get; set; }

        //通過check (日期有效)
        public bool IsCheckSuccess { get; set; }

        //管理員編輯
        public bool IsAdmin { get; set; }

        //指定編輯detail
        public Guid OrderDetailID { get; set; }

        //指派來的選手
        public List<mgt_UserProfile> TeamMemberAssigns { get; set; }
    }

    public enum OrderEditBlock
    {
        All,

        //基本資料
        BasicInfo,

        //單位
        UnitList,

        //明細的單位
        DetailUnit,

        //團隊成員
        TeamMember,

        //明細的成員
        DetailMember,

        //比賽項目與組別
        OrderItem,
    }

    /// <summary>
    /// 訂單篩選
    /// </summary>
    public class OrderFilter
    {
        /// <summary>
        /// 語系 for 訂單文章
        /// </summary>
        public LanguageType LangType { get; set; }

        /// <summary>
        /// 結構
        /// </summary>    
        public Guid StructureID { get; set; }

        /// <summary>
        /// 單位
        /// </summary>  
        public List<Guid> DepartmentIDs { get; set; }

        /// <summary>
        /// 搜尋字串
        /// </summary>
        public string SearchString { get; set; }

        /// <summary>
        /// 訂單文章
        /// </summary>
        public Guid? ArticleID { get; set; }
        public Guid? GroupArticleID { get; set; }

        /// <summary>
        /// 建立者
        /// </summary>
        public Guid? CreateUser { get; set; }

        /// <summary>
        /// 前端or後端
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// 篩選狀態
        /// </summary>
        public List<OrderStatus> SelectOrderStatus { get; set; }
        public List<OrderStatus> SelectOrderDetailStatus { get; set; }

        /// <summary>
        /// 篩選付款方式
        /// </summary>  
        public PayType? PayType { get; set; }

        /// <summary>
        /// 排序方式
        /// </summary>
        public OrderTeamSort OrderTeamSort { get; set; }

        /// <summary>
        /// 篩選自動入帳
        /// </summary>       
        public bool SelectAutoPay { get; set; }

        /// <summary>
        /// 排程查詢後入帳
        /// </summary>
        public bool IsByQuery { get; set; }

        /// <summary>
        /// 篩選異常
        /// </summary>
        public bool IsWrong { get; set; }

        /// <summary>
        /// 顯示個資
        /// </summary> 
        public bool DisplayProfile { get; set; }

        /// <summary>
        /// 開始範圍 
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 結束範圍 
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    /// <summary>
    /// OrderList
    /// </summary>
    public class OrderListModel
    {
        /// <summary>
        /// 類型
        /// </summary>
        public cms_Structure Structure { get; set; }

        /// <summary>
        /// 分頁參數
        /// </summary>
        public OrderPageModel OrderPageModel { get; set; }
    }

    /// <summary>
    /// OrderList分頁
    /// </summary>
    public class OrderPageResult
    {
        /// <summary>
        /// 結果資料
        /// </summary>
        public PageModel<OrderViewModel> DataResult { get; set; }

        /// <summary>
        /// 分頁搜尋參數
        /// </summary>
        public OrderPageModel OrderPageModel { get; set; }
    }

    /// <summary>
    /// Order分頁參數 (ajax不可有object結構)
    /// </summary> 
    public class OrderPageModel : PageParameter
    {
        public Guid StructureID { get; set; }
    }

    /// <summary>
    /// 狀態統計
    /// </summary>
    public class StatusModel
    {
        /// <summary>
        /// 狀態
        /// </summary>
        public OrderStatus OrderStatus { get; set; }

        /// <summary>
        /// 數量總計
        /// </summary>
        public int TotalCount { get; set; }
    }
}
