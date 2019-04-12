using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MyTool.ViewModels
{
    /// <summary>
    /// 多層下拉選單 (連動)
    /// </summary>
    /// <seealso cref="System.Web.Mvc.SelectListItem" />
    public class SelectOptionModel: SelectListItem
    {
        /// <summary>
        /// 子選項
        /// </summary>
        public List<SelectOptionModel> SubSelect { get; set; }

        /// <summary>
        /// 其他屬性
        /// </summary>
        public List<KeyValuePair<string, string>> Attributes { get; set; }
    }
}
