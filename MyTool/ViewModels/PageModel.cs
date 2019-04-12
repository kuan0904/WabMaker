using MyTool.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.ViewModels
{
    /// <summary>
    /// 分頁參數
    /// </summary>
    public class PageParameter
    {
        public PageParameter()
        {
            this.IsPaged = true;
            this.CurrentPage = 1;
            this.PageSize = 10;
            this.IsDescending = true;
            this.DataLevel = DataLevel.Simple;
        }

        /// <summary>
        /// 是否分頁
        /// </summary>  
        public bool IsPaged { get; set; }

        /// <summary>
        /// 現在頁數
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 每頁顯示筆數
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 排序欄位
        /// </summary>
        public SortColumn SortColumn { get; set; }

        /// <summary>
        /// 排序欄位方向
        /// </summary>
        public bool IsDescending { get; set; }

        /// <summary>
        /// 資料完整度
        /// </summary>
        public DataLevel DataLevel { get; set; }
    }

    /// <summary>
    /// 回傳分頁資料
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageModel
    {
        /// <summary>
        /// 現在頁數
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 每頁顯示筆數
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 資料筆數
        /// </summary>   
        public int TotalCount { get; set; }

        /// <summary>
        /// 總頁數
        /// </summary>     
        public int PageCount { get; set; }

        /// <summary>
        /// 現在資料顯示範圍: DataStart 到 DataEnd 筆
        /// </summary>
        public int DataStart { get; set; }
        public int DataEnd { get; set; }

        /// <summary>
        /// 金額總計
        /// </summary>
        public decimal PriceSum { get; set; }
    }

    public class PageModel<T> : PageModel
    {
        /// <summary>
        /// 本頁資料
        /// </summary>    
        public List<T> Data { get; set; }
    }


    /// <summary>
    /// 回傳資料與分頁
    /// </summary>
    public class PageResult<T>
    {
        public PageModel<T> DataResult { get; set; }

        public PageParameter PageModel { get; set; }
    }

    /// <summary>
    /// 按鈕(在前端運作)
    /// </summary>  
    public class PageButtonModel : PageModel
    {
        public PageButtonModel(PageModel baseData)
        {
            this.CurrentPage = baseData.CurrentPage;
            this.TotalCount = baseData.TotalCount;
            this.PageCount = baseData.PageCount;
            this.DataStart = baseData.DataStart;
            this.DataEnd = baseData.DataEnd;

            PageType = PageType.Number_PreNextAuto;
            ButtonMaxSize = 5;

            PreviousText = "<";
            NextText = ">";
            FirstText = "First";
            LastText = "Last";
        }

        /// <summary>
        /// 分頁類型
        /// </summary>
        public PageType PageType { get; set; }

        /// <summary>
        /// 按鈕最大數量
        /// </summary>
        public int ButtonMaxSize { get; set; }

        /// <summary>
        /// 按鈕顯示
        /// </summary>
        public string PreviousText { get; set; }
        public string NextText { get; set; }
        public string FirstText { get; set; }
        public string LastText { get; set; }
    }

    public class PageButtonResultModel
    {
        /// <summary>
        /// 名稱
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 顯示
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 頁數
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 是否是現在頁碼
        /// </summary>
        public bool IsCurrent { get; set; }

    }

    /// <summary>
    /// 分頁類型
    /// </summary>
    public enum PageType
    {
        /// <summary>
        /// 只顯示頁碼
        /// </summary>
        Number,

        /// <summary>
        /// 只顯下一頁
        /// </summary>
        Next,

        /// <summary>
        /// 只顯示上一頁下一頁
        /// </summary>
        PreNext,

        /// <summary>
        /// 頁碼、上一頁下一頁
        /// </summary>
        Number_PreNext,

        /// <summary>
        /// 頁碼、上一頁下一頁自動顯示或隱藏
        /// </summary>
        Number_PreNextAuto,

        /// <summary>
        /// 頁碼、上一頁下一頁、第一頁最後頁
        /// </summary>
        All,
    }

    /// <summary>
    /// 排序欄位
    /// </summary>
    public enum SortColumn
    {
        Date,
        Sort,
        CreateTime,
        UpdateTime,
        ViewCount,
        Random,
        Custom
    }

    /// <summary>
    /// 搜尋類型
    /// </summary>
    public enum SearchType
    {
        all,
        tag
    }
}
