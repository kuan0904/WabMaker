//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebMaker.Entity.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class cms_Item
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public cms_Item()
        {
            this.cms_ItemFile = new HashSet<cms_ItemFile>();
            this.cms_ItemLanguage = new HashSet<cms_ItemLanguage>();
            this.ChildItemRelations = new HashSet<cms_ItemRelation>();
            this.ParentItemRelations = new HashSet<cms_ItemRelation>();
            this.mgt_UserProfile = new HashSet<mgt_UserProfile>();
            this.cms_ItemOrderRoleRelation = new HashSet<cms_ItemOrderRoleRelation>();
            this.erp_OrderDiscount = new HashSet<erp_OrderDiscount>();
            this.erp_Order = new HashSet<erp_Order>();
            this.erp_OrderDetail = new HashSet<erp_OrderDetail>();
        }
    
        public System.Guid ID { get; set; }
        public System.Guid ClientID { get; set; }
        public Nullable<System.Guid> DepartmentID { get; set; }
        public string SystemNumber { get; set; }
        public System.Guid StructureID { get; set; }
        public string RouteName { get; set; }
        public bool IsTop { get; set; }
        public string MenuPositions { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal SalePrice { get; set; }
        public int StockCount { get; set; }
        public int SaleCount { get; set; }
        public Nullable<System.DateTime> SaleStartTime { get; set; }
        public Nullable<System.DateTime> SaleEndTime { get; set; }
        public int PeopleMin { get; set; }
        public int PeopleMax { get; set; }
        public string Options { get; set; }
        public int Sort { get; set; }
        public bool IsDelete { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime UpdateTime { get; set; }
        public System.Guid CreateUser { get; set; }
        public System.Guid UpdateUser { get; set; }
        public int ViewCount { get; set; }
        public int DiscountType { get; set; }
        public int SaleLimit { get; set; }
        public Nullable<System.DateTime> DateLimit { get; set; }
        public int PriceType { get; set; }
    
        public virtual mgt_Department mgt_Department { get; set; }
        public virtual mgt_User mgt_UserUpdate { get; set; }
        public virtual mgt_User mgt_UserCreate { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cms_ItemFile> cms_ItemFile { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cms_ItemLanguage> cms_ItemLanguage { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cms_ItemRelation> ChildItemRelations { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cms_ItemRelation> ParentItemRelations { get; set; }
        public virtual cms_Structure cms_Structure { get; set; }
        public virtual mgt_Client mgt_Client { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mgt_UserProfile> mgt_UserProfile { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cms_ItemOrderRoleRelation> cms_ItemOrderRoleRelation { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<erp_OrderDiscount> erp_OrderDiscount { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<erp_Order> erp_Order { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<erp_OrderDetail> erp_OrderDetail { get; set; }
    }
}
