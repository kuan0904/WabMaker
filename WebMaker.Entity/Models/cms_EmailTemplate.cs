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
    
    public partial class cms_EmailTemplate
    {
        public System.Guid ID { get; set; }
        public System.Guid ClientID { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string TemplateBcc { get; set; }
        public int SystemMailType { get; set; }
        public Nullable<System.Guid> StructureID { get; set; }
        public int Sort { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDelete { get; set; }
        public System.DateTime UpdateTime { get; set; }
        public Nullable<System.Guid> UpdateUser { get; set; }
        public string SMSContent { get; set; }
        public bool SMSIsEnabled { get; set; }
    
        public virtual cms_Structure cms_Structure { get; set; }
        public virtual mgt_Client mgt_Client { get; set; }
        public virtual mgt_User mgt_User { get; set; }
    }
}
