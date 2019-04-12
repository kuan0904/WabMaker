using MyTool.Enums;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebMaker.Entity.ViewModels;

namespace WabMaker.Web.WebViewModels
{
    /// <summary>
    /// 項目Model
    /// </summary>
    public class ItemWebViewModel
    {
        /// <summary>
        /// 項目主體
        /// </summary>
        public ItemWebModel Item { get; set; }

        /// <summary>
        /// 項目語系
        /// </summary>
        public ItemLanguageWebModel ItemLanguage { get; set; }

        /// <summary>
        /// 檔案
        /// </summary>
        public List<ItemWebFile> ItemFiles { get; set; }

        /// <summary>
        /// 個人簡介
        /// </summary>    
        public UserProfileWebModel UserProfile { get; set; }

        /// <summary>
        /// 麵包屑
        /// </summary>      
        public List<BreadCrumb> BreadCrumbs { get; set; }

        /// <summary>
        /// 分類
        /// </summary>
        public List<BreadCrumb> ParentItems { get; set; }
    }

    /// <summary>
    /// 項目主體
    /// </summary>
    public class ItemWebModel
    {
        public Guid ID { get; set; }
        public Guid StructureID { get; set; }

        /// <summary>
        /// 路由名稱
        /// </summary>
        public string RouteName { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// 開始時間
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 結束時間
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 定價(原價)
        /// </summary>      
        public decimal OriginalPrice { get; set; }

        /// <summary>
        /// 金額
        /// </summary>      
        public decimal SalePrice { get; set; }

        /// <summary>
        /// 販售數量
        /// </summary>     
        public int StockCount { get; set; }

        /// <summary>
        /// 銷售量
        /// </summary>       
        public int SaleCount { get; set; }

        /// <summary>
        /// 販售/報名開始
        /// </summary>     
        public DateTime? SaleStartTime { get; set; }

        /// <summary>
        /// 販售/報名結束
        /// </summary>      
        public DateTime? SaleEndTime { get; set; }
        
        /// <summary>
        /// 定義折扣類型
        /// </summary>
        public int DiscountType { get; set; }

        //[Display(Name = "人數最小限制")]
        //public int PeopleMin { get; set; }

        //[Display(Name = "人數最大限制")]
        //public int PeopleMax { get; set; }

        //public string Options { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 修改時間
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 觀看次數
        /// </summary>      
        public int ViewCount { get; set; }
    }

    /// <summary>
    /// 項目語系
    /// </summary>
    public class ItemLanguageWebModel
    {
        /// <summary>
        /// 標題
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 文字描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 內容編輯
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 樣板內容
        /// </summary>       
        public string TemplateText { get; set; }

        /// <summary>
        /// 自訂格式
        /// </summary>     
        public string CustomFormatText { get; set; }

        /// <summary>
        /// 網址連結
        /// </summary> 
        public string LinkUrl { get; set; }

        /// <summary>
        /// 是否開新視窗
        /// </summary>      
        public bool IsBlankUrl { get; set; }

        /// <summary>
        /// 關鍵字
        /// </summary>     
        public string Keywords { get; set; }

        /// <summary>
        /// 作者
        /// </summary>     
        public string Author { get; set; }

        /// <summary>
        /// 電話
        /// </summary>   
        public string Phone { get; set; }

        /// <summary>
        /// 地點
        /// </summary> 
        public string Address { get; set; }

        /// <summary>
        /// 地點描述
        /// </summary>  
        public string AddressInfo { get; set; }

    }

    /// <summary>
    /// 檔案
    /// </summary>
    public class ItemWebFile
    {
        public Guid? StructureID { get; set; }

        public string Subject { get; set; }

        public string OriName { get; set; }

        public string FilePath { get; set; }

        public string ThumbnailPath { get; set; }

        public FileType FileType { get; set; }

        public SourceType SourceType { get; set; }
    }

    /// <summary>
    /// 個人簡介
    /// </summary> 
    public class UserProfileWebModel
    {
        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string EngName { get; set; }

