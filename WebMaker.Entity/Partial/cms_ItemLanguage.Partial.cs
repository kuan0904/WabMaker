using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebMaker.Entity.Models
{
    [MetadataType(typeof(cms_ItemLanguageMetaData))]
    public partial class cms_ItemLanguage
    {
        public partial class cms_ItemLanguageMetaData
        {

            [Required]
            public Guid ItemID { get; set; }
            
            [Required]
            public int LanguageType { get; set; }

            [Display(Name = "標題")]
            public string Subject { get; set; }

            [Display(Name = "文字描述")]
            public string Description { get; set; }

            [Display(Name = "內容編輯")]
            public string Content { get; set; }

            [Display(Name = "內容")]
            public string TemplateText { get; set; }

            [Display(Name = "自訂格式")]
            public string CustomFormatText { get; set; }

            [StringLength(300, ErrorMessage = "不得大於300個字元")]
            [Display(Name = "網址連結")]
            public string LinkUrl { get; set; }

            [Display(Name = "是否開新視窗")]            
            public bool IsBlankUrl { get; set; }

            [StringLength(500, ErrorMessage = "不得大於100個字元")]
            [Display(Name = "關鍵字")]
            public string Keywords { get; set; }

            [StringLength(100, ErrorMessage = "不得大於100個字元")]
            [Display(Name = "作者")]
            public string Author { get; set; }
            
            [StringLength(100, ErrorMessage = "不得大於100個字元")]
            [Display(Name = "電話")]
            public string Phone { get; set; }
            
            [StringLength(200, ErrorMessage = "不得大於200個字元")]
            [Display(Name = "地點")]
            public string Address { get; set; }
            
            [StringLength(200, ErrorMessage = "不得大於200個字元")]
            [Display(Name = "地點描述")]
            public string AddressInfo { get; set; }

            [Display(Name = "是否啟用")]
            public bool IsEnabled { get; set; }

        }

    }

}
