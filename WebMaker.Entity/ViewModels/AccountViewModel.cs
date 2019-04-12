using MyTool.Enums;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using WebMaker.Entity.Models;

namespace WebMaker.Entity.ViewModels
{
    public class UserSimpleModel
    {
        public mgt_User User { get; set; }

        public mgt_UserProfile UserProfile { get; set; }
    }

    /// <summary>
    /// 帳號與角色列表
    /// </summary>
    public class UserViewModel : UserSimpleModel
    {
        public List<mgt_UserRoleRelation> RoleRelations { get; set; }

        public List<UserContentType> UserContentTypes { get; set; }

        public List<UserRequiredType> UserRequiredTypes { get; set; }

        public List<mgt_UserLog> mgt_UserLogs { get; set; }

        //Department-下拉選單
        public List<SelectListItem> DepartmentSelectList { get; set; }

        //修改管理員帳號角色
        //get
        public List<CheckBoxListItem> RoleCheckList { get; set; }
        //save
        public List<Guid> RoleIDs { get; set; }

        //註冊用
        public mgt_UserExternalLogin UserExternalLogin { get; set; }

        public LoginType LoginType { get; set; }
    }

    /// <summary>
    /// 登入
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "請輸入Email帳號")]
        public string Account { get; set; }

        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "請輸入驗證碼")]
        public string Captcha { get; set; }


        public string IP { get; set; }

        /// <summary>
        /// 帳號類型
        /// </summary>
        public AccountType AccountType { get; set; }

        /// <summary>
        /// 登入方式
        /// </summary>
        public LoginType LoginType { get; set; }
    }

    /// <summary>
    /// 外部登入
    /// </summary>
    public class ExternalLoginViewModel
    {
        public ExternalType ExternalType { get; set; }

        public string ExternalKey { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }
    }

    /// <summary>
    /// 註冊
    /// </summary>
    public class SignUpViewModel : PasswordViewModel
    {
        [Required(ErrorMessage = "請輸入Email帳號")]
        [EmailAddress(ErrorMessage = "請輸入正確的Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "請輸入姓名")]
        public string Name { get; set; }

        [RegularExpression(@"^[0-9\-\+\(\)\#]{7,18}$", ErrorMessage = "請輸入正確的電話")]
        public string Phone { get; set; }

        [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
        public string Address { get; set; }
        //public GenderType Gender { get; set; }

        //public int Birthday_year { get; set; }
        //public int Birthday_month { get; set; }
        //public int Birthday_day { get; set; }

        [Required(ErrorMessage = "請輸入驗證碼")]
        public string Captcha { get; set; }

        /// <summary>
        /// 外部登入
        /// </summary>
        public ExternalType? ExternalType { get; set; }
    }

    /// <summary>
    /// 個人修改密碼
    /// </summary>
    public class UpdatePasswordViewModel : PasswordViewModel
    {
        /// <summary>
        /// 舊密碼
        /// </summary>
        [Required(ErrorMessage = "請輸入舊密碼")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        /// <summary>
        /// 是否有舊密碼 (LoginTypes 包含 Email、Account)
        /// </summary>  
        public bool ExistPassword { get; set; }
    }

    /// <summary>
    /// 重設密碼(忘記密碼)
    /// </summary>
    public class ResetPasswordViewModel : PasswordViewModel
    {
        public Guid UserID { get; set; }
    }

    /// <summary>
    /// 密碼
    /// </summary>
    public class PasswordViewModel
    {
        /// <summary>
        /// 新密碼 (至少有一個數字、至少有一個英文字)
        /// </summary>
        [Required(ErrorMessage = "請輸入密碼")]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-zA-Z]).{6,12}", ErrorMessage = "請輸入6-12個英數字")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// 確認新密碼
        /// </summary>
        [Required(ErrorMessage = "請再次輸入密碼")]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "兩次密碼輸入不一致")]
        public string PasswordConfirm { get; set; }
    }

    /// <summary>
    /// 驗證碼
    /// </summary>
    public class ValidCodeViewModel
    {
        /// <summary>
        /// 驗證碼類型 ValidType
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 驗證碼 guid
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// User Email
        /// </summary>
        public string Key { get; set; }
    }

    /// <summary>
    /// 驗證結果
    /// </summary>
    public class ValidCodeResultViewModel
    {
        /// <summary>
        /// UserID
        /// </summary>
        public Guid UserID { get; set; }

        /// <summary>
        /// 驗證碼類型
        /// </summary>
        public ValidType ValidType { get; set; }
    }

    /// <summary>
    /// 寄送驗證信
    /// </summary>
    public class SendEmailViewModel
    {
        [Required(ErrorMessage = "請輸入Email帳號")]
        [EmailAddress(ErrorMessage = "請輸入正確的Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "請輸入驗證碼")]
        public string Captcha { get; set; }

        /// <summary>
        /// 系統郵件
        /// </summary>
        public SystemMailType SystemMailType { get; set; }

        /// <summary>
        /// 驗證碼類型
        /// </summary>
        public ValidType? ValidType { get; set; }
    }

    /// <summary>
    /// 會員篩選
    /// </summary>
    public class UserFilter
    {
        /// <summary>
        /// 搜尋字串 for 姓名, Email
        /// </summary>
        public string SearchString { get; set; }

        /// <summary>
        /// 篩選登入方式
        /// </summary>
        public List<LoginType> SelectLoginType { get; set; }

        /// <summary>
        /// 篩選身分
        /// </summary> 
        public List<Guid> SelectRoleID { get; set; }

        /// <summary>
        /// 篩選狀態
        /// </summary>
        public List<SelectUserStatus> SelectUserStatus { get; set; }

    }

    /// <summary>
    /// 會員指派
    /// </summary>
    public class UserAssignViewModel
    {
        public mgt_UserAssign UserAssign { get; set; }
        public mgt_UserProfile Member { get; set; }
        public mgt_User FromUser { get; set; }
        public string FromUserPhone { get; set; } //(解密資料直接存回Entity Model, 可能被誤save, 所以另外放)

        public mgt_User ToUser { get; set; }
    }

    //新增:修改
    //public class UpdateMyChild : mgt_UserProfile
    //{
    //    public bool IsCreate { get; set; }

    //    public mgt_UserProfile UpdateModel { get; set; }
    //}
}
