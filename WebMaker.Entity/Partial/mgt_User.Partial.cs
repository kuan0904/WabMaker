using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebMaker.Entity.Models
{
    [DataContract]
    [MetadataType(typeof(mgt_UserMetaData))]
    public partial class mgt_User
    {
        public partial class mgt_UserMetaData
        {
            [Required]
            [DataMember]
            public Guid ID { get; set; }

            [DataMember]
            public Guid? UserProfileID { get; set; }

            [Required]
            [DataMember]
            public Guid ClientID { get; set; }

            /// <summary>
            /// 系統編號
            /// </summary>
            [StringLength(25, ErrorMessage = "欄位長度不得大於 25 個字元")]
            [Required]
            [DataMember]
            public string SystemNumber { get; set; }

            /// <summary>
            /// 部門ID
            /// </summary>     
            [DisplayName("單位")]
            [DataMember]
            public Guid? DepartmentID { get; set; }

            /// <summary>
            /// 帳號
            /// </summary>
            [StringLength(200, ErrorMessage = "不得大於200個字元")]           
            [DisplayName("帳號")]
            [DataMember]
            public string Account { get; set; }

            /// <summary>
            /// 密碼
            /// </summary> 
            [DataType(DataType.Password)]
            [DisplayName("密碼")]
            [DataMember]
            public string Password { get; set; }

            /// <summary>
            /// 姓名
            /// </summary>
            [StringLength(50, ErrorMessage = "不得大於50個字元")]
            [DisplayName("姓名")]
            [DataMember]
            public string Name { get; set; }
            
            /// <summary>
            /// 信箱
            /// </summary>
            [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
            [DataMember]
            public string Email { get; set; }

            /// <summary>
            /// 信箱驗證
            /// </summary>
            [DisplayName("信箱驗證")]
            [DataMember]
            public bool EmailIsVerify { get; set; }

            /// <summary>
            /// 是否接收電子報
            /// </summary>
            [DisplayName("訂閱電子報")]
            [DataMember]
            public bool IsReceiveEpaper { get; set; }

            /// <summary>
            /// 手機
            /// </summary>           
            [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
            [DisplayName("手機")]
            [DataMember]
            public string Phone { get; set; }

            /// <summary>
            /// 手機驗證
            /// </summary>
            [DisplayName("手機驗證")]
            [DataMember]
            public bool PhoneIsVerify { get; set; }

            /// <summary>
            /// 郵遞區號
            /// </summary>
            [StringLength(5, ErrorMessage = "欄位長度不得大於 5 個字元")]
            [DisplayName("郵遞區號")]
            [DataMember]
            public string ZipCodeOption { get; set; }

            /// <summary>
            /// 地址
            /// </summary>           
            [StringLength(500, ErrorMessage = "欄位長度不得大於 500 個字元")]
            [DisplayName("地址")]
            [DataMember]
            public string Address { get; set; }

            /// <summary>
            /// 備註
            /// </summary>
            [DisplayName("備註")]
            [DataMember]
            public string Note { get; set; }

            /// <summary>
            /// 帳號類型
            /// </summary>         
            [Required]
            [DisplayName("帳號類型")]
            [DataMember]
            public int AccountType { get; set; }

            /// <summary>
            /// 登入類型
            /// </summary>
            [DisplayName("登入方式")]
            [DataMember]
            public string LoginTypes { get; set; }

            /// <summary>
            /// 使用者狀態
            /// </summary>    
            [DisplayName("狀態")]
            [DataMember]
            public int UserStatus { get; set; }

            /// <summary>
            /// 上次登入時間
            /// </summary>
            [DisplayName("上次登入時間")]
            [DataMember]
            public DateTime? LastLoginTime { get; set; }

            /// <summary>
            /// 上次登入IP
            /// </summary>
            [DisplayName("上次登入IP")]
            [DataMember]
            public string LastLoginIP { get; set; }

            [Required]
            [DataMember]
            public int Sort { get; set; }

            [Required]
            [DisplayName("建立時間")]
            [DataMember]
            public DateTime CreateTime { get; set; }

            [Required]
            [DisplayName("更新時間")]
            [DataMember]
            public DateTime UpdateTime { get; set; }

        }
    }

}
