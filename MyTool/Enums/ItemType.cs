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
    /// 內容組成欄位(多選) 000~999
    /// </summary>
    public enum ContentType
    {
        //------基本欄位------

        [Display(Name = "標題")]
        Subject = 1,

        [Display(Name = "標題多行")]
        SubjectMulti = 2,

        [Display(Name = "封面")]//主圖
        Cover = 5,

        [Display(Name = "內頁圖")] //次圖
        SubImage = 6,


        [Display(Name = "文字描述")]//純文字Area
        Description = 10,

        [Display(Name = "內容編輯")]//ckeditor
        Content = 11,

        [Display(Name = "自訂格式")]//[相關連結名稱](http://example.com)
        CustomFormat = 12,

        [Display(Name = "樣板內容")]//<h2 class="template" placeholder="(標題1)"></h2>
        Template = 13,


        [Display(Name = "網址連結")]
        LinkUrl = 29,

        [Display(Name = "日期")]
        Date = 30,

        [Display(Name = "時間範圍")]
        DateRange = 31,

        [Display(Name = "作者")]
        Author = 35,

        [Display(Name = "關鍵字")]
        Keywords = 36,


        [Display(Name = "電話")]
        Phone = 100,

        [Display(Name = "地點")]
        Address = 110,

        [Display(Name = "地點描述")]
        AddressInfo = 111,

        //------------

        [Display(Name = "路由名稱")]
        RouteName = 199,

        [Display(Name = "是否啟用")] //edit必存在
        IsEnabled = 200,

        [Display(Name = "是否置頂")]
        IsTop = 201,

        [Display(Name = "自訂排序")]
        Sort = 202,


        [Display(Name = "分類")] //列表顯示用
        Category = 210,

        [Display(Name = "觀看次數")]
        ViewCount = 211,

        [Display(Name = "編輯者")] //列表顯示用
        UpdateUser = 220,

        //------商品欄位------

        [Display(Name = "定價")]
        OriginalPrice = 300,

        [Display(Name = "售價")]
        SalePrice = 301,

        [Display(Name = "售價(每人)")] //同時check售價
        SalePricePerPerson = 302,

        [Display(Name = "價格描述")]
        DiscountText = 307,

        [Display(Name = "販售數量/優惠達數量")]//(訂單優惠-優惠達數量)
        StockCount = 310,

        [Display(Name = "銷售量")]
        SaleCount = 311,


        [Display(Name = "販售時間")]
        SaleDateRange = 320,

        [Display(Name = "販售時間加小時")]//(code條件)
        SaleDateHourRange = 321,

        [Display(Name = "計價方式")]
        PriceType = 335,

        [Display(Name = "定義折扣類型")]
        DiscountType = 340,

        [Display(Name = "人數限制")] //比賽報名
        PeopleRange = 350,

        [Display(Name = "銷售限制")]
        SaleLimit = 352,

        [Display(Name = "日期限制")]
        DateLimit = 355,


        [Display(Name = "訂單允許身分")]
        OrderAllowRole = 390,

        [Display(Name = "訂單完成身分")] //完成後新增身分
        OrderCreateRole = 391,

        [Display(Name = "手動新增身分")] //訂單完成身分-只做欄位檢查用, 儲存時管理員手動選擇身分
        OrderAutoRole_Not = 392,

        [Display(Name = "防止重複報名")] //(code條件)
        OrderNoRepeatMember = 400,

        //------比賽------

        [Display(Name = "單位部門(權限層級)")]
        Department = 500,

        [Display(Name = "自訂選項")]
        Options = 501,
    }

    /// <summary>
    /// 必填欄位(多選) 
    /// </summary>
    public enum ContentRequiredType
    {
        [Display(Name = "標題")]
        Subject = ContentType.Subject,

        [Display(Name = "封面")]//無作用
        Cover = ContentType.Cover,

        //[Display(Name = "文字描述")]
        //Description = ContentType.Description,

        //[Display(Name = "內容編輯")]
        //Content = ContentType.Content,

        [Display(Name = "網址連結")]
        LinkUrl = ContentType.LinkUrl,

        [Display(Name = "日期")]
        Date = ContentType.Date,

        [Display(Name = "時間範圍")]
        DateRange = ContentType.DateRange,


        [Display(Name = "售價")]
        SalePrice = ContentType.SalePrice,

        [Display(Name = "販售數量")]
        StockCount = ContentType.StockCount,

        [Display(Name = "販售時間範圍")]
        SaleDateRange = ContentType.SaleDateRange,

        [Display(Name = "自訂選項")]
        Options = ContentType.Options
    }

    /// <summary>
    /// 標籤類型
    /// </summary>
    public enum TagType
    {
        [Display(Name = "關鍵字")]
        Keywords = ContentType.Keywords,

        [Display(Name = "作者")]
        Author = ContentType.Author,
    }


    /// <summary>
    /// 內容組成工具(多選)
    /// </summary>
    public enum ItemToolType
    {

        //[Display(Name = "分享工具")] //前台FB、Twitter...
        //ShareTool = 3,

        //[Display(Name = "圖片編輯")]
        //ImageEdit = 4,

        //[Display(Name = "預覽")]
        //Preview = 5,
    }

    /// <summary>
    /// 項目關聯類型 (父層關聯類型=父層數量)
    /// </summary>
    public enum ItemRelationType
    {
        [Display(Name = "無")]
        None = 0,

        [Display(Name = "1")] //單選
        One = 1,

        [Display(Name = "多")] //多選,只能選最後一層
        Many = 2,

        [Display(Name = "多層")] //多選,可選所有層級分類
        MultiMany = 3,

        [Display(Name = "群組")] //文章群組下拉選單
        Group = 10,
    }

    /// <summary>
    /// 項目類型(多選)
    /// </summary>
    public enum ItemType
    {
        // Item add IsMenu
        //[Display(Name = "選單")] 
        //Menu = 1,

        /*
         API搜尋關鍵字: 包含Article, 排除Banner、Category
         */

        [Display(Name = "Banner")] //Partial圖文 (不計viewCount)
        Banner = 4,

        [Display(Name = "分類")] //選單
        Category = 5,

        [Display(Name = "文章")] //單頁圖文
        Article = 6,

        [Display(Name = "文章檔案")]
        Files = 7,

        [Display(Name = "分類擴充")] //選單擴充,不可單頁開啟(ex:分類加背景圖)
        CategoryExpand = 10,

        [Display(Name = "次分類")] //列表時,跳過一層分類 顯示下一層文章
        CategorySub = 12,

        [Display(Name = "文章群組")] //不出現在分類管理
        ArticleGroup = 20,


        [Display(Name = "訂單")] //(會員活動) 課程報名、賽事申請、賽事報名
        Order = 50,

        [Display(Name = "訂單項目")] //商品、比賽項目
        OrderItem = 55,

        [Display(Name = "訂單優惠")]
        OrderDiscount = 70,

        //--------其他--------
        [Display(Name = "Q8人才庫")]
        Q8People = 100,
    }

    /// <summary>
    /// 權限層級 //直接用DepartmentID
    /// </summary>
    //public enum AuthorityLevelType
    //{
    //    [Display(Name = "所有人")]
    //    All = 0,

    //    [Display(Name = "單位")] //同部門以上
    //    Department = 10,

    //    //[Display(Name = "個人")]
    //    //Personal = 20,
    //}
}
