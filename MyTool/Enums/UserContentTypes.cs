using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Enums
{
    /// <summary>
    /// 使用者組成欄位(多選) 000~999
    /// </summary>
    public enum UserContentType
    {
        //------User主檔------

        [Display(Name = "姓名")]
        Name = 1,

        [Display(Name = "信箱")] //=帳號
        Email = 2,

        //[Display(Name = "信箱已驗證")]
        //EmailIsVerify = 3,

        [Display(Name = "手機")]
        Phone = 4,

        //[Display(Name = "手機已驗證")]
        //PhoneIsVerify = 5,

        //[Display(Name = "郵遞區號")]
        //ZipCodeOption = 6,

        [Display(Name = "地址")]
        Address = 7,

        //[Display(Name = "備註")]
        //Note = 10,


        //------個人簡介------

        [Display(Name = "姓氏")]
        LastName = 100,

        [Display(Name = "名字")]
        FirstName = 101,

        [Display(Name = "英文姓名")]
        EngName = 102,

        [Display(Name = "綽號")]
        NickName = 103,

        //[Display(Name = "頭像")]
        //AvatarPath = 104,

        //[Display(Name = "描述")]
        //Description = 105,

        [Display(Name = "戶籍地")]
        HouseholdAddress = 190,


        [Display(Name = "身分證或護照")] //預設false=身分證
        IsPassportNumber = 199,

        [Display(Name = "身分證")]
        IdentityCard = 200,

        [Display(Name = "生日")]
        Birthday = 201,

        [Display(Name = "性別")]
        Gender = 202,

        [Display(Name = "婚姻狀態")]
        Marriage = 203,


        [Display(Name = "家用電話")]
        HomePhone = 300,

        [Display(Name = "公司電話")]
        CompanyPhone = 301,

        //[Display(Name = "備用信箱(其他信箱)")]
        //SecondaryEmail = 302,

        [Display(Name = "緊急聯絡人")]
        EmergencyContact = 303,

        [Display(Name = "緊急聯絡人電話")]
        EmergencyPhone = 304,


        [Display(Name = "服務單位")]
        Unit = 390,

        [Display(Name = "服務單位地址")]
        UnitAddress = 391,

        [Display(Name = "職業")]
        Occupation = 401,

        [Display(Name = "學歷")]
        Education = 402,

        [Display(Name = "學校")]
        School = 403,

        [Display(Name = "專長")]
        Skill = 404,

        [Display(Name = "語言")]
        Language = 405,

        [Display(Name = "社群網站")]
        SocialNetwork = 406,


        [Display(Name = "運動項目")]
        Sports = 420,


        [Display(Name = "身高(cm)")]
        Height = 500,

        [Display(Name = "體重(kg)")]
        Weight = 501,

        [Display(Name = "推薦人")]
        Referrer = 600,

    }

    /// <summary>
    /// 必填欄位
    /// </summary>
    public enum UserRequiredType
    {
        [Display(Name = "姓名")]
        Name = UserContentType.Name,

        [Display(Name = "信箱")]
        Email = UserContentType.Email,

        [Display(Name = "手機")]
        Phone = UserContentType.Phone,

        [Display(Name = "地址")]
        Address = UserContentType.Address,


        [Display(Name = "身分證")]
        IdentityCard = UserContentType.IdentityCard,

        [Display(Name = "生日")]
        Birthday = UserContentType.Birthday,

        [Display(Name = "性別")]
        Gender = UserContentType.Gender,

        [Display(Name = "學歷")]
        Education = UserContentType.Education,


        [Display(Name = "緊急聯絡人")]
        EmergencyContact = UserContentType.EmergencyContact,

        [Display(Name = "緊急聯絡人電話")]
        EmergencyPhone = UserContentType.EmergencyPhone,

    }
}