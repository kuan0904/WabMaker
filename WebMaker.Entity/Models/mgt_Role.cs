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
    
    public partial class mgt_Role
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public mgt_Role()
        {
            this.mgt_RoleMenuRelation = new HashSet<mgt_RoleMenuRelation>();
            this.cms_ItemOrderRoleRelation = new HashSet<cms_ItemOrderRoleRelation>();
            this.mgt_UserRoleRelation = new HashSet<mgt_UserRoleRelation>();
        }
    
        public System.Guid ID { get; set; }
        public System.Guid ClientID { get; set; }
        public string SystemNumber { get; set; }
        public string Name { get; set; }
        public int MemberLevel { get; set; }
        public int AccountType { get; set; }
        public string UserContentTypes { get; set; }
        public string UserRequiredTypes { get; set; }
        public int Sort { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDelete { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime UpdateTime { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mgt_RoleMenuRelation> mgt_RoleMenuRelation { get; set; }
        public virtual mgt_Client mgt_Client { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cms_ItemOrderRoleRelation> cms_ItemOrderRoleRelation { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mgt_UserRoleRelation> mgt_UserRoleRelation { get; set; }
    }
}