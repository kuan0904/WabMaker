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
    
    public partial class mgt_Department
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public mgt_Department()
        {
            this.ChildDepartments = new HashSet<mgt_Department>();
            this.mgt_User = new HashSet<mgt_User>();
            this.cms_Item = new HashSet<cms_Item>();
        }
    
        public System.Guid ID { get; set; }
        public Nullable<System.Guid> ParentID { get; set; }
        public System.Guid ClientID { get; set; }
        public string SystemNumber { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public int Sort { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDelete { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime UpdateTime { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mgt_Department> ChildDepartments { get; set; }
        public virtual mgt_Department ParentDepartment { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mgt_User> mgt_User { get; set; }
        public virtual mgt_Client mgt_Client { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cms_Item> cms_Item { get; set; }
    }
}
