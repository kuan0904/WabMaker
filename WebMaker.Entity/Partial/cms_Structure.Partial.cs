using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebMaker.Entity.Models
{
    [MetadataType(typeof(cms_StructureMetaData))]
    public partial class cms_Structure
    {
        public partial class cms_StructureMetaData
        {
            [Required]
            public Guid ID { get; set; }
                  
            // public Guid? ParentID { get; set; }

            [Required]
            public Guid ClientID { get; set; }

            public int SeqNo { get; set; }


            [StringLength(50, ErrorMessage = "不得大於50個字元")]
            [Required]
            [Display(Name = "名稱")]
            public string Name { get; set; }

            [Display(Name = "描述")]
            public string Description { get; set; }

            [StringLength(1000, ErrorMessage = "不得大於1000個字元")]
            [Display(Name = "樣板")]
            public string Template { get; set; }

            [StringLength(1000, ErrorMessage = "不得大於1000個字元")]
            [Display(Name = "自訂格式")]
            public string CustomFormat { get; set; }

            [Required]
            [Display(Name = "主要語系")]//LanguageType
            public int DefaultLanguage { get; set; }
                      
            [Display(Name = "包含語系")]//LanguageType
            public string LanguageTypes { get; set; }

            [Required]
            [Display(Name = "父關聯")]//ItemRelationType
            public int ParentRelationType { get; set; }

            [Required]
            [Display(Name = "子關聯")]//ItemRelationType
            public int ChildRelationType { get; set; }
                       
            [Display(Name = "包含功能")]//ItemToolType
            public string ToolTypes { get; set; }
                       
            [Display(Name = "包含欄位")]//ContentType
            public string ContentTypes { get; set; }
                       
            [Display(Name = "必填欄位")]//ContentType
            public string RequiredTypes { get; set; }
                       
            [Display(Name = "篩選包含欄位")]//ContentType
            public string FilterTypes { get; set; }
                      
            [Display(Name = "列表包含欄位")]//ContentType
            public string ListTypes { get; set; }
                       
            [Display(Name = "語系獨立欄位")]//ContentLanguageType
            public string ContentLanguageTypes { get; set; }

                      
            [Display(Name = "檔案包含類型")]//FileType
            public string FileTypes { get; set; }
                      
            [Display(Name = "檔案包含功能")]//FileToolType
            public string FileToolTypes { get; set; }
            
            [Required]
            [Display(Name = "圖片縮圖尺寸_寬")]
            public int ImageWidth { get; set; }

            [Required]
            [Display(Name = "圖片縮圖尺寸_高")]
            public int ImageHeight { get; set; }


            [Display(Name = "訂單名稱")]
            public string OrderName { get; set; }

            [Display(Name = "包含付費方式")]
            public string PayTypes { get; set; }

            [Display(Name = "包含出貨方式")]
            public string DeliveryTypes { get; set; }

            [Display(Name = "包含計價方式")]
            public string PriceTypes { get; set; }

            [Display(Name = "包含折扣類型")]
            public string DiscountTypes { get; set; }

            [Display(Name = "包含訂單流程")]
            public string OrderStatuses { get; set; }

            [Display(Name = "包含訂單欄位")]
            public string OrderContentTypes { get; set; }

            [Display(Name = "訂單錯誤返回頁面")]
            public int OrderErrorReturnPage { get; set; }


            [Display(Name = "項目類型")]
            public string ItemTypes { get; set; }
            
            [StringLength(20, ErrorMessage = "不得大於20個字")]
            public string ViewName { get; set; }
            
            [Required]
            [Display(Name = "排序")]
            public int Sort { get; set; }

            [Required]
            [Display(Name = "是否啟用")]
            public bool IsEnabled { get; set; }

            [Required]
            public bool IsDelete { get; set; }

            [Required]
            public DateTime CreateTime { get; set; }

            [Required]
            public DateTime UpdateTime { get; set; }


        }
    }
}
