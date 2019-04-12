using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WabMaker.Web.WebViewModels
{
    /// <summary>
    /// 樹狀Model (選單)
    /// </summary>
    public class TreeWebViewModel
    {
        /// <summary>
        /// 名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 路由名稱
        /// </summary>
        public string RouteName { get; set; }

        /// <summary>
        /// Url
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// 子節點
        /// </summary>
        public List<TreeWebViewModel> Nodes { get; set; }
    }
}