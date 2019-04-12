using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebMaker.Entity.Models
{

    [MetadataType(typeof(mgt_RoleMetaData))]
    public partial class mgt_Role
    {
        public partial class mgt_RoleMetaData
        {
            /// <summary>
            /// 角色ID
            /// </summary>
            [Required]
            public Guid ID { get; set; }

            /// <summary>
            /// 客戶ID
            /// </summary>
            [Required]
            public Guid ClientID { get; set; }

            /// <summary>
            /// 系統編號
            /// </summary>
            [StringLength(25, ErrorMessage = "欄位長度不得大於 25 個字元")]
            [Required]
            public string SystemNumber { get; set; }

            /// <summary>
            /// 角色名稱
            /// </summary>
            [StringLength(50, ErrorMessage = "不得大於50個字元")]
            [Required]
            [Display(Name = "權限名稱")]
            public string Name { get; set; }

            [Required]
            [DisplayName("會員層級")] //0=預設
            public int MemberLevel { get; set; }
                 
            [DisplayName("會員包含欄位")]
            public string UserContentTypes { get; set; }

            [DisplayName("會員必填欄位")]
            public string UserRequiredTypes { get; set; }

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
