using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebMaker.Entity.Models
{
    [MetadataType(typeof(mgt_DepartmentMetaData))]
    public partial class mgt_Department
    {
        public partial class mgt_DepartmentMetaData
        {
            /// <summary>
            /// 部門ID
            /// </summary>
            [Required]
            public Guid ID { get; set; }

            public Guid? ParentID { get; set; }

            [Required]
            public Guid ClientID { get; set; }

            /// <summary>
            /// 系統編號
            /// </summary>
            [StringLength(25, ErrorMessage = "欄位長度不得大於 25 個字元")]
            [Required]
            public string SystemNumber { get; set; }

            /// <summary>
            /// 部門名稱
            /// </summary>
            [StringLength(50, ErrorMessage = "不得大於50個字元")]
            [Required]
            [Display(Name = "名稱")]
            public string Name { get; set; }

            /// <summary>
            /// 地址
            /// </summary>
            [StringLength(200, ErrorMessage = "不得大於200個字元")]
            [Display(Name = "地址")]
            public string Address { get; set; }

            /// <summary>
            /// 電話
            /// </summary>
            [StringLength(50, ErrorMessage = "不得大於50個字元")]
            [Display(Name = "電話")]
            public string Phone { get; set; }

            [Required]
            public int Sort { get; set; }

            [Required]
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
