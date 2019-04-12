using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebMaker.Entity.Models
{
    [MetadataType(typeof(mgt_UserProfileMetaData))]
    public partial class mgt_UserProfile
    {
        [DataContract]
        public partial class mgt_UserProfileMetaData
        {
            [DataMember]
            public Guid ID { get; set; }

            [DataMember]
            public Guid ClientID { get; set; }

            //人物介紹文章
            [DataMember]
            public Guid? ItemID { get; set; }

            //比賽團隊成員
            [DataMember]
            public Guid? OrderID { get; set; }

            //複製的來源
            [DataMember]
            public Guid? FromSourceID { get; set; }

            [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
            [DisplayName("姓氏")]
            [DataMember]
            public string LastName { get; set; }

            [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
            [DisplayName("名字")]
            [DataMember]
            public string FirstName { get; set; }

            [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
            [DisplayName("英文姓名")]
            [DataMember]
            public string EngName { get; set; }

            [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
            [DisplayName("綽號")]
            [DataMember]
            public string NickName { get; set; }

            [StringLength(255, ErrorMessage = "欄位長度不得大於 255 個字元")]
            [DisplayName("頭像")]
            [DataMember]
            public string AvatarPath { get; set; }

            [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
            [DisplayName("描述")]
            [DataMember]
            public string Description { get; set; }


            [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
            [DisplayName("戶籍地")]
            [DataMember]
            public string HouseholdAddress { get; set; }

            //是護照或身分證
            [DisplayName("是護照(或身分證)")]
            [DataMember]
            public bool IsPassportNumber { get; set; }

            [StringLength(100, ErrorMessage = "欄位長度不得大於 100 個字元")]
            [DisplayName("身分證")]
            [DataMember]
            public string IdentityCard { get; set; }

            [DisplayName("生日")]
            [DataMember]
            public DateTime? Birthday { get; set; }
                   
            [DisplayName("性別")]
            [DataMember]
            public int Gender { get; set; }
                     
            [DisplayName("婚姻狀態")]
            [DataMember]
            public int Marriage { get; set; }
                    

            [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
            [DisplayName("家用電話")]
            [DataMember]
            public string HomePhone { get; set; }

            [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
            [DisplayName("公司電話")]
            [DataMember]
            public string CompanyPhone { get; set; }

            [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
            [DisplayName("備用信箱(其他信箱)")]
            [DataMember]
            public string SecondaryEmail { get; set; }

            [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
            [DisplayName("緊急聯絡人")]
            [DataMember]
            public string EmergencyContact { get; set; }

            [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
            [DisplayName("緊急聯絡人電話")]
            [DataMember]
            public string EmergencyPhone { get; set; }

            [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
            [DisplayName("服務單位")]
            [DataMember]
            public string Unit { get; set; }

            [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
            [DisplayName("服務單位簡寫")]
            [DataMember]
            public string UnitShort { get; set; }

            [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
            [DisplayName("服務單位地址")]
            [DataMember]
            public string UnitAddress { get; set; }

            [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
            [DisplayName("職業")]
            [DataMember]
            public string Occupation { get; set; }

            [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
            [DisplayName("學歷")]
            [DataMember]
            public string Education { get; set; }

            [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
            [DisplayName("學校")]
            [DataMember]
            public string School { get; set; }

            [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
            [DisplayName("專長")]
            [DataMember]
            public string Skill { get; set; }

            [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
            [DisplayName("語言")]
            [DataMember]
            public string Language { get; set; }

            [StringLength(1000, ErrorMessage = "欄位長度不得大於 1000 個字元")]
            [DisplayName("社群網站")]
            [DataMember]
            public string SocialNetwork { get; set; }

            [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
            [DisplayName("運動項目")]
            [DataMember]
            public string Sports { get; set; }

            [DisplayName("身高(cm)")]
            [DataMember]
            public int? Height { get; set; }
                     
            [DisplayName("體重(kg)")]
            [DataMember]
            public int? Weight { get; set; }

            [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
            [DisplayName("推薦人")]
            [DataMember]
            public string Referrer { get; set; }


            //訂單內 自訂選手排序 (未使用)
            [DataMember]
            public int Sort { get; set; }

            [DataMember]
            public Guid? CreateUser { get; set; }

            [DataMember]
            public DateTime CreateTime { get; set; }

            [DataMember]
            public DateTime UpdateTime { get; set; }
                  
            [DataMember]
            [DisplayName("是否已刪除")]
            public bool IsDelete { get; set; }

            //是否為常用 (未使用)
            [DataMember]
            public bool IsKeep { get; set; }

            //是家長建立的小孩
            [DataMember]
            public bool IsMyChild { get; set; }

            //簡易編號 for報表產出
            [DataMember]
            public int TempNo { get; set; }
                   
            //單位
            [DataMember]
            public Guid? UnitID { get; set; }

            //是否已寄信
            [DataMember]
            public bool IsSend { get; set; }
        }
    }

}
