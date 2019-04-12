namespace WebMaker.Entity.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(erp_OrderDetailMetaData))]
    public partial class erp_OrderDetail
    {
    }
    
    public partial class erp_OrderDetailMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
    
        [Required]
        public System.Guid OrderID { get; set; }
    
        [Required]
        public System.Guid ItemID { get; set; }
    
        [StringLength(200, ErrorMessage="欄位長度不得大於 200 個字元")]
        [Required]
        public string ItemSubject { get; set; }

        [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
        public string Option { get; set; }

        [StringLength(100, ErrorMessage = "欄位長度不得大於 100 個字元")]
        public string DetailTeamName { get; set; }

        [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
        public string FilePath { get; set; }

        [Required]
        public decimal SalePrice { get; set; }
    
        [Required]
        public int Quantity { get; set; }
    
        [Required]
        public int OrderStatus { get; set; }

        //合併前原ID
        public Guid? CombineOriOrderID { get; set; }
    }
}
