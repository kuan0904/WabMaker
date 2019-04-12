using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WabMaker.Web.WebViewModels
{
    /// <summary>
    /// 回傳分頁資料
    /// </summary>
    public class PageWebModel //for Api 不做按鈕
    {     
        /// <summary>
        /// 現在頁數
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 資料筆數
        /// </summary>   
        public int TotalCount { get; set; }

        /// <summary>
        /// 總頁數
        /// </summary>     
        public int PageCount { get; set; }

        /// <summary>
        /// 現在資料顯示範圍-起始
        /// </summary>
        public int DataStart { get; set; }

        /// <summary>
        /// 現在資料顯示範圍-結束
        /// </summary>
        public int DataEnd { get; set; }

        ///// <summary>
        ///// 分頁類型
        ///// </summary>
        //public PageType PageType { get; set; }

        ///// <summary>
        ///// 按鈕最大數量
        ///// </summary>
        //public int ButtonMaxSize { get; set; }

        ///// <summary>
        ///// 按鈕顯示
        ///// </summary>
        //public string PreviousText { get; set; }
        //public string NextText { get; set; }
        //public string FirstText { get; set; }
        //public string LastText { get; set; }
    }

    public class PageWebModel<T> : PageWebModel
    {
        /// <summary>
        /// 本頁資料
        /// </summary>    
        public List<T> Data { get; set; }
    }
    
}