        public string NickName { get; set; }

        public string AvatarPath { get; set; }

        public string Description { get; set; }

        //public string IdentityCard { get; set; }

        public DateTime? Birthday { get; set; }

        public int Gender { get; set; }

        public int Marriage { get; set; }

        //public string HomePhone { get; set; }

        //public string CompanyPhone { get; set; }

        public string SecondaryEmail { get; set; }

        //public string EmergencyContact { get; set; }

        //public string EmergencyPhone { get; set; }

        public string Unit { get; set; }

        public string UnitAddress { get; set; }

        public string Occupation { get; set; }

        public string Education { get; set; }

        public string School { get; set; }

        public string Skill { get; set; }

        public string Language { get; set; }

        public string SocialNetwork { get; set; }

        //public string Sports { get; set; }

        public int? Height { get; set; }

        public int? Weight { get; set; }

        public string Referrer { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

    }

    /// <summary>
    /// Item列表
    /// </summary>
    public class ItemListModel
    {
        /// <summary>
        /// 本體
        /// </summary>
        public ItemWebViewModel ItemViewModel { get; set; }

        /// <summary>
        /// 分頁參數
        /// </summary>
        public ItemPageModel ItemPageModel { get; set; }

        /// <summary>
        /// 子項目(次分類)
        /// </summary>
        public List<ItemWebViewModel> SubItemList { get; set; }
    }

    /// <summary>
    /// Item列表分頁
    /// </summary>
    public class ItemPageResult
    {
        /// <summary>
        /// 結果資料
        /// </summary>
        public PageModel<ItemWebViewModel> DataResult { get; set; }

        /// <summary>
        /// 分頁搜尋參數
        /// </summary>
        public ItemPageModel ItemPageModel { get; set; }
    }

    /// <summary>
    /// Item分頁參數
    /// </summary> 
    public class ItemPageModel //  繼承 PageParameter (ajax不可有object結構)
    {
        public ItemPageModel()
        {
            this.IsPaged = true;
            this.CurrentPage = 1;
            this.PageSize = 10;
            this.IsDescending = true;
            this.DataLevel = DataLevel.Normal;
        }

        //--- 有routeName: 先取分頁參數---

        /// <summary>
        /// 列表路由名稱 (自動代出 StructureJson、CategoryJson、SortColumn)
        /// </summary>
        public string RouteName { get; set; }

        //---- PageParameter ---

        /// <summary>
        /// 是否分頁 (default: true)
        /// </summary>  
        public bool IsPaged { get; set; }

        /// <summary>
        /// 現在頁數 (default: 1)
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 每頁顯示筆數 (default: 10)
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 排序欄位 (default: 自動代出)
        /// </summary>
        public SortColumn SortColumn { get; set; }

        /// <summary>
        /// 排序欄位方向 (default: true)
        /// </summary>
        public bool IsDescending { get; set; }

        /// <summary>
        /// 資料完整度 (default: Normal)
        /// </summary>
        public DataLevel DataLevel { get; set; }

        //---filter--- 

        /// <summary>
        /// 篩選結構 List Guid (default: 自動代出)
        /// </summary>  
        public string StructureJson { get; set; }

        /// <summary>
        /// 篩選類別 List Guid (default: 自動代出)
        /// </summary>
        public string CategoryJson { get; set; }

        /// <summary>
        /// 排除ID
        /// </summary>
        public Guid? ExceptID { get; set; }

        /// <summary>
        /// 篩選封面類型
        /// </summary> 
        public CoverType? CoverType { get; set; }

        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        public string SearchString { get; set; }

        /// <summary>
        /// 搜尋類型 (default: all)
        /// </summary>
        public SearchType SearchType { get; set; }


        /// <summary>
        /// (MVC view 使用)
        /// </summary>
        public string CacheName { get; set; }

        /// <summary>
        /// (MVC view 使用)
        /// </summary>
        public string ViewName { get; set; }
    }

}