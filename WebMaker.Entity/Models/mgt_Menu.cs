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
    
    public partial class mgt_Menu
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public mgt_Menu()
        {
            this.ChildMenus = new HashSet<mgt_Menu>();
            this.mgt_RoleMenuRelation = new HashSet<mgt_RoleMenuRelation>();
        }
    
        public System.Guid ID { get; set; }
        public Nullable<System.Guid> ParentID { get; set; }
        public int SeqNo { get; set; }
        public string Name { get; set; }
        public int Controller { get; set; }
        public int Action { get; set; }
        public string MenuActions { get; set; }
        public string Url { get; set; }
        public bool IsMenu { get; set; }
        public int Type { get; set; }
        public int Sort { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDelete { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mgt_Menu> ChildMenus { get; set; }
        public virtual mgt_Menu ParentMenu { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mgt_RoleMenuRelation> mgt_RoleMenuRelation { get; set; }
    }
}