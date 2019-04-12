using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Enums
{
    /// <summary>
    /// 訂單流程包括 
    /// </summary>
    public enum OrderStatus
    {
        //--------會員---------

        [Display(Name = "編輯中")] //暫存
        Editing = 10,

        //--------管理員---------

        [Display(Name = "待確認")] //訂單成立 11
        New = SystemMailType.OrderNew,

        [Display(Name = "待付款")] //20
        NonPayment = SystemMailType.OrderNonPayment,

        [Display(Name = "付款過期")]
        OverduePayment = 23,

        [Display(Name = "處理中")] //已付款 30
        Processing = SystemMailType.OrderProcessing,

        [Display(Name = "已出貨")] //31
        Shipment = SystemMailType.OrderShipment,

        //申請退貨
        //退貨完成

        //--------會員---------
        [Display(Name = "未完成")] //付款後編輯團隊
        TeamEdit = SystemMailType.OrderTeamEdit, //50

        [Display(Name = "編輯待確認")]
        TeamEditConfirm = 54,

        [Display(Name = "編輯完成")]
        TeamEditDone = 55,

        [Display(Name = "退回編輯")] // 70 可修改
        Refuse = SystemMailType.OrderRefuse,

        //-------不可編輯--------

        [Display(Name = "已放棄")]
        Abandon = 80,

        [Display(Name = "已合併")] //90
        Combine = 90,

        [Display(Name = "取消")] //99
        Cancel = SystemMailType.OrderCancel,

        [Display(Name = "完成")] //100
        Done = SystemMailType.OrderDone,

        [Display(Name = "截止未完成")]
        NotDone = 105,

        [Display(Name = "作廢")] //110
        Invalid = 110,

        [Display(Name = "刪除")] //不儲存(Only編輯時), 直接刪除Order
        Delete = 120,

        [Display(Name = "管理員編輯")] //只記在log
        AdminEdit = 150,

        [Display(Name = "超賣作廢")] //不算完成
        OverSoldFail = 300
    }

    /*配送方式會影響付款方式*/

    /// <summary>
    /// 付費方式
    /// </summary>
    public enum PayType
    {
        [Display(Name = "無")]
        None = 0,

        [Display(Name = "轉帳匯款")]
        ATM = 1,

        [Display(Name = "轉帳匯款(國泰)")] //虛擬帳號
        ATMVirtual = 2,

        [Display(Name = "ibon付款")]
        ibon = 20,

        [Display(Name = "線上刷卡")]
        CreditCard = 30,

        [Display(Name = "貨到付款")]
        CashOnDelivery = 40,

        [Display(Name = "現場付款")]
        CashOnSpot = 50,
    }

    /// <summary>
    /// 配送方式
    /// </summary>
    public enum DeliveryType
    {
        [Display(Name = "無")]
        None = 0,

        [Display(Name = "宅配")]
        Home = 1,

        [Display(Name = "7-11取貨")]
        StorePickup_711 = 10,

        [Display(Name = "自取")]
        Personally = 50,
    }

    /// <summary>
    /// todo 發票類型
    /// </summary>
    public enum InvoiceType
    {
        [Display(Name = "無")]
        None = 0,

        [Display(Name = "二聯式")]
        Two = 2,

        [Display(Name = "三聯式")]
        Three = 3
    }

    /// <summary>
    /// 訂單欄位包含
    /// </summary>
    public enum OrderContentType
    {
        [Display(Name = "以明細為主顯示")]
        Detail = 1,

        [Display(Name = "Admin公開備註")]
        PublicNote = 10,

        [Display(Name = "Admin私人備註")]
        PrivateNote = 11,

        [Display(Name = "折扣")]
        Discount = 50,

        //------角色------
        [Display(Name = "角色_上傳證明")]
        FileUpload = 100,

        [Display(Name = "角色_編號")]
        RoleNumber = 101,

        //[Display(Name = "角色_有期限")]
        //IsTimeLimited = 102,


        //------比賽------
        [Display(Name = "比賽_團隊名稱")]
        TeamName = 200,

        [Display(Name = "比賽_教練")]
        Coach = 201,

        [Display(Name = "比賽_領隊")]
        Leader = 202,

        [Display(Name = "比賽_管理")]
        Manager = 203,


        [Display(Name = "比賽項目_選手")]
        TeamMembers = 250,

        [Display(Name = "比賽項目_組別")]
        DetailOption = 251,

        [Display(Name = "比賽項目_檔案")]
        DetailFileUpload = 252,

        [Display(Name = "比賽項目_主辦縣市")]
        DetailDepartment = 253,

        [Display(Name = "比賽項目_團體隊名")]
        DetailTeamName = 254,


        [Display(Name = "訂單編輯期限")]
        EditDeadline = 300,

        [Display(Name = "單位在明細中")]
        UnitInOrderDetail = 350,
    }

    /// <summary>
    /// 訂單錯誤返回頁面
    /// </summary>
    public enum OrderErrorReturnPage
    {
        [Display(Name = "首頁")]
        Index = 0,

        [Display(Name = "Item內頁")]
        ItemDetail = 1,

        [Display(Name = "會員首頁")]
        MemberIndex = 2,

        [Display(Name = "上層Item內頁")]
        ParentItemDetail = 3,

    }

    /// <summary>
    /// 訂單與角色關聯類型
    /// </summary>
    public enum ItemOrderRoleType
    {
        //訂單允許身分
        OrderAllowRole = ContentType.OrderAllowRole,//390

        //訂單完成身分
        OrderCreateRole = ContentType.OrderCreateRole,//391

        //允許管理身分(權限範圍) AuthorityLevelType
        AuthorityLevelRole = 900,
    }

    /// <summary>
    /// 計價方式
    /// </summary>
    public enum PriceType
    {
        [Display(Name = "每人")]
        Person = 1, // 以人頭算數量

        [Display(Name = "每隊")]
        Group = 2, // 以團體算數量
    }

    /// <summary>
    /// 包含折扣類型
    /// </summary>
    public enum DiscountType
    {
        [Display(Name = "團體價限制")]
        [Description("項目單價以｛條件數量｝個計費")]//訂單明細直接使用此-數量
        GroupPrice = 100,

        // (ex: 每位競速選手800元可參加一個項目，每增加一項目加收200元)
        [Display(Name = "選手多件優惠")]　//多條件時,以條件數量大的優先
        [Description("同位選手參加｛條件數量｝個以上項目,每增加一下加收｛優惠價｝元")]//可跨項目   
        MemberMultiple = 110,
    }

    /// <summary>
    /// 篩選比賽結果統計
    /// </summary>
    public enum OrderTeamSelect
    {
        [Display(Name = "分組名單")]
        OrderTeam,

        [Display(Name = "單位清單")]
        CompetitionUnits,

        [Display(Name = "項目清單")]
        CompetitionItems,

        [Display(Name = "選手清單")]
        CompetitionMembers
    }

    public enum OrderTeamSort
    {
        [Display(Name = "依項目排序")]
        Item,

        [Display(Name = "依選手排序")]
        Member,

        [Display(Name = "依建立時間")]
        CreateTime,
    }

    /// <summary>
    /// 報表樣式
    /// </summary>
    public enum ReportStyle
    {
        [Display(Name = "style1")] //合併
        Mearge,

        [Display(Name = "style2")] //展開
        Join,

        [Display(Name = "style3")] //Json
        Json,
    }

    /// <summary>
    /// 驗證方式
    /// </summary>
    public enum VerifyType
    {
        None,
        Captcha,
        PhoneNumber
    }
}
