using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebMaker.Entity.Models
{
    [MetadataType(typeof(cms_ItemMetaData))]
    public partial class cms_Item
    {
        public partial class cms_ItemMetaData
        {
            [Required]
            public Guid ID { get; set; }
             
            [Required]
            public Guid ClientID { get; set; }

            [Display(Name = "單位")]
            public Guid? DepartmentID { get; set; }

            [StringLength(25, ErrorMessage = "長度不得大於25個字元")]
            [Required]
            public string SystemNumber { get; set; }
                      
            [Required]
            [Display(Name = "結構類型")]
            public Guid StructureID { get; set; }
            
            [Required]
            [Display(Name = "路由名稱")]
            public string RouteName { get; set; }

            [Required]
            [Display(Name = "是否置頂")]
            public bool IsTop { get; set; }
                       
            [Display(Name = "選單板位")]
            public string MenuPositions { get; set; }


            [Display(Name = "日期")]
            public DateTime? Date { get; set; }

            [Display(Name = "開始時間")]
            public DateTime? StartTime { get; set; }

            [Display(Name = "結束時間")]
            public DateTime? EndTime { get; set; }


            [Display(Name = "定價")]
            public decimal OriginalPrice { get; set; }

            [Display(Name = "金額")] //售價,目前使用整數.ToString("0")  比賽項目以人計費
            public decimal SalePrice { get; set; }

            [Display(Name = "販售數量")]//(優惠達數量)
            public int StockCount { get; set; }

            [Display(Name = "銷售量")]
            public int SaleCount { get; set; }

            [Display(Name = "販售/報名開始")]
            public DateTime? SaleStartTime { get; set; }

            [Display(Name = "販售/報名結束")]
            public DateTime? SaleEndTime { get; set; }

            [Display(Name = "計價方式")]
            public int PriceType { get; set; }

            [Display(Name = "定義折扣類型")]
            public int DiscountType { get; set; }

            [Display(Name = "人數最小限制")]
            public int PeopleMin { get; set; }

            [Display(Name = "人數最大限制")]
            public int PeopleMax { get; set; }

            [Display(Name = "銷售限制")]
            public int SaleLimit { get; set; }

            [Display(Name = "日期限制")]
            public DateTime? DateLimit { get; set; }

            //[Display(Name = "訂單完成加入身分")]
            //public Guid? OrderAutoRole { get; set; }

            //[Display(Name = "身分期限(年)")]
            //public int? RoleTimeLimitYear { get; set; }

            [Display(Name = "選項")]
            public string Options { get; set; }

            [Required]
            [Display(Name = "排序")]
            public int Sort { get; set; }
            
            [Required]
            public bool IsDelete { get; set; }
            
            [Required]
            public DateTime CreateTime { get; set; }
            
            [Required]
            public DateTime UpdateTime { get; set; }
            
            [Required]
            [Display(Name = "編輯者")]
            public Guid UpdateUser { get; set; }

            [Required]
            public Guid CreateUser { get; set; }

            [Display(Name = "觀看次數")]
            public int ViewCount { get; set; }

        }

    }

}
