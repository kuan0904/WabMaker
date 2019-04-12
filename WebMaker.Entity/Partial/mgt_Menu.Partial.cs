using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebMaker.Entity.Models
{
    [MetadataType(typeof(mgt_MenuMetaData))]
    public partial class mgt_Menu
    {
        public partial class mgt_MenuMetaData
        {
            /// <summary>
            /// 功能單元ID
            /// </summary>    
            [Required]
            public Guid ID { get; set; }
                     
            public Guid? ParentID { get; set; }

            [Required]
            public int SeqNo { get; set; }

            [StringLength(50, ErrorMessage = "大於50個字元")]
            [Required]      
            public string Name { get; set; }

            [Required]
            public int Controller { get; set; }

            [Required]
            public int Action { get; set; }

            /// <summary>
            /// 包含權限(ActionType)
            /// </summary>          
            [DisplayName("包含權限")]
            public string MenuActions { get; set; }

            [StringLength(200, ErrorMessage = "不得大於200個字元")]
            public string Url { get; set; }

            //[Required]
            //public bool IsLink { get; set; }
         
            [Required]
            public bool IsMenu { get; set; }

            [Required]
            public int Sort { get; set; }

            [Required]
            public int Type { get; set; }

            [Required]
            public bool IsEnabled { get; set; }

            [Required]
            public bool IsDelete { get; set; }

        }
    }
}

