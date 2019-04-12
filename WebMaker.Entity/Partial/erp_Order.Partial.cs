using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace WebMaker.Entity.Models
{
    [MetadataType(typeof(erp_OrderMetaData))]
    public partial class erp_Order
    {
        public partial class erp_OrderMetaData
        {
            [Required]
            public Guid ID { get; set; }

            [Required]
            public Guid ClientID { get; set; }

            [Required]
            public Guid StructureID { get; set; }

            //訂單文章
            public Guid? ItemID { get; set; }

            [StringLength(12, ErrorMessage = "欄位長度不得大於 12 個字元")]
            [Required]
            [Display(Name = "訂單編號")]
            public string OrderNumber { get; set; }

            [Required]
            [Display(Name = "付款方式")]
            public int PayType { get; set; }

            [StringLength(100, ErrorMessage = "欄位長度不得大於 100 個字元")]
            [Display(Name = "匯款資訊")]
            public string PayInfo { get; set; }

            [Required]
            [Display(Name = "配送方式")]
            public int DeliveryType { get; set; }

            [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
            [Display(Name = "收貨人姓名")]
            public string ReceiverName { get; set; }

            [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
            [Display(Name = "收貨人電話")]
            public string ReceiverPhone { get; set; }

            [StringLength(500, ErrorMessage = "欄位長度不得大於 500 個字元")]
            [Display(Name = "收貨人地址")]
            public string ReceiverAddres { get; set; }

            [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
            [Display(Name = "收貨人Email")]
            public string ReceiverEmail { get; set; }


            [StringLength(100, ErrorMessage = "欄位長度不得大於 200 個字元")]
            [Display(Name = "團隊名稱")]
            public string TeamName { get; set; }
            
            [StringLength(100, ErrorMessage = "欄位長度不得大於 200 個字元")]
            [Display(Name = "教練")]
            public string Coach { get; set; }

            [StringLength(100, ErrorMessage = "欄位長度不得大於 200 個字元")]
            [Display(Name = "領隊")]
            public string Leader { get; set; }

            [StringLength(100, ErrorMessage = "欄位長度不得大於 200 個字元")]
            [Display(Name = "管理")]
            public string Manager { get; set; }


            [StringLength(500, ErrorMessage = "欄位長度不得大於 500 個字元")]
            [Display(Name = "備註")]
            public string OrderNote { get; set; }

            //[Display(Name = "編號")]//證號
            //public string RoleNumber { get; set; }

            //[Display(Name = "角色ID")]
            //public Guid? RoleID { get; set; }

            [Display(Name = "證明上傳")]
            public string FilePath { get; set; }


            [StringLength(500, ErrorMessage = "欄位長度不得大於 500 個字元")]
            [Display(Name = "管理員備註")]
            public string PublicNote { get; set; }

            [StringLength(500, ErrorMessage = "欄位長度不得大於 500 個字元")]
            [Display(Name = "管理員私人備註")]
            public string PrivateNote { get; set; }

            [Required]
            [Display(Name = "小計")]
            public decimal DetailPrice { get; set; }

            [Required]
            [Display(Name = "運費")]
            public decimal ShippingFee { get; set; }

            [Required]
            [Display(Name = "總金額")]
            public decimal TotalPrice { get; set; }

            [Display(Name = "狀態")]
            [Required]
            public int OrderStatus { get; set; }

            [Required]
            [Display(Name = "建立日期")]
            public DateTime CreateTime { get; set; }

            [Required]
            [Display(Name = "姓名")]
            public Guid CreateUser { get; set; }


            [Display(Name = "虛擬帳號")]
            public string VirtualAccount { get; set; }
                     
            public DateTime? VirtualCreateTime { get; set; }

            [Display(Name = "繳費期限")]
            public DateTime? PayDeadline { get; set; }


            public Guid? CombineOrderID { get; set; }
        }
    }

}
