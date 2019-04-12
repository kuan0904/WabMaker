using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace WebMaker.Entity.Models
{
    [MetadataType(typeof(cms_EmailTemplateMetaData))]
    public partial class cms_EmailTemplate
    {
        public partial class cms_EmailTemplateMetaData
        {
            [Required]
            public Guid ID { get; set; }

            [Required]
            public Guid ClientID { get; set; }
            
            [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
            [Required]
            [Display(Name = "標題")]
            public string Subject { get; set; }

            [Display(Name = "內容")]
            public string Content { get; set; }

            [Display(Name = "BCC")]
            [StringLength(4000, ErrorMessage = "欄位長度不得大於 200 個字元")]
            public string TemplateBcc { get; set; }

            [Required]
            [Display(Name = "通知信類型")]
            public int SystemMailType { get; set; }

            [Display(Name = "活動類型")]
            public Guid? StructureID { get; set; }

            [Required]
            public int Sort { get; set; }

            [Required]
            [Display(Name = "Email是否啟用")]
            public bool IsEnabled { get; set; }

            [Required]
            public bool IsDelete { get; set; }

            [Required]
            public DateTime UpdateTime { get; set; }

            public Guid? UpdateUser { get; set; }


            [Display(Name = "簡訊內容")]
            public string SMSContent { get; set; }

            [Display(Name = "簡訊是否啟用")]
            public bool SMSIsEnabled { get; set; }
        }
    }
}
